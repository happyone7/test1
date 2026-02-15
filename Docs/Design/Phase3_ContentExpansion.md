# Phase 3: 콘텐츠 확장 기획서

| 항목 | 내용 |
|------|------|
| 문서 버전 | 1.0 |
| 최종 수정 | 2026-02-15 |
| 작성자 | 기획팀장 (Game Designer) |
| 기준 문서 | GDD v3.0, BalanceSheet v0.1 |
| 대상 Phase | Phase 3 (Phase 1: 코어 전투, Phase 2: 영구 성장 완료 후) |

---

## 0. 현재 시스템 분석 요약

### 0.1 기존 SO/코드 구조

| 항목 | 현재 상태 | 비고 |
|------|----------|------|
| **StageData SO** | Stage_01 1개만 존재 | waves[], hpMultiplier, speedMultiplier, bitDropMultiplier, coreReward, baseHp 필드 보유 |
| **WaveData SO** | Wave_01~05 (5개) | SpawnGroup[](nodeData, count, spawnInterval) + delayBeforeWave |
| **TowerData SO** | Arrow, Cannon, Ice 3종 | 레벨별 배열(damage[], attackSpeed[], range[]) 5레벨분 보유. placeCost 필드 있음 |
| **NodeData SO** | Bit, Quick, Heavy 3종 | hp, speed, bitDrop, damage, defense. NodeType enum에 Boss 포함되어 있음 |
| **GameManager** | stages[] 배열로 멀티 스테이지 지원 가능 | StartRun(stageIndex), OnRunEnd(cleared, bitEarned) |
| **RunManager** | CurrentStage, CurrentWaveIndex 추적 | RunModifiers 적용 (attackDamage/Speed multiplier, bonusBaseHp) |
| **MetaManager** | totalBit, totalCore, currentStageIndex 저장 | SkillNodeData 기반 영구 업그레이드, AddRunRewards() |
| **WaveSpawner** | 보스 웨이브 감지(IsBossWave), 웨이브 클리어 시 타워 드롭 | StageData의 스케일링 배율을 Node.Initialize에 전달 |
| **Tower** | Level 1~4 지원, TryMerge/LevelUp 메서드 보유 | RunModifiers 적용 |
| **TowerInventory** | 최대 8슬롯, InventorySlot(data, level) | 웨이브 클리어 시 랜덤 타워 1기 드롭 |
| **HubUI** | 스테이지 드롭다운(Dropdown), 스킬 트리, 방치 Bit 알림 | RefreshStageDropdown()으로 해금된 스테이지 표시 |

### 0.2 핵심 발견: 코드 변경 최소화 가능 포인트

1. **멀티 스테이지**: `GameManager.stages[]`에 StageData SO를 추가하면 즉시 동작. HubUI 드롭다운도 자동 반영.
2. **타워 레벨업**: `Tower.LevelUp()`, `Tower.TryMerge()` 이미 구현. 인게임 Bit 소모 업그레이드는 UI+호출 로직만 추가.
3. **보스 Node**: `NodeType.Boss` enum 존재, `WaveSpawner.IsBossWave()` 구현 완료. NodeData SO에 Boss 타입 에셋 추가만 필요.
4. **스케일링**: StageData의 hpMultiplier/speedMultiplier/bitDropMultiplier가 WaveSpawner에서 Node.Initialize에 전달됨.

---

## 1. 스테이지 시스템

### 1.1 설계 방향

GDD v3.0에서 10개 스테이지를 설계했으나, Phase 3에서는 **Stage 1~3을 완성**하고 이후 스테이지는 Phase 4에서 확장한다. "초반 30분 재미 극대화"에 집중.

### 1.2 스테이지 구성

| 스테이지 | ID | 이름 | 웨이브 수 | HP 배율 | 속도 배율 | Bit 드롭 배율 | 기지 HP | 클리어 보상 (Core) | 테마 |
|---------|-----|------|---------|---------|---------|-------------|---------|-----------------|------|
| 1 | stage_01 | Data Stream | 5 | x1.0 | x1.0 | x1.0 | 20 | 2 | 녹색 회로 |
| 2 | stage_02 | Memory Block | 7 | x1.8 | x1.0 | x1.5 | 25 | 3 | 파란 격자 |
| 3 | stage_03 | Cache Layer | 8 | x3.0 | x1.1 | x2.2 | 30 | 4 | 보라 네온 |

