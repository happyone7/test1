#!/usr/bin/env python3
import json
import os
import sys
import time
import urllib.error
import urllib.parse
import urllib.request


BASE_URL = "http://127.0.0.1:8188"
COMFY_OUTPUT_DIR = "/mnt/c/UnityProjects/test1/Tools/ComfyUI/output"


def http_get_json(url: str):
    with urllib.request.urlopen(url, timeout=30) as res:
        return json.loads(res.read().decode("utf-8"))


def http_post_json(url: str, payload: dict):
    data = json.dumps(payload).encode("utf-8")
    req = urllib.request.Request(
        url,
        data=data,
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    with urllib.request.urlopen(req, timeout=30) as res:
        return json.loads(res.read().decode("utf-8"))


def fetch_first_model_names():
    ckpt_info = http_get_json(f"{BASE_URL}/object_info/CheckpointLoaderSimple")
    lora_info = http_get_json(f"{BASE_URL}/object_info/LoraLoaderModelOnly")

    ckpt_req = ckpt_info["CheckpointLoaderSimple"]["input"]["required"]["ckpt_name"][0]
    lora_req = lora_info["LoraLoaderModelOnly"]["input"]["required"]["lora_name"][0]

    if not ckpt_req:
        raise RuntimeError("No checkpoint found in ComfyUI.")
    if not lora_req:
        raise RuntimeError("No LoRA found in ComfyUI.")
    return ckpt_req[0], lora_req[0]


def build_workflow(ckpt_name: str, lora_name: str):
    positive = (
        "top-down 2d game sprite, cyber neon tower defense, "
        "nodebreaker style, complex detailed design, readable silhouette, "
        "single tower object, emissive circuit details, transparent background"
    )
    negative = (
        "text, watermark, logo, blurry, low contrast, photo, realistic scene, "
        "multiple objects, frame, border, background environment"
    )

    return {
        "4": {
            "class_type": "CheckpointLoaderSimple",
            "inputs": {"ckpt_name": ckpt_name},
        },
        "10": {
            "class_type": "LoraLoaderModelOnly",
            "inputs": {
                "lora_name": lora_name,
                "strength_model": 0.7,
                "model": ["4", 0],
            },
        },
        "5": {
            "class_type": "EmptyLatentImage",
            "inputs": {"width": 768, "height": 768, "batch_size": 1},
        },
        "6": {
            "class_type": "CLIPTextEncode",
            "inputs": {"text": positive, "clip": ["4", 1]},
        },
        "7": {
            "class_type": "CLIPTextEncode",
            "inputs": {"text": negative, "clip": ["4", 1]},
        },
        "3": {
            "class_type": "KSampler",
            "inputs": {
                "seed": int(time.time()),
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
        "8": {
            "class_type": "VAEDecode",
            "inputs": {"samples": ["3", 0], "vae": ["4", 2]},
        },
        "9": {
            "class_type": "SaveImage",
            "inputs": {"filename_prefix": "Nodebreaker_Test", "images": ["8", 0]},
        },
    }


def wait_for_result(prompt_id: str, timeout_sec: int = 600):
    deadline = time.time() + timeout_sec
    while time.time() < deadline:
        hist = http_get_json(f"{BASE_URL}/history/{prompt_id}")
        entry = hist.get(prompt_id)
        if entry:
            outputs = entry.get("outputs", {})
            save_node = outputs.get("9", {})
            images = save_node.get("images", [])
            if images:
                return images
        time.sleep(2)
    raise TimeoutError(f"Timed out waiting for prompt_id={prompt_id}")


def main():
    try:
        _ = http_get_json(f"{BASE_URL}/system_stats")
    except urllib.error.URLError as e:
        print(f"ComfyUI not reachable at {BASE_URL}: {e}", file=sys.stderr)
        sys.exit(1)

    ckpt_name, lora_name = fetch_first_model_names()
    workflow = build_workflow(ckpt_name, lora_name)
    resp = http_post_json(f"{BASE_URL}/prompt", {"prompt": workflow, "client_id": "codex-nodebreaker"})
    prompt_id = resp["prompt_id"]
    print(f"Submitted prompt_id={prompt_id}")
    images = wait_for_result(prompt_id)

    print("Generated images:")
    for img in images:
        filename = img["filename"]
        subfolder = img.get("subfolder", "")
        full_path = os.path.join(COMFY_OUTPUT_DIR, subfolder, filename)
        print(f"- {full_path}")


if __name__ == "__main__":
    main()
