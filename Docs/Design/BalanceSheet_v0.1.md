# Nodebreaker TD - Balance Sheet v0.1

| 항목 | 내용 |
|------|------|
| 문서 버전 | 0.1 |
| 최종 수정 | 2026-02-15 |
| 작성자 | 기획팀장 (Game Designer) |
| 기준 문서 | GDD v2.0 |

---

## 1. 현재 SO 구조 및 밸런싱 가능 필드 정리

### 1.1 TowerData (타워)

| 필드 | 타입 | 설명 | 밸런싱 대상 |
|------|------|------|------------|
| `damage[5]` | float[] | Lv1~Lv4 데미지 (5번째 미사용) | O |
| `attackSpeed[5]` | float[] | Lv1~Lv4 공격속도 | O |
| `range[5]` | float[] | Lv1~Lv4 사거리 | O |
| `placeCost` | int | 인게임 배치 비용 (Bit) | O |

**현재 구현 상태 비교 (SO 에셋 vs GDD)**:

| 항목 | SO 에셋 (Tower_Arrow) | GDD 수치 |
|------|---------------------|----------|
| damage Lv1 | 10 | 8 |
| damage Lv2 | 15 | 12 |
| damage Lv3 | 22 | 18 |
| damage Lv4 | 33 | 26 |
| attackSpeed Lv1 | 1.0 (= 1초/발) | 0.8초/발 (= 공속 1.25) |
| attackSpeed Lv2 | 1.1 | 0.7초/발 (= 공속 1.43) |
| placeCost | 50 | 30 |

**문제 발견**: SO의 attackSpeed 필드는 "초당 공격 횟수"로 사용되고 있으며 (Tower.cs의 `1f / attackSpeed`), GDD는 "공격 간격(초)"으로 기술되어 있다. 현재 SO값 1.0은 "1초당 1회 = 1초 간격"을 의미하므로, GDD의 0.8초 간격에 대응하려면 SO 값을 `1/0.8 = 1.25`로 설정해야 한다.

### 1.2 NodeData (몬스터/Node)

| 필드 | 타입 | 설명 | 밸런싱 대상 |
|------|------|------|------------|
| `hp` | float | 기본 HP | O |
| `speed` | float | 이동속도 (유닛/초) | O |
| `bitDrop` | int | 처치 시 Bit 드롭 | O |
| `damage` | int | 기지에 주는 데미지 | O |

**현재 구현 상태 비교**:

| 항목 | SO 에셋 (Node_Bit) | GDD 수치 |
|------|-------------------|----------|
| hp | 30 | 20 |
| speed | 2.5 | 1.5 |
| bitDrop | 8 | 3 |
| damage | 1 | 1 |

**문제 발견**: Node_Bit의 HP가 GDD 대비 50% 높고, 속도가 67% 빠르며, Bit 드롭이 167% 높다. 현재 밸런스는 GDD 설계 의도와 상당히 괴리가 있다.

### 1.3 SkillNodeData (영구 업그레이드 스킬)

| 필드 | 타입 | 설명 | 밸런싱 대상 |
|------|------|------|------------|
| `valuePerLevel` | float | 레벨당 효과량 | O |
| `maxLevel` | int | 최대 레벨 | O |
| `baseCost` | int | 기본 비용 (Bit) | O |
| `growthRate` | float | 비용 증가 배율 | O |

**현재 구현 상태 비교**:

| 스킬 | SO baseCost | GDD baseCost | SO growthRate | GDD growthRate |
|------|-----------|------------|-------------|--------------|
| 공격력 (atk_dmg) | 50 | 50 | 1.3 | 1.3 |
| 공격속도 (atk_spd) | 40 | 80 | 1.25 | 1.3 |
| 기지 HP (base_hp) | 30 | 40 | 1.2 | 1.25 |

**문제 발견**: 공격속도 스킬의 baseCost가 GDD의 절반이고, 기지 HP 스킬의 baseCost도 75% 수준이다. 이로 인해 초반 업그레이드가 GDD 설계보다 훨씬 쉽다.

### 1.4 StageData (스테이지)

| 필드 | 타입 | 설명 | 밸런싱 대상 |
|------|------|------|------------|
| `hpMultiplier` | float | Node HP 배율 | O |
| `speedMultiplier` | float | Node 속도 배율 | O |
| `bitDropMultiplier` | float | Bit 드롭 배율 | O |
| `coreReward` | int | 클리어 시 Core 보상 | O |
| `baseHp` | int | 기지 HP | O |
| `waves` | WaveData[] | 웨이브 배열 | O |

**현재 Stage_01**:
- baseHp: **5** (GDD 설계: 20)
- coreReward: 2 (GDD와 일치)
- waves: Wave_01 1개만 존재 (GDD 설계: 5개 웨이브)

**심각한 문제**: 기지 HP가 GDD의 1/4 수준. Node_Bit의 damage가 1이므로 5마리만 통과하면 즉사.

### 1.5 WaveData (웨이브)

