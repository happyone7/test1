# Soulspire - 인게임 전투 시스템 기획서

| 항목 | 내용 |
|------|------|
| 문서 버전 | 0.2 |
| 최종 수정 | 2026-02-15 |
| 작성자 | 기획팀장 (Game Designer) |
| 기준 문서 | `GDD.md` v2.0, `ArtDirection_v0.1.md` v0.1 |
| 대상 독자 | 프로그래밍팀장, QA팀장 |
| 상태 | 총괄PD 피드백 반영 완료 |

---

## 변경 이력

| 날짜 | 버전 | 내용 |
|------|------|------|
| 2026-02-14 | 0.1 | 초안 작성 |
| 2026-02-15 | 0.2 | 총괄PD 피드백 8건 반영: 합성 규칙(같은 Lv끼리, 최대 Lv4), 타워 판매 불가, 인벤토리 초과 시 상자 열기 불가, 보물상자 시스템 추가, 일시정지 중 배치 불가, 첫 런 Arrow 정가운데, 분열체 재분열 없음, Floor간 독립 배치 확정. 게임명 Soulspire로 변경. |

---

## 1. 런 진입~종료 전체 플로우

### 1.1 상태 전환 다이어그램

```
[Sanctum] --출격 버튼--> [InGame: 준비] --> [InGame: 전투] --기지 HP 0--> [RunEnd: 패배]
                                                          |                                |
                                                          +--최종 Incursion 클리어--> [RunEnd: 승리]
                                                                                           |
                                                      [Sanctum] <--Sanctum 버튼-- [RunEnd] --재도전 버튼--> [InGame: 준비]
```

### 1.2 런 진입 흐름

| 단계 | 트리거 | 동작 | UI 변화 |
|------|--------|------|---------|
| 1 | Hub에서 "출격" 버튼 클릭 | `GameManager.StartRun(stageIndex)` 호출 | Sanctum UI 페이드아웃 |
| 2 | `GameManager.State = InGame` | `HubUI.Hide()` | InGame UI 페이드인 |
| 3 | `RunManager.StartRun()` | 기지 HP 초기화, BitEarned=0, Modifiers 적용 | 상단 HUD 표시 (Incursion/Bit/HP) |
| 4 | 기존 배치 타워 복원 | SaveData에서 배치 정보 로드, 타워 재생성 | 맵에 타워 표시 |
| 5 | `WaveSpawner.StartWaves()` | Incursion 1 딜레이(3초) 후 스폰 시작 | "Incursion 1" 배너 표시 |
| 6 | 전투 진행 | Node 스폰, 타워 자동 공격 | 하단 타워 인벤토리 활성 |

**첫 런 특수 처리 (FTUE)**:
- 게임 최초 시작 시 Hub를 스킵하고 바로 Floor 1 진입 (GDD 2.1 설계 의도)
- Arrow Tower 1기가 **맵 정가운데** 배치 가능 칸에 자동 배치된 상태로 시작 (총괄PD 확정)

### 1.3 런 중 상태 전환

```
[Incursion N 스폰] --> [모든 Node 처리됨 (aliveCount == 0)]
                         |
                    N < 최종 Incursion? --예--> [3초 대기] --> [Incursion N+1 스폰]
                         |
                        아니오
                         |
                    [Floor 클리어]
```

### 1.4 런 종료 조건 및 결과 처리

#### 패배 (기지 HP 0)

| 단계 | 동작 | 상세 |
|------|------|------|
| 1 | `RunManager.TakeDamage()` -> HP 0 | `EndRun(false)` 호출 |
| 2 | 스폰 중지 | `WaveSpawner.StopSpawning()` |
| 3 | 남은 Node 제거 | 0.5초 동안 남은 Node를 순차적으로 페이드아웃 (연출) |
| 4 | 보상 정산 | `MetaManager.AddRunRewards(bitEarned, 0, false, stageIdx)` |
| 5 | UI 표시 | RunEnd 패널 슬라이드업 (0.3초) |
| 6 | 타워 배치 저장 | 현재 배치 상태를 SaveData에 기록 |

#### 승리 (Floor 클리어)

| 단계 | 동작 | 상세 |
|------|------|------|
| 1 | 최종 Incursion의 마지막 Node 처리 | `RunManager.OnWaveCompleted()` -> 모든 Incursion 완료 |
| 2 | `EndRun(true)` 호출 | |
| 3 | 클리어 연출 | 화면 밝기 상승 + 금빛 파티클 1.5초 |
| 4 | 보상 정산 | `MetaManager.AddRunRewards(bitEarned, coreReward, true, stageIdx)` |
| 5 | UI 표시 | "STAGE CLEAR!" 배너 -> RunEnd 패널 (Core Fragment 포함) |
| 6 | 타워 배치 저장 | Floor별 현재 배치 상태를 SaveData에 기록 |
| 7 | 다음 Floor 해금 | `currentStageIndex` 증가 |

#### RunEnd 패널 버튼 동작

| 버튼 | 패배 시 | 승리 시 |
|------|---------|---------|
| Sanctum 버튼 | `GoToHub()` -> Sanctum UI 표시 | `GoToHub()` -> Sanctum UI 표시 |
| 재도전/다음 | `StartRun(같은 Floor)` | `StartRun(다음 Floor)` |

---

## 2. 타워 배치/합성 시스템 상세

### 2.1 배치 가능 영역 정의

**격자 기반 배치** (타일맵 연동)

```
맵 구성:
+---+---+---+---+---+---+---+---+
| W | W | W | W | W | W | W | W |   W = 벽/장애물 (배치 불가)
+---+---+---+---+---+---+---+---+
| W | P | P | P | P | P | P | W |   P = 경로 (배치 불가, Node 이동)
+---+---+---+---+---+---+---+---+
| W | T | T | P | T | T | P | W |   T = 배치 가능 칸
+---+---+---+---+---+---+---+---+
| W | P | P | P | P | P | P | W |
+---+---+---+---+---+---+---+---+
| W | T | P | T | T | P | T | W |
+---+---+---+---+---+---+---+---+
| W | W | W | W | W | W | W | W |
+---+---+---+---+---+---+---+---+
```

**구현 명세:**

| 항목 | 값 |
|------|-----|
| 타일 크기 | 1x1 Unity Unit (= 32x32 px, PPU 32) |
| 배치 칸 판정 | Tilemap 레이어에 "Placement" 타일 여부 |
| 배치 불가 판정 | 경로 타일, 벽 타일, 이미 타워가 배치된 칸 |
| 초기 배치 칸 수 | 12칸 (Floor 1 기준) |
| 최대 배치 칸 수 | 스킬 트리 "타워 슬롯 확장" 스킬로 +4칸/회 (최대 3회 = +12칸) |
| 스냅 | 드래그 중 가장 가까운 격자 중심으로 스냅 |

**타일맵 레이어 구성:**

```csharp
// 타일맵에서 배치 가능 여부 판정
public class PlacementGrid : MonoBehaviour
{
    public Tilemap placementTilemap;  // 배치 가능 칸만 있는 Tilemap
    public Tilemap pathTilemap;       // 경로 타일 Tilemap

    public bool CanPlace(Vector3Int cellPos)
    {
        // 1. 배치 가능 타일이 있는가?
        if (!placementTilemap.HasTile(cellPos)) return false;

        // 2. 이미 타워가 있는가?
        Vector3 worldPos = placementTilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);
        Collider2D existing = Physics2D.OverlapPoint(worldPos, towerLayer);
        if (existing != null) return false;

        return true;
    }

    public Vector3 SnapToGrid(Vector3 worldPos)
    {
        Vector3Int cell = placementTilemap.WorldToCell(worldPos);
        return placementTilemap.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0);
    }
}
```

### 2.2 타워 배치 UX (드래그)

**신규 배치 흐름:**

```
1. 플레이어가 하단 인벤토리의 타워 아이콘을 터치/클릭
2. 아이콘이 손가락/커서를 따라가며 "드래그 중" 상태 진입
3. 맵 위를 이동하면:
   - 배치 가능한 빈 칸 위: 초록색 하이라이트 (배치 가능)
   - 같은 종류 타워 위: 금색 하이라이트 + "LV UP!" 텍스트 (합성 가능)
   - 경로/벽/다른 종류 타워 위: 빨간색 하이라이트 (배치 불가)
4. 손을 떼면:
   - 배치 가능 칸: 즉시 배치 (확인 팝업 없음). 배치 사운드 재생.
   - 합성 가능 타워 위: 합성 실행
   - 배치 불가 위치: 인벤토리로 복귀 (타워 소비 안 됨)
```

**의사코드 (TowerDragController):**

```csharp
public class TowerDragController : MonoBehaviour
{
    // 상태
    enum DragState { None, Dragging }
    DragState _state = DragState.None;
    TowerData _draggedTowerData;
    int _draggedTowerLevel;         // 드래그 중인 타워의 레벨
    int _draggedInventoryIndex;
    GameObject _dragPreview;        // 반투명 프리뷰
    GameObject _gridHighlight;      // 격자 하이라이트

    void Update()
    {
        if (_state == DragState.Dragging)
        {
            // 커서/터치 위치를 월드 좌표로 변환
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;

            // 프리뷰를 커서 위치로
            _dragPreview.transform.position = worldPos;

            // 격자 스냅 위치 계산
            Vector3 snappedPos = _placementGrid.SnapToGrid(worldPos);
            Vector3Int cellPos = _placementGrid.WorldToCell(worldPos);

            // 하이라이트 업데이트
            UpdateHighlight(cellPos, snappedPos);

            // 드롭 (마우스 버튼 릴리즈)
            if (Input.GetMouseButtonUp(0))
            {
                TryDrop(cellPos, snappedPos);
            }
        }
    }

    void UpdateHighlight(Vector3Int cellPos, Vector3 snappedPos)
    {
        // 같은 종류 타워가 있는지 확인
        Tower existingTower = GetTowerAt(snappedPos);

        if (existingTower != null && existingTower.data.type == _draggedTowerData.type
            && existingTower.Level == _draggedTowerLevel
            && existingTower.Level < 4)
        {
            // 합성 가능: 금색
            SetHighlight(snappedPos, HighlightType.Merge);
        }
        else if (_placementGrid.CanPlace(cellPos))
        {
            // 배치 가능: 초록색
            SetHighlight(snappedPos, HighlightType.Place);
        }
        else
        {
            // 배치 불가: 빨간색
            SetHighlight(snappedPos, HighlightType.Invalid);
        }
    }

    void TryDrop(Vector3Int cellPos, Vector3 snappedPos)
    {
        Tower existingTower = GetTowerAt(snappedPos);

        if (existingTower != null && existingTower.data.type == _draggedTowerData.type
            && existingTower.Level == _draggedTowerLevel
            && existingTower.Level < 4)
        {
            // 합성 (같은 레벨끼리만 가능, 최대 Lv4)
            existingTower.LevelUp();  // Level++
            ConsumeFromInventory(_draggedInventoryIndex);
            PlayMergeEffect(snappedPos, existingTower.Level);
        }
        else if (_placementGrid.CanPlace(cellPos))
        {
            // 신규 배치
            TowerManager.Instance.PlaceTower(_draggedTowerData, snappedPos);
            ConsumeFromInventory(_draggedInventoryIndex);
            PlayPlaceEffect(snappedPos);
        }
        // else: 배치 불가 -> 인벤토리로 복귀 (아무 처리 안 함)

        EndDrag();
    }
}
```

