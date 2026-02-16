# Sprint 5 계획서 — "Soulspire 아이덴티티 확립 + 초반 경험 설계"

**프로젝트**: Soulspire
**작성일**: 2026-02-17
**작성자**: PM (project-manager)
**검토**: DevPD (검토 완료, 수정 반영)
**결재**: 총괄PD ✅ 승인 완료

---

## 스프린트 목표

Sprint 4까지 플레이 가능한 수직 슬라이스(Floor 1~3)가 완성되었으나, 이전 컨셉인 "Nodebreaker TD"의 흔적이 코드베이스 전반에 남아있다.

Sprint 5는 **"Soulspire 아이덴티티를 코드/프로젝트/문서 전역에 확립하고, TA 컨셉 아트 기반 세계관 용어 체계 + GDD v4.0 기획 변경을 코드에 반영"** 하는 것이 목표다.

### 핵심 목표 5가지

| # | 목표 | 근거 |
|---|------|------|
| G1 | **Nodebreaker → Soulspire 전면 리네이밍** | 네임스페이스(52개 파일), 프로젝트 설정, 빌드 파이프라인, 문서/스킬 |
| G2 | **세계관 용어 체계 적용** | TA 컨셉 아트 기준: Bit→Soul, Core→Core Fragment, Hub→Sanctum, Wave→Incursion, Stage→Floor |
| G3 | **GDD v4.0 기획 변경 반영** | Early Game Flow (타워 1기 시작, Floor 1 필패), 보물상자 타워 획득 시스템 |
| G4 | **TA 컨셉 아트 기반 에셋 적용** | 4화면 컨셉(Title/Sanctum/InGame/RunEnd) 기준으로 UI/스프라이트 개선 |
| G5 | **문서/에이전트/스킬 정비** | 경로/명칭 일관성 확보, Git Remote 변경 |

### 세계관 용어 매핑 (TA 컨셉 아트 확정)

| 기존 (코드) | 새 용어 (세계관) | 코드 변수명 |
|-------------|-----------------|------------|
| Bit | **Soul** | `soul`, `totalSoul` |
| Core | **Core Fragment** | `coreFragment`, `totalCoreFragment` |
| Hub | **Sanctum** | UI 텍스트만 (코드 클래스명은 유지) |
| Wave | **Incursion** | UI 텍스트만 (코드 WaveManager 등은 유지) |
| Stage | **Floor** | UI 텍스트만 (코드 StageData 등은 유지) |
| 시작 버튼 | **EMBARK** | UI 텍스트만 |

### 이번 스프린트에서 제외

- Lightning/Laser/Void Tower 구현 (리네이밍 완료 후 후속 스프린트)
- Floor 4~5 데이터 확장
- Idle Soul 수집기
- 사운드 폴리싱 (리네이밍 영향 확인만)

---

## Phase 구성 개요

```
Phase 1: 네임스페이스 + 프로젝트 설정 리네이밍 (전체 코드베이스 기반)
    모든 팀 착수 가능
    ↓
Phase 2: 세계관 용어 적용 + GDD v4.0 기획 변경 반영 + TA 에셋 적용
    Phase 1 완료 직후 착수, 병렬 가능
    ↓
Phase 3: 문서/스킬 정비 + QA + 빌드
    Phase 2 완료 후 착수
```

---

## Phase 1 — 네임스페이스 + 프로젝트 설정 리네이밍

### 프로그래밍팀장 (unity-gameplay-programmer)

| # | 태스크 | 산출물 | 예상 시간 |
|---|--------|--------|-----------|
| PG-1 | C# 네임스페이스 일괄 변경 (52개 파일) | `Nodebreaker.*` → `Soulspire.*` (Core, Editor, Audio, UI, Data, Tower, Node) | ~30분 |
| PG-2 | MenuItem 경로 일괄 변경 (20+건) | `"Tools/Nodebreaker/"` → `"Tools/Soulspire/"` | ~15분 |
| PG-3 | using 참조 일괄 검증 | 전체 C# 파일 컴파일 확인, 누락된 using 수정 | ~10분 |

