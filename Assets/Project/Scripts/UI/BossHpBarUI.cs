using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Nodebreaker.UI
{
    /// <summary>
    /// UI-4: 보스 전용 HP바 UI.
    /// 화면 상단에 보스 이름 + 체력바를 표시한다.
    /// 보스 등장 시 상단에서 슬라이드인, 사망 시 페이드아웃.
    /// 다크 판타지 스타일: 어두운 프레임 + 루비레드 체력바.
    /// </summary>
    public class BossHpBarUI : MonoBehaviour
    {
        // === 다크 판타지 색상 팔레트 ===
        private static readonly Color ColorPanelBg     = new Color32(0x12, 0x10, 0x1A, 0xEE);
        private static readonly Color ColorBorder      = new Color32(0x5A, 0x50, 0x70, 0xFF);
        private static readonly Color ColorBorderBoss  = new Color32(0xD4, 0x40, 0x40, 0xFF);
        private static readonly Color ColorHpFill      = new Color32(0xD4, 0x40, 0x40, 0xFF);
        private static readonly Color ColorHpDamaged   = new Color32(0xFF, 0xD8, 0x4D, 0xFF); // 지연 감소 바
        private static readonly Color ColorHpBg        = new Color32(0x1A, 0x18, 0x28, 0xFF);
        private static readonly Color ColorTextMain    = new Color32(0xE0, 0xDC, 0xD0, 0xFF);
        private static readonly Color ColorTextSub     = new Color32(0xA0, 0x98, 0x90, 0xFF);

        private const float BAR_WIDTH = 600f;
        private const float BAR_HEIGHT = 50f;
        private const float HP_BAR_HEIGHT = 16f;
        private const float SLIDE_DURATION = 0.5f;
        private const float FADE_DURATION = 0.4f;
        private const float DAMAGE_LERP_SPEED = 2f;

        [Header("UI 스프라이트")]
        [SerializeField] private UISprites uiSprites;

        // 런타임 생성 UI 요소
        private RectTransform _barPanel;
        private Text _nameText;
        private Image _hpFillImage;
        private Image _hpDamagedFillImage;
        private Text _hpValueText;
        private CanvasGroup _canvasGroup;

        // 상태
        private bool _isVisible;
        private string _bossName;
        private int _currentHp;
        private int _maxHp;
        private float _displayedDamagedRatio;
        private Coroutine _slideCoroutine;

        void Awake()
        {
            BuildUI();
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 보스 등장 시 호출. 슬라이드인 연출과 함께 HP바를 표시한다.
        /// </summary>
        public void ShowBoss(string bossName, int maxHp)
        {
            _bossName = bossName;
            _maxHp = maxHp;
            _currentHp = maxHp;
            _displayedDamagedRatio = 1f;

            if (_nameText != null)
                _nameText.text = bossName;

            UpdateHpDisplay();
            gameObject.SetActive(true);
            _isVisible = true;

            if (_slideCoroutine != null)
                StopCoroutine(_slideCoroutine);
            _slideCoroutine = StartCoroutine(SlideInAnimation());
        }

        /// <summary>
        /// 보스 HP 업데이트.
        /// </summary>
        public void UpdateHp(int currentHp)
        {
            _currentHp = Mathf.Max(0, currentHp);
            UpdateHpDisplay();

            if (_currentHp <= 0)
                HideBoss();
        }

        /// <summary>
        /// 보스 사망/퇴장 시 페이드아웃.
        /// </summary>
        public void HideBoss()
        {
            if (!_isVisible) return;
            _isVisible = false;

            if (_slideCoroutine != null)
                StopCoroutine(_slideCoroutine);
            _slideCoroutine = StartCoroutine(FadeOutAnimation());
        }

        void Update()
        {
            if (!_isVisible || _maxHp <= 0) return;

            // 지연 감소 바 (골드색) - 실제 HP 비율을 향해 부드럽게 감소
            float targetRatio = (float)_currentHp / _maxHp;
            if (_displayedDamagedRatio > targetRatio)
            {
                _displayedDamagedRatio = Mathf.MoveTowards(
                    _displayedDamagedRatio, targetRatio,
                    DAMAGE_LERP_SPEED * Time.unscaledDeltaTime);

                if (_hpDamagedFillImage != null)
                    _hpDamagedFillImage.fillAmount = _displayedDamagedRatio;
            }
        }

        private void UpdateHpDisplay()
        {
            if (_maxHp <= 0) return;

            float ratio = (float)_currentHp / _maxHp;

            if (_hpFillImage != null)
                _hpFillImage.fillAmount = ratio;

            if (_hpValueText != null)
                _hpValueText.text = $"{_currentHp:N0} / {_maxHp:N0}";
        }

        private void BuildUI()
        {
            // === 메인 패널 (상단 중앙) ===
            var panelObj = new GameObject("BossBarPanel", typeof(RectTransform));
            panelObj.transform.SetParent(transform, false);
            _barPanel = panelObj.GetComponent<RectTransform>();
            _barPanel.anchorMin = new Vector2(0.5f, 1f);
            _barPanel.anchorMax = new Vector2(0.5f, 1f);
            _barPanel.pivot = new Vector2(0.5f, 1f);
            _barPanel.sizeDelta = new Vector2(BAR_WIDTH, BAR_HEIGHT);
            _barPanel.anchoredPosition = new Vector2(0, -8f);

            // 패널 배경
            var panelImage = panelObj.AddComponent<Image>();
            if (uiSprites != null)
                uiSprites.ApplyPanelFrame(panelImage);
            panelImage.color = ColorPanelBg;

            // 패널 테두리
            var panelOutline = panelObj.AddComponent<Outline>();
            panelOutline.effectColor = ColorBorderBoss;
            panelOutline.effectDistance = new Vector2(2, 2);

            // CanvasGroup (페이드용)
            _canvasGroup = panelObj.AddComponent<CanvasGroup>();

            // === 보스 이름 ===
            var nameObj = new GameObject("BossName", typeof(RectTransform));
            nameObj.transform.SetParent(panelObj.transform, false);
            var nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.02f, 0.55f);
            nameRect.anchorMax = new Vector2(0.98f, 0.95f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;

            _nameText = nameObj.AddComponent<Text>();
            _nameText.text = "";
            _nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _nameText.fontSize = 14;
            _nameText.fontStyle = FontStyle.Bold;
            _nameText.alignment = TextAnchor.MiddleCenter;
            _nameText.color = ColorTextMain;
            _nameText.raycastTarget = false;

            // === HP 바 배경 ===
            var hpBgObj = new GameObject("HpBarBg", typeof(RectTransform));
            hpBgObj.transform.SetParent(panelObj.transform, false);
            var hpBgRect = hpBgObj.GetComponent<RectTransform>();
            hpBgRect.anchorMin = new Vector2(0.03f, 0.1f);
            hpBgRect.anchorMax = new Vector2(0.97f, 0.5f);
            hpBgRect.offsetMin = Vector2.zero;
            hpBgRect.offsetMax = Vector2.zero;

            var hpBgImage = hpBgObj.AddComponent<Image>();
            if (uiSprites != null)
                uiSprites.ApplyHpBarFrame(hpBgImage);
            hpBgImage.color = ColorHpBg;

            // === HP 지연 감소 바 (골드) ===
            var hpDmgObj = new GameObject("HpDamagedFill", typeof(RectTransform));
            hpDmgObj.transform.SetParent(hpBgObj.transform, false);
            var hpDmgRect = hpDmgObj.GetComponent<RectTransform>();
            StretchFull(hpDmgRect);

            _hpDamagedFillImage = hpDmgObj.AddComponent<Image>();
            _hpDamagedFillImage.color = ColorHpDamaged;
            _hpDamagedFillImage.type = Image.Type.Filled;
            _hpDamagedFillImage.fillMethod = Image.FillMethod.Horizontal;
            _hpDamagedFillImage.fillAmount = 1f;
            _hpDamagedFillImage.raycastTarget = false;

            // === HP 실제 바 (루비레드) ===
            var hpFillObj = new GameObject("HpFill", typeof(RectTransform));
            hpFillObj.transform.SetParent(hpBgObj.transform, false);
            var hpFillRect = hpFillObj.GetComponent<RectTransform>();
            StretchFull(hpFillRect);

            _hpFillImage = hpFillObj.AddComponent<Image>();
            if (uiSprites != null)
                uiSprites.ApplyHpBarFill(_hpFillImage);
            _hpFillImage.color = ColorHpFill;
            _hpFillImage.type = Image.Type.Filled;
            _hpFillImage.fillMethod = Image.FillMethod.Horizontal;
            _hpFillImage.fillAmount = 1f;
            _hpFillImage.raycastTarget = false;

            // === HP 값 텍스트 (바 위에 겹침) ===
            var hpTextObj = new GameObject("HpValueText", typeof(RectTransform));
            hpTextObj.transform.SetParent(hpBgObj.transform, false);
            var hpTextRect = hpTextObj.GetComponent<RectTransform>();
            StretchFull(hpTextRect);

            _hpValueText = hpTextObj.AddComponent<Text>();
            _hpValueText.text = "";
            _hpValueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _hpValueText.fontSize = 11;
            _hpValueText.alignment = TextAnchor.MiddleCenter;
            _hpValueText.color = ColorTextMain;
            _hpValueText.raycastTarget = false;
        }

        private IEnumerator SlideInAnimation()
        {
            if (_barPanel == null) yield break;

            Vector2 targetPos = new Vector2(0, -8f);
            Vector2 startPos = new Vector2(0, BAR_HEIGHT + 20f); // 화면 위로 숨김

            _barPanel.anchoredPosition = startPos;
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;

            float elapsed = 0f;
            while (elapsed < SLIDE_DURATION)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / SLIDE_DURATION;
                float easedT = 1f - (1f - t) * (1f - t) * (1f - t); // ease-out cubic

                _barPanel.anchoredPosition = Vector2.Lerp(startPos, targetPos, easedT);
                if (_canvasGroup != null)
                    _canvasGroup.alpha = easedT;

                yield return null;
            }

            _barPanel.anchoredPosition = targetPos;
            if (_canvasGroup != null) _canvasGroup.alpha = 1f;
            _slideCoroutine = null;
        }

        private IEnumerator FadeOutAnimation()
        {
            if (_canvasGroup == null) yield break;

            float elapsed = 0f;
            while (elapsed < FADE_DURATION)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = 1f - (elapsed / FADE_DURATION);
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
            _slideCoroutine = null;
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
