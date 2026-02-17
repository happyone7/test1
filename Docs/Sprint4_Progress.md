# Sprint 4 진행 현황

**프로젝트**: Soulspire
**브랜치**: feature/phase1-core-loop
**기준 문서**: `Docs/Sprint4_Plan.md`, `Docs/Design/GDD.md` (v3.0)
**최종 업데이트**: 2026-02-16
**상태**: 전 Phase 완료, 통합 QA Pass -- 빌드 대기 (총괄PD 승인 필요)

---

## 스프린트 목표

"Stage 1~3을 완전히 플레이 가능한 수직 슬라이스로 완성"
- 죽음 -> Hub 스킬트리 -> 영구 성장 -> 재시도 -> 첫 클리어 -> 보상 -> 다음 스테이지 해금의 전체 루프 완성
- Phase 1 (코어 루프) -> Phase 2 (영구 성장 루프) -> Phase 3 (콘텐츠 확장) 순서로 진행

---

## Notion 업무 카드 현황

| 카드 | 담당팀 | 상태 |
|------|--------|------|
| Phase 2: 데이터+매니저+모디파이어 (Steps 1-5) | 프로그래밍 | 완료 |
| Phase 2: Hub UI + InGameUI 업데이트 (Steps 6-7) | UI | 완료 |
| Phase 2: SO 에셋 생성 + 씬 설정 (Steps 8-9) | 기획/프로그래밍 | 완료 |
| Phase 2: Hub 스킬트리 아이콘/UI 에셋 제작 | TA | 완료 |
| Phase 2: Hub BGM + 스킬구매 SFX 제작 | 사운드 | 완료 |
| Phase 3: 콘텐츠 확장 코드 구현 | 프로그래밍 | 완료 |
| Phase 3: 콘텐츠 SO 에셋 대량 생성 | 기획 | 완료 |
| Phase 3: SFX 추가 (타워 업그레이드/FTUE/스테이지 해금) | 사운드 | 완료 |
| Phase 3: 보스HP바/업그레이드UI/FTUE/Core팝업 | UI | 완료 |
| Phase 3: 보스3종+Shield+이펙트 스프라이트 | TA | 완료 |
| 보물상자 시스템: 데이터 구조 + SO 16종 (GD-3) | 기획 | 완료 |
| 보물상자 시스템: TreasureManager 런타임 (PG-7) | 프로그래밍 | 완료 |
| 보물상자 시스템: SFX 3종 연동 (SD-2) | 사운드 | 완료 |
| 보물상자 시스템: TreasureChoiceUI 3택 패널 (UI-3) | UI | 완료 |
| Phase 2 Step 9: MetaManager 씬 설정 | 프로그래밍 | 완료 |
| 통합 QA: Phase 2 + Phase 3 + 보물상자 | QA | 완료 (Pass) |
| QA 이슈 수정 6건 | 프로그래밍/UI/사운드 | 완료 |

---

## 완료된 작업

### Phase 2: 영구 성장 루프

#### 프로그래밍팀장 (unity-gameplay-programmer) -- 커밋 00f304b
- [x] Step 1: 데이터 레이어 구현 -- PlayerSaveData, SkillNodeData, RunModifiers 3파일 생성
- [x] Step 2: MetaManager Tesseract.Save 연동 -- SaveManager로 세이브 시스템 교체
- [x] Step 3: GameManager 리팩토링 -- MetaManager로 영구 성장 로직 위임
- [x] Step 4: RunManager 모디파이어 적용 -- StartRun 시그니처 변경, CurrentModifiers 프로퍼티 추가
- [x] Step 5: Tower 모디파이어 적용 -- 공격속도/데미지 배율 곱연산 반영

#### UI팀장 (unity-ui-developer) -- 커밋 9b44ebb
- [x] Step 6: HubUI.cs, SkillNodeUI.cs 생성 -- 스킬 잠금 시스템 구현
- [x] Step 7: InGameUI RunEnd 패널 수정 -- 보유 Bit 표시

#### TA팀장 (unity-technical-artist) -- 커밋 f3bd91f
- [x] Hub 스킬트리 아이콘 3종 제작 -- SDXL 파이프라인
- [x] Hub 배경 에셋 제작 -- SDXL 파이프라인

