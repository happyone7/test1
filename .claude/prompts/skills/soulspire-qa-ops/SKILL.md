---
name: soulspire-qa-ops
description: |
  에디터 플레이모드 QA, 스크린샷 시각 검증, 콘솔 에러 확인, dev→sprint 머지 게이트.
  트리거: "QA 해줘", "검증해줘", "머지 검증", "통합 QA"
  제외: 코드 수정, UI 구현, 빌드, 에셋 제작
---

# Soulspire QA 운영

## 목적
에디터 플레이모드에서 게임 동작을 검증하고, 통과한 작업만 sprint 브랜치에 머지한다.

## QA 유형별 절차

### 단위 QA (dev/* 브랜치 검증)

각 팀장의 작업 완료 후 해당 dev/* 브랜치에서 수행.

```
1. 해당 dev/* 브랜치 체크아웃
2. refresh_unity — 리컴파일
3. read_console — 컴파일 에러 확인
4. manage_editor(action="enter_play_mode") — 플레이 시작
5. references/checklist.md 기반 항목 확인
6. manage_editor(action="take_screenshot") — 증거 스크린샷
7. manage_editor(action="exit_play_mode") — 플레이 종료
8. read_console — 런타임 에러/워닝 확인
```

검증 통과 시:
```bash
git checkout sprint{N}
git merge --no-ff dev/{팀장} -m "merge: {팀장}팀 작업 머지 (QA 통과)"
```

검증 실패 시 (재검증 루프, 최대 2회):
```
1. 실패 항목 + 구체적 증거(콘솔 에러, 스크린샷) 포함하여 해당 팀장에게 수정 요청
2. 팀장 수정 완료 후 → 해당 항목만 재검증 (전체 재실행 아님)
3. 2회 재검증 후에도 실패 → 개발PD에게 에스컬레이션 (블로커 보고)
```

### 통합 QA (빌드 직전)

모든 dev/* 브랜치가 sprint에 머지된 후 수행.

```
1. sprint 브랜치에서 전체 플로우 테스트
2. references/checklist.md 전체 항목 확인
3. 스크린샷 촬영 (Assets/Screenshots/)
4. 전체 통과 → 결과 보고 → 총괄PD 승인 → 빌더에게 빌드 명령
5. 실패 항목 존재 시:
   a. 해당 dev/* 머지 원인 파악
   b. 담당 팀장에게 수정 요청 (dev/* 브랜치에서 수정 후 재머지)
   c. 수정 후 실패 항목만 재검증
   d. 2회 재검증 실패 → 개발PD에게 에스컬레이션
```

## 머지 권한
- **sprint 브랜치 머지는 QA팀장만 가능** (다른 팀장 직접 머지 금지)
- DevPD_Guidelines.md 5절 참조

## QA 결과 연동 (Multi-MCP)

### Notion 업무카드 업데이트
QA 완료 후 해당 업무 카드의 상태를 갱신한다.
- DB ID: `58c89f190c684412969f7c41341489d1`
- `QA 상태` → "통과" 또는 "실패"
- `QA 결과` → 구체적 결과 기술 (통과 항목, 실패 항목, 스크린샷 경로)
- `관련 커밋` → 검증 대상 커밋 해시

### Discord 알림
통합 QA 완료 시 Discord Webhook으로 결과 전송:
- Webhook: QA 완료 알림 채널
- 내용: 스프린트명, 통과/실패 수, 블로커 유무, 빌드 가능 여부

## 주의 사항
- QA팀장은 코드 수정 금지 — 버그 발견 시 해당 팀장에게 수정 요청
- 커밋 시 `--author="QAEngineer <qa-engineer@soulspire.dev>"` 사용
- 스크린샷은 `Assets/Screenshots/` 에 저장
