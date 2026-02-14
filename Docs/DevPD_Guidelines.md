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

## 3. Git 커밋 규칙

### 3.1 팀장별 author 설정
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

### 3.2 커밋 메시지 규칙
- **한글로 작성**
- 접두사: `feat:`, `fix:`, `chore:`, `docs:`, `refactor:`, `art:`, `sound:`, `balance:`
- 예시: `feat: 돌진 마물(Charger) 기본 AI 구현`
- 예시: `balance: Stage 01 웨이브 3 몬스터 수 조정`
- 예시: `art: Arrow Tower L1~L4 스프라이트 추가`

### 3.3 커밋 단위
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

## 5. QA 프로세스 (개선됨)

### 5.1 조기 QA (Sprint 중)
- 각 팀장이 작업 단위를 완료할 때마다 QA팀장이 해당 부분 즉시 검증
- 프로그래밍팀장 기능 완성 → QA팀장 해당 기능 QA (병렬)
- UI팀장 화면 완성 → QA팀장 UI QA (병렬)

### 5.2 통합 QA (빌드 직전)
- 모든 작업 완료 후 전체 플로우 통합 테스트
- QA 통과 → 총괄PD 승인 → 빌더가 빌드

### 5.3 QA팀장 주도적 역할
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
개발PD: 업무 분배 (이 지침서 기준)
    ↓
각 팀장: 작업 수행 + 자체 QA + 팀장 이름으로 커밋
    ↓
QA팀장: 작업 단위별 조기 QA (병렬)
    ↓
PM: 진행 현황 갱신 + 병목 보고
    ↓
개발PD: 병목 조정 (필요시)
    ↓
모든 작업 완료 → 통합 QA → 총괄PD 승인 → 빌드
```

---

## 변경 이력

| 날짜 | 변경 내용 |
|------|-----------|
| 2026-02-15 | 초안 작성 — 3인 개발PD 정보공유 체계, 팀장별 git author, QA 조기착수, 기획팀장 활용 |
