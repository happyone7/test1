# Soulspire - UI Assets Guide v0.1

| 항목 | 내용 |
|------|------|
| 문서 버전 | 0.1 |
| 최종 수정 | 2026-02-15 |
| 작성자 | TA팀장 (아트디렉터) |
| 대상 독자 | UI팀장 |
| 기준 문서 | `ArtDirection_v0.1.md` 섹션 4.1, 5.4 |

---

## 1. 에셋 폴더 구조

```
Assets/Art/UI/
├── Frames/
│   ├── panel_frame.png        (64x64)  - 범용 패널 9-slice
│   ├── hp_bar_frame.png       (200x16) - HP 바 외곽 프레임
│   ├── hp_bar_fill.png        (196x12) - HP 바 채움 이미지
│   ├── tower_slot.png         (48x48)  - 타워 인벤토리 슬롯
│   ├── tooltip_frame.png      (48x48)  - 툴팁 9-slice
│   └── dropdown_frame.png     (64x32)  - 드롭다운 9-slice
├── Buttons/
│   ├── btn_basic_idle.png     (48x24)  - 돌 버튼 기본
│   ├── btn_basic_hover.png    (48x24)  - 돌 버튼 호버
│   ├── btn_basic_pressed.png  (48x24)  - 돌 버튼 눌림
│   ├── btn_basic_disabled.png (48x24)  - 돌 버튼 비활성
│   ├── btn_accent_idle.png    (48x24)  - 금박 버튼 기본
│   ├── btn_accent_hover.png   (48x24)  - 금박 버튼 호버
│   ├── btn_accent_pressed.png (48x24)  - 금박 버튼 눌림
│   └── btn_accent_disabled.png(48x24)  - 금박 버튼 비활성
└── Icons/
    ├── icon_bit.png           (24x24)  - 소울(Bit) 아이콘
    └── icon_core.png          (24x24)  - 정수(Core Fragment) 아이콘
```

---

## 2. 에셋별 상세 사용 가이드

### 2.1 패널 프레임 (`panel_frame.png`)

| 항목 | 값 |
|------|-----|
| 크기 | 64x64 px |
| 용도 | 모든 UI 패널의 배경 프레임 (HubUI, InGameUI, 팝업 등) |
| Image Type | **Sliced** (9-slice) |
| 9-slice Border | Left: 6, Right: 6, Top: 6, Bottom: 6 |
| Fill Center | true |

**사용법:**
1. Unity에서 Sprite Editor 열기
2. Border를 L:6 R:6 T:6 B:6 으로 설정
3. Image 컴포넌트의 Image Type을 "Sliced"로 변경
4. 어떤 크기로 늘려도 모서리 룬 장식과 테두리가 유지됨

**디자인 메모:**
- 석재+금박 테두리. 4개 모서리에 작은 금색 룬 장식.
- 상단/하단 중앙에 금색 악센트 라인.
- 내부 패널 색상: `#1A1828` (UI 패널 기본)
- 좌상단 광원 기준 하이라이트/섀도 처리.

---

### 2.2 버튼 프레임 - 기본 (`btn_basic_*.png`)

| 항목 | 값 |
|------|-----|
| 크기 | 48x24 px (각 상태) |
| 용도 | 일반 액션 버튼 (닫기, 취소, 일반 선택 등) |
| Image Type | **Sliced** (9-slice) |
| 9-slice Border | Left: 4, Right: 4, Top: 4, Bottom: 4 |
| 상태 수 | 4 (Idle / Hover / Pressed / Disabled) |

**Button 컴포넌트 설정:**

| Transition | Sprite Swap |
|------------|-------------|
| Normal Sprite | `btn_basic_idle` |
| Highlighted Sprite | `btn_basic_hover` |
| Pressed Sprite | `btn_basic_pressed` |
| Disabled Sprite | `btn_basic_disabled` |

**상태별 시각 차이:**
- **Idle**: 어두운 석재 톤, 기본 프레임 (`#5A5070`)
- **Hover**: 밝아진 석재, 금색 활성 프레임 (`#B0A080`)
- **Pressed**: 어두워진 석재, 하이라이트/섀도 반전 (눌림 효과)
- **Disabled**: 전체적으로 어둡고 낮은 대비, 프레임 색 흐림

---

### 2.3 버튼 프레임 - 강조/금박 (`btn_accent_*.png`)

| 항목 | 값 |
|------|-----|
| 크기 | 48x24 px (각 상태) |
| 용도 | 주요 액션 버튼 (구매, 확인, 시작, 업그레이드 등) |
| Image Type | **Sliced** (9-slice) |
| 9-slice Border | Left: 4, Right: 4, Top: 4, Bottom: 4 |
| 상태 수 | 4 (Idle / Hover / Pressed / Disabled) |

**Button 컴포넌트 설정:**

