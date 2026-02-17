#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 2 ]]; then
  echo "Usage:"
  echo "  bash Tools/ai-stack/add_model_from_url.sh checkpoint <direct_url_to_model_file>"
  echo "  bash Tools/ai-stack/add_model_from_url.sh lora <direct_url_to_model_file>"
  exit 1
fi

KIND="$1"   # checkpoint | lora | vae
URL="$2"

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
case "${KIND}" in
  checkpoint) TARGET_DIR="${ROOT_DIR}/Tools/ComfyUI/models/checkpoints" ;;
  lora) TARGET_DIR="${ROOT_DIR}/Tools/ComfyUI/models/loras" ;;
  vae) TARGET_DIR="${ROOT_DIR}/Tools/ComfyUI/models/vae" ;;
  *)
    echo "Invalid kind: ${KIND}"
    exit 1
    ;;
esac

mkdir -p "${TARGET_DIR}"

NAME="$(basename "${URL%%\?*}")"
if [[ -z "${NAME}" || "${NAME}" == "/" ]]; then
  NAME="${KIND}_model_$(date +%Y%m%d_%H%M%S).safetensors"
fi
if [[ "${NAME}" != *.* ]]; then
  NAME="${NAME}.safetensors"
fi

OUT="${TARGET_DIR}/${NAME}"
echo "Downloading to: ${OUT}"
curl -L --fail --retry 3 --connect-timeout 20 --max-time 0 "${URL}" -o "${OUT}"

echo "Done: ${OUT}"