| 필드 | 타입 | 설명 | 밸런싱 대상 |
|------|------|------|------------|
| `spawnGroups[].nodeData` | NodeData | 스폰할 Node 종류 | O |
| `spawnGroups[].count` | int | 스폰 수 | O |
| `spawnGroups[].spawnInterval` | float | 스폰 간격(초) | O |
| `delayBeforeWave` | float | 웨이브 시작 전 대기(초) | O |

**현재 Wave_01**:
- Node_Bit x10, 간격 0.6초
- 대기 2초

---

## 2. Stage 1 밸런싱 분석 및 수치 제안

### 2.1 현재 밸런스 시뮬레이션 (SO 에셋 기준)

**전제**: Arrow Tower Lv1, 영구 업그레이드 0

- Arrow DPS: damage 10 / (1/attackSpeed 1.0) = **10 DPS**
- Node_Bit HP: 30, 속도: 2.5
- 처치 소요 시간: 30/10 = **3.0초**
- 스폰 간격: 0.6초

> Arrow 1기로 첫 마리 처치에 3초 소요. 그 동안 4~5마리가 추가 스폰됨.
> 기지 HP가 5밖에 안 되므로 5마리 통과 = 게임 오버.
> 웨이브 1개(10마리) 중 Arrow 1기로 처치 가능한 수: ~3마리 (10초 동안).
> 나머지 ~7마리 중 상당수가 기지 도달 가능.

**결론**: 현재 SO 수치는 "초반 즉사" 설계 의도에는 부합하나, 기지 HP 5는 너무 적다. 1~2마리만 통과해도 기지가 위태롭고, 첫 런에서 충분한 Bit를 벌기 전에 죽어서 성장의 첫 맛을 보기 어렵다.

### 2.2 GDD 기준 밸런스 시뮬레이션

**GDD 수치 적용 시**:

- Arrow DPS: 8 / 0.8 = **10 DPS** (SO와 동일 결과)
- Node_Bit HP: 20, 속도: 1.5
- 처치 소요 시간: 20/10 = **2.0초**
- 웨이브1 스폰 간격: 1.2초

> Arrow 1기로 첫 마리 처치에 2초. 스폰 간격 1.2초이므로 동시 2마리까지 관리 가능.
> 기지 HP 20이므로 상당한 여유.
> 웨이브 1(5마리): 2~3마리 처치, 2~3마리 통과. 기지 HP 17~18.
> 웨이브 2(8마리): 추가 타워 배치 가능 (Bit 6~9 획득). 그러나 비용 30이라 배치 불가.
> 웨이브 3(Quick Node 등장): 높은 확률로 사망.

**GDD 경제 문제 발견**: GDD에서 Bit Node 드롭이 3이고 placeCost가 30이면, 10마리 처치해야 타워 1기 추가. 웨이브 1(5마리)로는 Bit 15밖에 못 벌어 타워 추가 불가. 이는 "첫 성장의 맛"을 너무 늦게 제공.

### 2.3 권장 밸런스: "빠른 죽음, 빠른 성장" 최적화

GDD의 설계 철학(30초 규칙: 30초 안에 첫 죽음 또는 첫 성장)과 도파민 설계를 결합한 최적 수치.

**핵심 원칙**:
1. 첫 런 30~60초에 죽는다
2. 첫 런에서 Bit 40~60 정도 획득한다 (영구 업그레이드 1개 가능)
3. 기지 HP는 적절히 버틸 수 있되, 웨이브 2~3에서 죽는다
4. Node는 한 방에 안 죽지만 너무 오래 걸리지도 않는다

---

## 3. 타워 밸런싱 테이블

### 3.1 Arrow Tower (기본 타워) - 최종 권장 수치

GDD 수치를 기반으로, 현재 코드의 attackSpeed 해석(초당 공격 횟수)에 맞게 변환.

| 스탯 | Lv1 | Lv2 | Lv3 | Lv4 |
|------|-----|-----|-----|-----|
| 데미지 | 8 | 12 | 18 | 26 |
| 공격속도 (초당 횟수) | 1.25 | 1.43 | 1.67 | 2.0 |
| 사거리 | 3.0 | 3.2 | 3.4 | 3.6 |
| DPS | 10.0 | 17.2 | 30.1 | 52.0 |
| 인게임 비용 (Bit) | 30 | - | - | - |

> 변환: GDD 공격간격 0.8초 = 초당 1.25회, 0.7초 = 1.43회, 0.6초 = 1.67회, 0.5초 = 2.0회

### 3.2 Cannon Tower (AoE) - 향후 구현

| 스탯 | Lv1 | Lv2 | Lv3 | Lv4 |
|------|-----|-----|-----|-----|
| 데미지 | 20 | 35 | 55 | 80 |
| 공격속도 (초당 횟수) | 0.67 | 0.77 | 0.91 | 1.11 |
| 사거리 | 2.5 | 2.7 | 3.0 | 3.3 |
| 폭발 범위 | 1.0 | 1.1 | 1.2 | 1.4 |
| DPS (단일) | 13.3 | 26.9 | 50.0 | 88.9 |
| 인게임 비용 (Bit) | 60 | - | - | - |

