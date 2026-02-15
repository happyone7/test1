# Soulspire QA Sheet

**프로젝트**: Soulspire (Nodebreaker TD)
**스프린트**: Sprint 3
**최종 업데이트**: 2026-02-15
**작성자**: QA팀장 (unity-qa-engineer)
**대상 브랜치**: `sprint3`

---

## Sprint 3 QA 항목

### 타워

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 담당 브랜치 | 메모 |
|----|---------|------|---------|------|------------|------|
| Q01 | 타워 | Arrow Tower 단일 타겟 공격 정상 동작 | P0 | PENDING | sprint3 (기존) | 기존 기능 회귀 테스트 |
| Q02 | 타워 | Arrow Tower 스프라이트 정상 렌더링 | P0 | PENDING | sprint3 (기존) | 2D 전환 후 검증 |
| Q03 | 타워 | Cannon Tower AoE 폭발 공격 동작 | P0 | PENDING | dev/programmer | PG-2 |
| Q04 | 타워 | Cannon Tower AoE 범위 내 복수 적 피격 확인 | P0 | PENDING | dev/programmer | PG-2 |
| Q05 | 타워 | Cannon Tower 스프라이트 렌더링 | P1 | PENDING | sprint3 (기존) | TA-1 스프라이트 |
| Q06 | 타워 | Ice Tower 감속 디버프 적용 | P0 | PENDING | dev/programmer | PG-2 |
| Q07 | 타워 | Ice Tower 감속 효과 지속시간/중첩 확인 | P1 | PENDING | dev/programmer | PG-2 |
| Q08 | 타워 | Ice Tower 스프라이트 렌더링 | P1 | PENDING | sprint3 (기존) | TA-1 스프라이트 |
| Q09 | 타워 | 타워 배치 슬롯 동작 정상 | P0 | PENDING | sprint3 (기존) | 기존 기능 회귀 |
| Q10 | 타워 | 타워 SO 데이터 참조 정상 (TowerData) | P0 | PENDING | sprint3 (기존) | |

### 몬스터

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 담당 브랜치 | 메모 |
|----|---------|------|---------|------|------------|------|
| Q11 | 몬스터 | Bit Node 기본 이동/피격/사망 동작 | P0 | PENDING | sprint3 (기존) | 회귀 테스트 |
| Q12 | 몬스터 | Quick Node 이동속도가 Bit 대비 빠름 확인 | P0 | PENDING | dev/programmer | PG-3 |
| Q13 | 몬스터 | Quick Node 스프라이트 렌더링 | P1 | PENDING | sprint3 (기존) | TA-2 스프라이트 |
| Q14 | 몬스터 | Heavy Node HP가 Bit 대비 높음 확인 | P0 | PENDING | dev/programmer | PG-3 |
| Q15 | 몬스터 | Heavy Node 스프라이트 렌더링 | P1 | PENDING | sprint3 (기존) | TA-2 스프라이트 |
| Q16 | 몬스터 | Shield Node SO 데이터 정상 (Node_Shield.asset) | P1 | PENDING | dev/game-designer | GD-3 신규 |
| Q17 | 몬스터 | 웨이브 스폰 시 몬스터 종류 혼합 정상 | P0 | PENDING | dev/programmer | |
| Q18 | 몬스터 | 몬스터 경로 따라 이동 정상 | P0 | PENDING | sprint3 (기존) | 회귀 테스트 |

