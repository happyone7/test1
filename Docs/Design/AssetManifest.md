# Nodebreaker TD - Asset Manifest

| 항목 | 내용 |
|------|------|
| 문서 버전 | 0.2 |
| 최종 수정 | 2026-02-14 |
| 기준 문서 | `Docs/Design/GDD.md` v2.0 |
| 용도 | 그래픽/사운드 리소스 제작 및 검수 기준 |

---

## 1. 목적

이 문서는 다음을 고정한다.

- 제작해야 할 에셋 목록 (누락 방지)
- 에셋 규격 (해상도, 포맷, 길이, 네이밍)
- AI 생성 + 수작업 보정 파이프라인
- Unity 반입/검수 체크리스트

### 1.1 확정 의사결정 (2026-02-14)

- 기준 해상도/PPU: `1920x1080`, `PPU 100`
- 아트 방향: `연출 우선`
- 실루엣/디테일: `복잡한 디테일` 우선, 역할 식별 유지
- 스테이지 톤: `초반 차분 -> 후반 강렬` 점진 상승
- UI 방향: `깔끔/정보 우선`
- 오디오 톤: `Lo-Fi` 중심
- BGM 레이어: `3트랙 (Normal/Tension/Boss)`
- 제작 비율: `AI 80% / 수작업 20%`

---

## 2. 스타일 방향

### 2.1 비주얼 키워드

- `2D 탑다운`, `네온 회로`, `데이터/노드`, `빠른 피드백`, `연출 우선`
- 스테이지 진행에 따라 색 온도가 상승하도록 설계
- 후반으로 갈수록 화면 이펙트 밀도 증가 (도파민 설계 반영)

### 2.2 컬러 가이드

- Neutral UI: `#0B0F1A`, `#121A2A`, `#D8E4FF`
- Accent Green (초반): `#2BFF88`
- Accent Blue: `#37B6FF`
- Accent Orange: `#FF9A3D`
- Alert Red: `#FF4D5A`
- Rare/Legend Accent (후반): `#FFD84D`

### 2.3 아트 우선순위 원칙

- 1순위: 성장 체감 (강화/합성/클리어 연출)
- 2순위: 전투 가독성 (적/타워/사거리/피격 여부)
- 3순위: 장식성 (배경 디테일, 환경 연출)

---

## 3. 기술 규격

## 3.1 그래픽

- 기본 단위: `Sprite (2D)` / Pivot 중앙
- 기준 해상도: `1920x1080`
- 기준 PPU: `100` (확정)
- 텍스처 포맷: `PNG` (알파 포함)
- 애니메이션: 짧은 루프 우선 (`6~12fps` 또는 이벤트 기반)
- 아틀라스: 카테고리별 분리 (`Tower`, `Node`, `UI`, `FX`)

## 3.2 사운드

- SFX 포맷: `WAV` (제작 원본), 반입 시 `OGG` 변환 가능
- BGM 포맷: `WAV`/`OGG` (루프 메타 포함)
- 권장 샘플레이트: `48kHz`, `24-bit`(원본)
- 루프 BGM은 앞/뒤 클릭 없는 seamless 조건

## 3.3 공통 네이밍 규칙

- 패턴: `NB_{Category}_{Name}_{Variant}_{v###}`
- 예시:
- `NB_Tower_Arrow_Idle_v001`
- `NB_Node_Quick_Death_v002`
- `NB_UI_WaveClear_Popup_v001`
- `NB_SFX_Hit_Arrow_03_v001`

---

## 4. 그래픽 에셋 목록

디자인 메모:
- 타워/Node 모두 `복잡한 디테일` 방향으로 제작
- 단, 실제 플레이 거리에서 타입 구분이 가능하도록 실루엣 대비 유지

## 4.1 타워 (6종)

| ID | 이름 | 필요 리소스 | 최소 수량 |
|----|------|------------|----------|
| T01 | Arrow Tower | 본체 Idle, 공격 프레임, 투사체, 히트 이펙트, 업그레이드 VFX | 5+ |
| T02 | Cannon Tower | 본체 Idle, 발사/반동, 포탄, 폭발 VFX, 합성 강화 VFX | 6+ |
| T03 | Ice Tower | 본체 Idle, 발사 FX, 빙결 히트 VFX, 감속 디버프 아이콘 | 6+ |
| T04 | Lightning Tower | 본체 Idle, 체인 번개 FX, 히트 스파크, 강화 VFX | 6+ |
| T05 | Laser Tower | 본체 Idle, 빔 시작/유지 FX, 관통 히트 FX | 6+ |
| T06 | Void Tower | 본체 Idle, 범위 오라 FX, 틱 데미지 FX, 강화 FX | 6+ |

