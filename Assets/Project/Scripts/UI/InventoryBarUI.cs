using System;
using Tesseract.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nodebreaker.UI
{
    /// <summary>
    /// InGame 화면 하단 8슬롯 타워 인벤토리 바 UI.
    /// 코드로 슬롯을 동적 생성하고, TowerInventory 이벤트에 반응하여 자동 갱신한다.
    /// 슬롯 클릭(PointerDown) 시 TowerDragController.StartDrag 를 호출한다.
    /// </summary>
    public class InventoryBarUI : MonoBehaviour
    {
        // === 색상 팔레트 (InGameUI와 통일) ===
        private static readonly Color ColorPanel = new Color32(0x12, 0x1A, 0x2A, 0xFF);
        private static readonly Color ColorBrightPanel = new Color32(0x1A, 0x24, 0x3A, 0xFF);
        private static readonly Color ColorNeonGreen = new Color32(0x2B, 0xFF, 0x88, 0xFF);
        private static readonly Color ColorNeonBlue = new Color32(0x37, 0xB6, 0xFF, 0xFF);
        private static readonly Color ColorTextMain = new Color32(0xD8, 0xE4, 0xFF, 0xFF);
        private static readonly Color ColorTextSub = new Color32(0xAF, 0xC3, 0xE8, 0xFF);
        private static readonly Color ColorBorder = new Color32(0x5B, 0x6B, 0x8A, 0xFF);
        private static readonly Color ColorPanelBg = new Color32(0x12, 0x1A, 0x2A, 0xDD); // 반투명 패널 배경

        private const int MAX_SLOTS = 8;
        private const float SLOT_SIZE = 72f;
        private const float SLOT_SPACING = 6f;
        private const float BAR_PADDING = 10f;
        private const float ICON_SIZE = 48f;
        private const int LEVEL_FONT_SIZE = 12;

        [SerializeField] private Tower.TowerDragController dragController;

        // 동적 생성된 슬롯 UI 요소들
        private Image[] _slotBackgrounds;
        private Image[] _slotIcons;
        private Text[] _slotLevelTexts;
        private Outline[] _slotOutlines;
        private GameObject[] _slotObjects;

        // 선택(하이라이트) 상태
        private int _highlightedSlot = -1;

        void Start()
        {
            CreateBar();

            if (Singleton<Tower.TowerInventory>.HasInstance)
                Tower.TowerInventory.Instance.OnInventoryChanged += RefreshSlots;

            // dragController가 지정되지 않았으면 씬에서 찾기
            if (dragController == null)
                dragController = FindAnyObjectByType<Tower.TowerDragController>();

            RefreshSlots();
        }

        void OnDestroy()
        {
            if (Singleton<Tower.TowerInventory>.HasInstance)
                Tower.TowerInventory.Instance.OnInventoryChanged -= RefreshSlots;
        }

        /// <summary>
        /// 인벤토리 바 전체 레이아웃을 코드로 생성한다.
        /// </summary>
        private void CreateBar()
        {
            _slotBackgrounds = new Image[MAX_SLOTS];
            _slotIcons = new Image[MAX_SLOTS];
            _slotLevelTexts = new Text[MAX_SLOTS];
            _slotOutlines = new Outline[MAX_SLOTS];
            _slotObjects = new GameObject[MAX_SLOTS];

            // === 바 배경 RectTransform 설정 (this 오브젝트) ===
            // Canvas 자식이므로 RectTransform이 이미 존재해야 함
            RectTransform barRect = GetComponent<RectTransform>();
            if (barRect == null)
            {
                Debug.LogError("[InventoryBarUI] RectTransform이 없습니다. Canvas 자식으로 배치해주세요.");
                return;
            }

            // 하단 중앙 앵커
            barRect.anchorMin = new Vector2(0.5f, 0f);
            barRect.anchorMax = new Vector2(0.5f, 0f);
            barRect.pivot = new Vector2(0.5f, 0f);

            float totalWidth = MAX_SLOTS * SLOT_SIZE + (MAX_SLOTS - 1) * SLOT_SPACING + BAR_PADDING * 2f;
            float totalHeight = SLOT_SIZE + BAR_PADDING * 2f;
            barRect.sizeDelta = new Vector2(totalWidth, totalHeight);
            barRect.anchoredPosition = new Vector2(0f, 8f); // 하단에서 약간 위

            // 바 배경 이미지
            Image barBg = gameObject.GetComponent<Image>();
            if (barBg == null)
                barBg = gameObject.AddComponent<Image>();
            barBg.color = ColorPanelBg;
            barBg.raycastTarget = true;

            // 바 테두리 (Outline)
            Outline barOutline = gameObject.GetComponent<Outline>();
            if (barOutline == null)
                barOutline = gameObject.AddComponent<Outline>();
            barOutline.effectColor = ColorBorder;
            barOutline.effectDistance = new Vector2(1f, 1f);

            // === HorizontalLayoutGroup ===
            HorizontalLayoutGroup layout = gameObject.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
                layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = SLOT_SPACING;
            layout.padding = new RectOffset(
                Mathf.RoundToInt(BAR_PADDING),
                Mathf.RoundToInt(BAR_PADDING),
                Mathf.RoundToInt(BAR_PADDING),
                Mathf.RoundToInt(BAR_PADDING)
            );
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            // === 8개 슬롯 생성 ===
            for (int i = 0; i < MAX_SLOTS; i++)
            {
                CreateSlot(i);
            }
        }

        /// <summary>
        /// 개별 슬롯 UI를 생성한다.
        /// </summary>
        private void CreateSlot(int index)
        {
            // --- Slot Root (Button 역할) ---
            GameObject slotGo = new GameObject($"Slot_{index}", typeof(RectTransform));
            slotGo.transform.SetParent(transform, false);

            RectTransform slotRect = slotGo.GetComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(SLOT_SIZE, SLOT_SIZE);

            // 배경 이미지
            Image bg = slotGo.AddComponent<Image>();
            bg.color = ColorBrightPanel;
            bg.raycastTarget = true;

            // 테두리
            Outline outline = slotGo.AddComponent<Outline>();
            outline.effectColor = ColorBorder;
            outline.effectDistance = new Vector2(1f, 1f);

            // PointerDown 이벤트 핸들러
            SlotPointerHandler handler = slotGo.AddComponent<SlotPointerHandler>();
            int capturedIndex = index;
            handler.onPointerDown = () => OnSlotPointerDown(capturedIndex);

            // --- 아이콘 이미지 ---
            GameObject iconGo = new GameObject("Icon", typeof(RectTransform));
            iconGo.transform.SetParent(slotGo.transform, false);

            RectTransform iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.55f);
            iconRect.anchorMax = new Vector2(0.5f, 0.55f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(ICON_SIZE, ICON_SIZE);
            iconRect.anchoredPosition = Vector2.zero;

            Image iconImg = iconGo.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            iconImg.enabled = false; // 빈 슬롯은 아이콘 숨김

            // --- 레벨 텍스트 ---
            GameObject levelGo = new GameObject("LevelText", typeof(RectTransform));
            levelGo.transform.SetParent(slotGo.transform, false);

            RectTransform levelRect = levelGo.GetComponent<RectTransform>();
            // 슬롯 하단에 배치
            levelRect.anchorMin = new Vector2(0f, 0f);
            levelRect.anchorMax = new Vector2(1f, 0f);
            levelRect.pivot = new Vector2(0.5f, 0f);
            levelRect.sizeDelta = new Vector2(0f, 18f);
            levelRect.anchoredPosition = new Vector2(0f, 2f);

            Text levelText = levelGo.AddComponent<Text>();
            levelText.text = "";
            levelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            levelText.fontSize = LEVEL_FONT_SIZE;
            levelText.alignment = TextAnchor.MiddleCenter;
            levelText.color = ColorTextSub;
            levelText.raycastTarget = false;

            // 참조 저장
            _slotObjects[index] = slotGo;
            _slotBackgrounds[index] = bg;
            _slotIcons[index] = iconImg;
            _slotLevelTexts[index] = levelText;
            _slotOutlines[index] = outline;
        }

        /// <summary>
        /// TowerInventory의 데이터로 모든 슬롯을 갱신한다.
        /// </summary>
        public void RefreshSlots()
        {
            bool hasInventory = Singleton<Tower.TowerInventory>.HasInstance;

            for (int i = 0; i < MAX_SLOTS; i++)
            {
                Tower.InventorySlot slot = null;
                if (hasInventory)
                    slot = Tower.TowerInventory.Instance.GetSlot(i);

                if (slot != null && slot.data != null)
                {
                    // 채워진 슬롯
                    _slotBackgrounds[i].color = ColorBrightPanel;

                    _slotIcons[i].enabled = true;
                    _slotIcons[i].sprite = slot.data.icon;

                    _slotLevelTexts[i].text = $"Lv.{slot.level}";
                    _slotLevelTexts[i].color = ColorTextSub;
                }
                else
                {
                    // 빈 슬롯
                    _slotBackgrounds[i].color = new Color32(0x10, 0x14, 0x22, 0xFF); // 더 어두운 색
                    _slotIcons[i].enabled = false;
                    _slotLevelTexts[i].text = "";
                }

                // 하이라이트 초기화
                _slotOutlines[i].effectColor = (i == _highlightedSlot) ? ColorNeonGreen : ColorBorder;
            }
        }

        /// <summary>
        /// 슬롯 PointerDown 처리: 드래그 시작 + 하이라이트.
        /// </summary>
        private void OnSlotPointerDown(int index)
        {
            if (!Singleton<Tower.TowerInventory>.HasInstance) return;

            var slot = Tower.TowerInventory.Instance.GetSlot(index);
            if (slot == null || slot.data == null) return;

            // 하이라이트 갱신
            SetHighlight(index);

            // 드래그 시작
            if (dragController != null)
                dragController.StartDrag(index);
        }

        /// <summary>
        /// 슬롯 하이라이트를 설정한다.
        /// </summary>
        public void SetHighlight(int index)
        {
            // 이전 하이라이트 해제
            if (_highlightedSlot >= 0 && _highlightedSlot < MAX_SLOTS)
                _slotOutlines[_highlightedSlot].effectColor = ColorBorder;

            _highlightedSlot = index;

            // 새 하이라이트 적용
            if (index >= 0 && index < MAX_SLOTS)
                _slotOutlines[index].effectColor = ColorNeonGreen;
        }

        /// <summary>
        /// 하이라이트를 해제한다.
        /// </summary>
        public void ClearHighlight()
        {
            SetHighlight(-1);
        }

        void Update()
        {
            // 드래그 종료 시 하이라이트 자동 해제
            if (_highlightedSlot >= 0 && dragController != null
                && dragController.State == Tower.TowerDragController.DragState.None)
            {
                ClearHighlight();
            }
        }
    }

    /// <summary>
    /// 슬롯에 부착되어 PointerDown 이벤트를 전달하는 헬퍼.
    /// EventTrigger보다 가벼운 구현.
    /// </summary>
    public class SlotPointerHandler : MonoBehaviour, IPointerDownHandler
    {
        public Action onPointerDown;

        public void OnPointerDown(PointerEventData eventData)
        {
            onPointerDown?.Invoke();
        }
    }
}