### 빌더 (unity-build-engineer)

| # | 태스크 | 산출물 | 예상 시간 |
|---|--------|--------|-----------|
| BLD-1 | ProjectSettings.asset 변경 | `companyName: SBGames`, `productName: Soulspire` | ~5분 |
| BLD-2 | BuildScript.cs 변경 | `MyGame.exe` → `Soulspire.exe` | ~5분 |
| BLD-3 | Steam VDF 3종 변경 | `"Nodebreaker TD"` → `"Soulspire"`, `MyGame.exe` → `Soulspire.exe` | ~10분 |
| BLD-4 | upload.bat/upload.sh 변경 | `MyGame.exe` → `Soulspire.exe` | ~5분 |

### 기획팀장 (game-designer)

| # | 태스크 | 산출물 | 예상 시간 |
|---|--------|--------|-----------|
| GD-1 | GDD + 기획 문서 세계관 용어 전면 적용 | Gold→Soul, Core→Core Fragment, Hub→Sanctum, Wave→Incursion, Stage→Floor 일괄 치환 + Nodebreaker 잔재 제거 | ~20분 |

### QA팀장 (unity-qa-engineer)

| # | 태스크 | 산출물 | 예상 시간 |
|---|--------|--------|-----------|
| QA-1 | Phase 1 리네이밍 컴파일/빌드 검증 | 에디터 컴파일 에러 0건, 빌드 성공 확인 | ~15분 |

### PM (project-manager)

| # | 태스크 | 산출물 | 예상 시간 |
|---|--------|--------|-----------|
| PM-1 | CLAUDE.md, 에이전트, 스킬 내 경로/명칭 변경 | `test1` → `soulspire`, `MyGame.exe` → `Soulspire.exe`, `Nodebreaker TD` → `Soulspire`, 세계관 용어 반영 | ~20분 |
| PM-2 | DevPD_Guidelines.md 경로 업데이트 | `test1/` → `soulspire/` | ~5분 |

### 미투입 팀 (Phase 1)
- TA팀장: Phase 2에서 컨셉 아트 기반 에셋 적용
- 사운드 디렉터: Phase 3에서 네임스페이스 변경 영향 확인만
- UI팀장: Phase 2에서 세계관 용어 UI 적용

---

## Phase 2 — 세계관 용어 적용 + GDD v4.0 기획 변경 반영

### 기획팀장 (game-designer)

| # | 태스크 | 산출물 | 예상 시간 |
|---|--------|--------|-----------|
| GD-2 | SO 수치 조정 (P0 항목 7건) | Skill 비용/성장률, Incursion 구성, Floor 1 초기 타워 설정 | ~25분 |
| GD-3 | Early Game Flow SO 데이터 | Floor 1: 타워 1기 시작, 몬스터 2~3마리, Soul 보상 | ~15분 |
| GD-4 | 보물상자 타워 획득 데이터 재구성 | 기존 TreasureConfig SO 16종 → 타워 획득 중심으로 변경 | ~20분 |

### 프로그래밍팀장 (unity-gameplay-programmer)

| # | 태스크 | 산출물 | 예상 시간 |
|---|--------|--------|-----------|
| PG-4 | 재화 변수명 리네이밍 (bit → soul) | PlayerSaveData, RunManager, Tower, Node 등 전역 변수 변경. Core → CoreFragment | ~20분 |
| PG-5 | 보물상자 시스템 변경 (런 내 버프 → 타워 획득) | TreasureManager 로직 변경, 타워 인벤토리 추가 로직 | ~25분 |
| PG-6 | Early Game Flow 구현 | Floor 1 타워 1기 시작 (GameManager/RunManager 초기화 로직) | ~15분 |

### UI팀장 (unity-ui-developer)