## 4.2 Node 적 (9종)

| ID | 이름 | 필요 리소스 | 최소 수량 |
|----|------|------------|----------|
| N01 | Bit Node | 이동, 피격, 파괴 | 3+ |
| N02 | Quick Node | 이동(빠른 모션), 피격, 파괴 | 3+ |
| N03 | Heavy Node | 이동(무거운 모션), 피격, 파괴 | 3+ |
| N04 | Shield Node | 이동, 피격, 방어막 피격 FX, 파괴 | 4+ |
| N05 | Swarm Node | 이동, 피격, 파괴(소형) | 3+ |
| N06 | Regen Node | 이동, 재생 FX 루프, 피격, 파괴 | 4+ |
| N07 | Phase Node | 이동, 무적 상태 FX, 피격, 파괴 | 4+ |
| N08 | Split Node | 이동, 피격, 분열 FX, 파괴 | 4+ |
| N09 | Boss Node | 이동, 피격, 특수 피격, 파괴(대형), 경고 연출 | 5+ |

## 4.3 스테이지 환경 (10종 테마)

| Stage | 이름 | 핵심 배경 리소스 |
|-------|------|------------------|
| 1 | Data Stream | 기본 타일, 회로 라인, 경로 하이라이트 |
| 2 | Memory Block | 격자 패턴, 블록형 장식, 경로 포인트 |
| 3 | Cache Layer | 보라 네온 오버레이, 캐시 노이즈 패턴 |
| 4 | Pipeline | 다중 경로 시각화 타일, 분기 마커 |
| 5 | Processor Core | 붉은 코어 오브젝트, 열기/펄스 오버레이 |
| 6 | Bus Network | 밝은 버스 라인, 교차 노드 타일 |
| 7 | Kernel Space | 심도감 배경, 보라 계열 경고 패턴 |
| 8 | Overflow Zone | 붉은/검은 파열 텍스처, 글리치 오버레이 |
| 9 | Root Access | 금색 회로, 고급 테두리 장식 |
| 10 | Kernel Panic | 전 테마 혼합, 최종 경고 오버레이 |

## 4.4 UI 에셋

- 공통 HUD:
- 상단 바 배경, HP 바 프레임/필, Bit/Core 아이콘, 웨이브 배지
- 인게임:
- 타워 인벤토리 슬롯, 드래그 하이라이트(배치/합성/판매), 배속 버튼(x1/x2/x3), 일시정지 버튼
- Hub:
- 스킬 노드(잠김/해금가능/해금완료), 연결선(비활성/활성), 노드 구매 팝업 프레임
- 결과 화면:
- `RUN COMPLETE`, `STAGE CLEAR` 배너, Core 보상 카드, 신규 해금 배지
- 시스템:
- 툴팁 박스, 버튼 상태(Idle/Hover/Pressed/Disabled), 경고 테두리(저HP)

## 4.5 VFX 에셋

GDD 도파민 설계(마이크로~특대)에 맞춰 계층화.

- 마이크로: 적 처치 파편, Bit 숫자 팝업
- 소형: 웨이브 클리어 플래시
- 중형: 영구 업그레이드 구매 이펙트
- 대형: 스테이지 클리어, Core 획득 연출
- 특대: 신규 타워 첫 사용 연출, 보스 처치 연출, 짧은 슬로우모션 강조 FX

---

## 5. 사운드 에셋 목록

## 5.1 SFX 이벤트 테이블

| 카테고리 | 이벤트 | 변형 수량 목표 |
|----------|--------|---------------|
| Combat | 타워 발사(타워별) | 각 3~5 |
| Combat | 타워 히트(타워별) | 각 3~5 |
| Combat | Node 사망(일반/엘리트/보스) | 각 3~5 |
| Combat | 연속 처치 콤보 알림 | 3 |
| Progress | 웨이브 시작/클리어 | 각 2~3 |
| Progress | 스테이지 클리어/Core 획득 | 각 2~3 |
| Meta | 업그레이드 구매/노드 해금 | 각 3 |
| UI | 버튼, 탭 전환, 에러, 확인 | 각 3 |
| Alert | 기지 저HP 경고, 보스 경고 | 각 2~3 |

## 5.2 BGM 구성

- Hub: 1트랙 (집중도 낮은 반복)
- InGame: 스테이지군별 3트랙
- 전투 레이어: `Normal` / `Tension` / `Boss`
- 전환 방식: 크로스페이드 0.5~1.5초

