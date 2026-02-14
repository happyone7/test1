#!/usr/bin/env python3
import argparse
import json
import shutil
import time
import urllib.request
from pathlib import Path


BASE_URL = "http://127.0.0.1:8188"
COMFY_OUTPUT_DIR = Path("Tools/ComfyUI/output")
LOGO_DIR = Path("Assets/Art/Source/AI/Brand")
ICON_DIR = Path("Assets/Art/Source/AI/Drafts/HubIcons")


HUB_ICONS = [
    ("AttackPower", "sword with upward arrow, attack boost symbol"),
    ("AttackSpeed", "winged clock, speed boost symbol"),
    ("BaseHP", "fortress shield with plus sign, base health"),
    ("Range", "radar ring expanding outward, range increase"),
    ("BitGain", "data chip coin with plus spark, currency gain"),
    ("StartBit", "starting wallet data chip, initial resource"),
    ("SpawnRate", "enemy portal with fast spawn streaks"),
    ("HPRegen", "healing cross with circular regen pulse"),
    ("Critical", "crosshair with star burst, critical hit unlock"),
    ("UnlockCannon", "cannon silhouette unlock emblem"),
    ("UnlockIce", "ice crystal turret unlock emblem"),
    ("UnlockLightning", "lightning turret unlock emblem"),
    ("UnlockLaser", "laser beam turret unlock emblem"),
    ("UnlockVoid", "void core turret unlock emblem"),
    ("IdleCollector", "offline collector drone with data container"),
    ("SpeedMode", "x2 x3 speed arrows icon"),
    ("TowerSlot", "grid slot expansion with plus tiles"),
    ("CoreNode", "central glowing core node emblem"),
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
            "inputs": {"lora_name": lora_name, "strength_model": 0.7, "model": ["4", 0]},
        },
        "5": {"class_type": "EmptyLatentImage", "inputs": {"width": width, "height": height, "batch_size": 1}},
        "6": {"class_type": "CLIPTextEncode", "inputs": {"text": positive_prompt, "clip": ["4", 1]}},
        "7": {"class_type": "CLIPTextEncode", "inputs": {"text": negative_prompt, "clip": ["4", 1]}},
        "3": {
            "class_type": "KSampler",
            "inputs": {
                "seed": seed,
                "steps": 22,
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
    resp = post_json(f"{BASE_URL}/prompt", {"prompt": wf, "client_id": "codex-logo-icons"})
    prompt_id = resp["prompt_id"]
    img = wait_for_image(prompt_id)
    src = COMFY_OUTPUT_DIR / img.get("subfolder", "") / img["filename"]
    out_path.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(src, out_path)


def logo_prompt():
    return (
        "game title logo for 'NODEBREAKER TD', cyber neon pixel-art style, "
        "bold futuristic typography, electric cyan core motif, dark navy and cyan palette, "
        "high contrast, centered composition, clean title card"
    )


def logo_negative():
    return (
        "watermark, random gibberish text, misspelled title, blurry, low contrast, "
        "crowded background, photo, real world scene"
    )


def icon_prompt(label: str):
    return (
        "single game UI ability icon, top-down cyber pixel-art style, chunky voxel-like blocks, "
        "cyan glowing core highlights, dark navy body, clean silhouette, centered icon, "
        f"{label}, no background scene"
    )


def icon_negative():
    return (
        "text, watermark, multiple objects, background landscape, floor tile, platform, "
        "pedestal, clutter, frame, border, realistic photo"
    )


def main():
    parser = argparse.ArgumentParser(description="Generate title logo + hub skill icons via ComfyUI.")
    parser.add_argument("--logo-variants", type=int, default=3)
    parser.add_argument("--icon-variants", type=int, default=1)
    parser.add_argument("--skip-logo", action="store_true")
    parser.add_argument("--skip-icons", action="store_true")
    parser.add_argument("--icon-variant-start", type=int, default=1)
    parser.add_argument("--icon-variant-end", type=int, default=0)
    parser.add_argument("--only-icons", type=str, default="", help="Comma-separated icon keys to generate")
    args = parser.parse_args()

    wait_for_ready()
    ckpt_name, lora_name = fetch_model_names()
    print(f"Using ckpt={ckpt_name}, lora={lora_name}", flush=True)

    if not args.skip_logo:
        # Title logo
        for i in range(1, args.logo_variants + 1):
            out = LOGO_DIR / f"NB_TitleLogo_v{i:03d}.png"
            print(f"[Logo {i}/{args.logo_variants}] -> {out}", flush=True)
            render_one(
                ckpt_name,
                lora_name,
                logo_prompt(),
                logo_negative(),
                prefix=f"NBLOGO_{i:03d}",
                out_path=out,
                width=1024,
                height=512,
            )

    if not args.skip_icons:
        selected = {x.strip() for x in args.only_icons.split(",") if x.strip()}
        items = HUB_ICONS if not selected else [x for x in HUB_ICONS if x[0] in selected]
        icon_variant_end = args.icon_variant_end if args.icon_variant_end > 0 else args.icon_variants
        icon_variant_start = args.icon_variant_start
        if icon_variant_end < icon_variant_start:
            raise ValueError("icon-variant-end must be >= icon-variant-start")
        icon_range = range(icon_variant_start, icon_variant_end + 1)

        # Hub icons
        total = len(items) * len(icon_range)
        done = 0
        for key, label in items:
            for v in icon_range:
                done += 1
                out = ICON_DIR / f"NB_HubIcon_{key}_v{v:03d}.png"
                print(f"[Icon {done}/{total}] {key} v{v:03d} -> {out}", flush=True)
                render_one(
                    ckpt_name,
                    lora_name,
                    icon_prompt(label) + ", single emblem only, one object only",
                    icon_negative() + ", multiple symbols, icon sheet, ui collage, text letters",
                    prefix=f"NBHUB_{key}_{v:03d}",
                    out_path=out,
                    width=512,
                    height=512,
                )

    print("Logo and hub icon generation complete.", flush=True)


if __name__ == "__main__":
    main()
