using System;
using Tesseract.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Nodebreaker.UI
{
    /// <summary>
    /// 스킬 노드 상태
    /// </summary>
    public enum SkillNodeState
    {
        /// <summary>선행 노드가 하나도 구매되지 않음 - 보이지 않음</summary>
        Hidden,
        /// <summary>선행 노드 중 1개 이상 구매됨, 전제 조건 미충족</summary>
        Locked,
        /// <summary>전제 조건 충족 + 자원 충분</summary>
        Available,
        /// <summary>전제 조건 충족 + 자원 부족</summary>
        AvailableNoFunds,
        /// <summary>최소 1레벨 이상 구매됨</summary>
        Purchased,
        /// <summary>최대 레벨 도달</summary>
        Maxed
    }

    /// <summary>
    /// 스킬 노드 UI 컴포넌트.
    /// 아이콘, 이름, 레벨, 비용, 잠금 오버레이를 관리하고 상태에 따라 시각 표현을 변경한다.
    /// </summary>
    public class SkillNodeUI : MonoBehaviour
    {
        [Header("데이터")]
        public Data.SkillNodeData data;

        [Header("UI - 아이콘")]
        public Image iconImage;
        public Image borderImage;

        [Header("UI - 텍스트")]
        public Text nameText;
        public Text levelText;
        public Text costText;

        [Header("UI - 오버레이")]
        public GameObject lockedOverlay;
        public Image lockIcon;

        [Header("UI - 버튼")]
        public Button button;

        [Header("색상")]
        public Color borderLocked = new Color(0.35f, 0.35f, 0.5f, 0.8f);
        public Color borderAvailable = new Color(0f, 1f, 0.8f, 1f);       // 밝은 청록
        public Color borderPurchased = new Color(0.25f, 1f, 0.53f, 1f);   // 네온 그린
        public Color borderMaxed = new Color(1f, 0.85f, 0.3f, 1f);        // 금색
        public Color costNormal = new Color(0.87f, 0.89f, 0.82f, 1f);
        public Color costInsufficient = new Color(1f, 0.3f, 0.35f, 1f);   // 빨간색

        private Action<Data.SkillNodeData> _onSelected;
        private SkillNodeState _currentState = SkillNodeState.Hidden;

        /// <summary>현재 노드 상태</summary>
        public SkillNodeState CurrentState => _currentState;

        public void Initialize(Action<Data.SkillNodeData> onSelected)
        {
            _onSelected = onSelected;

            if (button != null)
                button.onClick.AddListener(OnClick);

            if (data != null && iconImage != null && data.icon != null)
                iconImage.sprite = data.icon;

            Refresh();
        }

        /// <summary>
        /// MetaManager의 데이터를 기반으로 노드 상태를 갱신한다.
        /// </summary>
        public void Refresh()
        {
            if (data == null) return;

            _currentState = CalculateState();
            ApplyVisualState();
        }

        private SkillNodeState CalculateState()
        {
            if (!Singleton<Core.MetaManager>.HasInstance)
                return SkillNodeState.Hidden;

            var meta = Core.MetaManager.Instance;
            int level = meta.GetSkillLevel(data.skillId);

            // 1. 최대 레벨 도달
            if (level >= data.maxLevel && data.maxLevel > 0)
                return SkillNodeState.Maxed;

            // 2. 최소 1레벨 구매됨
            if (level > 0)
                return SkillNodeState.Purchased;

            // 3. 노드가 보이는지 (선행 노드 중 1개 이상 구매)
            if (!meta.IsNodeVisible(data.skillId))
                return SkillNodeState.Hidden;

            // 4. 전제 조건 충족 여부
            if (!meta.ArePrerequisitesMet(data.skillId))
                return SkillNodeState.Locked;

            // 5. 자원 충분 여부
            if (meta.CanAfford(data.skillId))
                return SkillNodeState.Available;
            else
                return SkillNodeState.AvailableNoFunds;
        }

        private void ApplyVisualState()
        {
            int level = 0;
            if (Singleton<Core.MetaManager>.HasInstance)
                level = Core.MetaManager.Instance.GetSkillLevel(data.skillId);

            switch (_currentState)
            {
                case SkillNodeState.Hidden:
                    gameObject.SetActive(false);
                    return;

                case SkillNodeState.Locked:
                    gameObject.SetActive(true);
                    SetBorderColor(borderLocked);
                    SetIconTint(new Color(0.3f, 0.3f, 0.4f, 0.6f));
                    if (lockedOverlay != null) lockedOverlay.SetActive(true);
                    if (button != null) button.interactable = true;
                    SetLevelText("", false);
                    SetCostText("", costNormal);
                    break;

                case SkillNodeState.Available:
                    gameObject.SetActive(true);
                    SetBorderColor(borderAvailable);
                    SetIconTint(Color.white);
                    if (lockedOverlay != null) lockedOverlay.SetActive(false);
                    if (button != null) button.interactable = true;
                    SetLevelText(data.IsRepeatable ? $"Lv.{level}/{data.maxLevel}" : "", false);
                    SetCostText(GetCostString(level), costNormal);
                    break;

                case SkillNodeState.AvailableNoFunds:
                    gameObject.SetActive(true);
                    SetBorderColor(borderAvailable);
                    SetIconTint(Color.white);
                    if (lockedOverlay != null) lockedOverlay.SetActive(false);
                    if (button != null) button.interactable = true;
                    SetLevelText(data.IsRepeatable ? $"Lv.{level}/{data.maxLevel}" : "", false);
                    SetCostText(GetCostString(level), costInsufficient);
                    break;

                case SkillNodeState.Purchased:
                    gameObject.SetActive(true);
                    SetBorderColor(borderPurchased);
                    SetIconTint(Color.white);
                    if (lockedOverlay != null) lockedOverlay.SetActive(false);
                    if (button != null) button.interactable = true;
                    SetLevelText(data.IsRepeatable ? $"Lv.{level}/{data.maxLevel}" : "ON", true);
                    if (data.IsRepeatable)
                        SetCostText(GetCostString(level), costNormal);
                    else
                        SetCostText("", costNormal);
                    break;

                case SkillNodeState.Maxed:
                    gameObject.SetActive(true);
                    SetBorderColor(borderMaxed);
                    SetIconTint(Color.white);
                    if (lockedOverlay != null) lockedOverlay.SetActive(false);
                    if (button != null) button.interactable = false;
                    SetLevelText("MAX", true);
                    SetCostText("", costNormal);
                    break;
            }

            if (nameText != null)
                nameText.text = data.skillName;
        }

        private string GetCostString(int currentLevel)
        {
            int cost = data.GetCost(currentLevel);
            string unit = data.resourceType == Data.SkillResourceType.Core ? "Core" : "Bit";
            return $"{cost} {unit}";
        }

        private void SetBorderColor(Color color)
        {
            if (borderImage != null)
                borderImage.color = color;
        }

        private void SetIconTint(Color color)
        {
            if (iconImage != null)
                iconImage.color = color;
        }

        private void SetLevelText(string text, bool highlight)
        {
            if (levelText != null)
            {
                levelText.text = text;
                levelText.color = highlight
                    ? (data.resourceType == Data.SkillResourceType.Core
                        ? borderMaxed
                        : borderAvailable)
                    : costNormal;
            }
        }

        private void SetCostText(string text, Color color)
        {
            if (costText != null)
            {
                costText.text = text;
                costText.color = color;
            }
        }

        private void OnClick()
        {
            _onSelected?.Invoke(data);
        }
    }
}