> 변환: GDD 공격간격 1.5초 = 0.67회/s, 1.3초 = 0.77회/s 등

### 3.3 Ice Tower (감속 유틸리티) - 향후 구현

| 스탯 | Lv1 | Lv2 | Lv3 | Lv4 |
|------|-----|-----|-----|-----|
| 데미지 | 4 | 7 | 12 | 18 |
| 공격속도 (초당 횟수) | 1.0 | 1.11 | 1.25 | 1.43 |
| 사거리 | 3.0 | 3.2 | 3.5 | 3.8 |
| 감속률 | 25% | 35% | 45% | 55% |
| 감속 지속(초) | 1.5 | 1.8 | 2.0 | 2.3 |
| 인게임 비용 (Bit) | 45 | - | - | - |

### 3.4 Lightning Tower (체인) - 향후 구현

| 스탯 | Lv1 | Lv2 | Lv3 | Lv4 |
|------|-----|-----|-----|-----|
| 데미지 | 12 | 20 | 32 | 48 |
| 공격속도 (초당 횟수) | 0.83 | 1.0 | 1.11 | 1.25 |
| 사거리 | 3.0 | 3.3 | 3.5 | 3.8 |
| 체인 횟수 | 2 | 3 | 4 | 5 |
| 체인 데미지 감소 | 30% | 25% | 20% | 15% |
| 인게임 비용 (Bit) | 80 | - | - | - |

### 3.5 Laser Tower (관통 지속) - 향후 구현

| 스탯 | Lv1 | Lv2 | Lv3 | Lv4 |
|------|-----|-----|-----|-----|
| 초당 데미지 | 25 | 40 | 60 | 90 |
| 빔 지속(초) | 2.0 | 2.2 | 2.5 | 2.8 |
| 쿨다운(초) | 3.0 | 2.8 | 2.5 | 2.2 |
| 사거리 | 4.0 | 4.3 | 4.5 | 4.8 |
| 관통 수 | 3 | 4 | 5 | 7 |
| 인게임 비용 (Bit) | 120 | - | - | - |

### 3.6 Void Tower (범위 지속) - 향후 구현

| 스탯 | Lv1 | Lv2 | Lv3 | Lv4 |
|------|-----|-----|-----|-----|
| 초당 데미지 (범위 전체) | 15 | 25 | 40 | 65 |
| 효과 범위 | 2.0 | 2.3 | 2.6 | 3.0 |
| 추가 효과 | - | - | 방어력 무시 | +감속 10% |
| 인게임 비용 (Bit) | 150 | - | - | - |

### 3.7 타워 DPS 비교표 (Lv1 기준, 비용효율 분석)

| 타워 | DPS | 비용 | DPS/비용 | 역할 |
|------|-----|------|---------|------|
| Arrow | 10.0 | 30 | 0.33 | 단일 범용 |
| Cannon | 13.3 (x적수) | 60 | 0.22 (x적수) | AoE 물량전 |
| Ice | 4.0 + 감속 | 45 | 유틸리티 | 감속 서포트 |
| Lightning | 10.0 + 체인 | 80 | 0.125 (x체인) | 다수 처리 |
| Laser | ~10.0 관통 | 120 | 0.08 (x관통) | 일직선 학살 |
| Void | 15.0 범위 | 150 | 0.10 (x범위) | 경로 장악 |

> Arrow의 DPS/비용이 가장 높은 것은 의도적. 기본 타워가 항상 유효해야 후반에도 사용 동기가 있다. 특수 타워는 상황별 효율에서 차별화.

---

## 4. 몬스터(Node) 밸런싱 테이블

### 4.1 Stage 1 기준 (배율 x1.0)

| ID | 이름 | HP | 이동속도 | 방어력 | 기지 DMG | Bit 드롭 | 특수 | 등장 |
|----|------|-----|---------|--------|---------|---------|------|------|
| N01 | Bit Node | 20 | 1.5 | 0 | 1 | 3 | - | St.1 W1~ |
| N02 | Quick Node | 12 | 3.0 | 0 | 1 | 4 | - | St.1 W3~ |
| N03 | Heavy Node | 80 | 0.8 | 0 | 2 | 8 | - | St.1 W5 |
| N04 | Shield Node | 40 | 1.2 | 5 | 1 | 6 | 고정 방어력 | St.3~ |
| N05 | Swarm Node | 5 | 2.0 | 0 | 1 | 1 | 대량 스폰 | St.4~ |
| N06 | Regen Node | 50 | 1.0 | 0 | 1 | 7 | HP 5%/s 회복 | St.5~ |
| N07 | Phase Node | 35 | 1.5 | 0 | 1 | 8 | 2초 이동/1초 무적 | St.6~ |
| N08 | Split Node | 30 | 1.3 | 0 | 1 | 5 | 분열(x2, 각 HP 15) | St.7~ |
| N09 | Boss Node | 300 | 0.5 | 3 | 5 | 50 | - | 각 스테이지 최종 웨이브 |

