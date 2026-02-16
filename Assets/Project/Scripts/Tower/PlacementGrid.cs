using Tesseract.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Soulspire.Tower
{
    public class PlacementGrid : Singleton<PlacementGrid>
    {
        [Header("Tilemap 기반 (선택)")]
        [Tooltip("배치 가능 영역을 나타내는 Tilemap. 없으면 fallback 그리드 사용.")]
        public Tilemap placementTilemap;

        [Header("Fallback 심플 그리드")]
        public Vector3 gridOrigin = Vector3.zero;
        public Vector2Int gridSize = new Vector2Int(10, 10);
        public float cellSize = 1f;

        [Header("레이어")]
        public LayerMask towerLayer;

        private bool[,] _occupiedCells;

        protected override void Awake()
        {
            base.Awake();
            _occupiedCells = new bool[gridSize.x, gridSize.y];

            // TilemapManager가 있으면 Placement Tilemap 자동 연결
            if (placementTilemap == null && Singleton<TilemapManager>.HasInstance)
            {
                placementTilemap = TilemapManager.Instance.placementTilemap;
            }
        }

        /// <summary>
        /// 해당 셀에 타워를 배치할 수 있는지 확인합니다.
        /// </summary>
        public bool CanPlace(Vector3Int cellPos)
        {
            if (placementTilemap != null)
            {
                // Tilemap 기반: 타일이 존재하고 + 해당 위치에 타워가 없어야 함
                if (!placementTilemap.HasTile(cellPos))
                    return false;

                Vector3 worldPos = CellToWorld(cellPos);
                return !HasTowerAt(worldPos);
            }
            else
            {
                // Fallback 그리드 기반
                int x = cellPos.x;
                int y = cellPos.y;

                if (x < 0 || x >= gridSize.x || y < 0 || y >= gridSize.y)
                    return false;

                if (_occupiedCells[x, y])
                    return false;

                Vector3 worldPos = CellToWorld(cellPos);
                return !HasTowerAt(worldPos);
            }
        }

        /// <summary>
        /// 월드 좌표를 가장 가까운 셀 중심으로 스냅합니다.
        /// </summary>
        public Vector3 SnapToGrid(Vector3 worldPos)
        {
            Vector3Int cell = WorldToCell(worldPos);
            return CellToWorld(cell);
        }

        /// <summary>
        /// 월드 좌표를 셀 좌표로 변환합니다.
        /// </summary>
        public Vector3Int WorldToCell(Vector3 worldPos)
        {
            if (placementTilemap != null)
            {
                return placementTilemap.WorldToCell(worldPos);
            }
            else
            {
                int x = Mathf.FloorToInt((worldPos.x - gridOrigin.x) / cellSize);
                int y = Mathf.FloorToInt((worldPos.y - gridOrigin.y) / cellSize);
                return new Vector3Int(x, y, 0);
            }
        }

        /// <summary>
        /// 셀 좌표를 월드 좌표(셀 중심)로 변환합니다.
        /// </summary>
        public Vector3 CellToWorld(Vector3Int cellPos)
        {
            if (placementTilemap != null)
            {
                return placementTilemap.GetCellCenterWorld(cellPos);
            }
            else
            {
                float x = gridOrigin.x + cellPos.x * cellSize + cellSize * 0.5f;
                float y = gridOrigin.y + cellPos.y * cellSize + cellSize * 0.5f;
                return new Vector3(x, y, 0f);
            }
        }

        /// <summary>
        /// 점유 상태를 설정합니다 (fallback 그리드 전용).
        /// </summary>
        public void SetOccupied(Vector3Int cellPos, bool occupied)
        {
            if (placementTilemap != null) return; // Tilemap 사용 시 Physics로 판정

            int x = cellPos.x;
            int y = cellPos.y;
            if (x >= 0 && x < gridSize.x && y >= 0 && y < gridSize.y)
                _occupiedCells[x, y] = occupied;
        }

        /// <summary>
        /// 점유 상태를 초기화합니다.
        /// </summary>
        public void ClearOccupied()
        {
            _occupiedCells = new bool[gridSize.x, gridSize.y];
        }

        /// <summary>
        /// 해당 월드 좌표에 타워가 있는지 Physics2D로 확인합니다.
        /// </summary>
        private bool HasTowerAt(Vector3 worldPos)
        {
            var collider = Physics2D.OverlapPoint(worldPos, towerLayer);
            return collider != null;
        }

        void OnDrawGizmosSelected()
        {
            if (placementTilemap != null) return;

            Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Vector3 center = new Vector3(
                        gridOrigin.x + x * cellSize + cellSize * 0.5f,
                        gridOrigin.y + y * cellSize + cellSize * 0.5f,
                        0f
                    );
                    Gizmos.DrawWireCube(center, new Vector3(cellSize * 0.9f, cellSize * 0.9f, 0f));
                }
            }
        }
    }
}
