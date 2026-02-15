using Tesseract.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Nodebreaker.UI
{
    /// <summary>
    /// UI-6: 타워 드래그 시 Canvas 위에 고스트 이미지를 표시하고,
    /// 배치 가능/합성 가능/불가 상태를 하이라이트로 표시한다.
    /// 기존 TowerDragController와 연동하여 동작한다.
    /// </summary>
    public class TowerDragGhostUI : MonoBehaviour
    {
        // === 다크 판타지 색상 ===
        private static readonly Color ColorGhostNormal    = new Color(1f, 1f, 1f, 0.6f);
        private static readonly Color ColorGhostPlaceable = new Color(0.25f, 0.83f, 0.44f, 0.7f);   // 초록 (배치 가능)
        private static readonly Color ColorGhostMergeable = new Color(1f, 0.85f, 0.3f, 0.7f);       // 골드 (합성 가능)
        private static readonly Color ColorGhostBlocked   = new Color(0.83f, 0.25f, 0.25f, 0.5f);   // 빨강 (불가)
        private static readonly Color ColorHighlightPlace = new Color32(0x40, 0xD4, 0x70, 0x44);     // 에메랄드 반투명
        private static readonly Color ColorHighlightMerge = new Color32(0xFF, 0xD8, 0x4D, 0x44);     // 골드 반투명
        private static readonly Color ColorHighlightBlock = new Color32(0xD4, 0x40, 0x40, 0x33);     // 루비 반투명

        private const float GHOST_SIZE = 64f;
        private const float GHOST_PULSE_SPEED = 3f;
        private const float GHOST_PULSE_AMOUNT = 0.08f;

        [Header("참조")]
        [SerializeField] private Tower.TowerDragController dragController;
        [SerializeField] private Canvas parentCanvas;

        // 런타임 생성 요소
        private GameObject _ghostObj;
        private Image _ghostImage;
        private RectTransform _ghostRect;
        private CanvasGroup _ghostCG;

        // 상태 텍스트 (배치/합성/불가 표시)
        private GameObject _statusTextObj;
        private Text _statusText;
        private RectTransform _statusRect;

        private Camera _mainCamera;
        private bool _isDragging;
        private float _pulseTimer;

        void Start()
        {
            if (dragController == null)
                dragController = FindAnyObjectByType<Tower.TowerDragController>();
            if (parentCanvas == null)
                parentCanvas = GetComponentInParent<Canvas>();

            _mainCamera = Camera.main;
            CreateGhostUI();
        }

        void Update()
        {
            if (dragController == null) return;

            bool wasDragging = _isDragging;
            _isDragging = dragController.State == Tower.TowerDragController.DragState.Dragging;

            // 드래그 시작
            if (_isDragging && !wasDragging)
                OnDragStart();

            // 드래그 종료
            if (!_isDragging && wasDragging)
                OnDragEnd();

            // 드래그 중
            if (_isDragging)
                UpdateGhost();
        }

        private void OnDragStart()
        {
            if (_ghostObj == null) return;
            _ghostObj.SetActive(true);

            if (_statusTextObj != null)
                _statusTextObj.SetActive(true);

            // 현재 드래그 중인 슬롯의 아이콘 설정
            if (Singleton<Tower.TowerInventory>.HasInstance)
            {
                // dragController에서 직접 인덱스를 가져올 수 없으므로,
                // 가장 최근 하이라이트된 슬롯의 아이콘 사용
                var inventoryBar = FindAnyObjectByType<InventoryBarUI>();
                // InventoryBarUI에서 하이라이트 인덱스를 접근할 수 없으므로
                // 간단히 모든 슬롯을 순회하여 아이콘이 있는 슬롯 찾기
                // (향후 dragController에 CurrentSlot 프로퍼티 추가 권장)
            }

            _pulseTimer = 0f;
        }

        private void OnDragEnd()
        {
            if (_ghostObj != null)
                _ghostObj.SetActive(false);
            if (_statusTextObj != null)
                _statusTextObj.SetActive(false);
        }

        private void UpdateGhost()
        {
            if (_ghostRect == null || parentCanvas == null) return;

            if (_mainCamera == null)
                _mainCamera = Camera.main;

            // 마우스 위치를 Canvas 로컬 좌표로 변환
            Vector2 screenPos = Input.mousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                screenPos,
                parentCanvas.worldCamera,
                out Vector2 localPos
            );

            _ghostRect.anchoredPosition = localPos + new Vector2(0, GHOST_SIZE * 0.6f);

            // 펄스 효과
            _pulseTimer += Time.unscaledDeltaTime * GHOST_PULSE_SPEED;
            float pulse = 1f + Mathf.Sin(_pulseTimer) * GHOST_PULSE_AMOUNT;
            _ghostRect.localScale = Vector3.one * pulse;

            // 배치 상태에 따른 색상 변경
            UpdateGhostColor();

            // 상태 텍스트 위치 (고스트 아래)
            if (_statusRect != null)
                _statusRect.anchoredPosition = localPos + new Vector2(0, -GHOST_SIZE * 0.3f);
        }

        private void UpdateGhostColor()
        {
            if (_mainCamera == null || _ghostImage == null) return;

            Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;

            bool hasGrid = Singleton<Tower.PlacementGrid>.HasInstance;
            Vector3Int cellPos = Vector3Int.zero;

            if (hasGrid)
            {
                cellPos = Tower.PlacementGrid.Instance.WorldToCell(mouseWorld);
                var snappedPos = Tower.PlacementGrid.Instance.CellToWorld(cellPos);

                // 기존 타워 확인
                var collider = Physics2D.OverlapPoint(snappedPos);
                var existingTower = collider != null ? collider.GetComponent<Tower.Tower>() : null;

                if (existingTower != null)
                {
                    // 합성 가능 여부는 간략하게 판단 (같은 위치에 타워 있음)
                    _ghostImage.color = ColorGhostMergeable;
                    SetStatusText("합성", ColorGhostMergeable);
                }
                else if (Tower.PlacementGrid.Instance.CanPlace(cellPos))
                {
                    _ghostImage.color = ColorGhostPlaceable;
                    SetStatusText("배치", ColorGhostPlaceable);
                }
                else
                {
                    _ghostImage.color = ColorGhostBlocked;
                    SetStatusText("불가", ColorGhostBlocked);
                }
            }
            else
            {
                _ghostImage.color = ColorGhostNormal;
                SetStatusText("", Color.clear);
            }
        }

        private void SetStatusText(string text, Color color)
        {
            if (_statusText == null) return;
            _statusText.text = text;
            _statusText.color = color;
        }

        private void CreateGhostUI()
        {
            // === 고스트 이미지 ===
            _ghostObj = new GameObject("DragGhost", typeof(RectTransform));
            _ghostObj.transform.SetParent(transform, false);
            _ghostRect = _ghostObj.GetComponent<RectTransform>();
            _ghostRect.sizeDelta = new Vector2(GHOST_SIZE, GHOST_SIZE);

            _ghostImage = _ghostObj.AddComponent<Image>();
            _ghostImage.color = ColorGhostNormal;
            _ghostImage.preserveAspect = true;
            _ghostImage.raycastTarget = false;

            _ghostCG = _ghostObj.AddComponent<CanvasGroup>();
            _ghostCG.blocksRaycasts = false;
            _ghostCG.interactable = false;

            _ghostObj.SetActive(false);

            // === 상태 텍스트 ===
            _statusTextObj = new GameObject("DragStatusText", typeof(RectTransform));
            _statusTextObj.transform.SetParent(transform, false);
            _statusRect = _statusTextObj.GetComponent<RectTransform>();
            _statusRect.sizeDelta = new Vector2(80, 20);

            _statusText = _statusTextObj.AddComponent<Text>();
            _statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _statusText.fontSize = 11;
            _statusText.fontStyle = FontStyle.Bold;
            _statusText.alignment = TextAnchor.MiddleCenter;
            _statusText.color = Color.clear;
            _statusText.raycastTarget = false;

            _statusTextObj.SetActive(false);
        }

        /// <summary>
        /// 고스트 이미지의 스프라이트를 설정한다.
        /// InventoryBarUI에서 드래그 시작 시 호출할 수 있다.
        /// </summary>
        public void SetGhostSprite(Sprite sprite)
        {
            if (_ghostImage != null && sprite != null)
            {
                _ghostImage.sprite = sprite;
                _ghostImage.color = ColorGhostNormal;
            }
        }
    }
}
