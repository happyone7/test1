# Unity Steam Game Project

## 프로젝트 개요
Unity로 만든 3D 게임을 Steam에 빌드/업로드하는 프로젝트.

## 기술 스택
- **엔진**: Unity 2022.3 LTS+
- **언어**: C#
- **플랫폼**: Steam (Windows 빌드)
- **Steam SDK**: Steamworks.NET (선택)

## 프로젝트 구조
- `Assets/Scripts/` - 게임 로직 (PlayerController, CameraFollow, SteamManager)
- `Assets/Editor/` - 에디터 도구 (SceneSetup, BuildScript)
- `SteamBuild/scripts/` - Steam VDF 빌드 설정
- `SteamBuild/content/` - 빌드 결과물 (gitignore)
- `ProjectSettings/` - Unity 프로젝트 설정

## MCP 서버
- **mcp-unity**: Unity Editor 씬/에셋/빌드 제어 (포트 8095)
- **playwright**: Steamworks 파트너 사이트 웹 자동화

## 주요 워크플로우
1. Unity에서 씬 구성 (Tools > Setup Game Scene)
2. 에디터에서 플레이 테스트
3. Windows 빌드 (Tools > Build Windows)
4. Steamworks 파트너 사이트에서 앱/디포 설정
5. SteamCMD로 업로드

## Steam 정보
- **게임 이름**: Nodebreaker TD
- **App ID**: 4426340
- **Depot ID**: 4426341
- **Steam 계정**: shatterbone7
- **Partner ID**: 377547
