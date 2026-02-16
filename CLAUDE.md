# Soulspire - Unity Steam Game Project

## 프로젝트 개요
다크 판타지 타워 디펜스 (로그라이트) 게임. Unity 2D로 개발, Steam에 배포.

## 기술 스택
- **엔진**: Unity 2022.3 LTS
- **언어**: C#
- **플랫폼**: Steam (Windows 빌드)
- **Steam SDK**: Steamworks.NET (현재 DISABLESTEAMWORKS 심볼로 비활성)

## 프로젝트 구조
- `Assets/Project/Scripts/` - 게임 로직 (Core, Tower, Node, Data, UI, Audio)
- `Assets/Project/Scenes/` - 게임 씬 (GameScene.unity)
- `Assets/Project/ScriptableObjects/` - SO 데이터 (Towers, Nodes, Stages, Waves, Skills)
- `Assets/Art/` - 스프라이트, UI, VFX, 오디오 에셋
- `Assets/Editor/` - 에디터 도구 (BuildScript 등)
- `SteamBuild/scripts/` - Steam VDF 빌드 설정
- `SteamBuild/content/` - 빌드 결과물 (gitignore)
- `Docs/` - 기획 문서, 스프린트 진행 현황, DevPD 지침서
- `.claude/agents/` - 에이전트 정의 (8개 팀장)
- `.claude/prompts/skills/` - 스킬 정의 (5개)

## Steam 정보
- **게임 이름**: Soulspire (구: Nodebreaker TD)
- **App ID**: 4426340
- **Depot ID**: 4426341
- **Steam 계정**: shatterbone7
- **Partner ID**: 377547
- **Launch Option**: MyGame.exe

## MCP 서버
- **mcp-unity**: Unity Editor 제어 (포트 8080, CoplayDev/unity-mcp v9.4.4)
- **comfy-ui-builder**: ComfyUI 이미지/오디오 생성
- **playwright**: 웹 자동화
- **Notion**: 업무 카드/문서 관리

---

## 팀 구조 및 역할

### 팀 별칭
- **총괄PD** (사용자): 최종 의사결정, 방향 설정
- **개발PD** (Claude/Codex): 업무 조율 및 할당 **전용** — 코드/UI/빌드 직접 작업 절대 금지
- **기획팀장** (game-designer): 메카닉 설계, 밸런싱, SO 수치 조정
- **프로그래밍팀장** (unity-gameplay-programmer): 코어 게임플레이 코드 구현
- **QA팀장** (unity-qa-engineer): 에디터 QA, 머지 게이트
- **UI팀장** (unity-ui-developer): 모든 UI/UX 구현
- **빌더** (unity-build-engineer): 빌드, CI/CD, Steam 배포
- **TA** (unity-technical-artist): 아트디렉팅, 셰이더, 스프라이트, VFX, ComfyUI AI 에셋
- **사운드 디렉터** (unity-sound-director): BGM/SFX 제작, 믹싱, Unity 적용
- **PM** (project-manager): 스프린트 현황 취합, 문서 갱신

### 위임 원칙 (필수 준수 — 예외 없음)
1. **개발PD는 절대 직접 코딩/UI/빌드 작업하지 않음** — 무조건 해당 에이전트에게 위임
2. **에이전트 실패 시에도 개발PD 직접 작업 금지** — 네트워크 오류/MCP 장애/API 에러 등으로 에이전트가 실패하면 원인 해결 후 에이전트를 재실행할 것. 어떤 예외 상황에서도 개발PD가 "직접 하겠다"고 판단해서는 안 됨
3. **실무자 자체 QA**: 프로그래밍팀장, UI팀장, 기획팀장 등은 작업 후 자체 QA
4. **QA 조기 착수**: 각 팀장 작업 단위 완료 시 즉시 해당 부분 QA
5. **QA 통과 후 빌더에게 빌드 명령**: 모든 작업 완료 + QA 통과 → 빌더가 빌드
6. **UI 작업은 반드시 UI팀장 경유**: UI 수정/생성은 unity-ui-developer 에이전트 사용
7. **UI팀장이 목업 + Unity 구현 모두 담당** (프로토타입 단계)
8. **UI 명세는 PPT로 작성** (기획팀장이 디자인 없는 레이아웃 명세 담당)
9. **TA↔UI 역할 분리**: TA는 UI 컨셉/이미지 리소스만, UI 시스템 구현은 UI팀장 담당
10. **SO 수치 수정은 기획팀장 담당**: 코드 변경 없이 SO 수치만 바꾸는 작업은 기획팀장이 직접 수행

