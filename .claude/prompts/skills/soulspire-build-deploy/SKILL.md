---
name: soulspire-build-deploy
description: |
  Unity Windows 빌드 → SteamCMD 업로드 → 브랜치 라이브 설정의 전체 파이프라인.
  트리거: "빌드해줘", "Steam 업로드", "dev_test 배포", "출시 빌드"
  제외: 게임 로직, UI, 에셋 제작
---

# Soulspire 빌드 & 배포

## 목적
Unity Windows 빌드를 생성하고 SteamCMD로 업로드하는 절차. 개발/QA/출시 3가지 경로가 있다.

## 사전 조건 (Gate 0 — 미충족 시 절차 시작 금지)
```
1. 총괄PD 빌드 승인 여부 확인 (프로토타입 기간 필수)
2. QA 통과 상태 확인 — Notion 업무카드 "QA 상태"가 "통과"인지
3. sprint 브랜치에서 작업 중인지 확인 (dev/* 브랜치에서 빌드 금지)
→ 하나라도 미충족 시 빌드 진행하지 않고 개발PD에게 보고
```

## 빌드 절차

### Step 1: Unity 준비
```
1. manage_scene(action="save") — 씬 저장
2. refresh_unity — 리컴파일
3. read_console — 컴파일 에러 없는지 확인
→ Gate 1: 컴파일 에러 0건이어야 Step 2 진행. 에러 존재 시 프로그래밍팀장에게 수정 요청.
```

### Step 2: Windows 빌드 (실패 시 최대 2회 재시도)
```
4. execute_menu_item → Tools/Build Windows
5. sleep 20 (빌드 완료 대기)
6. 빌드 결과 확인: SteamBuild/content/Soulspire.exe 존재 여부
7. 빌드 실패 시:
   a. read_console — 컴파일/빌드 에러 확인
   b. 에러 내용을 프로그래밍팀장에게 전달, 수정 요청
   c. 수정 완료 후 Step 1부터 재실행
   d. 2회 실패 → 개발PD에게 에스컬레이션
```

→ Gate 2: Soulspire.exe 존재 확인되어야 Step 2.5 진행. 미존재 시 빌드 실패 처리.

### Step 2.5: 로컬 직접 복사 테스트 (Steam CDN 전 필수)
```
1. 빌드 출력을 Steam 설치 폴더에 직접 복사:
   cp -r "SteamBuild/content/"* "C:/Program Files (x86)/Steam/steamapps/common/Soulspire/"
2. 게임 실행하여 핵심 기능 동작 확인 (UI 표시, 게임 루프 등)
3. Player.log 확인: AppData/LocalLow/SBGames/Soulspire/Player.log
→ Gate 2.5: 로컬 테스트 통과 시에만 Step 3 진행. 실패 시 빌드 금지, 수정 후 재빌드.
```

### Step 3: Steam 업로드 (VDF 자동 선택)

빌드 유형이 명시되지 않은 경우 아래 기준으로 자동 판단:
```
IF 개발PD가 "출시 빌드" 또는 "default 브랜치" 언급 → 출시 빌드
ELIF 개발PD가 "QA 빌드" 또는 "live_test" 언급 → QA 빌드
ELIF 통합 QA 통과 후 빌드 → QA 빌드
ELSE → 개발 빌드 (기본값)
```

| 빌드 유형 | VDF 파일 | 자동 라이브 브랜치 |
|----------|----------|-----------------|
| 개발 빌드 | app_build_dev.vdf | dev_test |
| QA 빌드 | app_build_qa.vdf | live_test |
| 출시 빌드 | app_build.vdf | (수동 설정 필요) |

```bash
/mnt/c/steamworks_sdk/tools/ContentBuilder/builder/steamcmd.exe \
  +login shatterbone7 \
  +run_app_build "../scripts/<VDF파일>" \
  +quit
```

### Step 4: 업로드 실패 대응 (최대 2회 재시도)
```
업로드 실패 시:
1. 에러 메시지 확인 (인증 만료, 네트워크, VDF 경로 오류 등)
2. 인증 문제 → SteamCMD 재로그인 후 재시도
3. VDF/경로 문제 → references/steam-config.md 경로 확인 후 수정
4. 2회 실패 → 개발PD에게 에스컬레이션
```

### Step 5: 출시 빌드 시 추가 (default 브랜치 라이브)
```bash
curl -X POST "https://partner.steam-api.com/ISteamApps/SetAppBuildLive/v2/" \
  -d "key=<API_KEY>&appid=4426340&buildid=<BUILDID>&betakey=default"
```
실패 시 buildid 값 재확인 (SteamCMD 출력에서 추출).

API 키와 상세 설정은 references/steam-config.md 참조.

### Step 6: 빌드/배포 결과 알림 (Multi-MCP)

#### Discord 알림
업로드 성공 시 Discord Webhook으로 결과 전송:
- 내용: 빌드 유형(개발/QA/출시), VDF, Steam 브랜치, 빌드 번호
- 실패 시에도 알림: 에러 요약 + 에스컬레이션 상태

#### Notion 업무카드 업데이트
빌드 관련 업무 카드의 상태를 갱신:
- DB ID: `58c89f190c684412969f7c41341489d1`
- `QA 상태` → "빌드 완료" 또는 "빌드 실패"
- `관련 커밋` → 빌드 대상 커밋 해시
