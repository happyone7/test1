# Soulspire QA Sheet

**프로젝트**: Soulspire (Nodebreaker TD)
**스프린트**: Sprint 3 ~ Sprint 4
**최종 업데이트**: 2026-02-15
**작성자**: QA팀장 (unity-qa-engineer)
**대상 브랜치**: `sprint3`, `sprint4` (Phase 2: dev/programmer `845d2ab`, dev/ui `d04ae3c`, dev/ta `85e89f6`)

---

## Sprint 3 QA 항목

### 타워

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 담당 브랜치 | 메모 |
|----|---------|------|---------|------|------------|------|
| Q01 | 타워 | Arrow Tower 단일 타겟 공격 정상 동작 | P0 | PENDING | sprint3 (기존) | 기존 기능 회귀 테스트 |
| Q02 | 타워 | Arrow Tower 스프라이트 정상 렌더링 | P0 | PENDING | sprint3 (기존) | 2D 전환 후 검증 |
| Q03 | 타워 | Cannon Tower AoE 폭발 공격 동작 | P0 | PENDING | dev/programmer | PG-2 |
| Q04 | 타워 | Cannon Tower AoE 범위 내 복수 적 피격 확인 | P0 | PENDING | dev/programmer | PG-2 |
| Q05 | 타워 | Cannon Tower 스프라이트 렌더링 | P1 | PENDING | sprint3 (기존) | TA-1 스프라이트 |
| Q06 | 타워 | Ice Tower 감속 디버프 적용 | P0 | PENDING | dev/programmer | PG-2 |
| Q07 | 타워 | Ice Tower 감속 효과 지속시간/중첩 확인 | P1 | PENDING | dev/programmer | PG-2 |
| Q08 | 타워 | Ice Tower 스프라이트 렌더링 | P1 | PENDING | sprint3 (기존) | TA-1 스프라이트 |
| Q09 | 타워 | 타워 배치 슬롯 동작 정상 | P0 | PENDING | sprint3 (기존) | 기존 기능 회귀 |
| Q10 | 타워 | 타워 SO 데이터 참조 정상 (TowerData) | P0 | PENDING | sprint3 (기존) | |

### 몬스터

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 담당 브랜치 | 메모 |
|----|---------|------|---------|------|------------|------|
| Q11 | 몬스터 | Bit Node 기본 이동/피격/사망 동작 | P0 | PENDING | sprint3 (기존) | 회귀 테스트 |
| Q12 | 몬스터 | Quick Node 이동속도가 Bit 대비 빠름 확인 | P0 | PENDING | dev/programmer | PG-3 |
| Q13 | 몬스터 | Quick Node 스프라이트 렌더링 | P1 | PENDING | sprint3 (기존) | TA-2 스프라이트 |
| Q14 | 몬스터 | Heavy Node HP가 Bit 대비 높음 확인 | P0 | PENDING | dev/programmer | PG-3 |
| Q15 | 몬스터 | Heavy Node 스프라이트 렌더링 | P1 | PENDING | sprint3 (기존) | TA-2 스프라이트 |
| Q16 | 몬스터 | Shield Node SO 데이터 정상 (Node_Shield.asset) | P1 | PENDING | dev/game-designer | GD-3 신규 |
| Q17 | 몬스터 | 웨이브 스폰 시 몬스터 종류 혼합 정상 | P0 | PENDING | dev/programmer | |
| Q18 | 몬스터 | 몬스터 경로 따라 이동 정상 | P0 | PENDING | sprint3 (기존) | 회귀 테스트 |

