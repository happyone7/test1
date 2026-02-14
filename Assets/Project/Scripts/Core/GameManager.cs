using Tesseract.Core;
using UnityEngine;
using Nodebreaker.Audio;

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

            // InGame UI 요소 숨기기 (Hub 상태이므로)
            var inGameUI = Object.FindFirstObjectByType<UI.InGameUI>(FindObjectsInactive.Include);
            if (inGameUI != null)
                inGameUI.HideAll();

            // DEBUG: 타이틀 스킵 - 바로 허브로 진입
            var titleUI = Object.FindFirstObjectByType<UI.TitleScreenUI>(FindObjectsInactive.Include);
            if (titleUI != null)
                titleUI.Hide();

            var hubUI = Object.FindFirstObjectByType<UI.HubUI>(FindObjectsInactive.Include);
            if (hubUI != null)
                hubUI.Show();

            SoundManager.Instance.PlayBgm(SoundKeys.BgmHub, 0f);
        }

        // DEBUG: 타이틀 스킵용 임시 코드 (나중에 제거)
        void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F1))
            {
                var titleUI = Object.FindFirstObjectByType<UI.TitleScreenUI>(FindObjectsInactive.Include);
                if (titleUI != null) titleUI.Hide();
                GoToHub();
            }
        }

        public void StartRun(int stageIndex = -1)
        {
            int stageIdx = stageIndex >= 0 ? stageIndex : MetaManager.Instance.CurrentStageIndex;
            if (stageIdx >= stages.Length)
            {
                Debug.LogWarning($"[GameManager] StartRun: stageIdx={stageIdx} >= stages.Length={stages.Length}, fallback to {stages.Length - 1}");
                stageIdx = Mathf.Max(0, stages.Length - 1);
            }

            State = GameState.InGame;
            SoundManager.Instance.PlayBgm(SoundKeys.BgmCombat, 0.5f);

            var hubUI = Object.FindFirstObjectByType<UI.HubUI>(FindObjectsInactive.Include);
            if (hubUI != null)
                hubUI.Hide();

            // 인벤토리 초기화 및 FTUE 타워 지급
            if (Singleton<Tower.TowerInventory>.HasInstance)
            {
                var inventory = Tower.TowerInventory.Instance;
                inventory.Clear();

                // 첫 런: Arrow 타워 1개 자동 지급
                if (Singleton<Tower.TowerManager>.HasInstance)
                {
                    var available = Tower.TowerManager.Instance.availableTowers;
                    if (available != null && available.Length > 0)
                    {
                        // Arrow 타워 우선, 없으면 첫 번째 타워
                        Data.TowerData starterTower = available[0];
                        foreach (var td in available)
                        {
                            if (td.type == Data.TowerType.Arrow)
                            {
                                starterTower = td;
                                break;
                            }
                        }
                        inventory.TryAddTower(starterTower, 1);
                    }
                }
            }

            // 배치된 타워 정리 (이전 런에서 남은 것)
            if (Singleton<Tower.TowerManager>.HasInstance)
                Tower.TowerManager.Instance.ClearAllTowers();

            // 배치 그리드 점유 초기화
            if (Singleton<Tower.PlacementGrid>.HasInstance)
                Tower.PlacementGrid.Instance.ClearOccupied();

            // InGameUI 활성화 (Hub에서 비활성화되었을 수 있음)
            var inGameUI = Object.FindFirstObjectByType<UI.InGameUI>(FindObjectsInactive.Include);
            if (inGameUI != null)
                inGameUI.ShowAll();

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
            SoundManager.Instance.PlayBgm(SoundKeys.BgmHub, 0.5f);

            // 런 상태 정리
            if (Singleton<RunManager>.HasInstance)
                RunManager.Instance.ResetRun();

            // 웨이브 스폰 중지
            if (Singleton<Node.WaveSpawner>.HasInstance)
                Node.WaveSpawner.Instance.StopSpawning();

            // 배치된 타워 정리
            if (Singleton<Tower.TowerManager>.HasInstance)
                Tower.TowerManager.Instance.ClearAllTowers();

            // 배치 그리드 점유 초기화
            if (Singleton<Tower.PlacementGrid>.HasInstance)
                Tower.PlacementGrid.Instance.ClearOccupied();

            // 살아있는 노드 정리
            CleanupNodes();

            // InGameUI 숨기기 (런엔드 패널 포함)
            var inGameUI = Object.FindFirstObjectByType<UI.InGameUI>(FindObjectsInactive.Include);
            if (inGameUI != null)
                inGameUI.HideAll();

            var hubUI = Object.FindFirstObjectByType<UI.HubUI>(FindObjectsInactive.Include);
            if (hubUI != null)
                hubUI.Show();
        }

        private void CleanupNodes()
        {
            var nodes = Object.FindObjectsByType<Node.Node>(FindObjectsSortMode.None);
            foreach (var node in nodes)
            {
                if (node.IsAlive)
                    Tesseract.ObjectPool.Poolable.TryPool(node.gameObject);
            }
        }
    }
}