| Transition | Sprite Swap |
|------------|-------------|
| Normal Sprite | `btn_accent_idle` |
| Highlighted Sprite | `btn_accent_hover` |
| Pressed Sprite | `btn_accent_pressed` |
| Disabled Sprite | `btn_accent_disabled` |

**상태별 시각 차이:**
- **Idle**: 금색 테두리 (`#FFD84D`), 4모서리에 금색 악센트 도트
- **Hover**: 더 밝은 금색, 전체적으로 발광 느낌 증가
- **Pressed**: 어두운 금색, 하이라이트/섀도 반전
- **Disabled**: 기본 버튼 Disabled와 동일 (금색 제거, 무채색)

**사용 기준:**
- 골드(Core Fragment) 소모 행동 -> 금박 버튼
- 소울(Bit) 소모 행동 -> 금박 버튼
- 단순 네비게이션/취소 -> 기본 버튼

---

### 2.4 HP 바 프레임 (`hp_bar_frame.png`)

| 항목 | 값 |
|------|-----|
| 크기 | 200x16 px |
| 용도 | 기지 HP 바 외곽 프레임 (InGameUI 상단) |
| Image Type | **Simple** (늘리지 않고 그대로 사용 권장) |
| Preserve Aspect | true |

**디자인 메모:**
- 석재 테두리, 2px 보더
- 양쪽 끝에 금색 악센트 도트 (룬 느낌)
- 내부 영역: 196x12 px (hp_bar_fill이 들어갈 공간)
- 내부 배경색: `#12101A`

---

### 2.5 HP 바 채움 (`hp_bar_fill.png`)

| 항목 | 값 |
|------|-----|
| 크기 | 196x12 px |
| 용도 | HP 바 내부 채움 이미지 |
| Image Type | **Filled** |
| Fill Method | Horizontal |
| Fill Origin | Left |

**사용법:**
1. HP 바 프레임 내부에 자식 오브젝트로 배치
2. RectTransform: Left:2, Right:2, Top:2, Bottom:2 (프레임 안쪽에 맞춤)
3. Image의 Fill Amount를 0~1로 조절하여 HP 표시
4. Fill Amount = 1.0 -> 전체 녹색 (체력 만땅)
5. Fill Amount = 0.5 -> 중간 황색 (체력 절반)
6. Fill Amount < 0.2 -> 적색 부분만 표시 (위험)

**그래디언트 구성:**
- 좌측(0.0): 에메랄드 그린 `#40D470`
- 중앙(0.5): 옐로우 `#E0D040`
- 우측(1.0): 루비 레드 `#D44040`
- 상단에 밝은 하이라이트 라인 (입체감)

**주의:** 이 이미지는 왼쪽부터 채워지도록 설계됨. HP가 줄어들면 오른쪽(적색)부터 사라지므로, 남은 HP가 적을수록 녹색만 보여 직관적.

---

### 2.6 소울(Bit) 아이콘 (`icon_bit.png`)

| 항목 | 값 |
|------|-----|
| 크기 | 24x24 px |
| 용도 | 소울(Bit) 자원 표시 아이콘 |
| Image Type | Simple |
| Preserve Aspect | true |
| Native Size 사용 | 권장 |

**디자인:**
- 에메랄드 그린 (`#40D470`) 보석 형태
- 6각형 팩싯 컷 다이아몬드/보석 모양
- 좌상단 하이라이트 (광원 방향)
- 외곽에 미세한 녹색 글로우 (1px)
- 1px 검정 외곽선

**사용처:**
- InGameUI 상단 자원 표시 영역
- Incursion 클리어 보상 팝업
- 타워 구매 비용 표시
- 숫자 팝업 ("+3" 옆에 아이콘)

---

### 2.7 정수(Core Fragment) 아이콘 (`icon_core.png`)

| 항목 | 값 |
|------|-----|
| 크기 | 24x24 px |
| 용도 | 정수(Core Fragment) 프리미엄 자원 표시 아이콘 |
| Image Type | Simple |
| Preserve Aspect | true |
| Native Size 사용 | 권장 |

**디자인:**
- 골드 (`#FFD84D`) 팔각형 룬석
- 내부에 십자형 룬 문양 (미세)
- 좌상단 하이라이트
- 외곽에 미세한 금색 글로우 (1px)
- 1px 검정 외곽선

**사용처:**
- HubUI 상단 자원 표시
- 스킬 트리 업그레이드 비용
- Floor 클리어 보상

---

### 2.8 타워 인벤토리 슬롯 (`tower_slot.png`)

| 항목 | 값 |
|------|-----|
| 크기 | 48x48 px |
| 용도 | 인게임 하단 타워 선택 바의 개별 슬롯 |
| Image Type | **Simple** (고정 크기 사용 권장) |
| 9-slice 가능 | Border L:5 R:5 T:5 B:5 (필요 시) |

**디자인:**
- 2px 석재 테두리 (이중 보더: 밝은 + 어두운)
- 내부 배경: `#12101A`
- 4모서리에 금색 악센트 도트
- 좌상단 미세 하이라이트 (입체감)
- 상단 테두리에 도트 패턴 (석재 질감 힌트)