### UI - 스킬트리

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 담당 브랜치 | 메모 |
|----|---------|------|---------|------|------------|------|
| Q19 | UI/스킬트리 | SkillTreeUI 18노드 배치 표시 | P0 | PENDING | dev/ui | UI-3 |
| Q20 | UI/스킬트리 | 스킬트리 줌 (마우스 스크롤) 동작 | P0 | PENDING | dev/ui | UI-3 |
| Q21 | UI/스킬트리 | 스킬트리 패닝 (드래그) 동작 | P0 | PENDING | dev/ui | UI-3 |
| Q22 | UI/스킬트리 | 스킬트리 키보드 패닝 (WASD) 동작 | P1 | PENDING | dev/ui | UI-3 |
| Q23 | UI/스킬트리 | 스킬트리 키보드 줌 (+/-) 동작 | P2 | PENDING | dev/ui | UI-3 |
| Q24 | UI/스킬트리 | 노드 연결선 표시 정상 | P0 | PENDING | dev/ui | UI-3 |
| Q25 | UI/스킬트리 | 노드 상태별 시각 표현 (Hidden/Locked/Available/Purchased/Maxed) | P0 | PENDING | dev/ui | UI-3 |
| Q26 | UI/스킬트리 | 구매 팝업 표시 (노드 클릭 시) | P0 | PENDING | dev/ui | UI-3 |
| Q27 | UI/스킬트리 | Bit 노드 구매 (반복 구매형) Before/After 표시 | P0 | PENDING | dev/ui | UI-3 |
| Q28 | UI/스킬트리 | Core 노드 구매 (1회 구매형) 효과 설명 표시 | P0 | PENDING | dev/ui | UI-3 |
| Q29 | UI/스킬트리 | 잠긴 노드 클릭 시 잠금 안내 + 선행 노드 목록 표시 | P1 | PENDING | dev/ui | UI-3 |
| Q30 | UI/스킬트리 | 구매 후 노드 상태 갱신 + 연결선 갱신 | P0 | PENDING | dev/ui | UI-3 |
| Q31 | UI/스킬트리 | 자원 부족 시 비용 텍스트 빨간색 표시 | P1 | PENDING | dev/ui | UI-3 |
| Q32 | UI/스킬트리 | 최대 레벨 도달 시 MAX 표시 + 버튼 비활성화 | P1 | PENDING | dev/ui | UI-3 |

### UI - 기본 화면

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 담당 브랜치 | 메모 |
|----|---------|------|---------|------|------------|------|
| Q33 | UI | 타이틀 화면 정상 표시 | P0 | PENDING | sprint3 (기존) | 회귀 |
| Q34 | UI | 허브 화면 정상 표시 + 버튼 동작 | P0 | PENDING | sprint3 (기존) | 회귀 |
| Q35 | UI | 인게임 HUD 표시 (웨이브 카운터, HP바 등) | P0 | PENDING | sprint3 (기존) | 회귀 |
| Q36 | UI | UI 에셋 16개 PNG 적용 확인 (버튼/프레임/아이콘) | P0 | PENDING | sprint3 (기존) | UI-1 |
| Q37 | UI | Hub에서 스킬트리 화면 전환 정상 | P0 | PENDING | dev/ui | |

### 사운드

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 담당 브랜치 | 메모 |
|----|---------|------|---------|------|------------|------|
| Q38 | 사운드 | BGM Hub 재생 정상 | P0 | PENDING | sprint3 (기존) | SD-2 교체 후 |
| Q39 | 사운드 | BGM Combat 재생 정상 | P0 | PENDING | sprint3 (기존) | SD-2 교체 후 |
| Q40 | 사운드 | SFX Tower Attack 트리거 정상 | P0 | PENDING | sprint3 (기존) | SD-1 |
| Q41 | 사운드 | SFX Node Die 트리거 정상 | P0 | PENDING | sprint3 (기존) | SD-1 |
| Q42 | 사운드 | SFX Stage Clear 트리거 정상 | P0 | PENDING | sprint3 (기존) | SD-1 |
| Q43 | 사운드 | 볼륨 밸런스 (BGM vs SFX 비율) 적절 | P1 | PENDING | sprint3 (기존) | SD-1 |
| Q44 | 사운드 | BGM 화면 전환 시 크로스페이드/전환 정상 | P1 | PENDING | sprint3 (기존) | |

### 스킬트리 로직

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 담당 브랜치 | 메모 |
|----|---------|------|---------|------|------------|------|
| Q45 | 스킬트리 | MetaManager 스킬 레벨 저장/불러오기 | P0 | PENDING | dev/ui | |
| Q46 | 스킬트리 | 전제 조건 미충족 시 구매 불가 | P0 | PENDING | dev/ui | |
| Q47 | 스킬트리 | Bit 자원 차감 정상 (구매 시) | P0 | PENDING | dev/ui | |
| Q48 | 스킬트리 | Core 자원 차감 정상 (구매 시) | P0 | PENDING | dev/ui | |
| Q49 | 스킬트리 | 스킬 효과가 RunModifiers에 반영 | P0 | PENDING | dev/ui | |
| Q50 | 스킬트리 | 초기 Bit 500 지급 (신규 세이브) | P1 | PENDING | dev/ui | |
| Q51 | 스킬트리 | 노드 가시성 (선행 노드 1개 구매 시 표시) | P0 | PENDING | dev/ui | |

