# Sprint 4 진행 현황

**프로젝트**: Soulspire
**브랜치**: feature/phase1-core-loop
**기준 문서**: `Docs/Sprint4_Plan.md`, `Docs/Design/GDD.md` (v3.0)
**최종 업데이트**: 2026-02-15
**상태**: Phase 2 (영구 성장 루프) 구현 진행 중

---

## 스프린트 목표

"Stage 1~3을 완전히 플레이 가능한 수직 슬라이스로 완성"
- 죽음 -> Hub 스킬트리 -> 영구 성장 -> 재시도 -> 첫 클리어 -> 보상 -> 다음 스테이지 해금의 전체 루프 완성
- Sprint 4는 Notion 업무 카드 기반으로 Phase 2 (영구 성장 루프) 집중 구현 중

---

## Notion 업무 카드 현황

| 카드 | 담당팀 | 상태 |
|------|--------|------|
| Phase 2: 데이터+매니저+모디파이어 (Steps 1-5) | 프로그래밍 | 작업 중 |
| Phase 2: Hub UI + InGameUI 업데이트 (Steps 6-7) | UI | 작업 중 |
| Phase 2: SO 에셋 생성 + 씬 설정 (Steps 8-9) | 기획 | 대기 (Steps 1-5 완료 후) |
| Phase 2: Hub 스킬트리 아이콘/UI 에셋 제작 | TA | 작업 중 |
| Phase 2: Hub BGM + 스킬구매 SFX 제작 | 사운드 | 작업 중 |

---

## 완료된 작업

### 프로그래밍팀장 (unity-gameplay-programmer)
- [x] Step 1: 데이터 레이어 구현 -- PlayerSaveData, SkillNodeData, RunModifiers 3파일 생성
- [x] Step 3: GameManager 리팩토링 -- MetaManager로 영구 성장 로직 위임
- [x] Step 4: RunManager 모디파이어 적용 -- StartRun 시그니처 변경, CurrentModifiers 프로퍼티 추가
- [x] Step 5: Tower 모디파이어 적용 -- 공격속도/데미지 배율 곱연산 반영

### 인프라
- [x] GDD v3.0 업데이트 -- 총괄PD 결정 5건 반영 (커밋 14b19f7)
- [x] Sprint 4 계획서 수립 -- 플레이 가능한 수직 슬라이스 목표 (커밋 1f6b6b4)
- [x] Sprint 3 QA 이슈 수정 -- SpriteLinker 신규 타워/몬스터 등록, defense SO 직렬화 (커밋 12d12db)
- [x] MetaManager Tesseract.Save 연동 기반 작업 -- SaveManager로 세이브 시스템 교체 (커밋 00f304b)

---

## 진행 중

### 프로그래밍팀장 (unity-gameplay-programmer)
- [ ] Step 2: MetaManager Tesseract.Save 연동 -- SaveManager 교체 커밋(00f304b) 완료, 남은 연동 작업 진행 중

### UI팀장 (unity-ui-developer)
- [x] Step 6: HubUI.cs 생성 -- 완료
- [x] Step 6: SkillNodeUI.cs 생성 -- 완료
- [ ] Step 7: InGameUI 수정 -- RunEnd 패널 업데이트 진행 중

### TA팀장 (unity-technical-artist)
- [ ] Hub 스킬트리 아이콘 3종 제작 -- 작업 시작
- [ ] Hub 배경 에셋 제작 -- 작업 시작
- [ ] 스킬노드 프레임/잠금 오버레이 제작 -- 작업 시작

### 사운드 디렉터 (unity-sound-director)
- [ ] Hub BGM 루프 제작 -- 작업 시작
- [ ] SFX 4종 제작 (스킬 구매/해금/레벨업/에러) -- 작업 시작
- [ ] SoundManager 연동 -- 대기 (리소스 제작 후)

---

## 미착수 / 대기

### 기획팀장 (game-designer)
- [ ] Step 8: SkillNodeData SO 3종 생성 -- 선행: Steps 1-5 (프로그래밍) 완료
- [ ] Step 9: MetaManager 씬 오브젝트 설정 -- 선행: Steps 1-5 완료
- [ ] Step 9: HubPanel UI 설정 -- 선행: Steps 6-7 (UI) 완료

