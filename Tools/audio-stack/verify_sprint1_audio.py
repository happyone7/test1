#!/usr/bin/env python3
import csv
import wave
from pathlib import Path

manifest = Path("Assets/Audio/Sprint1_SoundManifest.csv")
if not manifest.exists():
    raise SystemExit("manifest missing: Assets/Audio/Sprint1_SoundManifest.csv")

ok = 0
missing = 0
print("key,duration_sec,status")
with manifest.open("r", encoding="utf-8") as f:
    reader = csv.DictReader(f)
    for row in reader:
        path = Path(row["file"])
        if not path.exists():
            missing += 1
            print(f"{row['key']},0,missing")
            continue
        try:
            with wave.open(str(path), "rb") as wf:
                duration = wf.getnframes() / float(wf.getframerate())
        except wave.Error:
            duration = 0.0
        ok += 1
        print(f"{row['key']},{duration:.3f},ok")

print(f"\nsummary: ok={ok} missing={missing}")
