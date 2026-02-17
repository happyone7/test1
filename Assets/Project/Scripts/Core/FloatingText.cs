using UnityEngine;

namespace Soulspire.Core
{
    /// <summary>
    /// 월드 공간에서 텍스트가 위로 떠오르며 페이드아웃되는 컴포넌트.
    /// FeedbackManager.SpawnDamageText/SpawnSoulText에서 생성됩니다.
    /// 0.8초 동안 위로 이동 + 알파 감소 후 자동 파괴됩니다.
    /// </summary>
    public class FloatingText : MonoBehaviour
    {
        [Header("연출 설정")]
        public float duration = 0.8f;
        public float floatSpeed = 1.5f;

        float _timer;
        Color _baseColor;
        TextMesh _textMesh;
        bool _initialized;

        /// <summary>
        /// 텍스트와 색상을 설정하고 애니메이션을 시작합니다.
        /// </summary>
        public void Initialize(string text, Color color)
        {
            _textMesh = GetComponent<TextMesh>();
            if (_textMesh == null)
            {
                Debug.LogWarning("[FloatingText] TextMesh 컴포넌트를 찾을 수 없음");
                Destroy(gameObject);
                return;
            }

            _textMesh.text = text;
            _textMesh.color = color;
            _baseColor = color;
            _timer = 0f;
            _initialized = true;
        }

        void Update()
        {
            if (!_initialized) return;

            _timer += Time.unscaledDeltaTime;

            // 위로 떠오르기
            transform.position += Vector3.up * floatSpeed * Time.unscaledDeltaTime;

            // 알파 페이드아웃 (후반부에 가속)
            float progress = _timer / duration;
            float alpha = 1f - Mathf.Pow(progress, 2f); // 제곱 커브로 자연스러운 페이드아웃

            if (_textMesh != null)
            {
                _textMesh.color = new Color(
                    _baseColor.r,
                    _baseColor.g,
                    _baseColor.b,
                    Mathf.Clamp01(alpha)
                );
            }

            // 스케일 약간 증가 (팝업 느낌)
            if (progress < 0.15f)
            {
                float scale = 1f + progress * 2f; // 처음 15% 동안 1.0 -> 1.3 스케일업
                transform.localScale = new Vector3(scale, scale, 1f);
            }

            // 수명 종료
            if (_timer >= duration)
            {
                Destroy(gameObject);
            }
        }
    }
}
