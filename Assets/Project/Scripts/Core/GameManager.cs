using Tesseract.Core;
using UnityEngine;
using Soulspire.Audio;

namespace Soulspire.Core
{
    public enum GameState
    {
        Title,
        Hub,
        InGame,
        RunEnd
    }

    public class GameManager : Singleton<GameManager>
    {
        [Header("스테이지 데이터")]
        public Data.StageData[] stages;

        [Header("초기 타워 설정")]
        [Tooltip("런 시작 시 자동 배치할 기본 타워. null이면 TowerManager의 첫 번째 타워 사용.")]
        public Data.TowerData starterTowerData;

        [Tooltip("런 시작 시 자동 배치할 타워 수 (Early Game: 1기)")]
        public int starterTowerCount = 1;

        [Header("현재 상태")]
        public GameState State { get; private set; } = GameState.Title;

        // MetaManager 위임 프로퍼티
        public int TotalSoul => MetaManager.Instance.TotalSoul;
        public int TotalCoreFragment => MetaManager.Instance.TotalCoreFragment;
        public int CurrentStageIndex => MetaManager.Instance.CurrentStageIndex;

        // UI 캐시 참조 (FindFirstObjectByType 반복 호출 방지)
        private UI.InGameUI _cachedInGameUI;
        private UI.HubUI _cachedHubUI;
        private UI.TitleScreenUI _cachedTitleUI;

        protected override void Awake()
        {
            base.Awake();
        }

        void Start()
        {
            State = GameState.Title;

            // UI 참조 캐싱
            _cachedInGameUI = Object.FindFirstObjectByType<UI.InGameUI>(FindObjectsInactive.Include);
            _cachedHubUI = Object.FindFirstObjectByType<UI.HubUI>(FindObjectsInactive.Include);
            _cachedTitleUI = Object.FindFirstObjectByType<UI.TitleScreenUI>(FindObjectsInactive.Include);

            Debug.Log($"[GameManager] Start: inGameUI={_cachedInGameUI != null}, hubUI={_cachedHubUI != null}, titleUI={_cachedTitleUI != null}");

            // InGame UI 요소 숨기기
            if (_cachedInGameUI != null)
                _cachedInGameUI.HideAll();

            // Hub 숨기기 (타이틀 화면부터 시작)
            if (_cachedHubUI != null)
                _cachedHubUI.Hide();

            // 타이틀 화면은 모든 사용자에게 항상 표시
            if (_cachedTitleUI != null)
                _cachedTitleUI.Show();

            SoundManager.Instance.PlayBgm(SoundKeys.BgmHub, 0f);
        }