### UI - 스킬트리

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 담당 브랜치 | 메모 |
|----|---------|------|---------|------|------------|------|
| Q19 | UI/스킬트리 | SkillTreeUI 18노드 배치 표시 | P0 | PENDING | dev/ui | UI-3 |
| Q20 | UI/스킬트리 | 스킬트리 줌 (마우스 스크롤) 동작 | P0 | PENDING | dev/ui | UI-3 |
| Q21 | UI/스킬트리 | 스킬트리 패닝 (드래그) 동작 | P0 | PENDING | dev/ui | UI-3 |
| Q22 | UI/스킬트리 | 스킬트리 키보드 패닝 (WASD) 동작 | P1 | PENDING | dev/ui | UI-3 |
| Q23 | UI/스킬트리 | 스킬트리 키보드 줌 (+/-) 동작 | P2 | PENDING | dev/ui | UI-3 |
| Q24 | UI/스킬트리 | 노드 연결선 표시 정상 | P0 | PENDING | dev/ui | UI-3 |
| Q25 | UI/스킬트리 | 노드 상태별 시각 표현 (Hidden/Locked/Available/Purchased/Maxed) | P0 | PENDING | dev/ui | UI-3 |
| Q26 | UI/스킬트리 | 구매 팝업 표시 (노드 클릭 시) | P0 | PENDING | dev/ui | UI-3 |
| Q27 | UI/스킬트리 | Bit 노드 구매 (반복 구매형) Before/After 표시 | P0 | PENDING | dev/ui | UI-3 |
| Q28 | UI/스킬트리 | Core 노드 구매 (1회 구매형) 효과 설명 표시 | P0 | PENDING | dev/ui | UI-3 |
| Q29 | UI/스킬트리 | 잠긴 노드 클릭 시 잠금 안내 + 선행 노드 목록 표시 | P1 | PENDING | dev/ui | UI-3 |
| Q30 | UI/스킬트리 | 구매 후 노드 상태 갱신 + 연결선 갱신 | P0 | PENDING | dev/ui | UI-3 |
| Q31 | UI/스킬트리 | 자원 부족 시 비용 텍스트 빨간색 표시 | P1 | PENDING | dev/ui | UI-3 |
| Q32 | UI/스킬트리 | 최대 레벨 도달 시 MAX 표시 + 버튼 비활성화 | P1 | PENDING | dev/ui | UI-3 |

### UI - 기본 화면

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 담당 브랜치 | 메모 |
|----|---------|------|---------|------|------------|------|
| Q33 | UI | 타이틀 화면 정상 표시 | P0 | PENDING | sprint3 (기존) | 회귀 |
| Q34 | UI | 허브 화면 정상 표시 + 버튼 동작 | P0 | PENDING | sprint3 (기존) | 회귀 |
| Q35 | UI | 인게임 HUD 표시 (웨이브 카운터, HP바 등) | P0 | PENDING | sprint3 (기존) | 회귀 |
| Q36 | UI | UI 에셋 16개 PNG 적용 확인 (버튼/프레임/아이콘) | P0 | PENDING | sprint3 (기존) | UI-1 |
| Q37 | UI | Hub에서 스킬트리 화면 전환 정상 | P0 | PENDING | dev/ui | |

### 사운드

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 담당 브랜치 | 메모 |
|----|---------|------|---------|------|------------|------|
| Q38 | 사운드 | BGM Hub 재생 정상 | P0 | PENDING | sprint3 (기존) | SD-2 교체 후 |
| Q39 | 사운드 | BGM Combat 재생 정상 | P0 | PENDING | sprint3 (기존) | SD-2 교체 후 |
| Q40 | 사운드 | SFX Tower Attack 트리거 정상 | P0 | PENDING | sprint3 (기존) | SD-1 |
| Q41 | 사운드 | SFX Node Die 트리거 정상 | P0 | PENDING | sprint3 (기존) | SD-1 |
| Q42 | 사운드 | SFX Stage Clear 트리거 정상 | P0 | PENDING | sprint3 (기존) | SD-1 |
| Q43 | 사운드 | 볼륨 밸런스 (BGM vs SFX 비율) 적절 | P1 | PENDING | sprint3 (기존) | SD-1 |
| Q44 | 사운드 | BGM 화면 전환 시 크로스페이드/전환 정상 | P1 | PENDING | sprint3 (기존) | |