### 4.2 스테이지별 Node 스케일링

| 스테이지 | HP 배율 | 속도 배율 | Bit 드롭 배율 | 기지 HP | Core 보상 |
|---------|---------|---------|-------------|---------|----------|
| 1 | x1.0 | x1.0 | x1.0 | 20 | 2 |
| 2 | x1.8 | x1.0 | x1.5 | 20 | 3 |
| 3 | x3.0 | x1.1 | x2.2 | 25 | 4 |
| 4 | x5.0 | x1.1 | x3.5 | 25 | 5 |
| 5 | x8.0 | x1.2 | x5.0 | 30 | 6 |
| 6 | x13.0 | x1.2 | x8.0 | 30 | 7 |
| 7 | x22.0 | x1.3 | x12.0 | 35 | 9 |
| 8 | x40.0 | x1.3 | x20.0 | 35 | 11 |
| 9 | x70.0 | x1.4 | x35.0 | 40 | 14 |
| 10 | x120.0 | x1.5 | x60.0 | 40 | 엔딩 |

### 4.3 Node 처치 시간 분석 (Arrow Lv1 기준, 영구 업그레이드 0)

| Node | HP | Arrow DPS | 처치 시간 | 통과까지 추정 시간* | 처치 가능? |
|------|-----|----------|----------|------------------|----------|
| Bit (St.1) | 20 | 10 | 2.0초 | ~6초 | O (여유) |
| Quick (St.1) | 12 | 10 | 1.2초 | ~3초 | 빡빡 |
| Heavy (St.1) | 80 | 10 | 8.0초 | ~12초 | O (느림) |
| Bit (St.3) | 60 | 10 | 6.0초 | ~6초 | X (영구 업그레이드 필수) |

> *통과까지 추정 시간: 스폰 지점에서 기지까지 이동하는데 걸리는 시간 (맵 크기 의존)

---

## 5. 스킬(영구 업그레이드) 밸런싱 테이블

### 5.1 현재 구현된 스킬 (Bit 노드)

#### 공격력 강화 (atk_dmg)

| 레벨 | 효과 (누적) | 비용 (Bit) | 누적 비용 |
|------|-----------|----------|----------|
| 1 | +10% (x1.1) | 50 | 50 |
| 2 | +20% (x1.2) | 65 | 115 |
| 3 | +30% (x1.3) | 85 | 200 |
| 4 | +40% (x1.4) | 110 | 310 |
| 5 | +50% (x1.5) | 143 | 453 |
| 10 | +100% (x2.0) | 688 | 2,580 |
| 15 | +150% (x2.5) | 3,320 | 11,046 |
| 20 (MAX) | +200% (x3.0) | 16,024 | 50,252 |

> 비용 공식: 50 * 1.3^(Lv-1)

#### 공격속도 강화 (atk_spd) - **수치 조정 필요**

**현재 SO**: baseCost=40, growthRate=1.25, valuePerLevel=0.05, maxLevel=15

**GDD 수치**: baseCost=80, growthRate=1.3

| 레벨 | 효과 (누적) | 현재 비용 | GDD 권장 비용 |
|------|-----------|---------|-------------|
| 1 | +5% (x1.05) | 40 | 80 |
| 2 | +10% (x1.10) | 50 | 104 |
| 3 | +15% (x1.15) | 63 | 135 |
| 5 | +25% (x1.25) | 98 | 228 |
| 10 | +50% (x1.50) | 298 | 1,098 |
| 15 (MAX) | +75% (x1.75) | 906 | 5,303 |

> 현재 비용이 GDD의 ~50%에 불과하여 공격속도 올인 빌드가 지나치게 쉬움.

#### 기지 HP 강화 (base_hp) - **수치 조정 필요**

**현재 SO**: baseCost=30, growthRate=1.2, valuePerLevel=5, maxLevel=20

**GDD 수치**: baseCost=40, growthRate=1.25

| 레벨 | 효과 (누적) | 현재 비용 | GDD 권장 비용 |
|------|-----------|---------|-------------|
| 1 | +5 HP | 30 | 40 |
| 2 | +10 HP | 36 | 50 |
| 3 | +15 HP | 43 | 63 |
| 5 | +25 HP | 62 | 98 |
| 10 | +50 HP | 155 | 373 |
| 20 (MAX) | +100 HP | 959 | 3,471 |

### 5.2 추가 스킬 제안 (프로그래밍팀장 요청 사항)

아래 스킬들은 GDD에 정의되어 있으나 아직 SO/코드 미구현. 우선순위 순서로 나열.

#### 사거리 강화 (range_up) - 우선순위 1