        /// <summary>
        /// 타이틀 화면에서 Play 버튼 클릭 후 호출.
        /// FTUE(첫 플레이)이면 Hub 스킵하고 Stage 1 직행,
        /// 기존 사용자이면 Hub(스테이지 선택) 진입.
        /// </summary>
        public void OnTitlePlayClicked()
        {
            bool isFirst = MetaManager.Instance.IsFirstPlay;
            Debug.Log($"[GameManager] OnTitlePlayClicked: IsFirstPlay={isFirst}");

            if (isFirst)
            {
                Debug.Log("[GameManager] FTUE: 첫 플레이 감지 - Hub 스킵, Stage 1 바로 진입");
                MetaManager.Instance.SetFtueFlag(0, true); // 첫 플레이 완료 플래그
                StartRun(0);
            }
            else
            {
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
            Debug.Log($"[GameManager] StartRun: stageIdx={stageIdx}, State=InGame");
            SoundManager.Instance.PlayBgm(SoundKeys.BgmCombat, 0.5f);

            // 캐시 참조 유효성 검사 (DontDestroyOnLoad 이후 씬 오브젝트 참조 손실 대비)
            if (_cachedHubUI == null)
                _cachedHubUI = Object.FindFirstObjectByType<UI.HubUI>(FindObjectsInactive.Include);
            if (_cachedHubUI != null)
                _cachedHubUI.Hide();

            // 인벤토리 초기화
            if (Singleton<Tower.TowerInventory>.HasInstance)
                Tower.TowerInventory.Instance.Clear();

            // 배치된 타워 정리 (이전 런에서 남은 것)
            if (Singleton<Tower.TowerManager>.HasInstance)
                Tower.TowerManager.Instance.ClearAllTowers();

            // 배치 그리드 점유 초기화
            if (Singleton<Tower.PlacementGrid>.HasInstance)
                Tower.PlacementGrid.Instance.ClearOccupied();

            // 초기 타워 자동 배치 (Early Game Flow: 타워 1기로 시작)
            AutoPlaceStarterTowers(starterTowerCount);

            // InGameUI 활성화 (Hub에서 비활성화되었을 수 있음)
            if (_cachedInGameUI == null)
                _cachedInGameUI = Object.FindFirstObjectByType<UI.InGameUI>(FindObjectsInactive.Include);

            if (_cachedInGameUI != null)
            {
                _cachedInGameUI.ShowAll();
                Debug.Log("[GameManager] StartRun: InGameUI.ShowAll() 호출 완료");
            }
            else
            {
                Debug.LogError("[GameManager] StartRun: InGameUI를 찾을 수 없음!");
            }

            RunModifiers mods = MetaManager.Instance.CalculateModifiers();
            RunManager.Instance.StartRun(stages[stageIdx], mods);
        }

        public void OnRunEnd(bool cleared, int soulEarned)
        {
            int stageIdx = MetaManager.Instance.CurrentStageIndex;
            int coreFragmentEarned = cleared ? stages[Mathf.Min(stageIdx, stages.Length - 1)].coreFragmentReward : 0;

            // RunManager에서 노드 처치 수를 가져옴
            int nodesKilled = 0;
            if (Singleton<RunManager>.HasInstance)
                nodesKilled = RunManager.Instance.NodesKilled;

            MetaManager.Instance.AddRunRewards(soulEarned, coreFragmentEarned, cleared, stageIdx, nodesKilled);

            // FTUE: 첫 스테이지 클리어 시 보너스 Soul 100 지급
            if (cleared && !MetaManager.Instance.GetFtueFlag(3))
            {
                MetaManager.Instance.SetFtueFlag(3, true); // 첫 스테이지 클리어 플래그
                MetaManager.Instance.AddRunRewards(100, 0, false, stageIdx, 0); // 보너스 Soul 100
                Debug.Log("[GameManager] FTUE: 첫 클리어 보너스 Soul 100 지급!");
            }

            State = GameState.RunEnd;
        }

        public void GoToHub()
        {
            State = GameState.Hub;
            Debug.Log("[GameManager] GoToHub: State=Hub");
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
            if (_cachedInGameUI == null)
                _cachedInGameUI = Object.FindFirstObjectByType<UI.InGameUI>(FindObjectsInactive.Include);
            if (_cachedInGameUI != null)
                _cachedInGameUI.HideAll();

            if (_cachedHubUI == null)
                _cachedHubUI = Object.FindFirstObjectByType<UI.HubUI>(FindObjectsInactive.Include);
            if (_cachedHubUI != null)
            {
                _cachedHubUI.Show();

                // FTUE: Hub 첫 진입 가이드
                if (Singleton<UI.FTUEManager>.HasInstance)
                    UI.FTUEManager.Instance.TriggerHub("HubFirstEntry", UI.FTUEManager.GuideHubFirstEntry);
            }
        }

        /// <summary>
        /// PlacementGrid의 빈 buildable 셀에 기본 타워를 자동 배치합니다.
        /// starterTowerData가 설정되어 있으면 해당 타워를, 아니면 TowerManager의 첫 번째 타워를 사용합니다.
        /// </summary>
        private void AutoPlaceStarterTowers(int count)
        {
            if (!Singleton<Tower.TowerManager>.HasInstance || !Singleton<Tower.PlacementGrid>.HasInstance)
            {
                Debug.LogWarning("[GameManager] AutoPlaceStarterTowers: TowerManager 또는 PlacementGrid 인스턴스 없음");
                return;
            }

            var towerManager = Tower.TowerManager.Instance;
            var grid = Tower.PlacementGrid.Instance;

            // 기본 타워 데이터 결정: SO에서 지정된 starterTowerData 우선, 없으면 fallback
            Data.TowerData starterTower = starterTowerData;
            if (starterTower == null && towerManager.availableTowers != null && towerManager.availableTowers.Length > 0)
            {
                starterTower = towerManager.availableTowers[0];
            }

            if (starterTower == null)
            {
                Debug.LogWarning("[GameManager] AutoPlaceStarterTowers: 배치할 타워 데이터가 없음");
                return;
            }

            int placed = 0;

            // Tilemap 기반인 경우: BoundsInt를 순회하여 buildable 셀 탐색
            if (grid.placementTilemap != null)
            {
                var bounds = grid.placementTilemap.cellBounds;
                for (int x = bounds.xMin; x < bounds.xMax && placed < count; x++)
                {
                    for (int y = bounds.yMin; y < bounds.yMax && placed < count; y++)
                    {
                        var cellPos = new Vector3Int(x, y, 0);
                        if (grid.CanPlace(cellPos))
                        {
                            Vector3 worldPos = grid.CellToWorld(cellPos);
                            towerManager.PlaceTower(starterTower, worldPos, 1);
                            placed++;
                            Debug.Log($"[GameManager] 타워 자동 배치: {starterTower.towerName} at cell({x},{y}) world{worldPos}");
                        }
                    }
                }
            }
            else
            {
                // Fallback 그리드 기반
                for (int x = 0; x < grid.gridSize.x && placed < count; x++)
                {
                    for (int y = 0; y < grid.gridSize.y && placed < count; y++)
                    {
                        var cellPos = new Vector3Int(x, y, 0);
                        if (grid.CanPlace(cellPos))
                        {
                            Vector3 worldPos = grid.CellToWorld(cellPos);
                            towerManager.PlaceTower(starterTower, worldPos, 1);
                            grid.SetOccupied(cellPos, true);
                            placed++;
                            Debug.Log($"[GameManager] 타워 자동 배치: {starterTower.towerName} at cell({x},{y}) world{worldPos}");
                        }
                    }
                }
            }

            Debug.Log($"[GameManager] 타워 자동 배치 완료: {placed}/{count}개 배치됨");
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
