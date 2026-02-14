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

        [Header("프로토타입 설정")]
        public bool autoStartRun = true;
        public float autoStartDelay = 1f;

        [Header("현재 상태")]
        public GameState State { get; private set; } = GameState.Hub;

        public int CurrentStageIndex { get; set; }
        public int TotalBit { get; set; }
        public int TotalCore { get; set; }

        protected override void Awake()
        {
            base.Awake();
            LoadData();
        }

        void Start()
        {
            if (autoStartRun)
                Invoke(nameof(StartRun), autoStartDelay);
        }

        public void StartRun()
        {
            if (CurrentStageIndex >= stages.Length) return;
            State = GameState.InGame;
            RunManager.Instance.StartRun(stages[CurrentStageIndex]);
        }

        public void OnRunEnd(bool cleared, int bitEarned)
        {
            TotalBit += bitEarned;
            if (cleared)
            {
                TotalCore += stages[CurrentStageIndex].coreReward;
                CurrentStageIndex = Mathf.Min(CurrentStageIndex + 1, stages.Length - 1);
            }
            State = GameState.RunEnd;
            SaveData();
        }

        public void GoToHub()
        {
            State = GameState.Hub;
        }

        void LoadData()
        {
            TotalBit = PlayerPrefs.GetInt("TotalBit", 0);
            TotalCore = PlayerPrefs.GetInt("TotalCore", 0);
            CurrentStageIndex = PlayerPrefs.GetInt("CurrentStage", 0);
        }

        void SaveData()
        {
            PlayerPrefs.SetInt("TotalBit", TotalBit);
            PlayerPrefs.SetInt("TotalCore", TotalCore);
            PlayerPrefs.SetInt("CurrentStage", CurrentStageIndex);
            PlayerPrefs.Save();
        }
    }
}