#### 사운드 디렉터 (unity-sound-director) -- 커밋 1c73b16
- [x] Hub BGM 루프 제작
- [x] SFX 4종 제작 (스킬 구매/해금/레벨업/에러)
- [x] SoundManager 연동

#### 기획팀장 (game-designer) -- 커밋 d8d6bf4
- [x] Step 8: SkillNodeData SO 3종 생성 -- 공격력/공속/기지HP 밸런스 수치 및 네이밍 갱신

### Phase 3: 콘텐츠 확장

#### 프로그래밍팀장 (unity-gameplay-programmer) -- 커밋 f4cc8da
- [x] BossAbility 시스템 구현
- [x] Tower Upgrade 시스템 구현
- [x] Stage 해금 시스템 구현
- [x] FTUE (First Time User Experience) 온보딩 코드

#### 기획팀장 (game-designer) -- 커밋 13b7124
- [x] Boss 3종 SO 생성
- [x] Shield Node SO 생성
- [x] Stage 2/3 SO 생성
- [x] Wave 15종 SO 에셋 대량 생성

#### 사운드 디렉터 (unity-sound-director) -- 커밋 52974eb
- [x] TowerUpgrade SFX
- [x] UiGuide SFX (FTUE)
- [x] StageUnlock SFX

#### UI팀장 (unity-ui-developer) -- 커밋 31b4c9a
- [x] 보스 HP바 UI 구현
- [x] 타워 업그레이드 UI 구현
- [x] FTUE 가이드 UI 구현
- [x] 스테이지 해금 알림 UI
- [x] Core 팝업 UI

#### TA팀장 (unity-technical-artist) -- 커밋 d90061f
- [x] 보스 몬스터 3종 스프라이트 -- SDXL HD 파이프라인
- [x] Shield Node 스프라이트
- [x] 이펙트 스프라이트

### Phase 2 Step 9: MetaManager 씬 설정 (보류 해제)

#### 프로그래밍팀장 (unity-gameplay-programmer) -- 커밋 b7c703b
- [x] MetaManager allSkills 배열에 SkillNodeData SO 3종 연결

### 보물상자 시스템 (신규)

#### 기획팀장 (game-designer) -- GD-3, 커밋 b7c703b
- [x] TreasureChoiceData / TreasureConfig 데이터 구조 설계
- [x] TreasureConfig SO 16종 생성

#### 프로그래밍팀장 (unity-gameplay-programmer) -- PG-7, 커밋 b7c703b
- [x] TreasureManager 런타임 시스템 구현
- [x] WaveSpawner / RunManager / Tower / Node 연동
- [x] TreasureManager 씬 배치 + TreasureConfig SO 할당

#### 사운드 디렉터 (unity-sound-director) -- SD-2, 커밋 b7c703b
- [x] 보물상자 SFX 3종 연동

#### UI팀장 (unity-ui-developer) -- UI-3, 커밋 b7c703b
- [x] TreasureChoiceUI 풀스크린 3택 선택 패널 구현

### QA 이슈 수정 (6건)

#### 프로그래밍팀장 (unity-gameplay-programmer) -- 커밋 a620a2f
- [x] MetaManager.Save() null 가드 추가
- [x] GameManager.stages 배열에 Stage 3종 할당

#### UI팀장 (unity-ui-developer) -- 커밋 c650053
- [x] TreasureChoiceUI 씬 배치
- [x] BossHPBar 씬 배치
- [x] FTUE 가이드 씬 배치

#### 사운드 디렉터 (unity-sound-director) -- 커밋 78b2cf1
- [x] SoundManager UI_Hub_Click_01 경로 수정 (Hub/ -> UI/)

### QA 재검증

#### QA팀장 (unity-qa-engineer)
- [x] 6/6 전항목 Pass -- 에러 0건, 경고 0건

### 인프라
- [x] GDD v3.0 업데이트 -- 총괄PD 결정 5건 반영 (커밋 14b19f7)
- [x] Sprint 4 계획서 수립 -- 플레이 가능한 수직 슬라이스 목표 (커밋 1f6b6b4)
- [x] Sprint 3 QA 이슈 수정 -- SpriteLinker 신규 타워/몬스터 등록, defense SO 직렬화 (커밋 12d12db)
- [x] Phase 3 콘텐츠 확장 기획서 작성 -- 스테이지/보스/타워 업그레이드/온보딩 (커밋 57575f2)