### 밸런스/웨이브

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 담당 브랜치 | 메모 |
|----|---------|------|---------|------|------------|------|
| Q52 | 밸런스 | 스테이지 1 웨이브 1~5 정상 스폰 | P0 | PENDING | dev/game-designer | Wave_01~05 |
| Q53 | 밸런스 | 스테이지 1 후반 웨이브에 Quick/Heavy 혼합 등장 | P0 | PENDING | dev/game-designer | Wave_03~05 수정됨 |
| Q54 | 밸런스 | 스테이지 2 웨이브 1~7 정상 스폰 | P1 | PENDING | dev/game-designer | Wave_S2_01~07 신규 |
| Q55 | 밸런스 | 스테이지 3 웨이브 1~8 정상 스폰 | P1 | PENDING | dev/game-designer | Wave_S3_01~08 신규 |
| Q56 | 밸런스 | Stage_02 SO 데이터 정상 (hpMultiplier=1.8, core=3) | P1 | PENDING | dev/game-designer | |
| Q57 | 밸런스 | Stage_03 SO 데이터 정상 (hpMultiplier=3.0, core=4) | P1 | PENDING | dev/game-designer | |
| Q58 | 밸런스 | 난이도 곡선 체감 확인 (스테이지별 점진적 난이도 증가) | P2 | PENDING | dev/game-designer | 플레이테스트 필요 |

### 아트 에셋

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 담당 브랜치 | 메모 |
|----|---------|------|---------|------|------------|------|
| Q59 | 아트 | 스킬 아이콘 8종 임포트 정상 (skill_*.png) | P0 | PENDING | dev/ta | TA-3 |
| Q60 | 아트 | 스킬 아이콘 .meta 파일 존재 및 Sprite 설정 | P0 | PENDING | dev/ta | TA-3 |
| Q61 | 아트 | 스킬 아이콘 PPU=128, filterMode=Point 설정 | P1 | PENDING | dev/ta | 픽셀아트 선명도 |
| Q62 | 아트 | Arrow 투사체 PPU 1024->64 수정 반영 | P0 | PENDING | dev/ta | 인게임 가시성 |
| Q63 | 아트 | 타이틀 로고 재제작 반영 (SoulspireLogo_02.png) | P1 | PENDING | dev/game-designer | 투명 배경 |

### 통합/컴파일

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 담당 브랜치 | 메모 |
|----|---------|------|---------|------|------------|------|
| Q64 | 통합 | 모든 .cs 파일 컴파일 에러 없음 | P0 | PASS (정적) | sprint3 | 37개 .cs + 19개 Editor .cs 전수 검증 |
| Q65 | 통합 | Console에 에러 로그 없음 (경고 허용) | P0 | BLOCKED | sprint3 | MCP 세션 미응답 |
| Q66 | 통합 | 씬 로드 시 Missing Reference 없음 | P0 | BLOCKED | sprint3 | MCP 세션 미응답 |
| Q67 | 통합 | 에디터 Play 모드 정상 진입 | P0 | BLOCKED | sprint3 | MCP 세션 미응답 |
| Q68 | 통합 | 타이틀 -> 허브 -> 인게임 -> 런 종료 플로우 정상 | P0 | BLOCKED | sprint3 | MCP 세션 미응답 |

---

## Sprint 4 Phase 2 QA 항목 (단위 QA - 코드 리뷰)

**대상 커밋**: dev/programmer `845d2ab`, dev/ui `d04ae3c`, dev/ta `85e89f6`+`476026f`
**검증 방법**: `git show <branch>:<path>` 정적 코드 리뷰 (브랜치 체크아웃 없이)

### PG-3: 타워 합성 최대레벨 Lv5

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 검증 방법 | 메모 |
|----|---------|------|---------|------|----------|------|
| Q101 | PG-3 | Tower.TryMerge() Lv5 상한 검사 | P0 | PASS | 코드 리뷰 (Tower.cs) | `if (Level >= 5) return false;` -- 정상. 같은 타입+같은 레벨 조건도 유지 |
| Q102 | PG-3 | Tower.LevelUp() Lv5 상한 검사 | P0 | PASS | 코드 리뷰 (Tower.cs) | `if (Level < 5) Level++;` -- 드래그 합성용 메서드. 정상 |
| Q103 | PG-3 | TowerData.damage 배열 5개 요소 | P0 | PASS | 코드 리뷰 (TowerData.cs) | `float[] damage = { 10f, 15f, 22f, 33f, 50f }` -- Lv1~5 5개. 정상 |
| Q104 | PG-3 | TowerData.attackSpeed/range 배열 5개 요소 | P0 | PASS | 코드 리뷰 (TowerData.cs) | 각각 5개 기본값 보유. 정상 |
| Q105 | PG-3 | Phase 1 호환: Tower.cs 기존 로직 유지 | P0 | PASS | 코드 리뷰 | Update/FindTarget/Attack 메서드 변경 없음. TryMerge 상한값만 4->5로 변경 |

