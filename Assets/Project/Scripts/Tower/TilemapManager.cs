using Tesseract.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Soulspire.Tower
{
    /// <summary>
    /// 4-layer Tilemap 시스템을 관리합니다.
    /// Background, Path, Placement, Wall 레이어 참조 및 판정 로직을 제공합니다.
    /// </summary>
    public class TilemapManager : Singleton<TilemapManager>
    {
        [Header("Tilemap 레이어")]
        [Tooltip("배경 타일 (Sorting Layer: Tilemap, Order 0)")]
        public Tilemap backgroundTilemap;

        [Tooltip("경로 표시 타일 (Sorting Layer: Tilemap, Order 1)")]
        public Tilemap pathTilemap;

        [Tooltip("배치 가능 칸 타일 (Sorting Layer: Tilemap, Order 2)")]
        public Tilemap placementTilemap;

        [Tooltip("벽/장애물 타일 (Sorting Layer: Tilemap, Order 3)")]
        public Tilemap wallTilemap;

        [Header("타워 감지")]
        public LayerMask towerLayer;

        /// <summary>
        /// 해당 월드 좌표에 타워를 배치할 수 있는지 확인합니다.
        /// Placement Tilemap에 타일이 존재하고, 해당 위치에 타워가 없어야 합니다.
        /// </summary>
        public bool CanPlaceTower(Vector3 worldPos)
        {
            if (placementTilemap == null) return false;

            Vector3Int cellPos = placementTilemap.WorldToCell(worldPos);

            // 1. 배치 가능 타일이 있는가?
            if (!placementTilemap.HasTile(cellPos))
                return false;

            // 2. 이미 타워가 있는가?
            Vector3 cellCenter = SnapToGrid(worldPos);
            var collider = Physics2D.OverlapPoint(cellCenter, towerLayer);
            if (collider != null)
                return false;

            return true;
        }

        /// <summary>
        /// 월드 좌표를 그리드 셀 중심으로 스냅합니다.
        /// </summary>
        public Vector3 SnapToGrid(Vector3 worldPos)
        {
            if (placementTilemap == null)
                return worldPos;

            Grid grid = placementTilemap.layoutGrid;
            if (grid == null)
                return worldPos;

            Vector3Int cellPos = grid.WorldToCell(worldPos);
            return grid.GetCellCenterWorld(cellPos);
        }

        /// <summary>
        /// 해당 월드 좌표가 경로 타일인지 확인합니다.
        /// </summary>
        public bool IsPathTile(Vector3 worldPos)
        {
            if (pathTilemap == null) return false;

            Vector3Int cellPos = pathTilemap.WorldToCell(worldPos);
            return pathTilemap.HasTile(cellPos);
        }

        /// <summary>
        /// 해당 월드 좌표가 벽/장애물 타일인지 확인합니다.
        /// </summary>
        public bool IsWallTile(Vector3 worldPos)
        {
            if (wallTilemap == null) return false;

            Vector3Int cellPos = wallTilemap.WorldToCell(worldPos);
            return wallTilemap.HasTile(cellPos);
        }

        /// <summary>
        /// 월드 좌표를 셀 좌표로 변환합니다.
        /// </summary>
        public Vector3Int WorldToCell(Vector3 worldPos)
        {
            if (placementTilemap == null)
                return Vector3Int.zero;

            return placementTilemap.WorldToCell(worldPos);
        }

        /// <summary>
        /// 셀 좌표를 월드 좌표(셀 중심)로 변환합니다.
        /// </summary>
        public Vector3 CellToWorld(Vector3Int cellPos)
        {
            if (placementTilemap == null)
                return Vector3.zero;

            Grid grid = placementTilemap.layoutGrid;
            if (grid == null)
                return placementTilemap.CellToWorld(cellPos);

            return grid.GetCellCenterWorld(cellPos);
        }
    }
}
