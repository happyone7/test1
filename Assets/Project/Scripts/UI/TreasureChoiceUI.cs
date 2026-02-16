using System;
using System.Collections;
using System.Collections.Generic;
using Tesseract.Core;
using UnityEngine;
using UnityEngine.UI;
using Soulspire.Audio;
using Soulspire.Data;

namespace Soulspire.UI
{
    /// <summary>
    /// 보물상자 3택 선택 패널.
    /// TreasureManager.OnTreasureDropped 이벤트를 구독하여
    /// 풀스크린 오버레이 위에 카드 3장을 표시한다.
    /// 선택 시 TreasureManager.ApplyChoice()를 호출하고 패널을 닫는다.
    /// Time.timeScale = 0으로 게임을 일시정지시킨다.
    /// </summary>
    public class TreasureChoiceUI : MonoBehaviour
    {
        // === 등급별 색상 ===
        private static readonly Color ColorCommon = new Color32(0xA0, 0x98, 0x90, 0xFF); // 회색 (TextSub)
        private static readonly Color ColorRare = new Color32(0x40, 0x80, 0xD4, 0xFF);   // 파란 (SapphireBlue)
        private static readonly Color ColorEpic = new Color32(0x90, 0x60, 0xD0, 0xFF);   // 보라 (MagicPurple)
        // === 타워 타입별 색상 ===
        private static readonly Color ColorTowerArrow = new Color32(0xC0, 0xA8, 0x70, 0xFF);
        private static readonly Color ColorTowerCannon = new Color32(0xE0, 0x80, 0x30, 0xFF);
        private static readonly Color ColorTowerIce = new Color32(0x40, 0x80, 0xD4, 0xFF);
        private static readonly Color ColorTowerLightning = new Color32(0xE0, 0xD0, 0x40, 0xFF);
        private static readonly Color ColorTowerLaser = new Color32(0x90, 0x60, 0xD0, 0xFF);
        private static readonly Color ColorTowerVoid = new Color32(0x60, 0x40, 0xA0, 0xFF);


        // === UI 색상 (InGameUI 팔레트 기준) ===
        private static readonly Color ColorPanel = new Color32(0x1A, 0x18, 0x28, 0xFF);        // UI 패널
        private static readonly Color ColorBrightPanel = new Color32(0x24, 0x22, 0x36, 0xFF);   // UI 밝은 패널
        private static readonly Color ColorOverlay = new Color32(0x05, 0x08, 0x12, 0xCC);       // 80% 반투명 오버레이
        private static readonly Color ColorTextMain = new Color32(0xE0, 0xDC, 0xD0, 0xFF);      // 텍스트 메인
        private static readonly Color ColorTextSub = new Color32(0xA0, 0x98, 0x90, 0xFF);       // 텍스트 서브
        private static readonly Color ColorBorder = new Color32(0x5A, 0x50, 0x70, 0xFF);        // UI 프레임 기본
        private static readonly Color ColorGold = new Color32(0xFF, 0xD8, 0x4D, 0xFF);          // 골드/강조
        private static readonly Color ColorNeonGreen = new Color32(0x40, 0xD4, 0x70, 0xFF);     // 에메랄드

        [Header("UI 스프라이트")]
        public UISprites uiSprites;

        /// <summary>
        /// 타워 3택에서 타워를 선택했을 때 발행.
        /// TreasureManager 또는 TowerInventory에서 구독하여 타워 획득 처리.
        /// </summary>
        public event Action<TowerData> OnTowerChosen;

        // === 런타임 생성 오브젝트 ===
        private GameObject _overlayRoot;
        private GameObject _panelRoot;
        private readonly List<GameObject> _cardObjects = new List<GameObject>();

        // === 상태 ===
        private float _savedTimeScale;
        private bool _isShowing;
        private Coroutine _showAnimCoroutine;
        private bool _isTowerMode;

