# 기획 문서 동기화 방안 제안서

| 항목 | 내용 |
|------|------|
| 작성자 | 기획팀장 (GameDesigner) |
| 작성일 | 2026-02-17 |
| 대상 | 총괄PD, 개발PD |
| 상태 | 제안 (승인 대기) |

---

## 1. 현황 분석

### 1.1 CLAUDE.md 정책 (현행 규정)

CLAUDE.md에는 다음과 같이 명시되어 있다.

> - 모든 디자인 문서는 **Notion**에서 관리 (git 브랜치 독립)
> - 에이전트가 기획서 참조 시 Notion fetch 도구 사용
> - git의 Docs/Design/은 레거시 아카이브로만 유지, **신규 문서는 Notion에만 생성**

### 1.2 에이전트 설정 실태 (정책과 불일치)

모든 에이전트의 `.md` 설정 파일은 **로컬 md 파일만 참조**하도록 되어 있다. Notion 참조 지시는 어디에도 없다.

| 에이전트 | 설정 파일 | 참조하는 기획서 | Notion 지시 |
|----------|-----------|----------------|-------------|
| game-designer | `.claude/agents/game-designer.md` | `Docs/Design/GDD.md` (v2.0으로 표기) | 없음 |
| unity-gameplay-programmer | `.claude/agents/unity-gameplay-programmer.md` | 없음 (코드 구조만 참조) | 없음 |
| unity-ui-developer | `.claude/agents/unity-ui-developer.md` | 없음 (UI 구조만 참조) | 없음 |
| unity-technical-artist | `.claude/agents/unity-technical-artist.md` | `Docs/Design/ArtDirection_v0.1.md`, `Docs/Design/GDD.md` | 없음 |
| unity-sound-director | `.claude/agents/unity-sound-director.md` | `Docs/Design/GDD.md`, `Docs/Design/ArtDirection_v0.1.md` | 없음 |
| unity-qa-engineer | `.claude/agents/unity-qa-engineer.md` | 없음 (QA 체크리스트만 참조) | 없음 |
| project-manager | `.claude/agents/project-manager.md` | `Docs/Design/GDD.md` (v2.0으로 표기) | 없음 |

### 1.3 로컬 기획 문서 현황

`Docs/Design/` 에 24개의 마크다운 파일과 6개의 PPT 파일이 존재한다.

주요 문서와 버전 상태:

| 문서 | 로컬 버전 | 비고 |
|------|-----------|------|
| GDD.md | v3.0 (2026-02-15) | 에이전트 설정에는 "v2.0"으로 표기됨 -- 이미 불일치 |
| ArtDirection_v0.1.md | v0.1 | TA, 사운드 디렉터가 참조 |
| BalanceSheet_v0.1.md | v0.1 | 밸런싱 수치 |
| SkillTree_Spec.md | 버전 미표기 | 스킬 트리 명세 |
| TreasureChestDesign.md | 버전 미표기 | 보물상자 기획 |
| Phase3_ContentExpansion.md | 버전 미표기 | 콘텐츠 확장 기획 |

**핵심 문제**: game-designer 에이전트 설정에 "GDD v2.0"이라고 명시되어 있지만, 실제 GDD.md는 이미 v3.0이다. 에이전트 설정 파일 자체가 로컬 문서 버전 변경을 반영하지 못하는 구조적 문제가 존재한다.

### 1.4 Notion 활용 실태

CLAUDE.md 정책과 달리, 현재까지 에이전트가 작업 시 Notion을 참조한 사례가 확인되지 않는다. 실무적으로는 로컬 md 파일이 사실상의 기준 문서(source of truth)로 기능하고 있다.

---

## 2. 방안별 비교

### A안: Notion만 사용 (매번 fetch)

**설명**: 에이전트가 작업 시작 시 매번 Notion에서 최신 기획서를 fetch한다. 로컬 md는 완전히 폐기하거나 읽기 전용 아카이브로만 유지한다.

| 항목 | 평가 |
|------|------|
| 문서 일관성 | 최상 -- Notion이 유일한 source of truth |
| 속도 | 최하 -- 페이지당 2~5초, 에이전트가 GDD+ArtDirection+BalanceSheet 3개만 참조해도 6~15초 |
| 오프라인 작업 | 불가 -- Notion 서버 장애 시 에이전트 작업 불가 |
| 에이전트 구현 복잡도 | 중 -- 모든 에이전트에 Notion fetch 절차 추가 필요 |
| MCP 토큰 소비 | 높음 -- fetch 호출마다 API 토큰 소비, 팀 전체 8개 에이전트 x 매 작업 |