### 2.3 합성 UX 상세

| 항목 | 명세 |
|------|------|
| 합성 조건 | 인벤토리 타워를 배치된 **같은 타입 + 같은 레벨** 타워 위에 드롭 |
| 레벨 규칙 | 같은 레벨끼리만 합성 가능. 합성 시 Level + 1. (예: Lv1+Lv1=Lv2, Lv2+Lv2=Lv3) |
| 최대 레벨 | **4** (Lv4 타워에는 합성 불가, 금색 하이라이트 대신 빨간색 표시) |
| 합성 비용 | **없음** (타워 1기 소모가 유일한 비용) |
| 소모 | 인벤토리에서 소스 타워 1개 소모 |
| 이펙트 | 합성 성공 시 대상 타워 위치에 발광 이펙트 + "Lv N!" 텍스트 팝업 |
| 스탯 변화 표시 | 합성 직후 1.5초간 타워 위에 "DMG 12->18, SPD 0.7->0.6" 형태 팝업 |

**합성 이펙트 스케일링:**

| 합성 결과 | 이펙트 강도 |
|-----------|-----------|
| Lv1 -> Lv2 | 작은 빛남 + "+1" |
| Lv2 -> Lv3 | 중간 빛남 + 짧은 파티클 |
| Lv3 -> Lv4 | 화면 미세 흔들림 + 전용 발광 오라 + "MAX!" 텍스트 |

### 2.4 타워 판매

**타워 판매 불가** (총괄PD 확정)

배치된 타워는 판매/제거할 수 없다. 한번 배치하면 영구적으로 해당 칸을 점유한다.

설계 의도:
- 배치 결정에 무게감 부여 (신중한 위치 선택 유도)
- 합성이 타워 관리의 유일한 수단 (합성 비용도 없으므로, 같은 타입 + 같은 레벨 타워를 올려 레벨업하는 것이 핵심)
- 경제 시스템 단순화 (환불 계산 불필요)

### 2.5 배치 유지 규칙

**핵심 규칙: 런이 종료되어도(죽어도) 배치된 타워는 유지된다.**

이것은 게임의 핵심 메카닉. "성장이 누적된다"는 느낌의 핵심 중 하나.

**SaveData 확장:**

```csharp
[Serializable]
public class PlayerSaveData
{
    // 기존 필드
    public int totalBit;
    public int totalCore;
    public int currentStageIndex;
    public List<SkillLevelEntry> skillLevels;

    // 신규: 타워 배치 데이터
    public List<PlacedTowerEntry> placedTowers = new List<PlacedTowerEntry>();
}

[Serializable]
public struct PlacedTowerEntry
{
    public string towerId;      // TowerData.towerId 참조
    public int level;           // 1~4
    public int stageIndex;      // 어느 Floor에 배치된 타워인가
    public float posX;          // 배치 위치 X (월드 좌표)
    public float posY;          // 배치 위치 Y
}
```

**저장/복원 타이밍:**

| 시점 | 동작 |
|------|------|
| 런 종료 (패배/승리) | 현재 배치 상태를 SaveData에 기록 |
| 런 시작 | SaveData에서 해당 Floor의 배치 정보 로드 -> 타워 재생성 |
| 타워 배치/합성 시 | 즉시 SaveData 업데이트 (런 중 크래시 대비) |

**Floor별 독립 배치:**

각 Floor는 독립적인 배치 레이아웃을 가진다. Floor 1에 배치한 타워와 Floor 2에 배치한 타워는 별개.

### 2.6 타워 인벤토리 관리

**인벤토리 = 미배치 타워 목록**

| 항목 | 명세 |
|------|------|
| 최대 슬롯 | 8칸 (하단 바에 표시) |
| 초과 시 | **보물상자 열기 불가** (인벤토리에 빈 슬롯이 없으면 Hub에서 상자를 열 수 없음) |
| 표시 정보 | 타워 아이콘 + 타워 이름 (작게) + 레벨 표시 |
| 정렬 | 타입별 그룹핑, 같은 타입 내 레벨순 (오름차순) |

**타워 획득 경로:**

| 경로 | 조건 | 획득 |
|------|------|------|
| **보물상자 (Sanctum)** | Hub에서 보물상자 열기 | 3후보지 중 선택 또는 뱃지 획득 (섹션 2.7 참조) |
| 첫 런 자동 지급 | 게임 최초 시작 시 | Arrow Tower 1기 (맵 정가운데 자동 배치) |

> Incursion 클리어 시 타워를 직접 획득하지 않는다. 타워는 오직 Hub의 보물상자 시스템을 통해서만 획득 가능.

### 2.7 보물상자 시스템 (Sanctum)

타워 획득의 유일한 경로. Hub에서 보물상자를 열어 타워 또는 뱃지를 획득한다.

#### 상자 획득 조건

| 획득 경로 | 조건 | 수량 |
|-----------|------|------|
| Floor 클리어 | Floor를 처음 클리어 시 | 2개 |
| Incursion 도달 마일스톤 | 누적 Incursion 클리어 N회 달성 시 | 1개 |
| 런 종료 보상 | 매 런 종료 시 (패배 포함) | 0~1개 (도달 Incursion에 비례) |

> 상자는 Hub에서만 열 수 있다. 인게임 중에는 열 수 없음.

#### 상자 비용

보물상자를 여는 데 **비용은 없다** (무료). 상자 자체가 보상이며, 상자를 획득하는 것이 곧 진행 보상.

#### 상자 열기 흐름

```
[Sanctum 화면] -- 보물상자 아이콘(보유 수량 표시) 클릭 -->

[인벤토리 빈 슬롯 확인]
   |
   빈 슬롯 있음 --> [상자 오픈 연출 (0.8초)]
   |                    |
   |                    v
   |              [3후보지 표시] 또는 [뱃지 표시]
   |                    |
   |                    v
   |              [플레이어가 1개 선택]
   |                    |
   |                    v
   |              [획득 연출 + 인벤토리 추가]
   |
   빈 슬롯 없음 --> [안내 메시지: "인벤토리가 가득 찼습니다. 타워를 배치하거나 합성한 후 다시 시도하세요."]
                    [상자는 소모되지 않음]
```

#### 상자 내용물 확률

| 내용물 | 확률 | 설명 |
|--------|------|------|
| 타워 3후보지 | 75% | 해금된 타워 풀에서 랜덤 3종 표시, 1개 선택 |
| 뱃지 | 25% | 타워 능력치 상승 뱃지 1개 획득 |

#### 타워 3후보지 규칙

| 항목 | 규칙 |
|------|------|
| 후보 풀 | 플레이어가 **해금한 타워 타입**만 등장 |
| 후보 레벨 | 모두 **Lv1** (레벨업은 합성으로만) |
| 중복 | 같은 타입이 2개 이상 나올 수 있음 (확률 기반) |
| 가중치 | 기본 타워(Arrow, Cannon)는 가중치 높음, 고급 타워(Laser, Void)는 낮음 |
| 선택 | 3개 중 반드시 1개를 선택해야 함 (건너뛰기 불가) |

**타워 등장 가중치:**

| 타워 | 가중치 | 비고 |
|------|--------|------|
| Arrow | 30 | 기본, 흔함 |
| Cannon | 25 | 흔함 |
| Ice | 20 | 보통 |
| Lightning | 15 | 약간 희귀 |
| Laser | 7 | 희귀 |
| Void | 3 | 매우 희귀 |

> 해금되지 않은 타워는 풀에서 제외된다. 해금된 타워의 가중치 합으로 정규화하여 확률 계산.

#### 뱃지 시스템

뱃지는 **특정 타워 타입**의 능력치를 영구적으로 강화하는 아이템.

| 항목 | 명세 |
|------|------|
| 적용 범위 | 특정 타워 타입 1종 (예: "Arrow 공격력 뱃지") |
| 지속 | **영구** (런을 넘어서도 유지) |
| 중첩 | 같은 뱃지 중첩 가능 (최대 5중첩) |
| 인벤토리 | 뱃지는 타워 인벤토리 슬롯을 차지하지 않음 (별도 뱃지 목록) |

**뱃지 종류:**

| 뱃지 | 효과 | 중첩당 |
|------|------|--------|
| 공격력 뱃지 | 해당 타워 타입의 공격력 +10% | +10% |
| 공격속도 뱃지 | 해당 타워 타입의 공격속도 +8% | +8% |
| 사거리 뱃지 | 해당 타워 타입의 사거리 +0.3 | +0.3 |
| 특수능력 뱃지 | 해당 타워 타입의 특수 수치 강화 | 타워별 상이 |

**뱃지 표시 예시:**

```
상자를 열면 뱃지가 나왔을 때:
+-------------------------------+
|     [뱃지 아이콘]              |
|   "Arrow 공격력 뱃지"         |
|   Arrow Tower 공격력 +10%     |
|                               |
|        [획득] 버튼            |
+-------------------------------+
```

#### 상자 UI/UX 상세

**Hub에서의 상자 표시:**
- Sanctum 화면 하단 또는 우측에 보물상자 아이콘 + 보유 수량 배지
- 보유 수 > 0 이면 아이콘이 반짝이는 애니메이션 (주의 유도)
- 클릭 시 상자 오픈 화면으로 전환

**오픈 연출:**
1. 상자가 화면 중앙에 등장 (0.3초 확대 애니메이션)
2. 상자가 열리며 빛 방사 (0.5초)
3. 내용물 3개가 뒤집힌 카드 형태로 나타남 (타워 3후보지) 또는 뱃지 1개 표시
4. 타워 후보지의 경우: 카드를 하나씩 뒤집으며 어떤 타워인지 공개 (0.3초씩)
5. 플레이어가 원하는 카드 선택
6. 선택한 카드가 확대 -> 인벤토리로 날아가는 연출 (0.5초)

