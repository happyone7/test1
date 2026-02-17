# Sprint 6 계획서 — "게임플레이 심화 + 플레이어 편의 기능"

**프로젝트**: Soulspire
**작성일**: 2026-02-17
**작성자**: DevPD
**기반**: Sprint 5 완료 (v0.5.0) + Sprint 5 계획서 "다음 스프린트 예고"

---

## 스프린트 목표

Sprint 5에서 Soulspire 아이덴티티가 확립되었고, 기본 게임 루프(Floor 1~3, 3종 타워, 보물상자)가 완성되었다.
Sprint 6는 **"게임플레이 깊이를 더하고, 플레이어 편의 기능을 추가하여 프로토타입 완성도를 높이는 것"**이 목표다.

### 핵심 목표 5가지

| # | 목표 | 근거 |
|---|------|------|
| G1 | **Lightning Tower 구현** | 4번째 타워 추가로 전략 다양성 확보. GDD에 완전히 정의됨 |
| G2 | **Floor 1 밸런스 동기화** | Wave 3~5에 Quick/Heavy Node 미반영, Skill 비용 GDD 불일치 수정 |
| G3 | **Idle Soul 수집기 완성** | HubUI에 패널은 있으나 MetaManager 연동 미구현. 복귀 동기 부여 |
| G4 | **타워 판매 기능** | GDD 정의 완료("길게 클릭 → 70% 환불"). 전략적 재배치 가능 |
| G5 | **온보딩/튜토리얼 개선** | FTUEManager 기본 구조 활용. 신규 플레이어 이탈 방지 |

### 이번 스프린트에서 제외

- Floor 4~5 데이터 (선행 Node 타입 구현 필요 — Sprint 7)
- Laser/Void Tower (Floor 7/9 해금 — Sprint 8+)
- UI 전면 리뉴얼 (컨셉 아트 기반, 규모가 커서 별도 스프린트)
- 사운드 전면 폴리싱 (기본 SFX만 이번에 추가)

---

## Phase 구성 개요

```
Phase 1: Lightning Tower + 밸런스 수정 + 에셋 (병렬)
         ├─ GD: Floor 1 밸런스 동기화 + Lightning SO
         ├─ PG: Lightning Tower 코드 + Idle Soul 수집기
         ├─ TA: Lightning Tower 스프라이트/VFX
         ├─ SD: 기본 SFX 제작 (타워 3종 + Node 파괴)
         └─ QA: Phase 1 검증

Phase 2: 플레이어 편의 기능 (병렬)
         ├─ PG: 타워 판매 로직
         ├─ UI: 타워 판매 UI + 온보딩 팝업
         ├─ GD: 온보딩 시나리오 명세
         └─ QA: Phase 2 검증

Phase 3: 통합 QA + 마무리
         ├─ QA: 전체 통합 QA
         ├─ PM: Sprint6_Progress 갱신
         └─ BLD: 빌드 (총괄PD 승인 필요)
```

---

## Phase 1: Lightning Tower + 밸런스 수정

### 기획팀장 (game-designer)

| ID | 업무 | 상세 | 공수 |
|----|------|------|------|
| GD-1 | Floor 1 Wave SO 수정 | Wave_03: Bit×9→Bit×6+Quick×3, Wave_04: Bit×15→Bit×10+Quick×5, Wave_05: 보스 Incursion 구성 추가 | ~20분 |
| GD-2 | Skill SO 비용 조정 | Skill_AttackSpeed: baseCost 40→80, growthRate 1.25→1.3 / Skill_BaseHp: baseCost 30→40, growthRate 1.2→1.25 | ~10분 |
| GD-3 | Lightning Tower SO 작성 | TowerData_Lightning Lv1~5 (체인 데미지, 체인 수, 공격속도, 비용) GDD 5.2절 기준 | ~20분 |

### 프로그래밍팀장 (unity-gameplay-programmer)

| ID | 업무 | 상세 | 공수 |
|----|------|------|------|
| PG-1 | Lightning Tower 코드 | LightningTower.cs: 체인 공격 로직 (가장 가까운 적 → 주변 체인), TowerData에 chainCount/chainDamageDecay 필드 추가 | ~40분 |
| PG-2 | Idle Soul 수집기 | MetaManager에 오프라인 시간 추적 (lastPlayTime), Soul 계산 로직, HubUI.idleSoulPanel 연동, 세이브 연동 | ~30분 |

