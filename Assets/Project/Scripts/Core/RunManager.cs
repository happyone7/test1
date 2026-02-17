using Tesseract.Core;
using UnityEngine;
using Soulspire.Audio;

namespace Soulspire.Core
{
    public class RunManager : Singleton<RunManager>
    {
        [Header("런 상태")]
        public Data.StageData CurrentStage { get; private set; }
        public int CurrentWaveIndex { get; private set; }
        public int SoulEarned { get; private set; }
        public int NodesKilled { get; private set; }
        public bool IsRunning { get; private set; }
        public RunModifiers CurrentModifiers { get; private set; }

        int _baseHp;
        int _baseMaxHp;
        bool _playedHp30Warning;
        bool _playedHp10Warning;

        public int BaseHp => _baseHp;
        public int BaseMaxHp => _baseMaxHp;

        public void StartRun(Data.StageData stage, RunModifiers modifiers)
        {
            CurrentStage = stage;
            CurrentWaveIndex = 0;
            SoulEarned = 0;
            NodesKilled = 0;
            CurrentModifiers = modifiers;
            _baseMaxHp = stage.baseHp + modifiers.bonusBaseHp;
            _baseHp = _baseMaxHp;
            IsRunning = true;
            _playedHp30Warning = false;
            _playedHp10Warning = false;

            if (Singleton<Node.WaveSpawner>.HasInstance)
                Node.WaveSpawner.Instance.StartWaves(stage);
        }

        public void AddSoul(int amount)
        {
            SoulEarned += amount;
        }

        public bool SpendSoul(int amount)
        {
            if (SoulEarned < amount) return false;
            SoulEarned -= amount;
            return true;
        }

        public void OnNodeKilled()
        {
            NodesKilled++;
        }

        public void TakeDamage(int damage)
        {
            if (!IsRunning) return;
            Debug.Log($"[RunManager] TakeDamage: damage={damage}, baseHp={_baseHp}/{_baseMaxHp}");
            _baseHp = Mathf.Max(0, _baseHp - damage);
            SoundManager.Instance.PlaySfx(SoundKeys.BaseHit, 1f);

            float hpRatio = _baseMaxHp > 0 ? (float)_baseHp / _baseMaxHp : 0f;
            if (!_playedHp10Warning && hpRatio > 0f && hpRatio <= 0.1f)
            {
                SoundManager.Instance.PlaySfx(SoundKeys.Hp10Warning, 1f);
                _playedHp10Warning = true;
                _playedHp30Warning = true;
            }
            else if (!_playedHp30Warning && hpRatio > 0f && hpRatio <= 0.3f)
            {
                SoundManager.Instance.PlaySfx(SoundKeys.Hp30Warning, 1f);
                _playedHp30Warning = true;
            }

            if (_baseHp <= 0)
                EndRun(false);
        }

        public void OnSingleWaveCleared()
        {
            CurrentWaveIndex++;
        }

        public void OnAllWavesCompleted()
        {
            EndRun(true);
        }

        /// <summary>
        /// 보물상자 MaxHpBoost 효과를 즉시 적용한다.
        /// 최대 HP와 현재 HP를 동시에 증가시킨다.
        /// </summary>
        public void ApplyBonusMaxHp(int bonus)
        {
            _baseMaxHp += bonus;
            _baseHp += bonus;
            Debug.Log($"[RunManager] 보물 MaxHpBoost 적용: +{bonus} HP (현재 {_baseHp}/{_baseMaxHp})");
        }

        public void ResetRun()
        {
            IsRunning = false;
            CurrentStage = null;
            NodesKilled = 0;

            // 보물 효과 초기화
            if (Singleton<TreasureManager>.HasInstance)
                TreasureManager.Instance.ResetEffects();
        }

        void EndRun(bool cleared)
        {
            IsRunning = false;
            if (cleared)
                SoundManager.Instance.PlaySfx(SoundKeys.StageClear, 1f);

            // core 보상 계산 (GameManager.OnRunEnd와 동일 로직)
            int coreFragmentEarned = 0;
            if (cleared && CurrentStage != null)
                coreFragmentEarned = CurrentStage.coreFragmentReward;

            GameManager.Instance.OnRunEnd(cleared, SoulEarned);

            // UI에 런 종료 알림
            var ui = Object.FindFirstObjectByType<UI.InGameUI>(FindObjectsInactive.Include);
            if (ui != null)
            {
                ui.ShowRunEnd(cleared, SoulEarned, NodesKilled, coreFragmentEarned);

                // 보스 처치 Core 보상 팝업
                if (cleared && coreFragmentEarned > 0)
                    ui.ShowCoreFragmentPopup(coreFragmentEarned);
            }

            // FTUE: 첫 사망 가이드
            if (!cleared && Singleton<UI.FTUEManager>.HasInstance)
                UI.FTUEManager.Instance.TriggerInGame("FirstDeath", UI.FTUEManager.GuideFirstDeath);
        }
    }
}
