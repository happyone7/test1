# Unity Steam Game: 빌드부터 스팀 업로드까지

## 전체 흐름

```
[1. Unity 프로젝트 열기]
        ↓
[2. 씬 자동 생성 (Tools > Setup Game Scene)]
        ↓
[3. 플레이 테스트 (에디터에서)]
        ↓
[4. Windows 빌드 (Tools > Build Windows)]
        ↓
[5. Steamworks 설정 (파트너 사이트)]
        ↓
[6. SteamCMD로 업로드]
        ↓
[7. Steam 앱에서 실행]
```

---

## STEP 1: Unity에서 프로젝트 열기

1. Unity Hub를 열고 **"Open"** 클릭
2. 이 프로젝트 폴더(`test1`)를 선택
3. Unity 버전: **2022.3 LTS 이상** 권장 (6000.x도 가능)
4. 프로젝트가 열리면 자동으로 스크립트가 컴파일됩니다

## STEP 2: 씬 자동 생성

1. Unity 에디터 상단 메뉴: **Tools > Setup Game Scene**
2. 자동으로 생성되는 것들:
   - 회색 바닥 (30x30 크기)
   - **파란색 큐브** (플레이어) - WASD/방향키로 이동, Space로 점프
   - 빨강/초록/노란 큐브 장애물
   - 주황색 구체 장애물
   - 카메라 (플레이어 따라다님)
   - SteamManager 오브젝트
3. 씬이 `Assets/Scenes/MainScene.unity`에 자동 저장됩니다

## STEP 3: 플레이 테스트

1. **Play 버튼** (▶) 클릭
2. 조작법:
   - `WASD` 또는 `방향키`: 이동
   - `Space`: 점프
   - `ESC`: 종료
3. 파란 큐브가 바닥 위에서 잘 움직이면 성공

## STEP 4: Windows 빌드

### 방법 A: 에디터 메뉴
1. **Tools > Build Windows** 클릭
2. `SteamBuild/content/Soulspire.exe` 에 빌드 결과물이 생성됩니다

### 방법 B: 커맨드라인 (CI용)
```bash
"C:\Program Files\Unity\Hub\Editor\2022.3.xxx\Editor\Unity.exe" \
  -batchmode \
  -projectPath "이_프로젝트_경로" \
  -executeMethod BuildScript.BuildWindows \
  -quit
```

## STEP 5: Steamworks 파트너 사이트 설정

### 5-1. 개발자 등록
1. https://partner.steamgames.com/ 접속
2. Steam 개발자 등록 (등록비 $100)
3. 은행 및 세금 정보 입력

### 5-2. 앱 생성
1. 파트너 사이트 > **"앱 만들기"**
2. 앱 이름 입력, 타입: **게임** 선택
3. 생성 후 **App ID** 확인 (예: 1234560)
4. **Depot ID**도 확인 (보통 App ID + 1 = 1234561)

### 5-3. 프로젝트 파일에 ID 반영

다음 파일들의 placeholder를 실제 ID로 교체:

**`steam_appid.txt`:**
```
1234560
```

**`SteamBuild/scripts/app_build.vdf`:**
```
"AppID" "1234560"
...
"1234561" "depot_build.vdf"
```

**`SteamBuild/scripts/depot_build.vdf`:**
```
"DepotID" "1234561"
```

**`Assets/Scripts/SteamManager.cs`:** (AppId_t 값 교체)
```csharp
if (SteamAPI.RestartAppIfNecessary(new AppId_t(1234560)))
```

### 5-4. 런치 옵션 설정
1. 파트너 사이트 > 앱 관리 > **설치 > 일반 설치**
2. Launch Options 추가:
   - 실행 파일: `Soulspire.exe`
   - OS: Windows

## STEP 6: SteamCMD로 업로드

### 6-1. Steamworks SDK 다운로드
1. https://partner.steamgames.com/ > SDK 다운로드
2. 압축 해제 후 `tools/ContentBuilder/builder/steamcmd.exe` 경로 확인

### 6-2. Steamworks.NET 설치 (선택사항)
Steam 연동 기능이 필요한 경우:
1. Unity Package Manager > "Add package from git URL"
2. `https://github.com/rlabrecque/Steamworks.NET.git?path=/com.rlabrecque.steamworks.net`
3. 또는 https://github.com/rlabrecque/Steamworks.NET/releases 에서 `.unitypackage` 다운로드

> **참고:** 기본 빌드/업로드만 할 때는 Steamworks.NET이 없어도 됩니다.
> SteamManager.cs에서 `DISABLESTEAMWORKS` 심볼이 정의되면 Steam 코드가 비활성화됩니다.

### 6-3. 업로드 실행

**`SteamBuild/upload.bat`** 파일을 수정:
```bat
SET STEAMCMD_PATH=C:\steamworks_sdk\tools\ContentBuilder\builder\steamcmd.exe
SET STEAM_USERNAME=your_steam_username
```

그 다음 `upload.bat`을 실행. Steam Guard 인증이 필요할 수 있습니다.

## STEP 7: Steam 앱에서 실행 확인

1. https://partner.steamgames.com/ > 앱 관리 > **빌드** 탭
2. 업로드된 빌드가 보이면 **"기본값으로 설정"** 클릭
3. Steam 클라이언트 재시작
4. 라이브러리에서 게임 찾아서 **"플레이"** 클릭
5. 게임이 실행되면 성공!

---

## 프로젝트 구조

```
Soulspire/
├── Assets/
│   ├── Scripts/
│   │   ├── PlayerController.cs    # WASD 이동 + 점프
│   │   ├── CameraFollow.cs        # 카메라 추적
│   │   └── SteamManager.cs        # Steam API 초기화
│   ├── Editor/
│   │   ├── SceneSetup.cs          # 씬 자동 생성 (Tools 메뉴)
│   │   └── BuildScript.cs         # 빌드 자동화 (Tools 메뉴)
│   ├── Scenes/                    # 씬 파일 (자동 생성됨)
│   └── Plugins/Steamworks.NET/    # Steamworks.NET (별도 설치)
├── ProjectSettings/
│   └── ProjectSettings.asset
├── SteamBuild/
│   ├── scripts/
│   │   ├── app_build.vdf          # Steam 앱 빌드 설정
│   │   └── depot_build.vdf        # Steam 디포 빌드 설정
│   ├── content/                   # 빌드 결과물 (gitignore)
│   ├── output/                    # SteamCMD 로그 (gitignore)
│   ├── upload.bat                 # Windows 업로드 스크립트
│   └── upload.sh                  # macOS/Linux 업로드 스크립트
├── steam_appid.txt                # Steam App ID (개발용)
├── .gitignore
└── GUIDE.md                       # 이 문서
```

## 문제 해결

| 문제 | 해결 |
|------|------|
| Steam API 초기화 실패 | Steam 클라이언트가 실행 중인지 확인, steam_appid.txt 확인 |
| 빌드 오류 | Unity 에디터에서 Console 탭의 오류 메시지 확인 |
| SteamCMD 로그인 실패 | Steam Guard 코드 입력, 2FA 확인 |
| 업로드 후 게임 안보임 | 파트너 사이트에서 빌드를 "기본값"으로 설정했는지 확인 |
| DLL not found | Steamworks.NET 패키지를 설치하거나, DISABLESTEAMWORKS 심볼 추가 |
