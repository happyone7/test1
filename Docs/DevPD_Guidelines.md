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
2. 실무자 자체 QA: 각 팀장은 작업 후 동작 확인
3. QA 조기 착수: 각 팀장 작업 단위 완료 시 즉시 해당 부분 QA
4. UI 작업은 반드시 UI팀장 경유
5. TA↔UI 역할 분리: TA는 컨셉+이미지, UI팀장은 시스템 구현
6. 기획팀장 적극 활용: 수치 밸런싱, 레벨 디자인, SO 관리

---

## 3. Git 브랜칭 및 커밋 규칙

### 3.1 브랜칭 전략 (QA 게이트 방식)

```
feature/phase1-core-loop  ← 기존 (Sprint 1~2, 레거시)
sprint3                   ← 메인 작업 브랜치 (QA팀장만 머지 가능)
  ├─ dev/programmer       ← 프로그래밍팀장 작업 브랜치
  ├─ dev/ui               ← UI팀장 작업 브랜치
  ├─ dev/ta               ← TA팀장 작업 브랜치
  ├─ dev/game-designer    ← 기획팀장 작업 브랜치
  ├─ dev/sound            ← 사운드 디렉터 작업 브랜치
  └─ dev/build            ← 빌더 작업 브랜치
```

**핵심 규칙:**
1. **메인 작업 브랜치 이름**: `sprint2`, `sprint3`, `sprint4` 등 스프린트 단위
2. **각 팀장**: 자기 이름의 `dev/*` 브랜치에서 작업 → 자체 QA 후 커밋
3. **sprint 브랜치 머지 권한**: **QA팀장만 가능** (동작 검증 통과 후 머지)
4. **직접 sprint 브랜치 커밋 금지**: 모든 팀장은 반드시 `dev/*` 브랜치 경유

**워크플로우:**
```
팀장: dev/* 브랜치에서 작업 + 자체 QA + 커밋
    ↓
QA팀장: dev/* 브랜치 동작 검증
    ↓ (통과)
QA팀장: sprint* 메인 브랜치로 머지
    ↓ (실패)
QA팀장: 해당 팀장에게 수정 요청 → 팀장 수정 후 재검증
```

**브랜치 생성 명령 (각 팀장이 작업 시작 전 실행):**
```bash
git checkout sprint3
git pull
git checkout -b dev/programmer   # 프로그래밍팀장
git checkout -b dev/ui           # UI팀장
git checkout -b dev/ta           # TA팀장
# ... (각 팀장별)
```

