# Nodebreaker SDXL Baseline Workflow

Use this as the first ComfyUI graph to match the setup your friend shared.

## Graph (minimal)

1. `CheckpointLoaderSimple`
2. `CLIPTextEncode` (positive)
3. `CLIPTextEncode` (negative)
4. `EmptyLatentImage`
5. `KSampler`
6. `VAEDecode`
7. `SaveImage`

Optional:

- `LoraLoaderModelOnly` between `CheckpointLoaderSimple` and prompt encoders

## Suggested settings (first pass)

- `EmptyLatentImage`: `1024 x 1024`, batch `1`
- `KSampler`:
- `sampler`: `dpmpp_2m`
- `scheduler`: `karras`
- `steps`: `28`
- `cfg`: `6.5`
- `denoise`: `1.0`

## Prompt template (tower sprite)

```text
top-down 2d game sprite, cyber neon tower defense, complex detailed design,
strong readable silhouette at gameplay distance, emissive circuit details,
transparent background, no text, no watermark
```

## Negative prompt

```text
blurry, low contrast, text, watermark, logo, photo, 3d render, background scene
```

## Output location

- `Tools/ComfyUI/output`

## Per-asset pass

- Generate 8~16 candidates per tower/node ID
- Mark `Keep / Revise / Reject`
- Keep selected outputs under:
- `Assets/Art/Source/AI/Towers`
- `Assets/Art/Source/AI/Nodes`