---

## 3. 타워별 특수 능력 명세

### 3.1 공통 Attack 로직 (기존 구현)

```csharp
// 현재 Tower.cs의 Attack() - 모든 타워 공용
void Attack()
{
    float damage = data.GetDamage(Level) * modifiers.attackDamageMultiplier;

    if (data.projectilePrefab != null)
    {
        // 투사체 기반 공격
        var proj = SpawnProjectile();
        proj.Initialize(_currentTarget, damage);
    }
    else
    {
        // 즉발 공격 (projectilePrefab이 null인 경우)
        _currentTarget.TakeDamage(damage);
    }
}
```

### 3.2 T01 - Arrow Tower (기본, 단일 타격)

**동작**: 기존 구현 그대로. 단일 대상에게 투사체 발사.

```
타겟팅: 가장 가까운 적
투사체: 직선 이동, 단일 타격
특수 능력: 없음
```

변경 필요 없음. 현재 코드로 충분.

### 3.3 T02 - Cannon Tower (AoE 폭발)

**동작**: 투사체가 적에게 도달하면 폭발, 범위 내 모든 적에게 데미지.

```csharp
// CannonProjectile : Projectile (파생 클래스)
public class CannonProjectile : Projectile
{
    public float explosionRadius;   // TowerData에서 Lv별 참조
    public float splashDamageRatio = 1.0f;  // 스플래시 영역의 데미지 비율

    override void Hit()
    {
        // 1. 폭발 범위 내 모든 적 검색
        var colliders = Physics2D.OverlapCircleAll(
            transform.position, explosionRadius);

        foreach (var col in colliders)
        {
            var node = col.GetComponent<Node>();
            if (node != null && node.IsAlive)
            {
                // 2. 거리에 따른 데미지 감쇠
                float dist = Vector2.Distance(transform.position, node.transform.position);
                float distRatio = 1f - (dist / explosionRadius);  // 1.0(중심) ~ 0.0(가장자리)
                distRatio = Mathf.Clamp01(distRatio);

                float finalDamage = _damage * (0.5f + 0.5f * distRatio);
                // 중심: 100% 데미지, 가장자리: 50% 데미지

                node.TakeDamage(finalDamage);
            }
        }

        // 3. 폭발 이펙트 스폰
        SpawnExplosionEffect(transform.position, explosionRadius);

        // 4. 카메라 미세 흔들림 (3마리 이상 동시 타격 시)
        if (hitCount >= 3)
            CameraShake.Trigger(0.05f, 0.1f);  // intensity, duration

        Poolable.TryPool(gameObject);
    }
}
```

**Cannon 데미지 감쇠 모델:**

```
               100% 데미지
                  |
          75%     |     75%
            \     |     /
     50% ----[폭발 중심]---- 50%
            /     |     \
          75%     |     75%
                  |
               100% 데미지

공식: finalDamage = baseDamage * (0.5 + 0.5 * (1 - dist/radius))
중심(dist=0): 100%
절반(dist=radius/2): 75%
가장자리(dist=radius): 50%
```

**TowerData 확장 (Cannon 전용):**

```csharp
// TowerData에 추가 필드
[Header("Cannon 전용")]
public float[] explosionRadius = { 1.0f, 1.1f, 1.3f, 1.6f };  // Lv1~4
```

### 3.4 T03 - Ice Tower (감속)

**동작**: 투사체가 적에게 적중하면 데미지 + 감속 디버프 적용.

```csharp
// IceProjectile : Projectile (파생 클래스)
public class IceProjectile : Projectile
{
    public float slowPercent;      // 0.25 ~ 0.65 (Lv별)
    public float slowDuration;     // 1.5 ~ 2.5초 (Lv별)

    override void Hit()
    {
        _target.TakeDamage(_damage);
        _target.ApplySlow(slowPercent, slowDuration);
        SpawnIceHitEffect(transform.position);
        Poolable.TryPool(gameObject);
    }
}

// Node에 추가할 감속 시스템
public partial class Node
{
    float _slowMultiplier = 1f;    // 1.0 = 정상, 0.75 = 25% 감속
    float _slowTimer = 0f;

    public void ApplySlow(float slowPercent, float duration)
    {
        // 더 강한 감속이 우선 (중첩 불가, 덮어쓰기)
        float newMultiplier = 1f - slowPercent;
        if (newMultiplier < _slowMultiplier || _slowTimer <= 0)
        {
            _slowMultiplier = newMultiplier;
            _slowTimer = duration;
        }

        // 시각 피드백: 스프라이트에 파란 틴트
        SetSlowVisual(true);
    }

    void UpdateSlow()  // Update()에서 호출
    {
        if (_slowTimer > 0)
        {
            _slowTimer -= Time.deltaTime;
            if (_slowTimer <= 0)
            {
                _slowMultiplier = 1f;
                SetSlowVisual(false);
            }
        }
    }

    void Move()  // 기존 Move 수정
    {
        float effectiveSpeed = _scaledSpeed * _slowMultiplier;
        transform.position = Vector3.MoveTowards(
            transform.position, target, effectiveSpeed * Time.deltaTime);
    }
}
```

**감속 규칙:**

| 규칙 | 값 |
|------|-----|
| 중첩 | 불가. 가장 강한 감속만 적용. |
| 갱신 | 새 감속이 기존보다 강하면 덮어쓰기 |
| 지속 시간 | 새 감속 적용 시 타이머 리셋 |
| 시각 피드백 | 감속 중인 Node에 파란 틴트 오버레이 |
| 최소 속도 | 원래 속도의 10% (완전 정지 방지) |

### 3.5 T04 - Lightning Tower (체인)

**동작**: 첫 대상을 타격한 후 주변 적에게 연쇄 전이. 체인마다 데미지 감소.

```csharp
// LightningTower : Tower (파생 클래스의 Attack 오버라이드)
// Lightning은 투사체 없이 즉발 공격
public class LightningTower : Tower
{
    public int chainCount;           // 2~6 (Lv별)
    public float chainDamageDecay;   // 0.30~0.10 (Lv별, 체인당 데미지 감소율)
    public float chainRange = 2.0f;  // 체인이 전이되는 최대 거리

    override void Attack()
    {
        float baseDamage = data.GetDamage(Level) * modifiers.attackDamageMultiplier;

        List<Node> hitNodes = new List<Node>();
        Node current = _currentTarget;
        float currentDamage = baseDamage;

        for (int i = 0; i <= chainCount; i++)
        {
            if (current == null || !current.IsAlive) break;

            // 데미지 적용
            current.TakeDamage(currentDamage);
            hitNodes.Add(current);

            // 체인 라인 이펙트 (이전 타겟 -> 현재 타겟)
            if (i > 0)
                SpawnChainEffect(hitNodes[i-1].transform.position, current.transform.position);
            else
                SpawnChainEffect(firePoint.position, current.transform.position);

            // 다음 체인 타겟 검색 (이미 맞은 적 제외)
            currentDamage *= (1f - chainDamageDecay);
            current = FindNextChainTarget(current.transform.position, chainRange, hitNodes);
        }
    }

    Node FindNextChainTarget(Vector3 from, float range, List<Node> exclude)
    {
        var colliders = Physics2D.OverlapCircleAll(from, range);
        Node closest = null;
        float minDist = float.MaxValue;

        foreach (var col in colliders)
        {
            var node = col.GetComponent<Node>();
            if (node != null && node.IsAlive && !exclude.Contains(node))
            {
                float dist = Vector2.Distance(from, node.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = node;
                }
            }
        }
        return closest;
    }
}
```

**체인 데미지 예시 (Lv3, chainCount=4, decay=20%):**

```
1번째 (메인 타겟):  32.0 데미지 (100%)
2번째 (체인 1):     25.6 데미지 (80%)
3번째 (체인 2):     20.5 데미지 (64%)
4번째 (체인 3):     16.4 데미지 (51%)
5번째 (체인 4):     13.1 데미지 (41%)
```

**체인 이펙트**: 번개 선(Line Renderer 또는 파티클) 0.15초간 표시 후 페이드아웃.

### 3.6 T05 - Laser Tower (빔 관통 + 쿨다운)

**동작**: 빔을 발사하여 일직선상의 적에게 초당 데미지. 빔 지속 후 쿨다운.

```csharp
// LaserTower : Tower (파생 클래스)
public class LaserTower : Tower
{
    enum LaserState { Ready, Firing, Cooldown }
    LaserState _laserState = LaserState.Ready;

    float _firingTimer;
    float _cooldownTimer;
    LineRenderer _beamLine;          // 빔 시각화

    public float beamDuration;       // 2.0~3.0초 (Lv별)
    public float cooldownDuration;   // 3.0~2.0초 (Lv별)
    public float dps;                // 25~140 (Lv별)
    public int pierceCount;          // 3~무제한 (Lv별)

    override void Update()
    {
        if (data == null) return;

        switch (_laserState)
        {
            case LaserState.Ready:
                // 타겟이 사거리 내에 있으면 발사 시작
                _currentTarget = FindTarget();
                if (_currentTarget != null)
                {
                    _laserState = LaserState.Firing;
                    _firingTimer = beamDuration;
                    _beamLine.enabled = true;
                }
                break;

            case LaserState.Firing:
                _firingTimer -= Time.deltaTime;

                // 타겟 방향으로 빔 쏘기
                if (_currentTarget != null && _currentTarget.IsAlive)
                {
                    Vector2 dir = (_currentTarget.transform.position - firePoint.position).normalized;
                    float range = data.GetRange(Level);

                    // Raycast로 빔 경로상의 모든 적 검출
                    RaycastHit2D[] hits = Physics2D.RaycastAll(
                        firePoint.position, dir, range);

                    int pierced = 0;
                    foreach (var hit in hits)
                    {
                        var node = hit.collider.GetComponent<Node>();
                        if (node != null && node.IsAlive)
                        {
                            node.TakeDamage(dps * Time.deltaTime * modifiers.attackDamageMultiplier);
                            pierced++;
                            if (pierceCount > 0 && pierced >= pierceCount) break;
                        }
                    }

                    // 빔 시각화 업데이트
                    UpdateBeamVisual(firePoint.position, dir, range);
                }
                else
                {
                    // 타겟 사라지면 새 타겟 검색
                    _currentTarget = FindTarget();
                }

                if (_firingTimer <= 0)
                {
                    _laserState = LaserState.Cooldown;
                    _cooldownTimer = cooldownDuration;
                    _beamLine.enabled = false;
                }
                break;

            case LaserState.Cooldown:
                _cooldownTimer -= Time.deltaTime;
                if (_cooldownTimer <= 0)
                    _laserState = LaserState.Ready;
                break;
        }
    }
}
```

