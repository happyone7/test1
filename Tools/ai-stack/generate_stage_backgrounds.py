#!/usr/bin/env python3
import argparse
import json
import shutil
import time
import urllib.request
from pathlib import Path


BASE_URL = "http://127.0.0.1:8188"
COMFY_OUTPUT_DIR = Path("Tools/ComfyUI/output")
OUT_DIR = Path("Assets/Art/Source/AI/Drafts/Stages")


STAGES = [
    (1, "DataStream", "calm green circuit lanes, clean path highlights, beginner stage readability"),
    (2, "MemoryBlock", "blue grid matrix, block modules, memory sector lighting"),
    (3, "CacheLayer", "purple neon cache glow, light glitch grain, layered data plane"),
    (4, "Pipeline", "orange circuit pipes, multiple branching routes, flow direction markers"),
    (5, "ProcessorCore", "red hot processor core, pulse rings, thermal glow accents"),
    (6, "BusNetwork", "white and cyan bus lines, intersection nodes, high throughput feel"),
    (7, "KernelSpace", "deep violet spatial depth, warning patterns, high tension backdrop"),
    (8, "OverflowZone", "red and black rupture textures, overflow cracks, aggressive glitch overlay"),
    (9, "RootAccess", "golden circuitry, premium border trims, high security system aesthetic"),
    (10, "KernelPanic", "mixed themes, final emergency warnings, intense multi-layer cyber visuals"),
]


def get_json(url: str):
    with urllib.request.urlopen(url, timeout=30) as res:
        return json.loads(res.read().decode("utf-8"))


