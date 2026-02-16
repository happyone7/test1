# Tailscale + ComfyUI 원격 서버 설정 가이드

> 노트북 등 다른 기기에서 AI 리소스 생성(이미지/오디오) 원격 서버에 접속하기 위한 가이드

## 1. 원격 서버 정보

| 항목 | 값 |
|------|-----|
| 서버 역할 | ComfyUI AI 생성 서버 (이미지 + 오디오) |
| Tailscale IP | `100.94.138.29` |
| ComfyUI 웹 UI | `http://100.94.138.29:8189` |
| ComfyUI API | `http://100.94.138.29:8188` |
| 서버 OS | GPU 장착 데스크탑 (NVIDIA) |

## 2. Tailscale 설치 및 접속

### Windows (노트북)
```
1. https://tailscale.com/download 에서 Windows 클라이언트 설치
2. 설치 후 로그인 (기존 계정과 동일 계정 사용)
3. 시스템 트레이 > Tailscale 아이콘 > Connected 확인
4. 테스트: 브라우저에서 http://100.94.138.29:8189 접속
```

### WSL2 (리눅스)
```bash
# Tailscale 설치
curl -fsSL https://tailscale.com/install.sh | sh

# 시작 및 로그인
sudo tailscale up

# 접속 테스트
curl -s http://100.94.138.29:8188/system_stats | head -c 200
```

### 접속 확인
```bash
# Tailscale 상태 확인
tailscale status

# ComfyUI 서버 응답 확인
curl -s http://100.94.138.29:8188/system_stats
```

## 3. Claude Code MCP 설정 (.mcp.json)

프로젝트 루트의 `.mcp.json`에서 `comfy-ui-builder` 서버의 `COMFYUI_HOST`를 원격 IP로 변경:

### 로컬 서버 사용 시 (데스크탑, 기본값)
```json
{
  "comfy-ui-builder": {
    "command": "/home/happy/.nvm/versions/node/v20.20.0/bin/node",
    "args": ["/mnt/c/UnityProjects/test1/Tools/mcp-comfy-ui-builder/dist/mcp-server.js"],
    "env": {
      "COMFYUI_HOST": "http://127.0.0.1:8188",
      "COMFYUI_PATH": "/mnt/c/UnityProjects/test1/Tools/ComfyUI",
      "COMFYUI_KNOWLEDGE_DIR": "/mnt/c/UnityProjects/test1/Tools/mcp-comfy-ui-builder/data"
    }
  }
}
```

### 원격 서버 사용 시 (노트북)
```json
{
  "comfy-ui-builder": {
    "command": "node",
    "args": ["Tools/mcp-comfy-ui-builder/dist/mcp-server.js"],
    "env": {
      "COMFYUI_HOST": "http://100.94.138.29:8188",
      "COMFYUI_PATH": "Tools/ComfyUI",
      "COMFYUI_KNOWLEDGE_DIR": "Tools/mcp-comfy-ui-builder/data"
    }
  }
}
```

> `.mcp.json`은 git에서 추적되므로, 원격 전환 시 로컬 변경만 하고 커밋하지 말 것.
> 또는 환경별 오버라이드 방식을 사용 (아래 참조).

## 4. 설치된 AI 모델

### 이미지 생성
| 모델 | 파일 | 용도 |
|------|------|------|
| Juggernaut XL | `juggernaut-xl.safetensors` | SDXL 이미지 생성 (체크포인트) |
| Pixel Art XL | `pixel-art-xl.safetensors` | 픽셀 아트 스타일 (LoRA) |

### 오디오 생성
| 모델 | 파일 | 크기 | 용도 |
|------|------|------|------|
| ACE-Step v1 | `ace_step_v1_3.5b.safetensors` | 7.2GB | BGM 생성 (태그/가사/BPM/키 제어) |
| Stable Audio Open 1.0 | `stable_audio_open_1.0.safetensors` | 4.6GB | SFX 생성 (텍스트→효과음) |
| T5-base | `t5_base.safetensors` | 851MB | Stable Audio용 텍스트 인코더 |

### 모델 경로
```
Tools/ComfyUI/models/
├── checkpoints/     # SDXL, ACE-Step, Stable Audio 체크포인트
├── loras/           # LoRA 파일
└── clip/            # T5-base (CLIPLoader type=stable_audio)
```

## 5. 오디오 생성 워크플로우

### BGM (ACE-Step)
```
CheckpointLoaderSimple(ace_step_v1_3.5b)
→ TextEncodeAceStepAudio(tags="dark orchestral, gothic", lyrics="[inst]")
→ EmptyAceStepLatentAudio(seconds=60, sample_rate=44100)
→ KSampler(steps=100, cfg=3.0)
→ VAEDecodeAudio
→ SaveAudio
```

### SFX (Stable Audio)
```
CheckpointLoaderSimple(stable_audio_open_1.0)
+ CLIPLoader(t5_base, type=stable_audio)
→ CLIPTextEncode("magical explosion, dark fantasy")
→ ConditioningStableAudio(seconds=2.0, sample_rate=44100)
→ KSampler
→ VAEDecodeAudio
→ SaveAudio
```

### 출력 경로
- ComfyUI 출력: `Tools/ComfyUI/output/` (FLAC 포맷)
- Unity 임포트: `Assets/Audio/BGM/` 또는 `Assets/Audio/SFX/` (WAV/OGG 변환 후)

## 6. 브라우저에서 직접 사용

Tailscale 연결 후 브라우저에서:
- `http://100.94.138.29:8189` — ComfyUI 웹 UI

워크플로우 JSON을 로드하거나 수동으로 노드를 구성하여 생성 가능.

## 7. 트러블슈팅

| 증상 | 원인 | 해결 |
|------|------|------|
| 100.94.138.29 접속 불가 | Tailscale 미연결 | `tailscale status` 확인 후 `tailscale up` |
| ComfyUI 8189 접속 불가 | 서버측 ComfyUI 미실행 | 데스크탑에서 `bash Tools/ai-stack/run_comfyui.sh` |
| MCP 도구 타임아웃 | 네트워크 지연 | Tailscale ping 확인, VPN 충돌 여부 점검 |
| 모델 목록 비어있음 | 원격 서버 모델 경로 문제 | `COMFYUI_PATH`가 서버측 경로와 일치하는지 확인 |
| GPU OOM | VRAM 부족 | 해상도 낮추기, 배치 사이즈 1로 |

## 8. 사운드 디렉션 참고

- 장르: 다크 판타지 타워 디펜스
- BGM 키워드: dark orchestral, gothic, tension, epic
- SFX 키워드: magical, dark fantasy, arcane, visceral
- 포맷 사양: 44100Hz/16bit, BGM=OGG, SFX=WAV
- 볼륨: -14~-16 LUFS
- 상세: `.claude/prompts/skills/soulspire-sound-direction/references/audio-specs.md`
