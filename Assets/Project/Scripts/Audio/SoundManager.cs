using System.Collections;
using System.Collections.Generic;
using Tesseract.Core;
using UnityEngine;

namespace Nodebreaker.Audio
{
    public class SoundManager : Singleton<SoundManager>
    {
        private const string BgmVolumeKey = "nb.audio.bgm";
        private const string SfxVolumeKey = "nb.audio.sfx";

        private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();
        private readonly HashSet<string> _missingWarnings = new HashSet<string>();

        private AudioSource _bgmSource;
        private AudioSource _sfxSource;

        private Coroutine _bgmFadeCoroutine;
        private float _bgmVolume = 1f;
        private float _sfxVolume = 1f;

        protected override void Awake()
        {
            base.Awake();
            SetupSources();
            LoadVolumes();
            LoadDefaultCatalog();
        }

        private void SetupSources()
        {
            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.playOnAwake = false;
            _bgmSource.loop = true;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.loop = false;
        }

        private void LoadVolumes()
        {
            _bgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, 1f);
            _sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
            ApplyVolumes();
        }

        private void ApplyVolumes()
        {
            if (_bgmSource != null)
                _bgmSource.volume = _bgmVolume;
            if (_sfxSource != null)
                _sfxSource.volume = _sfxVolume;
        }

        private void Register(string key, string resourcePath)
        {
            AudioClip clip = Resources.Load<AudioClip>(resourcePath);
            if (clip == null)
            {
                Debug.LogWarning($"[SoundManager] Missing clip at Resources/{resourcePath}");
                return;
            }
            _clips[key] = clip;
        }

        private void LoadDefaultCatalog()
        {
            Register(SoundKeys.BgmHub, "Audio/BGM/BGM_Hub_MainTheme_01");
            Register(SoundKeys.BgmCombat, "Audio/BGM/BGM_Combat_Wave_01");
            Register(SoundKeys.BgmBoss, "Audio/BGM/BGM_Boss_01");

            Register(SoundKeys.TowerAttack, "Audio/SFX/Combat/SFX_Tower_Attack_01");
            Register(SoundKeys.ProjectileHit, "Audio/SFX/Combat/SFX_Projectile_Hit_01");
            Register(SoundKeys.NodeDie, "Audio/SFX/Combat/SFX_Node_Die_01");
            Register(SoundKeys.TowerPlace, "Audio/SFX/Combat/SFX_Tower_Place_01");
            Register(SoundKeys.TowerMerge, "Audio/SFX/Combat/SFX_Tower_Merge_01");
            Register(SoundKeys.CritHit, "Audio/SFX/Combat/SFX_Crit_Hit_01");

            Register(SoundKeys.WaveStart, "Audio/SFX/Alert/ALT_Wave_Start_01");
            Register(SoundKeys.WaveClear, "Audio/SFX/Alert/ALT_Wave_Clear_01");
            Register(SoundKeys.BossAppear, "Audio/SFX/Alert/ALT_Boss_Appear_01");
            Register(SoundKeys.BossDefeat, "Audio/SFX/Alert/ALT_Boss_Defeat_01");
            Register(SoundKeys.StageClear, "Audio/SFX/Alert/ALT_Stage_Clear_01");
            Register(SoundKeys.BaseHit, "Audio/SFX/Alert/ALT_Base_Hit_01");
            Register(SoundKeys.Hp30Warning, "Audio/SFX/Alert/ALT_Hp30_Warning_01");
            Register(SoundKeys.Hp10Warning, "Audio/SFX/Alert/ALT_Hp10_Warning_01");

            Register(SoundKeys.UiClick, "Audio/SFX/UI/UI_Button_Click_01");
        }

        private AudioClip GetClip(string key)
        {
            if (_clips.TryGetValue(key, out AudioClip clip))
                return clip;

            if (_missingWarnings.Add(key))
                Debug.LogWarning($"[SoundManager] Unregistered sound key: {key}");
            return null;
        }

        public void SetBgmVolume(float value)
        {
            _bgmVolume = Mathf.Clamp01(value);
            if (_bgmSource != null)
                _bgmSource.volume = _bgmVolume;
            PlayerPrefs.SetFloat(BgmVolumeKey, _bgmVolume);
        }

        public void SetSfxVolume(float value)
        {
            _sfxVolume = Mathf.Clamp01(value);
            if (_sfxSource != null)
                _sfxSource.volume = _sfxVolume;
            PlayerPrefs.SetFloat(SfxVolumeKey, _sfxVolume);
        }

        public float GetBgmVolume() => _bgmVolume;
        public float GetSfxVolume() => _sfxVolume;

        public void PlaySfx(string key, float volumeScale = 1f)
        {
            AudioClip clip = GetClip(key);
            if (clip == null || _sfxSource == null) return;
            _sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
        }

        public void PlayBgm(string key, float fadeSeconds = 0.35f)
        {
            AudioClip clip = GetClip(key);
            if (clip == null || _bgmSource == null) return;

            if (_bgmSource.clip == clip && _bgmSource.isPlaying)
                return;

            if (_bgmFadeCoroutine != null)
                StopCoroutine(_bgmFadeCoroutine);

            if (fadeSeconds <= 0f)
            {
                _bgmSource.clip = clip;
                _bgmSource.loop = true;
                _bgmSource.volume = _bgmVolume;
                _bgmSource.Play();
                return;
            }

            _bgmFadeCoroutine = StartCoroutine(FadeToClip(clip, fadeSeconds));
        }

        private IEnumerator FadeToClip(AudioClip nextClip, float fadeSeconds)
        {
            float half = fadeSeconds * 0.5f;
            float startVol = _bgmSource.volume;
            float t = 0f;

            if (_bgmSource.isPlaying)
            {
                while (t < half)
                {
                    t += Time.unscaledDeltaTime;
                    float k = t / Mathf.Max(half, 0.0001f);
                    _bgmSource.volume = Mathf.Lerp(startVol, 0f, k);
                    yield return null;
                }
            }

            _bgmSource.Stop();
            _bgmSource.clip = nextClip;
            _bgmSource.loop = true;
            _bgmSource.volume = 0f;
            _bgmSource.Play();

            t = 0f;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                float k = t / Mathf.Max(half, 0.0001f);
                _bgmSource.volume = Mathf.Lerp(0f, _bgmVolume, k);
                yield return null;
            }

            _bgmSource.volume = _bgmVolume;
            _bgmFadeCoroutine = null;
        }
    }
}
