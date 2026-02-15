using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Nodebreaker.UI
{
    /// <summary>
    /// UI-3: 보물상자 3택 선택 UI.
    /// 웨이브 클리어 보상으로 카드 3장을 표시하고, 플레이어가 1장을 선택하면
    /// 확대+페이드 연출 후 콜백을 호출한다.
    /// 다크 판타지 스타일: 어두운 패널 + 골드/보라 포인트 + 글로우 테두리.
    /// </summary>
    public class TreasureChoiceUI : MonoBehaviour
    {
        // === 다크 판타지 색상 팔레트 ===
        private static readonly Color ColorOverlay      = new Color32(0x05, 0x08, 0x12, 0xDD);
        private static readonly Color ColorPanel        = new Color32(0x12, 0x10, 0x1A, 0xFF);
        private static readonly Color ColorCardBg       = new Color32(0x1A, 0x18, 0x28, 0xFF);
        private static readonly Color ColorCardHover    = new Color32(0x24, 0x22, 0x36, 0xFF);
        private static readonly Color ColorBorder       = new Color32(0x5A, 0x50, 0x70, 0xFF);
        private static readonly Color ColorGold         = new Color32(0xFF, 0xD8, 0x4D, 0xFF);
        private static readonly Color ColorMagicPurple  = new Color32(0x90, 0x60, 0xD0, 0xFF);
        private static readonly Color ColorEmerald      = new Color32(0x40, 0xD4, 0x70, 0xFF);
        private static readonly Color ColorTextMain     = new Color32(0xE0, 0xDC, 0xD0, 0xFF);
        private static readonly Color ColorTextSub      = new Color32(0xA0, 0x98, 0x90, 0xFF);
        private static readonly Color ColorRarityCommon = new Color32(0xA0, 0x98, 0x90, 0xFF);
        private static readonly Color ColorRarityRare   = new Color32(0x40, 0x80, 0xD4, 0xFF);
        private static readonly Color ColorRarityEpic   = new Color32(0x90, 0x60, 0xD0, 0xFF);

        private const float CARD_WIDTH = 220f;
        private const float CARD_HEIGHT = 320f;
        private const float CARD_SPACING = 30f;
        private const float ICON_SIZE = 80f;
        private const float SELECT_ANIM_DURATION = 0.5f;

        [Header("UI 스프라이트")]
        [SerializeField] private UISprites uiSprites;

        // 런타임 생성 UI 요소
        private GameObject _overlayObj;
        private GameObject _titleObj;
        private GameObject[] _cardObjects;
        private Image[] _cardImages;
        private Outline[] _cardOutlines;
        private Text[] _nameTexts;
        private Text[] _descTexts;
        private Image[] _iconImages;

        private Data.TreasureRewardData[] _choices;
        private Action<int> _onSelected;
        private bool _isAnimating;
        private Coroutine _animCoroutine;

        /// <summary>
        /// 3택 UI를 표시한다.
        /// </summary>
        public void Show(Data.TreasureRewardData[] choices, Action<int> onSelected)
        {
            if (choices == null || choices.Length == 0) return;

            _choices = choices;
            _onSelected = onSelected;
            _isAnimating = false;

            gameObject.SetActive(true);
            Time.timeScale = 0f; // 선택 중 일시정지

            BuildUI(choices);
            _animCoroutine = StartCoroutine(ShowAnimation());
        }

        /// <summary>
        /// 3택 UI를 숨긴다.
        /// </summary>
        public void Hide()
        {
            if (_animCoroutine != null)
            {
                StopCoroutine(_animCoroutine);
                _animCoroutine = null;
            }

            ClearUI();
            gameObject.SetActive(false);
        }

        private void BuildUI(Data.TreasureRewardData[] choices)
        {
            ClearUI();

            // --- 오버레이 ---
            _overlayObj = new GameObject("TreasureOverlay", typeof(RectTransform));
            _overlayObj.transform.SetParent(transform, false);
            var overlayRect = _overlayObj.GetComponent<RectTransform>();
            StretchFull(overlayRect);
            var overlayImage = _overlayObj.AddComponent<Image>();
            overlayImage.color = ColorOverlay;
            overlayImage.raycastTarget = true;

            // --- 제목 ---
            _titleObj = new GameObject("TitleText", typeof(RectTransform));
            _titleObj.transform.SetParent(transform, false);
            var titleRect = _titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.78f);
            titleRect.anchorMax = new Vector2(1, 0.92f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            var titleText = _titleObj.AddComponent<Text>();
            titleText.text = "TREASURE CHEST";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 32;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = ColorGold;

            // --- 서브 타이틀 ---
            var subTitleObj = new GameObject("SubTitleText", typeof(RectTransform));
            subTitleObj.transform.SetParent(transform, false);
            var subTitleRect = subTitleObj.GetComponent<RectTransform>();
            subTitleRect.anchorMin = new Vector2(0, 0.72f);
            subTitleRect.anchorMax = new Vector2(1, 0.78f);
            subTitleRect.offsetMin = Vector2.zero;
            subTitleRect.offsetMax = Vector2.zero;
            var subTitleText = subTitleObj.AddComponent<Text>();
            subTitleText.text = "보상 카드를 선택하세요";
            subTitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            subTitleText.fontSize = 16;
            subTitleText.alignment = TextAnchor.MiddleCenter;
            subTitleText.color = ColorTextSub;

            // --- 카드 컨테이너 ---
            int cardCount = Mathf.Min(choices.Length, 3);
            _cardObjects = new GameObject[cardCount];
            _cardImages = new Image[cardCount];
            _cardOutlines = new Outline[cardCount];
            _nameTexts = new Text[cardCount];
            _descTexts = new Text[cardCount];
            _iconImages = new Image[cardCount];

            // 카드 영역 중앙 정렬 컨테이너
            var cardContainer = new GameObject("CardContainer", typeof(RectTransform));
            cardContainer.transform.SetParent(transform, false);
            var containerRect = cardContainer.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            float totalWidth = cardCount * CARD_WIDTH + (cardCount - 1) * CARD_SPACING;
            containerRect.sizeDelta = new Vector2(totalWidth, CARD_HEIGHT);
            containerRect.anchoredPosition = new Vector2(0, -20f);

            var hlg = cardContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = CARD_SPACING;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            for (int i = 0; i < cardCount; i++)
            {
                CreateCard(cardContainer.transform, i, choices[i]);
            }
        }

        private void CreateCard(Transform parent, int index, Data.TreasureRewardData data)
        {
            // --- 카드 루트 ---
            var cardObj = new GameObject($"Card_{index}", typeof(RectTransform));
            cardObj.transform.SetParent(parent, false);
            var cardRect = cardObj.GetComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(CARD_WIDTH, CARD_HEIGHT);

            // 배경
            var cardImage = cardObj.AddComponent<Image>();
            if (uiSprites != null)
                uiSprites.ApplyPanelFrame(cardImage);
            cardImage.color = ColorCardBg;
            cardImage.raycastTarget = true;

            // 테두리 (희귀도별 색상)
            var outline = cardObj.AddComponent<Outline>();
            outline.effectColor = GetRarityColor(data != null ? data.rarity : 0);
            outline.effectDistance = new Vector2(2, 2);

            // 버튼
            var button = cardObj.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = ColorCardBg;
            colors.highlightedColor = ColorCardHover;
            colors.pressedColor = ColorCardBg;
            button.colors = colors;

            int capturedIndex = index;
            button.onClick.AddListener(() => OnCardSelected(capturedIndex));

            // --- 아이콘 ---
            var iconObj = new GameObject("Icon", typeof(RectTransform));
            iconObj.transform.SetParent(cardObj.transform, false);
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.6f);
            iconRect.anchorMax = new Vector2(0.5f, 0.6f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(ICON_SIZE, ICON_SIZE);
            iconRect.anchoredPosition = new Vector2(0, 20f);

            var iconImage = iconObj.AddComponent<Image>();
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
            if (data != null && data.icon != null)
            {
                iconImage.sprite = data.icon;
                iconImage.color = Color.white;
            }
            else
            {
                // 플레이스홀더: 골드 사각형
                iconImage.color = GetRarityColor(data != null ? data.rarity : 0) * 0.6f;
            }

            // --- 희귀도 뱃지 ---
            if (data != null && data.rarity > 0)
            {
                var badgeObj = new GameObject("RarityBadge", typeof(RectTransform));
                badgeObj.transform.SetParent(cardObj.transform, false);
                var badgeRect = badgeObj.GetComponent<RectTransform>();
                badgeRect.anchorMin = new Vector2(0.5f, 0.88f);
                badgeRect.anchorMax = new Vector2(0.5f, 0.88f);
                badgeRect.pivot = new Vector2(0.5f, 0.5f);
                badgeRect.sizeDelta = new Vector2(80, 22);

                var badgeText = badgeObj.AddComponent<Text>();
                badgeText.text = data.rarity == 1 ? "RARE" : "EPIC";
                badgeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                badgeText.fontSize = 11;
                badgeText.fontStyle = FontStyle.Bold;
                badgeText.alignment = TextAnchor.MiddleCenter;
                badgeText.color = GetRarityColor(data.rarity);
                badgeText.raycastTarget = false;
            }

            // --- 이름 텍스트 ---
            var nameObj = new GameObject("NameText", typeof(RectTransform));
            nameObj.transform.SetParent(cardObj.transform, false);
            var nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0.3f);
            nameRect.anchorMax = new Vector2(0.95f, 0.42f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;

            var nameText = nameObj.AddComponent<Text>();
            nameText.text = data != null ? data.rewardName : "???";
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.fontSize = 16;
            nameText.fontStyle = FontStyle.Bold;
            nameText.alignment = TextAnchor.MiddleCenter;
            nameText.color = ColorTextMain;
            nameText.raycastTarget = false;

            // --- 설명 텍스트 ---
            var descObj = new GameObject("DescText", typeof(RectTransform));
            descObj.transform.SetParent(cardObj.transform, false);
            var descRect = descObj.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.08f, 0.06f);
            descRect.anchorMax = new Vector2(0.92f, 0.3f);
            descRect.offsetMin = Vector2.zero;
            descRect.offsetMax = Vector2.zero;

            var descText = descObj.AddComponent<Text>();
            descText.text = data != null ? data.description : "";
            descText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            descText.fontSize = 12;
            descText.alignment = TextAnchor.UpperCenter;
            descText.color = ColorTextSub;
            descText.raycastTarget = false;

            // 참조 저장
            _cardObjects[index] = cardObj;
            _cardImages[index] = cardImage;
            _cardOutlines[index] = outline;
            _nameTexts[index] = nameText;
            _descTexts[index] = descText;
            _iconImages[index] = iconImage;
        }

        private void OnCardSelected(int index)
        {
            if (_isAnimating) return;
            _isAnimating = true;

            _animCoroutine = StartCoroutine(SelectAnimation(index));
        }

        /// <summary>
        /// 등장 애니메이션: 카드가 아래에서 위로 슬라이드 + 페이드인.
        /// </summary>
        private IEnumerator ShowAnimation()
        {
            if (_cardObjects == null) yield break;

            // CanvasGroup으로 전체 페이드
            var cg = gameObject.GetComponent<CanvasGroup>();
            if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            float duration = 0.4f;
            float elapsed = 0f;

            // 카드 초기 위치 저장 및 오프셋
            Vector2[] originalPositions = new Vector2[_cardObjects.Length];
            for (int i = 0; i < _cardObjects.Length; i++)
            {
                var rt = _cardObjects[i].GetComponent<RectTransform>();
                originalPositions[i] = rt.anchoredPosition;
                rt.anchoredPosition = originalPositions[i] + new Vector2(0, -100f);
            }

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                // ease-out cubic
                float easedT = 1f - (1f - t) * (1f - t) * (1f - t);

                cg.alpha = easedT;

                for (int i = 0; i < _cardObjects.Length; i++)
                {
                    // 시차 적용 (각 카드 0.05초씩 지연)
                    float cardT = Mathf.Clamp01((elapsed - i * 0.05f) / duration);
                    float cardEased = 1f - (1f - cardT) * (1f - cardT) * (1f - cardT);

                    var rt = _cardObjects[i].GetComponent<RectTransform>();
                    rt.anchoredPosition = Vector2.Lerp(
                        originalPositions[i] + new Vector2(0, -100f),
                        originalPositions[i],
                        cardEased
                    );
                }

                yield return null;
            }

            cg.alpha = 1f;
            for (int i = 0; i < _cardObjects.Length; i++)
            {
                var rt = _cardObjects[i].GetComponent<RectTransform>();
                rt.anchoredPosition = originalPositions[i];
            }
        }

        /// <summary>
        /// 선택 애니메이션: 선택된 카드 확대 + 나머지 페이드아웃, 이후 전체 페이드아웃.
        /// </summary>
        private IEnumerator SelectAnimation(int selectedIndex)
        {
            // 선택된 카드 테두리를 골드로
            if (_cardOutlines[selectedIndex] != null)
                _cardOutlines[selectedIndex].effectColor = ColorGold;

            float duration = SELECT_ANIM_DURATION;
            float elapsed = 0f;

            // 비선택 카드 CanvasGroup
            CanvasGroup[] cardCGs = new CanvasGroup[_cardObjects.Length];
            for (int i = 0; i < _cardObjects.Length; i++)
            {
                var cg = _cardObjects[i].GetComponent<CanvasGroup>();
                if (cg == null) cg = _cardObjects[i].AddComponent<CanvasGroup>();
                cardCGs[i] = cg;
            }

            Vector3 originalScale = _cardObjects[selectedIndex].transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easedT = 1f - (1f - t) * (1f - t);

                // 선택된 카드: 확대
                _cardObjects[selectedIndex].transform.localScale = Vector3.Lerp(
                    originalScale, originalScale * 1.15f, easedT);

                // 비선택 카드: 페이드아웃 + 축소
                for (int i = 0; i < _cardObjects.Length; i++)
                {
                    if (i == selectedIndex) continue;
                    cardCGs[i].alpha = 1f - easedT;
                    _cardObjects[i].transform.localScale = Vector3.Lerp(
                        originalScale, originalScale * 0.9f, easedT);
                }

                yield return null;
            }

            // 0.3초 대기 후 전체 페이드아웃
            yield return new WaitForSecondsRealtime(0.3f);

            var mainCG = gameObject.GetComponent<CanvasGroup>();
            if (mainCG == null) mainCG = gameObject.AddComponent<CanvasGroup>();

            elapsed = 0f;
            float fadeDuration = 0.3f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                mainCG.alpha = 1f - (elapsed / fadeDuration);
                yield return null;
            }

            mainCG.alpha = 1f;
            Time.timeScale = 1f; // 일시정지 해제

            // 콜백 호출
            _onSelected?.Invoke(selectedIndex);

            Hide();
        }

        private void ClearUI()
        {
            // 모든 동적 자식 삭제
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            _cardObjects = null;
            _cardImages = null;
            _cardOutlines = null;
            _nameTexts = null;
            _descTexts = null;
            _iconImages = null;
            _overlayObj = null;
            _titleObj = null;
        }

        private static Color GetRarityColor(int rarity)
        {
            switch (rarity)
            {
                case 1: return ColorRarityRare;   // Rare
                case 2: return ColorRarityEpic;   // Epic
                default: return ColorRarityCommon; // Common
            }
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
