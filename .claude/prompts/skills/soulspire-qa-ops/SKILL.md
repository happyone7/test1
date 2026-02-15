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

검증 실패 시:
- 해당 팀장에게 구체적 수정 요청 메시지 전달
- 수정 후 재검증

### 통합 QA (빌드 직전)

모든 dev/* 브랜치가 sprint에 머지된 후 수행.

```
1. sprint 브랜치에서 전체 플로우 테스트
2. references/checklist.md 전체 항목 확인
3. 스크린샷 촬영 (Assets/Screenshots/)
4. 결과 보고 → 총괄PD 승인 → 빌더에게 빌드 명령
```

## 머지 권한
- **sprint 브랜치 머지는 QA팀장만 가능** (다른 팀장 직접 머지 금지)
- DevPD_Guidelines.md 5절 참조

## 주의 사항
- QA팀장은 코드 수정 금지 — 버그 발견 시 해당 팀장에게 수정 요청
- 커밋 시 `--author="QAEngineer <qa-engineer@soulspire.dev>"` 사용
- 스크린샷은 `Assets/Screenshots/` 에 저장