**장점**:
- 문서 단일 진실 원천(single source of truth) 확보
- 기획서 수정 시 즉시 모든 에이전트에 반영

**단점**:
- 누적 지연 심각: 스프린트당 수십~수백 회 fetch 발생
- Notion API 장애 시 전체 팀 작업 중단 리스크
- 에이전트 컨텍스트 윈도우를 Notion 원문으로 과다 소비 (Notion 문서는 포맷팅 노이즈가 많음)

### B안: 로컬 md만 사용 (Notion 미사용)

**설명**: CLAUDE.md의 Notion 정책을 철회하고, 로컬 `Docs/Design/` md 파일을 공식 source of truth로 전환한다. Notion은 외부 공유용/백업용으로만 유지한다.

| 항목 | 평가 |
|------|------|
| 문서 일관성 | 중 -- git 커밋으로 버전 관리 가능하나, 사람(총괄PD)이 Notion에서 먼저 수정하면 불일치 발생 |
| 속도 | 최상 -- 로컬 파일 읽기, 지연 없음 |
| 오프라인 작업 | 가능 |
| 에이전트 구현 복잡도 | 최하 -- 현재 상태 그대로 |
| MCP 토큰 소비 | 없음 |

**장점**:
- 현재 실태와 동일 -- 추가 구현 비용 제로
- 로컬 파일이므로 속도 최상, git 히스토리로 변경 추적 가능
- 에이전트 컨텍스트에 바로 로드 가능

**단점**:
- 총괄PD가 Notion에서 기획을 먼저 수정하는 경우, 수동으로 로컬 md에 반영해야 함
- Notion의 협업/코멘트/미디어 등 장점을 포기
- CLAUDE.md 정책 전면 수정 필요

### C안: 하이브리드 (로컬 md 기본 + 스프린트 시작 시 Notion 동기화)

**설명**: 평소에는 로컬 md를 기준으로 작업하되, 스프린트 시작 시점에 Notion에서 최신 기획서를 내려받아 로컬 md를 갱신한다.

| 항목 | 평가 |
|------|------|
| 문서 일관성 | 상 -- 스프린트 단위로 동기화, 스프린트 중간에는 로컬 기준 |
| 속도 | 상 -- 평소 작업 시 로컬 참조, 동기화는 스프린트 전환 시 1회만 |
| 오프라인 작업 | 가능 (동기화 시점 제외) |
| 에이전트 구현 복잡도 | 중 -- 동기화 절차 + 동기화 담당자 지정 필요 |
| MCP 토큰 소비 | 낮음 -- 스프린트당 1회만 |

**장점**:
- 두 시스템의 장점 결합 -- Notion의 편의성 + 로컬의 속도
- 스프린트 경계에서 명확한 동기화 포인트

**단점**:
- 스프린트 중간에 기획이 급변하면 반영이 늦어짐
- "누가 동기화를 수행하는가"에 대한 책임 소재 필요
- 동기화 실수(누락, 부분 반영) 가능성

### D안: 로컬 md 기준 + 변경 시점 동기화 (추천안)

**설명**: 로컬 `Docs/Design/` md를 에이전트의 기준 문서로 확정하되, **기획 변경이 발생하는 시점에 즉시 동기화**한다. Notion은 총괄PD의 기획 작업 공간으로 유지하고, 기획 변경 확정 시 기획팀장이 로컬 md를 갱신한다.

| 항목 | 평가 |
|------|------|
| 문서 일관성 | 최상 -- 변경 시점에 즉시 동기화 |
| 속도 | 최상 -- 에이전트는 항상 로컬 참조 |
| 오프라인 작업 | 가능 |
| 에이전트 구현 복잡도 | 하 -- 에이전트 설정은 로컬 참조만, 동기화는 기획팀장 절차 |
| MCP 토큰 소비 | 최소 -- 기획 변경 시에만 Notion fetch 1회 |

**장점**:
- 에이전트는 항상 로컬 md만 읽으면 됨 (속도/안정성 최상)
- 기획 변경 시 기획팀장이 Notion -> 로컬 동기화를 명시적으로 수행하므로 책임 소재 명확
- git 커밋 히스토리에 기획 변경 이력이 남음 (추적성 확보)
- Notion의 편의 기능(코멘트, 미디어, 공유)은 총괄PD가 그대로 활용 가능