| 속성 | 값 | 비고 |
|------|-----|------|
| skillId | `range_up` | |
| effectType | **신규 추가 필요**: `TowerRange` | SkillEffectType enum 확장 |
| valuePerLevel | 0.2 | 사거리 +0.2 유닛/레벨 |
| maxLevel | 10 | 최대 +2.0 사거리 |
| baseCost | 60 | |
| growthRate | 1.3 | |

#### Bit 획득량 증가 (bit_gain) - 우선순위 2

| 속성 | 값 | 비고 |
|------|-----|------|
| skillId | `bit_gain` | |
| effectType | **신규 추가 필요**: `BitGain` | SkillEffectType enum 확장 |
| valuePerLevel | 0.15 | Bit +15%/레벨 |
| maxLevel | 15 | 최대 +225% |
| baseCost | 100 | |
| growthRate | 1.35 | |

#### 시작 Bit (start_bit) - 우선순위 3

| 속성 | 값 | 비고 |
|------|-----|------|
| skillId | `start_bit` | |
| effectType | **신규 추가 필요**: `StartBit` | SkillEffectType enum 확장 |
| valuePerLevel | 30 | 런 시작 시 Bit +30/레벨 |
| maxLevel | 10 | 최대 +300 시작 Bit |
| baseCost | 120 | |
| growthRate | 1.4 | |

### 5.3 RunModifiers 확장 필요

현재 `RunModifiers`에 포함된 필드:
- `attackDamageMultiplier` (float)
- `attackSpeedMultiplier` (float)
- `bonusBaseHp` (int)

**추가 필요 필드**:
- `bonusRange` (float) - 사거리 보너스
- `bitGainMultiplier` (float) - Bit 획득 배율
- `startBit` (int) - 시작 Bit

---

## 6. 웨이브 구성표

### 6.1 Stage 1: Data Stream (5 웨이브)

**설계 목표**: 기본 메카닉 학습. 영구 업그레이드 0에서 웨이브 2~3에 죽음. ~15회 런으로 클리어.

| 웨이브 | Node 구성 | 수량 | 스폰 간격(초) | 대기(초) | 총 Bit (처치 시) | 설계 의도 |
|--------|----------|------|-------------|---------|-----------------|----------|
| 1 | Bit Node | 5 | 1.2 | 2 | 15 | 기본 학습. 타워가 잡는 걸 관찰 |
| 2 | Bit Node | 8 | 1.0 | 3 | 24 | 수량 증가. 기지 HP 감소 시작 |
| 3 | Bit Node x6 + Quick Node x3 | 9 | 0.8 | 3 | 30 | 빠른 적 등장. 긴장 |
| 4 | Bit Node x10 + Quick Node x5 | 15 | 0.7 | 3 | 50 | 물량 압박 |
| 5 | Bit Node x8 + Quick Node x4 + Heavy Node x2 | 14 | 0.6 | 3 | 56 | 최종 웨이브 |

**총 Bit (전 웨이브 클리어 시)**: 175 Bit
**예상 첫 죽음 웨이브**: 2~3 (영구 업그레이드 0)
**예상 첫 죽음 Bit 획득**: 30~50 Bit (영구 업그레이드 ~1개 구매 가능)

### 6.2 Stage 2: Memory Block (7 웨이브)

**설계 목표**: Cannon 해금 직후. AoE의 필요성을 체감. 물량이 증가.

| 웨이브 | Node 구성 | 수량 | 스폰 간격(초) | 대기(초) |
|--------|----------|------|-------------|---------|
| 1 | Bit Node x8 | 8 | 1.0 | 2 |
| 2 | Bit Node x10 + Quick Node x3 | 13 | 0.9 | 3 |
| 3 | Quick Node x8 | 8 | 0.7 | 3 |
| 4 | Bit Node x12 + Quick Node x6 | 18 | 0.7 | 3 |
| 5 | Bit Node x8 + Heavy Node x4 | 12 | 0.8 | 3 |
| 6 | Bit Node x15 + Quick Node x8 | 23 | 0.5 | 3 |
| 7 | Bit Node x10 + Quick Node x5 + Heavy Node x3 | 18 | 0.5 | 3 |

### 6.3 Stage 3: Cache Layer (8 웨이브)

**설계 목표**: Shield Node 등장. 고정 방어력에 의해 약한 공격이 무효화됨 -> 고데미지 필요성.

| 웨이브 | Node 구성 | 수량 | 스폰 간격(초) | 대기(초) |
|--------|----------|------|-------------|---------|
| 1 | Bit Node x10 | 10 | 0.9 | 2 |
| 2 | Bit Node x8 + Quick Node x5 | 13 | 0.8 | 3 |
| 3 | Shield Node x3 + Bit Node x5 | 8 | 1.0 | 3 |
| 4 | Quick Node x10 + Shield Node x2 | 12 | 0.6 | 3 |
| 5 | Bit Node x15 + Shield Node x5 | 20 | 0.6 | 3 |
| 6 | Heavy Node x5 + Shield Node x3 | 8 | 1.0 | 3 |
| 7 | Bit Node x12 + Quick Node x8 + Shield Node x4 | 24 | 0.5 | 3 |
| 8 | All types (Bit x10 + Quick x6 + Heavy x3 + Shield x4) | 23 | 0.5 | 3 |

