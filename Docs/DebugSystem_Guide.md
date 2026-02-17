# Soulspire Debug System Guide

## 개요

Soulspire 디버그 시스템은 `UNITY_EDITOR` 또는 `DEVELOPMENT_BUILD` 환경에서만 활성화되는 IMGUI 기반 치트/디버그 도구입니다. F12 키로 디버그 패널을 토글하며, 콘솔 에러/워닝 카운터 오버레이는 항상 표시됩니다.

## 파일 구조

| 파일 | 역할 |
|------|------|
| `Assets/Project/Scripts/Debug/DebugManager.cs` | F12 패널 토글, 콘솔 카운터 오버레이, 싱글톤 |
| `Assets/Project/Scripts/Debug/DebugCommands.cs` | static 메서드 모음 (실제 치트 로직) |
| `Assets/Project/Scripts/Debug/DebugPanel.cs` | IMGUI 패널 UI (섹션별 접이식) |

## 사용법

### 패널 열기/닫기
- **F12** 키를 눌러 디버그 패널을 토글합니다.
- 패널은 드래그로 이동할 수 있습니다 (상단 바 드래그).

### 콘솔 카운터 오버레이
- 화면 우상단에 항상 표시됩니다 (패널 닫혀도 표시).
- `E:N` (에러), `W:N` (워닝), `L:N` (로그) 카운트를 실시간 표시합니다.
- 새 에러 발생 시 2초간 빨간색 깜빡임 효과가 나타납니다.
- "Reset Counts" 버튼으로 카운트를 초기화할 수 있습니다.

---

## 디버그 기능 (9개)

### 1. Jump to Wave (웨이브 즉시 이동)
- **위치**: 패널 섹션 "1. Jump to Wave"
- **사용법**: 웨이브 번호 입력 후 "Go" 버튼 클릭
- **동작**: 현재 스폰 중인 웨이브를 중단하고, 살아있는 노드를 모두 정리한 뒤 지정 웨이브부터 재시작
- **조건**: 런이 진행 중이어야 합니다 (InGame 상태)
- **연동**: `RunManager.Debug_SetWaveIndex()`, `WaveSpawner.Debug_StartFromWave()`

### 2. Start Specific Stage (특정 스테이지 선택 시작)
- **위치**: 패널 섹션 "2. Start Specific Stage"
- **사용법**: 스테이지 번호 입력 후 "Start" 버튼 클릭, 또는 스테이지 목록 버튼 직접 클릭
- **동작**: 해금 조건을 무시하고 지정 스테이지로 즉시 런을 시작
- **연동**: `GameManager.StartRun(stageIndex)`

### 3. GoToState (게임 상태 강제 전환)
- **위치**: 패널 섹션 "3. GoToState"
- **사용법**: Title / Hub / InGame / RunEnd 버튼 중 하나 클릭
- **동작**:
  - **Title**: 현재 상태를 정리하고 Title 상태로 전환
  - **Hub**: `GameManager.GoToHub()` 호출 (UI 전환 포함)
  - **InGame**: 현재 스테이지 인덱스로 런 시작
  - **RunEnd**: 상태값만 RunEnd로 변경 (UI 전환 없음)
- **연동**: `GameManager.Debug_SetState()`, `GameManager.GoToHub()`, `GameManager.StartRun()`

### 4. Force Treasure Chest (보물상자 강제 드롭)
- **위치**: 패널 섹션 "4. Force Treasure Chest"
- **사용법**: "Force Treasure Drop" 버튼 클릭
- **동작**: 보스 처치 시 발생하는 타워 보물상자 드롭(3택 UI)을 확률 무시하고 강제 트리거
- **조건**: TreasureManager 및 TreasureConfig가 씬에 존재해야 합니다
- **연동**: `TreasureManager.Debug_ForceDrop()` -> `TreasureManager.OnBossKilled()`

### 5. Add Core / Bit (재화 추가)
- **위치**: 패널 섹션 "5. Add Core / Bit"
- **사용법**:
  - **Core (프리미엄 재화)**: +10, +50, +100 버튼 또는 커스텀 수량 입력 후 "Add"
  - **Bit/Soul (일반 재화)**: +100, +500, +1000 버튼 또는 커스텀 수량 입력 후 "Add"
- **연동**: `MetaManager.Debug_AddCoreFragment()`, `MetaManager.Debug_AddSoul()`

