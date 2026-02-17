using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Soulspire.Editor
{
    public class TilemapLayerSetup
    {
        [MenuItem("Tools/Soulspire/Setup Tilemap Layers")]
        static void SetupTilemapLayers()
        {
            // Grid 오브젝트 생성 또는 찾기
            var gridObj = GameObject.Find("TilemapGrid");
            if (gridObj == null)
            {
                gridObj = new GameObject("TilemapGrid");
                var grid = gridObj.AddComponent<Grid>();
                grid.cellSize = new Vector3(1f, 1f, 0f);
                grid.cellLayout = GridLayout.CellLayout.Rectangle;
            }
            else if (gridObj.GetComponent<Grid>() == null)
            {
                var grid = gridObj.AddComponent<Grid>();
                grid.cellSize = new Vector3(1f, 1f, 0f);
                grid.cellLayout = GridLayout.CellLayout.Rectangle;
            }

            // 4개 Tilemap 레이어 생성
            var bgTilemap = CreateTilemapLayer(gridObj.transform, "Tilemap_Background", "Tilemap", 0);
            var pathTilemap = CreateTilemapLayer(gridObj.transform, "Tilemap_Path", "Tilemap", 1);
            var placementTilemap = CreateTilemapLayer(gridObj.transform, "Tilemap_Placement", "Tilemap", 2);
            var wallTilemap = CreateTilemapLayer(gridObj.transform, "Tilemap_Wall", "Tilemap", 3);

            // TilemapManager 싱글톤 생성 또는 찾기
            var manager = Object.FindFirstObjectByType<Tower.TilemapManager>();
            if (manager == null)
            {
                var managerObj = new GameObject("TilemapManager");
                manager = managerObj.AddComponent<Tower.TilemapManager>();
            }

            // TilemapManager에 레이어 참조 연결
            manager.backgroundTilemap = bgTilemap;
            manager.pathTilemap = pathTilemap;
            manager.placementTilemap = placementTilemap;
            manager.wallTilemap = wallTilemap;

            // Tower 레이어 마스크 설정 (Layer 8 = Tower)
            manager.towerLayer = 1 << 8;

            // PlacementGrid에도 placementTilemap 연결 (기존 시스템 호환)
            var placementGrid = Object.FindFirstObjectByType<Tower.PlacementGrid>();
            if (placementGrid != null)
            {
                placementGrid.placementTilemap = placementTilemap;
                EditorUtility.SetDirty(placementGrid);
            }

            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(gridObj);

            Debug.Log("[Soulspire] Tilemap 4-layer 구조 설정 완료!\n" +
                      "  - Tilemap_Background (Order 0)\n" +
                      "  - Tilemap_Path (Order 1)\n" +
                      "  - Tilemap_Placement (Order 2)\n" +
                      "  - Tilemap_Wall (Order 3)\n" +
                      "  - TilemapManager에 참조 연결 완료");
        }

        static Tilemap CreateTilemapLayer(Transform parent, string name, string sortingLayerName, int sortingOrder)
        {
            // 기존 오브젝트 찾기
            var existing = parent.Find(name);
            GameObject obj;

            if (existing != null)
            {
                obj = existing.gameObject;
            }
            else
            {
                obj = new GameObject(name);
                obj.transform.SetParent(parent);
                obj.transform.localPosition = Vector3.zero;
            }

            // Tilemap 컴포넌트 추가
            var tilemap = obj.GetComponent<Tilemap>();
            if (tilemap == null)
                tilemap = obj.AddComponent<Tilemap>();

            // TilemapRenderer 컴포넌트 추가 및 Sorting 설정
            var renderer = obj.GetComponent<TilemapRenderer>();
            if (renderer == null)
                renderer = obj.AddComponent<TilemapRenderer>();

            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = sortingOrder;

            EditorUtility.SetDirty(obj);

            return tilemap;
        }
    }
}
