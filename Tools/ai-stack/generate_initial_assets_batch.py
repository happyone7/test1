#!/usr/bin/env python3
import argparse
import json
import os
import shutil
import time
import urllib.error
import urllib.request
from pathlib import Path


BASE_URL = "http://127.0.0.1:8188"
COMFY_OUTPUT_DIR = Path("Tools/ComfyUI/output")
TARGET_ROOT = Path("Assets/Art/Source/AI/Drafts")


TOWER_ITEMS = [
    ("T01", "Arrow", "single arrow tower, sleek barrel, high fire rate visual language"),
    ("T02", "Cannon", "single cannon tower, heavy chassis, explosive weapon silhouette"),
    ("T03", "Ice", "single ice tower, cryo core, frost crystal details"),
    ("T04", "Lightning", "single lightning tower, tesla coil arcs, electric core"),
    ("T05", "Laser", "single laser tower, focused emitter, beam channel housing"),
    ("T06", "Void", "single void tower, dark energy core, circular gravity ring"),
]

NODE_ITEMS = [
    ("N01", "Bit", "single enemy node, basic weak walker"),
    ("N02", "Quick", "single enemy node, very fast light body"),
    ("N03", "Heavy", "single enemy node, high hp armored body"),
    ("N04", "Shield", "single enemy node, visible shield shell"),
    ("N05", "Swarm", "single enemy node, tiny swarm type"),
    ("N06", "Regen", "single enemy node, self-heal energy veins"),
    ("N07", "Phase", "single enemy node, phase shift ghost-like effect"),
    ("N08", "Split", "single enemy node, split-core body"),
    ("N09", "Boss", "single enemy boss node, elite oversized threat"),
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


def build_workflow(ckpt_name: str, lora_name: str, seed: int, positive_prompt: str, prefix: str):
    negative = (
        "text, watermark, logo, blurry, low contrast, photo, realistic scene, "
        "multiple objects, frame, border, cluttered background, "
        "ground, floor, terrain, tilemap, landscape, pedestal, platform, base plate, "
        "building block, city, road, map"
    )
    return {
        "4": {"class_type": "CheckpointLoaderSimple", "inputs": {"ckpt_name": ckpt_name}},
        "10": {
            "class_type": "LoraLoaderModelOnly",
            "inputs": {"lora_name": lora_name, "strength_model": 0.7, "model": ["4", 0]},
        },
        "5": {"class_type": "EmptyLatentImage", "inputs": {"width": 768, "height": 768, "batch_size": 1}},
        "6": {"class_type": "CLIPTextEncode", "inputs": {"text": positive_prompt, "clip": ["4", 1]}},
        "7": {"class_type": "CLIPTextEncode", "inputs": {"text": negative, "clip": ["4", 1]}},
        "3": {
            "class_type": "KSampler",
            "inputs": {
                "seed": seed,
                "steps": 20,
                "cfg": 6.5,
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


def wait_for_image(prompt_id: str, timeout_sec: int = 900):
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


def make_prompt(kind: str, label_prompt: str):
    base = (
        "top-down 2d game sprite, pixel art style, chunky voxel-like blocks, "
        "cyber neon tower defense, nodebreaker style, cyan glowing core, dark navy body, "
        "crisp edges, complex detailed design, readable silhouette at gameplay distance, "
        "clean composition, single object centered"
    )
    if kind == "Tower":
        return (
            f"{base}, tower asset, {label_prompt}, "
            "same visual language as a strong lightning tower sprite, "
            "mechanical structure, no environment"
        )
    return (
        f"{base}, enemy node asset, {label_prompt}, "
        "single moving enemy unit, floating or hovering object, "
        "no ground contact, no floor tile, no platform, no pedestal, no environment"
    )


def submit_and_collect(ckpt_name: str, lora_name: str, kind: str, item_id: str, item_name: str, label_prompt: str, variant_idx: int):
    safe_name = item_name.replace(" ", "")
    target_dir = TARGET_ROOT / f"{kind}s"
    target_dir.mkdir(parents=True, exist_ok=True)
    final_name = f"NB_{kind}_{item_id}_{safe_name}_v{variant_idx:03d}.png"
    prefix = f"NBTMP_{kind}_{item_id}_{safe_name}_{variant_idx:03d}"
    prompt = make_prompt(kind, label_prompt)
    workflow = build_workflow(ckpt_name, lora_name, int(time.time() * 1000) % (2**31), prompt, prefix)

    resp = post_json(f"{BASE_URL}/prompt", {"prompt": workflow, "client_id": "codex-initial-batch"})
    prompt_id = resp["prompt_id"]
    image_info = wait_for_image(prompt_id)

    src = COMFY_OUTPUT_DIR / image_info.get("subfolder", "") / image_info["filename"]
    dst = target_dir / final_name
    shutil.copy2(src, dst)
    return dst


def main():
    parser = argparse.ArgumentParser(description="Generate initial Nodebreaker asset draft batch via ComfyUI API.")
    parser.add_argument("--variants", type=int, default=1, help="Images per asset type")
    parser.add_argument("--variant-start", type=int, default=1, help="Start variant index (inclusive)")
    parser.add_argument("--variant-end", type=int, default=0, help="End variant index (inclusive). 0 means use --variants.")
    parser.add_argument("--kinds", choices=["all", "towers", "nodes"], default="all", help="Which asset groups to generate")
    parser.add_argument("--ids", type=str, default="", help="Comma-separated IDs to generate only specific items, e.g. N04,N05")
    args = parser.parse_args()

    wait_for_ready()
    ckpt_name, lora_name = fetch_model_names()
    print(f"Using ckpt={ckpt_name}, lora={lora_name}")

    variant_start = args.variant_start
    variant_end = args.variant_end if args.variant_end > 0 else args.variants
    if variant_end < variant_start:
        raise ValueError("variant-end must be >= variant-start")
    variant_range = range(variant_start, variant_end + 1)

    selected_ids = {x.strip().upper() for x in args.ids.split(",") if x.strip()}

    tower_items = TOWER_ITEMS if args.kinds in ("all", "towers") else []
    node_items = NODE_ITEMS if args.kinds in ("all", "nodes") else []
    if selected_ids:
        tower_items = [x for x in tower_items if x[0].upper() in selected_ids]
        node_items = [x for x in node_items if x[0].upper() in selected_ids]

    total = (len(tower_items) + len(node_items)) * len(variant_range)
    done = 0

    for item_id, item_name, label_prompt in tower_items:
        for v in variant_range:
            done += 1
            print(f"[{done}/{total}] Tower {item_id} {item_name} v{v:03d} ...", flush=True)
            out = submit_and_collect(ckpt_name, lora_name, "Tower", item_id, item_name, label_prompt, v)
            print(f"  -> {out}", flush=True)

    for item_id, item_name, label_prompt in node_items:
        for v in variant_range:
            done += 1
            print(f"[{done}/{total}] Node {item_id} {item_name} v{v:03d} ...", flush=True)
            out = submit_and_collect(ckpt_name, lora_name, "Node", item_id, item_name, label_prompt, v)
            print(f"  -> {out}", flush=True)

    print("Batch generation complete.", flush=True)


if __name__ == "__main__":
    main()
