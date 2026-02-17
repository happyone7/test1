# Git 협업 규칙

## 1. 핵심 원칙

- 작업 완료 즉시 커밋 (다른 에이전트 작업과 섞이지 않도록)
- 씬/프리팹 동시 수정 금지 (한 시점에 한 사람만)
- 하나의 논리적 변경 = 하나의 커밋
- 커밋 메시지는 한글로 작성

## 2. 커밋 컨벤션

```
<타입>: <한글 설명>

타입:
- feat: 새로운 기능
- fix: 버그 수정
- balance: 밸런스 수치 조정
- refactor: 리팩토링
- art: 아트 에셋 추가/변경
- audio: 사운드 에셋 추가/변경
- ui: UI 관련 변경
- chore: 기타 잡무
- docs: 문서
```

## 3. 팀장별 Git Author (필수)

```bash
git commit --author="GameDesigner <game-designer@soulspire.dev>"
git commit --author="GameplayProgrammer <gameplay-programmer@soulspire.dev>"
git commit --author="QAEngineer <qa-engineer@soulspire.dev>"
git commit --author="UIDeveloper <ui-developer@soulspire.dev>"
git commit --author="TechnicalArtist <technical-artist@soulspire.dev>"
git commit --author="SoundDirector <sound-director@soulspire.dev>"
git commit --author="BuildEngineer <build-engineer@soulspire.dev>"
git commit --author="ProjectManager <project-manager@soulspire.dev>"
git commit --author="DevPD <dev-pd@soulspire.dev>"
```

## 4. 작업 전 체크리스트

```
□ git status 로 미커밋 변경사항 확인
□ git pull --rebase 으로 최신 상태 동기화
□ 내가 수정할 파일 목록 확인 (특히 .unity, .prefab)
□ Sprint Progress 문서에서 다른 에이전트가 같은 파일 수정 중인지 확인
□ 씬 파일 수정이 필요하면 개발PD에게 먼저 보고
```

## 5. 작업 후 체크리스트

```
□ 씬에 불필요한 오브젝트 남아있지 않은지 확인
□ 새 오브젝트가 프리팹화 되어있는지 확인
□ 즉시 커밋 (다른 에이전트 작업과 섞이지 않도록)
□ 커밋 메시지에 수정한 씬/프리팹 파일 명시
□ PM 에이전트 호출하여 Progress 문서 갱신
```

## 6. 파일 소유권 매트릭스

| 파일/폴더 | 주 담당 | 부 담당 (합의 필요) |
|-----------|---------|-------------------|
| `*.unity` (씬 파일) | 프로그래밍팀장 | 개발PD 승인 후 UI/TA |
| `Assets/Project/Prefabs/Towers/` | 프로그래밍팀장 | - |
| `Assets/Project/Prefabs/UI/` | UI팀장 | - |
| `Assets/Art/` | TA팀장 | - |
| `Assets/Audio/` | 사운드 디렉터 | - |
| `Assets/Project/ScriptableObjects/` | 기획팀장 | 프로그래밍 (구조 변경 시) |
| `Assets/Project/Scripts/` | 프로그래밍팀장 | UI (UI 스크립트) |
| `Assets/Project/Scripts/UI/` | UI팀장 | - |
| `Assets/Editor/` | 프로그래밍팀장 | - |
| `ProjectSettings/` | 프로그래밍팀장 | 개발PD 승인 |

## 7. 씬 파일 충돌 방지 전략

### 전략 A: 프리팹 최대화 (현재 권장)
- 씬에는 빈 루트 오브젝트와 매니저만 배치
- 실제 내용은 모두 프리팹으로 관리
- 씬 수정이 필요한 경우 담당자 지정 후 순차 작업

### 전략 B: Additive Scene 분할 (추후 확장 시)
```
GameScene.unity → 분할:
  ├── GameScene_Environment.unity  (TA 담당)
  ├── GameScene_Logic.unity        (프로그래밍 담당)
  ├── GameScene_UI.unity           (UI 담당)
  └── GameScene_Audio.unity        (사운드 담당)
```