**Laser 상태 사이클:**

```
[Ready] --타겟 발견--> [Firing (2~3초)] --지속 끝--> [Cooldown (3~2초)] --쿨다운 끝--> [Ready]

Firing 중:
- 빔은 타겟 방향 일직선으로 발사
- 경로상 모든 적에게 초당 데미지 적용 (pierceCount 제한)
- 타겟이 죽으면 새 타겟으로 빔 방향 전환
- LineRenderer로 보라색 빔 시각화
```

**빔 시각 피드백:**

| 상태 | 시각 표현 |
|------|----------|
| Ready | 타워 상단에 작은 보라 빛 (충전 완료) |
| Firing | 보라색 빔 + 적중 지점 스파크 파티클 |
| Cooldown | 타워 상단 빛 소멸 -> 점진적 충전 애니메이션 |

### 3.7 T06 - Void Tower (범위 지속 데미지)

**동작**: 자기 주변 범위 내 모든 적에게 지속적으로 데미지. 투사체 없음.

```csharp
// VoidTower : Tower (파생 클래스)
public class VoidTower : Tower
{
    public float aoeRadius;          // 2.0~3.5 (Lv별)
    public float dps;                // 15~100 (Lv별)
    public float tickInterval = 0.25f;  // 0.25초마다 데미지 적용

    float _tickTimer;
    ParticleSystem _aoeEffect;       // 상시 활성화된 범위 이펙트

    // Lv3+: 방어력 무시
    public bool ignoreArmor;         // Lv3부터 true
    // Lv4+: 감속 효과
    public float slowPercent;        // Lv3: 0.10, Lv4: 0.20
    public float slowDuration = 0.5f;

    override void Update()
    {
        if (data == null) return;

        _tickTimer += Time.deltaTime;
        if (_tickTimer >= tickInterval)
        {
            _tickTimer = 0f;
            ApplyAoEDamage();
        }
    }

    void ApplyAoEDamage()
    {
        float damagePerTick = dps * tickInterval * modifiers.attackDamageMultiplier;

        var colliders = Physics2D.OverlapCircleAll(transform.position, aoeRadius);
        foreach (var col in colliders)
        {
            var node = col.GetComponent<Node>();
            if (node != null && node.IsAlive)
            {
                if (ignoreArmor)
                    node.TakeDamage(damagePerTick, ignoreArmor: true);
                else
                    node.TakeDamage(damagePerTick);

                // Lv4+: 감속 적용
                if (slowPercent > 0)
                    node.ApplySlow(slowPercent, slowDuration);
            }
        }
    }
}
```

**Void Tower 범위 시각화:**

```
상시 활성화된 어두운 보라 오라 (파티클 시스템)
범위 내 적이 있을 때: 오라 강도 증가 + 왜곡 이펙트
Lv3+: 오라 색상이 더 짙어짐
Lv4+: 오라에 얼음 입자 추가 (감속 시각 표현)
```

### 3.8 타워 타입별 요약표

| 타워 | Attack 방식 | 투사체 | 타겟팅 | 특수 데이터 필드 |
|------|-----------|--------|--------|----------------|
| Arrow | 투사체 단일 타격 | Projectile (기존) | 가장 가까운 적 | - |
| Cannon | 투사체 -> 폭발 AoE | CannonProjectile (신규) | 가장 가까운 적 | explosionRadius[] |
| Ice | 투사체 + 감속 디버프 | IceProjectile (신규) | 가장 가까운 적 | slowPercent[], slowDuration[] |
| Lightning | 즉발 체인 | 없음 (즉발) | 가장 가까운 적 -> 체인 | chainCount[], chainDamageDecay[], chainRange |
| Laser | 빔 지속 -> 쿨다운 | 없음 (LineRenderer) | 가장 가까운 적 | beamDuration[], cooldownDuration[], dps[], pierceCount[] |
| Void | 범위 지속 데미지 | 없음 (상시 AoE) | 범위 내 전체 | aoeRadius[], dps[], ignoreArmor, slowPercent[] |

**TowerData 확장 제안:**

```csharp
// TowerData.cs에 추가할 필드들
[Header("Cannon 전용")]
public float[] explosionRadius;

[Header("Ice 전용")]
public float[] slowPercent;
public float[] slowDuration;

[Header("Lightning 전용")]
public int[] chainCount;
public float[] chainDamageDecay;
public float chainRange = 2.0f;

[Header("Laser 전용")]
public float[] beamDuration;
public float[] cooldownDuration;
public float[] dps;           // attackSpeed 대신 DPS 직접 사용
public int[] pierceCount;     // -1 = 무제한

[Header("Void 전용")]
public float[] aoeRadius;
// dps는 Laser와 공유 가능
public bool[] ignoreArmor;    // Lv별 방어력 무시 여부
public float[] aoeSlowPercent;
```

---

## 4. Node 특수 능력 명세

### 4.1 공통 Node 로직 (기존 구현)

현재 Node.cs는 모든 타입이 동일한 로직 (이동, TakeDamage, Die). 특수 능력 추가를 위해 다음 구조 변경 필요:

**구조 변경 방안: 컴포넌트 기반 (권장)**

```csharp
// 기존 Node.cs는 유지하되, 특수 능력은 별도 컴포넌트로 추가
// 각 Node 프리팹에 필요한 능력 컴포넌트를 붙이는 방식

public abstract class NodeAbility : MonoBehaviour
{
    protected Node _node;

    void Awake() { _node = GetComponent<Node>(); }

    public virtual void OnInitialize() { }
    public virtual void OnUpdate() { }
    public virtual float ModifyIncomingDamage(float damage) { return damage; }
    public virtual void OnDie() { }
}
```

이 방식을 택하면 Node.cs의 수정을 최소화하면서 각 타입별 능력을 독립적으로 구현/테스트할 수 있다.

**Node.cs 수정 사항:**

```csharp
public class Node : Poolable
{
    // 기존 필드...

    NodeAbility[] _abilities;  // 캐시

    public void Initialize(...)
    {
        // 기존 초기화...
        _abilities = GetComponents<NodeAbility>();
        foreach (var ability in _abilities)
            ability.OnInitialize();
    }

    public void TakeDamage(float damage, bool ignoreArmor = false)
    {
        if (!IsAlive) return;

        // 능력에 의한 데미지 수정
        float modified = damage;
        foreach (var ability in _abilities)
            modified = ability.ModifyIncomingDamage(modified);

        CurrentHp -= modified;
        // SpawnDamagePopup(modified);  // 데미지 숫자 팝업
        if (CurrentHp <= 0f)
            Die();
    }

    void Update()
    {
        if (!IsAlive || !IsUsing) return;

        foreach (var ability in _abilities)
            ability.OnUpdate();

        Move();
    }
}
```

### 4.2 N04 - Shield Node (고정 방어력)

**능력**: 받는 데미지에서 방어력만큼 고정 차감.

```csharp
public class ShieldAbility : NodeAbility
{
    [Header("방어력")]
    public float armor;  // NodeData에서 설정. 기본값 5.

    public override float ModifyIncomingDamage(float damage)
    {
        // 방어력만큼 감소, 최소 1 데미지 보장
        float reduced = Mathf.Max(1f, damage - armor);
        return reduced;
    }
}
```

**방어력 공식:**

```
최종 데미지 = Max(1, 원래 데미지 - 방어력)

예시 (방어력 5):
Arrow Lv1 (8 DMG): Max(1, 8-5) = 3
Arrow Lv3 (18 DMG): Max(1, 18-5) = 13
Cannon Lv1 (20 DMG): Max(1, 20-5) = 15

예외: Void Tower Lv3+ (ignoreArmor=true): 방어력 무시, 원래 데미지 적용
```

**시각 피드백**: 방어력으로 감소된 경우 "BLOCKED" 텍스트(작게) + 데미지 숫자가 회색으로 표시.

### 4.3 N06 - Regen Node (HP 자동 회복)

**능력**: 매 틱마다 최대 HP의 일정 비율 회복.

```csharp
public class RegenAbility : NodeAbility
{
    [Header("회복")]
    public float regenPercent = 0.05f;  // 초당 최대 HP의 5%
    public float tickInterval = 0.5f;    // 0.5초마다 회복 틱

    float _tickTimer;
    float _maxHp;

    public override void OnInitialize()
    {
        _maxHp = _node.CurrentHp;  // Initialize 직후 = 최대 HP
        _tickTimer = 0f;
    }

    public override void OnUpdate()
    {
        _tickTimer += Time.deltaTime;
        if (_tickTimer >= tickInterval)
        {
            _tickTimer = 0f;

            float healAmount = _maxHp * regenPercent * tickInterval;
            _node.Heal(healAmount);
        }
    }
}

// Node.cs에 Heal 추가
public void Heal(float amount)
{
    if (!IsAlive) return;
    float before = CurrentHp;
    CurrentHp = Mathf.Min(CurrentHp + amount, _scaledHp);  // 최대 HP 초과 불가

    if (CurrentHp > before)
        SpawnHealPopup(CurrentHp - before);  // 초록색 "+N" 팝업
}
```

**회복 수치 (Floor 1 기준):**

```
Regen Node HP: 50
초당 회복: 50 * 0.05 = 2.5 HP/초
0.5초 틱: 1.25 HP/틱

필요 DPS (회복을 상쇄하려면): 2.5 이상
Arrow Lv1 DPS: 10.0 -> 순 DPS: 10.0 - 2.5 = 7.5
처치 시간: 50 / 7.5 = 6.67초 (일반 Node 대비 3배 이상)
```

**시각 피드백**: 회복 틱마다 Node 주변에 작은 초록 파티클 상승. HP 바에 초록 글로우.

### 4.4 N07 - Phase Node (주기적 무적)

**능력**: 이동 중 주기적으로 무적 상태 전환. 무적 중에는 모든 데미지 무효.

