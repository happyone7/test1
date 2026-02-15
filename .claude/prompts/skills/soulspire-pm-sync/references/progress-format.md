# Sprint Progress 문서 포맷

## 문서 경로
- `Docs/Sprint1_Progress.md`
- `Docs/Sprint2_Progress.md`
- `Docs/Sprint3_Progress.md`
- `Docs/Sprint4_Progress.md`
- (스프린트 추가 시 번호 증가)

## 문서 헤더 (필수)

```markdown
# Sprint N 진행 현황

**프로젝트**: Soulspire
**브랜치**: <현재 작업 브랜치>
**기준 문서**: <참조 기획문서>
**최종 업데이트**: YYYY-MM-DD
**상태**: <전체 상태 한줄>
```

## 목표 섹션

```markdown
## 스프린트 목표
"<한줄 목표>"
- 구체적 달성 항목 나열
```

## 업무 카드 현황 (Notion 연동)

```markdown
## Notion 업무 카드 현황

| 카드 | 담당팀 | 상태 |
|------|--------|------|
| <업무명> | <팀> | 완료/진행중/대기/블로커 |
```

## 팀장별 작업 기록

```markdown
### <팀장명> (<에이전트명>) -- 커밋 <해시>
- [x] 완료 항목 -- 설명
- [ ] 미완료 항목 -- 설명
```

## 공수 산정 기준
- 에이전트 병렬 실행: 1시간 = 단일 에이전트 6~8시간 분량
- 모든 팀장에게 업무 배분 (유휴 팀 최소화)
- 팀당 P0 + P1 업무 2~3개씩 배정
- DevPD_Guidelines.md 8.1절 참조

## 관련 문서
- 개발PD 지침서: `Docs/DevPD_Guidelines.md`
- GDD: `Docs/Design/GDD.md`
- 아트 디렉션: `Docs/Design/ArtDirection_v0.1.md`
