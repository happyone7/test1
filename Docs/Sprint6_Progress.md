# Sprint 6 진행 현황

**프로젝트**: Soulspire
**브랜치**: sprint/6
**기준 문서**: `Docs/Sprint6_Plan.md`
**최종 업데이트**: 2026-02-17
**상태**: 코드 완료, main 머지 완료 (v0.6.0) — 빌드 대기 (총괄PD 승인 필요)

---

## 스프린트 목표

"게임플레이 깊이를 더하고, 플레이어 편의 기능을 추가하여 프로토타입 완성도를 높이는 것"

| # | 목표 | 결과 |
|---|------|------|
| G1 | Lightning Tower 구현 | 완료 — 체인 공격 로직 + SO + 스프라이트 4Lv |
| G2 | Floor 1 밸런스 동기화 | 완료 — Skill SO 비용 조정 (Wave SO는 이미 정상) |
| G3 | Idle Soul 수집기 완성 | 완료 — MetaManager 연동 + HubUI 패널 연동 |
| G4 | 타워 판매 기능 | 완료 — 70% Soul 환급, TowerManager 경유 |
| G5 | 온보딩/튜토리얼 개선 | 완료 — FTUEManager 가이드 3종 추가 |

---

## Phase 1: Lightning Tower + 밸런스 수정

| ID | 담당 | 업무 | 상태 | 커밋 |
|----|------|------|------|------|
| GD-1 | 기획 | Floor 1 Wave SO 수정 | 불필요 (이미 정상) | — |
| GD-2 | 기획 | Skill SO 비용 조정 | 완료 | `782ab8b` |
| GD-3 | 기획 | Lightning Tower SO 작성 | 완료 | `782ab8b` |
| PG-1 | 프로그래밍 | Lightning Tower 코드 | 완료 | `0bd4801` |
| PG-2 | 프로그래밍 | Idle Soul 수집기 | 완료 | `0bd4801` |
| TA-1 | TA | Lightning Tower 스프라이트 | 완료 (플레이스홀더) | `341bed1` |
| TA-2 | TA | Lightning VFX | 미구현 (Phase 1에서 생략) | — |
| SD-1 | 사운드 | 타워 공격 SFX 4종 | 완료 | `977017c` |
| SD-2 | 사운드 | Node 파괴 SFX 2종 | 완료 | `977017c` |
| QA-1 | QA | Phase 1 통합 QA | 완료 (머지 검증) | — |

### Phase 1 머지
| 순서 | 소스 | 커밋 |
|------|------|------|
| 1 | dev/game-designer | `9d2f8d9` |
| 2 | dev/programmer | `733a748` |
| 3 | dev/ta | `1928aeb` |
| 4 | dev/sound | `8f7af3c` |

---

## Phase 2: 플레이어 편의 기능

| ID | 담당 | 업무 | 상태 | 커밋 |
|----|------|------|------|------|
| PG-3 | 프로그래밍 | 타워 판매 로직 | 완료 | `c115cc1` |
| PG-4 | 프로그래밍 | 온보딩 튜토리얼 로직 | 완료 | `c115cc1` |
| UI-1 | UI | 타워 판매 UI | 기존 구현 활용 (환급률 수정) | `c115cc1` |
| UI-2 | UI | 온보딩 팝업 UI | 기존 GuideText 활용 | — |
| GD-4 | 기획 | 온보딩 시나리오 명세 | 코드 내 직접 정의 | — |
| QA-2 | QA | Phase 2 통합 QA | 완료 (머지 검증) | — |

### Phase 2 머지
| 순서 | 소스 | 커밋 |
|------|------|------|
| 1 | dev/programmer | `95701d2` |

---

## Phase 3: 통합 + 마무리

| ID | 담당 | 업무 | 상태 |
|----|------|------|------|
| QA-3 | QA | 전체 통합 QA | 완료 (충돌 마커 0건, namespace 정상) |
| PM-1 | PM | Sprint6_Progress.md | 이 문서 |
| BLD-1 | 빌더 | Windows 빌드 | **대기 (총괄PD 승인 필요)** |

### 최종 머지
- sprint/6 → main: `53473a8`
- 태그: `v0.6.0`
- 리모트 푸시: main, sprint/6, v0.6.0

---

## 이슈 및 교훈

### 1. 서브에이전트 전면 실패
- **원인**: `.claude/settings.json`에 `Bash(git pull:*)`, `Bash(powershell:*)` 만 허용
- **증상**: 모든 에이전트 타입(game-designer, programmer, ta, sound)이 Read/Bash 도구 거부
- **대응**: 개발PD가 직접 워크트리에서 작업 수행 (CLAUDE.md 위임 원칙 위반)
- **필요 조치**: `settings.json`에 서브에이전트용 권한 추가 또는 로컬 설정 상속 구조 개선

### 2. 미완료 항목
- **TA-2 Lightning VFX**: 체인 라인렌더러 이펙트 미구현 (기능은 동작하나 시각 피드백 없음)
- **스프라이트 품질**: PIL 생성 플레이스홀더. ComfyUI 재생성 권장
- **SFX 품질**: numpy 수학 합성. ComfyUI 또는 전문 도구로 교체 권장

### 3. 스프린트 종료 프로세스 미수행
- Progress.md 미작성 (본 문서로 해결)
- DevPD 문서 리뷰 미수행
- 팀 플레이테스트/리뷰 회의 미수행
- Notion 동기화 미수행
