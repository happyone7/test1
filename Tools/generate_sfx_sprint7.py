"""
Sprint 7 SD-1/SD-2: Generate tower attack SFX (4 types) + boss death SFX (1 type)
via math synthesis (numpy + scipy).

Output: WAV 16-bit 44100Hz, normalized to -14 ~ -16 LUFS target range.
"""

import os
import numpy as np
from scipy.io import wavfile
from scipy.signal import butter, lfilter, resample

SAMPLE_RATE = 44100
OUTPUT_DIR = os.path.join(
    os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
    "Assets", "Resources", "Audio", "SFX", "Combat"
)


def ensure_dir(path):
    os.makedirs(path, exist_ok=True)


def normalize_16bit(signal, target_peak=0.85):
    """Normalize signal to target peak level and convert to 16-bit."""
    peak = np.max(np.abs(signal))
    if peak > 0:
        signal = signal * (target_peak / peak)
    signal = np.clip(signal, -1.0, 1.0)
    return (signal * 32767).astype(np.int16)


def fade_in(signal, duration_s):
    """Apply linear fade-in."""
    n_samples = int(duration_s * SAMPLE_RATE)
    n_samples = min(n_samples, len(signal))
    envelope = np.ones(len(signal))
    envelope[:n_samples] = np.linspace(0, 1, n_samples)
    return signal * envelope


def fade_out(signal, duration_s):
    """Apply exponential fade-out."""
    n_samples = int(duration_s * SAMPLE_RATE)
    n_samples = min(n_samples, len(signal))
    envelope = np.ones(len(signal))
    envelope[-n_samples:] = np.exp(-np.linspace(0, 5, n_samples))
    return signal * envelope


def apply_envelope(signal, attack_s, decay_s, sustain_level, release_s):
    """ADSR envelope."""
    total = len(signal)
    attack_n = int(attack_s * SAMPLE_RATE)
    decay_n = int(decay_s * SAMPLE_RATE)
    release_n = int(release_s * SAMPLE_RATE)
    sustain_n = max(0, total - attack_n - decay_n - release_n)

    env = np.zeros(total)
    idx = 0

    # Attack
    n = min(attack_n, total - idx)
    env[idx:idx + n] = np.linspace(0, 1, n)
    idx += n

    # Decay
    n = min(decay_n, total - idx)
    env[idx:idx + n] = np.linspace(1, sustain_level, n)
    idx += n

    # Sustain
    n = min(sustain_n, total - idx)
    env[idx:idx + n] = sustain_level
    idx += n

    # Release
    n = min(release_n, total - idx)
    env[idx:idx + n] = np.linspace(sustain_level, 0, n)
    idx += n

    return signal * env


def lowpass(signal, cutoff, order=4):
    """Butterworth low-pass filter."""
    nyq = SAMPLE_RATE * 0.5
    cutoff = min(cutoff, nyq * 0.99)
    b, a = butter(order, cutoff / nyq, btype='low')
    return lfilter(b, a, signal)


def highpass(signal, cutoff, order=4):
    """Butterworth high-pass filter."""
    nyq = SAMPLE_RATE * 0.5
    cutoff = min(cutoff, nyq * 0.99)
    b, a = butter(order, cutoff / nyq, btype='high')
    return lfilter(b, a, signal)


def bandpass(signal, low, high, order=4):
    """Butterworth band-pass filter."""
    nyq = SAMPLE_RATE * 0.5
    low = max(low, 1.0)
    high = min(high, nyq * 0.99)
    b, a = butter(order, [low / nyq, high / nyq], btype='band')
    return lfilter(b, a, signal)


def white_noise(duration_s):
    """Generate white noise."""
    n = int(duration_s * SAMPLE_RATE)
    return np.random.randn(n)


def sine_wave(freq, duration_s, phase=0):
    """Generate sine wave."""
    t = np.linspace(0, duration_s, int(duration_s * SAMPLE_RATE), endpoint=False)
    return np.sin(2 * np.pi * freq * t + phase)


def saw_wave(freq, duration_s):
    """Generate sawtooth wave."""
    t = np.linspace(0, duration_s, int(duration_s * SAMPLE_RATE), endpoint=False)
    return 2 * (freq * t - np.floor(0.5 + freq * t))


def freq_sweep(start_freq, end_freq, duration_s):
    """Generate frequency sweep (chirp)."""
    t = np.linspace(0, duration_s, int(duration_s * SAMPLE_RATE), endpoint=False)
    phase = 2 * np.pi * (start_freq * t + (end_freq - start_freq) * t ** 2 / (2 * duration_s))
    return np.sin(phase)