### QA팀장 (unity-qa-engineer)
- [ ] 영구 성장 루프 e2e 검증 -- 선행: 전 팀 Phase 2 작업 완료
- [ ] Phase 2 단위 QA -- 선행: 각 팀 dev/* 브랜치 작업 완료

### 빌더 (unity-build-engineer)
- [ ] 빌드 -- 선행: 통합 QA 통과 + 총괄PD 승인

### PM (project-manager)
- [x] Sprint4_Progress.md 작성 -- 본 문서

---

## Step 진행 추적 (Phase 2: 영구 성장 루프)

| Step | 내용 | 담당 | 상태 |
|------|------|------|------|
| 1 | 데이터 레이어 (PlayerSaveData, SkillNodeData, RunModifiers) | 프로그래밍 | 완료 |
| 2 | MetaManager + Tesseract.Save 연동 | 프로그래밍 | 진행 중 |
| 3 | GameManager 리팩토링 (MetaManager 위임) | 프로그래밍 | 완료 |
| 4 | RunManager 모디파이어 (StartRun, CurrentModifiers) | 프로그래밍 | 완료 |
| 5 | Tower 모디파이어 (공속/데미지 배율) | 프로그래밍 | 완료 |
| 6 | HubUI + SkillNodeUI 생성 | UI | 완료 |
| 7 | InGameUI RunEnd 패널 수정 | UI | 진행 중 |
| 8 | SkillNodeData SO 3종 생성 | 기획 | 대기 |
| 9 | MetaManager 씬 오브젝트 + HubPanel UI 설정 | 기획 | 대기 |

---

## 완료 기준 체크리스트

### Phase 2: 영구 성장 루프
- [x] PlayerSaveData, SkillNodeData, RunModifiers 데이터 구조 구현
- [x] MetaManager가 GameManager에서 분리되어 영구 성장 관리
- [x] RunManager에 모디파이어 시그니처 반영
- [x] Tower 공속/데미지 배율 곱연산 동작
- [ ] MetaManager Tesseract.Save 완전 연동 (저장/로드)
- [x] HubUI, SkillNodeUI 스크립트 생성
- [ ] InGameUI RunEnd 패널 업데이트
- [ ] SkillNodeData SO 3종 (공격력/공속/기지HP) 생성
- [ ] MetaManager 씬 오브젝트 배치
- [ ] Hub 스킬트리 아이콘 3종 제작
- [ ] Hub BGM + SFX 4종 적용
- [ ] 영구 성장 루프 e2e 동작 (Hub 스킬 구매 -> 인게임 반영 -> 런 종료 -> Hub 복귀)

### Sprint 4 전체 (Sprint4_Plan.md 기준, 후속 Phase)
- [ ] 타워 드래그앤드롭 합성 시스템
- [ ] 보스 Node + Stage 클리어 판정
- [ ] 보물상자 시스템 (3택 선택지)
- [ ] 스테이지 조건 해금
- [ ] Stage 1~3 통합 QA 통과
- [ ] Windows 빌드 + Steam dev_test 업로드
- [ ] 총괄PD 빌드 승인

---

## 블로커

- 없음 (현재 각 팀 병렬 진행 중, Step 2 MetaManager 연동만 남은 상태)

---

## 스프린트 현황 요약

- 완료: 8건 / 진행중: 7건 / 대기: 6건
- 주요 진전: 프로그래밍 Steps 1/3/4/5 완료 (5개 중 4개), UI HubUI+SkillNodeUI 생성 완료
- 블로커: 없음
- 다음 예정: Step 2 MetaManager 연동 완료 -> Step 7 InGameUI 완료 -> 기획팀장 Steps 8-9 착수

---

## 주요 결정 사항

| 날짜 | 내용 | 비고 |
|------|------|------|
| 2026-02-15 | GDD v3.0 총괄PD 결정 5건 반영 | 타워합성/보물상자/해금조건/배치/클리어판정 |
| 2026-02-15 | MetaManager 세이브 시스템 Tesseract.Save.SaveManager로 교체 | 커밋 00f304b |
| 2026-02-15 | Sprint 4 Phase 2 (영구 성장 루프) 우선 착수 | Notion 카드 5건 등록 |

---

## 팀별 현재 업무

| 역할 | 에이전트 | 현재 업무 |
|------|----------|-----------|
| 총괄PD | 사용자 | 방향 설정, 피드백 |
| 개발PD | Claude (조율 전용) | 업무 배분, Phase 2 진행 조율 |
| 기획팀장 | game-designer | 대기 (Steps 1-5 완료 후 Steps 8-9 착수) |
| 프로그래밍팀장 | unity-gameplay-programmer | Step 2 MetaManager Tesseract.Save 연동 |
| QA팀장 | unity-qa-engineer | 대기 (Phase 2 작업 완료 후 검증) |
| UI팀장 | unity-ui-developer | Step 7 InGameUI RunEnd 패널 수정 |
| TA팀장 | unity-technical-artist | Hub 스킬트리 아이콘/UI 에셋 제작 |
| 사운드 디렉터 | unity-sound-director | Hub BGM + 스킬구매 SFX 제작 |
| 빌더 | unity-build-engineer | 대기 |
| PM | project-manager | Sprint4_Progress.md 갱신 |