---

## 진행 중

(없음 -- 전 작업 완료)

---

## 보류

(없음 -- Step 9 완료로 해소)

---

## 미착수 / 대기

### 빌더 (unity-build-engineer)
- [ ] 빌드 -- 선행: 총괄PD 승인 (QA 통과 완료)

---

## Step 진행 추적 (Phase 2: 영구 성장 루프)

| Step | 내용 | 담당 | 상태 |
|------|------|------|------|
| 1 | 데이터 레이어 (PlayerSaveData, SkillNodeData, RunModifiers) | 프로그래밍 | 완료 |
| 2 | MetaManager + Tesseract.Save 연동 | 프로그래밍 | 완료 |
| 3 | GameManager 리팩토링 (MetaManager 위임) | 프로그래밍 | 완료 |
| 4 | RunManager 모디파이어 (StartRun, CurrentModifiers) | 프로그래밍 | 완료 |
| 5 | Tower 모디파이어 (공속/데미지 배율) | 프로그래밍 | 완료 |
| 6 | HubUI + SkillNodeUI 생성 | UI | 완료 |
| 7 | InGameUI RunEnd 패널 수정 | UI | 완료 |
| 8 | SkillNodeData SO 3종 생성 | 기획 | 완료 |
| 9 | MetaManager 씬 오브젝트 + HubPanel UI 설정 | 프로그래밍 | 완료 (b7c703b) |

## Phase 3 진행 추적 (콘텐츠 확장)

| 항목 | 담당 | 상태 | 커밋 |
|------|------|------|------|
| BossAbility, Tower Upgrade, Stage 해금, FTUE 코드 | 프로그래밍 | 완료 | f4cc8da |
| Boss 3종, Shield, Stage 2/3, Wave 15종 SO | 기획 | 완료 | 13b7124 |
| TowerUpgrade/UiGuide/StageUnlock SFX | 사운드 | 완료 | 52974eb |
| 보스HP바, 업그레이드UI, FTUE, Core팝업 | UI | 완료 | 31b4c9a |
| 보스3종+Shield+이펙트 SDXL HD 스프라이트 | TA | 완료 | d90061f |
| 보물상자 데이터 구조 + SO 16종 (GD-3) | 기획 | 완료 | b7c703b |
| TreasureManager 런타임 + 씬 배치 (PG-7) | 프로그래밍 | 완료 | b7c703b |
| 보물상자 SFX 3종 (SD-2) | 사운드 | 완료 | b7c703b |
| TreasureChoiceUI 3택 패널 (UI-3) | UI | 완료 | b7c703b |
| QA 이슈 수정 6건 | 프로그래밍/UI/사운드 | 완료 | a620a2f, c650053, 78b2cf1 |
| 통합 QA 재검증 (6/6 Pass) | QA | 완료 | -- |

---

## 완료 기준 체크리스트

### Phase 2: 영구 성장 루프
- [x] PlayerSaveData, SkillNodeData, RunModifiers 데이터 구조 구현
- [x] MetaManager가 GameManager에서 분리되어 영구 성장 관리
- [x] RunManager에 모디파이어 시그니처 반영
- [x] Tower 공속/데미지 배율 곱연산 동작
- [x] MetaManager Tesseract.Save 완전 연동 (저장/로드)
- [x] HubUI, SkillNodeUI 스크립트 생성
- [x] InGameUI RunEnd 패널 업데이트
- [x] SkillNodeData SO 3종 (공격력/공속/기지HP) 생성
- [x] MetaManager 씬 오브젝트 배치 -- 완료 (b7c703b)
- [x] Hub 스킬트리 아이콘 3종 제작
- [x] Hub BGM + SFX 4종 적용
- [x] 영구 성장 루프 e2e 동작 (Hub 스킬 구매 -> 인게임 반영 -> 런 종료 -> Hub 복귀) -- QA Pass

### Phase 3: 콘텐츠 확장
- [x] BossAbility 시스템 구현
- [x] Tower Upgrade 시스템 구현
- [x] Stage 해금 시스템 구현
- [x] FTUE 온보딩 시스템 구현
- [x] Boss 3종 + Shield Node SO 생성
- [x] Stage 2/3 + Wave 15종 SO 생성
- [x] 보스 HP바/타워 업그레이드 UI/FTUE 가이드/Core 팝업 구현
- [x] 보스 3종 + Shield + 이펙트 스프라이트 제작
- [x] Phase 3 SFX 3종 적용
- [x] Phase 3 통합 QA -- QA Pass

