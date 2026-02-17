# Sprint 3 진행 현황

**프로젝트**: Soulspire
**기준 문서**: `Docs/DevPD_Guidelines.md`, `Docs/Design/GDD.md` (v2.0)
**최종 업데이트**: 2026-02-15
**상태**: Phase 3 작업 완료, 일부 머지 대기

---

## 범위 확정
- Sprint 3는 **3개 Phase**로 운영 (Phase 1 ~ Phase 3)
- 포함:
  - 4종 타워 시스템 구현 (Cannon, Ice, Quick, Heavy)
  - 스킬트리 UI 시스템 (18노드, 줌/패닝, 구매 팝업)
  - 난이도 곡선 설계 및 웨이브 데이터 (스테이지 1~3, SO 40개)
  - 스킬트리 노드 아이콘 스프라이트 8종
  - Tilemap 4레이어 시스템 + 맵 레이아웃 설계
  - 타이틀 로고 재제작
  - BGM/SFX 트리거 QA 및 리소스 교체
  - Hub/InGame 초기 UI, 타워/스킬/웨이브 정보 패널
  - QA 시트 68개 항목 작성 및 검증
- 브랜칭: QA 게이트 방식 (`sprint3` 메인 + `dev/*` 팀장 브랜치)

---

## Git 브랜칭 정책 (Sprint 3부터 적용)
- 커밋 `46e0466`에서 QA 게이트 방식 수립
- `sprint3` 브랜치: QA팀장만 머지 가능
- 각 팀장은 `dev/*` 브랜치에서 작업 후 QA 검증 통과 시 머지

---

## Phase 1 — 완료

### 기획팀장 (game-designer) -- 커밋 4d17553
- [x] 4종 타워 SO 데이터 작성 (Cannon, Ice, Quick, Heavy) -- 타워별 공격력/사거리/쿨다운/비용 등 수치 설정

### UI팀장 (unity-ui-developer) -- 커밋 5291b00
- [x] Hub 화면 초기 UI 구현 -- 스킬/타워/출격 패널
- [x] InGame 화면 초기 UI 구현 -- 타워/스킬/웨이브 정보 패널

### 사운드 디렉터 (unity-sound-director) -- 커밋 a56ef99
- [x] BGM/SFX 트리거 QA 및 리소스 교체 -- 기존 사운드 리소스 점검 및 교체

### 프로그래밍팀장 (unity-gameplay-programmer) -- 커밋 c4d96b0
- [x] Tower SO 데이터 연결 시스템 구현 -- SO 기반 타워 스폰 로직
- [x] 4종 타워 구현 (Cannon/Ice/Quick/Heavy) -- 각 타워 고유 공격 로직

### TA팀장 (unity-technical-artist) -- 커밋 a0540b4
- [x] 4종 타워 스프라이트 제작 -- 다크 판타지 스타일 픽셀아트
- [x] 몬스터 스프라이트 제작 -- 기존 몬스터 스프라이트 갱신
- [x] Soulspire 타이틀 로고 전면 재제작 -- 커밋 0e32a8f

---

## Phase 2 — 완료 (sprint3 머지 완료)

### 기획팀장 (game-designer) GD-3 -- 커밋 014694a
- [x] 스테이지 1~3 난이도 곡선 설계 -- 몬스터 구성/밀도/패턴별 난이도 설계
- [x] 웨이브 데이터 SO 40개 입력 -- 스테이지별 웨이브 구성 데이터
- [x] sprint3 머지 완료 -- 커밋 0aea38d (QA 통과)

### TA팀장 (unity-technical-artist) TA-3 -- 커밋 daff3af
- [x] 스킬트리 노드 아이콘 스프라이트 8종 생성 -- ComfyUI로 다크 판타지 스타일 아이콘 제작
- [x] 투사체 스프라이트 PPU 수정 (1024 -> 64) -- 커밋 67150de, 인게임 가시성 확보
- [x] 타워/몬스터 스프라이트 배경 투명화 처리 -- 커밋 d4593d7
- [x] sprint3 머지 완료 -- 커밋 11282c8 (QA 통과)

### UI팀장 (unity-ui-developer) UI-3 -- 커밋 d810a9d, e8b7b88
- [x] 스킬트리 UI 시스템 구현 -- 18노드 배치, 줌/패닝, 연결선, 구매 팝업
- [x] 스킬트리 코드 파일 복원 및 씬 계층구조 생성 -- Unity 리버트 대응 (커밋 e8b7b88)
- [x] sprint3 머지 완료 -- 커밋 45613ec (QA 통과)

### 프로그래밍팀장 (unity-gameplay-programmer) -- 커밋 1a5ea71
- [x] 인게임 UI 미표시 버그 수정 -- UI 활성화 로직 수정
- [x] Hub 스킬 구매 플로우 개선 -- 구매 확인/취소 로직 보완

