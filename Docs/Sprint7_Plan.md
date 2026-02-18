# Sprint 7 계획서 — "시각/청각 완성도 + 콘텐츠 기반 확장"

**프로젝트**: Soulspire
**작성일**: 2026-02-18
**작성자**: GameDesigner
**기반**: Sprint 6 완료 (v0.6.0) + Sprint 6 Progress 미완료 항목 + GDD v5.0

---

## 스프린트 목표

Sprint 6에서 핵심 게임플레이 기능(Lightning Tower, 타워 판매, Idle Soul, 온보딩)이 모두 완료되었다.
Sprint 7의 방향은 **"플레이어가 느끼는 퀄리티 레이어를 끌어올리는 것"** 이다.

현재 게임은 기능적으로 동작하지만, 아래 세 가지가 플레이 몰입을 방해한다:

1. Lightning Tower에 시각 피드백(VFX)이 없어 체인 공격의 쾌감이 전달되지 않는다.
2. 스프라이트와 SFX가 PIL/numpy 합성 플레이스홀더 수준이다.
3. Floor 1 보스 이후 콘텐츠(Floor 2~3 보스 Wave SO)가 기획·구현 양쪽에서 미완성이다.

이 세 가지를 Sprint 7에서 집중 해소한다.

### 핵심 목표 5가지

| # | 목표 | 근거 |
|---|------|------|
| G1 | **Lightning VFX 완성** | Sprint 6 TA-2 미구현. 체인 공격 시각화가 없으면 Lightning Tower의 핵심 재미가 전달 안 됨 |
| G2 | **스프라이트 ComfyUI 재생성** | PIL 플레이스홀더는 다크 판타지 아트 방향성과 불일치. 전 타워 + Node 1종(Bit) 재생성 |
| G3 | **SFX ComfyUI/ACE-Step 재생성** | numpy 합성음은 실제 게임 사운드로 부적합. 타워 공격음 4종 + 보스 처치음 재생성 |
| G4 | **Floor 2~3 Wave SO 구성** | 현재 Stage_02, Stage_03 Wave SO 미구성. GDD 4.3절 기준 보스 포함 웨이브 데이터 완성 |
| G5 | **보물상자 오픈 UX 완성** | 보스 드롭 확정이지만 Sanctum 오픈 UI가 미완성. 타워 선택 3종 제시 UI 구현 |

### 이번 스프린트에서 제외

- Floor 4~10 보스 Wave SO (Floor 2~3 완성 후 Sprint 8에서 순차 추가)
- 콤보 시스템 시각 피드백 UI (GDD 7.1절 콤보 보너스 명세 확인 후 Sprint 8)
- 타워 슬롯 수 확장 기능 (GDD 3.2절 '타워 슬롯 확장' 스킬 노드 구현 — 규모 큼)
- Laser/Void Tower (Floor 7/9 해금 — Sprint 8+)
- UI 전면 리뉴얼

---

## Phase 구성 개요

```
Phase 1: 에셋 품질 개선 (병렬)
         ├─ TA: Lightning VFX + 스프라이트 ComfyUI 재생성
         ├─ SD: SFX ComfyUI/ACE-Step 재생성
         └─ QA: Phase 1 검증

Phase 2: 콘텐츠 확장 (병렬)
         ├─ GD: Floor 2~3 Wave SO 구성
         ├─ PG: 보물상자 오픈 로직 (타워 선택 3종)
         ├─ UI: 보물상자 오픈 화면 UI
         └─ QA: Phase 2 검증

Phase 3: 통합 QA + 마무리
         ├─ QA: 전체 통합 QA (BAT 포함)
         ├─ PM: Sprint7_Progress.md 작성 + Notion 동기화
         └─ BLD: 빌드 (총괄PD 승인 필요)
```

---

## Phase 1: 에셋 품질 개선

### TA팀장 (unity-technical-artist)

| ID | 업무 | 상세 | 공수 |
|----|------|------|------|
| TA-1 | Lightning VFX 구현 | ChainLightningVFX.cs 활용. 체인 라인렌더러 이펙트: 타겟 → 체인1 → 체인2 순서로 번개 선이 연결. 색상: #FFFFAA (기본), #00FFFF (체인 끝). 라인 폭 0.08~0.03 감소. 유지시간 0.15초 | ~30분 |
| TA-2 | 타워 스프라이트 재생성 | ComfyUI로 Arrow/Cannon/Ice/Lightning Tower 64×64 픽셀 아트. 다크 판타지 테마. 기존 ArtDirection_v0.1.md 팔레트 준수 | ~40분 |
| TA-3 | Bit Node 스프라이트 재생성 | Bit Node 64×64. 녹색 회로 파편 느낌. 처치 파티클 스프라이트도 포함 | ~15분 |

### 사운드 디렉터 (unity-sound-director)

