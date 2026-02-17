# Telegram 송수신 환경 구축 가이드 (wt-dev-build)

## 1) 사전 준비
1. Telegram 앱에서 @BotFather 생성
2. `/newbot`으로 봇 생성 후 `BOT_TOKEN` 발급
3. 봇을 수신하려는 채팅(개인/그룹)에 초대
4. 아래 방식으로 chat_id 획득
   - `receive` 스크립트 실행 후 메시지 보내기
   - 또는 웹에서 `getUpdates` 확인

## 2) 파일 구성
- `Tools/telegram-bridge/send-telegram.ps1` : 메시지 송신
- `Tools/telegram-bridge/receive-telegram.ps1` : 메시지 수신(폴링)
- `Tools/telegram-bridge/.env.telegram.example` : 환경변수 템플릿

## 3) 설정
```
copy Tools\telegram-bridge\.env.telegram.example Tools\telegram-bridge\.env.telegram
```
- `TELEGRAM_BOT_TOKEN` 채우기
- `TELEGRAM_CHAT_ID` 채우기

PowerShell 세션에서 임시 사용하려면:
```powershell
$env:TELEGRAM_BOT_TOKEN = '발급된_토큰'
$env:TELEGRAM_CHAT_ID = '채팅ID'
```

## 4) 송신 테스트
```powershell
./Tools/telegram-bridge/send-telegram.ps1 -Text "텔레그램 환경 구축 테스트"
```

## 5) 수신 테스트
```powershell
# 5개 메시지 받으면 자동 종료
./Tools/telegram-bridge/receive-telegram.ps1 -Count 5 -Timeout 20
```

## 6) 자동화 연동 예시
- Sprint/QA 리포트 완료 시 send-telegram 스크립트로 알림:
```powershell
./Tools/telegram-bridge/send-telegram.ps1 -Text "Sprint5 baseline 저장 완료"
```

## 7) 보안 주의
- `.env.telegram` 파일은 민감 정보이므로 git 커밋하지 마세요
- 최소 권한(`SendMessage`, `GetUpdates`)만 사용하는 봇 토큰으로 사용
## 5) 비교 분석 완료 자동 알림(텔레그램)

Sprint6 비교 분석이 완성되면 다음으로 알림을 전송한다:

```powershell
./Tools/telegram-bridge/notify-comparison-report.ps1 -ReportPath Docs/Sprint6_Comparison_Report.md
```

리포트가 길 경우 3900자 기준 자동으로 잘라서 전송한다.
