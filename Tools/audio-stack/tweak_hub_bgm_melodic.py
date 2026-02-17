#!/usr/bin/env python3
import math
import shutil
import wave
from pathlib import Path

SR = 44100
MAX_AMP = 32767
BPM = 78.0
BEAT = 60.0 / BPM
BAR = BEAT * 4.0
DURATION = 24.0

TARGETS = [
    Path("Assets/Audio/BGM/BGM_Hub_MainTheme_01.wav"),
    Path("Assets/Resources/Audio/BGM/BGM_Hub_MainTheme_01.wav"),
]


# D minor progression: Dm -> Bb -> Gm -> A7
def chord_for_bar(bar_idx: int):
    prog = [
        (50, 53, 57),   # D3 F3 A3
        (46, 50, 53),   # Bb2 D3 F3
        (43, 46, 50),   # G2 Bb2 D3
        (45, 49, 52),   # A2 C#3 E3
    ]
    return prog[bar_idx % len(prog)]


def midi_to_freq(midi: int) -> float:
    return 440.0 * (2.0 ** ((midi - 69) / 12.0))


def tri(phase: float) -> float:
    return (2.0 / math.pi) * math.asin(math.sin(phase))


def env_note(t: float, start: float, dur: float, attack: float = 0.02, release: float = 0.2) -> float:
    if t < start or t > start + dur:
        return 0.0
    x = t - start
    a = min(1.0, x / max(attack, 1e-6))
    rem = (start + dur) - t
    r = min(1.0, rem / max(release, 1e-6))
    return max(0.0, min(a, r))


def build_melody_events(num_bars: int):
    # emotional minor motif around A3~D5
    bar_patterns = [
        [62, 65, 69, 72, 69, 65, 62, 60],  # Dm
        [65, 70, 74, 70, 69, 65, 62, 65],  # Bb feel
        [67, 70, 74, 70, 67, 65, 62, 60],  # Gm feel
        [69, 73, 76, 73, 72, 69, 65, 62],  # A7 tension
    ]
    events = []
    for b in range(num_bars):
        pat = bar_patterns[b % 4]
        bar_start = b * BAR
        for i, midi in enumerate(pat):
            start = bar_start + i * (BEAT * 0.5)
            dur = BEAT * 0.42
            # slight movement left-right
            pan = -0.35 if i % 2 == 0 else 0.35
            vel = 0.30 if i not in (0, 4) else 0.36
            events.append((start, dur, midi, vel, pan))
        # long support tone each bar
        root = chord_for_bar(b)[0] + 12
        events.append((bar_start, BEAT * 3.6, root, 0.14, 0.0))
    return events


def apply_delay(buf_l, buf_r, delay_sec: float, feedback: float):
    d = int(SR * delay_sec)
    if d <= 0:
        return
    n = len(buf_l)
    for i in range(d, n):
        buf_l[i] += buf_l[i - d] * feedback
        buf_r[i] += buf_r[i - d] * feedback


def render_hub_bgm():
    n = int(SR * DURATION)
    num_bars = int(DURATION / BAR)
    events = build_melody_events(num_bars)

    left = [0.0] * n
    right = [0.0] * n

    for i in range(n):
        t = i / SR
        bar_idx = int(t / BAR)
        beat_phase = (t % BEAT) / BEAT
        lfo = 0.5 + 0.5 * math.sin(2.0 * math.pi * 0.07 * t)

        # dark pad layer (low-mid)
        pad = 0.0
        chord = chord_for_bar(bar_idx)
        for m in chord:
            f = midi_to_freq(m)
            pad += math.sin(2.0 * math.pi * f * t) * 0.32
            pad += math.sin(2.0 * math.pi * (f * 0.5) * t) * 0.18
        pad /= 3.0
        pad *= (0.16 + 0.08 * lfo)

        # gentle pulse to avoid too-flat ambience
        pulse = 0.65 + 0.35 * math.sin(2.0 * math.pi * (BPM / 60.0) * t * 0.5)
        pad *= pulse

        # melody layer
        mel_l = 0.0
        mel_r = 0.0
        for start, dur, midi, vel, pan in events:
            e = env_note(t, start, dur, attack=0.01, release=0.16)
            if e <= 0.0:
                continue
            f = midi_to_freq(midi)
            sig = 0.70 * math.sin(2.0 * math.pi * f * t)
            sig += 0.30 * tri(2.0 * math.pi * (f * 2.0) * t)
            sig *= vel * e
            # pan (-1..1)
            pl = (1.0 - pan) * 0.5
            pr = (1.0 + pan) * 0.5
            mel_l += sig * pl
            mel_r += sig * pr

        # subtle shimmer
        shimmer = (math.sin(2.0 * math.pi * 1320.0 * t) + math.sin(2.0 * math.pi * 1760.0 * t)) * 0.01
        shimmer *= (0.4 + 0.6 * max(0.0, math.sin(2.0 * math.pi * 0.13 * t)))

        left[i] = pad + mel_l + shimmer
        right[i] = pad + mel_r + shimmer

    # simple roomy tail
    apply_delay(left, right, delay_sec=0.22, feedback=0.22)
    apply_delay(left, right, delay_sec=0.36, feedback=0.16)

    # fade in/out for smooth loop draft
    fade = int(SR * 0.35)
    for i in range(min(fade, n)):
        k = i / max(1, fade)
        left[i] *= k
        right[i] *= k
        left[n - 1 - i] *= k
        right[n - 1 - i] *= k

    # normalize
    peak = 1e-9
    for i in range(n):
        peak = max(peak, abs(left[i]), abs(right[i]))
    gain = min(0.92 / peak, 1.0)
    for i in range(n):
        left[i] *= gain
        right[i] *= gain

    return left, right


def write_stereo_wav(path: Path, left, right):
    path.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(path), "wb") as wf:
        wf.setnchannels(2)
        wf.setsampwidth(2)
        wf.setframerate(SR)
        frames = bytearray()
        for l, r in zip(left, right):
            li = int(max(-1.0, min(1.0, l)) * MAX_AMP)
            ri = int(max(-1.0, min(1.0, r)) * MAX_AMP)
            frames += li.to_bytes(2, "little", signed=True)
            frames += ri.to_bytes(2, "little", signed=True)
        wf.writeframes(frames)


def backup(path: Path):
    if path.exists():
        bak = path.with_suffix(path.suffix + ".bak_2026-02-15")
        shutil.copy2(path, bak)


if __name__ == "__main__":
    left, right = render_hub_bgm()
    for target in TARGETS:
        backup(target)
        write_stereo_wav(target, left, right)
    print("Rebuilt melodic hub BGM draft:")
    for target in TARGETS:
        print(f"- {target}")
