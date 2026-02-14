using Tesseract.Core;
using UnityEngine;

namespace Nodebreaker.Core
{
    public class RunManager : Singleton<RunManager>
    {
        [Header("런 상태")]
        public Data.StageData CurrentStage { get; private set; }
        public int CurrentWaveIndex { get; private set; }
        public int BitEarned { get; private set; }
        public bool IsRunning { get; private set; }

        int _baseHp;
        int _baseMaxHp;

        public int BaseHp => _baseHp;
        public int BaseMaxHp => _baseMaxHp;

        public void StartRun(Data.StageData stage)
        {
            CurrentStage = stage;
            CurrentWaveIndex = 0;
            BitEarned = 0;
            _baseMaxHp = stage.baseHp;
            _baseHp = _baseMaxHp;
            IsRunning = true;

            if (Singleton<Node.WaveSpawner>.HasInstance)
                Node.WaveSpawner.Instance.StartWaves(stage);
        }

        public void AddBit(int amount)
        {
            BitEarned += amount;
        }

        public void TakeDamage(int damage)
        {
            if (!IsRunning) return;
            _baseHp = Mathf.Max(0, _baseHp - damage);
            if (_baseHp <= 0)
                EndRun(false);
        }

        public void OnWaveCompleted()
        {
            CurrentWaveIndex++;
            if (CurrentWaveIndex >= CurrentStage.waves.Length)
                EndRun(true);
        }

        void EndRun(bool cleared)
        {
            IsRunning = false;
            GameManager.Instance.OnRunEnd(cleared, BitEarned);

            // UI에 런 종료 알림
            var ui = Object.FindFirstObjectByType<UI.InGameUI>();
            if (ui != null)
                ui.ShowRunEnd(cleared, BitEarned);
        }
    }
}
