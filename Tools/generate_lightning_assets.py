#!/usr/bin/env python3
"""Generate Lightning Tower sprites and VFX assets for Soulspire Sprint 6."""

from PIL import Image, ImageDraw
import os
import uuid
import random
import math
import sys

def generate_guid():
    """Generate a 32-char hex GUID for Unity .meta files."""
    return uuid.uuid4().hex

def write_meta(filepath, guid, max_tex_size=256, pixels_per_unit=100):
    """Write a Unity .meta file for a sprite texture."""
    meta_content = f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: {max_tex_size}
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 0
    wrapV: 0
    wrapW: 0
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: {pixels_per_unit}
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: {max_tex_size}
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  - serializedVersion: 4
    buildTarget: Standalone
    maxTextureSize: {max_tex_size}
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 1
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    customData:
    physicsShape: []
    bones: []
    spriteID: 5e97eb03825dee720800000000000000
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
    secondaryTextures: []
    spriteCustomMetadata:
      entries: []
    nameFileIdTable: {{}}
  mipmapLimitGroupName:
  pSDRemoveMatte: 0
  userData:
  assetBundleName:
  assetBundleVariant:
"""
    with open(filepath + ".meta", 'w', newline='\n') as f:
        f.write(meta_content)

def write_folder_meta(folder_path):
    """Write a Unity .meta file for a folder."""
    guid = generate_guid()
    meta_content = f"""fileFormatVersion: 2
