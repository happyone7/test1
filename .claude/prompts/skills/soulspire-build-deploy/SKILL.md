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

## 사전 조건
- 프로토타입 기간: **빌드 전 반드시 총괄PD 승인 필요** (에디터에서 먼저 확인)
- 모든 작업 완료 + QA 통과 상태여야 함

## 빌드 절차

### Step 1: Unity 준비
```
1. manage_scene(action="save") — 씬 저장
2. refresh_unity — 리컴파일
3. read_console — 컴파일 에러 없는지 확인
```

### Step 2: Windows 빌드
```
4. execute_menu_item → Tools/Build Windows
5. sleep 20 (빌드 완료 대기)
6. 빌드 결과 확인: SteamBuild/content/MyGame.exe 존재 여부
```

### Step 3: Steam 업로드 (VDF 선택)

references/steam-config.md 참조하여 VDF 선택 후 SteamCMD 실행.

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

### Step 4: 출시 빌드 시 추가 (default 브랜치 라이브)
```bash
curl -X POST "https://partner.steam-api.com/ISteamApps/SetAppBuildLive/v2/" \
  -d "key=<API_KEY>&appid=4426340&buildid=<BUILDID>&betakey=default"
```

API 키와 상세 설정은 references/steam-config.md 참조.
