# 오디오 사양 및 품질 기준

## 포맷 사양
- 샘플레이트: 44100Hz / 16bit
- BGM 포맷: OGG Vorbis (품질 6~8)
- SFX 포맷: WAV 무압축 (짧은 클립은 Unity 내 압축)

## 길이 기준
- BGM: 30초~3분 (루프)
- 게임플레이 SFX: 0.2~2초
- UI SFX: 0.1~0.5초

## 파일 크기 기준
- BGM 곡당: 3~8MB (OGG)
- SFX 개당: 10~500KB

## 볼륨 기준
- 마스터 출력: -14 ~ -16 LUFS
- 같은 카테고리 내: ±3dB 이내
- BGM + SFX 동시 재생 시 마스킹 없어야 함

## Unity 임포트 설정
- BGM: Load In Background + Streaming
- SFX (짧은): Decompress On Load
- SFX (긴): Compressed In Memory

## 퍼포먼스 기준
- 동시 재생 AudioSource: 32개 이하
- 사운드 전체 메모리: 50MB 이하

## 에셋 폴더 구조
```
Assets/Audio/
├── BGM/
│   ├── BGM_Hub.ogg
│   ├── BGM_Combat_Stage01.ogg
│   └── BGM_Boss.ogg
├── SFX/
│   ├── UI/
│   │   ├── SFX_ButtonClick.wav
│   │   └── SFX_SkillPurchase.wav
│   ├── Combat/
│   │   ├── SFX_ArrowShoot.wav
│   │   └── SFX_MonsterDeath.wav
│   └── Feedback/
│       ├── SFX_LevelUp.wav
│       └── SFX_StageUnlock.wav
└── Mixers/
    └── MainMixer.mixer
```

## ComfyUI 모델별 용도
| 모델 | CLIPLoader type | 용도 |
|------|----------------|------|
| ACE-Step | ace_step | BGM 생성 (음악적 구조) |
| Stable Audio | stable_audio | SFX 생성 (효과음, 환경음) |

## 사운드 디렉션 (Soulspire)
- 장르: 다크 판타지 타워 디펜스
- 톤: 어둡고 긴장감 있는, 고딕 느낌
- BGM 키워드: dark orchestral, gothic, tension, epic
- SFX 키워드: magical, dark fantasy, arcane, visceral
