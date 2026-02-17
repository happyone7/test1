using System;
using Tesseract.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Soulspire.UI
{
    /// <summary>
    /// 개별 스킬 노드 UI 컴포넌트.
    /// - 아이콘 표시 (SkillNodeData.icon)
    /// - 현재 레벨 / 최대 레벨 텍스트
    /// - 잠금 상태 표시 (prerequisite 미충족 시)
    /// - 클릭 시 상세 패널 업데이트
    /// - 구매 가능 여부에 따른 시각적 상태 변화
    /// </summary>
    public class SkillNodeUI : MonoBehaviour
    {
        [Header("데이터")]
        public Data.SkillNodeData data;

        [Header("UI")]
        public Image iconImage;
        public Text levelText;
        public Button button;
        public GameObject lockedOverlay;

        [Header("시각 상태")]
        public Image backgroundImage;       // 노드 배경 (상태별 색상 변경)
        public Image borderImage;           // 노드 테두리 (상태별 색상 변경)
        public Text lockedText;             // 잠금 시 "잠금" 텍스트

        [Header("UI 스프라이트")]
        public UISprites uiSprites;

        // 색상 (HubUI 팔레트 참조)
        private static readonly Color ColorPurchasableBg    = new Color32(0x1A, 0x30, 0x20, 0xFF); // 어두운 초록 배경
        private static readonly Color ColorPurchasableBorder = new Color32(0x2B, 0xFF, 0x88, 0xFF); // 네온 초록
        private static readonly Color ColorMaxedBg          = new Color32(0x20, 0x18, 0x30, 0xFF); // 어두운 보라 배경
        private static readonly Color ColorMaxedBorder      = new Color32(0xB8, 0x6C, 0xFF, 0xFF); // 네온 보라
        private static readonly Color ColorLockedBg         = new Color32(0x10, 0x14, 0x1C, 0xFF); // 매우 어두운 배경
        private static readonly Color ColorLockedBorder     = new Color32(0x3A, 0x40, 0x50, 0xFF); // 어두운 테두리
        private static readonly Color ColorNormalBg         = new Color32(0x12, 0x1A, 0x2A, 0xFF); // 기본 패널 색
        private static readonly Color ColorNormalBorder     = new Color32(0x5B, 0x6B, 0x8A, 0xFF); // 기본 테두리
        private static readonly Color ColorLockedIcon       = new Color32(0x50, 0x50, 0x50, 0xFF); // 잠금 시 아이콘 어둡게
        private static readonly Color ColorTextDisabled     = new Color32(0x60, 0x68, 0x80, 0xFF); // 비활성 텍스트

        private Action<Data.SkillNodeData> _onSelected;

        public void Initialize(Action<Data.SkillNodeData> onSelected)
        {
            _onSelected = onSelected;

            if (button != null)
                button.onClick.AddListener(OnClick);

            if (data != null && iconImage != null && data.icon != null)
                iconImage.sprite = data.icon;

            Refresh();
        }

        public void Refresh()
        {
            if (data == null) return;

            int level = 0;
            bool canPurchase = false;
            bool isLocked = false;

            if (Singleton<Core.MetaManager>.HasInstance)
            {
                var meta = Core.MetaManager.Instance;
                level = meta.GetSkillLevel(data.skillId);
                canPurchase = meta.CanPurchaseSkill(data.skillId);
                isLocked = !ArePrerequisitesMet(meta);
            }

            bool isMaxLevel = level >= data.maxLevel;

            // 레벨 텍스트
            if (levelText != null)
            {
                levelText.text = $"Lv.{level}/{data.maxLevel}";
                levelText.color = isLocked ? ColorTextDisabled : Color.white;
            }

            // 잠금 오버레이
            if (lockedOverlay != null)
                lockedOverlay.SetActive(isLocked);

            if (lockedText != null)
            {
                lockedText.gameObject.SetActive(isLocked);
                if (isLocked)
                    lockedText.text = "잠금";
            }

            // 아이콘 색상 (잠금 시 어둡게)
            if (iconImage != null)
                iconImage.color = isLocked ? ColorLockedIcon : Color.white;

            // 버튼 인터랙션 (잠금 시 비활성)
            if (button != null)
                button.interactable = !isLocked;

            // 시각적 상태: 잠금 > 최대 레벨 > 구매 가능 > 일반
            ApplyVisualState(isLocked, isMaxLevel, canPurchase);
        }

        /// <summary>
        /// prerequisite 스킬들이 모두 1레벨 이상인지 확인
        /// </summary>
        private bool ArePrerequisitesMet(Core.MetaManager meta)
        {
            if (data.prerequisiteIds == null || data.prerequisiteIds.Length == 0)
                return true;

            foreach (var prereqId in data.prerequisiteIds)
            {
                if (string.IsNullOrEmpty(prereqId)) continue;
                if (meta.GetSkillLevel(prereqId) <= 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 노드의 시각적 상태를 업데이트 (배경/테두리 색상)
        /// </summary>
        private void ApplyVisualState(bool isLocked, bool isMaxLevel, bool canPurchase)
        {
            Color bgColor;
            Color borderColor;

            if (isLocked)
            {
                bgColor = ColorLockedBg;
                borderColor = ColorLockedBorder;
            }
            else if (isMaxLevel)
            {
                bgColor = ColorMaxedBg;
                borderColor = ColorMaxedBorder;
            }
            else if (canPurchase)
            {
                bgColor = ColorPurchasableBg;
                borderColor = ColorPurchasableBorder;
            }
            else
            {
                bgColor = ColorNormalBg;
                borderColor = ColorNormalBorder;
            }

            if (backgroundImage != null)
                backgroundImage.color = bgColor;

            if (borderImage != null)
                borderImage.color = borderColor;

            // Outline 컴포넌트가 있으면 테두리 색상도 적용
            var outline = GetComponent<Outline>();
            if (outline != null)
                outline.effectColor = borderColor;
        }

        private void OnClick()
        {
            _onSelected?.Invoke(data);
        }
    }
}