### Sprint 4 전체 (Sprint4_Plan.md 기준)
- [x] 타워 업그레이드 시스템
- [x] 보스 Node + Stage 클리어 판정
- [x] 스테이지 조건 해금
- [x] FTUE 온보딩
- [x] 보물상자 시스템 (3택 선택지) -- 완료 (b7c703b)
- [x] Stage 1~3 통합 QA 통과 -- QA Pass (6/6 전항목)
- [ ] Windows 빌드 + Steam dev_test 업로드 -- 대기 (QA 통과, 총괄PD 승인 필요)
- [ ] 총괄PD 빌드 승인 -- 대기

---

## 블로커

- 없음 (전 블로커 해소)
- ~~Phase 2 Step 9 씬 설정~~ -- 완료 (b7c703b)
- ~~QA 통합 검증~~ -- Pass (6/6)

---

## 스프린트 현황 요약

- 완료: 35건 / 진행중: 0건 / 보류: 0건 / 대기: 1건 (빌드 -- 총괄PD 승인 필요)
- 주요 진전: 보물상자 시스템 완성, Step 9 보류 해제, QA 6/6 전항목 Pass
- 블로커: 없음
- 다음 예정: 총괄PD 빌드 승인 -> 빌더가 Windows 빌드 + Steam dev_test 업로드

---

## 주요 결정 사항

| 날짜 | 내용 | 비고 |
|------|------|------|
| 2026-02-15 | GDD v3.0 총괄PD 결정 5건 반영 | 타워합성/보물상자/해금조건/배치/클리어판정 |
| 2026-02-15 | MetaManager 세이브 시스템 Tesseract.Save.SaveManager로 교체 | 커밋 00f304b |
| 2026-02-15 | Sprint 4 Phase 2 (영구 성장 루프) 우선 착수 | Notion 카드 5건 등록 |
| 2026-02-15 | Phase 2 전 팀 작업 완료 | 커밋 00f304b, 9b44ebb, f3bd91f, 1c73b16, d8d6bf4 |
| 2026-02-15 | Phase 3 콘텐츠 확장 전 팀 작업 완료 | 커밋 f4cc8da, 13b7124, 52974eb, 31b4c9a, d90061f |
| 2026-02-15 | Phase 2 Step 9 씬 설정 보류 | Unity MCP 미응답 |
| 2026-02-16 | Step 9 보류 해제 + MetaManager 씬 설정 완료 | 커밋 b7c703b |
| 2026-02-16 | 보물상자 시스템 구현 완료 (GD-3, PG-7, SD-2, UI-3) | 커밋 b7c703b |
| 2026-02-16 | QA 이슈 6건 수정 완료 | 커밋 a620a2f, c650053, 78b2cf1 |
| 2026-02-16 | 통합 QA 재검증 6/6 전항목 Pass | 에러 0건, 경고 0건 |

---

## 팀별 현재 업무

| 역할 | 에이전트 | 현재 업무 |
|------|----------|-----------|
| 총괄PD | 사용자 | 빌드 승인 대기 |
| 개발PD | Claude (조율 전용) | 총괄PD 승인 대기, 빌드 지시 준비 |
| 기획팀장 | game-designer | 전 작업 완료, 유휴 |
| 프로그래밍팀장 | unity-gameplay-programmer | 전 작업 완료 (QA 이슈 수정 포함), 유휴 |
| QA팀장 | unity-qa-engineer | 통합 QA 완료 (6/6 Pass), 유휴 |
| UI팀장 | unity-ui-developer | 전 작업 완료 (QA 이슈 수정 포함), 유휴 |
| TA팀장 | unity-technical-artist | 전 작업 완료, 유휴 |
| 사운드 디렉터 | unity-sound-director | 전 작업 완료 (SFX 경로 수정 포함), 유휴 |
| 빌더 | unity-build-engineer | 대기 (총괄PD 승인 후 빌드) |
| PM | project-manager | Sprint4_Progress.md 갱신 완료 |
