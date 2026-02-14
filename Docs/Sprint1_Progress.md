# Sprint 1 진행 현황

**프로젝트**: Soulspire (구 Nodebreaker TD)
**브랜치**: feature/phase1-core-loop
**최종 업데이트**: 2026-02-15

---

## 스프린트 목표
코어 전투 루프 + 영구 성장 시스템 + 다크 판타지 아트 전환

---

## 완료된 작업

### 코어 시스템 (프로그래밍팀장)
- [x] 코어 전투 루프 (타워 배치 → Node 웨이브 → 공격 → 런 종료)
- [x] 영구 성장 시스템 (스킬 구매: 공격력/공격속도/기지HP)
- [x] 데이터 저장/로드 (JSON, MetaManager)
- [x] 타워 드래그 앤 드롭 + 인벤토리 시스템
- [x] 배속(x1/x2/x3) + 일시정지
- [x] 허브 ↔ 게임 씬 전환
- [x] 허브 복귀 버그 수정, Input System 호환 수정

### UI (UI팀장)
- [x] 타이틀 화면 (시작/설정/종료)
- [x] 허브 화면 (재화 표시 + 스킬 트리 + 스테이지 선택 + 출격)
- [x] 인게임 HUD (웨이브/Bit/기지HP/배속/인벤토리바)
- [x] 런 종료 패널 (클리어/패배 분기, 슬라이드업 애니메이션)
- [x] 타워 구매 패널, 타워 정보 툴팁
- [x] UI 에셋 임포트 설정 완료 (16개 PNG, 9-slice, Point 필터)
- [x] UISprites ScriptableObject 생성
- [x] UI 에셋 Unity 적용 완료 (16개 PNG 임포트, 9-slice, UISprites SO)

### 기획 (기획팀장)
- [x] GDD v2.0 완성
- [x] 용어 전환 확정 (Node→마물, Bit→소울, Core→정수, Tower→첨탑, Hub→성소, Wave→침공)
- [x] UI 명세 PPT v0.1

### 아트 (TA팀장)
- [x] ArtDirection v0.1 확정 (다크 판타지 픽셀아트, 3/4 쿼터뷰)
- [x] 사이버 아트 아카이브 → 다크 판타지 폴더 구조 생성
- [x] UIColors.cs 다크 판타지 팔레트 업데이트
- [x] ComfyUI 모델 파이프라인 구축 (SDXL + 3 LoRA)
- [x] UI 컨셉 이미지 16개 제작 (프레임/버튼/아이콘)
- [x] Arrow Tower 스타일 락 시안 생성 (SDXL, 5가지 변형)
- [x] Arrow Tower 레벨별(L1~L4) 폴리싱 완료 (총괄PD 승인)
  - L1 minimal → L2 basic → L3 ornate → L4 dark
- [x] 몬스터 스타일 락 시안 완료 (3종, 총괄PD 승인)
  - Soul: 그림자촉수체, Charger: 진홍늑대수, Brute: 보라골렘
- [x] 타일셋 스타일 락 시안 완료 (5종, 총괄PD 승인)
  - Path/Buildable/Decoration/Entrance/Base

### 인프라 (빌더)
- [x] Steam 빌드/업로드 파이프라인 (VDF 3종: dev/qa/release)
- [x] MCP Unity 연동 (CoplayDev/unity-mcp v9.4.4)

---

## 진행 중

### 기획 (기획팀장)
- [ ] 밸런싱 수치 설계 및 BalanceSheet v0.1 작성 중

### 사운드 (unity-sound-director + 개발PD)
- [x] 사운드 디렉션 v0.3 승인 (93점)
  - 문서: `Docs/Design/SoundDirection_FirstPass_Agent_v0.3.md`
- [x] Sprint 1 임시 오디오 리소스 생성 (BGM 3 + SFX/UI/Alert 15)
  - 경로: `Assets/Audio/`, `Assets/Resources/Audio/`
