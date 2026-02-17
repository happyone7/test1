# 에이전트 작성 기준

## Frontmatter (변경 시 총괄PD 승인 필수)

```yaml
---
name: "이모지 에이전트명"
description: |
  한줄 역할 설명. (~100 단어 이내)
  트리거: "키워드1", "키워드2"
  제외: 담당하지 않는 영역

  Examples:
  - <example>
    Context: 상황 설명
    user: "사용자 요청"
    assistant: "에이전트 사용 선언"
    <commentary>이유</commentary>
  </example>
---
```

- description은 100 단어 이내
- Examples 2~3개 필수 (트리거 단어가 포함된 자연스러운 대화)

## Body 구조 (필수 섹션)

```markdown
# [에이전트 한글명]

## 필수 참조 스킬 (작업 전 반드시 읽기)
- `.claude/prompts/skills/soulspire-dev-protocol/SKILL.md` — 설명
- (해당 에이전트 전용 스킬이 있으면 추가)

## 역할
한 문장으로 이 에이전트가 무엇을 하는지 명시.

## 프로젝트 구조 / 컨텍스트
- 파일 경로, SO 타입, 기존 코드 참조 등 프로젝트 고유 정보

## (에이전트별 특화 섹션)
- 빌더: VDF 선택 기준, 사전 조건
- QA: 수행 방법, 체크리스트 요약, 머지 게이트
- 기획: SO 수치 조정 방법, 성장 타임라인
- 등등

## 자체 QA
작업 후 확인 사항 (MCP 도구 활용)

## 커밋 규칙
- author 태그

## 협업
- 실제 팀 구성원만 명시 (비존재 에이전트 참조 금지)
```

## Body 금지 사항 (Pattern 5: Domain-Specific Intelligence)

| 금지 | 예시 | 이유 |
|------|------|------|
| 범용 C# 코드 | Health.cs, StateMachine, ObjectPool 등 | Claude가 이미 알고 있음 |
| 프레임워크 튜토리얼 | NUnit 사용법, UI Toolkit 설정, Shader Graph 기초 | 일반 지식 |
| 비존재 에이전트 참조 | unity-performance-optimizer, unity-analytics-engineer | 프로젝트에 없음 |
| 일반 전문 분야 목록 | "핵심 전문 분야" 불릿 나열 | 토큰 낭비 |
| CI/CD 파이프라인 | Jenkins, GitHub Actions 예시 | 프로젝트에서 사용하지 않음 |
| 모범 사례 목록 | "테스트 피라미드", "SOLID 원칙" 등 | 일반 지식 |

## Body 허용 사항

| 허용 | 예시 | 이유 |
|------|------|------|
| 프로젝트 파일 경로 | `Assets/Scripts/Core/`, `Assets/Audio/BGM/` | 프로젝트 고유 |
| SO 구조/수치 테이블 | TowerData, NodeData 필드 목록 | 프로젝트 고유 |
| MCP 도구 활용법 | `manage_editor(action="play")` | 프로젝트 도구 |
| 팀 협업 규칙 | TA↔UI 역할 분리, SO 수치는 기획팀장 | 프로젝트 규칙 |
| 품질 기준 수치 | 드로우콜 200 이하, LUFS -14~-16 | 프로젝트 목표 |
| 사전 조건/실패 대응 | 빌드 전 총괄PD 승인, 컴파일 에러 시 중단 | 프로젝트 절차 |

## 크기 가이드라인

- frontmatter description: ~100 단어
- body: **50~100줄** 권장 (100줄 초과 시 스킬 references로 분리)
- 코드 블록: 프로젝트 특화 예시만 (5줄 이내)