| # | 태스크 | 산출물 | 예상 시간 |
|---|--------|--------|-----------|
| UI-1 | UI 세계관 용어 일괄 적용 | Bit→Soul, Core→Core Fragment, Hub→Sanctum, Wave→Incursion, Stage→Floor, 시작→EMBARK | ~25분 |
| UI-2 | 보물상자 UI 변경 (3택 타워 선택) | TreasureChoiceUI: 타워 아이콘 + 이름 + 설명 표시 | ~20분 |

### TA팀장 (unity-technical-artist)

| # | 태스크 | 산출물 | 예상 시간 |
|---|--------|--------|-----------|
| TA-1 | 컨셉 아트 기반 UI 프레임/패널 스프라이트 제작 | 석재+금박 프레임, 룬 장식 패널 배경 (Title/Sanctum/InGame/RunEnd 공통) | ~25분 |
| TA-2 | 타이틀 화면 배경 + 로고 리파인 | 컨셉 기준: 마법 첨탑 실루엣 배경, SOULSPIRE 로고, PLAY/SETTINGS/QUIT 버튼 프레임 | ~20분 |
| TA-3 | 재화 아이콘 리뉴얼 | Soul 아이콘 (에메랄드 보석), Core Fragment 아이콘 (시안 팔각형) — 컨셉 아트 색상 기준 | ~15분 |

### QA팀장 (unity-qa-engineer)

