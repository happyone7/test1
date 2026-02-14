using Tesseract.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Nodebreaker.UI
{
    /// <summary>
    /// 스킬트리 노드 간 연결선.
    /// UI.Image (Stretched)로 직선을 그린다.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class SkillTreeConnectionLine : MonoBehaviour
    {
        [Header("색상 설정")]
        public Color activeColor = new Color(0f, 1f, 0.8f, 1f);          // #00FFCC 밝은 청록
        public Color semiActiveColor = new Color(0f, 1f, 0.8f, 0.4f);    // #00FFCC 40%
        public Color inactiveColor = new Color(0f, 0f, 0f, 0f);          // 투명 (숨김)

        [Header("두께")]
        public float activeThickness = 3f;
        public float semiActiveThickness = 2f;

        private Image _lineImage;
        private RectTransform _rectTransform;
        private SkillNodeUI _fromNode;
        private SkillNodeUI _toNode;

        void Awake()
        {
            _lineImage = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// 두 노드 사이의 연결선을 설정한다.
        /// </summary>
        public void Setup(SkillNodeUI from, SkillNodeUI to)
        {
            _fromNode = from;
            _toNode = to;

            UpdatePosition();
            Refresh();
        }

        /// <summary>
        /// 노드 위치에 맞춰 연결선 위치/크기/회전을 갱신한다.
        /// </summary>
        public void UpdatePosition()
        {
            if (_fromNode == null || _toNode == null) return;

            var fromRT = _fromNode.GetComponent<RectTransform>();
            var toRT = _toNode.GetComponent<RectTransform>();
            if (fromRT == null || toRT == null) return;

            Vector2 fromPos = fromRT.anchoredPosition;
            Vector2 toPos = toRT.anchoredPosition;

            // 중간점 계산
            Vector2 midPoint = (fromPos + toPos) * 0.5f;
            _rectTransform.anchoredPosition = midPoint;

            // 길이 계산
            float distance = Vector2.Distance(fromPos, toPos);
            // 피봇은 (0.5, 0.5) 기준
            _rectTransform.sizeDelta = new Vector2(distance, semiActiveThickness);

            // 각도 계산
            Vector2 direction = toPos - fromPos;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            _rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
        }

        /// <summary>
        /// 양쪽 노드의 구매 상태에 따라 연결선 스타일을 갱신한다.
        /// </summary>
        public void Refresh()
        {
            if (_fromNode == null || _toNode == null || _lineImage == null) return;

            bool fromPurchased = IsNodePurchased(_fromNode);
            bool toPurchased = IsNodePurchased(_toNode);
            bool fromVisible = _fromNode.gameObject.activeSelf;
            bool toVisible = _toNode.gameObject.activeSelf;

            if (fromPurchased && toPurchased)
            {
                // 양쪽 모두 구매 완료: 밝은 실선
                _lineImage.color = activeColor;
                _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, activeThickness);
                gameObject.SetActive(true);
            }
            else if ((fromPurchased || toPurchased) && fromVisible && toVisible)
            {
                // 한쪽만 구매 완료: 흐린 실선
                _lineImage.color = semiActiveColor;
                _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, semiActiveThickness);
                gameObject.SetActive(true);
            }
            else
            {
                // 양쪽 모두 미구매 또는 숨김: 보이지 않음
                gameObject.SetActive(false);
            }
        }

        private bool IsNodePurchased(SkillNodeUI node)
        {
            if (node == null || node.data == null) return false;
            if (!Singleton<Core.MetaManager>.HasInstance) return false;
            return Core.MetaManager.Instance.GetSkillLevel(node.data.skillId) > 0;
        }
    }
}
