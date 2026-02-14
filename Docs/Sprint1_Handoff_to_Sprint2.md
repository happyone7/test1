# Sprint 1 → Sprint 2 핸드오프 문서

**작성일**: 2026-02-15
**대상**: 개발PD 01, 02, 03 (모든 개발PD 필독)

---

## 1. Sprint 1 최종 상태

### 완료된 것
- 코어 전투 루프 (타워 배치 → 웨이브 → 공격 → 런 종료)
- 영구 성장 시스템 (스킬 3종, 허브 스킬트리)
- 허브 ↔ 게임 씬 전환
- 타이틀/허브/인게임 UI 3화면 구성
- 다크 판타지 아트 디렉션 확정 + 스타일 락 (타워/몬스터/타일)
- ComfyUI AI 에셋 파이프라인 구축
- 배경 이미지 + 로고 적용 (타이틀, 허브)
- 사운드 디렉션 + 임시 오디오 리소스 연동
- Steam 빌드/배포 파이프라인

### 미완료 / Sprint 2로 이월
- 사운드 이벤트 트리거 QA + 볼륨 밸런스
- P0 AI 최종 리소스 1차 교체
- 밸런싱 수치 (BalanceSheet v0.1)

---

## 2. 총괄PD 피드백 v0.2 핵심 (2026-02-15)

> 원본: `Docs/Design/Sprint1_Feedback_v0.2.md`

### UI/UX
- **스킬트리 UI 대폭 개선 필요** → Sprint 2 시작 전 상세 기획서 준비
- **3화면 UI 목업 PPT 제작** (타이틀/허브/인게임에 UI 리소스 적용한 모습)

### 아트
- Sprint 2에 **기본 아트 전체 적용** 필요 (현재 플레이스홀더 상태)

### 밸런싱
- 기획팀장이 **직접 SO에 수치 입력**하여 기획 의도 반영 빌드

### 프로세스 (심각 이슈)
1. **Git 사용법 일원화 필요** → 스킬 문서 작성 완료 (아래 참조)
2. **커밋 안 해서 작업 섞이는 문제** → 작업 완료 즉시 커밋 규칙
3. **씬/프리팹 동시 수정 충돌** → 파일 소유권 매트릭스 도입
4. **프리팹화 안 된 씬 오브젝트** → 프리팹 관리 프로토콜 도입

### PM 역할 강화
- 병목현상 관리, 업무 재분배, 스프린트 일정 관리

---

## 3. 신규 스킬 문서 (전 팀원 필독)

| 스킬 문서 | 경로 | 내용 |
|-----------|------|------|
| Git 워크플로우 | `.claude/prompts/skills/skill-unity-git-workflow.md` | 커밋 규칙, 파일 소유권, 충돌 방지 |
| 씬/프리팹 프로토콜 | `.claude/prompts/skills/skill-unity-scene-prefab-protocol.md` | 프리팹 vs 씬 구분, 편집 규칙 |
| 폴더/프리팹 관리 | `.claude/prompts/skills/skill-unity-folder-prefab-management.md` | 폴더 구조, 네이밍 (기존) |

**모든 에이전트는 작업 전 위 스킬 문서를 참조해야 합니다.**

---

## 4. Sprint 2 액션 아이템

### P0 (Sprint 2 시작 전 준비)
| 항목 | 담당 | 비고 |
|------|------|------|
| 스킬트리 UI 상세 기획서 | 기획팀장 | Sprint 2 시작 조건 |
| 3화면 UI 목업 PPT | UI팀장 | UI 리소스 적용 버전 |
| .gitattributes 파일 생성 | 개발PD | 스킬 문서 참조 |

### P1 (Sprint 2 초반)
| 항목 | 담당 | 비고 |
|------|------|------|
| 기본 아트 전체 적용 (타워/몬스터/타일 스프라이트) | TA팀장 | 스타일 락 완료된 에셋 적용 |
| 밸런싱 수치 SO 입력 (기획 의도 반영) | 기획팀장 | SO 직접 수정 |
| 스킬트리 UI 구현 | UI팀장 | 기획서 기반 |
| 씬 오브젝트 프리팹화 정리 | 프로그래밍팀장 + UI팀장 | 프로토콜 적용 |
| Material 파일 위치 정리 | TA팀장 | Prefabs → Art/Materials |

### P2 (Sprint 2 중반)
| 항목 | 담당 | 비고 |
|------|------|------|
| 추가 타워 종류 구현 | 프로그래밍팀장 | Prefab Variant 활용 |
| 추가 몬스터 종류 구현 | 프로그래밍팀장 | 3종 (Soul/Charger/Brute) |
| 사운드 최종 폴리싱/풀셋 교체 | 사운드 디렉터 | Sprint 1에서 이월 |
| 난이도 곡선 설계 | 기획팀장 | |

---

## 5. 개발PD 간 협업 주의사항

### 작업 전 필수 확인
1. `Docs/Sprint1_Progress.md` (또는 Sprint2_Progress.md) 읽기
2. `git log --oneline -20` 으로 최근 커밋 확인
3. `git status` 로 미커밋 변경사항 확인
4. **다른 PD가 작업 중인 내용과 겹치지 않는지 확인**

### 작업 후 필수 수행
1. 해당 팀장 author로 커밋
2. PM 에이전트 호출하여 Progress 문서 갱신
3. 공유 문서에 영향 주는 변경이면 메모 남기기

### 참조 문서
- 개발PD 지침서: `Docs/DevPD_Guidelines.md`
- 스프린트 진행 현황: `Docs/Sprint1_Progress.md`
- GDD: `Docs/Design/GDD.md`
- 아트 디렉션: `Docs/Design/ArtDirection_v0.1.md`

---

## 6. 즉시 적용 필요 인프라 변경

### .gitattributes 생성
프로젝트 루트에 `.gitattributes` 파일이 없어서 씬/프리팹 Smart Merge가 불가능합니다.
`skill-unity-git-workflow.md`의 7번 섹션 참조하여 즉시 생성 필요.

### UnityYAMLMerge 설정
Git config에 Unity Smart Merge 등록:
```bash
git config merge.unityyamlmerge.name "Unity YAML Merge"
git config merge.unityyamlmerge.driver '"C:/Program Files/Unity/Hub/Editor/2022.3.*/Editor/Data/Tools/UnityYAMLMerge.exe" merge -p %O %B %A %P'
```
