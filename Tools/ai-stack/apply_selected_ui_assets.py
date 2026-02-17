#!/usr/bin/env python3
from pathlib import Path


ROOT = Path("/mnt/c/UnityProjects/test1")
SELECTED_ICON_DIR = ROOT / "Assets/Art/Source/AI/Selected/HubIcons"
SELECTED_LOGO = ROOT / "Assets/Art/Source/AI/Selected/Brand/NB_TitleLogo_Selected.png"
UI_ICON_DIR = ROOT / "Assets/Art/UI/Hub"
UI_LOGO_DIR = ROOT / "Assets/Art/UI/Logo"


ICON_KEYS = [
    "AttackPower",
    "AttackSpeed",
    "BaseHP",
    "BitGain",
    "CoreNode",
    "Critical",
    "HPRegen",
    "IdleCollector",
    "Range",
    "SpawnRate",
    "SpeedMode",
    "StartBit",
    "TowerSlot",
    "UnlockCannon",
    "UnlockIce",
    "UnlockLaser",
    "UnlockLightning",
    "UnlockVoid",
]


def patch_meta_for_sprite(meta_path: Path):
    if not meta_path.exists():
        return
    lines = meta_path.read_text(encoding="utf-8").splitlines()
    out = []
    replacements = {
        "    enableMipMap:": "    enableMipMap: 0",
        "    filterMode:": "    filterMode: 0",
        "  spriteMode:": "  spriteMode: 1",
        "  textureType:": "  textureType: 8",
        "  alphaIsTransparency:": "  alphaIsTransparency: 1",
    }
    for line in lines:
        replaced = False
        for key, val in replacements.items():
            if line.startswith(key):
                out.append(val)
                replaced = True
                break
        if not replaced:
            out.append(line)
    meta_path.write_text("\n".join(out) + "\n", encoding="utf-8")


def copy_icons():
    UI_ICON_DIR.mkdir(parents=True, exist_ok=True)
    for key in ICON_KEYS:
        src = SELECTED_ICON_DIR / f"NB_HubIcon_{key}.png"
        dst = UI_ICON_DIR / f"NB_Hub_{key}.png"
        if not src.exists():
            raise FileNotFoundError(f"Missing selected icon: {src}")
        dst.write_bytes(src.read_bytes())
        patch_meta_for_sprite(dst.with_suffix(".png.meta"))


def copy_logo():
    UI_LOGO_DIR.mkdir(parents=True, exist_ok=True)
    if not SELECTED_LOGO.exists():
        raise FileNotFoundError(f"Missing selected logo: {SELECTED_LOGO}")

    # Keep existing naming convention and additionally export a Selected-named copy.
    dst_main = UI_LOGO_DIR / "NB_Logo_003.png"
    dst_selected = UI_LOGO_DIR / "NB_Logo_Selected.png"
    dst_main.write_bytes(SELECTED_LOGO.read_bytes())
    dst_selected.write_bytes(SELECTED_LOGO.read_bytes())

    patch_meta_for_sprite(dst_main.with_suffix(".png.meta"))
    # NB_Logo_Selected may not have a meta yet; Unity will generate one if missing.
    patch_meta_for_sprite(dst_selected.with_suffix(".png.meta"))


def main():
    copy_icons()
    copy_logo()
    print("Applied selected AI assets to Assets/Art/UI and patched import meta for Sprite settings.")


if __name__ == "__main__":
    main()