        void OnEnable()
        {
            if (Singleton<Core.TreasureManager>.HasInstance)
                Core.TreasureManager.Instance.OnTreasureDropped += OnTreasureDropped;
        }

        void OnDisable()
        {
            if (Singleton<Core.TreasureManager>.HasInstance)
                Core.TreasureManager.Instance.OnTreasureDropped -= OnTreasureDropped;
        }

        // =====================================================================
        // 이벤트 핸들러
        // =====================================================================

        private void OnTreasureDropped(List<TreasureChoiceData> choices)
        {
            if (choices == null || choices.Count == 0) return;
            Show(choices);
        }

        // =====================================================================
        // Show / Hide
        // =====================================================================

        public void Show(List<TreasureChoiceData> choices)
        {
            if (_isShowing) return;
            _isShowing = true;
            _isTowerMode = false;

            // 게임 일시정지
            _savedTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            // SFX: 보물상자 드랍
            if (Singleton<SoundManager>.HasInstance)
                SoundManager.Instance.PlaySfx(SoundKeys.TreasureDrop);

            BuildUI(choices);

            // 슬라이드 업 애니메이션
            if (_showAnimCoroutine != null)
                StopCoroutine(_showAnimCoroutine);
            _showAnimCoroutine = StartCoroutine(ShowAnimation());
        }


        // =====================================================================
        // Show (tower 3-pick - Phase 2)
        // =====================================================================

        /// <summary>
        /// Tower 3-pick selection panel.
        /// </summary>
        public void ShowTowerChoice(List<TowerData> towerChoices)
        {
            if (_isShowing) return;
            if (towerChoices == null || towerChoices.Count == 0) return;
            _isShowing = true;
            _isTowerMode = true;
            _savedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            if (Singleton<SoundManager>.HasInstance)
                SoundManager.Instance.PlaySfx(SoundKeys.TreasureDrop);
            BuildTowerUI(towerChoices);
            if (_showAnimCoroutine != null)
                StopCoroutine(_showAnimCoroutine);
            _showAnimCoroutine = StartCoroutine(ShowAnimation());
        }

        public void Hide()
        {
            if (!_isShowing) return;
            _isShowing = false;

            if (_showAnimCoroutine != null)
            {
                StopCoroutine(_showAnimCoroutine);
                _showAnimCoroutine = null;
            }

            // 게임 재개
            Time.timeScale = _savedTimeScale;

            // UI 제거
            CleanupUI();
        }

        // =====================================================================
        // UI 구축 (런타임 코드 기반)
        // =====================================================================

        private void BuildUI(List<TreasureChoiceData> choices)
        {
            CleanupUI();

            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogWarning("[TreasureChoiceUI] 부모 Canvas를 찾을 수 없음");
                return;
            }

            // --- 풀스크린 반투명 오버레이 ---
            _overlayRoot = new GameObject("TreasureOverlay", typeof(RectTransform));
            _overlayRoot.transform.SetParent(parentCanvas.transform, false);
            var overlayRect = _overlayRoot.GetComponent<RectTransform>();
            StretchFull(overlayRect);

            var overlayImage = _overlayRoot.AddComponent<Image>();
            overlayImage.color = ColorOverlay;
            overlayImage.raycastTarget = true; // 뒤쪽 클릭 차단

            // --- 메인 패널 (중앙, 카드 정렬) ---
            _panelRoot = new GameObject("TreasurePanel", typeof(RectTransform));
            _panelRoot.transform.SetParent(_overlayRoot.transform, false);
            var panelRect = _panelRoot.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(900, 500);
            panelRect.anchoredPosition = Vector2.zero;

            // 제목 텍스트: "보물 선택"
            var titleGo = CreateTextObject("TreasureTitle", _panelRoot.transform);
            var titleRect = titleGo.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(0, 50);
            titleRect.anchoredPosition = new Vector2(0, 30);
            var titleText = titleGo.GetComponent<Text>();
            titleText.text = "Soul Blessing";
            titleText.fontSize = 30;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = ColorGold;
            titleText.alignment = TextAnchor.MiddleCenter;