### PG-4: BossNode 구현

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 검증 방법 | 메모 |
|----|---------|------|---------|------|----------|------|
| Q106 | PG-4 | BossNode : Node 상속 관계 | P0 | PASS | 코드 리뷰 (BossNode.cs) | `public class BossNode : Node` -- 정상 |
| Q107 | PG-4 | BossNode.HpRatio 프로퍼티 (UI 연동용) | P0 | PASS | 코드 리뷰 | `MaxHp > 0f ? CurrentHp / MaxHp : 0f` -- 0 방어. 정상 |
| Q108 | PG-4 | BossNode.OnPop() 스케일 적용 | P1 | PASS | 코드 리뷰 | `base.OnPop()` 호출 후 `bossScale=1.5f` 적용. 정상 |
| Q109 | PG-4 | Node.Die() virtual 선언 | P0 | PASS | 코드 리뷰 (Node.cs) | `protected virtual void Die()` -- BossNode 오버라이드 가능. 정상 |
| Q110 | PG-4 | Node.Die()에서 보스 처치 이벤트 호출 | P0 | PASS | 코드 리뷰 (Node.cs) | `if (IsBossNode) RunManager.Instance.OnBossKilled()` -- 정상 |
| Q111 | PG-4 | Node.IsBossNode 프로퍼티 | P0 | PASS | 코드 리뷰 (Node.cs) | `Data != null && Data.type == NodeType.Boss` -- 정상 |
| Q112 | PG-4 | RunManager.OnBossKilled() 이벤트 발행 | P0 | PASS | 코드 리뷰 (RunManager.cs) | `BossDefeated = true; OnBossDefeated?.Invoke()` + 보물상자 100% 드랍 호출. 정상 |

### PG-5: 스테이지 클리어 판정

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 검증 방법 | 메모 |
|----|---------|------|---------|------|----------|------|
| Q113 | PG-5 | RunManager.EndRun() core 보상 계산 | P0 | PASS | 코드 리뷰 (RunManager.cs) | `coreEarned = cleared ? CurrentStage.coreReward : 0` -- 클리어 시만 지급. 정상 |
| Q114 | PG-5 | GameManager.StartNextStage() | P0 | PASS | 코드 리뷰 (GameManager.cs) | `nextIdx = Min(PlayingStageIndex + 1, stages.Length - 1)` -- 범위 초과 방어. 정상 |
| Q115 | PG-5 | GameManager.RetryStage() | P0 | PASS | 코드 리뷰 (GameManager.cs) | `StartRun(PlayingStageIndex)` -- 같은 인덱스로 재시도. 정상 |
| Q116 | PG-5 | GameManager.LastRunCleared 분기 | P0 | PASS | 코드 리뷰 (GameManager.cs) | OnRunEnd에서 설정, InGameUI.OnRetry()에서 분기 사용. 정상 |
| Q117 | PG-5 | InGameUI.OnRetry() 클리어/패배 분기 | P0 | PASS | 코드 리뷰 (InGameUI.cs) | `if (LastRunCleared) StartNextStage() else RetryStage()` -- 정상 |
| Q118 | PG-5 | StageData.coreReward 필드 | P0 | PASS | 코드 리뷰 (StageData.cs) | `int coreReward = 2` -- 기본값 2. SO에서 스테이지별 설정 가능. 정상 |

### PG-6: 스테이지 해금 시스템

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 검증 방법 | 메모 |
|----|---------|------|---------|------|----------|------|
| Q119 | PG-6 | UnlockConditionType enum (BossKill/TotalBit/TotalKills/None) | P0 | PASS | 코드 리뷰 (StageData.cs) | 4가지 해금 조건 타입 정의. 정상 |
| Q120 | PG-6 | StageData.unlockConditions 배열 | P0 | PASS | 코드 리뷰 (StageData.cs) | `StageUnlockCondition[]` -- 여러 조건 조합 가능. 정상 |
| Q121 | PG-6 | MetaManager.IsStageUnlocked() | P0 | PASS | 코드 리뷰 (MetaManager.cs) | 모든 조건 AND 검사. BossKill/TotalBit/TotalKills. null/빈 배열 시 항상 해금. 정상 |
| Q122 | PG-6 | MetaManager.IsStageCleared() | P0 | PASS | 코드 리뷰 (MetaManager.cs) | `_data.clearedStages.Contains(stageIndex)` -- 정상 |
| Q123 | PG-6 | PlayerSaveData.clearedStages 필드 | P0 | PASS | 코드 리뷰 (PlayerSaveData.cs) | `List<int>` + totalKills, totalBitEarned 추가. 정상 |
| Q124 | PG-6 | AddRunRewards()에서 클리어 기록 저장 | P0 | PASS | 코드 리뷰 (MetaManager.cs) | `if (cleared) clearedStages.Add(stageIdx)` -- 중복 방지 포함. 정상 |
| Q125 | PG-6 | HubUI.RefreshStageDropdown() 해금 연동 | P0 | PASS | 코드 리뷰 (HubUI.cs) | `meta.IsStageUnlocked(stages[i])` 으로 순차 해금. 정상 |

