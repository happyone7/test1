# ComfyUI Local Stack

This project uses a local ComfyUI stack for draft asset generation.

Friend reference mapping (Windows portable flow -> this project):

- Windows `ComfyUI_windows_portable` -> `Tools/ComfyUI`
- Windows `ComfyUI\models\checkpoints` -> `Tools/ComfyUI/models/checkpoints`
- Windows `ComfyUI\models\loras` -> `Tools/ComfyUI/models/loras`
- Windows `run_nvidia_gpu.bat` -> `bash Tools/ai-stack/run_comfyui.sh`

## 1) Install

Run from project root:

```bash
bash Tools/ai-stack/setup_comfyui.sh
bash Tools/ai-stack/install_comfy_manager.sh
```

Installed paths:

- ComfyUI: `Tools/ComfyUI`
- Virtual env: `Tools/.venvs/comfyui`

## 2) Add a Model

Place at least one checkpoint file (`.safetensors`) in:

`Tools/ComfyUI/models/checkpoints`

Recommended to start with an SDXL checkpoint that allows commercial use.
Always verify the license before using generated assets in a shipped game.

Example references you shared:

- Checkpoint example: `juggernaut-xl` (Civitai model page)
- LoRA example: `pixel-art-xl` (Civitai model page)

Place checkpoint in `models/checkpoints` and LoRA in `models/loras`.

If you have direct download links, you can add models with:

```bash
bash Tools/ai-stack/add_model_from_url.sh checkpoint "<direct_model_url>"
bash Tools/ai-stack/add_model_from_url.sh lora "<direct_lora_url>"
```

## 3) Run

```bash
bash Tools/ai-stack/run_comfyui.sh
```

Open:

`http://127.0.0.1:8188`

VS Code debug launch:

- Press `F5` and select `AI: Launch ComfyUI`

Note:

- In this Codex sandbox, GPU checks may require elevated execution.
- On your own local terminal session, CUDA should be available directly if NVIDIA/WSL is correctly installed.

## 4) First Validation

In ComfyUI:

1. Load `CheckpointLoaderSimple` and select the model.
2. (Optional) Add `LoraLoaderModelOnly` and pick a LoRA from `models/loras`.
3. Use positive/negative prompt nodes.
4. Generate 1 image at `1024x1024`.
5. Confirm output in `Tools/ComfyUI/output`.

## 5) Troubleshooting

- If install fails due network sandbox, rerun with elevated permissions.
- If CUDA OOM occurs, reduce resolution and batch size.
- If model is not visible, verify file location in `models/checkpoints`.

## 6) VS Code + MCP Collaboration

This repo is configured to use a ComfyUI MCP server.

Config file:

- `.mcp.json` -> `comfy-ui-builder`

What to run before using MCP tools:

1. Start ComfyUI:
   `bash Tools/ai-stack/run_comfyui.sh`
2. Reload MCP in VS Code (or restart the extension/app).

Server paths currently wired:

- MCP server: `Tools/mcp-comfy-ui-builder/dist/mcp-server.js`
- Comfy host: `http://127.0.0.1:8188`
- Comfy path: `Tools/ComfyUI`

VS Code task shortcuts (`Terminal -> Run Task`):

- `AI: Setup ComfyUI`
- `AI: Install ComfyUI-Manager`
- `AI: Run ComfyUI`
- `AI: Stop ComfyUI`
- `AI: Check Stack`
- `AI: Quick Verify (MCP + Comfy)`

ComfyUI Linker command:

- Open Command Palette and run `ComfyUI Linker: Generate Image`
- The generated image will be saved in your workspace root with `comfyui_saved_` prefix.