- [x] 전투 핵심 이벤트 사운드 코드 연동
  - 발사/적중/처치, 배치/합성, 웨이브/보스/클리어, HP 경고, BGM 전환
- [ ] 이벤트 트리거 QA + 1차 볼륨 밸런스 (다른 PD 채팅에서 진행 중)
- [ ] P0 AI 최종 리소스 1차 교체 (다른 PD 채팅에서 진행 중)

---

## 미착수 / 대기

### 컨텐츠 확장
- [ ] 추가 타워 종류 (현재 Arrow 1종만)
- [ ] 추가 노드(몬스터) 종류 (현재 Bit 1종만)
- [ ] 추가 스테이지 (현재 Stage 01만)
- [ ] 난이도 곡선 설계

### 시스템
- [ ] 방치(오프라인) 보상 시스템
- [ ] 튜토리얼/FTUE
- [ ] 사운드 최종 폴리싱/풀셋 교체 (Sprint 2 예정)

---

## 데이터 에셋 현황

| 카테고리 | 현재 | 목표 | 진행도 |
|----------|------|------|--------|
| Towers | 1 (Arrow) | 5+ | 20% |
| Nodes (몬스터) | 1 (Bit) | 5+ | 20% |
| Skills | 3 | 10+ | 30% |
| Stages | 1 | 10 | 10% |
| Waves | 1 | 30+ | 3% |

---

## 주요 결정 사항 기록

| 날짜 | 결정 | 비고 |
|------|------|------|
| 2026-02-14 | 게임명 Nodebreaker TD → Soulspire 변경 | |
| 2026-02-14 | 아트 스타일: 다크 판타지 픽셀아트 확정 | ArtDirection v0.1 |
| 2026-02-14 | 세계관 용어 전환 확정 | 6개 핵심 용어 |
| 2026-02-15 | Arrow Tower 레벨 구성: minimal→basic→ornate→dark | 총괄PD 피드백 |
| 2026-02-15 | 종교 상징(십자가 등) 게임 내 사용 금지 | 총괄PD 지시 |
| 2026-02-15 | Sprint 1 사운드 킥오프 착수 (임시 리소스 + 런타임 연동) | `Docs/Design/Sprint1_Sound_Kickoff_v0.1.md` |
| 2026-02-15 | PM 판단: Sprint 1 내 프로토타입 통합 가능, 풀셋 최종은 Sprint 2 | `project-manager` 응답 |
| 2026-02-15 | 몬스터 3종 스타일 락 승인 | Soul=그림자촉수체, Charger=진홍늑대수, Brute=보라골렘 |
| 2026-02-15 | 타일셋 5종 스타일 락 승인 | Path/Buildable/Decoration/Entrance/Base |
| 2026-02-15 | 팀장별 git author 설정 도입 | 커밋 추적 및 기여도 관리 |
| 2026-02-15 | QA 조기 착수 프로세스 도입 | 빌드 전 가동 → 병렬 협업 효율성 증대 |
| 2026-02-15 | 개발PD 3인 체제 정보공유 지침서 작성 | `Docs/DevPD_Guidelines.md` |

---

## 팀별 담당자

| 역할 | 에이전트 | 현재 업무 |
|------|----------|-----------|
| 총괄PD | 사용자 | 방향 설정, 피드백 |
| 개발PD | Claude (조율 전용) | 업무 배분, 진행 관리 |
| 기획팀장 | game-designer | 밸런싱/레벨디자인 작업 착수, BalanceSheet v0.1 작성 중 |
| 프로그래밍팀장 | unity-gameplay-programmer | 대기 |
| QA팀장 | unity-qa-engineer | 대기 (빌드 전 가동) |
| UI팀장 | unity-ui-developer | UI 에셋 Unity 적용 완료 |
| TA팀장 | unity-technical-artist | 스타일 락 완료, 후처리 대기 |
| 빌더 | unity-build-engineer | 대기 |
| PM | project-manager | 진행 현황 갱신 중 |