**사용법:**
- 슬롯 내부(5px 패딩)에 타워 아이콘 스프라이트 배치
- 선택된 슬롯은 코드에서 Color tint를 밝게 하거나, 별도 선택 오버레이 추가
- 구매 불가 상태: 전체 알파 50%로 처리 권장

---

### 2.9 툴팁 프레임 (`tooltip_frame.png`)

| 항목 | 값 |
|------|-----|
| 크기 | 48x48 px |
| 용도 | 타워 정보, 스킬 정보 등 작은 팝업 프레임 |
| Image Type | **Sliced** (9-slice) |
| 9-slice Border | Left: 4, Right: 4, Top: 4, Bottom: 4 |

**디자인:**
- panel_frame의 경량 버전
- 배경 알파 230/255 (약간 반투명)
- 단일 석재 테두리 (panel보다 얇음)

---

### 2.10 드롭다운 프레임 (`dropdown_frame.png`)

| 항목 | 값 |
|------|-----|
| 크기 | 64x32 px |
| 용도 | 드롭다운/리스트 선택 패널 |
| Image Type | **Sliced** (9-slice) |
| 9-slice Border | Left: 4, Right: 4, Top: 4, Bottom: 4 |

---

## 3. Unity Import 설정 (공통)

모든 UI 에셋에 아래 설정을 적용해야 합니다.

| 설정 | 값 | 이유 |
|------|-----|------|
| Texture Type | Sprite (2D and UI) | UI 에셋 |
| Sprite Mode | Single | 개별 스프라이트 |
| Pixels Per Unit | 100 (기본) | UI Canvas 기준 |
| Filter Mode | **Point (no filter)** | 픽셀 아트 선명도 유지 필수 |
| Compression | **None** | 픽셀 깨짐 방지 |
| Max Size | 스프라이트 원본 크기 이상 | 다운스케일 금지 |
| Read/Write | 필요 시만 체크 | 기본 off |

**중요:** Filter Mode를 Bilinear로 두면 픽셀 아트가 뿌옇게 번지므로, 반드시 **Point**로 설정하세요.

---

## 4. 9-slice 설정 요약

| 에셋 | Left | Right | Top | Bottom |
|------|------|-------|-----|--------|
| panel_frame | 6 | 6 | 6 | 6 |
| tooltip_frame | 4 | 4 | 4 | 4 |
| dropdown_frame | 4 | 4 | 4 | 4 |
| btn_basic_* | 4 | 4 | 4 | 4 |
| btn_accent_* | 4 | 4 | 4 | 4 |

---

## 5. 컬러 팔레트 참조 (UI 관련)

UI 코드에서 하드코딩 색상을 사용할 경우 아래 값을 참조하세요.

| 용도 | Hex | RGB |
|------|-----|-----|
| UI 배경 | `#12101A` | (18, 16, 26) |
| UI 패널 | `#1A1828` | (26, 24, 40) |
| UI 밝은 패널 | `#242236` | (36, 34, 54) |
| UI 프레임 (기본) | `#5A5070` | (90, 80, 112) |
| UI 프레임 (활성) | `#B0A080` | (176, 160, 128) |
| UI 프레임 (강조) | `#FFD84D` | (255, 216, 77) |
| 텍스트 메인 | `#E0DCD0` | (224, 220, 208) |
| 텍스트 서브 | `#A09890` | (160, 152, 144) |
| 텍스트 비활성 | `#605850` | (96, 88, 80) |
| 소울(Bit) 녹색 | `#40D470` | (64, 212, 112) |
| 정수(Core Fragment) 금색 | `#FFD84D` | (255, 216, 77) |
| HP 위험 적색 | `#D44040` | (212, 64, 64) |

---

## 6. 추후 추가 예정 에셋

스프린트 2 이후 TA팀에서 추가 제공 예정:

| 에셋 | 스프린트 | 비고 |
|------|---------|------|
| 스킬 노드 프레임 (3상태) | 2 | 잠김/해금가능/완료 |
| 스킬 연결선 (2상태) | 2 | 비활성/활성, 타일러블 |
| 토스트/배너 프레임 | 2 | "WAVE CLEAR" 등 |
| 드래그 하이라이트 (3색) | 2 | 배치가능/합성/불가 |
| 타이틀 로고 | 2 | "Soulspire" 판타지 로고 |
| 스킬 아이콘 18종 | 2 | 48x48 개별 아이콘 |

---

## 7. 재생성 방법

에셋 수정이 필요한 경우 생성 스크립트를 사용할 수 있습니다.

```bash
python3 Tools/generate_ui_assets.py
```

스크립트 위치: `/Tools/generate_ui_assets.py`
- 색상 팔레트, 형태, 크기 등 모든 파라미터가 코드에 정의되어 있음
- 수정 후 재실행하면 동일 경로에 덮어쓰기됨
