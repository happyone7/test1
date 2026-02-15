using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Nodebreaker.UI
{
    /// <summary>
    /// UI-5: Stage 클리어 화면.
    /// Core 획득 연출, 다음 스테이지 해금 안내, [Hub으로]/[다음 스테이지] 버튼.
    /// 다크 판타지 스타일: 딥 배경 + 마법 퍼플 Core 연출 + 골드 강조.
    /// </summary>
    public class StageClearUI : MonoBehaviour
    {
        // === 다크 판타지 색상 팔레트 ===
        private static readonly Color ColorOverlay      = new Color32(0x05, 0x08, 0x12, 0xDD);
        private static readonly Color ColorPanel        = new Color32(0x12, 0x10, 0x1A, 0xFF);
        private static readonly Color ColorBrightPanel  = new Color32(0x1A, 0x18, 0x28, 0xFF);
        private static readonly Color ColorBorder       = new Color32(0x5A, 0x50, 0x70, 0xFF);
        private static readonly Color ColorGold         = new Color32(0xFF, 0xD8, 0x4D, 0xFF);
        private static readonly Color ColorMagicPurple  = new Color32(0x90, 0x60, 0xD0, 0xFF);
        private static readonly Color ColorEmerald      = new Color32(0x40, 0xD4, 0x70, 0xFF);
        private static readonly Color ColorSapphire     = new Color32(0x40, 0x80, 0xD4, 0xFF);
        private static readonly Color ColorTextMain     = new Color32(0xE0, 0xDC, 0xD0, 0xFF);
        private static readonly Color ColorTextSub      = new Color32(0xA0, 0x98, 0x90, 0xFF);
        private static readonly Color ColorBtnBg        = new Color32(0x1A, 0x18, 0x28, 0xFF);

        private const float PANEL_WIDTH = 700f;
        private const float PANEL_HEIGHT = 500f;

        [Header("UI 스프라이트")]
        [SerializeField] private UISprites uiSprites;

        // 런타임 UI 요소
        private GameObject _overlayObj;
        private RectTransform _panelRect;
        private Text _titleText;
        private Text _stageNameText;
        private Text _coreText;
        private Text _bitText;
        private Text _unlockText;
        private Button _hubButton;
        private Button _nextButton;
        private CanvasGroup _canvasGroup;

        private Action _onGoToHub;
        private Action _onNextStage;
        private Coroutine _animCoroutine;

        /// <summary>
        /// Stage 클리어 화면을 표시한다.
        /// </summary>
        public void Show(string stageName, int coreEarned, int bitEarned,
                         bool hasNextStage, Action onGoToHub, Action onNextStage)
        {
            _onGoToHub = onGoToHub;
            _onNextStage = onNextStage;

            gameObject.SetActive(true);
            BuildUI(stageName, coreEarned, bitEarned, hasNextStage);

            if (_animCoroutine != null)
                StopCoroutine(_animCoroutine);
            _animCoroutine = StartCoroutine(ShowAnimation(coreEarned));
        }

        /// <summary>
        /// 화면을 숨긴다.
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

        private void BuildUI(string stageName, int coreEarned, int bitEarned, bool hasNextStage)
        {
            ClearUI();

            // === 오버레이 ===
            _overlayObj = CreateChild("Overlay");
            StretchFull(_overlayObj.GetComponent<RectTransform>());
            var overlayImage = _overlayObj.AddComponent<Image>();
            overlayImage.color = ColorOverlay;
            overlayImage.raycastTarget = true;

            // === 메인 패널 ===
            var panelObj = CreateChild("ClearPanel");
            _panelRect = panelObj.GetComponent<RectTransform>();
            _panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            _panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            _panelRect.pivot = new Vector2(0.5f, 0.5f);
            _panelRect.sizeDelta = new Vector2(PANEL_WIDTH, PANEL_HEIGHT);
            _panelRect.anchoredPosition = Vector2.zero;

            var panelImage = panelObj.AddComponent<Image>();
            if (uiSprites != null)
                uiSprites.ApplyPanelFrame(panelImage);
            panelImage.color = ColorPanel;

            var panelOutline = panelObj.AddComponent<Outline>();
            panelOutline.effectColor = ColorEmerald;
            panelOutline.effectDistance = new Vector2(2, 2);

            _canvasGroup = panelObj.AddComponent<CanvasGroup>();

            // === STAGE CLEAR 타이틀 ===
            var titleObj = CreateChild("TitleText", panelObj.transform);
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.82f);
            titleRect.anchorMax = new Vector2(1, 0.96f);
            titleRect.offsetMin = new Vector2(20, 0);
            titleRect.offsetMax = new Vector2(-20, 0);

            _titleText = titleObj.AddComponent<Text>();
            _titleText.text = "STAGE CLEAR";
            _titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _titleText.fontSize = 36;
            _titleText.fontStyle = FontStyle.Bold;
            _titleText.alignment = TextAnchor.MiddleCenter;
            _titleText.color = ColorEmerald;

            // === 스테이지 이름 ===
            var stageObj = CreateChild("StageNameText", panelObj.transform);
            var stageRect = stageObj.GetComponent<RectTransform>();
            stageRect.anchorMin = new Vector2(0, 0.74f);
            stageRect.anchorMax = new Vector2(1, 0.82f);
            stageRect.offsetMin = new Vector2(20, 0);
            stageRect.offsetMax = new Vector2(-20, 0);

            _stageNameText = stageObj.AddComponent<Text>();
            _stageNameText.text = stageName ?? "";
            _stageNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _stageNameText.fontSize = 16;
            _stageNameText.alignment = TextAnchor.MiddleCenter;
            _stageNameText.color = ColorTextSub;

            // === 구분선 ===
            var divObj = CreateChild("Divider", panelObj.transform);
            var divRect = divObj.GetComponent<RectTransform>();
            divRect.anchorMin = new Vector2(0.1f, 0.72f);
            divRect.anchorMax = new Vector2(0.9f, 0.72f);
            divRect.sizeDelta = new Vector2(0, 2);
            var divImage = divObj.AddComponent<Image>();
            divImage.color = ColorBorder;

            // === Core 획득 연출 ===
            var coreObj = CreateChild("CoreText", panelObj.transform);
            var coreRect = coreObj.GetComponent<RectTransform>();
            coreRect.anchorMin = new Vector2(0.1f, 0.52f);
            coreRect.anchorMax = new Vector2(0.9f, 0.68f);
            coreRect.offsetMin = Vector2.zero;
            coreRect.offsetMax = Vector2.zero;

            _coreText = coreObj.AddComponent<Text>();
            _coreText.text = $"Core  +{coreEarned}";
            _coreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _coreText.fontSize = 28;
            _coreText.fontStyle = FontStyle.Bold;
            _coreText.alignment = TextAnchor.MiddleCenter;
            _coreText.color = ColorMagicPurple;

            // === Bit 획득 ===
            var bitObj = CreateChild("BitText", panelObj.transform);
            var bitRect = bitObj.GetComponent<RectTransform>();
            bitRect.anchorMin = new Vector2(0.1f, 0.40f);
            bitRect.anchorMax = new Vector2(0.9f, 0.52f);
            bitRect.offsetMin = Vector2.zero;
            bitRect.offsetMax = Vector2.zero;

            _bitText = bitObj.AddComponent<Text>();
            _bitText.text = $"Bit  +{bitEarned}";
            _bitText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _bitText.fontSize = 22;
            _bitText.alignment = TextAnchor.MiddleCenter;
            _bitText.color = ColorGold;

            // === 다음 스테이지 해금 안내 ===
            var unlockObj = CreateChild("UnlockText", panelObj.transform);
            var unlockRect = unlockObj.GetComponent<RectTransform>();
            unlockRect.anchorMin = new Vector2(0.1f, 0.28f);
            unlockRect.anchorMax = new Vector2(0.9f, 0.40f);
            unlockRect.offsetMin = Vector2.zero;
            unlockRect.offsetMax = Vector2.zero;

            _unlockText = unlockObj.AddComponent<Text>();
            _unlockText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _unlockText.fontSize = 16;
            _unlockText.alignment = TextAnchor.MiddleCenter;
            _unlockText.raycastTarget = false;

            if (hasNextStage)
            {
                _unlockText.text = "새로운 스테이지가 해금되었습니다!";
                _unlockText.color = ColorEmerald;
            }
            else
            {
                _unlockText.text = "마지막 스테이지를 클리어했습니다!";
                _unlockText.color = ColorGold;
            }

            // === 버튼 영역 ===
            var btnArea = CreateChild("ButtonArea", panelObj.transform);
            var btnAreaRect = btnArea.GetComponent<RectTransform>();
            btnAreaRect.anchorMin = new Vector2(0.05f, 0.04f);
            btnAreaRect.anchorMax = new Vector2(0.95f, 0.22f);
            btnAreaRect.offsetMin = Vector2.zero;
            btnAreaRect.offsetMax = Vector2.zero;

            // [Hub으로] 버튼
            _hubButton = CreateButton(btnArea.transform, "HubButton",
                "[ Hub으로 ]",
                new Vector2(0.05f, 0.1f), new Vector2(0.45f, 0.9f),
                ColorSapphire);
            _hubButton.onClick.AddListener(OnHubClicked);

            // [다음 스테이지] 버튼
            string nextBtnText = hasNextStage ? "[ 다음 스테이지 ]" : "[ Hub으로 ]";
            _nextButton = CreateButton(btnArea.transform, "NextButton",
                nextBtnText,
                new Vector2(0.55f, 0.1f), new Vector2(0.95f, 0.9f),
                ColorEmerald);
            _nextButton.onClick.AddListener(OnNextClicked);

            if (!hasNextStage)
            {
                // 마지막 스테이지면 다음 버튼도 Hub으로
                _nextButton.onClick.RemoveAllListeners();
                _nextButton.onClick.AddListener(OnHubClicked);
            }
        }

        private Button CreateButton(Transform parent, string name, string text,
                                     Vector2 anchorMin, Vector2 anchorMax, Color outlineColor)
        {
            var btnObj = CreateChild(name, parent);
            var btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = anchorMin;
            btnRect.anchorMax = anchorMax;
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            var btnImage = btnObj.AddComponent<Image>();
            if (uiSprites != null)
                uiSprites.ApplyBasicButton(null); // 패턴만 따르기
            btnImage.color = ColorBtnBg;

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImage;

            if (uiSprites != null)
                uiSprites.ApplyBasicButton(btn);

            var btnOutline = btnObj.AddComponent<Outline>();
            btnOutline.effectColor = outlineColor;
            btnOutline.effectDistance = new Vector2(2, 2);

            // 텍스트
            var textObj = CreateChild("Text", btnObj.transform);
            var textRect = textObj.GetComponent<RectTransform>();
            StretchFull(textRect);

            var btnText = textObj.AddComponent<Text>();
            btnText.text = text;
            btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnText.fontSize = 20;
            btnText.fontStyle = FontStyle.Bold;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = ColorTextMain;

            return btn;
        }

        /// <summary>
        /// 등장 애니메이션: 슬라이드업 + Core 카운트업 연출.
        /// </summary>
        private IEnumerator ShowAnimation(int coreEarned)
        {
            if (_panelRect == null) yield break;

            // 패널 슬라이드 업
            Vector2 targetPos = Vector2.zero;
            Vector2 startPos = new Vector2(0, -400f);
            _panelRect.anchoredPosition = startPos;

            if (_canvasGroup != null) _canvasGroup.alpha = 0f;

            float slideDuration = 0.4f;
            float elapsed = 0f;

            while (elapsed < slideDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / slideDuration;
                float easedT = 1f - (1f - t) * (1f - t) * (1f - t);

                _panelRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, easedT);
                if (_canvasGroup != null)
                    _canvasGroup.alpha = easedT;
                yield return null;
            }

            _panelRect.anchoredPosition = targetPos;
            if (_canvasGroup != null) _canvasGroup.alpha = 1f;

            // Core 카운트업 연출 (0에서 최종값까지)
            if (coreEarned > 0 && _coreText != null)
            {
                float countDuration = 0.8f;
                elapsed = 0f;
                while (elapsed < countDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / countDuration);
                    int displayCore = Mathf.RoundToInt(Mathf.Lerp(0, coreEarned, t));
                    _coreText.text = $"Core  +{displayCore}";

                    // 펄스 스케일 효과
                    float pulse = 1f + 0.1f * Mathf.Sin(t * Mathf.PI * 4f) * (1f - t);
                    _coreText.transform.localScale = Vector3.one * pulse;

                    yield return null;
                }

                _coreText.text = $"Core  +{coreEarned}";
                _coreText.transform.localScale = Vector3.one;
            }

            _animCoroutine = null;
        }

        private void OnHubClicked()
        {
            _onGoToHub?.Invoke();
            Hide();
        }

        private void OnNextClicked()
        {
            _onNextStage?.Invoke();
            Hide();
        }

        private void ClearUI()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
        }

        private GameObject CreateChild(string name, Transform parent = null)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent ?? transform, false);
            return obj;
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
