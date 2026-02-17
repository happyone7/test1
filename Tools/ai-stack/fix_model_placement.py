#!/usr/bin/env python3
import argparse
import json
import shutil
import struct
from pathlib import Path


ROOT = Path("/mnt/c/UnityProjects/test1")
MODELS_ROOT = ROOT / "Tools/ComfyUI/models"


def load_header(path: Path):
    with path.open("rb") as f:
        size_raw = f.read(8)
        if len(size_raw) != 8:
            raise ValueError("Not a valid safetensors file (header too short).")
        header_size = struct.unpack("<Q", size_raw)[0]
        header_json = f.read(header_size)
    return json.loads(header_json)


def classify_model(path: Path):
    data = load_header(path)
    meta = data.get("__metadata__", {})
    tensors = [k for k in data.keys() if k != "__metadata__"]

    arch = str(meta.get("modelspec.architecture", "")).lower()
    net_mod = str(meta.get("ss_network_module", "")).lower()

    if "/lora" in arch or "networks.lora" in net_mod:
        return "lora"
    if any(t.startswith("lora_") for t in tensors[:200]):
        return "lora"

    if any("first_stage_model" in t for t in tensors[:500]) and len(tensors) < 500:
        return "vae"

    if any(
        ("model.diffusion_model" in t)
        or ("conditioner.embedders" in t)
        or ("first_stage_model." in t)
        for t in tensors[:500]
    ):
        return "checkpoint"

    return "unknown"


def expected_dir(kind: str):
    if kind == "checkpoint":
        return MODELS_ROOT / "checkpoints"
    if kind == "lora":
        return MODELS_ROOT / "loras"
    if kind == "vae":
        return MODELS_ROOT / "vae"
    return None


def main():
    parser = argparse.ArgumentParser(
        description="Detect safetensors model type and move it to the proper ComfyUI folder."
    )
    parser.add_argument("file", type=str, help="Path to a local .safetensors file")
    parser.add_argument("--dry-run", action="store_true")
    args = parser.parse_args()

    src = Path(args.file).expanduser().resolve()
    if not src.exists():
        raise FileNotFoundError(f"File not found: {src}")
    if src.suffix.lower() != ".safetensors":
        raise ValueError("Only .safetensors is supported.")

    kind = classify_model(src)
    target_dir = expected_dir(kind)

    print(f"file: {src}")
    print(f"detected: {kind}")
    if target_dir is None:
        print("Could not classify this file. Keep it as-is and verify model source/type.")
        return

    dst = target_dir / src.name
    print(f"target: {dst}")
    if src == dst:
        print("Already in the correct folder.")
        return

    if args.dry_run:
        print("dry-run mode: no changes made")
        return

    target_dir.mkdir(parents=True, exist_ok=True)
    shutil.move(str(src), str(dst))
    print("Moved successfully.")


if __name__ == "__main__":
    main()
