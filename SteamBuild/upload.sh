#!/bin/bash
# ============================================
#  Steam 빌드 업로드 스크립트 (macOS/Linux)
#  사용 전 아래 값들을 수정하세요.
# ============================================

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

# SteamCMD 경로
STEAMCMD_PATH="$HOME/steamworks_sdk/tools/ContentBuilder/builder_linux/steamcmd.sh"

# Steam 개발자 계정
STEAM_USERNAME="your_steam_username"

echo "============================================"
echo " Steam 빌드 업로드 시작"
echo "============================================"
echo ""

# VDF 파일 확인
if [ ! -f "$SCRIPT_DIR/scripts/app_build.vdf" ]; then
    echo "[오류] app_build.vdf 파일을 찾을 수 없습니다."
    exit 1
fi

# content 폴더 확인
if [ ! -f "$SCRIPT_DIR/content/MyGame.exe" ]; then
    echo "[오류] content 폴더에 빌드 파일이 없습니다."
    echo "Unity에서 먼저 빌드하세요: Tools > Build Windows"
    exit 1
fi

echo "SteamCMD 경로: $STEAMCMD_PATH"
echo "계정: $STEAM_USERNAME"
echo ""

"$STEAMCMD_PATH" +login "$STEAM_USERNAME" +run_app_build "$SCRIPT_DIR/scripts/app_build.vdf" +quit

if [ $? -eq 0 ]; then
    echo ""
    echo "============================================"
    echo " 업로드 성공!"
    echo " Steamworks 파트너 사이트에서 빌드를 확인하세요."
    echo " https://partner.steamgames.com/"
    echo "============================================"
else
    echo ""
    echo "============================================"
    echo " 업로드 실패. 위 오류 메시지를 확인하세요."
    echo "============================================"
fi