| # | 태스크 | 산출물 | 예상 시간 |
|---|--------|--------|-----------|
| QA-2 | Phase 2 각 팀 dev/* 브랜치 단위 QA | 재화 표시, 보물상자, Early Game Flow, UI 용어 개별 검증 | ~20분 |

---

## Phase 3 — 문서/스킬 정비 + QA + 빌드

### 기획팀장 (game-designer)

| # | 태스크 | 산출물 | 예상 시간 |
|---|--------|--------|-----------|
| GD-5 | Floor 1 Early Game 밸런스 조정 | QA 피드백 기반 타워 1기 초기 배치, 몬스터 수 조정 | ~15분 |

### 프로그래밍팀장 (unity-gameplay-programmer)

| # | 태스크 | 산출물 | 예상 시간 |
|---|--------|--------|-----------|
| PG-7 | 세이브 시스템 정비 | 세이브 파일명 `nodebreaker_save.json` → `soulspire_save.json`, FTUEManager PlayerPrefs → Tesseract Save 통합 | ~15분 |
| PG-8 | 버그 수정 (QA 피드백 대응) | QA에서 보고된 이슈 수정 | ~15분 |

### QA팀장 (unity-qa-engineer)

| # | 태스크 | 산출물 | 예상 시간 |
|---|--------|--------|-----------|
| QA-3 | Early Game Flow 통합 QA | 타워 1기 시작 → Floor 1 전체 패배 → Soul 획득 → Sanctum 업그레이드 → Floor 2 Incursion 1 생존 검증 | ~20분 |
| QA-4 | 리네이밍 전역 QA | UI/로그/에러 메시지에 "Nodebreaker", "Bit", "Hub", "Wave", "Stage" 잔재 확인 | ~15분 |

### 빌더 (unity-build-engineer)

| # | 태스크 | 산출물 | 예상 시간 |
|---|--------|--------|-----------|
| BLD-5 | Git Remote 변경 | `test1` → `soulspire` (GitHub 리포지토리명 변경, worktree URL 업데이트) | ~10분 |
| BLD-6 | Windows 빌드 + Steam dev_test 업로드 | `Soulspire.exe` 빌드, Steam VDF 업로드 검증 | ~15분 |

### TA팀장 (unity-technical-artist)

| # | 태스크 | 산출물 | 예상 시간 |
|---|--------|--------|-----------|
| TA-4 | 아트 에셋 내 Nodebreaker 잔재 확인 | 스프라이트/VFX 파일명 검색, 필요 시 변경 | ~10분 |

### 사운드 디렉터 (unity-sound-director)

| # | 태스크 | 산출물 | 예상 시간 |
|---|--------|--------|-----------|
| SD-1 | 사운드 에셋 내 Nodebreaker 잔재 확인 | 오디오 파일명/SoundManager 경로 검색, 필요 시 변경 | ~10분 |

### PM (project-manager)

| # | 태스크 | 산출물 | 예상 시간 |
|---|--------|--------|-----------|
| PM-3 | Sprint5_Progress.md 진행 현황 갱신 + Notion 동기화 | Phase별 완료/진행/대기 상태 실시간 추적 | 상시 |

---

## 업무 의존성

```
Phase 1 (병렬 착수)
├─ [프로그래밍: PG-1 네임스페이스 변경] ──────────────────────────────┐
├─ [프로그래밍: PG-2 MenuItem 경로 변경] ─────────────────────────────┤
├─ [빌더: BLD-1~4 프로젝트설정/빌드스크립트/VDF] ────────────────────┤
├─ [기획: GD-1 기획 문서 세계관 용어 적용] ───────────────────────────┤
├─ [PM: PM-1~2 CLAUDE.md/에이전트/스킬/DevPD Guidelines] ───────────┤
│                                                                   ↓
│                                       [QA: QA-1 컴파일/빌드 검증] ──┐
│                                                                     │
Phase 2 (Phase 1 완료 후, 내부 병렬)                                   │
├─ [기획: GD-2 SO 수치 조정] ───────────────────────────────────────┤
├─ [기획: GD-3 Early Game Flow SO] ──→ [PG-6 Early Game 코드] ────┤
├─ [기획: GD-4 보물상자 타워 획득 데이터] ──→ [PG-5 보물상자 변경] ──┤
├─ [프로그래밍: PG-4 재화 변수 리네이밍 bit→soul] ───────────────────┤
├─ [UI: UI-1 세계관 용어 UI 적용] ───────────────────────────────────┤
├─ [UI: UI-2 보물상자 UI 변경] ──────────────────────────────────────┤
├─ [TA: TA-1 UI 프레임/패널 스프라이트] ─────────────────────────────┤
├─ [TA: TA-2 타이틀 배경/로고] ──────────────────────────────────────┤
├─ [TA: TA-3 재화 아이콘 리뉴얼] ────────────────────────────────────┤
│                                                   ↓
│                                       [QA: QA-2 Phase 2 단위 QA]
│                                                   ↓
Phase 3 (Phase 2 QA 통과 후)
├─ [기획: GD-5 밸런스 조정] ──── (QA-3 피드백 반영)
├─ [프로그래밍: PG-7 세이브 시스템 정비]
├─ [프로그래밍: PG-8 버그 수정] ── (QA-2 이슈 대응)
├─ [TA: TA-4 아트 에셋 잔재 확인]
├─ [사운드: SD-1 사운드 에셋 확인]
│                           ↓
│               [QA-3 Early Game Flow 통합 QA]
│               [QA-4 리네이밍 전역 QA]
│                           ↓
│               [빌더: BLD-5 Git Remote 변경]
│                           ↓
│               [빌더: BLD-6 빌드 + Steam 업로드] ── 선행: 통합 QA 통과 + 총괄PD 승인
```

---

## 파일 소유권 (충돌 방지)

| 파일/영역 | 소유자 |
|-----------|--------|
| SO 에셋 (수치 데이터) | 기획팀장 |
| Assets/Project/Scripts/** (네임스페이스, 재화 변수명) | 프로그래밍팀장 |
| Assets/Project/Scripts/UI/** (UI 텍스트) | UI팀장 |
| Assets/Art/** (스프라이트, UI 프레임, 아이콘) | TA팀장 |
| ProjectSettings/ProjectSettings.asset | 빌더 |
| Assets/Editor/BuildScript.cs | 빌더 |
| SteamBuild/** | 빌더 |
| Docs/** | PM, 기획팀장 |
| .claude/agents/**, .claude/prompts/skills/** | PM |
| CLAUDE.md | PM |

---

## Git 브랜칭

```
sprint5                    ← 메인 작업 브랜치 (QA팀장만 머지 가능)
  ├─ dev/programmer        ← 프로그래밍팀장
  ├─ dev/ui                ← UI팀장
  ├─ dev/ta                ← TA팀장
  ├─ dev/game-designer     ← 기획팀장
  ├─ dev/sound             ← 사운드 디렉터
  └─ dev/build             ← 빌더
```

**워크플로우**: 팀장 dev/* 작업 + 자체 QA + 커밋 -> QA팀장 검증 -> sprint5 머지

**Sprint 5 브랜치 생성**: `feature/phase1-core-loop`에서 `sprint5` 분기

**Phase 1 리베이스 전략**: PG-1~3 완료 + QA-1 통과 → sprint5 머지 → 모든 dev/* 브랜치를 sprint5 기준으로 리베이스 (네임스페이스 변경이 모든 파일에 영향하므로 Phase 2 시작 전 필수)

---

## 완료 기준

모든 항목 충족 시 Sprint 5 완료:

### Phase 1
- [ ] C# 네임스페이스 `Nodebreaker.*` → `Soulspire.*` 52개 파일 변경 완료
- [ ] MenuItem 경로 `Tools/Nodebreaker/` → `Tools/Soulspire/` 20+건 변경 완료
- [ ] ProjectSettings.asset `companyName: SBGames`, `productName: Soulspire` 변경 완료
- [ ] BuildScript.cs, VDF 3종, upload.bat/sh에 `Soulspire.exe` 반영 완료
- [ ] GDD + 기획 문서 세계관 용어 전면 적용 완료
- [ ] CLAUDE.md, 에이전트, 스킬 내 경로/명칭 일괄 변경 완료
- [ ] 컴파일 에러 0건, 빌드 성공 확인

### Phase 2
- [ ] 재화 변수명 `bit` → `soul`, `core` → `coreFragment` 코드 전역 변경 완료
- [ ] UI 세계관 용어 전체 적용 (Soul/Core Fragment/Sanctum/Incursion/Floor/EMBARK)
- [ ] SO 수치 조정 (P0 7건) 완료
- [ ] Early Game Flow SO 데이터 (타워 1기 시작) 완료
- [ ] 보물상자 시스템 변경 (런 내 버프 → 타워 획득) 완료
- [ ] 보물상자 UI 변경 (3택 타워 선택) 완료
- [ ] TA 컨셉 기반 UI 프레임/패널/재화 아이콘 적용 완료

### Phase 3
- [ ] Early Game Flow 동작 (타워 1기 → Floor 1 패배 → Soul 획득 → Sanctum 업그레이드 → Floor 2 Incursion 1 생존)
- [ ] 리네이밍 전역 QA 통과 (UI/로그/에러에 "Nodebreaker"/"Bit"/"Hub"/"Wave"/"Stage" 잔재 0건)
- [ ] Git Remote `test1` → `soulspire` 변경 완료 (worktree URL 업데이트 포함)
- [ ] Windows 빌드 성공 + Steam dev_test 업로드 완료 (Soulspire.exe)
- [ ] 총괄PD 빌드 승인

---

## 리스크 및 완화책

| 리스크 | 영향 | 완화 |
|--------|------|------|
| 네임스페이스 일괄 변경 시 using 누락 | 컴파일 에러 대량 발생 | Phase 1 QA에서 전체 컴파일 검증, PG-3에서 using 일괄 수정 |
| 세이브 파일명 변경 시 기존 데이터 초기화 | 기존 플레이어 세이브 리셋 | 총괄PD 결정: 프로토타입 세이브 리셋 허용, Tesseract Save 통합 |
| Git Remote 변경 시 worktree URL 불일치 | push/pull 실패 | BLD-5에서 모든 worktree `.git/config` URL 일괄 업데이트 |
| 52개 파일 네임스페이스 변경 시 충돌 | dev/* 브랜치 머지 실패 | Phase 1 완료 후 즉시 전 브랜치에 리베이스 필수 |
| 세계관 용어 변경 범위 확대 (6개 용어) | 리네이밍 누락 가능 | QA-4에서 전역 잔재 검색 필수 |

---

## 공수 산정 근거

| 항목 | 값 |
|------|-----|
| 투입 팀 | 8개 (기획/프로그래밍/UI/TA/QA/사운드/빌더/PM) |
| 병렬 작업 가능 팀 수 | Phase 1: 5팀, Phase 2: 6팀, Phase 3: 7팀 |
| Phase 1 예상 소요 | ~35분 (병렬 기준) |
| Phase 2 예상 소요 | ~35분 (병렬 기준, TA 에셋 제작 포함) |
| Phase 3 예상 소요 | ~25분 (병렬 기준) |
| 총 예상 소요 | ~1.5~2시간 (Phase 간 QA 게이트 포함) |
| 총 태스크 수 | 28건 (기획 5, 프로그래밍 8, UI 2, TA 4, QA 4, 사운드 1, 빌드 6, PM 3) |

---

## 총괄PD 결정 사항 (확정)

| # | 항목 | 결정 |
|---|------|------|
| 1 | ProjectSettings.asset companyName | **SBGames** |
| 2 | 세이브 시스템 | **세이브 리셋** (마이그레이션 불필요) + **PlayerPrefs 사용 금지** (Tesseract Save 통합) |
| 3 | 세계관 용어 체계 | **TA 컨셉 아트 기준 채택**: Soul/Core Fragment/Sanctum/Incursion/Floor/EMBARK |

---

## TA 컨셉 아트 레퍼런스 (Sprint 5 기준)

| 화면 | 파일 (dev/ta 브랜치) | 핵심 비주얼 요소 |
|------|---------------------|-----------------|
| Title | `Assets/Art/Concepts/Sprint5/Concept_TitleScreen.png` | 마법 첨탑 실루엣, 석재+금박 프레임, PLAY/SETTINGS/QUIT |
| Sanctum (Hub) | `Assets/Art/Concepts/Sprint5/Concept_HubScreen.png` | 룬 각성 트리, Floor 선택, Soul/Core Fragment 표시, EMBARK |
| InGame Battle | `Assets/Art/Concepts/Sprint5/Concept_InGameBattle.png` | 아이소메트릭 타일맵, 타워 색상 코딩, 몬스터 HP바, 타워 인벤토리 |
| RunEnd | `Assets/Art/Concepts/Sprint5/Concept_RunEnd.png` | RUN COMPLETE 패널, Soul/Core Fragment 보상, RETURN TO SANCTUM/RETRY |

---

## GDD v4.0 기획 변경 요약 (Sprint 5 반영 대상)

### Early Game Flow
- **AS-IS**: 타워 0기, 플레이어가 첫 타워 구매
- **TO-BE**: 타워 1기 시작, Floor 1 전체 패배 (패배 전 몬스터 2~3마리 처치 가능), Soul 획득 → Sanctum 1회 업그레이드 → Floor 2 Incursion 1 생존

### 재화 시스템
- **AS-IS**: Bit (런 내 + 영구)
- **TO-BE**: Soul (런 내 + 영구 공통 명칭)

### 보물상자 시스템
- **AS-IS**: 런 내 버프 (공격력+, 공속+ 등)
- **TO-BE**: 타워 획득 전용, 보스 확정 드롭, Sanctum 오픈, 3지선다 타워 선택

### SO 수치 조정 (P0 7건)
1. Skill "공격력 증가" 비용/성장률 밸런스
2. Skill "공격속도 증가" 비용/성장률 밸런스
3. Skill "기지 HP 증가" 비용/성장률 밸런스
4. Floor 1 전체 Incursion 구성 (2~3마리 처치 후 패배 유도)
5. Floor 2 Incursion 1 몬스터 수 조정 (Sanctum 업그레이드 1회 후 생존 가능)
6. Floor 1 몬스터 Soul 드롭량 (Sanctum 1회 업그레이드 가능한 수준)
7. Floor 1 초기 타워 1기 배치 (SO 또는 코드 설정)

---

## Sprint 5 특별 고려사항

### 1. 네임스페이스 변경 시 Git 충돌 최소화
- Phase 1 완료 후 즉시 모든 dev/* 브랜치에 리베이스 필수
- 프로그래밍팀장 PG-1 작업 완료 → QA-1 통과 → 즉시 dev/ui, dev/ta, dev/game-designer에 반영

### 2. 세이브 시스템 정비
- 기존: Tesseract Save (`nodebreaker_save.json`) + PlayerPrefs (FTUEManager, SoundManager) 혼용
- 변경: Tesseract Save (`soulspire_save.json`) 단일 사용, PlayerPrefs 사용 금지
- 기존 세이브 리셋 (프로토타입 단계, 마이그레이션 불필요)

### 3. 세계관 용어 적용 범위
- **코드 변수명 변경**: bit→soul, core→coreFragment (PlayerSaveData, RunManager, MetaManager 등)
- **UI 텍스트만 변경**: Hub→Sanctum, Wave→Incursion, Stage→Floor (코드 클래스명은 유지 — HubUI, WaveManager, StageData 등)
- **이유**: 코드 클래스명까지 변경하면 리팩토링 범위 과다. UI 표시명만 세계관 용어로 전환

### 4. Steam 설치 폴더명
- 현재: `C:\Program Files (x86)\Steam\steamapps\common\Nodebreaker TD\`
- 변경: Steamworks 파트너 사이트에서 수동 변경 필요 (빌드 스크립트 외부)
- Sprint 5 범위 외: 총괄PD 직접 처리 또는 후속 스프린트

### 5. Git Worktree URL 업데이트
- 현재: `git remote -v` → `origin https://github.com/happyone7/test1.git`
- 변경 후: `origin https://github.com/happyone7/soulspire.git`
- 모든 worktree (7개) `.git/config` URL 일괄 업데이트 필수

---

## 주요 결정 사항

| 날짜 | 내용 | 비고 |
|------|------|------|
| 2026-02-17 | Sprint 5 목표: Nodebreaker → Soulspire 리네이밍 + 세계관 용어 적용 | 총괄PD 지시 |
| 2026-02-17 | 세계관 용어 체계 확정: Soul/Core Fragment/Sanctum/Incursion/Floor/EMBARK | TA 컨셉 아트 기준, 총괄PD 승인 |
| 2026-02-17 | companyName: SBGames 확정 | 총괄PD 결정 |
| 2026-02-17 | 세이브 리셋 + PlayerPrefs 금지 + Tesseract Save 통합 | 총괄PD 결정 |
| 2026-02-17 | 보물상자 시스템 변경 (런 내 버프 → 타워 획득 전용) 확정 | GDD v4.0 반영 |
| 2026-02-17 | Git Remote test1 → soulspire 변경 확정 | 빌더 담당 |

---

## 다음 스프린트 예고 (Sprint 6 후보)

1. Lightning Tower 구현 (프로그래밍팀장)
2. Floor 4~5 데이터 + 맵 레이아웃 (기획팀장)
3. Idle Soul 수집기 구현 (프로그래밍팀장)
4. 타워 판매 기능 (프로그래밍팀장)
5. 컨셉 아트 기반 InGame/Sanctum UI 전면 리뉴얼 (TA + UI)
6. 사운드 본격 폴리싱 (사운드 디렉터)
7. 온보딩/튜토리얼 팝업 개선 (UI팀장)