| ID | 업무 | 상세 | 공수 |
|----|------|------|------|
| SD-1 | 타워 공격 SFX 재생성 | ACE-Step 또는 Stable Audio로 화살 발사(휘익), 포탄 폭발(쾅), 결빙(찰칵), 번개 체인(지직직) 각 1개. numpy 합성 대체 | ~30분 |
| SD-2 | 보스 처치 SFX | 보스 사망 시 슬로우모션 연출에 맞는 중량감 있는 폭발음 1개. Stable Audio: "heavy boss explosion rumble" | ~15분 |
| SD-3 | Unity 적용 확인 | 기존 SFX 할당을 신규 파일로 교체 (AudioClip 교체, Inspector 재확인) | ~15분 |

### QA (Phase 1 검증)

| ID | 업무 | 상세 |
|----|------|------|
| QA-1 | Phase 1 에셋 QA | Lightning VFX 체인 표시 여부, 스프라이트 해상도 및 아트 방향성 확인, SFX 재생 정상 여부, 컴파일 에러 0건 |

---

## Phase 2: 콘텐츠 확장

### 기획팀장 (game-designer)

| ID | 업무 | 상세 | 공수 |
|----|------|------|------|
| GD-1 | Floor 2 Wave SO 구성 | Stage_02 / Wave_01~07 작성. Rush King 보스(W7) 포함. GDD 4.1, 6.5.2 기준. Quick Node 비중 높임(이동속도 보스 테마) | ~25분 |
| GD-2 | Floor 3 Wave SO 구성 | Stage_03 / Wave_01~08 작성. Iron Wall 보스(W8) 포함. Shield Node 등장(W4~). 방어력 돌파 전략 필요 | ~25분 |
| GD-3 | Floor 2~3 해금 조건 SO 확인 | StageData의 unlockCondition 필드 확인 (Soul 500+킬100 / Soul 3,000+킬500). SO에 반영 | ~10분 |

#### Floor 2 Wave 구성 상세

| Wave | Node 구성 | 스폰 간격(초) | 설계 의도 |
|------|----------|-------------|----------|
| W1 | Bit Node x8 | 1.0 | Floor 2 입문. HP 배율 1.8 적용 |
| W2 | Bit Node x8, Quick Node x4 | 0.9 | Quick Node 첫 등장 체감 |
| W3 | Quick Node x8, Bit Node x4 | 0.8 | 속도 압박 가중 |
| W4 | Bit Node x12, Quick Node x6 | 0.8 | 물량 + 속도 복합 |
| W5 | Quick Node x10, Heavy Node x2 | 0.7 | Heavy Node 등장 |
| W6 | Bit Node x15, Quick Node x8, Heavy Node x3 | 0.7 | 종합 압박 |
| W7 (보스) | Bit Node x10, Quick Node x6, **Boss: Rush King x1** | 0.6 | Rush King: HP 540, HP 50% 이하 시 이동속도 2배 |

#### Floor 3 Wave 구성 상세

| Wave | Node 구성 | 스폰 간격(초) | 설계 의도 |
|------|----------|-------------|----------|
| W1 | Bit Node x10, Quick Node x4 | 1.0 | Floor 3 입문. HP 배율 3.0 적용 |
| W2 | Heavy Node x6, Bit Node x8 | 0.9 | 체력 압박 |
| W3 | Quick Node x10, Heavy Node x4 | 0.8 | 속도 + 체력 |
| W4 | Shield Node x4, Bit Node x12 | 0.8 | Shield Node 첫 등장. 방어력 5 |
| W5 | Shield Node x6, Heavy Node x4, Quick Node x6 | 0.7 | 방어 + 체력 + 속도 복합 |
| W6 | Shield Node x8, Quick Node x10, Bit Node x10 | 0.7 | 전면 압박 |
| W7 | Heavy Node x8, Shield Node x6, Quick Node x8 | 0.6 | 보스 전 워밍업 |
| W8 (보스) | Bit Node x10, Shield Node x4, **Boss: Iron Wall x1** | 0.6 | Iron Wall: HP 900, 방어력 10, 10초마다 방어력+5(최대25), Cannon AoE 피격 시 리셋 |

### 프로그래밍팀장 (unity-gameplay-programmer)

| ID | 업무 | 상세 | 공수 |
|----|------|------|------|
| PG-1 | 보물상자 오픈 로직 | TreasureChest 관련 코드: 보유 상자 수 저장/불러오기, 오픈 시 해금된 타워 풀에서 3종 랜덤 추출, 1개 선택 시 인벤토리 추가. ChestManager 또는 MetaManager 확장 | ~35분 |
| PG-2 | 타워 선택 데이터 바인딩 | UI에서 받은 선택 인덱스(0~2)로 인벤토리에 타워 TowerData SO 참조 추가. 저장 시스템 연동 | ~15분 |

### UI팀장 (unity-ui-developer)

