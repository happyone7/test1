#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
COMFY_DIR="${ROOT_DIR}/Tools/ComfyUI"
VENV_DIR="${ROOT_DIR}/Tools/.venvs/comfyui"
UV_CACHE_DIR="${UV_CACHE_DIR:-/tmp/uv-cache}"
UV_LINK_MODE="${UV_LINK_MODE:-copy}"

echo "[1/6] Preparing directories..."
mkdir -p "${ROOT_DIR}/Tools/.venvs"
mkdir -p "${UV_CACHE_DIR}"

echo "[2/6] Cloning ComfyUI (if missing)..."
if [[ ! -d "${COMFY_DIR}/.git" ]]; then
  git clone --depth 1 https://github.com/comfyanonymous/ComfyUI.git "${COMFY_DIR}"
else
  echo "ComfyUI already exists at ${COMFY_DIR}"
fi

echo "[3/6] Creating virtual environment with uv..."
if [[ ! -x "${VENV_DIR}/bin/python" ]]; then
  UV_LINK_MODE="${UV_LINK_MODE}" UV_CACHE_DIR="${UV_CACHE_DIR}" uv venv "${VENV_DIR}" --python 3.12
else
  echo "Venv already exists at ${VENV_DIR}"
fi

echo "[4/6] Installing PyTorch (CUDA 12.4 wheels)..."
UV_LINK_MODE="${UV_LINK_MODE}" UV_CACHE_DIR="${UV_CACHE_DIR}" uv pip install \
  --python "${VENV_DIR}/bin/python" \
  --index-url https://download.pytorch.org/whl/cu124 \
  torch torchvision torchaudio

echo "[5/6] Installing ComfyUI requirements..."
UV_LINK_MODE="${UV_LINK_MODE}" UV_CACHE_DIR="${UV_CACHE_DIR}" uv pip install \
  --python "${VENV_DIR}/bin/python" \
  -r "${COMFY_DIR}/requirements.txt"

echo "[6/6] Creating model folders..."
mkdir -p "${COMFY_DIR}/models/checkpoints"
mkdir -p "${COMFY_DIR}/models/vae"
mkdir -p "${COMFY_DIR}/models/loras"
mkdir -p "${COMFY_DIR}/models/clip"

cat <<EOF

Setup complete.

Next:
1) Place at least one checkpoint in:
   ${COMFY_DIR}/models/checkpoints
2) Run:
   bash Tools/ai-stack/run_comfyui.sh
3) Open:
   http://127.0.0.1:8188

EOF
