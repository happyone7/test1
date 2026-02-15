---
name: soulspire-pm-sync
description: |
  스프린트 진행 현황 취합, Sprint*_Progress.md 갱신, 팀별 상태 보고.
  트리거: "진행 현황 갱신", "스프린트 현황", "PM 보고"
  제외: 코드/UI/빌드/에셋 작업 일체
---

# Soulspire 스프린트 동기화

## 목적
스프린트 진행 현황 문서를 최신 상태로 유지하고, 팀간 상태를 취합하여 보고한다.

## 현황 갱신 절차

### Step 1: 정보 수집
```
1. git log --oneline -30 으로 최근 커밋 확인
2. 현재 Sprint Progress 문서 읽기
3. (팀 모드 시) 각 팀장에게 메시지로 현재 상태 확인
```

### Step 2: Progress 문서 갱신

references/progress-format.md 참조하여 갱신.

갱신 대상 파일: `Docs/Sprint{N}_Progress.md` (현재 활성 스프린트)

갱신 항목:
- 각 팀장별 완료/진행중/대기 상태
- 커밋 해시 연결
- 블로커 및 의존성 기록
- 최종 업데이트 시간

### Step 3: 상태 보고

보고 포맷:
```
## 스프린트 현황 요약
- 완료: N건 / 진행중: N건 / 대기: N건
- 주요 진전: [한줄 요약]
- 블로커: [있으면 기술, 없으면 "없음"]
- 다음 예정: [우선순위 높은 작업 1~2개]
```

## 주의 사항
- PM은 코드/UI/빌드/에셋 작업 절대 금지
- 커밋 시 `--author="ProjectManager <project-manager@soulspire.dev>"` 사용
- 병목 구간 식별 시 개발PD에게 보고