**QA팀장 머지 명령:**
```bash
git checkout sprint3
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
| 스프린트 진행 현황 | `Docs/Sprint1_Progress.md` | 작업 상태 추적 |
| 스프린트 피드백 | `Docs/Design/Sprint1_Feedback.md` | 총괄PD 피드백 |
| GDD | `Docs/Design/GDD.md` | 게임 디자인 문서 |
| 아트 디렉션 | `Docs/Design/ArtDirection_v0.1.md` | 아트 가이드 |
| 이 지침서 | `Docs/DevPD_Guidelines.md` | 개발PD 운영 기준 |
| MEMORY.md | `.claude/projects/.../memory/MEMORY.md` | 영구 기억 (Claude Code 전용) |

### 4.2 작업 전 확인 사항
1. `Docs/Sprint1_Progress.md`를 먼저 읽고 현재 진행 상황 파악
2. `git log --oneline -20`으로 최근 커밋 확인 (다른 PD가 작업한 내용)
3. `git status`로 미커밋 변경사항 확인

### 4.3 작업 후 필수 사항
1. 해당 팀장 이름으로 커밋
2. PM 에이전트를 호출하여 Sprint1_Progress.md 갱신
3. 다른 팀에 영향을 주는 변경이면 Sprint1_Progress.md에 메모

---

## 5. QA 프로세스 (QA 게이트 방식)

### 5.1 QA 2계층 체계

| 계층 | 담당 | 플랫폼 | 역할 |
|------|------|--------|------|
| **QA 실행** | Codex CLI | OpenAI Codex (gpt-5.3-codex) | QA 시트 기반 전체 항목 검증 (MCP Unity 활용) |
| **QA 관리/감독** | QA팀장 (Claude) | Claude Code 에이전트 | QA 룰 정리, 크리티컬 버그 직접 처리, 팀간 의사소통 |

**역할 분담 원칙:**
- **Codex CLI**: 모든 QA 항목을 보고 직접 검증 수행 (비용 절감). MCP Unity 연동으로 에디터 상태/스크린샷/컴파일 에러 확인 가능
- **QA팀장 (Claude)**: QA 룰/시트 관리, 크리티컬 버그(전 팀 중단 필요 수준)만 직접 처리, **팀간 의사소통 담당** (Codex CLI는 에이전트 간 메시지 불가)

**Codex CLI QA 실행 방법:**
```bash
# 프로젝트 루트에서 실행
codex -q "Docs/QA_Sheet.md를 읽고 모든 PENDING 항목을 순서대로 검증해줘. MCP Unity 도구를 사용하여 에디터 상태, 스크린샷, 컴파일 에러를 확인하고, 각 항목의 상태를 PASS/FAIL/BLOCKED로 업데이트해. 결과를 Docs/QA_Sheet.md에 직접 반영해줘."
```

### 5.2 단위 QA + 머지 게이트 (Sprint 중)
- 각 팀장이 `dev/*` 브랜치에서 작업 완료 + 자체 QA 후 커밋
- **Codex CLI가 해당 `dev/*` 브랜치를 검증** (QA팀장이 Codex 실행)
- **검증 통과 → QA팀장이 sprint 브랜치로 머지**
- **검증 실패 → QA팀장이 해당 팀장에게 수정 요청 (의사소통 담당)**

### 5.3 통합 QA (빌드 직전)
- 모든 dev/* 브랜치가 sprint 브랜치에 머지 완료 후
- **Codex CLI가 sprint 브랜치에서 전체 플로우 통합 테스트** 수행
- QA 통과 → 총괄PD 승인 → 빌더가 빌드

### 5.4 QA팀장 권한과 역할
- **sprint 브랜치 머지 권한**: QA팀장만 보유 (다른 팀장 직접 머지 금지)
- **Codex CLI QA 실행 관리**: 언제/어떤 항목을 Codex에 돌릴지 판단
- **크리티컬 버그 직접 처리**: 전 팀 중단 필요 수준의 이슈만 직접 개입
- **팀간 의사소통 브릿지**: Codex가 발견한 이슈를 해당 팀장에게 전달
- 병목 구간 식별하여 PM/개발PD에게 보고

### 5.5 Codex CLI 설정 (참고)
```toml
# ~/.codex/config.toml
model = "gpt-5.3-codex"
model_reasoning_effort = "high"

[mcp_servers.unity-mcp]
url = "http://127.0.0.1:8080/mcp"
```

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

---

## 9. Sprint 2 계획 (1시간 축소판, 총괄PD 승인됨)

**계획서 전문**: `Docs/Sprint2_Plan.md`

### 핵심 목표
1. **기본 아트 Unity 적용** - 스타일 락 완료된 타워/몬스터/타일 스프라이트
2. **밸런싱 수치 기획 의도 반영** - 기획팀장 SO 직접 입력
3. **프리팹/Git 프로세스 정리** - 씬 오브젝트 프리팹화

### 팀별 업무 (병렬 진행)
| 담당 | 태스크 | 예상 시간 |
|------|--------|-----------|
| 기획팀장 | SO 밸런싱 수치 입력 | ~15분 |
| TA팀장 | Arrow Tower/몬스터3종/타일셋5종 스프라이트 적용 | ~20분 |
| 프로그래밍팀장 | 씬 오브젝트 프리팹화 정리 | ~15분 |
| QA팀장 | 위 작업 완료 후 통합 검증 | ~10분 |

### 제외 항목 (Sprint 3 이후)
- 스킬트리 UI 기획서/구현, 추가 타워/몬스터 코드, 사운드 폴리싱, Steam 빌드

---

## 변경 이력

| 날짜 | 변경 내용 |
|------|-----------|
| 2026-02-15 | 초안 작성 — 3인 개발PD 정보공유 체계, 팀장별 git author, QA 조기착수, 기획팀장 활용 |
| 2026-02-15 | Sprint 2 계획 추가 (1시간 축소판, 총괄PD 승인) |