            // 부제목: "하나를 선택하세요"
            var subTitleGo = CreateTextObject("TreasureSubTitle", _panelRoot.transform);
            var subTitleRect = subTitleGo.GetComponent<RectTransform>();
            subTitleRect.anchorMin = new Vector2(0f, 1f);
            subTitleRect.anchorMax = new Vector2(1f, 1f);
            subTitleRect.pivot = new Vector2(0.5f, 1f);
            subTitleRect.sizeDelta = new Vector2(0, 30);
            subTitleRect.anchoredPosition = new Vector2(0, -25);
            var subTitleText = subTitleGo.GetComponent<Text>();
            subTitleText.text = "축복 하나를 선택하세요";
            subTitleText.fontSize = 16;
            subTitleText.color = ColorTextSub;
            subTitleText.alignment = TextAnchor.MiddleCenter;

            // --- 카드 컨테이너 (HorizontalLayoutGroup) ---
            var cardContainer = new GameObject("CardContainer", typeof(RectTransform));
            cardContainer.transform.SetParent(_panelRoot.transform, false);
            var containerRect = cardContainer.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.anchoredPosition = new Vector2(0, -30);

            float cardWidth = 240f;
            float cardHeight = 340f;
            float spacing = 30f;
            int cardCount = choices.Count;
            float totalWidth = (cardWidth * cardCount) + (spacing * (cardCount - 1));
            containerRect.sizeDelta = new Vector2(totalWidth, cardHeight);

            var hlg = cardContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = spacing;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.padding = new RectOffset(0, 0, 0, 0);

            // --- 카드 생성 ---
            for (int i = 0; i < choices.Count; i++)
            {
                var card = CreateBuffCard(choices[i], cardContainer.transform, cardWidth, cardHeight);
                _cardObjects.Add(card);
            }
        }