### 6.4 Stage 4~10 기본 설계 (스케일링 원칙)

| 스테이지 | 웨이브 수 | 신규 Node | 핵심 챌린지 | 핵심 타워 |
|---------|---------|----------|-----------|----------|
| 4 | 10 | Swarm Node | 물량 + 다경로 | Cannon 필수 |
| 5 | 10 | Regen Node | 빠른 처치 vs 회복 | Lightning 해금 |
| 6 | 12 | Phase Node | 타이밍 + 물량 | Lightning + Laser |
| 7 | 12 | Split Node | 분열 관리 | AoE 조합 |
| 8 | 15 | (복합) | 모든 특수 Node 혼합 | 전략 최적화 |
| 9 | 15 | (복합+강화) | 극한 스케일링 | Void 해금 |
| 10 | 20 | Boss Node(강화) | 최종 도전 | 풀빌드 |

---

## 7. 경제 밸런스

### 7.1 Bit 경제 사이클 (Stage 1 기준)

#### 수입 분석

| 상황 | Bit 수입/런 | 비고 |
|------|-----------|------|
| 첫 런 (W1~2에서 사망) | 30~50 | 업그레이드 ~1개 |
| 3~5회차 (W3에서 사망) | 60~80 | 업그레이드 1개 |
| 10회차 (W4에서 사망) | 90~120 | 업그레이드 1~2개 |
| 15~20회차 (클리어) | 175 + Core 2 | 대량 업그레이드 |

#### 지출 분석

| 항목 | 비용 | 시점 |
|------|------|------|
| 인게임 Arrow 추가 배치 | 30 Bit | 런 중 |
| 공격력 Lv1 | 50 Bit | 첫 허브 복귀 |
| 공격력 Lv2 | 65 Bit | 2~3회차 |
| 공격속도 Lv1 | 80 Bit (GDD 수치) | 4~5회차 |
| 기지HP Lv1 | 40 Bit | 2~3회차 |

#### 성장 곡선 (예상)

| 회차 | 누적 Bit 획득 | 업그레이드 상태 | Arrow DPS (보정) |
|------|-------------|---------------|-----------------|
| 1 | ~40 | 없음 | 10.0 |
| 3 | ~150 | 공격력 Lv1, 기지HP Lv1 | 11.0 |
| 5 | ~300 | 공격력 Lv2, 기지HP Lv2, 공속 Lv1 | 12.6 |
| 10 | ~700 | 공격력 Lv4, 기지HP Lv3, 공속 Lv2 | 15.4 |
| 15 | ~1,200 | 공격력 Lv5, 기지HP Lv4, 공속 Lv3 | 17.3 |
| 20 | ~1,800 | 공격력 Lv7, 기지HP Lv5, 공속 Lv4 | 20.0 (St.1 클리어 가능) |

### 7.2 Core 경제

| 시점 | 누적 Core | 주요 구매 |
|------|----------|----------|
| St.1 첫 클리어 | 2 | 배속 해금(2) 또는 크리티컬(2) |
| St.2 첫 클리어 | 5 | Cannon 해금(3) |
| St.3 첫 클리어 | 9 | Ice 해금(5), 여유 1 |
| St.5 첫 클리어 | 20 | Lightning 해금(8) |
| St.7 첫 클리어 | 36 | Laser 해금(12) |
| St.9 첫 클리어 | 61 | Void 해금(18) |

**Core 부족 검증**: St.1~9 첫 클리어만으로 총 61 Core. 모든 타워 해금에 필요한 Core = 3+5+8+12+18 = 46. 나머지 15 Core로 크리티컬(2), 배속(2), Idle(3), 타워슬롯(6) = 13 Core. **딱 맞으며 약간 여유**. 재클리어 30% 보상이 부족분을 메울 수 있어 적절.

### 7.3 Idle Bit 경제

| 항목 | 값 |
|------|-----|
| 기본 생산 | 100 Bit/시간 |
| 최대 저장 | 2시간분 (200 Bit) |
| 업그레이드 시 최대 | 8시간분 (800 Bit) |
| 능동 플레이 대비 효율 | ~20% |

> 30분 능동 플레이 = ~500 Bit (Stage 1 반복). 30분 방치 = 50 Bit. 방치가 보조 수단일 뿐이라는 인식 유지.

---

## 8. 플레이 사이클 시뮬레이션

### 8.1 Phase 1: 온보딩 (0~5분, 런 3~5회)

```
런 1: 스테이지 1 진입 -> W1 클리어 -> W2에서 사망 -> Bit ~40
     허브: 공격력 Lv1 구매 (50 Bit)... 부족! -> 기지HP Lv1 구매 (40 Bit) -> 재시도

런 2: W1 쉽게 클리어 -> W2 생존 -> W2~3에서 사망 -> Bit ~60
     허브: 공격력 Lv1 구매 (50 Bit) -> 재시도

런 3: DPS 11.0 -> W3까지 안정 -> W3에서 사망 -> Bit ~70
     허브: 기지HP Lv2 또는 공속 Lv1 선택 -> 재시도
```

