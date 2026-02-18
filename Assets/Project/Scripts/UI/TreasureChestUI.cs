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
    /// Hub(Sanctum) 보물상자 오픈 UI.
    /// 전체 화면 반투명 오버레이 위에 타워 카드 3장을 표시하고,
    /// 플레이어가 하나를 선택하면 TreasureManager.ApplyTowerChoice()를 호출한다.
    ///
    /// 사용법:
    /// 1. HubUI에서 "보물상자 오픈" 버튼 클릭
    /// 2. TreasureManager.OnBossKilled() 호출 (또는 직접 ShowWithChoices 호출)
    /// 3. OnTowerTreasureDropped 이벤트로 본 패널 표시
    /// 4. 카드 클릭 -> 선택 확정 -> 패널 닫기
    /// </summary>
    public class TreasureChestUI : MonoBehaviour
    {
        // === PPT 명세 색상 팔레트 ===
        private static readonly Color ColorPanelBg      = new Color32(0x12, 0x1A, 0x2A, 0xFF); // #121A2A
        private static readonly Color ColorBrightPanel   = new Color32(0x1A, 0x24, 0x3A, 0xFF); // #1A243A
        private static readonly Color ColorOverlay       = new Color32(0x05, 0x08, 0x12, 0xCC); // #050812 80%
        private static readonly Color ColorNeonGreen     = new Color32(0x2B, 0xFF, 0x88, 0xFF); // #2BFF88
        private static readonly Color ColorTextMain      = new Color32(0xD8, 0xE4, 0xFF, 0xFF); // #D8E4FF
        private static readonly Color ColorTextSub       = new Color32(0xAF, 0xC3, 0xE8, 0xFF); // #AFC3E8
        private static readonly Color ColorBorder        = new Color32(0x5B, 0x6B, 0x8A, 0xFF); // #5B6B8A
        private static readonly Color ColorGold          = new Color32(0xFF, 0xD8, 0x4D, 0xFF); // #FFD84D

        // === 타워 타입별 색상 ===
        private static readonly Color ColorTowerArrow     = new Color32(0xC0, 0xA8, 0x70, 0xFF);
        private static readonly Color ColorTowerCannon    = new Color32(0xE0, 0x80, 0x30, 0xFF);
        private static readonly Color ColorTowerIce       = new Color32(0x40, 0x80, 0xD4, 0xFF);
        private static readonly Color ColorTowerLightning = new Color32(0xE0, 0xD0, 0x40, 0xFF);
        private static readonly Color ColorTowerLaser     = new Color32(0x90, 0x60, 0xD0, 0xFF);
        private static readonly Color ColorTowerVoid      = new Color32(0x60, 0x40, 0xA0, 0xFF);

        [Header("UI 스프라이트")]
        public UISprites uiSprites;

        // === 런타임 생성 오브젝트 ===
        private GameObject _overlayRoot;
        private GameObject _panelRoot;
        private readonly List<GameObject> _cardObjects = new List<GameObject>();

        // === 상태 ===
        private bool _isShowing;
        private Coroutine _showAnimCoroutine;
        private List<TowerData> _currentChoices;
        private int _selectedIndex = -1;

        void OnEnable()
        {
            if (Singleton<Core.TreasureManager>.HasInstance)
                Core.TreasureManager.Instance.OnTowerTreasureDropped += OnTowerTreasureDropped;
        }

        void OnDisable()
        {
            if (Singleton<Core.TreasureManager>.HasInstance)
                Core.TreasureManager.Instance.OnTowerTreasureDropped -= OnTowerTreasureDropped;
        }

        // =====================================================================
        // 이벤트 핸들러
        // =====================================================================

        private void OnTowerTreasureDropped(List<TowerData> choices)
        {
            if (choices == null || choices.Count == 0) return;
            ShowWithChoices(choices);
        }

        // =====================================================================
        // Show / Hide
        // =====================================================================

        /// <summary>
        /// 타워 카드 3택 패널을 표시합니다.
        /// </summary>
        public void ShowWithChoices(List<TowerData> choices)
        {
            if (_isShowing) return;
            if (choices == null || choices.Count == 0) return;

            _isShowing = true;
            _currentChoices = choices;
            _selectedIndex = -1;

            // SFX: 보물상자 오픈
            if (Singleton<SoundManager>.HasInstance)
                SoundManager.Instance.PlaySfx(SoundKeys.TreasureOpen);

            BuildUI(choices);

            // 슬라이드 업 애니메이션
            if (_showAnimCoroutine != null)
                StopCoroutine(_showAnimCoroutine);
            _showAnimCoroutine = StartCoroutine(ShowAnimation());
        }

        /// <summary>
        /// 패널을 숨기고 정리합니다.
        /// </summary>
        public void Hide()
        {
            if (!_isShowing) return;
            _isShowing = false;

            if (_showAnimCoroutine != null)
            {
                StopCoroutine(_showAnimCoroutine);
                _showAnimCoroutine = null;
            }

            CleanupUI();
        }

        /// <summary>
        /// 현재 패널이 표시중인지 여부.
        /// </summary>
        public bool IsShowing => _isShowing;

        // =====================================================================
        // UI 구축
        // =====================================================================

        private void BuildUI(List<TowerData> choices)
        {
            CleanupUI();

            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogWarning("[TreasureChestUI] 부모 Canvas를 찾을 수 없음");
                return;
            }

            // --- 풀스크린 반투명 오버레이 ---
            _overlayRoot = new GameObject("TreasureChestOverlay", typeof(RectTransform));
            _overlayRoot.transform.SetParent(parentCanvas.transform, false);
            var overlayRect = _overlayRoot.GetComponent<RectTransform>();
            StretchFull(overlayRect);

            var overlayImage = _overlayRoot.AddComponent<Image>();
            overlayImage.color = ColorOverlay;
            overlayImage.raycastTarget = true;

            // --- 메인 패널 (중앙 800x500) ---
            _panelRoot = new GameObject("TreasureChestPanel", typeof(RectTransform));
            _panelRoot.transform.SetParent(_overlayRoot.transform, false);
            var panelRect = _panelRoot.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(800, 500);
            panelRect.anchoredPosition = Vector2.zero;

            // 패널 배경
            var panelImage = _panelRoot.AddComponent<Image>();
            panelImage.color = ColorPanelBg;
            if (uiSprites != null && uiSprites.panelFrame != null)
            {
                panelImage.sprite = uiSprites.panelFrame;
                panelImage.type = Image.Type.Sliced;
            }

            // 패널 테두리
            var panelOutline = _panelRoot.AddComponent<Outline>();
            panelOutline.effectColor = ColorBorder;
            panelOutline.effectDistance = new Vector2(2, -2);

            // --- 제목 텍스트 ---
            var titleGo = CreateTextObject("Title", _panelRoot.transform);
            var titleRect = titleGo.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(0, 50);
            titleRect.anchoredPosition = new Vector2(0, -10);
            var titleText = titleGo.GetComponent<Text>();
            titleText.text = "Soul Chest";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = ColorGold;
            titleText.alignment = TextAnchor.MiddleCenter;

            // --- 부제목 ---
            var subGo = CreateTextObject("SubTitle", _panelRoot.transform);
            var subRect = subGo.GetComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0f, 1f);
            subRect.anchorMax = new Vector2(1f, 1f);
            subRect.pivot = new Vector2(0.5f, 1f);
            subRect.sizeDelta = new Vector2(0, 24);
            subRect.anchoredPosition = new Vector2(0, -56);
            var subText = subGo.GetComponent<Text>();
            subText.text = "\ud68d\ub4dd\ud560 \ud0c0\uc6cc\ub97c \uc120\ud0dd\ud558\uc138\uc694";
            subText.fontSize = 14;
            subText.color = ColorTextSub;
            subText.alignment = TextAnchor.MiddleCenter;

            // --- 카드 컨테이너 (HorizontalLayoutGroup, spacing 20) ---
            var cardContainer = new GameObject("CardContainer", typeof(RectTransform));
            cardContainer.transform.SetParent(_panelRoot.transform, false);
            var containerRect = cardContainer.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.anchoredPosition = new Vector2(0, -20);

            float cardWidth = 220f;
            float cardHeight = 320f;
            float spacing = 20f;
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
                var card = CreateTowerCard(choices[i], i, cardContainer.transform, cardWidth, cardHeight);
                _cardObjects.Add(card);
            }

            // --- 닫기 버튼 (선택 취소용, 우상단) ---
            var closeGo = new GameObject("CloseButton", typeof(RectTransform));
            closeGo.transform.SetParent(_panelRoot.transform, false);
            var closeRect = closeGo.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 1f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.sizeDelta = new Vector2(36, 36);
            closeRect.anchoredPosition = new Vector2(-8, -8);

            var closeImage = closeGo.AddComponent<Image>();
            closeImage.color = ColorBrightPanel;

            var closeBtn = closeGo.AddComponent<Button>();
            closeBtn.targetGraphic = closeImage;
            closeBtn.onClick.AddListener(OnCloseClicked);

            var closeTextGo = CreateTextObject("CloseX", closeGo.transform);
            var closeTextRect = closeTextGo.GetComponent<RectTransform>();
            StretchFull(closeTextRect);
            var closeText = closeTextGo.GetComponent<Text>();
            closeText.text = "X";
            closeText.fontSize = 18;
            closeText.fontStyle = FontStyle.Bold;
            closeText.color = ColorTextMain;
            closeText.alignment = TextAnchor.MiddleCenter;
            closeText.raycastTarget = false;
        }

        private GameObject CreateTowerCard(TowerData data, int index, Transform parent, float width, float height)
        {
            Color typeColor = GetTowerTypeColor(data.type);

            // --- 카드 루트 ---
            var cardGo = new GameObject($"TowerCard_{index}", typeof(RectTransform));
            cardGo.transform.SetParent(parent, false);
            var cardRect = cardGo.GetComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(width, height);
            var cardLE = cardGo.AddComponent<LayoutElement>();
            cardLE.preferredWidth = width;
            cardLE.preferredHeight = height;

            // 카드 배경
            var cardImage = cardGo.AddComponent<Image>();
            cardImage.color = ColorPanelBg;
            if (uiSprites != null && uiSprites.panelFrame != null)
            {
                cardImage.sprite = uiSprites.panelFrame;
                cardImage.type = Image.Type.Sliced;
            }

            // 테두리 (기본: Border 색상, 선택 시 NeonGreen)
            var outline = cardGo.AddComponent<Outline>();
            outline.effectColor = ColorBorder;
            outline.effectDistance = new Vector2(2, -2);

            // 버튼
            var button = cardGo.AddComponent<Button>();
            button.targetGraphic = cardImage;
            var colors = button.colors;
            colors.normalColor = ColorPanelBg;
            colors.highlightedColor = ColorBrightPanel;
            colors.pressedColor = new Color32(0x0B, 0x12, 0x20, 0xFF);
            colors.selectedColor = ColorBrightPanel;
            button.colors = colors;

            int capturedIndex = index;
            var capturedData = data;
            button.onClick.AddListener(() => OnCardClicked(capturedIndex, capturedData));

            // --- 카드 내용 ---

            // 타워 스프라이트 아이콘 (64x64)
            var iconGo = new GameObject("TowerIcon", typeof(RectTransform));
            iconGo.transform.SetParent(cardGo.transform, false);
            var iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 1f);
            iconRect.anchorMax = new Vector2(0.5f, 1f);
            iconRect.pivot = new Vector2(0.5f, 1f);
            iconRect.sizeDelta = new Vector2(64, 64);
            iconRect.anchoredPosition = new Vector2(0, -20);
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

            // 타워 이름
            var nameGo = CreateTextObject("TowerName", cardGo.transform);
            var nameRect = nameGo.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 1f);
            nameRect.anchorMax = new Vector2(1f, 1f);
            nameRect.pivot = new Vector2(0.5f, 1f);
            nameRect.sizeDelta = new Vector2(-16, 28);
            nameRect.anchoredPosition = new Vector2(0, -90);
            var nameText = nameGo.GetComponent<Text>();
            nameText.text = data.towerName ?? "???";
            nameText.fontSize = 16;
            nameText.fontStyle = FontStyle.Bold;
            nameText.color = typeColor;
            nameText.alignment = TextAnchor.MiddleCenter;

            // 타입 라벨
            var typeGo = CreateTextObject("TypeLabel", cardGo.transform);
            var typeRect = typeGo.GetComponent<RectTransform>();
            typeRect.anchorMin = new Vector2(0f, 1f);
            typeRect.anchorMax = new Vector2(1f, 1f);
            typeRect.pivot = new Vector2(0.5f, 1f);
            typeRect.sizeDelta = new Vector2(-16, 20);
            typeRect.anchoredPosition = new Vector2(0, -118);
            var typeText = typeGo.GetComponent<Text>();
            typeText.text = data.type.ToString();
            typeText.fontSize = 11;
            typeText.color = ColorTextSub;
            typeText.alignment = TextAnchor.MiddleCenter;

            // 구분선
            var divGo = new GameObject("Divider", typeof(RectTransform));
            divGo.transform.SetParent(cardGo.transform, false);
            var divRect = divGo.GetComponent<RectTransform>();
            divRect.anchorMin = new Vector2(0.1f, 1f);
            divRect.anchorMax = new Vector2(0.9f, 1f);
            divRect.pivot = new Vector2(0.5f, 0.5f);
            divRect.sizeDelta = new Vector2(0, 1);
            divRect.anchoredPosition = new Vector2(0, -144);
            var divImage = divGo.AddComponent<Image>();
            divImage.color = ColorBorder;
            divImage.raycastTarget = false;

            // 주요 스탯 라인들 (Lv1 기준)
            float statY = -152f;
            float statH = 22f;

            CreateStatLine(cardGo.transform, "DMG", $"{data.GetDamage(1):F1}", ColorTextMain, statY, statH);
            statY -= statH;
            CreateStatLine(cardGo.transform, "SPD", $"{data.GetAttackSpeed(1):F2}/s", ColorTextMain, statY, statH);
            statY -= statH;
            CreateStatLine(cardGo.transform, "RNG", $"{data.GetRange(1):F1}", ColorTextMain, statY, statH);
            statY -= statH;

            // 타입별 특수 스탯
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
            else if (data.type == TowerType.Lightning && data.GetChainCount(1) > 0)
            {
                CreateStatLine(cardGo.transform, "CHAIN", $"{data.GetChainCount(1)}", ColorTowerLightning, statY, statH);
                statY -= statH;
            }

            // 배치 비용 (하단)
            var costGo = CreateTextObject("CostText", cardGo.transform);
            var costRect = costGo.GetComponent<RectTransform>();
            costRect.anchorMin = new Vector2(0f, 0f);
            costRect.anchorMax = new Vector2(1f, 0f);
            costRect.pivot = new Vector2(0.5f, 0f);
            costRect.sizeDelta = new Vector2(-16, 24);
            costRect.anchoredPosition = new Vector2(0, 8);
            var costText = costGo.GetComponent<Text>();
            costText.text = $"\ubc30\uce58: {data.placeCost} Soul";
            costText.fontSize = 12;
            costText.fontStyle = FontStyle.Bold;
            costText.color = ColorNeonGreen;
            costText.alignment = TextAnchor.MiddleCenter;

            return cardGo;
        }

        private void CreateStatLine(Transform parent, string label, string value, Color valueColor, float yPos, float height)
        {
            var labelGo = CreateTextObject($"Stat_{label}_Label", parent);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 1f);
            labelRect.anchorMax = new Vector2(0.5f, 1f);
            labelRect.pivot = new Vector2(0f, 1f);
            labelRect.sizeDelta = new Vector2(0, height);
            labelRect.anchoredPosition = new Vector2(14, yPos);
            var labelText = labelGo.GetComponent<Text>();
            labelText.text = label;
            labelText.fontSize = 11;
            labelText.color = ColorTextSub;
            labelText.alignment = TextAnchor.MiddleLeft;

            var valGo = CreateTextObject($"Stat_{label}_Value", parent);
            var valRect = valGo.GetComponent<RectTransform>();
            valRect.anchorMin = new Vector2(0.5f, 1f);
            valRect.anchorMax = new Vector2(1f, 1f);
            valRect.pivot = new Vector2(1f, 1f);
            valRect.sizeDelta = new Vector2(0, height);
            valRect.anchoredPosition = new Vector2(-14, yPos);
            var valText = valGo.GetComponent<Text>();
            valText.text = value;
            valText.fontSize = 11;
            valText.fontStyle = FontStyle.Bold;
            valText.color = valueColor;
            valText.alignment = TextAnchor.MiddleRight;
        }

        // =====================================================================
        // 카드 선택
        // =====================================================================

        private void OnCardClicked(int index, TowerData data)
        {
            if (!_isShowing) return;

            _selectedIndex = index;

            // 모든 카드 테두리를 기본으로 리셋 후, 선택된 카드만 네온 그린 하이라이트
            for (int i = 0; i < _cardObjects.Count; i++)
            {
                var outline = _cardObjects[i].GetComponent<Outline>();
                if (outline != null)
                {
                    outline.effectColor = (i == index) ? ColorNeonGreen : ColorBorder;
                    outline.effectDistance = (i == index) ? new Vector2(3, -3) : new Vector2(2, -2);
                }
            }

            // SFX: 보물 선택
            if (Singleton<SoundManager>.HasInstance)
                SoundManager.Instance.PlaySfx(SoundKeys.TreasureSelect);

            // 선택 확정: TreasureManager에 적용
            if (Singleton<Core.TreasureManager>.HasInstance)
                Core.TreasureManager.Instance.ApplyTowerChoice(data);

            // 짧은 딜레이 후 패널 닫기 (시각적 피드백 확인용)
            StartCoroutine(DelayedHide(0.3f));
        }

        private void OnCloseClicked()
        {
            if (!_isShowing) return;

            // SFX
            if (Singleton<SoundManager>.HasInstance)
                SoundManager.Instance.PlaySfx(SoundKeys.UiClick);

            Hide();
        }

        private IEnumerator DelayedHide(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            Hide();
        }

        // =====================================================================
        // 애니메이션
        // =====================================================================

        private IEnumerator ShowAnimation()
        {
            if (_panelRoot == null) yield break;

            var panelRect = _panelRoot.GetComponent<RectTransform>();
            if (panelRect == null) yield break;

            Vector2 targetPos = Vector2.zero;
            Vector2 startPos = new Vector2(0f, -400f);

            float duration = 0.35f;
            float elapsed = 0f;

            panelRect.anchoredPosition = startPos;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                // ease-out cubic
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
            _currentChoices = null;
            _selectedIndex = -1;

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
    }
}