def mix(*signals_and_gains):
    """Mix multiple (signal, gain) pairs. Pads shorter signals with zeros."""
    max_len = max(len(s) for s, _ in signals_and_gains)
    result = np.zeros(max_len)
    for signal, gain in signals_and_gains:
        padded = np.zeros(max_len)
        padded[:len(signal)] = signal
        result += padded * gain
    return result


# ============================================================
# SFX 1: Arrow Shot (short whoosh, crossbow fire)
# Duration: ~0.4s
# ============================================================
def generate_arrow():
    """
    Arrow shot: sharp attack, high-pitched whoosh with pitch drop,
    layered with a short bow string snap.
    """
    duration = 0.4

    # Layer 1: Whoosh - filtered noise sweep
    noise = white_noise(duration)
    # Frequency sweep from high to mid
    sweep = freq_sweep(4000, 800, duration)
    whoosh = noise * np.abs(sweep) * 0.5
    whoosh = bandpass(whoosh, 800, 6000)
    whoosh = apply_envelope(whoosh, 0.005, 0.08, 0.3, 0.25)

    # Layer 2: String snap - short high-freq transient
    snap_dur = 0.05
    snap = sine_wave(2500, snap_dur) + sine_wave(3800, snap_dur) * 0.5
    snap_padded = np.zeros(int(duration * SAMPLE_RATE))
    snap_env = apply_envelope(np.ones(int(snap_dur * SAMPLE_RATE)), 0.001, 0.015, 0.2, 0.02)
    snap = snap * snap_env
    snap_padded[:len(snap)] = snap

    # Layer 3: Air movement - very short noise burst
    air = white_noise(0.15)
    air = highpass(air, 2000)
    air = apply_envelope(air, 0.01, 0.04, 0.15, 0.08)
    air_padded = np.zeros(int(duration * SAMPLE_RATE))
    offset = int(0.02 * SAMPLE_RATE)
    air_padded[offset:offset + len(air)] = air

    result = mix(
        (whoosh, 0.6),
        (snap_padded, 0.8),
        (air_padded, 0.4)
    )

    result = fade_out(result, 0.1)
    return result


# ============================================================
# SFX 2: Cannon Explosion (deep boom, short impact)
# Duration: ~0.7s
# ============================================================
def generate_cannon():
    """
    Cannon explosion: low-frequency boom with transient impact,
    followed by rumble and debris.
    """
    duration = 0.7

    # Layer 1: Impact transient - very short click
    impact_dur = 0.02
    impact = white_noise(impact_dur)
    impact = apply_envelope(impact, 0.001, 0.005, 0.1, 0.01)
    impact_padded = np.zeros(int(duration * SAMPLE_RATE))
    impact_padded[:len(impact)] = impact

    # Layer 2: Low boom - sine sweep from ~120Hz dropping to ~40Hz
    boom = freq_sweep(120, 35, duration)
    boom = apply_envelope(boom, 0.005, 0.15, 0.3, 0.4)
    boom = lowpass(boom, 200)

    # Layer 3: Mid-range explosion body
    body_noise = white_noise(duration)
    body_noise = bandpass(body_noise, 100, 1500)
    body_noise = apply_envelope(body_noise, 0.003, 0.1, 0.15, 0.45)

    # Layer 4: High-frequency sizzle/debris
    debris = white_noise(0.4)
    debris = highpass(debris, 2000)
    debris = apply_envelope(debris, 0.01, 0.08, 0.1, 0.25)
    debris_padded = np.zeros(int(duration * SAMPLE_RATE))
    offset = int(0.01 * SAMPLE_RATE)
    debris_padded[offset:offset + len(debris)] = debris

    # Layer 5: Sub-bass thud
    sub = sine_wave(45, 0.3)
    sub = apply_envelope(sub, 0.002, 0.05, 0.4, 0.2)
    sub_padded = np.zeros(int(duration * SAMPLE_RATE))
    sub_padded[:len(sub)] = sub

    result = mix(
        (impact_padded, 1.0),
        (boom, 0.7),
        (body_noise, 0.5),
        (debris_padded, 0.3),
        (sub_padded, 0.6)
    )

    result = fade_out(result, 0.15)
    return result