```csharp
public class PhaseAbility : NodeAbility
{
    [Header("무적 사이클")]
    public float visibleDuration = 2.0f;   // 공격 가능 상태 지속
    public float phaseDuration = 1.0f;     // 무적 상태 지속

    float _cycleTimer;
    bool _isPhased;

    public override void OnInitialize()
    {
        _cycleTimer = 0f;
        _isPhased = false;
        SetPhaseVisual(false);
    }

    public override void OnUpdate()
    {
        _cycleTimer += Time.deltaTime;

        if (!_isPhased && _cycleTimer >= visibleDuration)
        {
            // 무적 진입
            _isPhased = true;
            _cycleTimer = 0f;
            SetPhaseVisual(true);
        }
        else if (_isPhased && _cycleTimer >= phaseDuration)
        {
            // 무적 해제
            _isPhased = false;
            _cycleTimer = 0f;
            SetPhaseVisual(false);
        }
    }

    public override float ModifyIncomingDamage(float damage)
    {
        if (_isPhased)
        {
            // 무적 중: 데미지 0 + "IMMUNE" 텍스트
            SpawnImmunePopup();
            return 0f;
        }
        return damage;
    }

    void SetPhaseVisual(bool phased)
    {
        // 무적 시: 스프라이트 반투명 (alpha 0.3) + 보라 글로우
        // 해제 시: 불투명 복귀 (0.2초 페이드)
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = phased ? 0.3f : 1.0f;
            sr.color = c;
        }
    }
}
```

**무적 사이클 타임라인:**

```
|--공격가능(2초)--|--무적(1초)--|--공격가능(2초)--|--무적(1초)--|...
```

**대응 전략**: Laser/Void Tower는 지속 데미지이므로 공격 가능 시간 동안 효율적으로 데미지를 넣을 수 있음.

**시각 피드백**: 무적 진입 0.2초 전 경고 (Node가 깜빡임), 무적 중 반투명 + 보라 오라.

### 4.5 N08 - Split Node (분열)

**능력**: 죽으면 소형 Node 2마리로 분열.

```csharp
public class SplitAbility : NodeAbility
{
    [Header("분열")]
    public NodeData splitNodeData;      // 분열체 NodeData (소형 Bit Node 변형)
    public int splitCount = 2;           // 분열 수
    public float splitHpRatio = 0.3f;    // 분열체 HP = 원본 maxHP의 30%
    public float splitSpeedMultiplier = 1.2f;  // 분열체는 약간 빠름

    public override void OnDie()
    {
        // 현재 위치에서 분열체 스폰
        for (int i = 0; i < splitCount; i++)
        {
            // 약간의 오프셋으로 겹침 방지
            Vector3 offset = new Vector3(
                Random.Range(-0.3f, 0.3f),
                Random.Range(-0.3f, 0.3f), 0);

            var go = Poolable.TryGetPoolable(splitNodeData.prefab, null);
            go.transform.position = transform.position + offset;

            var splitNode = go.GetComponent<Node>();
            if (splitNode != null)
            {
                // 분열체 초기화: 현재 위치부터 남은 웨이포인트 따라 이동
                splitNode.InitializeAsSplit(
                    splitNodeData,
                    _node.RemainingWaypoints,  // 현재 Node의 남은 경로
                    splitHpRatio * _node.MaxHp, // 분열체 HP
                    _node.ScaledSpeed * splitSpeedMultiplier,
                    splitNodeData.bitDrop        // 분열체 Soul 드롭
                );

                WaveSpawner.Instance.OnNodeAdded();  // aliveCount++
            }
        }

        // 분열 이펙트
        SpawnSplitEffect(transform.position);
    }
}

// Node.cs에 추가
public Transform[] RemainingWaypoints
{
    get
    {
        // 현재 waypointIndex부터 끝까지의 경로 반환
        var remaining = new Transform[_waypoints.Length - _waypointIndex];
        System.Array.Copy(_waypoints, _waypointIndex, remaining, 0, remaining.Length);
        return remaining;
    }
}

public float MaxHp => _scaledHp;
public float ScaledSpeed => _scaledSpeed;

public void InitializeAsSplit(NodeData data, Transform[] waypoints,
    float hp, float speed, int bitDrop)
{
    Data = data;
    _waypoints = waypoints;
    _waypointIndex = 0;
    _scaledHp = hp;
    _scaledSpeed = speed;
    _scaledBitDrop = bitDrop;
    CurrentHp = hp;
    IsUsing = true;
}
```

**분열체 스펙 (Floor 1 기준):**

| 항목 | 원본 (Split Node) | 분열체 |
|------|-------------------|--------|
| HP | 30 | 9 (30% of 30) |
| 속도 | 1.3 | 1.56 (1.3 * 1.2) |
| Soul 드롭 | 5 | 2 |
| 기지 데미지 | 1 | 1 |
| 수량 | 1 | 2 |

**분열체 재분열: 불가** (총괄PD 확정). 분열체는 SplitAbility 컴포넌트를 가지지 않으며, 1회만 분열한다. 분열체의 프리팹에는 SplitAbility를 부착하지 않는 것으로 구현.

**시각 피드백**: 죽을 때 "쪼개지는" 이펙트 + 2마리가 좌우로 살짝 벌어지며 등장.

### 4.6 N09 - Boss Node (보스)

**능력**: 극히 높은 HP + 방어력. 특수 패턴 없음 (순수 스탯 보스). 다만 시각적 위압감 극대화.

```csharp
// Boss는 ShieldAbility를 가지며 (방어력 3), 추가로:
public class BossAbility : NodeAbility
{
    [Header("보스 연출")]
    public float entranceSlowdown = 0.5f;  // 등장 시 0.5초 슬로우모션

    public override void OnInitialize()
    {
        // 보스 등장 연출
        StartCoroutine(BossEntrance());
    }

    IEnumerator BossEntrance()
    {
        // 경고 UI
        ShowBossWarning();

        // 슬로우 모션
        Time.timeScale = 0.3f;
        yield return new WaitForSecondsRealtime(entranceSlowdown);
        Time.timeScale = _savedTimeScale;  // 이전 배속 복원

        HideBossWarning();
    }

    public override void OnDie()
    {
        // 보스 처치 연출
        StartCoroutine(BossDeathSequence());
    }

    IEnumerator BossDeathSequence()
    {
        // 0.5초 슬로우 모션
        Time.timeScale = 0.2f;

        // 대형 폭발 이펙트
        SpawnBossExplosionEffect(transform.position);

        // 카메라 흔들림
        CameraShake.Trigger(0.15f, 0.5f);

        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = _savedTimeScale;
    }
}
```

**보스 등장 타이밍**: 각 Floor의 최종 Incursion에서 1체 등장 (다른 Node와 함께).

### 4.7 Node 능력 요약표

| Node | 컴포넌트 | 핵심 파라미터 | 대응 전략 |
|------|---------|-------------|---------|
| Bit (Node) | 없음 (기본) | - | Arrow |
| Quick | 없음 (빠른 속도) | speed: 3.0 | Ice 감속 |
| Heavy | 없음 (높은 HP) | hp: 80 | 화력 집중 |
| Shield | ShieldAbility | armor: 5 | 고데미지 타워 or Void(Lv3+) |
| Swarm | 없음 (대량 스폰) | hp: 5, 대량 | AoE (Cannon, Lightning, Void) |
| Regen | RegenAbility | 5% HP/초, 0.5s틱 | 고DPS 집중 |
| Phase | PhaseAbility | 2초 공격/1초 무적 | 지속 데미지 (Laser, Void) |
| Split | SplitAbility | 분열2, HP 30% | AoE |
| Boss | BossAbility + ShieldAbility | armor: 3, 고HP | 전체 조합 |

---

## 5. Incursion 진행 시스템 상세

### 5.1 Incursion 흐름

```
[런 시작]
   |
   v
[Incursion 1 딜레이] --delayBeforeWave 초--> [Incursion 1 스폰]
                                              |
                                    [SpawnGroup 순차 실행]
                                    - group[0]: NodeData A x count, interval초 간격
                                    - group[1]: NodeData B x count, interval초 간격
                                              |
                                    [모든 Node 죽거나 통과 (aliveCount == 0)]
                                              |
                                    [Incursion 클리어 판정]
                                    - "WAVE CLEAR" 배너 (0.8초)
                                    - Incursion 클리어 보너스 Soul 지급
                                              |
[Incursion N+1 딜레이] --delayBeforeWave 초--> [Incursion N+1 스폰]
                                              |
                                            ...
                                              |
                                    [최종 Incursion의 모든 Node 처리됨]
                                              |
                                    [RunManager.OnWaveCompleted()]
                                              |
                                    [EndRun(true) = Floor 클리어]
```

### 5.2 Incursion 간 대기 시간 규칙

| 상황 | 대기 시간 | 근거 |
|------|----------|------|
| Incursion 1 시작 전 | 3초 (WaveData.delayBeforeWave) | 맵 확인 + 첫 타워 배치 시간 |
| 이후 Incursion 시작 전 | 3초 (기본) | 타워 추가 배치/합성 시간 |
| 보스 Incursion 전 | 5초 (총괄PD 확정) | 긴장감 + 준비 시간 |

현재 구현에서는 `WaveData.delayBeforeWave`로 개별 Incursion마다 설정 가능. 기본값 3초.

### 5.3 Incursion 클리어 판정 로직

**현재 구현 (WaveSpawner.cs):**

```csharp
// SpawnAllWaves 코루틴 내부
while (_aliveCount > 0)
    yield return null;
// -> 다음 Incursion로 진행
```

**수정 필요 사항:**

현재 코드는 Incursion 단위가 아닌 전체 Incursion 루프로 처리됨. 개별 Incursion 클리어 이벤트가 필요.

```csharp
// WaveSpawner.cs 수정안
IEnumerator SpawnAllWaves()
{
    for (int i = 0; i < _currentStage.waves.Length; i++)
    {
        _currentWaveIndex = i;
        var wave = _currentStage.waves[i];

        // Incursion 시작 배너
        OnWaveStart(i);

        yield return new WaitForSeconds(wave.delayBeforeWave);
        yield return StartCoroutine(SpawnWave(wave));

        // 모든 Node 처리 대기
        while (_aliveCount > 0)
            yield return null;

        // Incursion 클리어 이벤트 (마지막 Incursion 제외)
        if (i < _currentStage.waves.Length - 1)
            OnWaveClear(i);
    }

    // 최종 Incursion 클리어 = Floor 클리어
    if (Singleton<RunManager>.HasInstance)
        RunManager.Instance.OnWaveCompleted();
}

void OnWaveStart(int waveIndex)
{
    // UI: "Incursion N" 배너 표시
    // 보스 Incursion면 경고 연출
}

void OnWaveClear(int waveIndex)
{
    // 1. "WAVE CLEAR" 배너
    // 2. 보너스 Soul 지급
    int bonus = CalculateWaveClearBonus(waveIndex);
    RunManager.Instance.AddBit(bonus);

    // 3. UI: "+{bonus} Soul" 팝업
    // (타워는 Sanctum 보물상자로만 획득 가능 - Incursion 클리어 시 타워 지급 없음)
}
```