**단점**:
- 기획팀장의 동기화 작업 부담 (기획 변경마다 1회)
- 총괄PD가 Notion에서 수정 후 기획팀장에게 알리지 않으면 동기화 누락 가능

---

## 3. 추천 방안: D안 (로컬 md 기준 + 변경 시점 동기화)

### 추천 근거

1. **현실과 정합**: 현재 모든 에이전트가 이미 로컬 md를 기준으로 동작하고 있다. D안은 이 현실을 공식화하고, 부족한 동기화 절차만 보강한다.

2. **속도와 안정성**: 에이전트가 매 작업마다 Notion API를 호출하는 것은 비효율적이다. 로컬 파일 읽기는 0초, Notion fetch는 2~5초/페이지이며, 팀 규모(8 에이전트)를 고려하면 누적 지연이 심각하다.

3. **변경 빈도 기반 판단**: Soulspire는 프로토타입 단계로, 기획 문서 변경은 스프린트당 수 회 수준이다. 반면 에이전트의 문서 참조는 스프린트당 수십~수백 회이다. "변경은 드물고 참조는 빈번한" 상황에서는 변경 시점에 1회 동기화하는 것이 효율적이다.

4. **git 추적성**: 로컬 md를 기준으로 하면, 기획 변경 내역이 git 커밋 히스토리에 자연스럽게 남는다. "어떤 스프린트에서 어떤 기획이 변경되었는지"를 git log로 추적할 수 있다.

5. **장애 내성**: Notion 서버 장애, MCP 연결 불안정 등 외부 요인이 에이전트 작업을 중단시키지 않는다.

---

## 4. 구현 방법 (D안 실행 절차)

### 4.1 문서 흐름 정의

```
[총괄PD] Notion에서 기획 수정/확정
    |
    v
[총괄PD -> 기획팀장] "GDD 3.1로 업데이트됨, 반영해줘"
    |
    v
[기획팀장] Notion fetch -> 로컬 md 갱신 -> git 커밋
    |
    v
[모든 에이전트] 로컬 md 참조 (최신 상태 보장)
```

### 4.2 기획팀장의 동기화 절차

기획 변경 통보를 받으면 다음을 수행한다:

1. **Notion fetch**: `notion-fetch` MCP 도구로 해당 페이지 내용을 가져온다
2. **로컬 md 갱신**: `Docs/Design/` 내 해당 파일을 최신 내용으로 업데이트한다
3. **버전 번호 갱신**: 문서 상단의 버전 번호와 수정일을 업데이트한다
4. **에이전트 설정 동기화**: 에이전트 md에 명시된 버전 번호가 있으면 함께 갱신한다
5. **git 커밋**: `docs: [문서명] vX.Y 동기화 (Notion 반영)` 형식으로 커밋한다

### 4.3 로컬 md 문서 표준 헤더

모든 기획 문서에 다음 헤더를 적용하여 동기화 상태를 추적한다:

```markdown
| 항목 | 내용 |
|------|------|
| 문서 버전 | X.Y |
| 최종 수정 | YYYY-MM-DD |
| 작성자 | [작성자] |
| Notion 원본 | [Notion 페이지 ID 또는 URL] |
| 동기화 일시 | YYYY-MM-DD HH:MM |
```

### 4.4 CLAUDE.md 수정 사항

현행:
```
## 디자인 문서 관리 (Notion 중심)
- 모든 디자인 문서는 **Notion**에서 관리 (git 브랜치 독립)
- Notion 위치: Soulspire 프로젝트 > 디자인 문서
- 에이전트가 기획서 참조 시 Notion fetch 도구 사용
- git의 Docs/Design/은 레거시 아카이브로만 유지, **신규 문서는 Notion에만 생성**
```

변경안:
```
## 디자인 문서 관리 (로컬 md 기준 + Notion 동기화)
- **에이전트 기준 문서**: `Docs/Design/` 로컬 md 파일 (항상 최신 상태 유지)
- **총괄PD 작업 공간**: Notion (기획 초안 작성, 코멘트, 외부 공유용)
- **동기화 흐름**: Notion 기획 확정 -> 기획팀장이 로컬 md 갱신 -> git 커밋
- **동기화 책임**: 기획팀장 (game-designer)
- **동기화 시점**: 기획 변경 확정 시 즉시 (총괄PD가 기획팀장에게 통보)
- **에이전트는 Notion을 직접 참조하지 않음** (로컬 md만 참조)
```

