---
name: soulspire-sound-direction
description: |
  ComfyUI 오디오 생성(ACE-Step BGM, Stable Audio SFX)과 수학 합성으로 게임 사운드 제작.
  트리거: "BGM 만들어줘", "효과음 제작", "사운드 생성", "ComfyUI 오디오"
  제외: UI 구현, 코드 로직, 이미지 에셋
---

# Soulspire 사운드 제작

## 목적
ComfyUI MCP 도구와 수학 합성(Python)을 활용하여 BGM과 SFX를 제작하고 Unity에 적용한다.

## BGM 제작 절차

### Step 1: 컨셉 확인 (Gate: 컨셉 미확정 시 생성 금지)
```
1. GDD (Docs/Design/GDD.md) 에서 해당 씬/상황의 사운드 요구사항 확인
2. 아트 디렉션 (Docs/Design/ArtDirection_v0.1.md) 에서 톤/무드 확인
3. 기존 에셋 (Assets/Audio/) 에 중복 없는지 manage_asset(action="search")로 확인
→ Gate: 요구사항 + 톤 확정 + 중복 없음 확인 후 Step 2 진행
```

### Step 2: ComfyUI로 BGM 생성
references/audio-specs.md 참조하여 ComfyUI 워크플로우 구성.

```
1. list_models — 사용 가능한 오디오 모델 확인
2. build_workflow 또는 create_workflow — ACE-Step 워크플로우 구성
   - ACE-Step: BGM 전용 (CLIPLoader type='ace_step' 필요)
3. execute_workflow — 생성 실행
4. get_last_output — 결과 확인
5. download_output — 파일 다운로드
```

### Step 3: 품질 검증 (반복 루프, 최대 3회)
```
1. 파일 길이 확인: 30초~3분 범위 내인지
2. 파일 크기 확인: 3~8MB(OGG) 범위 내인지
3. 기준 미달 시 → 프롬프트 조정(duration, BPM 등) 후 Step 2 재실행
4. 3회 시도 후에도 미달 → 개발PD에게 보고 (모델/설정 문제 가능성)
```

→ Gate: Step 3 품질 검증 통과 후에만 Step 4 진행

### Step 4: Unity 적용 (Multi-MCP: ComfyUI → MCP Unity)
```
1. download_output (ComfyUI MCP) — 생성 파일을 로컬에 다운로드
2. 파일을 Assets/Audio/BGM/ 또는 Assets/Audio/SFX/ 에 복사
3. refresh_unity (MCP Unity) — 에셋 인식
4. manage_asset(action="search") (MCP Unity) — 임포트된 에셋 확인
5. manage_asset 으로 임포트 설정 조정:
   - BGM: Load In Background + Streaming
   - SFX (짧은): Decompress On Load
   - SFX (긴): Compressed In Memory
6. read_console (MCP Unity) — 임포트 에러 확인. 에러 시 포맷/설정 수정 후 재임포트
```

## SFX 제작 절차

### 방법 A: ComfyUI Stable Audio
```
1. build_workflow — Stable Audio 워크플로우
   - Stable Audio: SFX 전용 (CLIPLoader type='stable_audio' 필요)
2. 짧은 프롬프트로 효과음 생성 (0.5~2초)
```

### 방법 B: 수학 합성 (Python)
간단한 UI/피드백 사운드는 Python으로 직접 합성.
```python
# WAV 파일 직접 생성 (numpy + scipy)
# 사인파, 노이즈, 엔벨로프 조합
# Assets/Audio/SFX/ 에 직접 저장
```

### SFX 품질 검증 (반복 루프, 최대 3회)
```
1. 파일 길이 확인: 게임플레이 SFX 0.2~2초, UI SFX 0.1~0.5초
2. 파일 크기 확인: 10~500KB
3. 기준 미달 시 → 프롬프트/파라미터 조정 후 재생성
4. 3회 시도 후에도 미달 → 개발PD에게 보고
```

### 방법 선택 기준 (Context-Aware)
| 유형 | 1차 방법 | 폴백 |
|------|---------|------|
| BGM (30초+) | ComfyUI ACE-Step | 폴백 없음 — 실패 시 개발PD 보고 |
| 게임플레이 SFX (폭발, 타격) | ComfyUI Stable Audio | ComfyUI 접속 불가 시 → 수학 합성으로 대체 |
| UI SFX (클릭, 팝업) | 수학 합성 | — (항상 사용 가능) |

```
ComfyUI 접속 판단:
1. get_system_resources 호출 시도
2. 응답 없음/타임아웃 → ComfyUI 미가동으로 판단
3. SFX는 수학 합성으로 폴백, BGM은 개발PD에게 보고
```

## 주의 사항
- 커밋 시 `--author="SoundDirector <sound-director@soulspire.dev>"` 사용
- 에셋 경로: BGM → `Assets/Audio/BGM/`, SFX → `Assets/Audio/SFX/`
- 품질 기준은 references/audio-specs.md 참조
