using UnityEngine;
using UnityEngine.UI;

namespace Nodebreaker.UI
{
    /// <summary>
    /// 타워 정보 툴팁 (PPT 명세 슬라이드 5-2)
    /// 350x300px, 타워 인접, 테두리 #37B6FF 2px
    /// 이름+레벨: 16pt 굵게, #37B6FF
    /// 스탯: 12pt, 라벨=#AFC3E8, 값=#D8E4FF 굵게
    /// 판매: 11pt, #FF9A3D
    /// </summary>
    public class TowerInfoTooltip : MonoBehaviour
    {
        [Header("기본 정보")]
        public Text nameText;
        public Text levelText;

        [Header("스탯")]
        public Text damageText;
        public Text attackSpeedText;
        public Text rangeText;

        [Header("판매")]
        public Text sellText;
        public Button sellButton;

        [Header("닫기")]
        public Button closeButton;

        private Tower.Tower _targetTower;

        void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (sellButton != null)
                sellButton.onClick.AddListener(OnSell);

            gameObject.SetActive(false);
        }

        public void Show(Tower.Tower tower)
        {
            _targetTower = tower;

            if (tower == null || tower.data == null)
            {
                Hide();
                return;
            }

            gameObject.SetActive(true);

            var data = tower.data;
            int level = tower.Level;

            if (nameText != null)
                nameText.text = data.towerName;

            if (levelText != null)
                levelText.text = $"Lv.{level}";

            if (damageText != null)
                damageText.text = $"{data.GetDamage(level):F1}";

            if (attackSpeedText != null)
                attackSpeedText.text = $"{data.GetAttackSpeed(level):F2}/s";

            if (rangeText != null)
                rangeText.text = $"{data.GetRange(level):F1}";

            if (sellText != null)
            {
                int sellValue = Mathf.RoundToInt(data.placeCost * 0.5f);
                sellText.text = $"판매: +{sellValue} Bit";
            }

            // 타워 위치 근처에 표시
            PositionNearTower(tower.transform.position);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _targetTower = null;
        }

        private void PositionNearTower(Vector3 worldPos)
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            var cam = Camera.main;
            if (cam == null) return;

            Vector2 screenPos = cam.WorldToScreenPoint(worldPos);
            var rectTransform = GetComponent<RectTransform>();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPos,
                canvas.worldCamera,
                out Vector2 localPos
            );

            // 약간 오른쪽 위로 오프셋
            localPos += new Vector2(180, 50);
            rectTransform.anchoredPosition = localPos;
        }

        private void OnSell()
        {
            if (_targetTower == null) return;

            int sellValue = Mathf.RoundToInt(_targetTower.data.placeCost * 0.5f);
            Debug.Log($"[TowerInfoTooltip] 타워 판매: {_targetTower.data.towerName}, +{sellValue} Bit");

            // TODO: RunManager에 Bit 추가, 타워 제거 로직 연결
            Destroy(_targetTower.gameObject);
            Hide();
        }
    }
}