# ============================================================
# SFX 3: Ice Freeze (cracking cold, crystal formation)
# Duration: ~0.5s
# ============================================================
def generate_ice():
    """
    Ice freeze: sharp crystalline crack followed by shimmering
    ice formation sounds.
    """
    duration = 0.5

    # Layer 1: Initial crack - sharp transient
    crack_dur = 0.04
    crack = white_noise(crack_dur)
    crack = bandpass(crack, 1500, 8000)
    crack = apply_envelope(crack, 0.001, 0.01, 0.15, 0.02)
    crack_padded = np.zeros(int(duration * SAMPLE_RATE))
    crack_padded[:len(crack)] = crack

    # Layer 2: Crystal shimmer - layered high-frequency tones
    shimmer_dur = 0.4
    shimmer1 = sine_wave(3200, shimmer_dur)
    shimmer2 = sine_wave(4800, shimmer_dur) * 0.6
    shimmer3 = sine_wave(6400, shimmer_dur) * 0.3
    shimmer4 = sine_wave(7200, shimmer_dur) * 0.15
    shimmer = shimmer1 + shimmer2 + shimmer3 + shimmer4
    # Add slight vibrato
    t_shim = np.linspace(0, shimmer_dur, int(shimmer_dur * SAMPLE_RATE), endpoint=False)
    shimmer *= (1 + 0.15 * np.sin(2 * np.pi * 12 * t_shim))
    shimmer = apply_envelope(shimmer, 0.01, 0.08, 0.25, 0.25)
    shimmer_padded = np.zeros(int(duration * SAMPLE_RATE))
    offset = int(0.03 * SAMPLE_RATE)
    shimmer_padded[offset:offset + len(shimmer)] = shimmer

    # Layer 3: Freezing noise - filtered crackle
    crackle = white_noise(0.35)
    # Create crackling by amplitude-modulating noise
    t_crk = np.linspace(0, 0.35, int(0.35 * SAMPLE_RATE), endpoint=False)
    crackle_mod = np.abs(np.sin(2 * np.pi * 25 * t_crk)) ** 4
    crackle = crackle * crackle_mod
    crackle = bandpass(crackle, 2000, 10000)
    crackle = apply_envelope(crackle, 0.02, 0.05, 0.2, 0.2)
    crackle_padded = np.zeros(int(duration * SAMPLE_RATE))
    offset = int(0.02 * SAMPLE_RATE)
    crackle_padded[offset:offset + len(crackle)] = crackle

    # Layer 4: Sub-tone resonance (ice mass)
    sub = sine_wave(180, 0.2) + sine_wave(260, 0.2) * 0.5
    sub = apply_envelope(sub, 0.005, 0.05, 0.2, 0.12)
    sub_padded = np.zeros(int(duration * SAMPLE_RATE))
    sub_padded[:len(sub)] = sub

    result = mix(
        (crack_padded, 0.9),
        (shimmer_padded, 0.5),
        (crackle_padded, 0.4),
        (sub_padded, 0.3)
    )

    result = fade_out(result, 0.08)
    return result


