#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
COMFY_DIR="${ROOT_DIR}/Tools/ComfyUI"
VENV_DIR="${ROOT_DIR}/Tools/.venvs/comfyui"
MANAGER_DIR="${COMFY_DIR}/custom_nodes/ComfyUI-Manager"
UV_CACHE_DIR="${UV_CACHE_DIR:-/tmp/uv-cache}"
UV_LINK_MODE="${UV_LINK_MODE:-copy}"

if [[ ! -d "${COMFY_DIR}" ]]; then
  echo "ComfyUI not found. Run: bash Tools/ai-stack/setup_comfyui.sh"
  exit 1
fi

if [[ ! -x "${VENV_DIR}/bin/python" ]]; then
  echo "Venv not found. Run: bash Tools/ai-stack/setup_comfyui.sh"
  exit 1
fi

mkdir -p "${COMFY_DIR}/custom_nodes"
mkdir -p "${UV_CACHE_DIR}"

if [[ ! -d "${MANAGER_DIR}/.git" ]]; then
  git clone --depth 1 https://github.com/ltdrdata/ComfyUI-Manager.git "${MANAGER_DIR}"
else
  echo "ComfyUI-Manager already exists at ${MANAGER_DIR}"
fi

if [[ -f "${MANAGER_DIR}/requirements.txt" ]]; then
  UV_LINK_MODE="${UV_LINK_MODE}" UV_CACHE_DIR="${UV_CACHE_DIR}" uv pip install \
    --python "${VENV_DIR}/bin/python" \
    -r "${MANAGER_DIR}/requirements.txt"
fi

echo "ComfyUI-Manager install complete."