### 개발PD 3인 체제
- 개발PD 01: Claude Code, 개발PD 02: Claude Code, 개발PD 03: Codex
- **정보공유 핵심 문서**: `Docs/DevPD_Guidelines.md`
- 작업 전 반드시 Sprint Progress 문서 + git log 확인
- 작업 후 반드시 PM 호출하여 진행 현황 갱신

---

## Git 정책

### 브랜칭 (QA 게이트 방식)
- **메인 작업 브랜치**: `sprint2`, `sprint3` 등 스프린트 단위
- **팀장 작업 브랜치**: `dev/programmer`, `dev/ui`, `dev/ta`, `dev/game-designer`, `dev/sound`, `dev/build`
- **sprint 브랜치 머지 권한**: QA팀장만 가능 (동작 검증 통과 후)
- **워크플로우**: 팀장 dev/* 작업+커밋 → QA팀장 검증 → 통과 시 sprint 브랜치 머지
- **스프린트 브랜치 격리**: 이전 스프린트 브랜치에 다음 스프린트 작업 절대 커밋 금지

### 팀장별 Git Author (커밋 시 필수 사용)
- 기획: `--author="GameDesigner <game-designer@soulspire.dev>"`
- 프로그래밍: `--author="GameplayProgrammer <gameplay-programmer@soulspire.dev>"`
- QA: `--author="QAEngineer <qa-engineer@soulspire.dev>"`
- UI: `--author="UIDeveloper <ui-developer@soulspire.dev>"`
- TA: `--author="TechnicalArtist <technical-artist@soulspire.dev>"`
- 사운드: `--author="SoundDirector <sound-director@soulspire.dev>"`
- 빌더: `--author="BuildEngineer <build-engineer@soulspire.dev>"`
- PM: `--author="ProjectManager <project-manager@soulspire.dev>"`
- 개발PD: `--author="DevPD <dev-pd@soulspire.dev>"`

### 커밋 규칙
- 커밋 메시지는 **한글**로 작성
- 작업 완료 즉시 커밋 (미커밋 상태로 세션 종료 금지)
- 프로젝트 관리 파일(에이전트/스킬/메모리/문서) 수정 후 다른 작업으로 넘어갈 때 즉시 커밋
- **stash 금지** — 브랜치 전환 전 반드시 커밋 (WIP 접두사 허용)

### Git Worktree (단일 로컬 머신)
에이전트별 독립 워킹 디렉토리로 브랜치 전환 없이 작업 가능:
```
/mnt/c/UnityProjects/test1/           ← 메인 (Unity 에디터 연결)
/mnt/c/UnityProjects/wt-dev-programmer/ ← dev/programmer
/mnt/c/UnityProjects/wt-dev-ui/        ← dev/ui
/mnt/c/UnityProjects/wt-dev-ta/        ← dev/ta
/mnt/c/UnityProjects/wt-dev-game-designer/ ← dev/game-designer
/mnt/c/UnityProjects/wt-dev-sound/     ← dev/sound
/mnt/c/UnityProjects/wt-dev-build/     ← dev/build
```
- **스크립트/SO/문서 편집**: worktree에서 직접 (브랜치 전환 불필요)
- **Unity MCP 작업** (씬, 컴포넌트, 플레이모드): 메인 프로젝트에서만, 해당 dev/*로 전환 후 수행
- 전환 프로토콜: 커밋 → checkout → refresh_unity → read_console → 작업 → 커밋

---

## 씬 수정 규칙 (프리팹 기반)
- **씬 직접 수정**: 프로그래밍팀장만 (매니저, 게임 로직 오브젝트)
- **UI 수정**: UI팀장이 프리팹 파일 수정 (씬 터치 불필요)
- **TA/사운드**: 에셋 파일만 수정 (씬/프리팹 직접 수정 금지)
- 씬 수정이 필요하면 개발PD에게 보고 후 순서 조율
- 프리팹 추출 대상: TopHUD, RunEndPanel, HubPanel, BottomBar, SettingsOverlay, TowerPurchasePanel, TowerInfoTooltip, IdleBitOverlay, InventoryBar, HpWarningOverlay, TitleScreenPanel, ConnectionLine → `Assets/Prefabs/UI/`

---

## 디자인 문서 관리 (로컬 md 기준 + Notion 동기화)
- **에이전트 기준 문서**: `Docs/Design/` 로컬 md 파일 (항상 최신 상태 유지)
- **총괄PD 확인 공간**: Notion (기획 확인, 피드백, 외부 공유용)
- **동기화 흐름**: 기획팀장이 로컬 md 수정 → Notion에도 최신화 → git 커밋
- **동기화 책임**: 기획팀장 (game-designer)
- **에이전트는 Notion을 직접 참조하지 않음** (로컬 md만 참조)
- **총괄PD 피드백**: 총괄PD가 Notion에서 기획을 수시로 확인하고 기획팀장에게 피드백

---

## 빌드/배포 정책
- 빌드 절차는 `soulspire-build-deploy` 스킬 참조
- **프로토타입 기간: 빌드 전 반드시 총괄PD 승인 필요**
- **로컬 직접 복사 테스트 통과 후에만 Steam 업로드 진행**

---

## QA 정책
- QA 운영 절차는 `soulspire-qa-ops` 스킬 참조
- **sprint 브랜치 머지는 QA팀장만 가능** (QA 통과 필수)
- **빌드 전 BAT 필수** (1건이라도 실패 시 빌드 금지)

---

## 스프린트 운영
- **공수 산정**: 에이전트 병렬 실행 기준 1시간 = 단일 에이전트 6~8시간 분량
- 모든 팀장에게 업무 배분 (유휴 팀 최소화), 팀당 2~3건
- 상세 지침: `Docs/DevPD_Guidelines.md` 8.1절

---

## 에이전트/스킬 거버넌스
- frontmatter 수정, 스킬 생성/변경, reference/asset 구성 변경은 **반드시 총괄PD 컨펌 후 실행**
- 스킬 생성/수정 후 6항목 자동 검증 필수 (SKILL.md 존재, frontmatter 유효성, description/body 길이, 참조 유효성, 미사용 리소스)
- 에이전트/스킬 변경 시 노션 "에이전트 - 스킬 매핑" 페이지 동기화 필수
- 스프린트 종료 후 각 팀장이 게임 플레이 후 개선점 보고서 PPT 3페이지 이내 작성

---

## CLAUDE.md / MEMORY.md 관리 원칙
- **CLAUDE.md** (git 커밋): 따라야 할 **규칙과 지시** — 팀 정책, 컨벤션, 워크플로우, 코딩 표준
- **MEMORY.md** (로컬 `~/.claude/`): 작업에 도움이 되는 **맥락 정보** — 환경 설정, 민감 정보, 디버깅 경험, 학습한 패턴
- 판단 기준: "이것은 따라야 할 규칙인가?" → CLAUDE.md / "이것은 작업 맥락 정보인가?" → MEMORY.md
- MEMORY.md는 CLAUDE.md와 **중복 금지** — 규칙이 CLAUDE.md에 있으면 MEMORY.md에 다시 쓰지 않음
- **수정 권한**: CLAUDE.md와 MEMORY.md 모두 **총괄PD 승인 후에만 수정** (에이전트/스킬 거버넌스와 동일 기준)

---

## 중요 교훈
- **UPM 패키지에는 반드시 .meta 파일 필요**: Unity는 .meta 없이 asmdef, .cs 인식 불가
- 새 패키지 생성 시 모든 파일/폴더에 .meta 파일 생성 필수 (GUID 32자 hex)