# ============================================================
# SFX 4: Lightning Chain (electric arc, tesla crackling)
# Duration: ~0.5s
# ============================================================
def generate_lightning():
    """
    Lightning chain: sharp electric discharge with crackling arcs,
    buzzy harmonics, and sizzle tail.
    """
    duration = 0.5

    # Layer 1: Main discharge - harsh buzz with harmonics
    t = np.linspace(0, duration, int(duration * SAMPLE_RATE), endpoint=False)
    # Sawtooth-based buzz at 120Hz (electrical hum) with overtones
    buzz = saw_wave(120, duration)
    buzz += saw_wave(240, duration) * 0.5
    buzz += saw_wave(360, duration) * 0.25
    buzz = bandpass(buzz, 80, 4000)
    buzz = apply_envelope(buzz, 0.002, 0.06, 0.2, 0.3)

    # Layer 2: Crackling arcs - intermittent noise bursts
    arc = white_noise(duration)
    # Create random crackling pattern
    arc_mod = np.zeros(len(arc))
    np.random.seed(42)
    n_arcs = 12
    for i in range(n_arcs):
        pos = int(np.random.uniform(0, len(arc) * 0.7))
        width = int(np.random.uniform(200, 800))
        end = min(pos + width, len(arc))
        arc_mod[pos:end] = np.random.uniform(0.5, 1.0)
    arc = arc * arc_mod
    arc = bandpass(arc, 1000, 12000)
    arc = apply_envelope(arc, 0.001, 0.03, 0.3, 0.3)

    # Layer 3: Initial zap transient
    zap_dur = 0.03
    zap = white_noise(zap_dur) + sine_wave(5000, zap_dur) * 0.5
    zap = apply_envelope(zap, 0.001, 0.008, 0.2, 0.015)
    zap_padded = np.zeros(int(duration * SAMPLE_RATE))
    zap_padded[:len(zap)] = zap

    # Layer 4: Sizzle tail
    sizzle_dur = 0.3
    sizzle = white_noise(sizzle_dur)
    sizzle = highpass(sizzle, 4000)
    sizzle = apply_envelope(sizzle, 0.02, 0.05, 0.15, 0.2)
    sizzle_padded = np.zeros(int(duration * SAMPLE_RATE))
    offset = int(0.15 * SAMPLE_RATE)
    sizzle_padded[offset:offset + len(sizzle)] = sizzle

    # Layer 5: Low rumble from discharge
    rumble = sine_wave(60, 0.2)
    rumble = apply_envelope(rumble, 0.003, 0.04, 0.3, 0.12)
    rumble_padded = np.zeros(int(duration * SAMPLE_RATE))
    rumble_padded[:len(rumble)] = rumble

    result = mix(
        (buzz, 0.5),
        (arc, 0.6),
        (zap_padded, 1.0),
        (sizzle_padded, 0.35),
        (rumble_padded, 0.3)
    )

    result = fade_out(result, 0.1)
    return result


# ============================================================
# SFX 5: Boss Death (cinematic heavy explosion + rumble)
# Duration: ~1.5s
# ============================================================
def generate_boss_death():
    """
    Boss death: massive cinematic impact with slow-motion feel.
    Deep rumble, layered explosions, and a reverberant tail.
    """
    duration = 1.5

    # Layer 1: Initial massive impact
    impact = white_noise(0.05)
    impact = apply_envelope(impact, 0.001, 0.01, 0.3, 0.03)
    impact_padded = np.zeros(int(duration * SAMPLE_RATE))
    impact_padded[:len(impact)] = impact

    # Layer 2: Deep bass boom - very low sweep
    boom = freq_sweep(80, 20, 1.0)
    boom = apply_envelope(boom, 0.005, 0.2, 0.25, 0.6)
    boom = lowpass(boom, 150)
    boom_padded = np.zeros(int(duration * SAMPLE_RATE))
    boom_padded[:len(boom)] = boom

    # Layer 3: Sub-bass foundation
    sub = sine_wave(30, 1.0)
    sub = apply_envelope(sub, 0.01, 0.15, 0.35, 0.6)
    sub_padded = np.zeros(int(duration * SAMPLE_RATE))
    sub_padded[:len(sub)] = sub

    # Layer 4: Mid-range explosion body
    body = white_noise(1.2)
    body = bandpass(body, 200, 2000)
    body = apply_envelope(body, 0.003, 0.15, 0.2, 0.8)
    body_padded = np.zeros(int(duration * SAMPLE_RATE))
    body_padded[:len(body)] = body

    # Layer 5: Secondary explosion (delayed for cinematic feel)
    sec_offset = int(0.15 * SAMPLE_RATE)
    sec_boom = freq_sweep(100, 30, 0.8)
    sec_boom = apply_envelope(sec_boom, 0.005, 0.1, 0.2, 0.5)
    sec_boom = lowpass(sec_boom, 200)
    sec_padded = np.zeros(int(duration * SAMPLE_RATE))
    sec_padded[sec_offset:sec_offset + len(sec_boom)] = sec_boom

    # Layer 6: High-freq shatter/debris
    shatter = white_noise(0.8)
    shatter = highpass(shatter, 3000)
    shatter = apply_envelope(shatter, 0.01, 0.1, 0.1, 0.55)
    shatter_padded = np.zeros(int(duration * SAMPLE_RATE))
    offset = int(0.02 * SAMPLE_RATE)
    shatter_padded[offset:offset + len(shatter)] = shatter

    # Layer 7: Reverb-like tail (simulated with decaying filtered noise)
    tail = white_noise(1.2)
    tail = bandpass(tail, 150, 1200)
    tail = apply_envelope(tail, 0.05, 0.15, 0.1, 0.9)
    tail_padded = np.zeros(int(duration * SAMPLE_RATE))
    offset = int(0.2 * SAMPLE_RATE)
    tail_padded[offset:offset + len(tail)] = tail

    # Layer 8: Cinematic tonal element (dark chord)
    tone1 = sine_wave(55, 1.2)   # Low A
    tone2 = sine_wave(82.4, 1.0) * 0.6  # Low E
    tone3 = sine_wave(110, 0.8) * 0.3   # A
    tones = tone1[:len(tone3)] * 0.5 + tone2[:len(tone3)] * 0.3 + tone3 * 0.2
    tones_full = np.zeros(int(1.2 * SAMPLE_RATE))
    tones_full[:len(tones)] = tones[:len(tones_full)]
    tones_full = apply_envelope(tones_full, 0.01, 0.2, 0.2, 0.7)
    tones_padded = np.zeros(int(duration * SAMPLE_RATE))
    tones_padded[:len(tones_full)] = tones_full

    result = mix(
        (impact_padded, 1.0),
        (boom_padded, 0.7),
        (sub_padded, 0.5),
        (body_padded, 0.45),
        (sec_padded, 0.5),
        (shatter_padded, 0.25),
        (tail_padded, 0.3),
        (tones_padded, 0.4)
    )

    result = fade_out(result, 0.3)
    return result