**핵심 체감**: "매번 확실히 더 오래 버틴다" -> 성장 도파민 확인

### 8.2 Phase 2: 성장 가속 (5~15분, 런 5~10회)

```
런 5~10: W3~4까지 안정 도달. 인게임에서 Arrow 추가 배치 가능.
         Bit 80~120/런. 여러 업그레이드 누적.
         "스테이지 1 클리어가 가까워지는" 느낌.
```

### 8.3 Phase 3: 첫 클리어 (~20~30분, 런 15~20회)

```
런 15~20: DPS ~20, 기지HP 35+
          W5 Heavy Node까지 돌파. Stage 1 클리어!
          Core 2 획득 -> 크리티컬 또는 배속 해금
          Stage 2 해금 -> "새로운 도전!"
```

### 8.4 Phase 4: 타워 해금과 폭발적 성장 (~30분~2시간)

```
Stage 2~3 반복: Cannon, Ice 해금. AoE + 감속 조합 발견.
                화면이 이펙트로 화려해짐.
                Bit 드롭 배율 증가로 업그레이드 가속.
```

### 8.5 Phase 5: 콘텐츠 소화 (~2~4시간)

```
Stage 4~8: Lightning, Laser 해금. 타워 조합 전략 다양화.
           스케일링으로 숫자가 커지는 쾌감.
```

### 8.6 Phase 6: 엔딩 (~4~5시간)

```
Stage 9~10: Void 해금. 풀빌드. 최종 보스 도전.
            모든 Node 타입 혼합. 최대 긴장감.
            클리어 -> 엔딩!
```

---

## 9. 즉시 적용 가능한 SO 수치 변경 제안

### 9.1 Node_Bit.asset 수정 (긴급)

| 필드 | 현재값 | 권장값 | 이유 |
|------|-------|-------|------|
| hp | 30 | **20** | GDD 기준 복원. 첫 런에서 타워가 잡는 쾌감 |
| speed | 2.5 | **1.5** | GDD 기준 복원. 너무 빠르면 첫 런이 답 없음 |
| bitDrop | 8 | **3** | GDD 기준 복원. 경제 밸런스 정상화 |

### 9.2 Tower_Arrow.asset 수정

| 필드 | 현재값 | 권장값 | 이유 |
|------|-------|-------|------|
| damage | [10,15,22,33,50] | **[8,12,18,26,38]** | GDD 기준 (5번째는 미사용이지만 Lv5 대비) |
| attackSpeed | [1,1.1,1.2,1.35,1.5] | **[1.25,1.43,1.67,2.0,2.5]** | GDD 공격간격 -> 초당 횟수 변환 |
| placeCost | 50 | **30** | GDD 기준. 첫 타워 추가 배치를 더 빨리 |

### 9.3 Skill_AttackSpeed.asset 수정

| 필드 | 현재값 | 권장값 | 이유 |
|------|-------|-------|------|
| baseCost | 40 | **80** | GDD 기준 복원 |
| growthRate | 1.25 | **1.3** | GDD 기준 복원 |

### 9.4 Skill_BaseHp.asset 수정

| 필드 | 현재값 | 권장값 | 이유 |
|------|-------|-------|------|
| baseCost | 30 | **40** | GDD 기준 복원 |
| growthRate | 1.2 | **1.25** | GDD 기준 복원 |

### 9.5 Stage_01.asset 수정 (긴급)

| 필드 | 현재값 | 권장값 | 이유 |
|------|-------|-------|------|
| baseHp | 5 | **20** | GDD 기준 복원. HP 5는 즉사 수준으로 성장 경험 불가 |

### 9.6 Wave SO 추가 생성 필요

현재 Wave_01만 존재. **Wave_02 ~ Wave_05 SO 신규 생성** 필요.

Stage_01.waves 배열에 5개 웨이브를 모두 연결해야 함.

---

## 10. 프로그래밍팀장 요청 사항

### 10.1 SO 에셋 수치 변경 (즉시 가능)

위 9절의 수치 변경 작업. SO 에셋 파일만 수정하면 되므로 코드 변경 불필요.

### 10.2 신규 Wave SO 생성

Wave_02 ~ Wave_05 ScriptableObject 생성 및 Stage_01의 waves 배열에 연결.

| Wave | nodeData | count | spawnInterval | delayBeforeWave |
|------|----------|-------|---------------|-----------------|
| Wave_02 | Node_Bit | 8 | 1.0 | 3 |
| Wave_03 | Node_Bit x6, **Node_Quick**(신규 필요) x3 | 9 | 0.8 | 3 |
| Wave_04 | Node_Bit x10, Node_Quick x5 | 15 | 0.7 | 3 |
| Wave_05 | Node_Bit x8, Node_Quick x4, **Node_Heavy**(신규 필요) x2 | 14 | 0.6 | 3 |