## 5.3 오디오 믹스 기준

- 톤 방향: `Lo-Fi` (과도한 하이엔드/자극성 억제)

- SFX 버스 피크: `-6dB` 이하
- BGM 버스 피크: `-10dB` 이하
- UI 클릭은 전투 중에도 묻히지 않는 중고역 확보
- 장시간 플레이 피로 방지를 위해 2~4kHz 대역 과자극 금지

---

## 6. AI 제작 워크플로

## 6.1 기본 원칙

- AI는 `대량 초안 생성`, 사람은 `최종 방향 확정/정제`
- 제작 비율 목표: `AI 80% / 수작업 20%`
- 카테고리마다 스타일 레퍼런스 1세트만 고정
- 모델/프롬프트/버전 기록을 남겨 재현성 확보

## 6.2 그래픽 워크플로

1. 스타일 탐색: 타워/Node 각각 20~40개 시안 생성
2. 스타일 락: 1개 방향 선택, 금지 요소 정의
3. 본생산: ID 단위로 베리에이션 제작
4. 보정(20% 수작업): 실루엣/가독성/색상 대비/피벗 통일
5. 반입: Unity Import Preset + Atlas 적용
6. 플레이테스트: 전투 중 인식 속도/가독성 검수

## 6.3 사운드 워크플로

1. 이벤트 목록 고정
2. 이벤트별 3~5개 샘플 생성
3. 톤 통일 (Lo-Fi 톤 기반 EQ/컴프/노이즈 컷)
4. 런타임 랜덤 재생 규칙 연결
5. 믹스 밸런스 조정

## 6.4 프롬프트 템플릿 (초안)

### 그래픽 (타워)

```text
Top-down 2D game sprite for a cyber neon tower defense.
Object: {TowerName}
Style: complex detailed design, strong silhouette readability at gameplay distance, emissive circuit details.
Background: transparent.
No text, no watermark, no UI frame.
Color bias: {Palette}.
```

### 그래픽 (적)

```text
Top-down 2D enemy sprite for cyber data-themed tower defense.
Enemy type: {NodeName}, role: {RoleKeyword}.
Readable shape language, strong contrast against dark map.
Background: transparent.
No text, no watermark.
```

### 사운드 (SFX)

```text
Create a short game SFX for {EventName}.
Length: {0.1~1.2}s, punchy transient, minimal tail.
Tone: lo-fi cyber/electronic, soft high-end, layer-friendly.
Deliver 3 variations with similar character.
```

---

## 7. Unity 반입 구조 (권장)

```text
Assets/
  Art/
    Towers/
    Nodes/
    Stages/
    UI/
    FX/
  Audio/
    BGM/
    SFX/
  Data/
    ScriptableObjects/
```

---

## 8. 검수 체크리스트

## 8.1 그래픽

- [ ] 1초 이내 식별 가능 (타워/적 타입 구분)
- [ ] 탑다운 시점 일관성 유지
- [ ] 동일 계열 에셋의 색/광원 규칙 통일
- [ ] 전투 이펙트 과밀 시에도 핵심 정보(HUD, 적 경로) 가림 없음
- [ ] Pivot/PPU/슬라이스 규격 통일

## 8.2 사운드

- [ ] 동일 이벤트 내 변형 간 음량 편차 과도하지 않음
- [ ] BGM 루프 클릭/단절 없음
- [ ] 전투 중 UI/경고음이 묻히지 않음
- [ ] 장시간 청취 피로도 점검 완료

## 8.3 라이선스/리스크

- [ ] 사용 모델/플랜의 상업 이용 가능 여부 확인
- [ ] 생성 로그(모델/프롬프트/날짜) 저장
- [ ] 특정 IP 유사도 점검 완료

---

## 9. 우선 제작 백로그 (MVP 기준)

1. Arrow/Bit/Quick/Heavy + 공통 HUD + 기본 처치 VFX/SFX
2. 스테이지 1/2 배경 + 웨이브/런 종료 UI
3. Cannon/Ice + Shield/Swarm + 클리어 연출
4. Hub 스킬 트리 노드 상태 3종 + 구매 연출
5. 후반 타워(Lightning/Laser/Void), 보스 연출, 고급 BGM 레이어

---

## 10. 오픈 이슈 (수정 필요)

- 실제 타일맵 셀 크기와 스프라이트 크기 비율 확정
- AI 생성 툴 체인(이미지/오디오) 최종 선정
- 외주/내부 제작 분담 범위 확정 (내부 수작업 20% 기준)
