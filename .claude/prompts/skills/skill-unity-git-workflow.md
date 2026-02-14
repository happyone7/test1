---
name: unity-git-workflow
description: "Unity 프로젝트 Git 워크플로우, 충돌 방지, 커밋 규칙, 파일 소유권 관리"
---

# Unity Git 워크플로우

## 1. 핵심 원칙

- **작업 완료 즉시 커밋** (다른 에이전트 작업과 섞이지 않도록)
- **씬/프리팹 동시 수정 금지** (한 시점에 한 사람만 수정)
- **하나의 논리적 변경 = 하나의 커밋**
- **커밋 메시지는 한글로 작성**

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

팀장별 author 필수 사용:
```bash
git commit --author="GameDesigner <game-designer@soulspire.dev>"
git commit --author="GameplayProgrammer <gameplay-programmer@soulspire.dev>"
git commit --author="UIDeveloper <ui-developer@soulspire.dev>"
git commit --author="TechnicalArtist <technical-artist@soulspire.dev>"
git commit --author="SoundDirector <sound-director@soulspire.dev>"
```

## 3. 작업 전 체크리스트

```
□ git status 로 미커밋 변경사항 확인
□ git pull --rebase 으로 최신 상태 동기화
□ 내가 수정할 파일 목록 확인 (특히 .unity, .prefab)
□ Sprint Progress 문서에서 다른 에이전트가 같은 파일 수정 중인지 확인
□ 씬 파일 수정이 필요하면 개발PD에게 먼저 보고
```

## 4. 작업 후 체크리스트

```
□ 씬에 불필요한 오브젝트 남아있지 않은지 확인
□ 프리팹화 되지 않은 새 오브젝트가 씬에 있으면 프리팹으로 전환
□ 즉시 커밋 (다른 에이전트 작업과 섞이지 않도록)
□ 커밋 메시지에 수정한 씬/프리팹 파일 명시
□ PM 에이전트 호출하여 Progress 문서 갱신
```

## 5. 파일 소유권 매트릭스

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

## 6. 씬 파일 충돌 방지 전략

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

### 전략 C: UnityYAMLMerge (Smart Merge)
- Unity 내장 도구로 씬/프리팹 YAML 충돌 자동 해결
- 설정: `.gitattributes`에 `*.unity merge=unityyamlmerge` 추가

## 7. .gitattributes (프로젝트 루트에 필수)

```gitattributes
# Unity YAML - Smart Merge + LF 강제
*.unity text merge=unityyamlmerge eol=lf
*.prefab text merge=unityyamlmerge eol=lf
*.asset text merge=unityyamlmerge eol=lf
*.meta text eol=lf
*.mat text merge=unityyamlmerge eol=lf
*.anim text merge=unityyamlmerge eol=lf
*.controller text merge=unityyamlmerge eol=lf

# 바이너리 (LFS 대상)
*.png filter=lfs diff=lfs merge=lfs -text
*.jpg filter=lfs diff=lfs merge=lfs -text
*.wav filter=lfs diff=lfs merge=lfs -text
*.mp3 filter=lfs diff=lfs merge=lfs -text
*.ogg filter=lfs diff=lfs merge=lfs -text
*.psd filter=lfs diff=lfs merge=lfs -text
*.fbx filter=lfs diff=lfs merge=lfs -text
*.ttf filter=lfs diff=lfs merge=lfs -text
*.otf filter=lfs diff=lfs merge=lfs -text
*.dll filter=lfs diff=lfs merge=lfs -text
*.pptx filter=lfs diff=lfs merge=lfs -text
```

## 8. 충돌 해결 절차

```
1. git pull 시 충돌 감지
2. 파일 유형별 대응:
   - .cs (스크립트): 일반 Git merge로 해결
   - .unity / .prefab: git mergetool (UnityYAMLMerge)
   - .png 등 바이너리: 한쪽 선택 (git checkout --ours/--theirs)
3. 충돌 해결 후 Unity 에디터에서 열어 확인
4. 문제 없으면 커밋
```