### 스킬트리 로직

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 담당 브랜치 | 메모 |
|----|---------|------|---------|------|------------|------|
| Q45 | 스킬트리 | MetaManager 스킬 레벨 저장/불러오기 | P0 | PENDING | dev/ui | |
| Q46 | 스킬트리 | 전제 조건 미충족 시 구매 불가 | P0 | PENDING | dev/ui | |
| Q47 | 스킬트리 | Bit 자원 차감 정상 (구매 시) | P0 | PENDING | dev/ui | |
| Q48 | 스킬트리 | Core 자원 차감 정상 (구매 시) | P0 | PENDING | dev/ui | |
| Q49 | 스킬트리 | 스킬 효과가 RunModifiers에 반영 | P0 | PENDING | dev/ui | |
| Q50 | 스킬트리 | 초기 Bit 500 지급 (신규 세이브) | P1 | PENDING | dev/ui | |
| Q51 | 스킬트리 | 노드 가시성 (선행 노드 1개 구매 시 표시) | P0 | PENDING | dev/ui | |

### 밸런스/웨이브

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 담당 브랜치 | 메모 |
|----|---------|------|---------|------|------------|------|
| Q52 | 밸런스 | 스테이지 1 웨이브 1~5 정상 스폰 | P0 | PENDING | dev/game-designer | Wave_01~05 |
| Q53 | 밸런스 | 스테이지 1 후반 웨이브에 Quick/Heavy 혼합 등장 | P0 | PENDING | dev/game-designer | Wave_03~05 수정됨 |
| Q54 | 밸런스 | 스테이지 2 웨이브 1~7 정상 스폰 | P1 | PENDING | dev/game-designer | Wave_S2_01~07 신규 |
| Q55 | 밸런스 | 스테이지 3 웨이브 1~8 정상 스폰 | P1 | PENDING | dev/game-designer | Wave_S3_01~08 신규 |
| Q56 | 밸런스 | Stage_02 SO 데이터 정상 (hpMultiplier=1.8, core=3) | P1 | PENDING | dev/game-designer | |
| Q57 | 밸런스 | Stage_03 SO 데이터 정상 (hpMultiplier=3.0, core=4) | P1 | PENDING | dev/game-designer | |
| Q58 | 밸런스 | 난이도 곡선 체감 확인 (스테이지별 점진적 난이도 증가) | P2 | PENDING | dev/game-designer | 플레이테스트 필요 |

### 아트 에셋

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 담당 브랜치 | 메모 |
|----|---------|------|---------|------|------------|------|
| Q59 | 아트 | 스킬 아이콘 8종 임포트 정상 (skill_*.png) | P0 | PENDING | dev/ta | TA-3 |
| Q60 | 아트 | 스킬 아이콘 .meta 파일 존재 및 Sprite 설정 | P0 | PENDING | dev/ta | TA-3 |
| Q61 | 아트 | 스킬 아이콘 PPU=128, filterMode=Point 설정 | P1 | PENDING | dev/ta | 픽셀아트 선명도 |
| Q62 | 아트 | Arrow 투사체 PPU 1024->64 수정 반영 | P0 | PENDING | dev/ta | 인게임 가시성 |
| Q63 | 아트 | 타이틀 로고 재제작 반영 (SoulspireLogo_02.png) | P1 | PENDING | dev/game-designer | 투명 배경 |

### 통합/컴파일

| ID | 카테고리 | 항목 | 우선순위 | 상태 | 담당 브랜치 | 메모 |
|----|---------|------|---------|------|------------|------|
| Q64 | 통합 | 모든 .cs 파일 컴파일 에러 없음 | P0 | PASS (정적) | sprint3 | 37개 .cs + 19개 Editor .cs 전수 검증. .meta 56/56 정합. namespace/타입 참조 정상. MCP 미응답으로 에디터 컴파일 미확인 |
| Q65 | 통합 | Console에 에러 로그 없음 (경고 허용) | P0 | BLOCKED | sprint3 | MCP 세션 미응답 (Unity ping timeout). 에디터 콘솔 확인 불가. 수동 확인 필요 |
| Q66 | 통합 | 씬 로드 시 Missing Reference 없음 | P0 | BLOCKED | sprint3 | MCP 세션 미응답. manage_scene/get_hierarchy 실행 불가. 수동 확인 필요 |
| Q67 | 통합 | 에디터 Play 모드 정상 진입 | P0 | BLOCKED | sprint3 | MCP 세션 미응답. manage_editor/play 실행 불가. 수동 확인 필요 |
| Q68 | 통합 | 타이틀 -> 허브 -> 인게임 -> 런 종료 플로우 정상 | P0 | BLOCKED | sprint3 | MCP 세션 미응답. Play 모드 진입 불가. 수동 확인 필요 |

