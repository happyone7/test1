using UnityEngine;
using UnityEngine.Serialization;

namespace Soulspire.UI
{
    /// <summary>
    /// UI 스프라이트 에셋 참조를 중앙 관리하는 ScriptableObject.
    /// 런타임에서 동적 UI 생성 시 사용.
    /// 에디터: Assets/Project/ScriptableObjects/UI/UISprites.asset
    /// </summary>
    [CreateAssetMenu(fileName = "UISprites", menuName = "Soulspire/UI Sprites")]
    public class UISprites : ScriptableObject
    {
        [Header("프레임")]
        public Sprite panelFrame;
        public Sprite hpBarFrame;
        public Sprite hpBarFill;
        public Sprite towerSlot;
        public Sprite tooltipFrame;
        public Sprite dropdownFrame;

        [Header("버튼 - Basic")]
        public Sprite btnBasicIdle;
        public Sprite btnBasicHover;
        public Sprite btnBasicPressed;
        public Sprite btnBasicDisabled;

        [Header("버튼 - Accent")]
        public Sprite btnAccentIdle;
        public Sprite btnAccentHover;
        public Sprite btnAccentPressed;
        public Sprite btnAccentDisabled;

        [Header("아이콘")]
        [FormerlySerializedAs("iconBit")]
        public Sprite iconSoul;     // 소울(Soul) 아이콘
        [FormerlySerializedAs("iconCore")]
        public Sprite iconCoreFragment;    // 정수(Essence) 아이콘

        [Header("배경")]
        public Sprite titleBackground;   // 타이틀 배경 (TitleBG_01)
        public Sprite hubBackground;     // 허브 배경 (HubBG_03_dimmed)

        [Header("로고")]
        public Sprite logo;              // Soulspire 로고 (SoulspireLogo_02)

        /// <summary>
        /// Button에 Basic SpriteSwap 설정 적용.
        /// </summary>
        public void ApplyBasicButton(UnityEngine.UI.Button button)
        {
            ApplyButtonSprites(button, btnBasicIdle, btnBasicHover, btnBasicPressed, btnBasicDisabled);
        }

        /// <summary>
        /// Button에 Accent SpriteSwap 설정 적용.
        /// </summary>
        public void ApplyAccentButton(UnityEngine.UI.Button button)
        {
            ApplyButtonSprites(button, btnAccentIdle, btnAccentHover, btnAccentPressed, btnAccentDisabled);
        }

        /// <summary>
        /// Image에 9-slice Panel Frame 설정.
        /// </summary>
        public void ApplyPanelFrame(UnityEngine.UI.Image image)
        {
            if (image == null || panelFrame == null) return;
            image.sprite = panelFrame;
            image.type = UnityEngine.UI.Image.Type.Sliced;
        }

        /// <summary>
        /// Image에 9-slice Tooltip Frame 설정.
        /// </summary>
        public void ApplyTooltipFrame(UnityEngine.UI.Image image)
        {
            if (image == null || tooltipFrame == null) return;
            image.sprite = tooltipFrame;
            image.type = UnityEngine.UI.Image.Type.Sliced;
        }

        /// <summary>
        /// Image에 9-slice HP Bar Frame 설정.
        /// </summary>
        public void ApplyHpBarFrame(UnityEngine.UI.Image image)
        {
            if (image == null || hpBarFrame == null) return;
            image.sprite = hpBarFrame;
            image.type = UnityEngine.UI.Image.Type.Sliced;
        }

        /// <summary>
        /// Image에 HP Bar Fill 설정 (Filled).
        /// </summary>
        public void ApplyHpBarFill(UnityEngine.UI.Image image)
        {
            if (image == null || hpBarFill == null) return;
            image.sprite = hpBarFill;
            image.type = UnityEngine.UI.Image.Type.Filled;
            image.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
        }

        /// <summary>
        /// Image에 9-slice Tower Slot 설정.
        /// </summary>
        public void ApplyTowerSlot(UnityEngine.UI.Image image)
        {
            if (image == null || towerSlot == null) return;
            image.sprite = towerSlot;
            image.type = UnityEngine.UI.Image.Type.Sliced;
        }

        /// <summary>
        /// Image에 9-slice Dropdown Frame 설정.
        /// </summary>
        public void ApplyDropdownFrame(UnityEngine.UI.Image image)
        {
            if (image == null || dropdownFrame == null) return;
            image.sprite = dropdownFrame;
            image.type = UnityEngine.UI.Image.Type.Sliced;
        }

        /// <summary>
        /// Image에 배경 이미지 설정 (Simple, 전체 표시).
        /// </summary>
        public void ApplyBackground(UnityEngine.UI.Image image, bool isHub)
        {
            if (image == null) return;
            Sprite bg = isHub ? hubBackground : titleBackground;
            if (bg == null) return;
            image.sprite = bg;
            image.type = UnityEngine.UI.Image.Type.Simple;
            image.preserveAspect = false;
            image.color = Color.white;
        }

        /// <summary>
        /// Image에 로고 설정.
        /// </summary>
        public void ApplyLogo(UnityEngine.UI.Image image)
        {
            if (image == null || logo == null) return;
            image.sprite = logo;
            image.type = UnityEngine.UI.Image.Type.Simple;
            image.preserveAspect = true;
            image.color = Color.white;
        }

        private static void ApplyButtonSprites(
            UnityEngine.UI.Button button,
            Sprite idle, Sprite hover, Sprite pressed, Sprite disabled)
        {
            if (button == null || idle == null) return;

            var image = button.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                image.sprite = idle;
                image.type = UnityEngine.UI.Image.Type.Sliced;
            }

            button.transition = UnityEngine.UI.Selectable.Transition.SpriteSwap;
            var spriteState = new UnityEngine.UI.SpriteState
            {
                highlightedSprite = hover,
                pressedSprite = pressed,
                disabledSprite = disabled
            };
            button.spriteState = spriteState;
        }
    }
}
