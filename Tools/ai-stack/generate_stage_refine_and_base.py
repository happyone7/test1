#!/usr/bin/env python3
import argparse
import json
import shutil
import time
import urllib.request
from pathlib import Path


BASE_URL = "http://127.0.0.1:8188"
COMFY_OUTPUT_DIR = Path("Tools/ComfyUI/output")
STAGE_OUT_DIR = Path("Assets/Art/Source/AI/Drafts/Stages")
BASE_OUT_DIR = Path("Assets/Art/Source/AI/Drafts/Base")


REFINED_STAGE_THEMES = {
    5: (
        "ProcessorCore",
        "top-down motherboard battlefield, copper circuit traces as enemy lanes, glowing red processor sectors, "
        "route lines clearly readable, tactical empty areas for tower placement, high contrast lane-vs-buildable zoning",
    ),
    8: (
        "OverflowZone",
        "damaged circuit board with ruptured traces, red-black overflow leaks, glitching data cracks following lane paths, "
        "clear route readability, tactical tower build zones, high-pressure late-game mood",
    ),
}


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
    resp = post_json(f"{BASE_URL}/prompt", {"prompt": wf, "client_id": "codex-stage-refine"})
    prompt_id = resp["prompt_id"]
    img = wait_for_image(prompt_id)
    src = COMFY_OUTPUT_DIR / img.get("subfolder", "") / img["filename"]
    out_path.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(src, out_path)


def stage_prompt(stage_no: int, stage_name: str, theme: str):
    return (
        "in-game tower-defense stage background, top-down orthographic map, pixel-art style, "
        "cyber circuit-board world, enemies travel along visible circuit traces, "
        "route readability first, clean gameplay composition, "
        f"stage {stage_no} {stage_name}, {theme}, no characters"
    )


def stage_negative():
    return (
        "text, watermark, logo, ui frame, towers, enemies, character portraits, "
        "isometric angle, perspective tilt, cluttered foreground, low readability"
    )


def base_prompt():
    return (
        "main base core sprite for tower-defense, top-down chip fortress, central CPU-like crystal core, "
        "defense terminals around edges, cyber pixel-art style, single object only, "
        "clear silhouette for gameplay target, dark navy body with cyan-red glow accents"
    )


def base_negative():
    return (
        "text, watermark, multiple objects, full map background, floor tiles, ui frame, "
        "character, photorealistic, blurry"
    )


def main():
    parser = argparse.ArgumentParser(description="Regenerate circuit-focused stage backgrounds and base core image.")
    parser.add_argument("--variant-start", type=int, default=2)
    parser.add_argument("--variant-end", type=int, default=4)
    parser.add_argument("--stage-width", type=int, default=1536)
    parser.add_argument("--stage-height", type=int, default=864)
    parser.add_argument("--base-variants", type=int, default=3)
    parser.add_argument("--base-size", type=int, default=512)
    parser.add_argument("--skip-base", action="store_true")
    args = parser.parse_args()

    if args.variant_end < args.variant_start:
        raise ValueError("variant-end must be >= variant-start")

    wait_for_ready()
    ckpt_name, lora_name = fetch_model_names()
    print(f"Using ckpt={ckpt_name}, lora={lora_name}", flush=True)

    total_stage = len(REFINED_STAGE_THEMES) * (args.variant_end - args.variant_start + 1)
    done = 0
    for stage_no, stage_data in REFINED_STAGE_THEMES.items():
        stage_name, theme = stage_data
        for v in range(args.variant_start, args.variant_end + 1):
            done += 1
            out = STAGE_OUT_DIR / f"NB_StageBG_S{stage_no:02d}_{stage_name}_v{v:03d}.png"
            print(f"[Stage {done}/{total_stage}] S{stage_no:02d} v{v:03d} -> {out}", flush=True)
            render_one(
                ckpt_name=ckpt_name,
                lora_name=lora_name,
                positive_prompt=stage_prompt(stage_no, stage_name, theme),
                negative_prompt=stage_negative(),
                prefix=f"NBSTAGE_S{stage_no:02d}_{stage_name}_{v:03d}",
                out_path=out,
                width=args.stage_width,
                height=args.stage_height,
            )

    if not args.skip_base:
        for v in range(1, args.base_variants + 1):
            out = BASE_OUT_DIR / f"NB_BaseChip_Core_v{v:03d}.png"
            print(f"[Base {v}/{args.base_variants}] -> {out}", flush=True)
            render_one(
                ckpt_name=ckpt_name,
                lora_name=lora_name,
                positive_prompt=base_prompt(),
                negative_prompt=base_negative(),
                prefix=f"NBBASE_CoreChip_{v:03d}",
                out_path=out,
                width=args.base_size,
                height=args.base_size,
            )

    print("Refined stage/background generation complete.", flush=True)


if __name__ == "__main__":
    main()