def post_json(url: str, payload: dict):
    req = urllib.request.Request(
        url,
        data=json.dumps(payload).encode("utf-8"),
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    with urllib.request.urlopen(req, timeout=30) as res:
        return json.loads(res.read().decode("utf-8"))


def wait_for_ready(timeout_sec: int = 180):
    deadline = time.time() + timeout_sec
    while time.time() < deadline:
        try:
            get_json(f"{BASE_URL}/system_stats")
            return
        except Exception:
            time.sleep(2)
    raise TimeoutError("ComfyUI is not ready.")


def fetch_model_names():
    ckpt_info = get_json(f"{BASE_URL}/object_info/CheckpointLoaderSimple")
    lora_info = get_json(f"{BASE_URL}/object_info/LoraLoaderModelOnly")
    ckpts = ckpt_info["CheckpointLoaderSimple"]["input"]["required"]["ckpt_name"][0]
    loras = lora_info["LoraLoaderModelOnly"]["input"]["required"]["lora_name"][0]
    if not ckpts:
        raise RuntimeError("No checkpoint models in ComfyUI.")
    if not loras:
        raise RuntimeError("No LoRA models in ComfyUI.")
    return ckpts[0], loras[0]


def build_workflow(
    ckpt_name: str,
    lora_name: str,
    seed: int,
    positive_prompt: str,
    negative_prompt: str,
    prefix: str,
    width: int,
    height: int,
):
    return {
        "4": {"class_type": "CheckpointLoaderSimple", "inputs": {"ckpt_name": ckpt_name}},
        "10": {
            "class_type": "LoraLoaderModelOnly",
            "inputs": {"lora_name": lora_name, "strength_model": 0.75, "model": ["4", 0]},
        },
        "5": {"class_type": "EmptyLatentImage", "inputs": {"width": width, "height": height, "batch_size": 1}},
        "6": {"class_type": "CLIPTextEncode", "inputs": {"text": positive_prompt, "clip": ["4", 1]}},
        "7": {"class_type": "CLIPTextEncode", "inputs": {"text": negative_prompt, "clip": ["4", 1]}},
        "3": {
            "class_type": "KSampler",
            "inputs": {
                "seed": seed,
                "steps": 24,
                "cfg": 6.0,
                "sampler_name": "dpmpp_2m",
                "scheduler": "karras",
                "denoise": 1.0,
                "model": ["10", 0],
                "positive": ["6", 0],
                "negative": ["7", 0],
                "latent_image": ["5", 0],
            },
        },
        "8": {"class_type": "VAEDecode", "inputs": {"samples": ["3", 0], "vae": ["4", 2]}},
        "9": {"class_type": "SaveImage", "inputs": {"filename_prefix": prefix, "images": ["8", 0]}},
    }


def wait_for_image(prompt_id: str, timeout_sec: int = 1200):
    deadline = time.time() + timeout_sec
    while time.time() < deadline:
        data = get_json(f"{BASE_URL}/history/{prompt_id}")
        entry = data.get(prompt_id)
        if entry:
            status = entry.get("status", {})
            if status.get("completed") and status.get("status_str") not in ("success", None):
                raise RuntimeError(f"ComfyUI prompt failed: {prompt_id} status={status.get('status_str')}")
            outputs = entry.get("outputs", {})
            node = outputs.get("9", {})
            images = node.get("images", [])
            if images:
                return images[0]
        time.sleep(2)
    raise TimeoutError(f"Timeout waiting for {prompt_id}")


def render_one(
    ckpt_name: str,
    lora_name: str,
    positive_prompt: str,
    negative_prompt: str,
    prefix: str,
    out_path: Path,
    width: int,
    height: int,
):
    wf = build_workflow(
        ckpt_name=ckpt_name,
        lora_name=lora_name,
        seed=int(time.time() * 1000) % (2**31),
        positive_prompt=positive_prompt,
        negative_prompt=negative_prompt,
        prefix=prefix,
        width=width,
        height=height,
    )
    resp = post_json(f"{BASE_URL}/prompt", {"prompt": wf, "client_id": "codex-stage-bg"})
    prompt_id = resp["prompt_id"]
    img = wait_for_image(prompt_id)
    src = COMFY_OUTPUT_DIR / img.get("subfolder", "") / img["filename"]
    out_path.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(src, out_path)


def stage_prompt(stage_no: int, name: str, theme: str):
    return (
        "in-game stage background for a cyber tower-defense map, top-down orthographic layout, "
        "pixel-art style, lo-fi neon mood, dark navy base, readable path lanes, "
        "playable route contrast, clean gameplay composition, "
        f"stage {stage_no} {name}, {theme}, no characters"
    )


def stage_negative():
    return (
        "text, watermark, logo, ui frame, characters, enemies, towers, large foreground objects, "
        "isometric camera, first person camera, perspective heavy tilt, blurry, low detail, photorealistic"
    )


def parse_stage_filter(raw: str):
    if not raw.strip():
        return {x[0] for x in STAGES}
    out = set()
    for token in raw.split(","):
        token = token.strip()
        if not token:
            continue
        out.add(int(token))
    return out


def main():
    parser = argparse.ArgumentParser(description="Generate in-game stage background images via ComfyUI.")
    parser.add_argument("--variants", type=int, default=1, help="variants per stage")
    parser.add_argument("--width", type=int, default=1536)
    parser.add_argument("--height", type=int, default=864)
    parser.add_argument("--stages", type=str, default="", help="Comma-separated stage numbers (e.g. 1,2,5)")
    args = parser.parse_args()

    wait_for_ready()
    ckpt_name, lora_name = fetch_model_names()
    stage_filter = parse_stage_filter(args.stages)

    items = [x for x in STAGES if x[0] in stage_filter]
    if not items:
        raise RuntimeError("No stages selected.")

    total = len(items) * args.variants
    done = 0

    print(f"Using ckpt={ckpt_name}, lora={lora_name}", flush=True)
    for stage_no, name, theme in items:
        for v in range(1, args.variants + 1):
            done += 1
            out = OUT_DIR / f"NB_StageBG_S{stage_no:02d}_{name}_v{v:03d}.png"
            print(f"[{done}/{total}] S{stage_no:02d} v{v:03d} -> {out}", flush=True)
            render_one(
                ckpt_name=ckpt_name,
                lora_name=lora_name,
                positive_prompt=stage_prompt(stage_no, name, theme),
                negative_prompt=stage_negative(),
                prefix=f"NBSTAGE_S{stage_no:02d}_{name}_{v:03d}",
                out_path=out,
                width=args.width,
                height=args.height,
            )

    print("Stage background generation complete.", flush=True)


if __name__ == "__main__":
    main()