### 1.3 Stage 1 웨이브 구성 (기존 유지, GDD 기준 보정)

현재 Wave_01~05 모두 Bit Node만 사용. GDD 기준으로 Quick/Heavy Node 혼합 필요.

| 웨이브 | Node 구성 | 수량 | 스폰 간격(초) | 대기 시간(초) | 설계 의도 |
|--------|----------|------|-------------|-------------|----------|
| W1 | Bit | 5 | 1.2 | 2 | 기본 학습 (현행 유지) |
| W2 | Bit | 8 | 1.0 | 3 | 수량 증가 (현행 유지) |
| W3 | Bit x6 + Quick x3 | 9 | 0.8 | 3 | Quick 첫 등장. 빠른 적의 위협 |
| W4 | Bit x10 + Quick x5 | 15 | 0.7 | 3 | 물량 압박. 여기서 첫 사망 예상 |
| W5 (보스) | Bit x8 + Quick x4 + Heavy x2 + **Boss(Bit Lord) x1** | 15 | 0.6 | 3 | 보스 웨이브 |

**변경 필요 사항**: Wave_03~05의 spawnGroups에 Quick/Heavy Node 추가. Wave_05에 Boss Node 추가.

### 1.4 Stage 2 웨이브 구성 (신규)

| 웨이브 | Node 구성 | 수량 | 스폰 간격(초) | 대기 시간(초) | 설계 의도 |
|--------|----------|------|-------------|-------------|----------|
| W1 | Bit x8 | 8 | 1.0 | 2 | 스테이지 1보다 시작부터 밀도 높음 |
| W2 | Bit x6 + Quick x4 | 10 | 0.9 | 3 | Quick 비율 증가 |
| W3 | Bit x8 + Heavy x3 | 11 | 0.8 | 3 | Heavy 본격 등장. 방어력 없지만 HP 높음 |
| W4 | Quick x8 + Bit x5 | 13 | 0.7 | 3 | Quick 러시. 속도의 압박 |
| W5 | Bit x10 + Quick x5 + Heavy x3 | 18 | 0.6 | 3 | 혼합 웨이브. 전략 테스트 |
| W6 | Quick x10 + Heavy x5 | 15 | 0.5 | 3 | 고밀도. Cannon 필요성 체감 |
| W7 (보스) | Bit x10 + Quick x6 + Heavy x4 + **Boss(Rush King) x1** | 21 | 0.5 | 3 | 보스: HP 50% 이하 시 속도 2배 |

### 1.5 Stage 3 웨이브 구성 (신규)

| 웨이브 | Node 구성 | 수량 | 스폰 간격(초) | 대기 시간(초) | 설계 의도 |
|--------|----------|------|-------------|-------------|----------|
| W1 | Bit x10 + Quick x3 | 13 | 0.9 | 2 | 높은 시작 밀도 |
| W2 | Quick x8 + Heavy x4 | 12 | 0.8 | 3 | 속도+탱커 조합 |
| W3 | Bit x12 + Shield x3 | 15 | 0.7 | 3 | Shield Node 첫 등장 (방어력 5) |
| W4 | Quick x10 + Shield x5 | 15 | 0.6 | 3 | Shield 비율 증가. 관통형 DPS 필요 |
| W5 | Bit x8 + Quick x6 + Heavy x5 + Shield x3 | 22 | 0.6 | 3 | 4종 혼합 |
| W6 | Heavy x8 + Shield x6 | 14 | 0.5 | 3 | 탱커 러시. Ice로 감속하며 집중 공격 |
| W7 | Quick x12 + Shield x4 + Heavy x4 | 20 | 0.5 | 3 | 혼합 러시 |
| W8 (보스) | Bit x12 + Quick x8 + Heavy x5 + Shield x4 + **Boss(Iron Wall) x1** | 30 | 0.4 | 3 | 보스: 방어막 재생, Cannon으로 리셋 |

### 1.6 스테이지 해금 조건

GDD 기준 (MetaManager 코드 변경 필요):