### 6. Set Skill Level (스킬 레벨 설정)
- **위치**: 패널 섹션 "6. Set Skill Level"
- **사용법**:
  - 스킬 목록에서 스킬 버튼을 클릭하여 선택 (현재 레벨/최대 레벨 표시)
  - Skill ID와 레벨 값을 입력 후 "Set" 버튼 클릭
  - "All Skills MAX": 모든 스킬을 최대 레벨로 설정
  - "All Skills Reset (Lv 0)": 모든 스킬을 레벨 0으로 초기화
- **연동**: `MetaManager.Debug_SetSkillLevel()`

### 7. FTUE Flags (FTUE 플래그 제어)
- **위치**: 패널 섹션 "7. FTUE Flags"
- **사용법**:
  - 각 FTUE 플래그별 ON/OFF 토글 버튼
  - "Reset All FTUE Flags": 모든 FTUE 플래그 + 가이드 표시 기록 초기화
- **플래그 목록**:
  - `[0]` FirstPlay Complete (첫 플레이 완료)
  - `[1]` FirstTowerPlacement (첫 타워 배치)
  - `[2]` FirstUpgrade (첫 업그레이드)
  - `[3]` FirstStageClear (첫 스테이지 클리어)
  - `[4]` HubFirstVisit (Hub 최초 방문)
- **연동**: `MetaManager.SetFtueFlag()`, `MetaManager.Debug_ResetAllFtueFlags()`

### 8. Error Counter (콘솔 에러/워닝 카운터 오버레이)
- **위치**: 화면 우상단 (항상 표시)
- **동작**: `Application.logMessageReceived` 이벤트를 구독하여 Error, Warning, Log 카운트를 실시간 집계
- **기능**: 새 에러 발생 시 2초간 빨간색 깜빡임, "Reset Counts" 버튼으로 초기화
- **연동**: `DebugManager` 내부 구현 (별도 API 불필요)

### 9. Node Kill Count (노드 킬 카운트 조작)
- **위치**: 패널 섹션 "9. Node Kill Count"
- **사용법**:
  - 킬 카운트 직접 입력 후 "Set" 버튼
  - +100, +500 증분 버튼
  - "Reset 0" 버튼으로 0으로 초기화
- **연동**: `MetaManager.Debug_SetNodesKilled()`

---

## 코어 매니저 Debug_ API 목록

모든 Debug_ 메서드는 `#if UNITY_EDITOR || DEVELOPMENT_BUILD` 전처리기로 감싸져 있습니다 (릴리스 빌드에서 제외).

### MetaManager
| 메서드 | 설명 |
|--------|------|
| `Debug_AddCoreFragment(int amount)` | CoreFragment 재화 추가 |
| `Debug_AddSoul(int amount)` | Soul 재화 추가 |
| `Debug_SetSkillLevel(string skillId, int level)` | 스킬 레벨 강제 설정 |
| `Debug_SetNodesKilled(int count)` | totalNodesKilled 직접 설정 |
| `Debug_ResetAllFtueFlags()` | 모든 FTUE 플래그 + 가이드 기록 초기화 |

### RunManager
| 메서드 | 설명 |
|--------|------|
| `Debug_SetWaveIndex(int index)` | CurrentWaveIndex 강제 설정 |

### GameManager
| 메서드 | 설명 |
|--------|------|
| `Debug_SetState(GameState state)` | GameState 강제 전환 (UI 전환 없이 상태값만) |

### WaveSpawner
| 메서드 | 설명 |
|--------|------|
| `Debug_StartFromWave(StageData stage, int waveIndex)` | 지정 웨이브부터 스폰 재시작 |

### TreasureManager
| 메서드 | 설명 |
|--------|------|
| `Debug_ForceDrop()` | 보물상자 강제 드롭 (OnBossKilled 트리거) |

---

## 주의사항

1. **릴리스 빌드에서는 비활성화**: `#if UNITY_EDITOR || DEVELOPMENT_BUILD` 전처리기로 보호되어 릴리스 빌드에 포함되지 않습니다.
2. **세이브 데이터 변경**: 재화, 스킬, FTUE 등의 조작은 즉시 세이브 파일에 반영됩니다. 테스트 후 세이브 파일 초기화가 필요할 수 있습니다.
3. **상태 전환 시 UI**: `Debug_SetState()`는 상태값만 변경하고 UI 전환은 하지 않습니다. UI 전환이 필요한 경우 `GoToHub()` 등 정식 메서드를 사용하세요.
4. **웨이브 점프**: 런이 진행 중이 아닌 상태에서는 웨이브 점프가 작동하지 않습니다.
