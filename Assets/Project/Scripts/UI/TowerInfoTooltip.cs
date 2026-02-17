using Tesseract.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Soulspire.UI
{
    /// <summary>
    /// 타워 정보 툴팁 (PPT 명세 슬라이드 5-2)
    /// 350x300px, 타워 인접, 테두리 #37B6FF 2px
    /// 이름+레벨: 16pt 굵게, #37B6FF
    /// 스탯: 12pt, 라벨=#AFC3E8, 값=#D8E4FF 굵게
    /// 판매: 11pt, #FF9A3D
    /// 업그레이드: 비용 표시, 최대 레벨 시 MAX 표시
    /// </summary>
    public class TowerInfoTooltip : MonoBehaviour
    {
        // 색상 팔레트
        private static readonly Color ColorNeonBlue = new Color32(0x37, 0xB6, 0xFF, 0xFF);
        private static readonly Color ColorNeonGreen = new Color32(0x2B, 0xFF, 0x88, 0xFF);
        private static readonly Color ColorRed = new Color32(0xFF, 0x4D, 0x5A, 0xFF);
        private static readonly Color ColorTextMain = new Color32(0xD8, 0xE4, 0xFF, 0xFF);
        private static readonly Color ColorTextSub = new Color32(0xAF, 0xC3, 0xE8, 0xFF);
        private static readonly Color ColorYellow = new Color32(0xFF, 0xD8, 0x4D, 0xFF);

        [Header("기본 정보")]
        public Text nameText;
        public Text levelText;

        [Header("스탯")]
        public Text damageText;
        public Text attackSpeedText;
        public Text rangeText;

        [Header("업그레이드")]
        public Button upgradeButton;
        public Text upgradeButtonText;
        public Text upgradeCostText;

        [Header("판매")]
        public Text sellText;
        public Button sellButton;

        [Header("닫기")]
        public Button closeButton;

        [Header("UI 스프라이트")]
        public UISprites uiSprites;

        private Tower.Tower _targetTower;

        void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (sellButton != null)
                sellButton.onClick.AddListener(OnSell);

            if (upgradeButton != null)
                upgradeButton.onClick.AddListener(OnUpgrade);

            // UI 스프라이트 적용
            if (uiSprites != null)
            {
                var bgImage = GetComponent<Image>();
                uiSprites.ApplyTooltipFrame(bgImage);
                uiSprites.ApplyBasicButton(closeButton);
                uiSprites.ApplyBasicButton(sellButton);
                uiSprites.ApplyAccentButton(upgradeButton);
            }

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
            RefreshDisplay();

            // 타워 위치 근처에 표시
            PositionNearTower(tower.transform.position);
        }

        /// <summary>
        /// 현재 타겟 타워의 정보로 표시를 갱신합니다.
        /// </summary>
        private void RefreshDisplay()
        {
            if (_targetTower == null || _targetTower.data == null) return;

            var data = _targetTower.data;
            int level = _targetTower.Level;

            if (nameText != null)
            {
                nameText.text = data.towerName;
                nameText.color = ColorNeonBlue;
            }

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
                sellText.text = $"판매: +{sellValue} Soul";
            }

            // 업그레이드 버튼 상태
            RefreshUpgradeButton();
        }

        private void RefreshUpgradeButton()
        {
            if (upgradeButton == null) return;
            if (_targetTower == null || _targetTower.data == null) return;

            var data = _targetTower.data;
            int level = _targetTower.Level;
            int cost = data.GetUpgradeCost(level);
            bool isMaxLevel = cost < 0;

            if (isMaxLevel)
            {
                // 최대 레벨
                upgradeButton.interactable = false;
                if (upgradeButtonText != null)
                {
                    upgradeButtonText.text = "MAX";
                    upgradeButtonText.color = ColorTextSub;
                }
                if (upgradeCostText != null)
                    upgradeCostText.gameObject.SetActive(false);
            }
            else
            {
                int currentBit = Singleton<Core.RunManager>.HasInstance
                    ? Core.RunManager.Instance.SoulEarned : 0;
                bool canAfford = currentBit >= cost;

                upgradeButton.interactable = canAfford;

                if (upgradeButtonText != null)
                {
                    upgradeButtonText.text = "업그레이드";
                    upgradeButtonText.color = canAfford ? ColorNeonGreen : ColorTextSub;
                }

                if (upgradeCostText != null)
                {
                    upgradeCostText.gameObject.SetActive(true);
                    upgradeCostText.text = $"{cost} Soul";
                    upgradeCostText.color = canAfford ? ColorYellow : ColorRed;
                }
            }
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

        private void OnUpgrade()
        {
            if (_targetTower == null) return;

            bool success = _targetTower.UpgradeWithSoul();
            if (success)
            {
                Debug.Log($"[TowerInfoTooltip] 업그레이드 성공: {_targetTower.data.towerName} Lv{_targetTower.Level}");
                RefreshDisplay();
            }
            else
            {
                Debug.Log("[TowerInfoTooltip] 업그레이드 실패: Bit 부족 또는 최대 레벨");
            }
        }

        private void OnSell()
        {
            if (_targetTower == null) return;

            int sellValue = Mathf.RoundToInt(_targetTower.data.placeCost * 0.5f);
            Debug.Log($"[TowerInfoTooltip] 타워 판매: {_targetTower.data.towerName}, +{sellValue} Soul");

            // RunManager에 Bit 추가
            if (Singleton<Core.RunManager>.HasInstance)
                Core.RunManager.Instance.AddSoul(sellValue);

            Destroy(_targetTower.gameObject);
            Hide();
        }
    }
}