### PG-7: 보물상자 3택 시스템

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 검증 방법 | 메모 |
|----|---------|------|---------|------|----------|------|
| Q126 | PG-7 | TreasureChestManager Singleton 패턴 | P0 | PASS | 코드 리뷰 | `Singleton<TreasureChestManager>` 상속. 정상 |
| Q127 | PG-7 | 드랍 확률: 일반 3%, 웨이브 20%, 보스 100% | P0 | PASS | 코드 리뷰 | `normalKillDropRate=0.03f`, `waveClearDropRate=0.20f`, `bossKillDropRate=1.0f`. 정상 |
| Q128 | PG-7 | DropChest() 3택 랜덤 선택 + 이벤트 발행 | P0 | PASS | 코드 리뷰 | `PickRandomRewards(3)` -> `OnChestDropped?.Invoke(choices)`. 가중치 기반. 정상 |
| Q129 | PG-7 | SelectReward() 보상 적용 + 게임 재개 | P0 | PASS | 코드 리뷰 | `ApplyReward(reward)`, `Time.timeScale=1f`. _waitingForSelection 가드. 정상 |
| Q130 | PG-7 | ApplyReward() 8가지 보상 타입 처리 | P0 | PASS | 코드 리뷰 | 8종 모두 switch 처리. 정상 |
| Q131 | PG-7 | RunManager.HealBase/IncreaseMaxHp 추가 | P0 | PASS | 코드 리뷰 (RunManager.cs) | HealBase: Min 클램프, IncreaseMaxHp: 동시 증가. 정상 |
| Q132 | PG-7 | RunManager.SetModifiers() 추가 | P0 | PASS | 코드 리뷰 (RunManager.cs) | 보물상자 보상으로 모디파이어 갱신. 정상 |
| Q133 | PG-7 | TreasureRewardData SO 정의 | P0 | PASS | 코드 리뷰 (TreasureRewardData.cs) | CreateAssetMenu, rewardType enum 8종, weight/rarity 필드. 정상 |
| Q134 | PG-7 | Node.Die() 일반 처치 시 보물상자 확률 드랍 | P0 | PASS | 코드 리뷰 (Node.cs) | 보스 아닌 경우 TryDropOnNormalKill(). 정상 |
| Q135 | PG-7 | WaveSpawner 웨이브 클리어 시 보물상자 드랍 | P0 | PASS | 코드 리뷰 (WaveSpawner.cs) | TryDropOnWaveClear() 모든 웨이브 클리어 시. 정상 |
| Q136 | PG-7 | SoundKeys 보물상자 사운드 키 추가 | P1 | PASS | 코드 리뷰 (SoundKeys.cs) | TreasureChestDrop, TreasureRewardSelect 2개 추가. 정상 |

### UI-3: 보물상자 3택 UI

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 검증 방법 | 메모 |
|----|---------|------|---------|------|----------|------|
| Q137 | UI-3 | TreasureChoiceUI.Show() 시그니처 | P0 | WARN | 코드 리뷰 | **데이터 타입 불일치**: UI는 `TreasureChoiceData[]` 사용, PG는 `TreasureRewardData[]` 이벤트 발행. 머지 시 통합 필요 |
| Q138 | UI-3 | TreasureChoiceUI 카드 3장 동적 생성 | P0 | PASS | 코드 리뷰 | BuildUI에서 HorizontalLayoutGroup + 3개 카드 생성. 정상 |
| Q139 | UI-3 | 카드 선택 시 확대+페이드 애니메이션 | P0 | PASS | 코드 리뷰 | SelectAnimation: 선택 1.15x 확대, 비선택 페이드아웃, Time.unscaledDeltaTime. 정상 |
| Q140 | UI-3 | 등장 애니메이션 | P1 | PASS | 코드 리뷰 | ShowAnimation: CanvasGroup alpha, ease-out cubic, 카드별 시차. 정상 |
| Q141 | UI-3 | Time.timeScale=0 일시정지 | P0 | PASS | 코드 리뷰 | Show()에서 0, 선택 후 1f. 정상 |
| Q142 | UI-3 | 다크 판타지 색상 팔레트 적용 | P1 | PASS | 코드 리뷰 | 일관된 팔레트. 정상 |
| Q143 | UI-3 | TreasureChoiceData SO 정의 | P0 | PASS | 코드 리뷰 | TreasureEffectType enum 14종, TreasureRarity 3단계. 정상 |

