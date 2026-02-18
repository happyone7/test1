#!/usr/bin/env python3
"""Soulspire Telegram Notification Script

Usage:
    python Tools/notify.py <type> <message>

Types:
    sprint_start   - 스프린트 시작
    sprint_end     - 스프린트 완료
    phase_done     - 페이즈 완료
    approval       - 총괄PD 승인 요청
    build          - 빌드 완료/실패
    qa             - QA 결과
    info           - 일반 알림
    error          - 에러/장애

Examples:
    python Tools/notify.py sprint_start "Sprint 5 시작 — 6개 팀 병렬 작업 개시"
    python Tools/notify.py approval "Sprint 4 빌드 Steam 업로드 승인 필요"
    python Tools/notify.py phase_done "Phase 2 완료 — QA 6/6 통과"
"""

import sys
import json
import urllib.request
from datetime import datetime

BOT_TOKEN = "7702502230:AAHI0eX_46BR1VdsDRNWGLw_5ASXJ--F5LM"
CHAT_ID = 8595187261

ICONS = {
    "sprint_start": "\U0001F3C1",   # 🏁
    "sprint_end":   "\U0001F3C6",   # 🏆
    "phase_done":   "\u2705",       # ✅
    "approval":     "\U0001F514",   # 🔔
    "build":        "\U0001F4E6",   # 📦
    "qa":           "\U0001F50D",   # 🔍
    "info":         "\U0001F4AC",   # 💬
    "error":        "\U0001F6A8",   # 🚨
}

LABELS = {
    "sprint_start": "스프린트 시작",
    "sprint_end":   "스프린트 완료",
    "phase_done":   "페이즈 완료",
    "approval":     "승인 요청",
    "build":        "빌드",
    "qa":           "QA 결과",
    "info":         "알림",
    "error":        "에러",
}


def send(msg_type: str, message: str) -> bool:
    icon = ICONS.get(msg_type, "\U0001F4AC")
    label = LABELS.get(msg_type, msg_type)
    now = datetime.now().strftime("%m/%d %H:%M")

    text = f"{icon} <b>[{label}]</b> — {now}\n\n{message}"

    data = json.dumps({
        "chat_id": CHAT_ID,
        "text": text,
        "parse_mode": "HTML",
    }).encode("utf-8")

    req = urllib.request.Request(
        f"https://api.telegram.org/bot{BOT_TOKEN}/sendMessage",
        data=data,
        headers={"Content-Type": "application/json"},
    )

    try:
        resp = urllib.request.urlopen(req, timeout=10)
        result = json.loads(resp.read())
        return result.get("ok", False)
    except Exception as e:
        print(f"Telegram send failed: {e}", file=sys.stderr)
        return False


if __name__ == "__main__":
    if len(sys.argv) < 3:
        print(__doc__)
        sys.exit(1)

    msg_type = sys.argv[1]
    message = " ".join(sys.argv[2:])
    message = message.replace("\\n", "\n")

    if send(msg_type, message):
        print(f"Sent [{msg_type}]: {message}")
    else:
        print("Failed to send", file=sys.stderr)
        sys.exit(1)
