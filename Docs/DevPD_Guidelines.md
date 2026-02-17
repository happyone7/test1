# 개발PD 지침서

**목적**: 개발PD가 여러 채팅(분신)으로 운영될 때, 모든 개발PD가 동일한 기준으로 작업하기 위한 공유 지침

---

## 1. 개발PD 현황

| PD | 플랫폼 | 비고 |
|----|--------|------|
| 개발PD 01 | Claude Code | |
| 개발PD 02 | Claude Code | |
| 개발PD 03 | Codex | |

---

## 2. 핵심 원칙

### 2.1 개발PD는 직접 작업 금지
- 코딩, UI 구현, 빌드, 아트 작업 등 실무는 **반드시 해당 팀장 에이전트에게 위임**
- 개발PD 역할: 업무 조율, 방향 설정, 팀간 소통, 진행 관리

### 2.2 팀장 에이전트 목록

| 역할 | 에이전트 | 담당 |
|------|----------|------|
| 기획팀장 | game-designer | 메카닉, 밸런싱, SO 수치, 레벨 디자인 |
| 프로그래밍팀장 | unity-gameplay-programmer | 게임 로직, 시스템 코드 |
| QA팀장 | unity-qa-engineer | 품질 검증 (조기 착수) |
| UI팀장 | unity-ui-developer | UI/UX 구현 |
| TA팀장 | unity-technical-artist | 아트, 셰이더, VFX, ComfyUI |
| 사운드 디렉터 | unity-sound-director | BGM/SFX |
| 빌더 | unity-build-engineer | 빌드, 배포 |
| PM | project-manager | 스프린트 진행 관리, 문서 갱신 |

### 2.3 위임 원칙
1. 개발PD는 절대 직접 코딩/UI/빌드 작업하지 않음
2. **에이전트 실패 시에도 개발PD 직접 작업 금지** — 네트워크 오류/MCP 장애/API 에러 등으로 에이전트가 실패하면 원인 해결 후 에이전트를 재실행할 것. 어떤 예외 상황에서도 개발PD가 "직접 하겠다"고 판단해서는 안 됨
3. 실무자 자체 QA: 각 팀장은 작업 후 동작 확인
4. QA 조기 착수: 각 팀장 작업 단위 완료 시 즉시 해당 부분 QA
5. UI 작업은 반드시 UI팀장 경유
6. TA↔UI 역할 분리: TA는 컨셉+이미지, UI팀장은 시스템 구현
7. 기획팀장 적극 활용: 수치 밸런싱, 레벨 디자인, SO 관리

### 2.4 에이전트 실패 복구 절차
에이전트가 실패(권한 오류, MCP 타임아웃, 네트워크 장애 등)할 경우:
1. **에러 원인 진단**: 에러 메시지를 확인하고 원인 분류 (권한, MCP, 네트워크, 코드 오류)
2. **원인 해결**: 설정 수정, 서버 재시작, 의존성 설치 등
3. **에이전트 재실행**: 동일 업무를 에이전트에게 다시 위임
4. **절대 개발PD가 직접 대행하지 않음**: 반복 실패 시에도 원인 해결 → 재실행 루프 유지
5. **3회 이상 실패 시**: 총괄PD에게 보고하고 대응 방향 결정

---

## 3. Git 브랜칭 및 커밋 규칙

### 3.1 브랜칭 전략 (QA 게이트 방식)

```
main                      ← 릴리스/배포 (Steam 빌드 기준)
 └── sprint/N             ← 메인 작업 브랜치 (QA팀장만 머지 가능)
      ├── feature/*       ← 크로스팀 기능 통합 (2개+ 팀 협업 시)
      ├─ dev/programmer   ← 프로그래밍팀장 작업 브랜치
      ├─ dev/ui           ← UI팀장 작업 브랜치
      ├─ dev/ta           ← TA팀장 작업 브랜치
      ├─ dev/game-designer← 기획팀장 작업 브랜치
      ├─ dev/sound        ← 사운드 디렉터 작업 브랜치
      └─ dev/build        ← 빌더 작업 브랜치
```

**핵심 규칙:**
1. **메인 작업 브랜치 이름**: `sprint/N` (N = 스프린트 번호)
2. **각 팀장**: 자기 이름의 `dev/*` 브랜치에서 작업 → 자체 QA 후 커밋
3. **sprint 브랜치 머지 권한**: **QA팀장만 가능** (동작 검증 통과 후 머지)
4. **직접 sprint 브랜치 커밋 금지**: 모든 팀장은 반드시 `dev/*` 브랜치 경유

