using UnityEngine;

namespace Nodebreaker.UI
{
    /// <summary>
    /// UI 스프라이트 에셋 참조를 중앙 관리하는 ScriptableObject.
    /// 런타임에서 동적 UI 생성 시 사용.
    /// 에디터: Assets/Project/ScriptableObjects/UI/UISprites.asset
    /// </summary>
    [CreateAssetMenu(fileName = "UISprites", menuName = "Nodebreaker/UI Sprites")]
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
        public Sprite iconBit;     // 소울(Soul) 아이콘
        public Sprite iconCore;    // 정수(Essence) 아이콘

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
