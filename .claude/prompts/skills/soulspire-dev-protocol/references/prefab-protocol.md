# 프리팹/씬 관리 프로토콜

## 1. 프리팹 vs 씬 오브젝트 구분 기준

| 기준 | 프리팹으로 만들 것 | 씬에 남겨둘 것 |
|------|---------------------|----------------|
| 재사용성 | 2개 이상 씬에서 사용 | 해당 씬에서만 존재 |
| 동적 생성 | Instantiate로 생성 | 절대 동적 생성 안 됨 |
| 팀 충돌 | 여러 사람이 수정 가능 | 씬 오너만 수정 |
| 데이터 | SO로 설정값 분리 가능 | 씬 고유 설정 |

### 프리팹이어야 하는 것
- 타워, 몬스터, 발사체, 이펙트
- UI 패널 (HUD, 팝업, 메뉴 등)
- 반복 배치되는 오브젝트 (타일, 장애물 등)

### 씬에 남겨둘 것
- Main Camera, Directional Light
- EventSystem
- Canvas (루트, 프리팹 컨테이너 역할만)
- 전역 매니저 오브젝트 (GameManager, AudioManager 등)

## 2. 프리팹 명명 규칙

```
{카테고리}_{이름}.prefab

예시:
Tower_Arrow.prefab           # 타워
Monster_Brute.prefab         # 몬스터
Projectile_Arrow.prefab      # 발사체
UI_HealthBar.prefab          # UI 요소
VFX_Explosion.prefab         # 이펙트
```

## 3. Prefab Variant 계층 (우리 프로젝트)

```
Tower_Base.prefab            # 베이스 프리팹
  ├── Tower_Arrow.prefab     # Variant - 빠른 속도, 단일 타겟
  ├── Tower_Cannon.prefab    # Variant - 느림, 범위 공격
  └── Tower_Magic.prefab     # Variant - 중간, 디버프

Monster_Base.prefab
  ├── Monster_Soul.prefab    # Variant
  ├── Monster_Charger.prefab # Variant
  └── Monster_Brute.prefab   # Variant
```

### Nested Prefab 사용
```
Tower_Arrow.prefab
  └── Contains: ProjectileSpawnPoint (별도 프리팹)
  └── Contains: RangeIndicator (별도 프리팹)
```

### 깊이 제한
- 최대 3단계: Root > Child > GrandChild
- Override는 반드시 Apply 또는 Revert (방치 금지)

## 4. 폴더 구조

```
Assets/Project/Prefabs/
├── Towers/
│   ├── Tower_Arrow.prefab
│   └── Tower_Arrow_Lv2.prefab  (Variant)
├── Monsters/
│   ├── Monster_Soul.prefab
│   ├── Monster_Charger.prefab
│   └── Monster_Brute.prefab
├── Projectiles/
├── Effects/
└── UI/
    ├── Panels/
    │   ├── UI_TowerPurchasePanel.prefab
    │   ├── UI_RunEndPanel.prefab
    │   └── UI_SettingsPopup.prefab
    ├── HUD/
    │   ├── UI_HealthBar.prefab
    │   ├── UI_WaveCounter.prefab
    │   └── UI_InventoryBar.prefab
    └── Common/
        ├── UI_Button.prefab
        └── UI_Tooltip.prefab
```

## 5. 씬 편집 프로토콜

### 씬 수정 전 반드시 확인
```
1. "이 작업이 반드시 씬 파일을 수정해야 하는가?"
   → 프리팹만으로 해결 가능하면 씬 건드리지 않기

2. "다른 에이전트가 이 씬을 수정 중인가?"
   → Sprint Progress 문서 확인
   → 수정 중이면 해당 에이전트 작업 완료 후 진행

3. "씬에 추가할 오브젝트가 프리팹인가?"
   → 아니면 먼저 프리팹으로 만든 후 씬에 배치
```

### 씬 편집 후 반드시 확인
```
1. 테스트용으로 추가한 오브젝트 제거했는가?
2. 새 오브젝트가 프리팹화 되어있는가?
3. 불필요한 오브젝트 참조가 끊어진 것은 없는가?
4. 즉시 커밋했는가? (씬 파일은 반드시 작업 직후 커밋)
```

## 6. 하지 말아야 할 것

- 씬에 오브젝트를 직접 만들고 프리팹화하지 않기 → 반드시 프리팹 먼저
- Material을 Prefabs 폴더에 두기 → `Art/Materials/`로 분리
- 씬 파일을 여러 명이 동시 수정 → 한 시점에 한 사람만
- 프리팹 Override를 Apply하지 않고 방치 → 충돌 원인
- 작업 후 커밋을 미루기 → 즉시 커밋 필수
- 다른 팀장의 담당 폴더 파일 수정 → 파일 소유권 확인 필수
