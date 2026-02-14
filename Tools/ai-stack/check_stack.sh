#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
MCP_CFG="${ROOT_DIR}/.mcp.json"
COMFY_URL="http://127.0.0.1:8188"
MCP_SERVER="${ROOT_DIR}/Tools/mcp-comfy-ui-builder/dist/mcp-server.js"

echo "[1/4] Validate .mcp.json"
python3 - <<PY
import json
json.load(open("${MCP_CFG}", "r", encoding="utf-8"))
print("mcp_json_ok")
PY

echo "[2/4] Check ComfyUI HTTP"
if curl -fsS "${COMFY_URL}/system_stats" >/dev/null; then
  echo "comfy_http_ok"
else
  echo "comfy_http_not_ready"
fi

echo "[3/4] Check Comfy MCP server binary"
if [[ -f "${MCP_SERVER}" ]]; then
  echo "mcp_server_file_ok"
else
  echo "mcp_server_file_missing"
  exit 1
fi

echo "[4/4] Check model folders"
for d in checkpoints loras vae; do
  path="${ROOT_DIR}/Tools/ComfyUI/models/${d}"
  count=$(find "${path}" -maxdepth 1 -type f \( -name "*.safetensors" -o -name "*.ckpt" -o -name "*.pt" \) | wc -l)
  echo "models/${d}: ${count} model files"
done
