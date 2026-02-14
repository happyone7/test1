#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
COMFY_DIR="${ROOT_DIR}/Tools/ComfyUI"
VENV_DIR="${ROOT_DIR}/Tools/.venvs/comfyui"

if [[ ! -d "${COMFY_DIR}" ]]; then
  echo "ComfyUI is not installed. Run: bash Tools/ai-stack/setup_comfyui.sh"
  exit 1
fi

if [[ ! -x "${VENV_DIR}/bin/python" ]]; then
  echo "ComfyUI venv is missing. Run: bash Tools/ai-stack/setup_comfyui.sh"
  exit 1
fi

cd "${COMFY_DIR}"
exec "${VENV_DIR}/bin/python" main.py --listen 127.0.0.1 --port 8188