### 5.4 Incursion 클리어 보너스 계산

```
Incursion 클리어 보너스 Soul = 10 + (Incursion 번호 * 5)

예시 (Floor 1):
Incursion 1 클리어: 10 + (1*5) = 15 Soul
Incursion 2 클리어: 10 + (2*5) = 20 Soul
Incursion 3 클리어: 10 + (3*5) = 25 Soul
Incursion 4 클리어: 10 + (4*5) = 30 Soul
Incursion 5 클리어: (Floor 클리어 보상으로 대체)

Floor 배율 적용:
최종 보너스 = 기본 보너스 * StageData.bitDropMultiplier
```

### 5.5 Floor 클리어 연출 타이밍

```
[최종 Incursion 마지막 Node 처리] (t=0)
   |
   v (0~0.5초)
[잠시 정적] - 화면에 적이 없는 0.5초간 여운
   |
   v (0.5~2.0초)
[화면 밝기 상승 + 금빛 파티클] - 1.5초 연출
   |
   v (2.0~2.5초)
["STAGE CLEAR!" 대형 배너] - 0.5초 등장 애니메이션
   |
   v (2.5~4.0초)
[Core Fragment 획득 연출] - 금색 룬석 회전 + 광선 방사 1.5초
   |
   v (4.0초)
[RunEnd 패널 표시] - 결과 정산 + 버튼
```

---

## 6. 인게임 경제 (런 내)

### 6.1 Soul 수입/지출 전체 흐름

```
[수입]                                [지출]
Node 처치 드롭 ----+                  +---- 타워 구매 (placeCost) [인게임 Soul 구매]
Incursion 클리어 보너스 --+-> 보유 Soul <--+
콤보 보너스 --------+                  +---- (런 종료 시 잔여 Bit는 영구 저장)

(타워 판매 불가 - 환불 경로 없음)
```

### 6.2 Node 처치 Soul 드롭 상세

```
드롭량 = NodeData.bitDrop * StageData.bitDropMultiplier * RunModifiers.bitDropMultiplier(*)

(*) 현재 RunModifiers에 bitDropMultiplier 필드가 없음.
    스킬 트리의 "Soul 획득량" 노드 효과를 반영하려면 추가 필요.
```

**RunModifiers 확장 필요:**

```csharp
public struct RunModifiers
{
    public float attackDamageMultiplier;
    public float attackSpeedMultiplier;
    public int bonusBaseHp;

    // 신규 추가
    public float bitDropMultiplier;     // 기본 1.0, "Soul 획득량" 스킬로 증가
    public float rangeMultiplier;       // 기본 1.0, "사거리 강화" 스킬로 증가
    public int startBit;                // 기본 0, "시작 Soul" 스킬로 증가
    public float spawnRateMultiplier;   // 기본 1.0, "스폰율 증가" 스킬로 증가
    public float baseHpRegen;           // 기본 0, "HP 회복" 스킬로 설정
    public bool critUnlocked;           // 크리티컬 해금 여부
    public float critChance;            // 크리티컬 확률
    public float critMultiplier;        // 크리티컬 배율
}
```

### 6.3 타워 구매 비용 규칙

| 타워 | Lv1 배치 비용 (Soul) |
|------|-------------------|
| Arrow | 30 |
| Cannon | 60 |
| Ice | 45 |
| Lightning | 80 |
| Laser | 120 |
| Void | 150 |

**구매 방법**: 하단 [+구매] 버튼 -> 해금된 타워 목록 팝업 -> 타워 선택 -> Soul 차감 -> 인벤토리에 Lv1 타워 추가.

비용은 TowerData.placeCost에 정의. 합성 비용은 없음 (타워 1기 소모가 곧 비용).

### 6.4 콤보 보너스 규칙

**콤보 정의**: 3초 이내에 연속으로 Node를 처치하면 콤보 카운터 증가.

```csharp
public class ComboSystem : MonoBehaviour
{
    float _comboTimer;
    int _comboCount;

    const float COMBO_WINDOW = 3.0f;    // 3초 이내 연속 처치
    const int COMBO_THRESHOLD = 5;       // 5콤보부터 보너스
    const float COMBO_BONUS_RATE = 0.5f; // 50% 추가 Soul

    public void OnNodeKilled(int baseBitDrop)
    {
        _comboTimer = COMBO_WINDOW;
        _comboCount++;

        int bonus = 0;
        if (_comboCount >= COMBO_THRESHOLD)
        {
            bonus = Mathf.RoundToInt(baseBitDrop * COMBO_BONUS_RATE);
            // UI: "COMBO x{_comboCount}! +{bonus}" 표시
        }

        return bonus;
    }

    void Update()
    {
        if (_comboTimer > 0)
        {
            _comboTimer -= Time.deltaTime;
            if (_comboTimer <= 0)
            {
                if (_comboCount >= COMBO_THRESHOLD)
                    ShowComboEndUI(_comboCount);  // "COMBO BREAK" 연출
                _comboCount = 0;
            }
        }
    }
}
```

**콤보 보너스 테이블:**

| 콤보 수 | 추가 Soul | 시각 피드백 |
|---------|---------|-----------|
| 1~4 | 없음 | 콤보 카운터 표시 시작 (x1, x2...) |
| 5~9 | +50% | "COMBO x5!" 텍스트 + 파티클 크기 증가 |
| 10~19 | +100% | "COMBO x10!" + 화면 미세 흔들림 |
| 20+ | +200% | "MEGA COMBO!" + 화면 플래시 |

### 6.5 경제 밸런스 시뮬레이션 (Floor 1, 영구 업그레이드 0)

```
런 시작: 0 Soul (시작 Soul 스킬 없음)

Incursion 1 (Bit Node x5):
  처치 수입: 3 Soul * 5 = 15 Soul
  Incursion 클리어 보너스: 15 Soul
  소계: 30 Soul
  -> Arrow Tower 1기 구매 가능 (30 Soul)

Incursion 2 (Bit Node x8):
  처치 수입: 3 * 8 = 24 Soul
  클리어 보너스: 20 Soul
  소계: 44 Soul
  -> 누적 ~74 Soul (2번째 Arrow 구매 후 잔여 ~14 Soul)

Incursion 3 (Bit x6, Quick x3):
  처치 수입: 3*6 + 4*3 = 30 Soul
  클리어 보너스: 25 Soul
  소계: 55 Soul
  -> 대부분 여기서 사망 (초기 상태)
  -> 획득 총 Soul: ~80~130 (도달 Incursion에 따라)

매 런 영구 업그레이드: 공격력 Lv1(50), 공속 Lv1(80) 등 1~2개 가능
```

---

## 7. 전투 피드백/이펙트 트리거 명세

### 7.1 전투 이벤트별 피드백 목록

| 이벤트 | 시각 피드백 | 사운드 | 카메라 | 조건 |
|--------|-----------|--------|--------|------|
| 타워 공격 발사 | 타워 발사 애니메이션 (2프레임) | 타워별 공격음 | - | 매 공격 |
| 투사체 적중 | 타격 스파크 (타워 컬러) | 히트음 (짧고 경쾌) | - | 매 적중 |
| Node 처치 | 파괴 파티클(4~6조각) + "+N Soul" 팝업 | 처치음 (Node 타입별) | - | 매 처치 |
| 동시 다수 처치 (AoE) | 큰 폭발 이펙트 + 합산 Soul 팝업 | 폭발음 (강조) | 미세 흔들림 (0.05s) | 3마리+ 동시 |
| 콤보 5+ | "COMBO xN!" 텍스트 + 파티클 확대 | 콤보 효과음 (상승) | - | 콤보 5+ |
| Node 기지 도착 | 빨간 플래시 (기지 위치) + HP 바 감소 | 피격음 | 화면 테두리 붉게 | HP 감소 시 |
| Incursion 시작 | "Incursion N" 배너 슬라이드인 | 경고음 (짧게) | - | 매 Incursion |
| Incursion 클리어 | "WAVE CLEAR" 배너 + 화면 가장자리 플래시 | 클리어 팡파르 (짧게) | - | 매 클리어 |
| 보스 등장 | "WARNING!" 배너 + 화면 어두워짐 | 보스 등장 사운드 | 슬로우 모션 0.5초 | 보스 Incursion |
| 보스 처치 | 대형 폭발 + 대형 Soul 비산 | 보스 처치 팡파르 | 슬로우 0.5초 + 흔들림 | 보스 사망 |
| 타워 배치 | 배치 이펙트 (마법진 출현) | 배치음 (돌 놓는 느낌) | - | 배치 시 |
| 타워 합성 | 발광 + "Lv N!" 팝업 | 강화음 (상승 글리산도) | Lv4(MAX) 시 미세 흔들림 | 합성 시 |
| ~~타워 판매~~ | (삭제 - 타워 판매 불가) | - | - | - |
| Floor 클리어 | 전체 밝기 상승 + Core Fragment 등장 + 금빛 파티클 | 대형 팡파르 | - | Floor 클리어 |
| HP < 30% | 화면 테두리 붉은 바이넷 (지속) | 심장박동 사운드 (반복) | - | 지속 경고 |
| HP < 10% | 바이넷 강도 증가 + 펄스 | 심장박동 빨라짐 | - | 위급 경고 |

### 7.2 데미지 숫자 팝업 시스템

```csharp
public class DamagePopup : MonoBehaviour  // 오브젝트 풀링 대상
{
    TextMeshPro _text;

    public void Show(Vector3 position, float damage, PopupType type)
    {
        transform.position = position + RandomOffset();

        switch (type)
        {
            case PopupType.Normal:
                _text.text = Mathf.RoundToInt(damage).ToString();
                _text.color = Color.white;
                _text.fontSize = 3f;
                break;
            case PopupType.Critical:
                _text.text = Mathf.RoundToInt(damage).ToString() + "!";
                _text.color = Color.yellow;  // 금색
                _text.fontSize = 4f;
                break;
            case PopupType.Soul:
                _text.text = "+" + damage;
                _text.color = new Color(0.25f, 1f, 0.53f);  // 에메랄드
                _text.fontSize = 3.5f;
                break;
            case PopupType.Heal:
                _text.text = "+" + Mathf.RoundToInt(damage).ToString();
                _text.color = Color.green;
                _text.fontSize = 2.5f;
                break;
            case PopupType.Immune:
                _text.text = "IMMUNE";
                _text.color = new Color(0.44f, 0.38f, 0.56f);  // 안개 보라
                _text.fontSize = 2.5f;
                break;
            case PopupType.Blocked:
                _text.text = Mathf.RoundToInt(damage).ToString();
                _text.color = Color.gray;
                _text.fontSize = 2.5f;
                break;
        }

        // 위로 떠오르며 페이드아웃 (0.8초)
        StartCoroutine(FloatAndFade());
    }

    IEnumerator FloatAndFade()
    {
        float duration = 0.8f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            transform.position = startPos + Vector3.up * (t * 0.8f);

            Color c = _text.color;
            c.a = 1f - t;
            _text.color = c;

            yield return null;
        }

        Poolable.TryPool(gameObject);
    }
}
```

