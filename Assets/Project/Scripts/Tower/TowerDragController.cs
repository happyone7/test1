using Tesseract.Core;
using UnityEngine;
using Soulspire.Audio;

namespace Soulspire.Tower
{
    public class TowerDragController : MonoBehaviour
    {
        public enum DragState { None, Dragging }

        [Header("설정")]
        public LayerMask towerLayer;

        [Header("런타임")]
        public DragState State { get; private set; } = DragState.None;

        private int _dragInventoryIndex = -1;
        private InventorySlot _dragSlot;
        private GameObject _previewObject;
        private SpriteRenderer _highlightRenderer;
        private Camera _mainCamera;

        // 하이라이트 색상
        private static readonly Color ColorPlaceable = new Color(0f, 1f, 0.3f, 0.5f);   // 초록
        private static readonly Color ColorMergeable = new Color(1f, 0.84f, 0f, 0.5f);   // 금색
        private static readonly Color ColorBlocked = new Color(1f, 0f, 0f, 0.5f);         // 빨강

        void Start()
        {
            _mainCamera = Camera.main;
        }

        /// <summary>
        /// UI에서 호출: 인벤토리 슬롯 드래그 시작.
        /// </summary>
        public void StartDrag(int inventoryIndex)
        {
            if (!Singleton<TowerInventory>.HasInstance) return;
            if (Time.timeScale == 0f) return; // 일시정지 중 드래그 불가

            var slot = TowerInventory.Instance.GetSlot(inventoryIndex);
            if (slot == null || slot.data == null) return;

            _dragInventoryIndex = inventoryIndex;
            _dragSlot = slot;
            State = DragState.Dragging;

            // 프리뷰 오브젝트 생성 (반투명)
            CreatePreview(slot.data);

            // 하이라이트 오브젝트 생성
            CreateHighlight();
        }

        void Update()
        {
            if (State != DragState.Dragging) return;

            // 일시정지 되면 드래그 취소
            if (Time.timeScale == 0f)
            {
                EndDrag();
                return;
            }

            if (_mainCamera == null)
                _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            // 마우스 → 월드 좌표
            Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;

            // 그리드 스냅
            Vector3 snappedPos = mouseWorld;
            Vector3Int cellPos = Vector3Int.zero;
            bool hasGrid = Singleton<PlacementGrid>.HasInstance;

            if (hasGrid)
            {
                var grid = PlacementGrid.Instance;
                cellPos = grid.WorldToCell(mouseWorld);
                snappedPos = grid.CellToWorld(cellPos);
            }

            // 프리뷰 위치 업데이트
            if (_previewObject != null)
                _previewObject.transform.position = snappedPos;

            // 하이라이트 위치 및 색상 업데이트
            UpdateHighlight(snappedPos, cellPos, hasGrid);

            // 드롭 (마우스 버튼 놓기)
            if (Input.GetMouseButtonUp(0))
            {
                HandleDrop(snappedPos, cellPos, hasGrid);
            }
        }

        private void HandleDrop(Vector3 snappedPos, Vector3Int cellPos, bool hasGrid)
        {
            // 기존 타워 확인 (합성 가능 여부)
            Tower existingTower = GetTowerAt(snappedPos);

            if (existingTower != null && CanMerge(existingTower))
            {
                // 합성: 대상 타워 레벨업, 인벤토리에서 제거
                existingTower.LevelUp();
                TowerInventory.Instance.RemoveAt(_dragInventoryIndex);
                SoundManager.Instance.PlaySfx(SoundKeys.TowerMerge, 0.95f);
            }
            else if (existingTower == null)
            {
                // 빈 칸 배치 가능 여부 확인
                bool canPlace = !hasGrid || PlacementGrid.Instance.CanPlace(cellPos);

                if (canPlace && Singleton<TowerManager>.HasInstance)
                {
                    TowerManager.Instance.PlaceTower(_dragSlot.data, snappedPos, _dragSlot.level);

                    // fallback 그리드 점유 표시
                    if (hasGrid)
                        PlacementGrid.Instance.SetOccupied(cellPos, true);

                    TowerInventory.Instance.RemoveAt(_dragInventoryIndex);
                    SoundManager.Instance.PlaySfx(SoundKeys.TowerPlace, 0.9f);
                }
                // else: 배치 불가 → 아무것도 안 함 (원위치)
            }
            // else: 합성 불가 타워 위 → 아무것도 안 함

            EndDrag();
        }

        private void UpdateHighlight(Vector3 snappedPos, Vector3Int cellPos, bool hasGrid)
        {
            if (_highlightRenderer == null) return;

            _highlightRenderer.transform.position = snappedPos;

            Tower existingTower = GetTowerAt(snappedPos);

            if (existingTower != null && CanMerge(existingTower))
            {
                _highlightRenderer.color = ColorMergeable;
            }
            else if (existingTower == null)
            {
                bool canPlace = !hasGrid || PlacementGrid.Instance.CanPlace(cellPos);
                _highlightRenderer.color = canPlace ? ColorPlaceable : ColorBlocked;
            }
            else
            {
                _highlightRenderer.color = ColorBlocked;
            }
        }

        private bool CanMerge(Tower target)
        {
            if (_dragSlot == null || _dragSlot.data == null) return false;
            if (target.data == null) return false;
            // 같은 타입 + 같은 레벨 + 대상 레벨 < MaxLevel (TowerData 기반 동적 캡)
            return target.data.type == _dragSlot.data.type
                && target.Level == _dragSlot.level
                && target.Level < target.data.MaxLevel;
        }

        private Tower GetTowerAt(Vector3 worldPos)
        {
            var collider = Physics2D.OverlapPoint(worldPos, towerLayer);
            if (collider != null)
                return collider.GetComponent<Tower>();
            return null;
        }

        private void CreatePreview(Data.TowerData towerData)
        {
            if (towerData.prefab == null) return;

            _previewObject = Instantiate(towerData.prefab);
            _previewObject.name = "TowerPreview";

            // 비활성 컴포넌트들 (공격 등 방지)
            var tower = _previewObject.GetComponent<Tower>();
            if (tower != null) tower.enabled = false;

            // 콜라이더 비활성화 (배치 판정에 영향 방지)
            var colliders = _previewObject.GetComponentsInChildren<Collider2D>();
            foreach (var col in colliders)
                col.enabled = false;

            // 반투명 처리
            var renderers = _previewObject.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in renderers)
            {
                var c = sr.color;
                c.a = 0.5f;
                sr.color = c;
            }
        }

        private void CreateHighlight()
        {
            var go = new GameObject("DragHighlight");
            _highlightRenderer = go.AddComponent<SpriteRenderer>();

            // 1x1 흰색 스프라이트 생성
            var tex = new Texture2D(4, 4);
            var pixels = new Color[16];
            for (int i = 0; i < 16; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();

            float cellSize = Singleton<PlacementGrid>.HasInstance
                ? PlacementGrid.Instance.cellSize
                : 1f;

            _highlightRenderer.sprite = Sprite.Create(
                tex,
                new Rect(0, 0, 4, 4),
                new Vector2(0.5f, 0.5f),
                4f / cellSize
            );
            _highlightRenderer.sortingOrder = -1;
            _highlightRenderer.color = ColorPlaceable;
        }

        private void EndDrag()
        {
            State = DragState.None;
            _dragInventoryIndex = -1;
            _dragSlot = null;

            if (_previewObject != null)
            {
                Destroy(_previewObject);
                _previewObject = null;
            }

            if (_highlightRenderer != null)
            {
                Destroy(_highlightRenderer.gameObject);
                _highlightRenderer = null;
            }
        }
    }
}