def save_wav(signal, filename):
    """Save signal as 16-bit WAV."""
    data = normalize_16bit(signal, target_peak=0.85)
    filepath = os.path.join(OUTPUT_DIR, filename)
    wavfile.write(filepath, SAMPLE_RATE, data)
    file_size = os.path.getsize(filepath)
    duration = len(signal) / SAMPLE_RATE
    print(f"  {filename}: {duration:.2f}s, {file_size:,} bytes ({file_size/1024:.1f} KB)")
    return filepath


def main():
    ensure_dir(OUTPUT_DIR)

    print("=" * 60)
    print("Sprint 7 SFX Generation - Math Synthesis")
    print("=" * 60)

    # SD-1: Tower Attack SFX (4 types)
    print("\n[SD-1] Tower Attack SFX (4 types)")
    print("-" * 40)

    sfx_files = {}

    print("Generating Arrow Shot...")
    arrow = generate_arrow()
    sfx_files["arrow"] = save_wav(arrow, "SFX_Tower_Arrow_01.wav")

    print("Generating Cannon Explosion...")
    cannon = generate_cannon()
    sfx_files["cannon"] = save_wav(cannon, "SFX_Tower_Cannon_01.wav")

    print("Generating Ice Freeze...")
    ice = generate_ice()
    sfx_files["ice"] = save_wav(ice, "SFX_Tower_Ice_01.wav")

    print("Generating Lightning Chain...")
    lightning = generate_lightning()
    sfx_files["lightning"] = save_wav(lightning, "SFX_Tower_Lightning_01.wav")

    # SD-2: Boss Death SFX
    print("\n[SD-2] Boss Death SFX")
    print("-" * 40)

    print("Generating Boss Death...")
    boss_death = generate_boss_death()
    sfx_files["boss_death"] = save_wav(boss_death, "SFX_Boss_Death_01.wav")

    print("\n" + "=" * 60)
    print("All SFX generated successfully!")
    print(f"Output directory: {OUTPUT_DIR}")
    print("=" * 60)

    # Quality validation
    print("\n[QA] Quality Validation")
    print("-" * 40)
    all_pass = True
    for name, path in sfx_files.items():
        rate, data = wavfile.read(path)
        duration = len(data) / rate
        file_size = os.path.getsize(path)

        issues = []
        if name == "boss_death":
            if duration < 1.0 or duration > 2.0:
                issues.append(f"Duration {duration:.2f}s out of range (1.0~2.0s)")
        else:
            if duration < 0.2 or duration > 2.0:
                issues.append(f"Duration {duration:.2f}s out of range (0.2~2.0s)")

        if file_size < 10 * 1024:
            issues.append(f"File too small ({file_size} bytes < 10KB)")
        elif file_size > 500 * 1024:
            issues.append(f"File too large ({file_size/1024:.0f}KB > 500KB)")

        if rate != 44100:
            issues.append(f"Sample rate {rate} != 44100")

        status = "PASS" if not issues else "FAIL"
        print(f"  {name}: {status}")
        if issues:
            for issue in issues:
                print(f"    - {issue}")
            all_pass = False

    if all_pass:
        print("\nAll quality checks PASSED.")
    else:
        print("\nSome quality checks FAILED. Review above.")

    return all_pass


if __name__ == "__main__":
    main()