### 7.3 카메라 셰이크 명세

```csharp
public static class CameraShake
{
    // intensity: 흔들림 강도 (0.01 ~ 0.2)
    // duration: 지속 시간 (0.05 ~ 0.5초)

    public static void Trigger(float intensity, float duration)
    {
        // Perlin Noise 기반 랜덤 오프셋
        // 시작은 강하고 점점 약해짐 (지수 감쇠)
    }
}
```

| 트리거 | intensity | duration |
|--------|-----------|----------|
| Cannon AoE (3+ 타격) | 0.05 | 0.1 |
| 보스 등장 | 0.08 | 0.3 |
| 보스 처치 | 0.15 | 0.5 |
| 타워 Lv4 합성 (MAX) | 0.03 | 0.1 |
| 기지 HP 1회 큰 피해 (3+) | 0.04 | 0.15 |

---

## 8. 맵/경로 시스템

### 8.1 경로 방식: 웨이포인트

현재 구현은 `WaveSpawner.waypoints` (Transform 배열)을 사용하는 웨이포인트 방식.

```
스폰 포인트 ---> WP1 ---> WP2 ---> WP3 ---> ... ---> 기지 (엔드 포인트)
```

**유지 결정**: 웨이포인트 방식 유지. 타일맵은 시각적 맵 표현에만 사용하고, Node 이동 경로는 웨이포인트가 담당.

### 8.2 타일맵 연동 구조

```
[타일맵 레이어 구성]

Tilemap_Background  : 배경 타일 (L1)
Tilemap_Path        : 경로 표시 타일 (L3, 시각용)
Tilemap_Placement   : 배치 가능 칸 타일 (보이지 않지만 판정에 사용)
Tilemap_Wall        : 벽/장애물 (L1)

[웨이포인트는 별도]
- 빈 GameObject로 경로 꼭짓점 배치
- Gizmo로 에디터에서 시각화
- WaveSpawner.waypoints에 할당
```

### 8.3 배치 가능 영역 규칙

| 규칙 | 상세 |
|------|------|
| 경로 위 배치 불가 | 경로 타일에는 타워 배치 불가 |
| 벽 위 배치 불가 | 벽/장애물 타일에 배치 불가 |
| 1칸 1타워 | 같은 칸에 2개 이상 타워 배치 불가 |
| 경로 인접 우선 | 경로 바로 옆 칸이 최적 배치 (사거리 내 Node 접근) |
| 배치 칸 시각화 | 평소: 거의 보이지 않는 격자점. 드래그 중: 초록 하이라이트 |

### 8.4 다중 경로

**Floor 4 이후 도입.**

```
Floor 1~3: 단일 경로
Floor 4+: 분기/합류 경로

예시 (Floor 4):
              +---> WP_B1 ---> WP_B2 ---+
              |                          |
Spawn --> WP1 +                          +--> WP_merge --> 기지
              |                          |
              +---> WP_C1 ---> WP_C2 ---+
```

**구현 방안:**

```csharp
// WaveData.SpawnGroup에 경로 인덱스 추가
[Serializable]
public class SpawnGroup
{
    public NodeData nodeData;
    public int count = 5;
    public float spawnInterval = 0.5f;
    public int pathIndex = 0;  // 신규: 어느 경로로 보낼 것인가 (0=기본, 1=분기1, ...)
}

// WaveSpawner에 다중 경로 지원
public Transform[][] pathWaypoints;  // pathWaypoints[0] = 기본 경로, [1] = 분기1, ...
```

---

## 9. 밸런싱 가이드

### 9.1 Floor 1 상세 Incursion 구성 (GDD 확인 + 보완)

| Incursion | Node 구성 | 스폰 간격(초) | 지연(초) | 총 Node | 총 Soul (처치 전부) | 설계 의도 |
|--------|----------|-------------|---------|---------|-----------------|----------|
| 1 | Bit x5 | 1.2 | 3.0 | 5 | 15 | 학습. 타워가 잡는다. |
| 2 | Bit x8 | 1.0 | 3.0 | 8 | 24 | 물량 증가. 타워 추가 유도. |
| 3 | Bit x6 + Quick x3 | 0.8 | 3.0 | 9 | 30 | 빠른 적. 긴장감. |
| 4 | Bit x10 + Quick x5 | 0.7 | 3.0 | 15 | 50 | 물량 압박. 초기 상태 사망 구간. |
| 5 | Bit x8 + Quick x4 + Heavy x2 | 0.6 | 5.0 | 14 | 60 | 최종. 첫 클리어 시 성취감. |

**총 가능 Soul (전부 처치 + 클리어 보너스):**

```
처치: 15+24+30+50+60 = 179 Soul
클리어 보너스: 15+20+25+30 = 90 Soul (Incursion5는 Floor 클리어로 대체)
합계: 269 Soul (모든 Node 처치 + 모든 Incursion 클리어)
Floor 클리어 시: +Core Fragment 2
```

### 9.2 첫 런~첫 클리어 경험 곡선

```
런 1 (영구 업그레이드 0):
  Arrow 1기 자동 배치 (DPS 10)
  Incursion 1: 클리어 (여유)
  Incursion 2: 타워 1기 추가 (Soul 30 소모) -> 클리어 (빠듯)
  Incursion 3: Quick Node 등장 -> 1~2마리 기지 도달 -> HP 감소
  Incursion 3~4에서 사망 (HP 0)
  획득: ~80~100 Soul

런 2~5 (공격력+1, 공속+1 구매):
  영구 업그레이드 효과 체감
  Incursion 3~4까지 안정적 도달
  획득: ~100~150 Soul / 런

런 6~10 (공격력+3, 공속+2, HP+2):
  DPS 체감 상승. 타워 2~3기 운용
  Incursion 4~5 진입
  획득: ~130~200 Soul / 런

런 11~20 (다수 업그레이드):
  Floor 1 클리어 가능
  총 플레이 시간: ~20~30분
```

### 9.3 타워 DPS 대 Node HP 비율 가이드라인

| 지표 | 목표 값 | 설명 |
|------|---------|------|
| 기본 Node 처치 시간 | 1.5~2.5초 | 너무 빠르면 긴장감 없음, 너무 느리면 답답 |
| 빠른 Node 처치 시간 | 1.0~1.5초 | 감속 없이 사거리 안에 있는 시간 |
| Heavy Node 처치 시간 | 6~10초 | 화력 집중 유도 |
| Boss Node 처치 시간 | 20~40초 | 전체 타워 동원 |
| Incursion당 기지 데미지 (목표) | 기지 HP의 10~25% | Incursion 2~3에서 죽도록 |
| 런당 생존 시간 (초기) | 30초~1분 | 빠른 죽음 -> 빠른 재시도 |

**DPS 비율 검증 (Floor 1, 영구 업그레이드 0):**

```
Arrow Lv1 DPS: 10.0
Bit Node HP: 20 -> 처치 시간: 2.0초 (목표 범위 내)
Quick Node HP: 12, 속도 3.0 -> 사거리 통과 시간 ~2초, 처치 시간 1.2초 -> 통과 가능성 있음 (의도)
Heavy Node HP: 80 -> Arrow 1기로 8.0초 (목표 범위 내)
```

### 9.4 Floor별 권장 빌드 (밸런스 가이드)

| Floor | 주력 타워 | 보조 타워 | 권장 영구 업그레이드 |
|---------|---------|---------|-----------------|
| 1 | Arrow x3~4 | - | 공격력 Lv3, 공속 Lv2, HP Lv2 |
| 2 | Arrow x3, Cannon x1~2 | - | 크리티컬 해금, Cannon 해금 |
| 3 | Arrow x2, Cannon x2, Ice x1~2 | - | Ice 해금, 사거리+ |
| 4~5 | Cannon x2, Ice x2, Lightning x1~2 | Arrow x2 | Lightning 해금 |
| 6~7 | Lightning x3, Laser x1~2 | Ice x2, Cannon x2 | Laser 해금 |
| 8~9 | Laser x2, Void x1~2 | Lightning x2, Ice x2 | Void 해금 |
| 10 | 전 타워 조합 | - | 가능한 모든 업그레이드 |

---

## 10. 크리티컬 시스템

스킬 트리에서 "크리티컬 해금" 노드를 찍으면 활성화.

```csharp
// Tower.cs Attack()에 통합
void Attack()
{
    float damage = data.GetDamage(Level) * modifiers.attackDamageMultiplier;

    // 크리티컬 판정
    bool isCrit = false;
    if (modifiers.critUnlocked && Random.value < modifiers.critChance)
    {
        damage *= modifiers.critMultiplier;
        isCrit = true;
    }

    // 투사체 발사 or 즉발 공격
    // ...

    // 크리티컬 이펙트
    if (isCrit)
    {
        SpawnCritEffect(_currentTarget.transform.position);
        // 데미지 팝업: 노란색 + 크게 + "!" 접미사
    }
}
```

| 항목 | 값 |
|------|-----|
| 기본 확률 | 15% |
| 기본 배율 | 2.0x |
| 시각 피드백 | 금색 데미지 숫자 + 별 모양 파티클 + 타격 시 잠깐 밝아짐 |
| 사운드 | 일반 히트음보다 강조된 "크랭" 사운드 |

---

## 11. 배속/일시정지 시스템

현재 InGameUI.cs에 기본 구현 완료 (TimeScale 조작).

**보완 사항:**

| 항목 | 현재 | 수정/추가 |
|------|------|---------|
| 배속 단계 | x1, x2, x3 | 유지 (스킬 트리 "배속 해금" 필요) |
| 기본 해금 | 전부 해금 | x1만 기본. x2/x3은 "배속 해금" Core Fragment 노드 필요. |
| 일시정지 중 조작 | 불가 | **불가 유지** (총괄PD 확정). 일시정지 중에는 타워 배치/합성 불가. |
| 배속 중 이펙트 | 정상 속도 | 배속에 맞춰 이펙트 지속시간도 단축 (Time.deltaTime 기반이면 자동) |