### UI-4: 보스 HP바

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 검증 방법 | 메모 |
|----|---------|------|---------|------|----------|------|
| Q144 | UI-4 | BossHpBarUI.ShowBoss() 인터페이스 | P0 | WARN | 코드 리뷰 | ShowBoss(string, int) 존재하나, **호출자 없음** |
| Q145 | UI-4 | BossHpBarUI.UpdateHp() 실시간 갱신 | P0 | WARN | 코드 리뷰 | UpdateHp(int) 존재하나, **Node/BossNode에서 호출 코드 없음** |
| Q146 | UI-4 | 지연 감소 바 (골드색) | P1 | PASS | 코드 리뷰 | MoveTowards + Time.unscaledDeltaTime. 정상 |
| Q147 | UI-4 | 슬라이드인/페이드아웃 애니메이션 | P1 | PASS | 코드 리뷰 | ease-out cubic 슬라이드, alpha 페이드. 정상 |
| Q148 | UI-4 | HP=0 시 자동 HideBoss() | P1 | PASS | 코드 리뷰 | UpdateHp에서 0 체크. 정상 |

### UI-5: 스테이지 클리어 화면

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 검증 방법 | 메모 |
|----|---------|------|---------|------|----------|------|
| Q149 | UI-5 | StageClearUI.Show() 인터페이스 | P0 | WARN | 코드 리뷰 | Show() 존재하나, **호출자 없음**. 기존 InGameUI.ShowRunEnd()와 역할 중복 |
| Q150 | UI-5 | Core 카운트업 연출 | P1 | PASS | 코드 리뷰 | 0.8초 Lerp + 펄스 스케일 효과. 정상 |
| Q151 | UI-5 | [Hub으로]/[다음 스테이지] 버튼 | P0 | PASS | 코드 리뷰 | hasNextStage=false 시 두 버튼 모두 Hub. 정상 |
| Q152 | UI-5 | 슬라이드업 애니메이션 | P1 | PASS | 코드 리뷰 | ease-out cubic, 0.4초. 정상 |
| Q153 | UI-5 | 다크 판타지 색상 팔레트 | P1 | PASS | 코드 리뷰 | 일관성 유지. 정상 |

### UI-6: 드래그 고스트

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 검증 방법 | 메모 |
|----|---------|------|---------|------|----------|------|
| Q154 | UI-6 | TowerDragGhostUI.Update() 드래그 상태 감지 | P0 | PASS | 코드 리뷰 | DragState.Dragging 체크. Start/End 전환 감지. 정상 |
| Q155 | UI-6 | 마우스 위치에서 Canvas 좌표 변환 | P0 | PASS | 코드 리뷰 | ScreenPointToLocalPointInRectangle. 정상 |
| Q156 | UI-6 | 배치/합성/불가 3색 하이라이트 | P0 | PASS | 코드 리뷰 | Physics2D.OverlapPoint + PlacementGrid.CanPlace. 정상 |
| Q157 | UI-6 | SetGhostSprite() 외부 호출 | P0 | PASS | 코드 리뷰 | InventoryBarUI.OnSlotPointerDown에서 호출. 정상 |
| Q158 | UI-6 | CanvasGroup blocksRaycasts=false | P1 | PASS | 코드 리뷰 | 이벤트 가로채기 방지. 정상 |
| Q159 | UI-6 | 펄스 효과 (크기 변동) | P2 | PASS | 코드 리뷰 | Sin * 0.08f. 정상 |

### TA: 아트 에셋 (TA-1+2+4)

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 검증 방법 | 메모 |
|----|---------|------|---------|------|----------|------|
| Q160 | TA | 보스 스프라이트 (Boss_BitLord, Boss_RushKing) 임포트 | P0 | PASS | git show 85e89f6 --stat | Final/ 폴더 확정본, .meta 포함. 정상 |
| Q161 | TA | 합성 이펙트 (VFX_MergeLight) 임포트 | P1 | PASS | git show 85e89f6 --stat | Final/ + Candidates/ 구조. .meta 포함. 정상 |
| Q162 | TA | Tilemap 타일 스프라이트 (Ground/Wall) 임포트 | P0 | PASS | git show 476026f --stat | Tile_Ground.png, Tile_Wall.png Final/. .meta 포함. 정상 |
| Q163 | TA | 보물상자 스프라이트 (Chest_*) 임포트 | P1 | PASS | git show 85e89f6 --stat | TreasureChest/ 폴더. .meta 포함. 정상 |
| Q164 | TA | 모든 Final 스프라이트 .meta 존재 확인 | P0 | PASS | git show --stat | 70개 파일, 모든 .png 대응 .meta 존재. 정상 |