---

## 5. 에이전트 설정 변경 사항

### 5.1 game-designer (기획팀장)

**파일**: `.claude/agents/game-designer.md`

**변경 1**: 프로젝트 컨텍스트의 GDD 버전 표기 수정
```
현행: - **GDD**: `Docs/Design/GDD.md` (v2.0)
변경: - **GDD**: `Docs/Design/GDD.md` (최신 버전은 파일 헤더 참조)
```

**변경 2**: "Notion 동기화 책임" 섹션 신규 추가
```markdown
## Notion -> 로컬 동기화 (기획팀장 전담)

총괄PD가 Notion에서 기획을 변경/확정하면, 기획팀장이 로컬 md를 갱신한다.

### 동기화 절차
1. `notion-fetch` MCP 도구로 Notion 페이지 내용 가져오기
2. `Docs/Design/` 내 해당 md 파일 갱신 (버전 번호, 수정일 포함)
3. 에이전트 설정 파일에 버전이 하드코딩되어 있으면 함께 갱신
4. `docs: [문서명] vX.Y 동기화 (Notion 반영)` 형식으로 커밋

### 동기화 대상 문서
| 문서 | 경로 | 참조 에이전트 |
|------|------|--------------|
| GDD | `Docs/Design/GDD.md` | game-designer, project-manager, technical-artist, sound-director |
| ArtDirection | `Docs/Design/ArtDirection_v0.1.md` | technical-artist, sound-director |
| BalanceSheet | `Docs/Design/BalanceSheet_v0.1.md` | game-designer |
| SkillTree_Spec | `Docs/Design/SkillTree_Spec.md` | game-designer, gameplay-programmer |
```

**변경 3**: 협업 섹션에 동기화 관련 내용 추가
```
현행 협업 항목에 추가:
- **총괄PD**: 설계 결과 보고, 밸런싱 방향 협의, **Notion 기획 변경 통보 수신 -> 로컬 md 동기화**
```

### 5.2 unity-gameplay-programmer (프로그래밍팀장)

**파일**: `.claude/agents/unity-gameplay-programmer.md`

**변경**: 기획서 참조 지침 추가 (현재는 기획서 참조 경로가 명시되어 있지 않음)
```markdown
## 기획서 참조
- 기획서는 `Docs/Design/` 로컬 md 파일을 참조한다 (Notion 직접 접근 불필요)
- 주요 참조 문서: `Docs/Design/GDD.md`, `Docs/Design/SkillTree_Spec.md`
- 기획 변경은 기획팀장이 로컬 md에 반영하므로, 로컬 파일이 항상 최신이다
```

### 5.3 unity-ui-developer (UI팀장)

**파일**: `.claude/agents/unity-ui-developer.md`

**변경**: 기획서 참조 지침 추가
```markdown
## 기획서 참조
- UI 명세 및 기획서는 `Docs/Design/` 로컬 md/pptx 파일을 참조한다
- 주요 참조 문서: `Docs/Design/GDD.md`, UI 명세 PPT 파일
- 기획팀장이 기획서를 로컬에 동기화하므로, 로컬 파일이 항상 최신이다
```

### 5.4 unity-technical-artist (TA)

**파일**: `.claude/agents/unity-technical-artist.md`

**변경**: 기존 참조 경로 유지, Notion 미참조 원칙 명시
```
현행 유지:
- **아트 디렉션**: `Docs/Design/ArtDirection_v0.1.md`
- **GDD**: `Docs/Design/GDD.md`

추가:
- 기획서는 로컬 md를 참조한다 (Notion 직접 접근 불필요, 기획팀장이 동기화)
```

### 5.5 unity-sound-director (사운드 디렉터)

**파일**: `.claude/agents/unity-sound-director.md`

**변경**: 기존 참조 경로 유지, Notion 미참조 원칙 명시
```
현행 유지:
- **GDD**: `Docs/Design/GDD.md`
- **아트 디렉션**: `Docs/Design/ArtDirection_v0.1.md`

추가:
- 기획서는 로컬 md를 참조한다 (Notion 직접 접근 불필요, 기획팀장이 동기화)
```

### 5.6 unity-qa-engineer (QA팀장)