| 스테이지 | 해금 조건 | 설계 근거 |
|---------|----------|----------|
| 1 | 시작 시 해금 | - |
| 2 | 누적 Bit >= 500 AND 총 킬 >= 100 | Stage 1을 3~5회 반복하면 자연 달성 |
| 3 | 누적 Bit >= 3,000 AND 총 킬 >= 500 | Stage 1~2를 10~15회 반복하면 자연 달성 |

**현재 코드 상태**: MetaManager.AddRunRewards()에서 `currentStageIndex`를 클리어 시만 증가시키는 로직. GDD의 "누적 조건 해금"으로 변경 필요.

### 1.7 필요한 SO 에셋 목록

**신규 생성 필요**:

| 에셋 | 경로 | 내용 |
|------|------|------|
| Stage_02.asset | ScriptableObjects/Stages/ | Memory Block, 웨이브 7개, HP배율 1.8 |
| Stage_03.asset | ScriptableObjects/Stages/ | Cache Layer, 웨이브 8개, HP배율 3.0 |
| Wave_S1_01~05.asset | ScriptableObjects/Waves/ | Stage 1 웨이브 (기존 Wave_01~05 수정 또는 재생성) |
| Wave_S2_01~07.asset | ScriptableObjects/Waves/ | Stage 2 웨이브 7개 |
| Wave_S3_01~08.asset | ScriptableObjects/Waves/ | Stage 3 웨이브 8개 |
| Node_Shield.asset | ScriptableObjects/Nodes/ | Shield Node (hp:40, speed:1.2, defense:5, bitDrop:6) |
| Node_Boss_BitLord.asset | ScriptableObjects/Nodes/ | Boss (hp:300, speed:0.5, defense:3, damage:5, bitDrop:50) |
| Node_Boss_RushKing.asset | ScriptableObjects/Nodes/ | Boss (hp:300, speed:1.0, defense:3, damage:7, bitDrop:50) |
| Node_Boss_IronWall.asset | ScriptableObjects/Nodes/ | Boss (hp:300, speed:0.4, defense:10, damage:10, bitDrop:50) |

**참고**: Boss Node의 기본 HP는 300이며, StageData의 hpMultiplier가 곱연산으로 적용된다. 따라서:
- Stage 1 Boss: 300 * 1.0 = HP 300
- Stage 2 Boss: 300 * 1.8 = HP 540
- Stage 3 Boss: 300 * 3.0 = HP 900

### 1.8 코드 변경 사항

| 변경 | 파일 | 난이도 | 상세 |
|------|------|--------|------|
| GameManager.stages 배열에 Stage_02, Stage_03 추가 | Inspector에서 설정 | SO만 | 코드 변경 없음 |
| Stage 1 Wave_03~05 수정 | SO 수정 | SO만 | spawnGroups에 Quick/Heavy/Boss 추가 |
| 스테이지 해금 조건 변경 | MetaManager.cs | 코드 | 누적 Bit/킬 수 기반 해금 로직 추가 |
| PlayerSaveData에 totalNodesKilled 추가 | PlayerSaveData.cs | 코드 | 총 킬 수 저장 |
| RunManager.OnNodeKilled에서 누적 킬 수 기록 | MetaManager.cs | 코드 | AddRunRewards에 nodesKilled 파라미터 추가 |

---

## 2. 타워 인게임 업그레이드 (Bit 소모)

### 2.1 설계 방향

현재 타워 레벨업은 합성(같은 타워 겹치기)으로만 가능. GDD에서는 Bit 소모 업그레이드도 지원. Phase 3에서 **Bit 소모 레벨업**을 추가하여 합성과 병행 가능하게 한다.

### 2.2 업그레이드 비용 (인게임 Bit)

GDD 기준 타워별 인게임 업그레이드 비용:

| 타워 | Lv1->2 | Lv2->3 | Lv3->4 | Lv4->5 | 비고 |
|------|--------|--------|--------|--------|------|
| Arrow | 25 | 35 | 50 | 70 | 저렴. 가장 먼저 레벨업 |
| Cannon | 50 | 70 | 100 | 140 | 중간. AoE 효율 고려 |
| Ice | 35 | 50 | 70 | 100 | 중간. 감속 강화 가치 |

