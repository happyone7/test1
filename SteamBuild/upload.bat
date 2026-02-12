@echo off
REM ============================================
REM  Steam 빌드 업로드 스크립트 (Windows)
REM  사용 전 아래 값들을 수정하세요.
REM ============================================

REM SteamCMD 경로 (Steamworks SDK 내부 또는 별도 설치)
SET STEAMCMD_PATH=C:\steamworks_sdk\tools\ContentBuilder\builder\steamcmd.exe

REM Steam 개발자 계정 정보
SET STEAM_USERNAME=shatterbone7

echo ============================================
echo  Steam 빌드 업로드 시작
echo ============================================
echo.

REM VDF 파일 경로 확인
if not exist "%~dp0scripts\app_build.vdf" (
    echo [오류] app_build.vdf 파일을 찾을 수 없습니다.
    pause
    exit /b 1
)

REM content 폴더 확인
if not exist "%~dp0content\MyGame.exe" (
    echo [오류] content 폴더에 빌드 파일이 없습니다.
    echo Unity에서 먼저 빌드하세요: Tools ^> Build Windows
    pause
    exit /b 1
)

echo SteamCMD 경로: %STEAMCMD_PATH%
echo 계정: %STEAM_USERNAME%
echo.

%STEAMCMD_PATH% +login %STEAM_USERNAME% +run_app_build "%~dp0scripts\app_build.vdf" +quit

echo.
if %ERRORLEVEL% EQU 0 (
    echo ============================================
    echo  업로드 성공!
    echo  Steamworks 파트너 사이트에서 빌드를 확인하세요.
    echo  https://partner.steamgames.com/
    echo ============================================
) else (
    echo ============================================
    echo  업로드 실패. 위 오류 메시지를 확인하세요.
    echo ============================================
)

pause