---

## 부록 A: 구현 우선순위 (프로그래밍팀장 참조)

### Phase 1: 핵심 전투 루프 (최우선)

| 순서 | 작업 | 관련 섹션 | 난이도 |
|------|------|---------|--------|
| 1-1 | PlacementGrid (격자 배치 판정) | 2.1 | 중 |
| 1-2 | TowerDragController (드래그 배치) | 2.2 | 중 |
| 1-3 | 타워 인벤토리 시스템 (TowerInventory) | 2.6 | 중 |
| 1-4 | Incursion 클리어 이벤트 + 보너스 (WaveSpawner 수정) | 5.3, 5.4 | 하 |
| 1-5 | 콤보 시스템 | 6.4 | 하 |
| 1-6 | 데미지 숫자 팝업 | 7.2 | 하 |

### Phase 2: 타워 특수 능력

| 순서 | 작업 | 관련 섹션 | 난이도 |
|------|------|---------|--------|
| 2-1 | CannonProjectile (AoE) | 3.3 | 중 |
| 2-2 | Node 감속 시스템 + IceProjectile | 3.4 | 중 |
| 2-3 | LightningTower (체인 공격) | 3.5 | 상 |
| 2-4 | LaserTower (빔 + 쿨다운) | 3.6 | 상 |
| 2-5 | VoidTower (범위 지속) | 3.7 | 중 |

### Phase 3: Node 특수 능력

| 순서 | 작업 | 관련 섹션 | 난이도 |
|------|------|---------|--------|
| 3-1 | NodeAbility 기반 구조 변경 | 4.1 | 중 |
| 3-2 | ShieldAbility (방어력) | 4.2 | 하 |
| 3-3 | RegenAbility (회복) | 4.3 | 하 |
| 3-4 | PhaseAbility (무적) | 4.4 | 중 |
| 3-5 | SplitAbility (분열) | 4.5 | 상 |
| 3-6 | BossAbility (연출) | 4.6 | 중 |

### Phase 4: 시스템 보완

| 순서 | 작업 | 관련 섹션 | 난이도 |
|------|------|---------|--------|
| 4-1 | 타워 배치 SaveData 저장/복원 | 2.5 | 중 |
| 4-2 | ~~타워 판매 시스템~~ (삭제 - 판매 불가) | 2.4 | - |
| 4-3 | RunModifiers 확장 | 6.2 | 하 |
| 4-4 | 크리티컬 시스템 | 10 | 하 |
| 4-5 | 다중 경로 지원 | 8.4 | 중 |
| 4-6 | 전투 피드백/이펙트 | 7 | 중~상 |

---

## 부록 B: TowerData.cs 확장안 (전체)

프로그래밍팀장이 참조할 수 있도록 TowerData의 전체 확장 필드를 정리한다.

```csharp
[CreateAssetMenu(fileName = "Tower_", menuName = "Soulspire/Tower Data")]
public class TowerData : ScriptableObject
{
    // === 기존 필드 (유지) ===
    public string towerId;
    public string towerName;
    public TowerType type;
    public Sprite icon;
    public GameObject prefab;
    public GameObject projectilePrefab;

    [Header("레벨별 스탯 (Lv1~4)")]
    public float[] damage = { 10f, 18f, 30f, 50f };
    public float[] attackSpeed = { 1f, 1.15f, 1.3f, 1.5f };
    public float[] range = { 3f, 3.2f, 3.5f, 4f };

    [Header("인게임 배치 비용 (Soul)")]
    public int placeCost = 50;

    // === 신규 필드 ===

    [Header("Cannon 전용 (AoE)")]
    public float[] explosionRadius = { 1.0f, 1.2f, 1.4f, 1.6f };

    [Header("Ice 전용 (감속)")]
    public float[] slowPercent = { 0.25f, 0.40f, 0.55f, 0.65f };
    public float[] slowDuration = { 1.5f, 2.0f, 2.3f, 2.5f };

    [Header("Lightning 전용 (체인)")]
    public int[] chainCount = { 2, 3, 5, 6 };
    public float[] chainDamageDecay = { 0.30f, 0.22f, 0.15f, 0.10f };
    public float chainRange = 2.0f;

    [Header("Laser 전용 (빔)")]
    public float[] beamDuration = { 2.0f, 2.4f, 2.8f, 3.0f };
    public float[] cooldownDuration = { 3.0f, 2.6f, 2.2f, 2.0f };
    public float[] dps = { 25f, 50f, 90f, 140f };
    public int[] pierceCount = { 3, 5, 7, -1 };  // -1 = 무제한

    [Header("Void 전용 (범위 지속)")]
    public float[] aoeRadius = { 2.0f, 2.5f, 3.0f, 3.5f };
    public float[] aoeDps = { 15f, 35f, 65f, 100f };
    public bool[] ignoreArmor = { false, false, true, true };
    public float[] aoeSlowPercent = { 0f, 0f, 0.10f, 0.20f };

    // === 기존 헬퍼 메서드 ===
    public float GetDamage(int level) => damage[Mathf.Clamp(level - 1, 0, damage.Length - 1)];
    public float GetAttackSpeed(int level) => attackSpeed[Mathf.Clamp(level - 1, 0, attackSpeed.Length - 1)];
    public float GetRange(int level) => range[Mathf.Clamp(level - 1, 0, range.Length - 1)];

    // === 신규 헬퍼 메서드 ===
    public float GetExplosionRadius(int level) => GetArrayValue(explosionRadius, level);
    public float GetSlowPercent(int level) => GetArrayValue(slowPercent, level);
    public float GetSlowDuration(int level) => GetArrayValue(slowDuration, level);
    public int GetChainCount(int level) => (int)GetArrayValue(chainCount, level);
    public float GetChainDecay(int level) => GetArrayValue(chainDamageDecay, level);
    public float GetBeamDuration(int level) => GetArrayValue(beamDuration, level);
    public float GetCooldown(int level) => GetArrayValue(cooldownDuration, level);
    public float GetDPS(int level) => GetArrayValue(dps, level);
    public int GetPierceCount(int level) => (int)GetArrayValue(pierceCount, level);
    public float GetAoeRadius(int level) => GetArrayValue(aoeRadius, level);
    public float GetAoeDPS(int level) => GetArrayValue(aoeDps, level);
    public bool GetIgnoreArmor(int level) => level >= 1 && level <= ignoreArmor.Length && ignoreArmor[level - 1];
    public float GetAoeSlowPercent(int level) => GetArrayValue(aoeSlowPercent, level);

    float GetArrayValue(float[] arr, int level)
    {
        if (arr == null || arr.Length == 0) return 0f;
        return arr[Mathf.Clamp(level - 1, 0, arr.Length - 1)];
    }

    float GetArrayValue(int[] arr, int level)
    {
        if (arr == null || arr.Length == 0) return 0;
        return arr[Mathf.Clamp(level - 1, 0, arr.Length - 1)];
    }
}
```

---

## 부록 C: NodeData.cs 확장안

```csharp
[CreateAssetMenu(fileName = "Node_", menuName = "Soulspire/Node Data")]
public class NodeData : ScriptableObject
{
    // === 기존 필드 (유지) ===
    public string nodeId;
    public string nodeName;
    public NodeType type;
    public Sprite icon;
    public GameObject prefab;

    [Header("기본 스탯")]
    public float hp = 30f;
    public float speed = 2f;
    public int bitDrop = 5;
    public int damage = 1;

    // === 신규 필드 ===

    [Header("Shield 전용")]
    public float armor = 0f;            // 방어력 (0=없음)

    [Header("Regen 전용")]
    public float regenPercent = 0f;     // 초당 최대 HP의 N% 회복 (0=없음)
    public float regenTickInterval = 0.5f;

    [Header("Phase 전용")]
    public float phaseVisibleTime = 0f; // 공격 가능 시간 (0=Phase 없음)
    public float phaseInvulnTime = 0f;  // 무적 시간

    [Header("Split 전용")]
    public NodeData splitChildData;     // 분열체 NodeData (null=분열 없음)
    public int splitCount = 0;          // 분열 수
    public float splitHpRatio = 0.3f;   // 분열체 HP 비율
    public float splitSpeedMultiplier = 1.2f;

    [Header("Boss 전용")]
    public bool isBoss = false;
}
```

---

## [총괄PD 확정 사항] (v0.2에서 전건 확정)

### 의사결정 완료

| # | 항목 | 확정 결과 | 비고 |
|---|------|----------|------|
| 1 | **인벤토리 초과 시 처리** | 보물상자 열기 불가 (인벤토리 가득 차면 상자 못 연다) | 섹션 2.6, 2.7 반영 |
| 2 | **일시정지 중 타워 배치 가능 여부** | **불가** | 섹션 11 반영 |
| 3 | **보스 Incursion 대기 시간** | **5초** | 섹션 5.2 반영 |
| 4 | **첫 런 Arrow 자동 배치 위치** | **맵 정가운데** | 섹션 1.2 반영 |
| 5 | **합성 규칙** | **같은 레벨끼리만 합성 가능, 최대 Lv4** | 섹션 2.3 반영 |
| 6 | **타워 판매** | **판매 불가. 합성 비용 없음.** | 섹션 2.4 반영 |
| 7 | **분열체 재분열** | **없음 (1회만 분열)** | 섹션 4.5 반영 |
| 8 | **Floor 간 타워 배치** | **독립 (Floor별 별도 배치)** | 섹션 2.5 반영 |

### 확인 불필요 (기획팀장 자체 결정)

아래 항목들은 합리적 근거에 따라 기획팀장이 결정하였습니다.

- 감속 중첩 불가 (가장 강한 것만 적용): 밸런스 관리 용이
- 데미지 최소 1 보장 (방어력 관계없이): 완전 무적 방지
- 콤보 기준 3초/5킬: Vampire Survivors 레퍼런스 + 게임 속도 고려
- Incursion 클리어 보너스 공식 (10 + Incursion*5): 선형 증가로 예측 가능
- Laser 빔 방향은 첫 타겟 기준 (발사 중 타겟 전환 가능): 유연한 활용
- Void Tower 방어력 무시는 Lv3부터: Shield Node 대응 전략 제공 시점
- 크리티컬 확률 15%, 배율 2.0x: TD 장르 표준 범위 내