### QA팀장 (unity-qa-engineer) -- 커밋 79ce0d1, 101b84b
- [x] Sprint 3 QA 시트 작성 -- 68개 테스트 항목, 9개 카테고리
- [x] Phase 2 QA 검증 -- dev/ui, dev/game-designer(GD-3), dev/ta 브랜치 검증 통과
- [x] Phase 2 sprint3 머지 3건 완료 -- 커밋 45613ec, 0aea38d, 11282c8

---

## Phase 3 — 작업 완료, 머지 대기

### 프로그래밍팀장 (unity-gameplay-programmer) PG-4 -- 커밋 e7ce719, a4d3abf
- [x] Tilemap 4레이어 시스템 구현 -- Ground/Path/Obstacle/Decoration 레이어
- [x] NodeData에 defense 필드 추가 및 데미지 계산 반영
- [ ] **sprint3 머지 대기** -- QA 검증 필요

### 기획팀장 (game-designer) GD-4 -- 커밋 383865b
- [x] 스테이지 1 맵 레이아웃 설계 (MapLayout_Stage1.md) -- 16x12 그리드, 경로/장애물/타워 배치 정의
- [ ] **sprint3 머지 대기** -- QA 검증 필요

---

## 미착수 / 대기

- [ ] Phase 3 dev/programmer (PG-4) sprint3 머지 -- 선행: QA 검증 통과
- [ ] Phase 3 dev/game-designer (GD-4) sprint3 머지 -- 선행: QA 검증 통과
- [ ] 통합 QA -- 선행: 모든 dev/* 브랜치 sprint3 머지 완료
- [ ] 빌드 -- 선행: 통합 QA 통과 + 총괄PD 승인

---

## 완료 기준 체크리스트

- [x] 4종 타워(Cannon/Ice/Quick/Heavy) SO 데이터 + 코드 구현 완료
- [x] Hub/InGame 초기 UI 동작
- [x] 스킬트리 UI 18노드 배치 + 줌/패닝 + 구매 팝업 동작
- [x] 난이도 곡선 설계 + 웨이브 SO 40개 입력 완료
- [x] 스킬트리 노드 아이콘 8종 스프라이트 적용
- [x] 타이틀 로고 재제작 완료
- [x] BGM/SFX 트리거 QA 완료
- [x] Tilemap 4레이어 시스템 구현 완료
- [x] 맵 레이아웃 설계 문서 완료
- [x] QA 시트 68개 항목 작성 완료
- [x] Phase 2 QA 검증 + sprint3 머지 3건 완료
- [ ] Phase 3 QA 검증 + sprint3 머지 2건 (대기)
- [ ] 통합 QA 통과
- [ ] 총괄PD 빌드 승인

---

## 블로커
- **Phase 3 머지 대기**: dev/programmer (PG-4), dev/game-designer (GD-4) 2개 브랜치가 아직 sprint3에 머지되지 않음. QA팀장 검증 후 머지 필요.

---

## 스프린트 현황 요약

- 완료: 16건 / 진행중: 0건 / 대기: 4건 (머지 2건, 통합 QA 1건, 빌드 1건)
- 주요 진전: Phase 1~2 전체 완료 + Phase 2 sprint3 머지 3건 완료, Phase 3 작업 자체는 완료
- 블로커: Phase 3 dev/programmer, dev/game-designer 2개 브랜치 sprint3 머지 대기 (QA 검증 필요)
- 다음 예정: Phase 3 QA 검증 + sprint3 머지 -> 통합 QA -> 총괄PD 승인 -> 빌드

---

## 주요 결정 사항

| 날짜 | 내용 | 비고 |
|------|------|------|
| 2026-02-15 | QA 게이트 방식 Git 브랜칭 정책 수립 | 커밋 46e0466, sprint3 브랜치 도입 |
| 2026-02-15 | Sprint 3 QA 시트 68개 항목 작성 | 9개 카테고리, 커밋 79ce0d1 |
| 2026-02-15 | Phase 2 3개 브랜치 QA 통과 후 sprint3 머지 | UI/기획/TA 브랜치 머지 완료 |

---

## 운영 메모
- Sprint 3부터 QA 게이트 방식 적용: 모든 dev/* 브랜치는 QA 통과 후에만 sprint3에 머지
- 개발PD는 실무(코딩/UI/아트/빌드)를 수행하지 않고 조율/추적만 수행
- Phase 1 커밋은 feature/phase1-core-loop 브랜치에 직접 커밋 (Sprint 3 초기, 브랜칭 정책 수립 전)
- Phase 2부터 dev/* 브랜치 분리 운영