**파일**: `.claude/agents/unity-qa-engineer.md`

**변경**: QA 기준 문서 참조 추가
```markdown
## 기획서 참조 (QA 기준)
- QA 판정 기준이 되는 기획서는 `Docs/Design/` 로컬 md를 참조한다
- 주요 참조: `Docs/Design/GDD.md` (게임 규칙), `Docs/Design/BalanceSheet_v0.1.md` (수치 기준)
- 로컬 md가 최신 기획이므로, Notion을 별도로 확인할 필요 없다
```

### 5.7 project-manager (PM)

**파일**: `.claude/agents/project-manager.md`

**변경**: GDD 버전 하드코딩 제거
```
현행: - **GDD**: `Docs/Design/GDD.md` (v2.0)
변경: - **GDD**: `Docs/Design/GDD.md` (최신 버전은 파일 헤더 참조)
```

### 5.8 soulspire-dev-protocol 스킬

**파일**: `.claude/prompts/skills/soulspire-dev-protocol/SKILL.md`

**변경**: references에 문서 참조 규칙 추가 검토
```
기획 문서 참조 원칙:
- 에이전트는 `Docs/Design/` 로컬 md를 기준 문서로 사용한다
- Notion은 총괄PD의 작업 공간이며, 에이전트가 직접 참조하지 않는다
- 기획팀장(game-designer)이 Notion -> 로컬 md 동기화를 전담한다
```

---

## 6. 변경 요약 및 실행 순서

### 즉시 수정 대상 (총괄PD 승인 후)

| 순서 | 대상 | 변경 내용 | 담당 |
|------|------|----------|------|
| 1 | CLAUDE.md | 디자인 문서 관리 섹션 재작성 (4.4절) | 개발PD (총괄PD 승인 후) |
| 2 | game-designer.md | GDD 버전 표기 수정 + 동기화 책임 섹션 추가 (5.1절) | 개발PD |
| 3 | 나머지 6개 에이전트 md | 기획서 참조 지침 추가 (5.2~5.7절) | 개발PD |
| 4 | soulspire-dev-protocol | 문서 참조 규칙 추가 (5.8절) | 개발PD |
| 5 | 로컬 기획 문서 | 표준 헤더 적용 + Notion 동기화 1회 수행 (4.3절) | 기획팀장 |

### 향후 운영 (스프린트 루틴에 편입)

- **기획 변경 발생 시**: 총괄PD -> 기획팀장 통보 -> 기획팀장 로컬 md 동기화 -> 커밋
- **스프린트 시작 시**: 기획팀장이 주요 문서의 Notion/로컬 일치 여부 점검 (1회)
- **PM 체크**: PM이 스프린트 현황 취합 시, 기획 문서 동기화 상태도 함께 확인

---

## 7. 리스크 및 완화 방안

| 리스크 | 발생 확률 | 영향도 | 완화 방안 |
|--------|-----------|--------|-----------|
| 총괄PD가 Notion 수정 후 기획팀장에게 통보 누락 | 중 | 높음 | 스프린트 시작 시 기획팀장의 전수 점검으로 보완 |
| 기획팀장 동기화 실수 (부분 반영, 누락) | 낮 | 중 | 문서 헤더의 버전/동기화 일시 필드로 교차 검증 |
| Notion MCP 도구 장애로 동기화 불가 | 낮 | 낮 | 수동 복사/붙여넣기로 대체 가능 (로컬 작업에는 영향 없음) |
| 에이전트가 구버전 문서로 작업 | 낮 | 중 | 기획 변경 커밋 메시지에 영향 받는 에이전트 명시 |

---

## 부록: 방안별 요약 비교표

| 평가 기준 | A안 (Notion Only) | B안 (로컬 Only) | C안 (하이브리드-주기) | D안 (하이브리드-시점) |
|-----------|-------------------|-----------------|----------------------|----------------------|
| 문서 일관성 | 최상 | 중 | 상 | **최상** |
| 에이전트 속도 | 최하 | **최상** | 상 | **최상** |
| 구현 복잡도 | 중 | **최하** | 중 | **하** |
| 장애 내성 | 최하 | **최상** | 상 | **최상** |
| 운영 부담 | 없음 | 높음 (수동 동기화) | 중 | **낮음** |
| git 추적성 | 없음 | **최상** | 상 | **최상** |
| **종합 추천** | 비추천 | 차선 | 가능 | **추천** |