### 2.3 TowerData SO 변경

현재 TowerData에 `placeCost` (배치 비용)만 있음. **레벨업 비용 배열 추가 필요**:

```csharp
[Header("인게임 레벨업 비용 (Bit) - Lv1->2, Lv2->3, Lv3->4, Lv4->5")]
public int[] upgradeCost = { 25, 35, 50, 70 };

public int GetUpgradeCost(int currentLevel)
{
    int index = Mathf.Clamp(currentLevel - 1, 0, upgradeCost.Length - 1);
    return upgradeCost[index];
}
```

### 2.4 업그레이드 UI 흐름

1. 배치된 타워를 **탭/클릭**하면 타워 정보 툴팁 표시 (TowerInfoTooltip 기존 존재)
2. 툴팁에 **"업그레이드" 버튼** 추가
3. 버튼 클릭 시:
   - 현재 레벨 < 최대 레벨(5) 확인
   - RunManager.BitEarned >= upgradeCost 확인
   - Bit 차감 + Tower.LevelUp() 호출
   - 업그레이드 이펙트 + 사운드

### 2.5 합성과의 관계

| 방법 | 조건 | 결과 | 비용 |
|------|------|------|------|
| Bit 업그레이드 | 타워 탭 + Bit 충분 | 레벨 +1 | Bit 소모 |
| 합성 | 같은 종류+레벨 타워 드래그 | 레벨 +1 | 타워 1기 소모 |

두 방법 모두 최대 Lv5까지. 합성이 더 효율적이지만 같은 타워가 필요. Bit 업그레이드는 확실하지만 비용 높음.

### 2.6 코드 변경 사항

| 변경 | 파일 | 난이도 | 상세 |
|------|------|--------|------|
| upgradeCost[] 필드 추가 | TowerData.cs | 코드 (소) | 배열 + getter 메서드 |
| 기존 SO에 비용 수치 입력 | Tower_Arrow/Cannon/Ice.asset | SO만 | Inspector 수치 입력 |
| 인게임 Bit 차감 로직 | RunManager.cs 또는 Tower.cs | 코드 (소) | SpendBit(int) 메서드 추가 |
| 업그레이드 버튼 UI | TowerInfoTooltip.cs | 코드 (중) | 기존 툴팁에 버튼 추가 |
| 업그레이드 이펙트/사운드 | Tower.cs + SoundKeys.cs | 코드 (소) | LevelUp 시 연출 |

---

## 3. 보스 몬스터

### 3.1 설계 방향

GDD v3.0에서 이미 상세 설계 완료. Phase 3에서는 **Stage 1~3의 보스 3종**을 구현.

### 3.2 보스 공통 시스템

#### 3.2.1 보스 HP 바 (화면 상단)

보스 등장 시 화면 상단에 전용 HP 바 표시:
- 보스 이름 + HP 게이지
- 일반 기지 HP 바 아래에 위치
- 보스가 살아있는 동안만 표시

#### 3.2.2 보스 처치 연출

1. 슬로우모션 1초 (Time.timeScale = 0.2f -> 1초 후 복귀)
2. 대형 파괴 이펙트
3. Core 보상 팝업 (+N Core)
4. 스테이지 클리어 판정

#### 3.2.3 보스 특수 능력 시스템

Node.cs에 보스 전용 로직을 추가하는 방식. 현재 Node.cs는 범용이므로, **NodeData에 bossAbilityType 필드를 추가**하여 분기:

```csharp
public enum BossAbilityType
{
    None,           // 일반 Node 또는 능력 없는 보스
    Enrage,         // Rush King: HP 50% 이하 시 속도 2배
    ArmorRegen,     // Iron Wall: 주기적 방어력 재생, AoE 피격 시 리셋
}
```

### 3.3 Stage 1 보스: Bit Lord

| 항목 | 값 |
|------|-----|
| NodeType | Boss |
| HP (기본) | 300 |
| 이동속도 | 0.5 |
| 방어력 | 3 |
| 기지 데미지 | 5 |
| Bit 드롭 | 50 |
| 특수 능력 | None (순수 HP 스펀지) |

