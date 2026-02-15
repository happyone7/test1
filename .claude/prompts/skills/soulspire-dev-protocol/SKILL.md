---
name: soulspire-dev-protocol
description: |
  Soulspire 프로젝트 개발 규칙: Git 협업, 프리팹/씬 관리, 폴더 구조.
  모든 에이전트가 코드/에셋 작업 전 반드시 참조.
  트리거: 커밋, 프리팹, 씬 수정, 폴더 구조, 파일 소유권
  제외: 게임 디자인, 밸런싱, 사운드 제작
---

# Soulspire 개발 프로토콜

코드/에셋 작업 시작 전 아래 references/를 읽고 규칙을 따른다.

## 참조 파일

1. **references/git-rules.md** — Git 커밋 컨벤션, 팀장별 author, 파일 소유권, 충돌 방지
2. **references/prefab-protocol.md** — 프리팹 vs 씬 구분, 명명 규칙, 폴더 구조, 씬 편집 절차

## 핵심 3줄 요약

1. 작업 완료 즉시 커밋. 팀장별 `--author` 필수 사용.
2. 씬 파일 직접 수정 최소화. 프리팹 우선. 한 시점에 한 사람만 씬 수정.
3. 파일 소유권 확인 후 작업. 남의 담당 폴더 함부로 건드리지 않기.
