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

            // InGame UI 요소 숨기기
            var inGameUI = Object.FindFirstObjectByType<UI.InGameUI>(FindObjectsInactive.Include);
            if (inGameUI != null)
                inGameUI.HideAll();

            // Hub 숨기기 (타이틀 화면부터 시작)
            var hubUI = Object.FindFirstObjectByType<UI.HubUI>(FindObjectsInactive.Include);
            if (hubUI != null)
                hubUI.Hide();

            // 타이틀 화면 표시
            var titleUI = Object.FindFirstObjectByType<UI.TitleScreenUI>(FindObjectsInactive.Include);
            if (titleUI != null)
                titleUI.Show();

            SoundManager.Instance.PlayBgm(SoundKeys.BgmHub, 0f);
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

            // 런 상태 정리 (직접 재시도 시 RunEnd 상태에서 바로 오는 경우)
            if (Singleton<RunManager>.HasInstance && RunManager.Instance.IsRunning)
                RunManager.Instance.ResetRun();

            // 날아가는 투사체 정리
            CleanupProjectiles();

            // 살아있는 노드 정리
            CleanupNodes();

            // 인벤토리 초기화
            if (Singleton<Tower.TowerInventory>.HasInstance)
                Tower.TowerInventory.Instance.Clear();

            // 배치된 타워 정리 (이전 런에서 남은 것)
            if (Singleton<Tower.TowerManager>.HasInstance)
                Tower.TowerManager.Instance.ClearAllTowers();

            // 배치 그리드 점유 초기화
            if (Singleton<Tower.PlacementGrid>.HasInstance)
                Tower.PlacementGrid.Instance.ClearOccupied();

            // FTUE: Arrow 타워 3개를 필드에 자동 배치
            AutoPlaceStarterTowers(3);

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

            // 날아가는 투사체 정리
            CleanupProjectiles();

            // InGameUI 숨기기 (런엔드 패널 포함)
            var inGameUI = Object.FindFirstObjectByType<UI.InGameUI>(FindObjectsInactive.Include);
            if (inGameUI != null)
                inGameUI.HideAll();

            var hubUI = Object.FindFirstObjectByType<UI.HubUI>(FindObjectsInactive.Include);
            if (hubUI != null)
                hubUI.Show();
        }

        /// <summary>
        /// PlacementGrid의 빈 buildable 셀에 Arrow 타워를 자동 배치합니다.
        /// 인벤토리 드래그 시스템이 구현될 때까지의 임시 FTUE 로직입니다.
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

            // Arrow 타워 데이터 찾기
            Data.TowerData starterTower = null;
            if (towerManager.availableTowers != null)
            {
                foreach (var td in towerManager.availableTowers)
                {
                    if (td.type == Data.TowerType.Arrow)
                    {
                        starterTower = td;
                        break;
                    }
                }
                // Arrow 타워가 없으면 첫 번째 타워 사용
                if (starterTower == null && towerManager.availableTowers.Length > 0)
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

        private void CleanupProjectiles()
        {
            var projectiles = Object.FindObjectsByType<Projectile.Projectile>(FindObjectsSortMode.None);
            foreach (var proj in projectiles)
            {
                Tesseract.ObjectPool.Poolable.TryPool(proj.gameObject);
            }
        }
    }
}