---

## 검증 결과 요약

### 브랜치별 코드 검증 (Phase 3 - QA팀장)

| 브랜치 | 커밋 | 변경 요약 | .meta 검증 | 코드 검증 | 머지 상태 |
|--------|------|-----------|-----------|-----------|----------|
| dev/ui | d810a9d, e8b7b88 | 스킬트리 UI 시스템 (6개 .cs + 씬 + 에디터) | PASS | PASS | MERGED (45613ec) |
| dev/game-designer | 014694a, 0e32a8f, 383865b | 난이도 곡선 SO (Stage 2~3, Wave 19개, Node_Shield) + 로고 + 맵 레이아웃 | PASS | N/A (코드 없음) | MERGED (0aea38d) |
| dev/ta | daff3af, 67150de | 스킬 아이콘 8종 + 투사체 PPU 수정 | PASS | N/A (코드 없음) | MERGED (11282c8) |
| dev/programmer | 08c4487 | Cannon/Ice Tower, Quick/Heavy Monster, Tilemap 4-layer, defense, MetaManager SaveManager 전환 | PASS | PASS | MERGED (08c4487) |

### Sprint 3 최종 통합 QA (Q64~Q68)

| 항목 | 검증 방법 | 결과 | 비고 |
|------|----------|------|------|
| Q64 - 컴파일 에러 | 정적 코드 분석 (37+19개 .cs 전수 검토) | PASS (정적) | .meta 56/56 정합, namespace/타입 참조 정상, Tesseract 패키지 의존성 확인 |
| Q65 - 에러 로그 | Unity MCP read_console | BLOCKED | MCP 세션 미응답 (ping timeout 반복) |
| Q66 - Missing Ref | Unity MCP manage_scene | BLOCKED | MCP 세션 미응답 |
| Q67 - Play 모드 | Unity MCP manage_editor | BLOCKED | MCP 세션 미응답 |
| Q68 - 게임 플로우 | Unity MCP Play + 수동 확인 | BLOCKED | MCP 세션 미응답 |

**BLOCKER**: Unity MCP 세션(localhost:8080)이 HTTP 404 응답(서버 자체는 활성)하나, 내부 ping 응답이 없어 모든 에디터 제어 명령이 실패함.
- `refresh_unity`: 60초 timeout 반복
- `read_console`: "Unity session not ready" 반복
- `manage_scene`: TimeoutError
- `manage_editor/telemetry_ping`: queued 성공하나 응답 없음

**추정 원인**: feature/phase1-core-loop -> sprint3 브랜치 전환 후 대량 에셋 re-import 중 MCP 서버 세션이 끊어진 것으로 판단. Unity 에디터에서 Window > MCP for Unity > Start Server 재시작 필요.

**권장 조치**:
1. 총괄PD가 Unity 에디터에서 MCP 서버 수동 재시작
2. QA팀장이 Q65~Q68 재검증 수행
3. 또는 총괄PD가 에디터에서 수동으로 Q65~Q68 직접 확인

---

## 변경 이력

| 날짜 | 내용 |
|------|------|
| 2026-02-15 | Sprint 3 QA 시트 초기 생성 (QA팀장) |
| 2026-02-15 | dev/ui, dev/game-designer, dev/ta 브랜치 검증 통과 및 sprint3 머지 완료 |
| 2026-02-15 | Sprint 3 최종 통합 QA (Q64~Q68) 수행. Q64 정적 PASS, Q65~Q68 MCP BLOCKED |