### 시스템 간 연동 (PG-UI 인터페이스)

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 검증 방법 | 메모 |
|----|---------|------|---------|------|----------|------|
| Q165 | 연동 | **보물상자: PG TreasureRewardData vs UI TreasureChoiceData 타입 불일치** | P0 | FAIL | 코드 리뷰 | PG: `TreasureRewardData`+`TreasureRewardType`(8종), UI: `TreasureChoiceData`+`TreasureEffectType`(14종) 독립 정의. 필드명/타입 다름. 머지 시 컴파일 에러 또는 연결 불가 |
| Q166 | 연동 | **보물상자: OnChestDropped 이벤트에서 TreasureChoiceUI.Show() 미연결** | P0 | FAIL | 코드 리뷰 | PG 이벤트: `Action<TreasureRewardData[]>`, UI Show: `(TreasureChoiceData[], Action<int>)`. 구독/콜백 코드 없음 |
| Q167 | 연동 | **보스 HP바: BossHpBarUI 호출자 없음** | P0 | FAIL | 코드 리뷰 | 보스 등장/HP 갱신 연동 코드 미구현. WaveSpawner/Node에서 UI 호출 없음 |
| Q168 | 연동 | **StageClearUI vs InGameUI.ShowRunEnd() 역할 중복** | P1 | WARN | 코드 리뷰 | PG는 기존 InGameUI.ShowRunEnd() 호출. UI는 별도 StageClearUI 신규 생성. 통합 시 역할 결정 필요 |
| Q169 | 연동 | TowerDragGhostUI - TowerDragController 연동 | P0 | PASS | 코드 리뷰 | DragState enum 참조, FindAnyObjectByType 폴백. 정상 |
| Q170 | 연동 | InventoryBarUI - TowerInventory 이벤트 연동 | P0 | PASS | 코드 리뷰 | OnInventoryChanged 구독/해제 정상. 정상 |
| Q171 | 연동 | Phase 1 호환: 기존 코드 수정이 Phase 1을 깨뜨리지 않음 | P0 | PASS | 코드 리뷰 | ShowRunEnd 시그니처 동일, 신규 메서드만 추가. 정상 |

### Phase 1 회귀 검증

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 검증 방법 | 메모 |
|----|---------|------|---------|------|----------|------|
| Q172 | 회귀 | Tower.cs 기존 Attack/FindTarget/Update 변경 없음 | P0 | PASS | git diff | TryMerge 상한값과 LevelUp만 변경/추가 |
| Q173 | 회귀 | Node.cs Die() virtual화가 기존 동작 유지 | P0 | PASS | 코드 리뷰 | protected virtual 변경만. 기존 로직 유지 |
| Q174 | 회귀 | RunManager 기존 메서드 시그니처 유지 | P0 | PASS | 코드 리뷰 | 신규 메서드만 추가 |
| Q175 | 회귀 | GameManager 기존 StartRun/GoToHub 시그니처 유지 | P0 | PASS | 코드 리뷰 | 신규 메서드만 추가 |
| Q176 | 회귀 | MetaManager 기존 API 유지 | P0 | PASS | 코드 리뷰 | AddRunRewards 오버로드 추가. 기존 4인자 버전 유지. 정상 |

---

### Sprint 4 Phase 2 발견 이슈

| 심각도 | 내용 | 관련 ID | 권장 조치 |
|--------|------|---------|----------|
| **P0 (FAIL)** | PG-UI 보물상자 데이터 타입 불일치: `TreasureRewardData` vs `TreasureChoiceData` | Q165, Q166 | UI의 TreasureChoiceData를 제거하고 PG의 TreasureRewardData로 통일 권장. PG 쪽이 weight/rarity 등 필수 필드를 이미 포함 |
| **P0 (FAIL)** | 보물상자 이벤트-UI 미연결: OnChestDropped -> TreasureChoiceUI.Show() 구독 코드 없음 | Q166 | 통합 시 InGameUI 또는 별도 브릿지에서 이벤트 구독 + UI Show 호출 + SelectReward 콜백 연결 필요 |
| **P0 (FAIL)** | BossHpBarUI 호출자 없음: 보스 등장/HP 갱신 연동 코드 미구현 | Q167 | WaveSpawner에서 보스 웨이브 시작 시 ShowBoss() 호출, BossNode TakeDamage 후 UpdateHp() 호출 연동 추가 필요 |
| **P1 (WARN)** | StageClearUI vs InGameUI.ShowRunEnd() 역할 중복 | Q168 | 클리어 시 StageClearUI / 패배 시 기존 RunEnd 패널 분리 사용 추천 |

