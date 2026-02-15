# 원격 ComfyUI 접근 설정 가이드

데스크탑(GPU)의 ComfyUI를 노트북에서 Tailscale VPN으로 원격 사용하는 방법.

## 아키텍처

```
노트북 (Claude Code)
  → MCP comfy-ui-builder (COMFYUI_HOST=http://100.94.138.29:8189)
  → Tailscale VPN
  → 데스크탑 Windows (portproxy 8189→8188)
  → WSL2 (ComfyUI :8188, RTX 3080)
```

## 데스크탑 상태 (이미 설정 완료)

| 항목 | 상태 |
|------|------|
| Tailscale 설치 | 완료 (Windows) |
| Tailscale IP | `100.94.138.29` |
| ComfyUI listen | `0.0.0.0:8188` (WSL2) |
| portproxy | `0.0.0.0:8189 → 127.0.0.1:8188` |
| 방화벽 | TCP 8189 Inbound Allow |

## 노트북 설정 (따라하기)

### 1. Tailscale 설치

```bash
# macOS
brew install tailscale
# 또는 https://tailscale.com/download 에서 설치

# Windows 노트북이면
winget install Tailscale.Tailscale

# Linux
curl -fsSL https://tailscale.com/install.sh | sh
```

### 2. Tailscale 로그인

```bash
sudo tailscale up
# 브라우저에서 같은 계정(happyone765@gmail.com)으로 로그인
```

### 3. 연결 확인

```bash
# 데스크탑이 보이는지 확인
tailscale status
# "node" 항목이 보여야 함

# ComfyUI 접근 테스트
curl http://100.94.138.29:8189/
# HTML 응답이 오면 성공
```

### 4. Claude Code MCP 설정

`.claude/settings.json` 또는 프로젝트의 MCP 설정에서 comfy-ui-builder의 환경변수 추가:

```json
{
  "mcpServers": {
    "comfy-ui-builder": {
      "command": "npx",
      "args": ["-y", "comfy-ui-builder@latest"],
      "env": {
        "COMFYUI_HOST": "http://100.94.138.29:8189"
      }
    }
  }
}
```

## 데스크탑 재부팅 후 체크리스트

데스크탑을 재시작한 경우 아래를 확인:

1. **Tailscale 자동시작**: 보통 자동 실행됨. 트레이 아이콘 확인
2. **ComfyUI 실행**:
   ```bash
   # WSL2 터미널에서
   bash /mnt/c/UnityProjects/test1/Tools/ai-stack/run_comfyui.sh
   ```
3. **portproxy 재설정** (재부팅 시 초기화됨):
   ```powershell
   # 관리자 PowerShell에서
   netsh interface portproxy add v4tov4 listenaddress=0.0.0.0 listenport=8189 connectaddress=127.0.0.1 connectport=8188
   ```
4. **방화벽**: 규칙은 재부팅 후에도 유지됨

## 트러블슈팅

### "연결 시간 초과"
- 데스크탑이 켜져 있는지 확인
- 데스크탑에서 ComfyUI가 실행 중인지 확인: `curl http://127.0.0.1:8188/`
- portproxy 확인: `netsh interface portproxy show v4tov4`
- Tailscale 양쪽 모두 Connected 상태인지 확인

### "포트 8189가 아닌 8188을 사용해야 하나요?"
- 아닙니다. 8189 사용이 필수입니다
- WSL2 mirrored networking에서 같은 포트(8188) portproxy 시 루프 발생
- 외부 접근: 8189 → 내부: 8188로 포워딩

### portproxy 영구 설정 (선택)
재부팅마다 portproxy가 초기화됩니다. 자동화하려면:

```powershell
# 작업 스케줄러로 시작 시 실행 등록 (관리자 PowerShell)
$action = New-ScheduledTaskAction -Execute "netsh" -Argument "interface portproxy add v4tov4 listenaddress=0.0.0.0 listenport=8189 connectaddress=127.0.0.1 connectport=8188"
$trigger = New-ScheduledTaskTrigger -AtStartup
Register-ScheduledTask -TaskName "ComfyUI-PortProxy" -Action $action -Trigger $trigger -RunLevel Highest -User "SYSTEM"
```