**밸런스 계산**:
- Arrow Lv1 DPS: 10.0 (defense 3 감안하면 실질 DPS 약 6.25)
- Arrow 3기(자동 배치): 실질 DPS 약 18.75
- 처치 시간: 300 / 18.75 = 약 16초
- 그 사이 일반 Node(Bit x8 + Quick x4 + Heavy x2)의 기지 도달을 막아야 함
- 영구 업그레이드 없이도 2~3회차에 클리어 가능한 수준

### 3.4 Stage 2 보스: Rush King

| 항목 | 값 |
|------|-----|
| NodeType | Boss |
| HP (기본) | 300 (x1.8 스케일링 = 540) |
| 이동속도 | 1.0 |
| 방어력 | 3 |
| 기지 데미지 | 7 |
| Bit 드롭 | 50 (x1.5 스케일링 = 75) |
| 특수 능력 | Enrage: HP 50% 이하 시 속도 2배 (2.0) |

**Enrage 구현**:
```csharp
// Node.cs Move() 내부
if (Data.bossAbilityType == BossAbilityType.Enrage)
{
    if (CurrentHp <= _scaledHp * 0.5f)
        speed *= 2f;
}
```

### 3.5 Stage 3 보스: Iron Wall

| 항목 | 값 |
|------|-----|
| NodeType | Boss |
| HP (기본) | 300 (x3.0 스케일링 = 900) |
| 이동속도 | 0.4 |
| 방어력 | 10 |
| 기지 데미지 | 10 |
| Bit 드롭 | 50 (x2.2 스케일링 = 110) |
| 특수 능력 | ArmorRegen: 10초마다 방어력 +5 (최대 25). AoE 피격 시 방어력 리셋 |

**ArmorRegen 구현**:
```csharp
// Node.cs 필드 추가
float _armorRegenTimer;
int _bonusDefense;

// Update() 내부
if (Data.bossAbilityType == BossAbilityType.ArmorRegen)
{
    _armorRegenTimer += Time.deltaTime;
    if (_armorRegenTimer >= 10f)
    {
        _bonusDefense = Mathf.Min(_bonusDefense + 5, 25);
        _armorRegenTimer = 0f;
    }
}

// TakeDamage에서 _defense 대신 (_defense + _bonusDefense) 사용
// AoE 피격 판정 시 _bonusDefense = 0으로 리셋
```

### 3.6 필요한 SO 에셋

| 에셋 | 타입 | 수치 |
|------|------|------|
| Node_Boss_BitLord.asset | NodeData | type:Boss, hp:300, speed:0.5, defense:3, damage:5, bitDrop:50 |
| Node_Boss_RushKing.asset | NodeData | type:Boss, hp:300, speed:1.0, defense:3, damage:7, bitDrop:50 |
| Node_Boss_IronWall.asset | NodeData | type:Boss, hp:300, speed:0.4, defense:10, damage:10, bitDrop:50 |

### 3.7 코드 변경 사항

| 변경 | 파일 | 난이도 | 상세 |
|------|------|--------|------|
| BossAbilityType enum 추가 | NodeData.cs | 코드 (소) | enum + 필드 1개 |
| 보스 능력 로직 (Enrage, ArmorRegen) | Node.cs | 코드 (중) | Update/TakeDamage 분기 |
| 보스 HP 바 UI | InGameUI.cs | 코드 (중) | 상단 HP 바 추가. WaveSpawner에서 보스 참조 전달 |
| 보스 처치 연출 (슬로모션, 이펙트) | Node.cs + RunManager.cs | 코드 (중) | Die()에서 보스 타입 확인 시 연출 |
| 보스 사운드 | SoundKeys.cs | 코드 (소) | BossAppear, BossDefeat 이미 존재. 보스 전용 BGM 전환도 구현 완료 |
| Boss Node 프리팹 3종 | Prefabs/Nodes/ | 아트/프리팹 | 시각적 차별화 (크기, 색상) |

---

## 4. 초반 경험 설계 (온보딩/FTUE)

### 4.1 설계 철학

GDD의 "3초/30초/30분 규칙"을 구현. **텍스트 설명이 아닌 플레이로 배우게** 한다.

- 유료 게임이므로 메뉴/로딩 최소화
- 첫 10분 안에 "이 게임 재밌다" 확신
- Steam 환불 시점(2시간) 훨씬 이전에 몰입 확보