### Sprint 4 Phase 2 QA 결과 요약

| 구분 | PASS | WARN | FAIL | 합계 |
|------|------|------|------|------|
| PG-3 합성 확장 (Q101~Q105) | 5 | 0 | 0 | 5 |
| PG-4 BossNode (Q106~Q112) | 7 | 0 | 0 | 7 |
| PG-5 클리어 판정 (Q113~Q118) | 6 | 0 | 0 | 6 |
| PG-6 해금 시스템 (Q119~Q125) | 7 | 0 | 0 | 7 |
| PG-7 보물상자 (Q126~Q136) | 11 | 0 | 0 | 11 |
| UI-3 보물상자 3택 (Q137~Q143) | 6 | 1 | 0 | 7 |
| UI-4 보스 HP바 (Q144~Q148) | 3 | 2 | 0 | 5 |
| UI-5 클리어 화면 (Q149~Q153) | 4 | 1 | 0 | 5 |
| UI-6 드래그 고스트 (Q154~Q159) | 6 | 0 | 0 | 6 |
| TA 아트 에셋 (Q160~Q164) | 5 | 0 | 0 | 5 |
| 시스템 연동 (Q165~Q171) | 3 | 1 | 3 | 7 |
| Phase 1 회귀 (Q172~Q176) | 5 | 0 | 0 | 5 |
| **합계** | **68** | **5** | **3** | **76** |

**정적 코드 검증 68/76 PASS (89.5%)**. FAIL 3건은 모두 PG-UI 간 인터페이스 불일치로 머지 시 통합 작업 필요.

---

## 검증 결과 요약

### 브랜치별 코드 검증 (Sprint 3 Phase 3 - QA팀장)

| 브랜치 | 커밋 | 변경 요약 | .meta 검증 | 코드 검증 | 머지 상태 |
|--------|------|-----------|-----------|-----------|----------|
| dev/ui | d810a9d, e8b7b88 | 스킬트리 UI 시스템 (6개 .cs + 씬 + 에디터) | PASS | PASS | MERGED (45613ec) |
| dev/game-designer | 014694a, 0e32a8f, 383865b | 난이도 곡선 SO (Stage 2~3, Wave 19개, Node_Shield) + 로고 + 맵 레이아웃 | PASS | N/A (코드 없음) | MERGED (0aea38d) |
| dev/ta | daff3af, 67150de | 스킬 아이콘 8종 + 투사체 PPU 수정 | PASS | N/A (코드 없음) | MERGED (11282c8) |
| dev/programmer | 08c4487 | Cannon/Ice Tower, Quick/Heavy Monster, Tilemap 4-layer, defense, MetaManager SaveManager 전환 | PASS | PASS | MERGED (08c4487) |

### Sprint 3 최종 통합 QA (Q64~Q68)

| 항목 | 검증 방법 | 결과 | 비고 |
|------|----------|------|------|
| Q64 - 컴파일 에러 | 정적 코드 분석 (37+19개 .cs 전수 검토) | PASS (정적) | .meta 56/56 정합, namespace/타입 참조 정상, Tesseract 패키지 의존성 확인 |
| Q65 - 에러 로그 | Unity MCP read_console | BLOCKED | MCP 세션 미응답 (ping timeout 반복) |
| Q66 - Missing Ref | Unity MCP manage_scene | BLOCKED | MCP 세션 미응답 |
| Q67 - Play 모드 | Unity MCP manage_editor | BLOCKED | MCP 세션 미응답 |
| Q68 - 게임 플로우 | Unity MCP Play + 수동 확인 | BLOCKED | MCP 세션 미응답 |

---

## 변경 이력

| 날짜 | 내용 |
|------|------|
| 2026-02-15 | Sprint 3 QA 시트 초기 생성 (QA팀장) |
| 2026-02-15 | dev/ui, dev/game-designer, dev/ta 브랜치 검증 통과 및 sprint3 머지 완료 |
| 2026-02-15 | Sprint 3 최종 통합 QA (Q64~Q68) 수행. Q64 정적 PASS, Q65~Q68 MCP BLOCKED |
| 2026-02-15 | Sprint 4 Phase 2 단위 QA (Q101~Q176) 수행. 68/76 PASS, 3 FAIL (PG-UI 인터페이스 불일치), 5 WARN |
