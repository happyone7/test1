using Tesseract.Core;
using UnityEngine;

namespace Nodebreaker.Core
{
    public enum GameState
    {
        Hub,
        InGame,
        RunEnd
    }

    public class GameManager : Singleton<GameManager>
    {
        [Header("스테이지 데이터")]
        public Data.StageData[] stages;

        [Header("현재 상태")]
        public GameState State { get; private set; } = GameState.Hub;

        // MetaManager 위임 프로퍼티
        public int TotalBit => MetaManager.Instance.TotalBit;
        public int TotalCore => MetaManager.Instance.TotalCore;
        public int CurrentStageIndex => MetaManager.Instance.CurrentStageIndex;

        protected override void Awake()
        {
            base.Awake();
        }

        void Start()
        {
            State = GameState.Hub;
            var hubUI = Object.FindFirstObjectByType<UI.HubUI>();
            if (hubUI != null)
                hubUI.Show();
        }

        public void StartRun()
        {
            int stageIdx = MetaManager.Instance.CurrentStageIndex;
            if (stageIdx >= stages.Length) return;

            State = GameState.InGame;

            var hubUI = Object.FindFirstObjectByType<UI.HubUI>();
            if (hubUI != null)
                hubUI.Hide();

            RunModifiers mods = MetaManager.Instance.CalculateModifiers();
            RunManager.Instance.StartRun(stages[stageIdx], mods);
        }

        public void OnRunEnd(bool cleared, int bitEarned)
        {
            int stageIdx = MetaManager.Instance.CurrentStageIndex;
            int coreEarned = cleared ? stages[Mathf.Min(stageIdx, stages.Length - 1)].coreReward : 0;

            MetaManager.Instance.AddRunRewards(bitEarned, coreEarned, cleared, stageIdx);
            State = GameState.RunEnd;
        }

        public void GoToHub()
        {
            State = GameState.Hub;
            var hubUI = Object.FindFirstObjectByType<UI.HubUI>();
            if (hubUI != null)
                hubUI.Show();
        }
    }
}