### 4.2 첫 플레이 흐름 (분 단위)

```
[0:00] 타이틀 화면 -> "시작" 클릭
[0:03] Hub 화면 스킵, 바로 Stage 1 진입
       - 첫 플레이 감지: currentStageIndex == 0 && totalBit == 0
       - "타워가 적을 잡습니다!" 1줄 텍스트 오버레이 (2초 표시 후 자동 소멸)
[0:05] Wave 1 시작. Arrow 3기 자동 배치(기존 FTUE 로직).
       Node 5마리가 천천히 이동. 타워가 자동 공격.
[0:30] Wave 2. 더 많은 Node. 기지 HP가 줄기 시작.
[0:45] Wave 3. Quick Node 등장. "빠른 적이다!" 느낌.
[1:00] 기지 HP 0. 첫 사망.
       - 런 종료 패널: "패배" + 획득 Bit 표시
       - "Hub로 이동하여 강화하세요!" 가이드 텍스트 (첫 사망 시만)
[1:10] Hub 화면. 스킬 트리에서 첫 업그레이드 구매.
       - 구매 가능한 스킬에 펄스 강조 이펙트 (첫 방문 시만)
[1:15] "출격" 클릭. Stage 1 재도전.
       - "강해진 것 같다!" 체감. 더 오래 생존.
[1:30] 두 번째 사망 (Wave 3~4에서).
       - 더 많은 Bit로 더 많은 업그레이드.
[2:00] 3~5회차. 성장 루프 체득 완료.
[2:30] Wave 5(보스) 도달. 보스와의 첫 대면.
[3:00] 보스 처치 성공. Stage 1 클리어! Core 획득!
       - "새 스테이지 해금!" 알림
```

### 4.3 FTUE 가이드 시스템

**최소한의 텍스트 가이드** (팝업이 아닌 화면 하단 오버레이 1줄):

| 트리거 | 가이드 텍스트 | 표시 조건 |
|--------|-------------|----------|
| 첫 런 시작 | "타워가 적을 자동으로 공격합니다" | totalBit == 0, 첫 웨이브 시작 시 |
| 웨이브 클리어 보상 | "타워를 획득했습니다! 드래그하여 배치하세요" | 첫 타워 드롭 시 |
| 첫 사망 | "성장하면 더 강해집니다 -- Hub에서 업그레이드!" | 첫 RunEnd 시 |
| Hub 첫 진입 | "스킬을 눌러 영구 강화를 구매하세요" | Hub 첫 표시 시 |
| 보스 등장 | "보스가 나타났습니다! 처치하면 클리어!" | 보스 웨이브 시작 시 |

**구현 방식**: `FtueManager` (MetaManager에 ftue 플래그 저장) -> InGameUI/HubUI에서 가이드 텍스트 표시

### 4.4 첫 런 클리어 보상

Stage 1 클리어 시 특별 보상으로 성장 루프 체감 극대화:

| 보상 | 양 | 효과 |
|------|-----|------|
| Core | 2 (기본) | 스킬 트리에서 첫 Core 노드 구매 가능 |
| Bonus Bit | 100 | "클리어 보너스!" 팝업으로 추가 지급 |
| 신규 스테이지 해금 알림 | - | "Stage 2: Memory Block 해금!" (조건 충족 시) |

### 4.5 코드 변경 사항

| 변경 | 파일 | 난이도 | 상세 |
|------|------|--------|------|
| FtueManager 또는 PlayerSaveData에 ftue 플래그 | PlayerSaveData.cs | 코드 (소) | bool[] ftueFlags 추가 |
| 가이드 텍스트 오버레이 UI | InGameUI.cs + HubUI.cs | 코드 (중) | 1줄 텍스트 + 페이드 애니메이션 |
| 첫 플레이 시 Hub 스킵 로직 | GameManager.cs | 코드 (소) | Start()에서 분기 |
| 첫 클리어 보너스 Bit | RunManager.cs 또는 MetaManager.cs | 코드 (소) | 첫 클리어 감지 + 보너스 |

---

## 5. Shield Node 설계 (신규 적 타입)

Stage 3에서 등장하는 Shield Node 상세 설계.

