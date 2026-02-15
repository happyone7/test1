# Steam 배포 설정

## 앱 정보
- App ID: 4426340
- Depot ID: 4426341
- Steam 계정: shatterbone7
- Partner ID: 377547
- Launch Option: MyGame.exe (Windows, Launch Default)

## 경로
- SteamCMD: `/mnt/c/steamworks_sdk/tools/ContentBuilder/builder/steamcmd.exe`
- VDF 스크립트: `/mnt/c/UnityProjects/test1/SteamBuild/scripts/`
- 빌드 출력: `/mnt/c/UnityProjects/test1/SteamBuild/content/`

## VDF 파일별 용도

### app_build_dev.vdf — 개발 빌드
- 용도: 개발팀 내부 확인
- 업로드 후 `dev_test` 브랜치에 자동 라이브
- Steamworks에 `dev_test` 브랜치 사전 생성 필요

### app_build_qa.vdf — QA 빌드
- 용도: QA 검증용
- 업로드 후 `live_test` 브랜치에 자동 라이브
- Steamworks에 `live_test` 브랜치 사전 생성 필요

### app_build.vdf — 출시 빌드
- 용도: 일반 사용자에게 배포
- 업로드만 됨, default 브랜치 라이브는 수동 설정 필요

## Publisher Web API
- Key: `44FB17E1D144F7EDC90C4993A3DE89D2`
- 권한: Everyone 그룹, General
- SetAppBuildLive 엔드포인트:
  ```
  POST https://partner.steam-api.com/ISteamApps/SetAppBuildLive/v2/
  파라미터: key, appid, buildid, betakey=default
  ```

## Steam 설치 경로
- `C:\Program Files (x86)\Steam\steamapps\common\Nodebreaker TD`