**워크플로우:**
```
팀장: dev/* 브랜치에서 작업 + 자체 QA + 커밋
    ↓
QA팀장: dev/* 브랜치 동작 검증
    ↓ (통과)
QA팀장: sprint/N 메인 브랜치로 머지
    ↓ (실패)
QA팀장: 해당 팀장에게 수정 요청 → 팀장 수정 후 재검증
```

**브랜치 생성 명령 (각 팀장이 작업 시작 전 실행):**
```bash
git checkout sprint/N
git pull
git checkout -b dev/programmer   # 프로그래밍팀장
git checkout -b dev/ui           # UI팀장
git checkout -b dev/ta           # TA팀장
# ... (각 팀장별)
```

**QA팀장 머지 명령:**
```bash
git checkout sprint/N
git merge --no-ff dev/programmer -m "merge: 프로그래밍팀장 작업 머지 (QA 통과)"
```

### 3.2 팀장별 author 설정
각 팀장 에이전트가 커밋할 때 아래 author를 사용:

```bash
# 기획팀장
git commit --author="GameDesigner <game-designer@soulspire.dev>"

# 프로그래밍팀장
git commit --author="GameplayProgrammer <gameplay-programmer@soulspire.dev>"

# QA팀장
git commit --author="QAEngineer <qa-engineer@soulspire.dev>"

# UI팀장
git commit --author="UIDeveloper <ui-developer@soulspire.dev>"

# TA팀장
git commit --author="TechnicalArtist <technical-artist@soulspire.dev>"

# 사운드 디렉터
git commit --author="SoundDirector <sound-director@soulspire.dev>"

# 빌더
git commit --author="BuildEngineer <build-engineer@soulspire.dev>"

# PM
git commit --author="ProjectManager <project-manager@soulspire.dev>"

# 개발PD (조율 작업만)
git commit --author="DevPD <dev-pd@soulspire.dev>"
```

### 3.3 커밋 메시지 규칙
- **한글로 작성**
- 접두사: `feat:`, `fix:`, `chore:`, `docs:`, `refactor:`, `art:`, `sound:`, `balance:`
- 예시: `feat: 돌진 마물(Charger) 기본 AI 구현`
- 예시: `balance: Stage 01 웨이브 3 몬스터 수 조정`
- 예시: `art: Arrow Tower L1~L4 스프라이트 추가`

### 3.4 커밋 단위
- 팀장별 작업 단위마다 커밋 (한 번에 몰아서 커밋 금지)
- 기능 단위로 분리: 하나의 커밋에 하나의 기능/수정

---

## 4. 정보 공유 체계

### 4.1 공유 문서 (모든 개발PD가 참조)
| 문서 | 경로 | 용도 |
|------|------|------|
| 스프린트 진행 현황 | `Docs/SprintN_Progress.md` | 작업 상태 추적 (N = 스프린트 번호) |
| 스프린트 계획 | `Docs/SprintN_Plan.md` | 스프린트 목표 및 업무 배분 |
| GDD | `Docs/Design/GDD.md` | 게임 디자인 문서 |
| 아트 디렉션 | `Docs/Design/ArtDirection_v0.1.md` | 아트 가이드 |
| 이 지침서 | `Docs/DevPD_Guidelines.md` | 개발PD 운영 기준 |
| MEMORY.md | `.claude/projects/.../memory/MEMORY.md` | 영구 기억 (Claude Code 전용) |

### 4.2 작업 전 확인 사항
1. 해당 스프린트의 `Docs/SprintN_Progress.md`를 먼저 읽고 현재 진행 상황 파악
2. `git log --oneline -20`으로 최근 커밋 확인 (다른 PD가 작업한 내용)
3. `git status`로 미커밋 변경사항 확인

### 4.3 작업 후 필수 사항
1. 해당 팀장 이름으로 커밋
2. PM 에이전트를 호출하여 SprintN_Progress.md 갱신
3. 다른 팀에 영향을 주는 변경이면 SprintN_Progress.md에 메모

---

## 5. QA 프로세스 (QA 게이트 방식)

### 5.1 단위 QA + 머지 게이트 (Sprint 중)
- 각 팀장이 `dev/*` 브랜치에서 작업 완료 + 자체 QA 후 커밋
- QA팀장이 해당 `dev/*` 브랜치를 체크아웃하여 동작 검증
- **검증 통과 → QA팀장이 sprint 브랜치로 머지**
- **검증 실패 → 해당 팀장에게 수정 요청, 수정 후 재검증**

