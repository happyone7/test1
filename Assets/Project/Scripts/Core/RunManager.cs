using System;
using Tesseract.Core;
using UnityEngine;
using Nodebreaker.Audio;

namespace Nodebreaker.Core
{
    public class RunManager : Singleton<RunManager>
    {
        [Header("런 상태")]
        public Data.StageData CurrentStage { get; private set; }
        public int CurrentWaveIndex { get; private set; }
        public int BitEarned { get; private set; }
        public int NodesKilled { get; private set; }
        public bool IsRunning { get; private set; }
        public RunModifiers CurrentModifiers { get; private set; }
        public bool BossDefeated { get; private set; }

        /// <summary>보스 처치 시 발생하는 이벤트</summary>
        public event Action OnBossDefeated;

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
            BitEarned = modifiers.startBitBonus;
            NodesKilled = 0;
            CurrentModifiers = modifiers;
            _baseMaxHp = stage.baseHp + modifiers.bonusBaseHp;
            _baseHp = _baseMaxHp;
            IsRunning = true;
            BossDefeated = false;
            _playedHp30Warning = false;
            _playedHp10Warning = false;
            _regenAccumulator = 0f;

            // 보물상자 매니저 초기화
            if (Singleton<TreasureChestManager>.HasInstance)
                TreasureChestManager.Instance.ResetForNewRun();

            if (BitEarned > 0)
                Debug.Log($"[RunManager] 시작 Bit 보너스 적용: +{BitEarned}");

            if (Singleton<Node.WaveSpawner>.HasInstance)
                Node.WaveSpawner.Instance.StartWaves(stage);
        }

        float _regenAccumulator;

        void Update()
        {
            if (!IsRunning) return;

            // HP 자동 회복
            if (CurrentModifiers.hpRegenPerSec > 0f && _baseHp > 0 && _baseHp < _baseMaxHp)
            {
                _regenAccumulator += CurrentModifiers.hpRegenPerSec * Time.deltaTime;
                if (_regenAccumulator >= 1f)
                {
                    int regenAmount = Mathf.FloorToInt(_regenAccumulator);
                    _regenAccumulator -= regenAmount;
                    _baseHp = Mathf.Min(_baseMaxHp, _baseHp + regenAmount);
                }
            }
        }

        public void AddBit(int amount)
        {
            BitEarned += amount;
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

        public void ResetRun()
        {
            IsRunning = false;
            CurrentStage = null;
            NodesKilled = 0;
            BossDefeated = false;

            // 배속을 x1로 복귀 (TimeScale 안전 보장)
            if (Singleton<SpeedController>.HasInstance)
                SpeedController.Instance.ResetToDefault();
        }

        /// <summary>보스 처치 시 호출</summary>
        public void OnBossKilled()
        {
            BossDefeated = true;
            OnBossDefeated?.Invoke();

            // 보스 처치 시 보물상자 100% 드랍
            if (Singleton<TreasureChestManager>.HasInstance)
                TreasureChestManager.Instance.TryDropOnBossKill();
        }

        /// <summary>런 모디파이어 갱신 (보물상자 보상 적용 시 사용)</summary>
        public void SetModifiers(RunModifiers mods)
        {
            CurrentModifiers = mods;
        }

        /// <summary>기지 HP 회복 (최대 HP 초과 불가)</summary>
        public void HealBase(int amount)
        {
            _baseHp = Mathf.Min(_baseMaxHp, _baseHp + amount);
        }

        /// <summary>기지 최대 HP 증가 (현재 HP도 동일량 증가)</summary>
        public void IncreaseMaxHp(int amount)
        {
            _baseMaxHp += amount;
            _baseHp += amount;
        }

        void EndRun(bool cleared)
        {
            IsRunning = false;
            if (cleared)
                SoundManager.Instance.PlaySfx(SoundKeys.StageClear, 1f);

            // core 보상 계산 (GameManager.OnRunEnd와 동일 로직)
            int coreEarned = 0;
            if (cleared && CurrentStage != null)
                coreEarned = CurrentStage.coreReward;

            GameManager.Instance.OnRunEnd(cleared, BitEarned);

            // UI에 런 종료 알림
            var ui = Object.FindFirstObjectByType<UI.InGameUI>(FindObjectsInactive.Include);
            if (ui != null)
                ui.ShowRunEnd(cleared, BitEarned, NodesKilled, coreEarned);
        }
    }
}
