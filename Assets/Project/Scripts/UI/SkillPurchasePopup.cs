using System;
using Tesseract.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Nodebreaker.UI
{
    /// <summary>
    /// 스킬 노드 구매 확인 팝업.
    /// Bit 반복구매형과 Core 1회구매형 두 가지 레이아웃을 지원한다.
    /// </summary>
    public class SkillPurchasePopup : MonoBehaviour
    {
        [Header("공통")]
        public GameObject popupRoot;
        public Image popupBackground;
        public Image overlayBackground;  // 반투명 오버레이 (팝업 외부 클릭 감지)
        public Button overlayCloseButton; // 오버레이 클릭 시 닫기
        public Button closeButton;

        [Header("노드 정보")]
        public Image nodeIcon;
        public Text nodeNameText;
        public Text levelChangeText;    // "Lv 3 -> Lv 4"

        [Header("Bit 노드 - Before/After")]
        public GameObject beforeAfterPanel;
        public Text beforeValueText;
        public Text afterValueText;

        [Header("Core 노드 - 효과 설명")]
        public GameObject effectPanel;
        public Text effectDescText;

        [Header("비용/자원")]
        public Text costText;
        public Text balanceText;        // "보유: 250 Bit (구매 후 140 Bit)"

        [Header("버튼")]
        public Button purchaseButton;
        public Text purchaseButtonText;

        [Header("잠김 안내")]
        public GameObject lockedInfoPanel;
        public Text lockedReasonText;
        public Button goToPrereqButton;
        public Text goToPrereqButtonText;

        [Header("색상")]
        public Color normalCostColor = new Color(0.87f, 0.89f, 0.82f, 1f);
        public Color insufficientCostColor = new Color(1f, 0.3f, 0.35f, 1f);
        public Color afterValueColor = new Color(0.25f, 0.83f, 0.44f, 1f);  // 녹색
        public Color levelArrowColor = new Color(0.25f, 0.83f, 0.44f, 1f);

        private Data.SkillNodeData _currentData;
        private Action _onPurchased;
        private Action<string> _onGoToPrereq;  // 선행 노드로 이동 콜백

        void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (overlayCloseButton != null)
                overlayCloseButton.onClick.AddListener(Hide);

            if (purchaseButton != null)
                purchaseButton.onClick.AddListener(OnPurchaseClicked);

            if (goToPrereqButton != null)
                goToPrereqButton.onClick.AddListener(OnGoToPrereqClicked);

            Hide();
        }

        /// <summary>
        /// 노드 데이터로 팝업을 표시한다.
        /// </summary>
        public void Show(Data.SkillNodeData data, SkillNodeState nodeState,
            Action onPurchased = null, Action<string> onGoToPrereq = null)
        {
            _currentData = data;
            _onPurchased = onPurchased;
            _onGoToPrereq = onGoToPrereq;

            if (data == null)
            {
                Hide();
                return;
            }

            if (popupRoot != null) popupRoot.SetActive(true);
            if (overlayBackground != null) overlayBackground.gameObject.SetActive(true);

            // 잠김 상태: 안내 패널만 표시
            if (nodeState == SkillNodeState.Locked)
            {
                ShowLockedInfo(data);
                return;
            }

            // 최대 레벨: 간단 메시지
            if (nodeState == SkillNodeState.Maxed)
            {
                ShowMaxedInfo(data);
                return;
            }

            // 일반 구매 팝업
            ShowPurchaseInfo(data);
        }

        public void Hide()
        {
            if (popupRoot != null) popupRoot.SetActive(false);
            if (overlayBackground != null) overlayBackground.gameObject.SetActive(false);
            _currentData = null;
        }

        public bool IsVisible => popupRoot != null && popupRoot.activeSelf;

        private void ShowPurchaseInfo(Data.SkillNodeData data)
        {
            if (lockedInfoPanel != null) lockedInfoPanel.SetActive(false);

            int level = 0;
            int totalBit = 0;
            int totalCore = 0;

            if (Singleton<Core.MetaManager>.HasInstance)
            {
                var meta = Core.MetaManager.Instance;
                level = meta.GetSkillLevel(data.skillId);
                totalBit = meta.TotalBit;
                totalCore = meta.TotalCore;
            }

            bool isMaxLevel = level >= data.maxLevel;
            int cost = isMaxLevel ? 0 : data.GetCost(level);
            bool isCoreNode = data.resourceType == Data.SkillResourceType.Core;
            int currentResource = isCoreNode ? totalCore : totalBit;
            string unit = isCoreNode ? "Core" : "Bit";
            bool canAfford = currentResource >= cost;

            // 아이콘
            if (nodeIcon != null && data.icon != null)
            {
                nodeIcon.sprite = data.icon;
                nodeIcon.preserveAspect = true;
            }

            // 이름
            if (nodeNameText != null)
                nodeNameText.text = data.skillName;

            // 레벨 변화
            if (levelChangeText != null)
            {
                if (isMaxLevel)
                    levelChangeText.text = $"Lv {level} (MAX)";
                else if (data.IsRepeatable)
                    levelChangeText.text = $"Lv {level} -> Lv {level + 1}";
                else
                    levelChangeText.text = level > 0 ? "해금 완료" : "해금";
            }

            // Bit 노드 (반복 구매형): Before/After 패널
            if (data.IsRepeatable)
            {
                if (beforeAfterPanel != null) beforeAfterPanel.SetActive(true);
                if (effectPanel != null) effectPanel.SetActive(false);

                float currentValue = data.GetTotalValue(level);
                if (beforeValueText != null)
                    beforeValueText.text = $"현재: +{currentValue:F1}";

                if (afterValueText != null)
                {
                    if (isMaxLevel)
                        afterValueText.text = "최대 레벨";
                    else
                    {
                        float nextValue = data.GetTotalValue(level + 1);
                        afterValueText.text = $"변경 후: +{nextValue:F1}";
                        afterValueText.color = afterValueColor;
                    }
                }
            }
            else
            {
                // Core 노드 (1회 구매형): 효과 설명 패널
                if (beforeAfterPanel != null) beforeAfterPanel.SetActive(false);
                if (effectPanel != null) effectPanel.SetActive(true);

                if (effectDescText != null)
                    effectDescText.text = data.description;
            }

            // 비용
            if (costText != null)
            {
                if (isMaxLevel)
                {
                    costText.text = "MAX";
                    costText.color = normalCostColor;
                }
                else
                {
                    costText.text = $"비용: {cost} {unit}";
                    costText.color = canAfford ? normalCostColor : insufficientCostColor;
                }
            }

            // 잔액
            if (balanceText != null)
            {
                if (isMaxLevel)
                    balanceText.text = "";
                else
                {
                    int remaining = currentResource - cost;
                    balanceText.text = $"보유: {currentResource:N0} {unit} (구매 후 {remaining:N0} {unit})";
                }
            }

            // 구매 버튼
            if (purchaseButton != null)
                purchaseButton.interactable = canAfford && !isMaxLevel;

            if (purchaseButtonText != null)
            {
                if (isMaxLevel)
                    purchaseButtonText.text = "최대";
                else
                    purchaseButtonText.text = data.IsRepeatable ? "구매" : "해금";
            }
        }

        private void ShowLockedInfo(Data.SkillNodeData data)
        {
            // 구매 관련 패널 숨김
            if (beforeAfterPanel != null) beforeAfterPanel.SetActive(false);
            if (effectPanel != null) effectPanel.SetActive(false);
            if (costText != null) costText.text = "";
            if (balanceText != null) balanceText.text = "";
            if (purchaseButton != null) purchaseButton.gameObject.SetActive(false);

            if (lockedInfoPanel != null) lockedInfoPanel.SetActive(true);

            // 아이콘/이름
            if (nodeIcon != null && data.icon != null)
            {
                nodeIcon.sprite = data.icon;
                nodeIcon.color = new Color(0.3f, 0.3f, 0.4f, 0.6f);
            }
            if (nodeNameText != null)
                nodeNameText.text = data.skillName;
            if (levelChangeText != null)
                levelChangeText.text = "";

            // 미충족 전제 조건 표시
            if (lockedReasonText != null && data.prerequisiteIds != null)
            {
                string reasons = "이 노드를 해금하려면:\n";
                string firstUnmetPrereq = null;

                foreach (var prereqId in data.prerequisiteIds)
                {
                    if (string.IsNullOrEmpty(prereqId)) continue;
                    bool met = Singleton<Core.MetaManager>.HasInstance &&
                               Core.MetaManager.Instance.GetSkillLevel(prereqId) > 0;

                    var prereqData = FindSkillData(prereqId);
                    string prereqName = prereqData != null ? prereqData.skillName : prereqId;

                    reasons += met
                        ? $"  - {prereqName} (완료)\n"
                        : $"  - {prereqName} (미완료)\n";

                    if (!met && firstUnmetPrereq == null)
                        firstUnmetPrereq = prereqId;
                }

                lockedReasonText.text = reasons;

                // "해당 노드로 이동" 버튼
                if (goToPrereqButton != null)
                {
                    goToPrereqButton.gameObject.SetActive(firstUnmetPrereq != null);
                    goToPrereqButton.tag = firstUnmetPrereq ?? "";
                }
            }
        }

        private void ShowMaxedInfo(Data.SkillNodeData data)
        {
            if (lockedInfoPanel != null) lockedInfoPanel.SetActive(false);
            if (beforeAfterPanel != null) beforeAfterPanel.SetActive(false);
            if (effectPanel != null) effectPanel.SetActive(false);

            if (nodeIcon != null && data.icon != null)
            {
                nodeIcon.sprite = data.icon;
                nodeIcon.color = Color.white;
            }
            if (nodeNameText != null)
                nodeNameText.text = data.skillName;
            if (levelChangeText != null)
                levelChangeText.text = "최대 레벨 도달!";
            if (costText != null)
                costText.text = "";
            if (balanceText != null)
                balanceText.text = "";

            if (purchaseButton != null)
                purchaseButton.interactable = false;
            if (purchaseButtonText != null)
                purchaseButtonText.text = "최대";
        }

        private void OnPurchaseClicked()
        {
            if (_currentData == null) return;

            if (Singleton<Core.MetaManager>.HasInstance)
            {
                bool success = Core.MetaManager.Instance.TryPurchaseSkill(_currentData.skillId);
                if (success)
                {
                    _onPurchased?.Invoke();

                    // 구매 후 팝업 갱신 (다음 레벨 정보로)
                    int newLevel = Core.MetaManager.Instance.GetSkillLevel(_currentData.skillId);
                    if (newLevel >= _currentData.maxLevel)
                    {
                        // 최대 레벨 도달 시 자동 닫힘
                        Hide();
                    }
                    else
                    {
                        // 다음 레벨 정보로 갱신 (연속 구매 가능)
                        ShowPurchaseInfo(_currentData);
                    }
                }
            }
        }

        private void OnGoToPrereqClicked()
        {
            if (goToPrereqButton == null) return;

            string prereqId = goToPrereqButton.tag;
            if (!string.IsNullOrEmpty(prereqId))
            {
                _onGoToPrereq?.Invoke(prereqId);
                Hide();
            }
        }

        private Data.SkillNodeData FindSkillData(string skillId)
        {
            if (!Singleton<Core.MetaManager>.HasInstance) return null;
            var meta = Core.MetaManager.Instance;
            if (meta.allSkills == null) return null;

            foreach (var skill in meta.allSkills)
            {
                if (skill != null && skill.skillId == skillId)
                    return skill;
            }
            return null;
        }
    }
}