### 5.2 통합 QA (빌드 직전)
- 모든 dev/* 브랜치가 sprint 브랜치에 머지 완료 후
- sprint 브랜치에서 전체 플로우 통합 테스트
- QA 통과 → 총괄PD 승인 → 빌더가 빌드

### 5.3 QA팀장 권한과 역할
- **sprint 브랜치 머지 권한**: QA팀장만 보유 (다른 팀장 직접 머지 금지)
- 각 팀장의 진척도를 모니터링
- 먼저 QA할 수 있는 부분을 찾아 선제적으로 진행
- 병목 구간 식별하여 PM/개발PD에게 보고

---

## 6. 기획팀장 활용 가이드

### 6.1 기획팀장이 담당하는 업무
- ScriptableObject 수치 조정 (몬스터 HP/속도/보상, 타워 공격력/사거리/비용)
- 스테이지별 웨이브 구성 (몬스터 수/종류/스폰 간격)
- 난이도 곡선 설계
- 게임 밸런스 시뮬레이션
- 신규 컨텐츠 기획 (새 타워/몬스터 설계)

### 6.2 프로그래밍팀장과의 협업
- 프로그래밍팀장: 로직/시스템 코드 구현
- 기획팀장: SO 기반 수치 데이터 관리
- 프로그래밍팀장이 SO 구조를 만들면 → 기획팀장이 수치 채움

### 6.3 수시 업무
- 프로그래밍팀장에게 업무가 몰리지 않도록 수치 작업을 먼저 처리
- 새 기능이 추가되면 즉시 밸런싱 수치 설계

---

## 7. PM 운영 가이드

### 7.1 PM 핵심 역할
- 스프린트 진행 현황 취합 및 문서 갱신
- 병목 구간 식별 → 개발PD 컨펌 하에 업무 재분배
- 스프린트 일정 내 완료 보장

### 7.2 PM 보고 포맷
```
## 스프린트 현황 요약
- 완료: N건 / 진행중: N건 / 대기: N건
- 주요 진전: [한줄 요약]
- 블로커: [있으면 기술, 없으면 "없음"]
- 다음 예정: [우선순위 높은 작업 1~2개]
```

---

## 8. 스프린트 운영 흐름

```
총괄PD 방향 설정
    ↓
개발PD: 업무 분배 + sprint 브랜치 생성
    ↓
각 팀장: dev/* 브랜치에서 작업 + 자체 QA + 커밋
    ↓
QA팀장: dev/* 브랜치 동작 검증 → 통과 시 sprint 브랜치에 머지
    ↓
PM: 진행 현황 갱신 + 병목 보고
    ↓
개발PD: 병목 조정 (필요시)
    ↓
모든 dev/* 머지 완료 → 통합 QA → 총괄PD 승인 → 빌드
```

### 8.1 스프린트 공수 산정 기준 (필수 준수)

**핵심 원칙: 에이전트는 병렬 실행된다.**

- 팀장 에이전트 6~8개가 **동시에** 작업 가능
- 따라서 1시간 스프린트 = **단일 에이전트 6~8시간 분량**의 작업
- PM은 공수 산정 시 "병렬 팀 수 × 시간"으로 계산해야 함

**잘못된 산정 (금지):**
- "1시간이니까 팀당 1개 태스크만" → 너무 소극적
- 순차적 기준으로 업무량 산정 → 병렬 실행을 고려하지 않음
- 투입 팀을 줄여서 범위 축소 → 유휴 에이전트 낭비

**올바른 산정:**
- 모든 팀장에게 업무 배분 (유휴 팀 최소화)
- 각 팀장에게 P0 + P1 업무를 2~3개씩 배정
- 의존성 없는 작업은 모두 동시 착수
- 의존성 있는 작업은 선행 완료 직후 즉시 후행 착수
- QA는 각 팀 작업 완료 시 즉시 단위 검증 (최종 통합 QA만 대기)

**1시간 스프린트 기대 산출량 예시:**
- 기획팀장: SO 수치 + 밸런스시트 + 기획서 초안 (2~3건)
- 프로그래밍팀장: 기능 구현 + 버그 수정 (2~3건)
- UI팀장: UI 에셋 적용 + 화면 개선 (2~3건)
- TA팀장: 스프라이트 적용 + VFX + 임포트 설정 (2~3건)
- 사운드 디렉터: 리소스 교체 + 믹싱 (1~2건)
- QA팀장: 단위 QA 3~4건 + 통합 QA 1건

---

## 9. Git Worktree 운영 (단일 로컬 머신)

### 9.1 문제
단일 머신에서 `git checkout`으로 브랜치를 전환하면:
- Unity 씬 파일(.unity)이 24,000줄 이상 변경되어 충돌 발생
- Library/ 캐시 무효화로 에셋 재임포트 필요
- stash 누적으로 작업 상태 오염

### 9.2 Worktree 구조
각 dev/* 브랜치에 독립 워킹 디렉토리를 할당:

```
C:\UnityProjects\
  Soulspire\              ← 메인 (Unity 에디터 연결, feature/ 또는 sprint 브랜치)
  wt-dev-programmer\      ← dev/programmer 전용
  wt-dev-ui\              ← dev/ui 전용
  wt-dev-ta\              ← dev/ta 전용
  wt-dev-game-designer\   ← dev/game-designer 전용
  wt-dev-sound\           ← dev/sound 전용
  wt-dev-build\           ← dev/build 전용
```

### 9.3 Worktree 사용 규칙
1. **스크립트/SO/문서 편집**: worktree 디렉토리에서 직접 편집 + 커밋 (브랜치 전환 불필요)
2. **Unity MCP 작업** (씬 수정, 컴포넌트 조작, 플레이모드): 메인 프로젝트에서만 수행
   - 메인 프로젝트의 브랜치를 해당 dev/*로 전환 필요
   - 전환 전 반드시 현재 브랜치 커밋 완료 (stash 금지)
   - 전환 후 `refresh_unity` 필수
3. **Worktree 갱신**: sprint 브랜치에서 dev/*로 최신 변경 가져오기
   ```bash
   cd C:\UnityProjects\wt-dev-programmer
   git merge sprint/N --no-edit
   ```

### 9.4 브랜치 전환 프로토콜 (Unity MCP 작업 시)
```
1. 현재 브랜치에서 모든 변경 커밋 (WIP 커밋 허용, stash 금지)
2. git checkout dev/{팀장} 실행
3. refresh_unity 실행 (에셋 재인식)
4. read_console — 컴파일 에러 확인
5. 작업 수행
6. 작업 완료 후 커밋
7. 다른 브랜치로 전환 시 1번부터 반복
```

**금지 사항:**
- `git stash` 사용 금지 → 반드시 커밋 (WIP 접두사 허용)
- 커밋 없이 `git checkout` 금지
- 동시에 두 팀이 같은 .unity 씬 파일 수정 금지

---

## 10. 씬 수정 규칙 (프리팹 기반)

### 10.1 문제
GameScene.unity에 UI 오브젝트가 직접 배치되어 있어, 여러 팀이 동시에 수정하면 머지 불가능.

### 10.2 프리팹 추출 대상
현재 씬에 직접 존재하는 UI 오브젝트를 프리팹으로 분리:

| 씬 오브젝트 | 프리팹 경로 | 담당 |
|------------|------------|------|
| Canvas/TopHUD | Assets/Prefabs/UI/TopHUD.prefab | UI팀장 |
| Canvas/RunEndPanel | Assets/Prefabs/UI/RunEndPanel.prefab | UI팀장 |
| Canvas/HubPanel | Assets/Prefabs/UI/HubPanel.prefab | UI팀장 |
| Canvas/BottomBar | Assets/Prefabs/UI/BottomBar.prefab | UI팀장 |
| Canvas/SettingsOverlay | Assets/Prefabs/UI/SettingsOverlay.prefab | UI팀장 |
| Canvas/TowerPurchasePanel | Assets/Prefabs/UI/TowerPurchasePanel.prefab | UI팀장 |
| Canvas/TowerInfoTooltip | Assets/Prefabs/UI/TowerInfoTooltip.prefab | UI팀장 |
| Canvas/IdleBitOverlay | Assets/Prefabs/UI/IdleBitOverlay.prefab | UI팀장 |
| Canvas/InventoryBar | Assets/Prefabs/UI/InventoryBar.prefab | UI팀장 |
| Canvas/HpWarningOverlay | Assets/Prefabs/UI/HpWarningOverlay.prefab | UI팀장 |
| TitleScreenCanvas/TitleScreenPanel | Assets/Prefabs/UI/TitleScreenPanel.prefab | UI팀장 |
| ConnectionLine | Assets/Prefabs/UI/ConnectionLine.prefab | 프로그래밍팀장 |

### 10.3 씬 수정 권한
프리팹화 완료 후:
- **씬 직접 수정**: 프로그래밍팀장만 (매니저 오브젝트, 게임 로직)
- **UI 수정**: UI팀장이 프리팹 파일 수정 (씬 터치 불필요)
- **TA/사운드**: 에셋 파일만 수정 (씬/프리팹 직접 수정 금지)
- 씬을 수정해야 하는 상황이 생기면 개발PD에게 보고 후 순서 조율

---

## 11. 디자인 문서 관리 (로컬 md 기준 + Notion 동기화)

### 11.1 원칙
- **에이전트 기준 문서**: `Docs/Design/` 로컬 md 파일 (항상 최신 상태 유지)
- **총괄PD 확인 공간**: Notion (기획 확인, 피드백, 외부 공유용)
- **에이전트는 Notion을 직접 참조하지 않음** — 로컬 md만 참조

### 11.2 동기화 흐름
- 기획팀장이 로컬 md 수정 → Notion에도 최신화 → git 커밋
- **동기화 책임**: 기획팀장 (game-designer)

### 11.3 Notion 디자인 문서 위치
- 부모 페이지: Soulspire 프로젝트 > 디자인 문서
- 각 문서는 하위 페이지로 생성

### 11.4 문서 업데이트 규칙
- 기획팀장이 로컬 md 수정 후 Notion에 동기화
- 중요 변경은 커밋 메시지나 PM 보고에 언급
- 총괄PD가 Notion에서 수시로 확인하고 기획팀장에게 피드백

---

## 12. 스프린트 종료 필수 체크리스트

스프린트 코드 머지 완료 후, 아래 항목을 **모두 수행해야** 스프린트 종료로 인정:

### 12.1 필수 프로세스 (순서대로)
| # | 항목 | 담당 | 설명 |
|---|------|------|------|
| 1 | SprintN_Progress.md 작성 | PM | 모든 작업 항목 상태, 커밋 해시, 이슈 기록 |
| 2 | DevPD 문서 리뷰 | 개발PD | 스프린트 중 발생 문제 분석 → 원인 문서 수정 적용 |
| 3 | 팀 플레이테스트 + 리뷰 회의 | 전 팀장 | 빌드 플레이 → 현재 빌드 평가 + 개선 방향 공유 |
| 4 | 각 팀장 개선점 보고서 | 각 팀장 | 게임 플레이 후 PPT 3페이지 이내 작성 |
| 5 | Notion 동기화 | PM | 스프린트 계획서 + 진행 기록을 Notion에 동기화 |
| 6 | 빌드 | 빌더 | 총괄PD 승인 후 Windows 빌드 + Steam 업로드 |

### 12.2 주의사항
- **코드 머지 + 태그 = 완료가 아님**: 위 6개 항목까지 마쳐야 스프린트 종료
- 스프린트 종료 프로세스 미수행 시, 다음 스프린트 착수 금지
- DevPD 문서 리뷰 대상: CLAUDE.md, DevPD_Guidelines.md, 스킬, 에이전트, prefab-protocol 등

---

## 13. 서브에이전트 권한 관리

### 13.1 settings.json vs settings.local.json
| 파일 | 적용 범위 | 용도 |
|------|-----------|------|
| `.claude/settings.json` | **서브에이전트 포함 전체** | 프로젝트 수준 공통 권한 (git 커밋 대상) |
| `.claude/settings.local.json` | 메인 세션만 | 개인 로컬 설정 (gitignore 대상) |

### 13.2 주의사항
- 서브에이전트는 `settings.json`만 상속받음 (`settings.local.json` 미상속)
- 에이전트에게 필요한 Bash 권한은 반드시 `settings.json`에 추가
- 권한 변경은 총괄PD 승인 후 적용 (에이전트/스킬 거버넌스 동일 기준)

---

## 변경 이력

| 날짜 | 변경 내용 |
|------|-----------|
| 2026-02-15 | 초안 작성 — 3인 개발PD 정보공유 체계, 팀장별 git author, QA 조기착수, 기획팀장 활용 |
| 2026-02-15 | Sprint 2 계획 추가 (1시간 축소판, 총괄PD 승인) |
| 2026-02-16 | 9~11절 추가: Git Worktree 운영, 씬 수정 규칙(프리팹 기반), 디자인 문서 Notion 이전 |
| 2026-02-17 | Sprint 6 DevPD 문서 리뷰: 위임 원칙 2조 추가, 에이전트 실패 복구 절차, 브랜치 일반화(sprint/N), 문서 참조 일반화(SprintN), 디자인 문서 정책 CLAUDE.md 통일, Sprint 2 레거시 제거, 스프린트 종료 체크리스트 신설, 서브에이전트 권한 관리 절 신설 |