### 5.1 Shield Node 수치

| 스탯 | 값 | 비고 |
|------|-----|------|
| HP | 40 | Bit(20)의 2배 |
| 이동속도 | 1.2 | 보통 |
| 방어력 | 5 | Arrow Lv1(데미지 8)로 실질 3 데미지만 가함 |
| 기지 데미지 | 1 | 일반 |
| Bit 드롭 | 6 | Heavy(8)보다는 낮음 |

### 5.2 밸런스 분석

- Arrow Lv1(dmg 8) vs Shield(def 5): 실질 데미지 3 -> 처치에 14히트 필요 (약 11초)
- Arrow Lv3(dmg 18) vs Shield(def 5): 실질 데미지 13 -> 처치에 4히트 필요 (약 2.4초)
- Cannon Lv1(dmg 20) vs Shield(def 5): 실질 데미지 15 -> 처치에 3히트 (+ AoE로 다수 동시 처리)

설계 의도: **레벨업과 타워 다양성의 중요성**을 체감시킴. 낮은 레벨의 Arrow만으로는 Shield를 효율적으로 처리할 수 없어, 업그레이드와 Cannon 도입 동기 부여.

---

## 6. 에이전트별 업무 분배 제안

### 6.1 프로그래밍팀장 (unity-gameplay-programmer)

| 우선순위 | 작업 | 공수 | 상세 |
|---------|------|------|------|
| P0 | NodeData에 BossAbilityType 필드 추가 | 소 | enum + 필드 1개 |
| P0 | Node.cs 보스 능력 로직 (Enrage, ArmorRegen) | 중 | Update/TakeDamage 분기 |
| P0 | TowerData에 upgradeCost[] 추가 | 소 | 배열 + getter |
| P0 | RunManager에 SpendBit() 메서드 추가 | 소 | Bit 차감 로직 |
| P1 | MetaManager 스테이지 해금 조건 변경 | 중 | 누적 Bit/킬 기반 |
| P1 | PlayerSaveData에 totalNodesKilled 추가 | 소 | 세이브 필드 |
| P1 | 보스 처치 연출 (슬로모션) | 소 | Time.timeScale 제어 |
| P2 | FtueManager (FTUE 플래그 관리) | 중 | 가이드 트리거 시스템 |

### 6.2 기획팀장 (game-designer) - 본인

| 우선순위 | 작업 | 공수 | 상세 |
|---------|------|------|------|
| P0 | Stage 1 Wave_03~05 SO 수정 | 소 | Quick/Heavy/Boss Node 추가 |
| P0 | Boss Node SO 3종 생성 | 소 | Node_Boss_BitLord/RushKing/IronWall |
| P0 | Shield Node SO 생성 | 소 | Node_Shield |
| P0 | Stage_02, Stage_03 SO 생성 | 소 | 웨이브 배열 연결 |
| P0 | Stage 2 Wave SO 7개 생성 | 중 | Wave_S2_01~07 |
| P0 | Stage 3 Wave SO 8개 생성 | 중 | Wave_S3_01~08 |
| P1 | TowerData SO 업그레이드 비용 수치 입력 | 소 | Inspector 작업 |

### 6.3 UI팀장 (unity-ui-developer)

| 우선순위 | 작업 | 공수 | 상세 |
|---------|------|------|------|
| P0 | 보스 HP 바 UI (화면 상단) | 중 | 보스 이름 + HP 게이지 |
| P0 | 타워 업그레이드 버튼 (TowerInfoTooltip 확장) | 중 | 탭 -> 툴팁 -> 업그레이드 버튼 |
| P1 | FTUE 가이드 텍스트 오버레이 | 중 | 하단 1줄 텍스트 + 페이드 |
| P1 | 스테이지 해금 알림 UI | 소 | 런엔드 패널에 "새 스테이지 해금!" 표시 |
| P2 | 보스 처치 연출 UI (Core 보상 팝업) | 소 | "+2 Core" 팝업 |

### 6.4 TA팀장 (unity-technical-artist)

