---
name: soulspire-agent-skill-standards
description: |
  에이전트/스킬 생성·수정 시 품질 기준. 일반 지식 금지, 프로젝트 특화만 허용.
  트리거: 에이전트 생성, 스킬 생성, 에이전트 수정, 스킬 수정
  제외: 게임플레이 코드, UI, 빌드
---

# 에이전트/스킬 품질 기준

에이전트(.claude/agents/)나 스킬(.claude/prompts/skills/)을 생성·수정할 때 이 기준을 따른다.

## 참조 파일

- [에이전트 작성 기준](references/agent-standards.md) — body 구조, 금지 사항, 필수 섹션
- [스킬 작성 기준](references/skill-standards.md) — frontmatter, body, references 규칙
- [검증 체크리스트](references/validation-checklist.md) — 생성/수정 후 자동 수행 6항목

## 핵심 원칙 (요약)

1. **일반 지식 금지**: Claude가 이미 아는 내용(범용 C# 패턴, NUnit, UI Toolkit 등)을 body에 넣지 않는다
2. **프로젝트 특화만**: 파일 경로, SO 구조, 팀 협업 규칙, MCP 도구 활용법 등 프로젝트 고유 정보만
3. **스킬에 절차, 에이전트에 역할**: 스킬 = "어떻게 하는가"(절차/규칙), 에이전트 = "누가 무엇을"(역할/맥락)
4. **변경 후 반드시 검증**: references/validation-checklist.md 6항목 수행
5. **노션 동기화**: 에이전트/스킬 변경 시 노션 "에이전트 - 스킬 매핑" 페이지도 갱신
