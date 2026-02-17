#!/usr/bin/env python3
import math
import random
import wave
from pathlib import Path

SR = 44100
MAX_AMP = 32767

TARGET_ROOTS = [
    Path("Assets/Audio"),
    Path("Assets/Resources/Audio"),
]


def envelope(i, n, attack=0.01, release=0.12):
    if n <= 1:
        return 1.0
    t = i / (n - 1)
    a = min(1.0, t / max(attack, 1e-6))
    r = min(1.0, (1.0 - t) / max(release, 1e-6))
    return max(0.0, min(a, r))


def write_wav(path: Path, samples):
    path.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(path), "wb") as wf:
        wf.setnchannels(1)
        wf.setsampwidth(2)
        wf.setframerate(SR)
        frames = bytearray()
        for s in samples:
            v = max(-1.0, min(1.0, s))
            iv = int(v * MAX_AMP)
            frames += iv.to_bytes(2, "little", signed=True)
        wf.writeframes(frames)


def tone(duration, freqs, amp=0.5, pulse_hz=0.0, noise=0.0, attack=0.01, release=0.12):
    n = int(SR * duration)
    out = []
    for i in range(n):
        t = i / SR
        sig = 0.0
        for f in freqs:
            sig += math.sin(2.0 * math.pi * f * t)
        sig /= max(1, len(freqs))
        if pulse_hz > 0.0:
            pulse = 0.5 + 0.5 * math.sin(2.0 * math.pi * pulse_hz * t)
            sig *= 0.35 + 0.65 * pulse
        if noise > 0.0:
            sig += (random.random() * 2.0 - 1.0) * noise
        sig *= amp * envelope(i, n, attack=attack, release=release)
        out.append(sig)
    return out


def layered_bgm(duration, base_freqs, movement=0.05, pulse_hz=0.0):
    n = int(SR * duration)
    out = []
    for i in range(n):
        t = i / SR
        drift = 1.0 + movement * math.sin(2.0 * math.pi * 0.07 * t)
        sig = 0.0
        for idx, f in enumerate(base_freqs):
            sig += math.sin(2.0 * math.pi * (f * drift) * t + idx * 0.4)
        sig /= max(1, len(base_freqs))
        if pulse_hz > 0.0:
            pulse = 0.55 + 0.45 * math.sin(2.0 * math.pi * pulse_hz * t)
            sig *= pulse
        sig *= 0.28
        out.append(sig)
    # soft fade to reduce loop clicks in placeholder assets
    fade = int(SR * 0.3)
    for i in range(min(fade, n)):
        out[i] *= i / max(1, fade)
        out[n - 1 - i] *= i / max(1, fade)
    return out


def render_all(root: Path):
    files = {
        "BGM/BGM_Hub_MainTheme_01.wav": layered_bgm(20.0, [110.0, 165.0, 220.0], movement=0.03, pulse_hz=0.0),
        "BGM/BGM_Combat_Wave_01.wav": layered_bgm(20.0, [98.0, 147.0, 196.0, 294.0], movement=0.06, pulse_hz=1.9),
        "BGM/BGM_Boss_01.wav": layered_bgm(20.0, [82.0, 123.0, 164.0, 246.0], movement=0.08, pulse_hz=2.4),
        "SFX/Combat/SFX_Tower_Attack_01.wav": tone(0.18, [780.0, 1040.0], amp=0.7, noise=0.03, release=0.18),
        "SFX/Combat/SFX_Projectile_Hit_01.wav": tone(0.12, [260.0, 520.0], amp=0.8, noise=0.14, release=0.2),
        "SFX/Combat/SFX_Node_Die_01.wav": tone(0.35, [310.0, 210.0, 130.0], amp=0.55, noise=0.08, release=0.35),
        "SFX/Combat/SFX_Tower_Place_01.wav": tone(0.30, [180.0, 360.0], amp=0.6, noise=0.05, release=0.22),
        "SFX/Combat/SFX_Tower_Merge_01.wav": tone(0.55, [420.0, 560.0, 740.0], amp=0.55, pulse_hz=8.0, noise=0.03, release=0.15),
        "SFX/Combat/SFX_Crit_Hit_01.wav": tone(0.16, [1200.0, 1800.0], amp=0.65, noise=0.02, release=0.2),
        "SFX/UI/UI_Button_Click_01.wav": tone(0.08, [900.0], amp=0.5, noise=0.01, release=0.25),
        "SFX/Alert/ALT_Wave_Start_01.wav": tone(1.0, [140.0, 210.0], amp=0.48, pulse_hz=3.5, noise=0.04, attack=0.02, release=0.22),
        "SFX/Alert/ALT_Wave_Clear_01.wav": tone(1.0, [392.0, 523.0, 659.0], amp=0.46, pulse_hz=5.0, noise=0.02, attack=0.01, release=0.2),
        "SFX/Alert/ALT_Boss_Appear_01.wav": tone(1.7, [70.0, 105.0], amp=0.65, pulse_hz=1.5, noise=0.08, attack=0.03, release=0.15),
        "SFX/Alert/ALT_Boss_Defeat_01.wav": tone(2.0, [196.0, 262.0, 330.0], amp=0.52, pulse_hz=4.0, noise=0.04, attack=0.01, release=0.18),
        "SFX/Alert/ALT_Stage_Clear_01.wav": tone(2.2, [294.0, 392.0, 523.0], amp=0.5, pulse_hz=4.8, noise=0.03, attack=0.01, release=0.2),
        "SFX/Alert/ALT_Base_Hit_01.wav": tone(0.33, [120.0, 180.0], amp=0.72, noise=0.16, attack=0.005, release=0.2),
        "SFX/Alert/ALT_Hp30_Warning_01.wav": tone(0.7, [90.0], amp=0.52, pulse_hz=2.0, noise=0.03, release=0.18),
        "SFX/Alert/ALT_Hp10_Warning_01.wav": tone(0.45, [110.0, 220.0], amp=0.55, pulse_hz=5.5, noise=0.05, release=0.2),
    }

    for rel, samples in files.items():
        write_wav(root / rel, samples)


if __name__ == "__main__":
    random.seed(20260215)
    for root in TARGET_ROOTS:
        render_all(root)
    print("Generated Sprint 1 placeholder audio in:")
    for root in TARGET_ROOTS:
        print(f"- {root}")
