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
        private readonly Dictionary<string, float> _balanceMap = new Dictionary<string, float>();
        private readonly HashSet<string> _missingWarnings = new HashSet<string>();

        private AudioSource _bgmSource;
        private AudioSource _sfxSource;

        private Coroutine _bgmFadeCoroutine;
        private float _bgmVolume = 1f;
        private float _sfxVolume = 1f;
        private float _currentBgmBalance = 1f;

        protected override void Awake()
        {
            base.Awake();
            SetupSources();
            LoadVolumes();
            LoadDefaultCatalog();
            LoadBalanceMap();
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
            _bgmVolume = Mathf.Clamp(PlayerPrefs.GetFloat(BgmVolumeKey, 1f), 0f, 1f);
            _sfxVolume = Mathf.Clamp(PlayerPrefs.GetFloat(SfxVolumeKey, 1f), 0f, 1f);
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

            // Phase 2: Hub
            Register(SoundKeys.BgmHubPhase2, "Audio/BGM/BGM_Hub_Phase2_01");
            Register(SoundKeys.SkillPurchase, "Audio/SFX/Hub/SFX_Skill_Purchase_01");
            Register(SoundKeys.SkillMaxLevel, "Audio/SFX/Hub/SFX_Skill_MaxLevel_01");
            Register(SoundKeys.UiHubClick, "Audio/SFX/UI/UI_Hub_Click_01");
            Register(SoundKeys.RunStart, "Audio/SFX/Hub/SFX_Run_Start_01");

            // Phase 3: Tower Upgrade / FTUE / Stage Unlock
            Register(SoundKeys.TowerUpgrade, "Audio/SFX/Combat/SFX_Tower_Upgrade_01");
            Register(SoundKeys.UiGuideAppear, "Audio/SFX/UI/UI_Guide_Appear_01");
            Register(SoundKeys.StageUnlock, "Audio/SFX/Alert/SFX_Stage_Unlock_01");

            // Phase 4: Treasure/Reward
            Register(SoundKeys.TreasureDrop, "Audio/SFX/Reward/SFX_Treasure_Drop_01");
            Register(SoundKeys.TreasureOpen, "Audio/SFX/Reward/SFX_Treasure_Open_01");
            Register(SoundKeys.TreasureSelect, "Audio/SFX/Reward/SFX_Treasure_Select_01");
        }

        /// <summary>
        /// 중앙 볼륨 밸런스 맵 - 게임 로직 스크립트 수정 없이 per-key 볼륨 조정.
        /// 값은 caller의 volumeScale에 곱해지는 추가 배율 (1.0 = 변화 없음).
        /// </summary>
        private void LoadBalanceMap()
        {
            // === 고빈도 전투 SFX: 피로도 감소를 위해 낮게 ===
            _balanceMap[SoundKeys.TowerAttack] = 0.75f;
            _balanceMap[SoundKeys.ProjectileHit] = 0.7f;

            // === 중빈도 전투 SFX: 적당한 피드백 유지 ===
            _balanceMap[SoundKeys.NodeDie] = 0.85f;
            _balanceMap[SoundKeys.TowerPlace] = 0.9f;
            _balanceMap[SoundKeys.TowerMerge] = 0.9f;
            _balanceMap[SoundKeys.CritHit] = 0.9f;

            // === 저빈도 알림 SFX: 주의 환기를 위해 높게 ===
            _balanceMap[SoundKeys.WaveStart] = 0.95f;
            _balanceMap[SoundKeys.WaveClear] = 0.95f;
            _balanceMap[SoundKeys.BossAppear] = 1.0f;
            _balanceMap[SoundKeys.BossDefeat] = 1.0f;
            _balanceMap[SoundKeys.StageClear] = 1.0f;
            _balanceMap[SoundKeys.BaseHit] = 0.9f;
            _balanceMap[SoundKeys.Hp30Warning] = 0.95f;
            _balanceMap[SoundKeys.Hp10Warning] = 1.0f;

            // === UI ===
            _balanceMap[SoundKeys.UiClick] = 0.85f;

            // === BGM: 전투 BGM은 SFX와의 마스킹 방지를 위해 살짝 낮게 ===
            _balanceMap[SoundKeys.BgmHub] = 0.85f;
            _balanceMap[SoundKeys.BgmCombat] = 0.7f;
            _balanceMap[SoundKeys.BgmBoss] = 0.75f;

            // === Phase 2: Hub ===
            _balanceMap[SoundKeys.BgmHubPhase2] = 0.85f;
            _balanceMap[SoundKeys.SkillPurchase] = 0.9f;
            _balanceMap[SoundKeys.SkillMaxLevel] = 0.95f;
            _balanceMap[SoundKeys.UiHubClick] = 0.85f;
            _balanceMap[SoundKeys.RunStart] = 1.0f;

            // === Phase 3: Tower Upgrade / FTUE / Stage Unlock ===
            _balanceMap[SoundKeys.TowerUpgrade] = 0.9f;
            _balanceMap[SoundKeys.UiGuideAppear] = 0.8f;
            _balanceMap[SoundKeys.StageUnlock] = 0.95f;

            // === Phase 4: Treasure/Reward ===
            _balanceMap[SoundKeys.TreasureDrop] = 0.9f;
            _balanceMap[SoundKeys.TreasureOpen] = 0.95f;
            _balanceMap[SoundKeys.TreasureSelect] = 0.85f;
        }

        private float GetBalance(string key)
        {
            return _balanceMap.TryGetValue(key, out float balance) ? balance : 1f;
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
                _bgmSource.volume = _bgmVolume * _currentBgmBalance;
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
            float finalScale = Mathf.Clamp01(volumeScale * GetBalance(key));
            _sfxSource.PlayOneShot(clip, finalScale);
        }

        public void PlayBgm(string key, float fadeSeconds = 0.35f)
        {
            AudioClip clip = GetClip(key);
            if (clip == null || _bgmSource == null) return;

            if (_bgmSource.clip == clip && _bgmSource.isPlaying)
                return;

            if (_bgmFadeCoroutine != null)
                StopCoroutine(_bgmFadeCoroutine);

            _currentBgmBalance = GetBalance(key);
            float targetVol = _bgmVolume * _currentBgmBalance;

            if (fadeSeconds <= 0f)
            {
                _bgmSource.clip = clip;
                _bgmSource.loop = true;
                _bgmSource.volume = targetVol;
                _bgmSource.Play();
                return;
            }

            _bgmFadeCoroutine = StartCoroutine(FadeToClip(clip, fadeSeconds));
        }

        private IEnumerator FadeToClip(AudioClip nextClip, float fadeSeconds)
        {
            float half = fadeSeconds * 0.5f;
            float startVol = _bgmSource.volume;
            float targetVol = _bgmVolume * _currentBgmBalance;
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
                _bgmSource.volume = Mathf.Lerp(0f, targetVol, k);
                yield return null;
            }

            _bgmSource.volume = targetVol;
            _bgmFadeCoroutine = null;
        }
    }
}