### TA팀장 (unity-technical-artist)

| ID | 업무 | 상세 | 공수 |
|----|------|------|------|
| TA-1 | Lightning Tower 스프라이트 | ComfyUI로 Lightning Tower 스프라이트 생성 (64×64, 5레벨) | ~20분 |
| TA-2 | Lightning VFX | 체인 라이트닝 이펙트 파티클/라인렌더러 에셋 | ~15분 |

### 사운드 디렉터 (unity-sound-director)

| ID | 업무 | 상세 | 공수 |
|----|------|------|------|
| SD-1 | 타워 공격 SFX | Arrow(활시위), Cannon(폭발), Ice(결빙), Lightning(전기) 각 1개 | ~25분 |
| SD-2 | Node 파괴 SFX | 기본 파괴음 1개 + 보스 파괴음 1개 | ~10분 |

### QA (Phase 1 검증)

| ID | 업무 | 상세 |
|----|------|------|
| QA-1 | Phase 1 통합 QA | 컴파일 에러 확인, Lightning Tower SO 필드 연결, Wave SO 구성 검증, SFX 할당 확인 |

---

## Phase 2: 플레이어 편의 기능

### 프로그래밍팀장 (unity-gameplay-programmer)

| ID | 업무 | 상세 | 공수 |
|----|------|------|------|
| PG-3 | 타워 판매 로직 | Tower.cs에 Sell() 메서드 (70% Soul 환급), TowerManager에 SellTower() 추가, 배치 그리드 해제 | ~25분 |
| PG-4 | 온보딩 튜토리얼 로직 | FTUEManager 확장: Hub 첫진입/InGame 첫출격/보물상자 첫획득 팝업 트리거, ftueShownKeys 활용 | ~20분 |

### UI팀장 (unity-ui-developer)

| ID | 업무 | 상세 | 공수 |
|----|------|------|------|
| UI-1 | 타워 판매 UI | 길게 클릭 시 판매 확인 팝업 (타워 아이콘 + 환급 Soul 표시 + 확인/취소), TowerInfoTooltip에 판매 버튼 추가 | ~25분 |
| UI-2 | 온보딩 팝업 UI | FTUEPopup 프리팹: 타이틀+본문+닫기 버튼, 3가지 튜토리얼 내용 (스킬트리/타워배치/보물상자) | ~20분 |

### 기획팀장 (game-designer)

| ID | 업무 | 상세 | 공수 |
|----|------|------|------|
| GD-4 | 온보딩 시나리오 명세 | 각 팝업의 텍스트 내용, 표시 조건, 순서 정의. 로컬 md 문서 작성 | ~15분 |

### QA (Phase 2 검증)

| ID | 업무 | 상세 |
|----|------|------|
| QA-2 | Phase 2 통합 QA | 타워 판매 기능 검증 (배치→판매→Soul 환급), 온보딩 팝업 표시 조건 검증, UI 레이아웃 확인 |

---

## Phase 3: 통합 QA + 마무리

| ID | 담당 | 업무 | 상세 |
|----|------|------|------|
| QA-3 | QA팀장 | 전체 통합 QA | 모든 Phase 통합 상태에서 게임 루프 검증, 컴파일 에러 0건 확인 |
| PM-1 | PM | 진행 문서 갱신 | Sprint6_Progress.md 작성, Notion 동기화 |
| BLD-1 | 빌더 | 빌드 | Windows 빌드 + Steam dev_test 업로드 (총괄PD 승인 필요) |

---

## 머지 순서

1. dev/game-designer (SO 데이터 — 다른 팀이 참조)
2. dev/programmer (Lightning Tower, Idle Soul, 판매 로직)
3. dev/ta (아트 에셋)
4. dev/sound (오디오)
5. dev/ui (UI — 위 항목에 의존)
6. dev/build (빌드 설정)

---

## 리스크

| 리스크 | 대응 |
|--------|------|
| Lightning Tower 체인 로직 복잡도 | 기본 1→2 체인부터 구현, 점진적 확장 |
| ComfyUI 에셋 품질 불일치 | 기존 pixelArtSpriteDiffusion 체크포인트 + 시드 고정 |
| 총괄PD 부재 시 빌드 | 빌드까지 준비 완료, 승인 대기 상태로 유지 |