### 10.3 신규 NodeData SO 생성

| SO 파일 | nodeId | type | hp | speed | bitDrop | damage |
|---------|--------|------|-----|-------|---------|--------|
| Node_Quick | quick | Quick | 12 | 3.0 | 4 | 1 |
| Node_Heavy | heavy | Heavy | 80 | 0.8 | 8 | 2 |

> Node_Quick과 Node_Heavy는 프리팹도 필요. 최소한 Node_Bit 프리팹을 복제하고 색상/크기만 변경하면 프로토타입 단계에서 충분.

### 10.4 SkillEffectType enum 확장 (P1)

```csharp
public enum SkillEffectType
{
    AttackDamage,
    AttackSpeed,
    BaseHp,
    TowerRange,     // 신규
    BitGain,        // 신규
    StartBit        // 신규
}
```

### 10.5 RunModifiers 확장 (P1)

```csharp
public struct RunModifiers
{
    public float attackDamageMultiplier;
    public float attackSpeedMultiplier;
    public int bonusBaseHp;
    public float bonusRange;          // 신규
    public float bitGainMultiplier;   // 신규
    public int startBit;              // 신규
}
```

### 10.6 MetaManager.CalculateModifiers() 확장 (P1)

신규 SkillEffectType에 대한 switch case 추가.

### 10.7 Node.cs의 Bit 드롭에 bitGainMultiplier 적용 (P1)

```csharp
// Node.Die()에서:
int finalBitDrop = Mathf.RoundToInt(_scaledBitDrop * RunManager.Instance.CurrentModifiers.bitGainMultiplier);
RunManager.Instance.AddBit(finalBitDrop);
```

### 10.8 Tower.cs의 사거리에 bonusRange 적용 (P1)

```csharp
// Tower.FindTarget()에서:
float range = data.GetRange(Level) + RunManager.Instance.CurrentModifiers.bonusRange;
```

### 10.9 TowerData.damage 배열 크기 관련

현재 코드에서 `damage`, `attackSpeed`, `range` 배열이 5개 원소이나 Tower.LevelUp()은 최대 Lv4까지만 허용. GDD는 Lv5까지 설계. **Lv5까지 지원하려면 TryMerge/LevelUp의 `Level >= 4` 조건을 `Level >= 5`로 변경 필요**. 단, 현재 프로토타입에서는 Lv4까지로 충분.

---

## 11. 우선순위 정리

### P0 (이번 주 - 프로토타입 밸런스 정상화)

1. **Node_Bit 수치 GDD 복원** (hp:20, speed:1.5, bitDrop:3)
2. **Tower_Arrow 수치 GDD 복원** (damage, attackSpeed 변환, placeCost:30)
3. **Stage_01 baseHp GDD 복원** (20)
4. **Wave_02~05 SO 생성 + Stage_01 연결**
5. **Skill_AttackSpeed, Skill_BaseHp 비용 GDD 복원**

### P1 (다음 주 - Stage 1 완전한 경험)

1. Node_Quick, Node_Heavy SO + 프리팹 생성
2. SkillEffectType / RunModifiers 확장 (사거리, Bit획득, 시작Bit)
3. 신규 스킬 SO 3종 생성 (range_up, bit_gain, start_bit)
4. Stage_02 데이터 세팅 (7 웨이브)
5. 첫 3회 런 Bit 보너스(+50%) 시스템 (GDD 리스크 완화책)

### P2 (이후 - 콘텐츠 확장)

1. Cannon, Ice Tower SO + 프리팹 + 코드
2. Shield, Swarm Node SO + 프리팹 + 특수 능력 코드
3. Stage_03~05 데이터 세팅
4. Core 스킬 노드 (타워 해금, 크리티컬 등)
5. 스테이지별 스케일링 검증 (자동화 시뮬레이션)

---

## 부록: 밸런스 시뮬레이션 공식 참조

### Arrow DPS 공식

```
base_damage = TowerData.damage[level-1]
attack_per_sec = TowerData.attackSpeed[level-1]
damage_multiplier = 1 + (SkillAttackDamage.valuePerLevel * skill_level)
speed_multiplier = 1 + (SkillAttackSpeed.valuePerLevel * skill_level)

effective_dps = base_damage * damage_multiplier * attack_per_sec * speed_multiplier
```

### Node 처치 시간 공식

```
node_hp = NodeData.hp * StageData.hpMultiplier
kill_time = node_hp / effective_dps
```

### 웨이브 통과율 공식 (대략)

```
path_time = 맵거리 / (NodeData.speed * StageData.speedMultiplier)
nodes_killed = path_time / kill_time  (단일 타워)
nodes_passed = wave_total - nodes_killed
base_damage_taken = nodes_passed * NodeData.damage
```

---

*이 문서는 프로토타입 단계의 초기 밸런싱 지침이며, 에디터 플레이테스트 결과에 따라 지속 업데이트됩니다.*
