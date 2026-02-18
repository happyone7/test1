using System.Collections;
using Tesseract.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Soulspire.UI
{
    /// <summary>
    /// 인게임 HUD 콤보 카운터.
    /// ComboSystem.ComboCount를 폴링하여 콤보 5+ 시 화면 중앙 상단에 텍스트를 표시한다.
    ///
    /// 색상 규칙 (PPT 명세):
    /// - 5~9: 녹색 (#2BFF88)
    /// - 10~19: 주황 (#FF9A3D)
    /// - 20+: 빨강 (#FF4D5A)
    ///
    /// 콤보 리셋(0) 시 페이드아웃 애니메이션.
    /// 콤보 증가 시 스케일 팝 애니메이션.
    /// </summary>
    public class ComboCounterUI : MonoBehaviour
    {
        // === 콤보 구간별 색상 ===
        private static readonly Color ColorComboGreen  = new Color32(0x2B, 0xFF, 0x88, 0xFF); // #2BFF88
        private static readonly Color ColorComboOrange = new Color32(0xFF, 0x9A, 0x3D, 0xFF); // #FF9A3D
        private static readonly Color ColorComboRed    = new Color32(0xFF, 0x4D, 0x5A, 0xFF); // #FF4D5A

        [Header("콤보 텍스트")]
        [Tooltip("콤보 카운터를 표시할 Text 컴포넌트. null이면 런타임 자동 생성.")]
        public Text comboText;

        [Header("설정")]
        [Tooltip("콤보 표시 최소 임계값")]
        public int displayThreshold = 5;

        [Tooltip("페이드아웃 시간 (초)")]
        public float fadeOutDuration = 0.5f;

        [Tooltip("스케일 팝 크기")]
        public float popScale = 1.3f;

        [Tooltip("스케일 팝 복귀 시간 (초)")]
        public float popDuration = 0.15f;

        // === 내부 상태 ===
        private int _lastComboCount;
        private bool _isVisible;
        private Coroutine _fadeCoroutine;
        private Coroutine _popCoroutine;
        private CanvasGroup _canvasGroup;
        private RectTransform _textRect;

        void Start()
        {
            // 텍스트가 미할당이면 런타임 생성
            if (comboText == null)
            {
                CreateComboText();
            }

            _textRect = comboText.GetComponent<RectTransform>();

            // CanvasGroup 확보 (페이드아웃용)
            _canvasGroup = comboText.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = comboText.gameObject.AddComponent<CanvasGroup>();

            // 초기 비활성
            SetVisible(false);
            _lastComboCount = 0;
        }

        void Update()
        {
            if (!Singleton<Core.ComboSystem>.HasInstance) return;

            int currentCombo = Core.ComboSystem.Instance.ComboCount;

            // 콤보가 증가했을 때
            if (currentCombo > _lastComboCount && currentCombo >= displayThreshold)
            {
                UpdateComboDisplay(currentCombo);

                if (!_isVisible)
                    SetVisible(true);

                // 스케일 팝 애니메이션
                TriggerPop();
            }
            // 콤보가 리셋되었을 때 (0으로 떨어짐)
            else if (currentCombo == 0 && _lastComboCount > 0 && _isVisible)
            {
                TriggerFadeOut();
            }
            // 콤보가 유지되지만 임계값 미만으로 떨어진 경우
            else if (currentCombo < displayThreshold && currentCombo > 0 && _isVisible)
            {
                TriggerFadeOut();
            }

            _lastComboCount = currentCombo;
        }

        // =====================================================================
        // 표시 갱신
        // =====================================================================

        private void UpdateComboDisplay(int combo)
        {
            if (comboText == null) return;

            // 페이드아웃 진행 중이면 중단
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            // 텍스트 갱신
            if (combo >= 20)
                comboText.text = $"MEGA COMBO x{combo}!";
            else
                comboText.text = $"COMBO x{combo}!";

            // 색상 갱신
            comboText.color = GetComboColor(combo);

            // 완전 불투명
            if (_canvasGroup != null)
                _canvasGroup.alpha = 1f;
        }

        private Color GetComboColor(int combo)
        {
            if (combo >= 20) return ColorComboRed;
            if (combo >= 10) return ColorComboOrange;
            return ColorComboGreen;
        }

        // =====================================================================
        // 애니메이션
        // =====================================================================

        private void TriggerPop()
        {
            if (_popCoroutine != null)
                StopCoroutine(_popCoroutine);
            _popCoroutine = StartCoroutine(PopAnimation());
        }

        private IEnumerator PopAnimation()
        {
            if (_textRect == null) yield break;

            Vector3 targetScale = Vector3.one;
            Vector3 bigScale = Vector3.one * popScale;

            // 즉시 확대
            _textRect.localScale = bigScale;

            // popDuration 동안 원래 크기로 복귀
            float elapsed = 0f;
            while (elapsed < popDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / popDuration;
                // ease-out
                t = 1f - (1f - t) * (1f - t);
                _textRect.localScale = Vector3.Lerp(bigScale, targetScale, t);
                yield return null;
            }

            _textRect.localScale = targetScale;
            _popCoroutine = null;
        }

        private void TriggerFadeOut()
        {
            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeOutAnimation());
        }

        private IEnumerator FadeOutAnimation()
        {
            if (_canvasGroup == null) yield break;

            float startAlpha = _canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeOutDuration;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            SetVisible(false);
            _fadeCoroutine = null;
        }

        // =====================================================================
        // 유틸리티
        // =====================================================================

        private void SetVisible(bool visible)
        {
            _isVisible = visible;
            if (comboText != null)
                comboText.gameObject.SetActive(visible);
        }

        /// <summary>
        /// 런타임에서 콤보 텍스트를 자동 생성합니다.
        /// InGameUI 하위에 배치되며, 화면 중앙 상단에 위치합니다.
        /// </summary>
        private void CreateComboText()
        {
            var go = new GameObject("ComboText", typeof(RectTransform));
            go.transform.SetParent(transform, false);

            var rect = go.GetComponent<RectTransform>();
            // 화면 상단 중앙
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(400, 60);
            rect.anchoredPosition = new Vector2(0, -80); // 상단에서 80px 아래

            comboText = go.AddComponent<Text>();
            comboText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            comboText.fontSize = 32;
            comboText.fontStyle = FontStyle.Bold;
            comboText.alignment = TextAnchor.MiddleCenter;
            comboText.color = ColorComboGreen;
            comboText.raycastTarget = false;
            comboText.horizontalOverflow = HorizontalWrapMode.Overflow;
            comboText.verticalOverflow = VerticalWrapMode.Overflow;

            // 가독성을 위한 그림자
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            shadow.effectDistance = new Vector2(2, -2);
        }

        /// <summary>
        /// 외부에서 강제로 콤보 표시를 리셋합니다 (런 시작/종료 시).
        /// </summary>
        public void ResetDisplay()
        {
            _lastComboCount = 0;

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
            if (_popCoroutine != null)
            {
                StopCoroutine(_popCoroutine);
                _popCoroutine = null;
            }

            SetVisible(false);

            if (_canvasGroup != null)
                _canvasGroup.alpha = 1f;
            if (_textRect != null)
                _textRect.localScale = Vector3.one;
        }
    }
}