| 우선순위 | 작업 | 공수 | 상세 |
|---------|------|------|------|
| P0 | Boss Node 프리팹 3종 (시각 차별화) | 중 | 크기 2~3배, 고유 색상, 오라 이펙트 |
| P0 | Shield Node 프리팹 | 소 | 방패 시각 표현 |
| P1 | 보스 처치 이펙트 | 중 | 대형 파괴 파티클 |
| P1 | 타워 업그레이드 이펙트 | 소 | 레벨업 시 번쩍임 |
| P2 | Stage 2/3 시각 테마 변경 | 중 | 맵 컬러 팔레트, 배경 변화 |

### 6.5 QA팀장 (unity-qa-engineer)

| 우선순위 | 작업 | 공수 | 상세 |
|---------|------|------|------|
| P0 | Stage 1 보스 웨이브 밸런스 테스트 | 중 | 3~5회차에 클리어 가능한지 |
| P0 | Stage 2/3 웨이브 난이도 검증 | 중 | 적절한 사망/클리어 비율 |
| P1 | 타워 업그레이드 기능 테스트 | 소 | Bit 차감, 레벨업 동작 |
| P1 | 스테이지 해금 조건 테스트 | 소 | 누적 조건 충족 시 정상 해금 |
| P2 | FTUE 흐름 테스트 | 소 | 신규 유저 시뮬레이션 |

### 6.6 사운드 디렉터 (unity-sound-director)

| 우선순위 | 작업 | 공수 | 상세 |
|---------|------|------|------|
| P1 | 보스 전용 BGM (이미 BgmBoss 키 존재, 음원 확인) | 소 | 기존 리소스 검증 |
| P1 | 타워 업그레이드 SFX | 소 | 레벨업 효과음 |
| P2 | Stage 2/3 전투 BGM 변주 | 중 | 스테이지별 분위기 차별화 |

---

## 7. 구현 우선순위 및 일정 제안

### Phase 3 스프린트 계획

| 순서 | 작업 그룹 | 담당 | 선행 조건 | 예상 공수 |
|------|----------|------|----------|----------|
| 1 | NodeData 코드 변경 (BossAbility, Shield SO) | 프로그래밍 + 기획 | - | 소 |
| 2 | TowerData 코드 변경 (upgradeCost) | 프로그래밍 + 기획 | - | 소 |
| 3 | Boss/Shield Node 프리팹 + SO 생성 | TA + 기획 | 1 완료 | 중 |
| 4 | Stage 1 Wave 수정 + Stage 2/3 SO 생성 | 기획 | 3 완료 | 중 |
| 5 | 보스 HP 바 UI + 타워 업그레이드 UI | UI | 1, 2 완료 | 중 |
| 6 | 스테이지 해금 조건 변경 | 프로그래밍 | 4 완료 | 중 |
| 7 | FTUE 시스템 | 프로그래밍 + UI | 6 완료 | 중 |
| 8 | QA 검증 | QA | 5, 6 완료 | 중 |

---

## 8. 리스크 및 참고 사항

### 8.1 코드 변경 최소화 전략

- **SO 주도 설계**: 대부분의 콘텐츠(스테이지, 웨이브, Node, 타워 수치)는 SO 에셋만으로 확장 가능
- **기존 시스템 활용**: WaveSpawner의 IsBossWave(), Tower의 LevelUp(), HubUI의 RefreshStageDropdown() 등 이미 구현된 인프라 최대한 활용
- **점진적 확장**: 보스 능력은 Node.cs에 분기 추가. 별도 BossNode 클래스를 만들지 않아 상속 복잡도 회피

### 8.2 밸런스 리스크

- Stage 1 보스(HP 300)가 초보자에게 너무 어렵거나 쉬울 수 있음 -> QA 검증 후 SO 수치 조정
- Stage 2/3 스케일링(x1.8, x3.0)이 영구 업그레이드 진행 속도와 맞지 않을 수 있음 -> 테스트 플레이 필수
- Shield Node 방어력 5가 Arrow Lv1에게 너무 가혹할 수 있음 -> 방어력 3으로 하향 검토

### 8.3 Phase 4 예고

Phase 3 완료 후 확장 방향:
- Stage 4~6 추가 (중반 콘텐츠)
- Lightning Tower 구현
- 보물상자 시스템
- Swarm/Regen Node 타입 추가
- 보스 특수 패턴 확장 (Swarm Mother, Regen Core)