guid: {guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData:
  assetBundleName:
  assetBundleVariant:
"""
    with open(folder_path + ".meta", 'w', newline='\n') as f:
        f.write(meta_content)


# Determine output base path
if len(sys.argv) > 1:
    base_path = sys.argv[1]
else:
    base_path = "C:/UnityProjects/wt-dev-ta"

print(f"Output base path: {base_path}")

# ============================================================
# TA-1: Lightning Tower Sprites (5 levels)
# ============================================================
tower_dir = os.path.join(base_path, "Assets/Art/Sprites/Towers/LightningTower")
os.makedirs(tower_dir, exist_ok=True)

# Create folder .meta if it doesn't exist
if not os.path.exists(tower_dir + ".meta"):
    write_folder_meta(tower_dir)
    print(f"Created folder meta: LightningTower.meta")

# Also ensure VFX dir .meta exists
vfx_dir = os.path.join(base_path, "Assets/Art/VFX")
os.makedirs(vfx_dir, exist_ok=True)
if not os.path.exists(vfx_dir + ".meta"):
    write_folder_meta(vfx_dir)
    print(f"Created folder meta: VFX.meta")

for lv in range(1, 6):
    img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # -- Tower Base (dark stone platform) --
    base_dark = (60, 58, 70)
    base_mid = (80, 78, 92)
    base_light = (100, 98, 112)
    # Wide foundation
    draw.rectangle([12, 52, 52, 60], fill=base_dark)
    draw.rectangle([14, 50, 50, 52], fill=base_mid)
    # Highlight strip on base
    draw.rectangle([14, 50, 50, 51], fill=base_light)

    # -- Tower Body (gets taller with level) --
    body_height = 18 + lv * 4  # 22, 26, 30, 34, 38
    body_top = 50 - body_height
    body_left = 22
    body_right = 42

    # Main body - dark blue-gray stone
    body_color = (55, 55, 75)
    body_highlight = (70, 70, 95)
    body_shadow = (40, 40, 55)

    draw.rectangle([body_left, body_top, body_right, 50], fill=body_color)
    # Left highlight edge
    draw.rectangle([body_left, body_top, body_left+2, 50], fill=body_highlight)
    # Right shadow edge
    draw.rectangle([body_right-2, body_top, body_right, 50], fill=body_shadow)

    # Horizontal stone lines (mortar)
    for y in range(body_top + 4, 50, 5):
        draw.line([(body_left+1, y), (body_right-1, y)], fill=(45, 45, 60), width=1)

    # -- Tower Top / Cap --
    cap_top = body_top - 4
    draw.rectangle([body_left-2, body_top-2, body_right+2, body_top+2], fill=base_mid)

    # -- Lightning Rod --
    rod_color = (180, 170, 140)  # metallic gold-ish
    rod_x = 32
    rod_top = cap_top - 6 - lv * 2
    draw.line([(rod_x, cap_top), (rod_x, rod_top)], fill=rod_color, width=2)
    # Rod tip - orb
    orb_radius = 2 + (lv // 2)
    orb_color_inner = (200, 220, 255)
    orb_color_outer = (100 + lv*25, 140 + lv*20, 255)
    draw.ellipse([rod_x-orb_radius, rod_top-orb_radius, rod_x+orb_radius, rod_top+orb_radius],
                 fill=orb_color_outer, outline=orb_color_inner)

    # -- Lightning Bolt decoration on tower body --
    bolt_color = (255, 255, 80 + lv*30)
    bolt_glow = (120 + lv*25, 160 + lv*18, 255)

    # Main bolt zigzag on the body
    bolt_mid = (body_left + body_right) // 2
    bolt_start_y = body_top + 6
    bolt_points = [
        (bolt_mid, bolt_start_y),
        (bolt_mid - 3, bolt_start_y + 6),
        (bolt_mid + 2, bolt_start_y + 10),
        (bolt_mid - 2, bolt_start_y + 16),
        (bolt_mid + 1, bolt_start_y + 20),
    ]
    for i in range(len(bolt_points) - 1):
        if bolt_points[i+1][1] < 50:
            draw.line([bolt_points[i], bolt_points[i+1]], fill=bolt_color, width=2)

    # -- Level-dependent embellishments --
    if lv >= 2:
        # Side sparks
        draw.point((body_left + 4, body_top + 10), fill=bolt_glow)
        draw.point((body_right - 4, body_top + 14), fill=bolt_glow)

    if lv >= 3:
        # Glowing runes on tower sides
        for ry in range(body_top + 8, 48, 8):
            draw.rectangle([body_left+1, ry, body_left+3, ry+2], fill=bolt_glow)
            draw.rectangle([body_right-3, ry, body_right-1, ry+2], fill=bolt_glow)

    if lv >= 4:
        # Electric aura around orb
        aura_r = orb_radius + 3
        for angle_i in range(8):
            angle = angle_i * math.pi / 4
            ax = int(rod_x + aura_r * math.cos(angle))
            ay = int(rod_top + aura_r * math.sin(angle))
            draw.point((ax, ay), fill=(200, 230, 255))

    if lv >= 5:
        # Double lightning bolts on body + stronger glow
        bolt2_points = [
            (bolt_mid + 4, bolt_start_y + 2),
            (bolt_mid + 1, bolt_start_y + 8),
            (bolt_mid + 5, bolt_start_y + 12),
            (bolt_mid + 2, bolt_start_y + 18),
        ]
        for i in range(len(bolt2_points) - 1):
            if bolt2_points[i+1][1] < 50:
                draw.line([bolt2_points[i], bolt2_points[i+1]], fill=bolt_glow, width=1)

        # Crown arcs
        draw.arc([rod_x-6, rod_top-6, rod_x+6, rod_top+6], 200, 340, fill=(220, 240, 255), width=1)

    filepath = os.path.join(tower_dir, f"tower_lightning_lv{lv}.png")
    img.save(filepath)
    write_meta(filepath, generate_guid(), max_tex_size=256, pixels_per_unit=100)
    print(f"Created tower_lightning_lv{lv}.png + .meta")


# ============================================================
# TA-2: Lightning VFX Assets
# ============================================================

# --- lightning_chain_line.png (128x16) ---
chain_img = Image.new('RGBA', (128, 16), (0, 0, 0, 0))
chain_draw = ImageDraw.Draw(chain_img)

# Core bolt path
random.seed(42)  # Reproducible
y_center = 8
points = [(0, y_center)]
for x in range(4, 128, 4):
    y_offset = random.randint(-4, 4)
    points.append((x, y_center + y_offset))

# Draw glow (wider, dimmer)
for i in range(len(points) - 1):
    chain_draw.line([points[i], points[i+1]], fill=(80, 120, 255, 100), width=5)

# Draw core (narrower, bright)
for i in range(len(points) - 1):
    chain_draw.line([points[i], points[i+1]], fill=(200, 220, 255, 220), width=2)

# Draw center (very bright, 1px)
for i in range(len(points) - 1):
    chain_draw.line([points[i], points[i+1]], fill=(255, 255, 255, 255), width=1)

chain_path = os.path.join(vfx_dir, "lightning_chain_line.png")
chain_img.save(chain_path)
write_meta(chain_path, generate_guid(), max_tex_size=256, pixels_per_unit=100)
print(f"Created lightning_chain_line.png + .meta")

# --- lightning_hit_effect.png (64x64) ---
hit_img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
hit_draw = ImageDraw.Draw(hit_img)

cx, cy = 32, 32

# Outer glow ring
for r in range(20, 10, -1):
    alpha = int(40 * (20 - r) / 10)
    color = (100, 150, 255, alpha)
    hit_draw.ellipse([cx-r, cy-r, cx+r, cy+r], fill=color)

# Inner bright flash
hit_draw.ellipse([cx-8, cy-8, cx+8, cy+8], fill=(180, 210, 255, 200))
hit_draw.ellipse([cx-4, cy-4, cx+4, cy+4], fill=(240, 245, 255, 240))
hit_draw.ellipse([cx-2, cy-2, cx+2, cy+2], fill=(255, 255, 255, 255))

# Electric sparks radiating outward
random.seed(123)
for i in range(12):
    angle = i * math.pi / 6 + random.uniform(-0.2, 0.2)
    length = random.randint(10, 22)
    mid_length = length // 2

    # Start from center area
    sx = cx + int(4 * math.cos(angle))
    sy = cy + int(4 * math.sin(angle))

    # Midpoint with jitter
    mx = cx + int(mid_length * math.cos(angle)) + random.randint(-3, 3)
    my = cy + int(mid_length * math.sin(angle)) + random.randint(-3, 3)

    # End point
    ex = cx + int(length * math.cos(angle))
    ey = cy + int(length * math.sin(angle))

    # Draw spark lines
    hit_draw.line([(sx, sy), (mx, my)], fill=(180, 210, 255, 200), width=2)
    hit_draw.line([(mx, my), (ex, ey)], fill=(140, 180, 255, 150), width=1)

hit_path = os.path.join(vfx_dir, "lightning_hit_effect.png")
hit_img.save(hit_path)
write_meta(hit_path, generate_guid(), max_tex_size=256, pixels_per_unit=100)
print(f"Created lightning_hit_effect.png + .meta")

print("\n=== All Lightning Tower assets generated successfully! ===")