| ID | 업무 | 상세 | 공수 |
|----|------|------|------|
| UI-1 | 보물상자 오픈 화면 UI | Sanctum에서 [보물상자 오픈] 버튼 → 오버레이 패널: 타워 카드 3종 나란히 표시 (타워 스프라이트 + 이름 + 주요 스탯 1개) → 카드 클릭 시 선택 확정 → 닫기. TreasureChestUI 프리팹 신규 | ~30분 |
| UI-2 | 콤보 카운터 UI | 인게임 HUD에 콤보 수 표시 (3초 내 5마리 이상 처치 시 활성화). FloatingText 기반 또는 별도 HUD 요소 | ~20분 |

### QA (Phase 2 검증)

| ID | 업무 | 상세 |
|----|------|------|
| QA-2 | Phase 2 콘텐츠 QA | Floor 2~3 SO 로드 정상, 보물상자 오픈 플로우 (상자 보유 → Sanctum 오픈 → 3종 표시 → 선택 → 인벤토리 반영), 콤보 카운터 동작, 컴파일 에러 0건 |

---

## Phase 3: 통합 QA + 마무리

| ID | 담당 | 업무 | 상세 |
|----|------|------|------|
| QA-3 | QA팀장 | 전체 통합 QA + BAT | 전 Phase 통합 상태에서 BAT 전체 실행. 1건이라도 실패 시 빌드 금지 |
| PM-1 | PM | Progress.md 작성 | Sprint7_Progress.md 신규 작성 + Notion 동기화 |
| BLD-1 | 빌더 | Windows 빌드 | 총괄PD 승인 후 Steam dev_test 업로드 |

---

## 머지 순서

1. dev/game-designer (Floor 2~3 Wave SO — 프로그래밍·QA 의존)
2. dev/ta (스프라이트/VFX 에셋)
3. dev/sound (SFX 에셋)
4. dev/programmer (보물상자 로직)
5. dev/ui (보물상자 UI, 콤보 UI)
6. dev/build (빌드 설정)

---

## 예상 마일스톤

| 마일스톤 | 완료 기준 | 담당 |
|---------|---------|------|
| M1: Lightning VFX 동작 | 체인 공격 시 라인렌더러 시각 표시 확인 | TA |
| M2: 에셋 품질 개선 완료 | ComfyUI 스프라이트 4타워 + Bit Node, ACE-Step SFX 전 교체 | TA, SD |
| M3: Floor 2~3 플레이 가능 | Stage_02, Stage_03 Wave SO 로드 + Rush King/Iron Wall 보스 처치 확인 | GD, QA |
| M4: 보물상자 오픈 플로우 완성 | 보스 처치 → Sanctum 오픈 → 타워 선택 → 인벤토리 반영 전 과정 동작 | PG, UI, QA |
| M5: 전체 QA 통과 | BAT 전 항목 통과 | QA |
| M6: 빌드 및 배포 | Steam dev_test 업로드 완료 | BLD |

---

## 리스크

| 리스크 | 대응 |
|--------|------|
| ChainLightningVFX.cs 기반 없으면 TA가 VFX 구현 불가 | 프로그래밍팀장에게 ChainLightningVFX 스텁 코드 확인 요청 (Sprint 6 .meta 파일 존재 확인됨) |
| Floor 2~3 SO 구성 후 Wave 스케일링 불균형 | QA 검증 시 Floor 2~3 각 1회 풀런 + 사망 시점 검증 |
| 보물상자 UI가 기존 HubUI 레이아웃과 충돌 | UI팀장이 오버레이 패널로 구현 (기존 씬 직접 수정 최소화) |
| ComfyUI 에셋 생성 품질 불일치 | 기존 픽셀 아트 체크포인트 + 시드 고정, TA가 레퍼런스 스프라이트 대조 |
| 총괄PD 부재 시 빌드 | 빌드 준비 완료 후 승인 대기 상태 유지 |

---

## Sprint 7 테마 선정 근거

세 후보 중 **"시각/청각 완성도 UP + 콘텐츠 기반 확장"** 복합 방향을 선택한 이유:

- **VFX/스프라이트/SFX 품질 미완성 상태로는 플레이테스트 피드백의 신뢰도가 낮다.** 아트가 플레이스홀더인 상태에서 "재미없다"는 피드백이 나오면 기획/밸런싱 문제인지 아트 문제인지 분리가 어렵다. 에셋 품질 개선이 선행되어야 한다.
- **Floor 2~3 Wave SO가 없으면 30분~1시간 플레이 구간이 막힌다.** GDD 성장 타임라인상 30분이면 첫 스테이지 클리어를 경험해야 하는데, Floor 1 이후 콘텐츠가 없으면 루프가 끊긴다.
- **보물상자 오픈 UX는 타워 획득의 유일한 경로.** 현재 보스 드롭 로직은 있으나 Sanctum에서 실제로 오픈하는 UI가 미완성이라 핵심 게임플레이 루프에 구멍이 있다.
- "메타 깊이" (스킬 트리 확장)는 현재 스킬 트리가 이미 동작하고 있어 Sprint 8로 충분히 미룰 수 있다.
