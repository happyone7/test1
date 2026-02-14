# Sprint 2 진행 현황

**프로젝트**: Soulspire
**기준 문서**: `Docs/Sprint2_Plan.md`, `Docs/DevPD_Guidelines.md`(9번 섹션)
**최종 업데이트**: 2026-02-15
**상태**: 완료

---

## 범위 확정
- 이번 Sprint 2는 **1시간 축소판**으로 운영
- 포함:
  - 기본 아트 Unity 적용
  - SO 밸런싱 수치 반영
  - 씬 오브젝트 프리팹화 정리
- 제외:
  - 스킬트리 UI 기획/구현
  - 3화면 UI 목업 PPT
  - 추가 타워/몬스터 코드
  - 사운드 최종 폴리싱
  - Steam 빌드/업로드

---

## 완료된 작업

### 기획팀장 (game-designer) -- 커밋 256b156
- [x] SO 밸런싱 수치 GDD 기준 검증 -- Sprint 1에서 이미 GDD 기준으로 조정 완료 확인, 추가 수정 불필요

### TA팀장 (unity-technical-artist) -- 커밋 d9e446f, fef4c19
- [x] Arrow Tower 프리팹 3D->2D 전환 (SpriteRenderer 적용)
- [x] Monster_Bit 프리팹 2D 전환
- [x] Projectile_Arrow 프리팹 2D 전환
- [x] 신규 몬스터 프리팹 3종 생성 (Monster_Soul, Monster_Charger, Monster_Brute)
- [x] 스프라이트 임포트 설정 일괄 수정 (PPU 1024, Bilinear, Uncompressed)
- [x] 에디터 유틸리티 2개 추가 (SpritePrefabConverter, SpriteImportFixer)

### 프로그래밍팀장 (unity-gameplay-programmer) -- 커밋 86fc75c, ab1cf03
- [x] 프리팹 폴더 정리 및 명명규칙 표준화 (Tower_XXX, Monster_XXX, Projectile_XXX)
- [x] Nodes/ -> Monsters/ 폴더 이동
- [x] 에디터 스크립트 4개 경로 참조 업데이트

### QA팀장 (unity-qa-engineer)
- [x] 통합 검증 전 항목 PASS
  - 컴파일 오류 없음
  - 에셋 참조 정상
  - 씬 구조 정상
  - 에디터 플레이 정상 동작
  - 프리팹 구조 정상

### 개발PD
- [x] .gitattributes 설정 -- Sprint 1에서 이미 완료

---

## 진행 중

없음 (전 항목 완료)

---

## 미착수 / 대기

없음 (전 항목 완료)

---

## 완료 기준 체크리스트

- [x] Arrow Tower L1~L4 스프라이트가 게임 프리팹에 적용됨
- [x] 몬스터 3종 스프라이트가 Unity에 임포트/연결됨
- [x] 타일셋 5종 스프라이트가 Unity에 임포트/연결됨
- [x] 기획 의도 밸런싱 수치가 SO에 반영됨 (기존 수치 검증 완료)
- [x] 주요 씬 오브젝트 프리팹화 완료
- [x] `.gitattributes` 설정 완료
- [x] UnityYAMLMerge 로컬 Git 설정 완료 (현재 환경)
- [x] QA 검증 통과 (에디터 플레이 정상 동작)

---

## 블로커
- 없음

---

## 스프린트 현황 요약

- 완료: 5건 / 진행중: 0건 / 대기: 0건
- 주요 진전: 전체 프리팹 2D 전환 + 폴더 정리 + QA PASS 완료
- 블로커: 없음
- 다음 예정: Sprint 3 (스킬트리 UI, 추가 타워/몬스터, 사운드 폴리싱)

---

## 운영 메모
- 개발PD는 실무(코딩/UI/아트/빌드)를 수행하지 않고 조율/추적만 수행
- 착수 지시 원문: `Docs/Sprint2_WorkOrders_2026-02-15.md`
- Sprint 2 계획서: `Docs/Sprint2_Plan.md`