        private GameObject CreateBuffCard(TreasureChoiceData data, Transform parent, float width, float height)
        {
            Color rarityColor = GetRarityColor(data.rarity);
            string rarityLabel = GetRarityLabel(data.rarity);

            // --- 카드 루트 (버튼 역할) ---
            var cardGo = new GameObject($"Card_{data.displayName}", typeof(RectTransform));
            cardGo.transform.SetParent(parent, false);
            var cardRect = cardGo.GetComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(width, height);
            var cardLE = cardGo.AddComponent<LayoutElement>();
            cardLE.preferredWidth = width;
            cardLE.preferredHeight = height;

            // 카드 배경
            var cardImage = cardGo.AddComponent<Image>();
            cardImage.color = ColorPanel;
            if (uiSprites != null && uiSprites.panelFrame != null)
            {
                cardImage.sprite = uiSprites.panelFrame;
                cardImage.type = Image.Type.Sliced;
            }

            // 테두리 (등급 색상)
            var outline = cardGo.AddComponent<Outline>();
            outline.effectColor = rarityColor;
            outline.effectDistance = new Vector2(2, -2);

            // 버튼
            var button = cardGo.AddComponent<Button>();
            button.targetGraphic = cardImage;
            var colors = button.colors;
            colors.normalColor = ColorPanel;
            colors.highlightedColor = ColorBrightPanel;
            colors.pressedColor = new Color32(0x30, 0x2E, 0x46, 0xFF);
            colors.selectedColor = ColorBrightPanel;
            button.colors = colors;

            // 클릭 이벤트
            var capturedData = data;
            button.onClick.AddListener(() => OnBuffCardClicked(capturedData));

            // --- 카드 내부 레이아웃 ---

            // 등급 라벨 (상단)
            var rarityGo = CreateTextObject("RarityLabel", cardGo.transform);
            var rarityRect = rarityGo.GetComponent<RectTransform>();
            rarityRect.anchorMin = new Vector2(0f, 1f);
            rarityRect.anchorMax = new Vector2(1f, 1f);
            rarityRect.pivot = new Vector2(0.5f, 1f);
            rarityRect.sizeDelta = new Vector2(-20, 24);
            rarityRect.anchoredPosition = new Vector2(0, -8);
            var rarityText = rarityGo.GetComponent<Text>();
            rarityText.text = rarityLabel;
            rarityText.fontSize = 12;
            rarityText.fontStyle = FontStyle.Bold;
            rarityText.color = rarityColor;
            rarityText.alignment = TextAnchor.MiddleCenter;

            // 아이콘 (중앙 상부)
            var iconGo = new GameObject("Icon", typeof(RectTransform));
            iconGo.transform.SetParent(cardGo.transform, false);
            var iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 1f);
            iconRect.anchorMax = new Vector2(0.5f, 1f);
            iconRect.pivot = new Vector2(0.5f, 1f);
            iconRect.sizeDelta = new Vector2(80, 80);
            iconRect.anchoredPosition = new Vector2(0, -40);
            var iconImage = iconGo.AddComponent<Image>();
            iconImage.raycastTarget = false;
            if (data.icon != null)
            {
                iconImage.sprite = data.icon;
                iconImage.preserveAspect = true;
                iconImage.color = Color.white;
            }
            else
            {
                // 아이콘 없을 때 placeholder: 등급 색상의 불투명 사각형
                iconImage.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, 0.3f);
            }

            // 이름 텍스트
            var nameGo = CreateTextObject("NameText", cardGo.transform);
            var nameRect = nameGo.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 1f);
            nameRect.anchorMax = new Vector2(1f, 1f);
            nameRect.pivot = new Vector2(0.5f, 1f);
            nameRect.sizeDelta = new Vector2(-20, 28);
            nameRect.anchoredPosition = new Vector2(0, -130);
            var nameText = nameGo.GetComponent<Text>();
            nameText.text = data.displayName ?? "???";
            nameText.fontSize = 16;
            nameText.fontStyle = FontStyle.Bold;
            nameText.color = ColorTextMain;
            nameText.alignment = TextAnchor.MiddleCenter;

            // 구분선
            var dividerGo = new GameObject("Divider", typeof(RectTransform));
            dividerGo.transform.SetParent(cardGo.transform, false);
            var dividerRect = dividerGo.GetComponent<RectTransform>();
            dividerRect.anchorMin = new Vector2(0.1f, 1f);
            dividerRect.anchorMax = new Vector2(0.9f, 1f);
            dividerRect.pivot = new Vector2(0.5f, 0.5f);
            dividerRect.sizeDelta = new Vector2(0, 1);
            dividerRect.anchoredPosition = new Vector2(0, -164);
            var dividerImage = dividerGo.AddComponent<Image>();
            dividerImage.color = ColorBorder;
            dividerImage.raycastTarget = false;

            // 설명 텍스트
            var descGo = CreateTextObject("DescText", cardGo.transform);
            var descRect = descGo.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0f, 0f);
            descRect.anchorMax = new Vector2(1f, 1f);
            descRect.pivot = new Vector2(0.5f, 0.5f);
            descRect.offsetMin = new Vector2(12, 40);  // left, bottom
            descRect.offsetMax = new Vector2(-12, -172); // right, top
            var descText = descGo.GetComponent<Text>();
            descText.text = data.description ?? "";
            descText.fontSize = 13;
            descText.color = ColorTextSub;
            descText.alignment = TextAnchor.UpperCenter;
            descText.horizontalOverflow = HorizontalWrapMode.Wrap;
            descText.verticalOverflow = VerticalWrapMode.Truncate;

            // 효과 수치 (하단)
            var valueGo = CreateTextObject("ValueText", cardGo.transform);
            var valueRect = valueGo.GetComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0f, 0f);
            valueRect.anchorMax = new Vector2(1f, 0f);
            valueRect.pivot = new Vector2(0.5f, 0f);
            valueRect.sizeDelta = new Vector2(-20, 28);
            valueRect.anchoredPosition = new Vector2(0, 8);
            var valueText = valueGo.GetComponent<Text>();
            valueText.text = FormatValue(data);
            valueText.fontSize = 14;
            valueText.fontStyle = FontStyle.Bold;
            valueText.color = rarityColor;
            valueText.alignment = TextAnchor.MiddleCenter;

            return cardGo;
        }

        // =====================================================================
        // 카드 선택
        // =====================================================================

        private void OnBuffCardClicked(TreasureChoiceData choice)
        {
            if (!_isShowing) return;

            // SFX: 보물 선택
            if (Singleton<SoundManager>.HasInstance)
                SoundManager.Instance.PlaySfx(SoundKeys.TreasureSelect);

            // TreasureManager에 선택 적용
            if (Singleton<Core.TreasureManager>.HasInstance)
                Core.TreasureManager.Instance.ApplyChoice(choice);

            Hide();
        }


        // =====================================================================
        // Tower 3-pick UI (Phase 2)
        // =====================================================================

        private void BuildTowerUI(List<TowerData> towerChoices)
        {
            CleanupUI();

            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogWarning("[TreasureChoiceUI] Canvas not found");
                return;
            }

            // --- fullscreen overlay ---
            _overlayRoot = new GameObject("TowerOverlay", typeof(RectTransform));
            _overlayRoot.transform.SetParent(parentCanvas.transform, false);
            var overlayRect = _overlayRoot.GetComponent<RectTransform>();
            StretchFull(overlayRect);
            var overlayImage = _overlayRoot.AddComponent<Image>();
            overlayImage.color = ColorOverlay;
            overlayImage.raycastTarget = true;

            // --- main panel ---
            _panelRoot = new GameObject("TowerPanel", typeof(RectTransform));
            _panelRoot.transform.SetParent(_overlayRoot.transform, false);
            var panelRect = _panelRoot.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(900, 520);
            panelRect.anchoredPosition = Vector2.zero;

            // title
            var titleGo = CreateTextObject("TowerTitle", _panelRoot.transform);
            var titleRect = titleGo.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(0, 50);
            titleRect.anchoredPosition = new Vector2(0, 30);
            var titleText = titleGo.GetComponent<Text>();
            titleText.text = "Soul Armory";
            titleText.fontSize = 30;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = ColorGold;
            titleText.alignment = TextAnchor.MiddleCenter;

            // subtitle
            var subGo = CreateTextObject("TowerSubTitle", _panelRoot.transform);
            var subRect = subGo.GetComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0f, 1f);
            subRect.anchorMax = new Vector2(1f, 1f);
            subRect.pivot = new Vector2(0.5f, 1f);
            subRect.sizeDelta = new Vector2(0, 30);
            subRect.anchoredPosition = new Vector2(0, -25);
            var subText = subGo.GetComponent<Text>();
            subText.text = "배치할 타워를 선택하세요";
            subText.fontSize = 16;
            subText.color = ColorTextSub;
            subText.alignment = TextAnchor.MiddleCenter;

            // card container
            var cardContainer = new GameObject("CardContainer", typeof(RectTransform));
            cardContainer.transform.SetParent(_panelRoot.transform, false);
            var containerRect = cardContainer.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.anchoredPosition = new Vector2(0, -30);

            float cardWidth = 240f;
            float cardHeight = 360f;
            float spacing = 30f;
            int cardCount = towerChoices.Count;
            float totalWidth = (cardWidth * cardCount) + (spacing * (cardCount - 1));
            containerRect.sizeDelta = new Vector2(totalWidth, cardHeight);

            var hlg = cardContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = spacing;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            for (int i = 0; i < towerChoices.Count; i++)
            {
                var card = CreateTowerCard(towerChoices[i], cardContainer.transform, cardWidth, cardHeight);
                _cardObjects.Add(card);
            }
        }

        private GameObject CreateTowerCard(TowerData data, Transform parent, float width, float height)
        {
            Color typeColor = GetTowerTypeColor(data.type);

            var cardGo = new GameObject($"TowerCard_{data.towerName}", typeof(RectTransform));
            cardGo.transform.SetParent(parent, false);
            var cardRect = cardGo.GetComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(width, height);
            var cardLE = cardGo.AddComponent<LayoutElement>();
            cardLE.preferredWidth = width;
            cardLE.preferredHeight = height;

            var cardImage = cardGo.AddComponent<Image>();
            cardImage.color = ColorPanel;
            if (uiSprites != null && uiSprites.panelFrame != null)
            {
                cardImage.sprite = uiSprites.panelFrame;
                cardImage.type = Image.Type.Sliced;
            }

            var outline = cardGo.AddComponent<Outline>();
            outline.effectColor = typeColor;
            outline.effectDistance = new Vector2(2, -2);

            var button = cardGo.AddComponent<Button>();
            button.targetGraphic = cardImage;
            var colors = button.colors;
            colors.normalColor = ColorPanel;
            colors.highlightedColor = ColorBrightPanel;
            colors.pressedColor = new Color32(0x30, 0x2E, 0x46, 0xFF);
            colors.selectedColor = ColorBrightPanel;
            button.colors = colors;

            var capturedData = data;
            button.onClick.AddListener(() => OnTowerCardClicked(capturedData));

            // type label (top)
            var typeGo = CreateTextObject("TypeLabel", cardGo.transform);
            var typeRect = typeGo.GetComponent<RectTransform>();
            typeRect.anchorMin = new Vector2(0f, 1f);
            typeRect.anchorMax = new Vector2(1f, 1f);
            typeRect.pivot = new Vector2(0.5f, 1f);
            typeRect.sizeDelta = new Vector2(-20, 24);
            typeRect.anchoredPosition = new Vector2(0, -8);
            var typeText = typeGo.GetComponent<Text>();
            typeText.text = data.type.ToString();
            typeText.fontSize = 12;
            typeText.fontStyle = FontStyle.Bold;
            typeText.color = typeColor;
            typeText.alignment = TextAnchor.MiddleCenter;

            // icon (96x96)
            var iconGo = new GameObject("Icon", typeof(RectTransform));
            iconGo.transform.SetParent(cardGo.transform, false);
            var iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 1f);
            iconRect.anchorMax = new Vector2(0.5f, 1f);
            iconRect.pivot = new Vector2(0.5f, 1f);
            iconRect.sizeDelta = new Vector2(96, 96);
            iconRect.anchoredPosition = new Vector2(0, -36);
            var iconImage = iconGo.AddComponent<Image>();
            iconImage.raycastTarget = false;
            if (data.icon != null)
            {
                iconImage.sprite = data.icon;
                iconImage.preserveAspect = true;
                iconImage.color = Color.white;
            }
            else
            {
                iconImage.color = new Color(typeColor.r, typeColor.g, typeColor.b, 0.3f);
            }

            // tower name
            var nameGo = CreateTextObject("NameText", cardGo.transform);
            var nameRect = nameGo.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 1f);
            nameRect.anchorMax = new Vector2(1f, 1f);
            nameRect.pivot = new Vector2(0.5f, 1f);
            nameRect.sizeDelta = new Vector2(-20, 28);
            nameRect.anchoredPosition = new Vector2(0, -140);
            var nameText = nameGo.GetComponent<Text>();
            nameText.text = data.towerName ?? "???";
            nameText.fontSize = 18;
            nameText.fontStyle = FontStyle.Bold;
            nameText.color = typeColor;
            nameText.alignment = TextAnchor.MiddleCenter;

            // divider
            var divGo = new GameObject("Divider", typeof(RectTransform));
            divGo.transform.SetParent(cardGo.transform, false);
            var divRect = divGo.GetComponent<RectTransform>();
            divRect.anchorMin = new Vector2(0.1f, 1f);
            divRect.anchorMax = new Vector2(0.9f, 1f);
            divRect.pivot = new Vector2(0.5f, 0.5f);
            divRect.sizeDelta = new Vector2(0, 1);
            divRect.anchoredPosition = new Vector2(0, -176);
            var divImage = divGo.AddComponent<Image>();
            divImage.color = ColorBorder;
            divImage.raycastTarget = false;

            // stats (Lv1)
            float statY = -184f;
            float statH = 20f;
            CreateStatLine(cardGo.transform, "DMG", $"{data.GetDamage(1):F1}", ColorTextMain, statY, statH);
            statY -= statH;
            CreateStatLine(cardGo.transform, "SPD", $"{data.GetAttackSpeed(1):F2}/s", ColorTextMain, statY, statH);
            statY -= statH;
            CreateStatLine(cardGo.transform, "RNG", $"{data.GetRange(1):F1}", ColorTextMain, statY, statH);
            statY -= statH;

            // special stats
            if (data.type == TowerType.Cannon && data.GetExplosionRadius(1) > 0)
            {
                CreateStatLine(cardGo.transform, "AoE", $"{data.GetExplosionRadius(1):F1}", ColorTowerCannon, statY, statH);
                statY -= statH;
            }
            else if (data.type == TowerType.Ice && data.GetSlowRate(1) > 0)
            {
                CreateStatLine(cardGo.transform, "SLOW", $"{data.GetSlowRate(1) * 100f:F0}%", ColorTowerIce, statY, statH);
                statY -= statH;
            }

            // place cost (bottom)
            var costGo = CreateTextObject("CostText", cardGo.transform);
            var costRect = costGo.GetComponent<RectTransform>();
            costRect.anchorMin = new Vector2(0f, 0f);
            costRect.anchorMax = new Vector2(1f, 0f);
            costRect.pivot = new Vector2(0.5f, 0f);
            costRect.sizeDelta = new Vector2(-20, 28);
            costRect.anchoredPosition = new Vector2(0, 8);
            var costText = costGo.GetComponent<Text>();
            costText.text = $"배치: {data.placeCost} Soul";
            costText.fontSize = 14;
            costText.fontStyle = FontStyle.Bold;
            costText.color = ColorNeonGreen;
            costText.alignment = TextAnchor.MiddleCenter;

            return cardGo;
        }

        private void CreateStatLine(Transform parent, string label, string value, Color valueColor, float yPos, float height)
        {
            // label (left)
            var labelGo = CreateTextObject($"Stat_{label}_Label", parent);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 1f);
            labelRect.anchorMax = new Vector2(0.5f, 1f);
            labelRect.pivot = new Vector2(0f, 1f);
            labelRect.sizeDelta = new Vector2(0, height);
            labelRect.anchoredPosition = new Vector2(16, yPos);
            var labelText = labelGo.GetComponent<Text>();
            labelText.text = label;
            labelText.fontSize = 12;
            labelText.color = ColorTextSub;
            labelText.alignment = TextAnchor.MiddleLeft;

            // value (right)
            var valGo = CreateTextObject($"Stat_{label}_Value", parent);
            var valRect = valGo.GetComponent<RectTransform>();
            valRect.anchorMin = new Vector2(0.5f, 1f);
            valRect.anchorMax = new Vector2(1f, 1f);
            valRect.pivot = new Vector2(1f, 1f);
            valRect.sizeDelta = new Vector2(0, height);
            valRect.anchoredPosition = new Vector2(-16, yPos);
            var valText = valGo.GetComponent<Text>();
            valText.text = value;
            valText.fontSize = 12;
            valText.fontStyle = FontStyle.Bold;
            valText.color = valueColor;
            valText.alignment = TextAnchor.MiddleRight;
        }

        private void OnTowerCardClicked(TowerData towerData)
        {
            if (!_isShowing) return;

            if (Singleton<SoundManager>.HasInstance)
                SoundManager.Instance.PlaySfx(SoundKeys.TreasureSelect);

            OnTowerChosen?.Invoke(towerData);
            Hide();
        }

        private static Color GetTowerTypeColor(TowerType type)
        {
            switch (type)
            {
                case TowerType.Arrow:     return ColorTowerArrow;
                case TowerType.Cannon:    return ColorTowerCannon;
                case TowerType.Ice:       return ColorTowerIce;
                case TowerType.Lightning: return ColorTowerLightning;
                case TowerType.Laser:     return ColorTowerLaser;
                case TowerType.Void:      return ColorTowerVoid;
                default:                  return ColorTextMain;
            }
        }

        // =====================================================================
        // 애니메이션
        // =====================================================================

        private IEnumerator ShowAnimation()
        {
            if (_panelRoot == null) yield break;

            var panelRect = _panelRoot.GetComponent<RectTransform>();
            if (panelRect == null) yield break;

            Vector2 targetPos = new Vector2(0, -30);
            Vector2 startPos = new Vector2(0, -430);

            float duration = 0.35f;
            float elapsed = 0f;

            panelRect.anchoredPosition = startPos;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                // ease-out cubic (InGameUI와 동일 패턴)
                t = 1f - (1f - t) * (1f - t) * (1f - t);
                panelRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                yield return null;
            }

            panelRect.anchoredPosition = targetPos;
            _showAnimCoroutine = null;
        }

        // =====================================================================
        // 유틸리티
        // =====================================================================

        private void CleanupUI()
        {
            _cardObjects.Clear();

            if (_overlayRoot != null)
            {
                Destroy(_overlayRoot);
                _overlayRoot = null;
            }
            _panelRoot = null;
        }

        private static GameObject CreateTextObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.raycastTarget = false;
            return go;
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Color GetRarityColor(TreasureRarity rarity)
        {
            switch (rarity)
            {
                case TreasureRarity.Common: return ColorCommon;
                case TreasureRarity.Rare:   return ColorRare;
                case TreasureRarity.Epic:   return ColorEpic;
                default:                     return ColorCommon;
            }
        }

        private static string GetRarityLabel(TreasureRarity rarity)
        {
            switch (rarity)
            {
                case TreasureRarity.Common: return "Common";
                case TreasureRarity.Rare:   return "Rare";
                case TreasureRarity.Epic:   return "Epic";
                default:                     return "Common";
            }
        }

        private static string FormatValue(TreasureChoiceData data)
        {
            switch (data.choiceType)
            {
                case TreasureChoiceType.DamageBoost:
                    return $"+{data.value * 100f:F0}% 데미지";
                case TreasureChoiceType.AttackSpeedBoost:
                    return $"+{data.value * 100f:F0}% 공격속도";
                case TreasureChoiceType.RangeBoost:
                    return $"+{data.value * 100f:F0}% 사거리";
                case TreasureChoiceType.BitBonus:
                    return $"+{data.value:F0} Soul/킬";
                case TreasureChoiceType.MaxHpBoost:
                    return $"+{data.value:F0} 최대 HP";
                case TreasureChoiceType.TowerCostDiscount:
                    return $"-{data.value * 100f:F0}% 타워 비용";
                case TreasureChoiceType.CritChance:
                    return $"+{data.value * 100f:F0}% 크리티컬";
                case TreasureChoiceType.SlowEffect:
                    return $"+{data.value * 100f:F0}% 감속 효과";
                default:
                    return $"+{data.value}";
            }
        }
    }
}
