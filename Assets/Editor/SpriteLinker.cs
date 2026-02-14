using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Nodebreaker.Data;

namespace Nodebreaker.Editor
{
    /// <summary>
    /// 스프라이트 에셋을 ScriptableObject, 프리팹, UI 컴포넌트에 연결하는 에디터 도구.
    /// </summary>
    public static class SpriteLinker
    {
        #region Asset Paths

        // UI Frame sprites
        private const string PanelFramePath     = "Assets/Art/UI/Frames/panel_frame.png";
        private const string HpBarFramePath     = "Assets/Art/UI/Frames/hp_bar_frame.png";
        private const string HpBarFillPath      = "Assets/Art/UI/Frames/hp_bar_fill.png";
        private const string TowerSlotPath      = "Assets/Art/UI/Frames/tower_slot.png";
        private const string TooltipFramePath   = "Assets/Art/UI/Frames/tooltip_frame.png";
        private const string DropdownFramePath  = "Assets/Art/UI/Frames/dropdown_frame.png";

        // Button sprites
        private const string BtnBasicIdlePath     = "Assets/Art/UI/Buttons/btn_basic_idle.png";
        private const string BtnBasicHoverPath    = "Assets/Art/UI/Buttons/btn_basic_hover.png";
        private const string BtnBasicPressedPath  = "Assets/Art/UI/Buttons/btn_basic_pressed.png";
        private const string BtnBasicDisabledPath = "Assets/Art/UI/Buttons/btn_basic_disabled.png";
        private const string BtnAccentIdlePath     = "Assets/Art/UI/Buttons/btn_accent_idle.png";
        private const string BtnAccentHoverPath    = "Assets/Art/UI/Buttons/btn_accent_hover.png";
        private const string BtnAccentPressedPath  = "Assets/Art/UI/Buttons/btn_accent_pressed.png";
        private const string BtnAccentDisabledPath = "Assets/Art/UI/Buttons/btn_accent_disabled.png";

        // Icon sprites
        private const string IconBitPath  = "Assets/Art/UI/Icons/icon_bit.png";
        private const string IconCorePath = "Assets/Art/UI/Icons/icon_core.png";

        #endregion

        [MenuItem("Tools/Link Sprites to Assets")]
        public static void LinkSprites()
        {
            int linked = 0;

            // --- Tower ScriptableObjects ---
            linked += LinkTowerSprite("Assets/Project/ScriptableObjects/Towers/Tower_Arrow.asset",
                "Assets/Art/Sprites/Towers/ArrowTower/Final/ArrowTower_Level1_Minimal.png");
            linked += LinkTowerSprite("Assets/Project/ScriptableObjects/Towers/Tower_Cannon.asset",
                "Assets/Art/Sprites/Towers/CannonTower/Final/CannonTower_Level1.png");
            linked += LinkTowerSprite("Assets/Project/ScriptableObjects/Towers/Tower_Ice.asset",
                "Assets/Art/Sprites/Towers/IceTower/Final/IceTower_Level1.png");

            // --- Node ScriptableObjects ---
            linked += LinkNodeSprite("Assets/Project/ScriptableObjects/Nodes/Node_Bit.asset",
                "Assets/Art/Sprites/Monsters/Soul/Soul_Concept_A.png");
            linked += LinkNodeSprite("Assets/Project/ScriptableObjects/Nodes/Node_Quick.asset",
                "Assets/Art/Sprites/Monsters/Quick/Final/QuickNode.png");
            linked += LinkNodeSprite("Assets/Project/ScriptableObjects/Nodes/Node_Heavy.asset",
                "Assets/Art/Sprites/Monsters/Heavy/Final/HeavyNode.png");

            // --- Skill ScriptableObjects ---
            linked += LinkSkillSprite("Assets/Project/ScriptableObjects/Skills/Skill_AttackDamage.asset",
                "Assets/Art/UI/Hub/NB_Hub_AttackPower.png");
            linked += LinkSkillSprite("Assets/Project/ScriptableObjects/Skills/Skill_AttackSpeed.asset",
                "Assets/Art/UI/Hub/NB_Hub_AttackSpeed.png");
            linked += LinkSkillSprite("Assets/Project/ScriptableObjects/Skills/Skill_BaseHp.asset",
                "Assets/Art/UI/Hub/NB_Hub_BaseHP.png");

            // --- Prefab SpriteRenderers ---
            linked += LinkPrefabSprite("Assets/Project/Prefabs/Towers/Tower_Arrow.prefab",
                "Assets/Art/Sprites/Towers/ArrowTower/Final/ArrowTower_Level1_Minimal.png");
            linked += LinkPrefabSprite("Assets/Project/Prefabs/Towers/Tower_Cannon.prefab",
                "Assets/Art/Sprites/Towers/CannonTower/Final/CannonTower_Level1.png");
            linked += LinkPrefabSprite("Assets/Project/Prefabs/Towers/Tower_Ice.prefab",
                "Assets/Art/Sprites/Towers/IceTower/Final/IceTower_Level1.png");
            linked += LinkPrefabSprite("Assets/Project/Prefabs/Monsters/Monster_Bit.prefab",
                "Assets/Art/Sprites/Monsters/Soul/Soul_Concept_A.png");
            linked += LinkPrefabSprite("Assets/Project/Prefabs/Monsters/Monster_Quick.prefab",
                "Assets/Art/Sprites/Monsters/Quick/Final/QuickNode.png");
            linked += LinkPrefabSprite("Assets/Project/Prefabs/Monsters/Monster_Heavy.prefab",
                "Assets/Art/Sprites/Monsters/Heavy/Final/HeavyNode.png");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SpriteLinker] Linked {linked} sprite references.");
        }

        /// <summary>
        /// UI 씬 오브젝트에 새 UI 에셋 스프라이트를 연결.
        /// Image.type = Sliced (9-slice), Button Transition = SpriteSwap 적용.
        /// </summary>
        [MenuItem("Tools/Nodebreaker/Link UI Art Sprites to Scene")]
        public static void LinkUIArtSprites()
        {
            int linked = 0;

            // 스프라이트 로드
            var panelFrame     = LoadSprite(PanelFramePath);
            var hpBarFrame     = LoadSprite(HpBarFramePath);
            var hpBarFill      = LoadSprite(HpBarFillPath);
            var towerSlot      = LoadSprite(TowerSlotPath);
            var tooltipFrame   = LoadSprite(TooltipFramePath);
            var dropdownFrame  = LoadSprite(DropdownFramePath);
            var iconBit        = LoadSprite(IconBitPath);
            var iconCore       = LoadSprite(IconCorePath);

            var btnBasicIdle     = LoadSprite(BtnBasicIdlePath);
            var btnBasicHover    = LoadSprite(BtnBasicHoverPath);
            var btnBasicPressed  = LoadSprite(BtnBasicPressedPath);
            var btnBasicDisabled = LoadSprite(BtnBasicDisabledPath);
            var btnAccentIdle     = LoadSprite(BtnAccentIdlePath);
            var btnAccentHover    = LoadSprite(BtnAccentHoverPath);
            var btnAccentPressed  = LoadSprite(BtnAccentPressedPath);
            var btnAccentDisabled = LoadSprite(BtnAccentDisabledPath);

            // === InGameUI 연결 ===
            var inGameUI = Object.FindFirstObjectByType<UI.InGameUI>(FindObjectsInactive.Include);
            if (inGameUI != null)
            {
                // HP 바 프레임
                if (inGameUI.hpBarBackground != null && hpBarFrame != null)
                {
                    SetSlicedSprite(inGameUI.hpBarBackground, hpBarFrame);
                    linked++;
                    Debug.Log("[SpriteLinker] InGameUI.hpBarBackground -> hp_bar_frame");
                }

                // HP 바 채움
                if (inGameUI.hpBarFill != null && hpBarFill != null)
                {
                    inGameUI.hpBarFill.sprite = hpBarFill;
                    inGameUI.hpBarFill.type = Image.Type.Filled;
                    inGameUI.hpBarFill.fillMethod = Image.FillMethod.Horizontal;
                    linked++;
                    Debug.Log("[SpriteLinker] InGameUI.hpBarFill -> hp_bar_fill");
                }

                // 런 종료 패널 배경
                if (inGameUI.runEndPanelBody != null && panelFrame != null)
                {
                    SetSlicedSprite(inGameUI.runEndPanelBody, panelFrame);
                    linked++;
                    Debug.Log("[SpriteLinker] InGameUI.runEndPanelBody -> panel_frame");
                }

                // 배속/일시정지 버튼에 Basic 버튼 스프라이트 적용
                if (inGameUI.speedButtons != null && btnBasicIdle != null)
                {
                    foreach (var btn in inGameUI.speedButtons)
                    {
                        if (btn != null)
                        {
                            linked += SetupButtonSpriteSwap(btn, btnBasicIdle, btnBasicHover, btnBasicPressed, btnBasicDisabled);
                        }
                    }
                }
                if (inGameUI.pauseButton != null && btnBasicIdle != null)
                {
                    linked += SetupButtonSpriteSwap(inGameUI.pauseButton, btnBasicIdle, btnBasicHover, btnBasicPressed, btnBasicDisabled);
                }

                // Hub/Retry 버튼에 Accent 버튼 스프라이트 적용
                if (inGameUI.hubButton != null && btnAccentIdle != null)
                {
                    linked += SetupButtonSpriteSwap(inGameUI.hubButton, btnAccentIdle, btnAccentHover, btnAccentPressed, btnAccentDisabled);
                }
                if (inGameUI.retryButton != null && btnAccentIdle != null)
                {
                    linked += SetupButtonSpriteSwap(inGameUI.retryButton, btnAccentIdle, btnAccentHover, btnAccentPressed, btnAccentDisabled);
                }

                EditorUtility.SetDirty(inGameUI);
            }
            else
            {
                Debug.LogWarning("[SpriteLinker] InGameUI not found in scene.");
            }

            // === HubUI 연결 ===
            var hubUI = Object.FindFirstObjectByType<UI.HubUI>(FindObjectsInactive.Include);
            if (hubUI != null)
            {
                // 상세 패널 배경
                if (hubUI.detailPanel != null && panelFrame != null)
                {
                    var detailImage = hubUI.detailPanel.GetComponent<Image>();
                    if (detailImage != null)
                    {
                        SetSlicedSprite(detailImage, panelFrame);
                        linked++;
                        Debug.Log("[SpriteLinker] HubUI.detailPanel -> panel_frame");
                    }
                }

                // 구매 버튼 (Accent)
                if (hubUI.purchaseButton != null && btnAccentIdle != null)
                {
                    linked += SetupButtonSpriteSwap(hubUI.purchaseButton, btnAccentIdle, btnAccentHover, btnAccentPressed, btnAccentDisabled);
                }

                // 출격 버튼 (Accent)
                if (hubUI.startRunButton != null && btnAccentIdle != null)
                {
                    linked += SetupButtonSpriteSwap(hubUI.startRunButton, btnAccentIdle, btnAccentHover, btnAccentPressed, btnAccentDisabled);
                }

                // 설정/종료 버튼 (Basic)
                if (hubUI.settingsButton != null && btnBasicIdle != null)
                {
                    linked += SetupButtonSpriteSwap(hubUI.settingsButton, btnBasicIdle, btnBasicHover, btnBasicPressed, btnBasicDisabled);
                }
                if (hubUI.quitButton != null && btnBasicIdle != null)
                {
                    linked += SetupButtonSpriteSwap(hubUI.quitButton, btnBasicIdle, btnBasicHover, btnBasicPressed, btnBasicDisabled);
                }

                // 드롭다운 프레임
                if (hubUI.stageDropdown != null && dropdownFrame != null)
                {
                    var dropdownImage = hubUI.stageDropdown.GetComponent<Image>();
                    if (dropdownImage != null)
                    {
                        SetSlicedSprite(dropdownImage, dropdownFrame);
                        linked++;
                        Debug.Log("[SpriteLinker] HubUI.stageDropdown -> dropdown_frame");
                    }
                }

                EditorUtility.SetDirty(hubUI);
            }
            else
            {
                Debug.LogWarning("[SpriteLinker] HubUI not found in scene.");
            }

            // === TowerInfoTooltip 연결 ===
            var tooltip = Object.FindFirstObjectByType<UI.TowerInfoTooltip>(FindObjectsInactive.Include);
            if (tooltip != null && tooltipFrame != null)
            {
                var tooltipImage = tooltip.GetComponent<Image>();
                if (tooltipImage != null)
                {
                    SetSlicedSprite(tooltipImage, tooltipFrame);
                    linked++;
                    Debug.Log("[SpriteLinker] TowerInfoTooltip -> tooltip_frame");
                }

                // 판매 버튼 (Basic)
                if (tooltip.sellButton != null && btnBasicIdle != null)
                {
                    linked += SetupButtonSpriteSwap(tooltip.sellButton, btnBasicIdle, btnBasicHover, btnBasicPressed, btnBasicDisabled);
                }

                EditorUtility.SetDirty(tooltip);
            }

            // === TowerPurchasePanel 연결 ===
            var purchasePanel = Object.FindFirstObjectByType<UI.TowerPurchasePanel>(FindObjectsInactive.Include);
            if (purchasePanel != null && panelFrame != null)
            {
                var panelImage = purchasePanel.GetComponent<Image>();
                if (panelImage != null)
                {
                    SetSlicedSprite(panelImage, panelFrame);
                    linked++;
                    Debug.Log("[SpriteLinker] TowerPurchasePanel -> panel_frame");
                }
                EditorUtility.SetDirty(purchasePanel);
            }

            // === TitleScreenUI 연결 ===
            var titleUI = Object.FindFirstObjectByType<UI.TitleScreenUI>(FindObjectsInactive.Include);
            if (titleUI != null)
            {
                // TitleScreenUI의 버튼은 SerializeField로 private이므로 SerializedObject 사용
                var so = new SerializedObject(titleUI);

                var startBtnProp = so.FindProperty("startButton");
                var settingsBtnProp = so.FindProperty("settingsButton");
                var quitBtnProp = so.FindProperty("quitButton");

                if (startBtnProp != null && startBtnProp.objectReferenceValue is Button startBtn && btnAccentIdle != null)
                {
                    linked += SetupButtonSpriteSwap(startBtn, btnAccentIdle, btnAccentHover, btnAccentPressed, btnAccentDisabled);
                }
                if (settingsBtnProp != null && settingsBtnProp.objectReferenceValue is Button settingsBtn && btnBasicIdle != null)
                {
                    linked += SetupButtonSpriteSwap(settingsBtn, btnBasicIdle, btnBasicHover, btnBasicPressed, btnBasicDisabled);
                }
                if (quitBtnProp != null && quitBtnProp.objectReferenceValue is Button quitBtn && btnBasicIdle != null)
                {
                    linked += SetupButtonSpriteSwap(quitBtn, btnBasicIdle, btnBasicHover, btnBasicPressed, btnBasicDisabled);
                }

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(titleUI);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[SpriteLinker] UI Art sprite linking complete. Linked {linked} references.");
        }

        #region Helper Methods

        /// <summary>
        /// Image에 Sliced 스프라이트 설정 (9-slice용).
        /// </summary>
        private static void SetSlicedSprite(Image image, Sprite sprite)
        {
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1f;
        }

        /// <summary>
        /// Button에 SpriteSwap Transition 설정.
        /// 반환: 링크 성공 시 1, 실패 시 0.
        /// </summary>
        private static int SetupButtonSpriteSwap(Button button, Sprite idle, Sprite hover, Sprite pressed, Sprite disabled)
        {
            if (button == null || idle == null) return 0;

            // Image 컴포넌트에 idle 스프라이트 설정 (Sliced)
            var image = button.GetComponent<Image>();
            if (image != null)
            {
                SetSlicedSprite(image, idle);
            }

            // SpriteSwap Transition 설정
            button.transition = Selectable.Transition.SpriteSwap;
            var spriteState = new SpriteState
            {
                highlightedSprite = hover,
                pressedSprite = pressed,
                disabledSprite = disabled
            };
            button.spriteState = spriteState;

            EditorUtility.SetDirty(button);
            Debug.Log($"[SpriteLinker] Button '{button.name}' -> SpriteSwap ({idle.name}/{hover.name}/{pressed.name}/{disabled.name})");
            return 1;
        }

        /// <summary>
        /// 에셋 경로에서 Sprite 로드.
        /// </summary>
        private static Sprite LoadSprite(string assetPath)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null)
                Debug.LogWarning($"[SpriteLinker] Sprite not found: {assetPath}");
            return sprite;
        }

        static int LinkTowerSprite(string soPath, string spritePath)
        {
            var tower = AssetDatabase.LoadAssetAtPath<TowerData>(soPath);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (tower == null) { Debug.LogWarning($"[SpriteLinker] Tower SO not found: {soPath}"); return 0; }
            if (sprite == null) { Debug.LogWarning($"[SpriteLinker] Sprite not found: {spritePath}"); return 0; }

            tower.icon = sprite;
            EditorUtility.SetDirty(tower);
            Debug.Log($"[SpriteLinker] Tower '{tower.towerName}' icon linked to {spritePath}");
            return 1;
        }

        static int LinkNodeSprite(string soPath, string spritePath)
        {
            var node = AssetDatabase.LoadAssetAtPath<NodeData>(soPath);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (node == null) { Debug.LogWarning($"[SpriteLinker] Node SO not found: {soPath}"); return 0; }
            if (sprite == null) { Debug.LogWarning($"[SpriteLinker] Sprite not found: {spritePath}"); return 0; }

            node.icon = sprite;
            EditorUtility.SetDirty(node);
            Debug.Log($"[SpriteLinker] Node '{node.nodeName}' icon linked to {spritePath}");
            return 1;
        }

        static int LinkSkillSprite(string soPath, string spritePath)
        {
            var skill = AssetDatabase.LoadAssetAtPath<SkillNodeData>(soPath);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (skill == null) { Debug.LogWarning($"[SpriteLinker] Skill SO not found: {soPath}"); return 0; }
            if (sprite == null) { Debug.LogWarning($"[SpriteLinker] Sprite not found: {spritePath}"); return 0; }

            skill.icon = sprite;
            EditorUtility.SetDirty(skill);
            Debug.Log($"[SpriteLinker] Skill '{skill.skillName}' icon linked to {spritePath}");
            return 1;
        }

        static int LinkPrefabSprite(string prefabPath, string spritePath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (prefab == null) { Debug.LogWarning($"[SpriteLinker] Prefab not found: {prefabPath}"); return 0; }
            if (sprite == null) { Debug.LogWarning($"[SpriteLinker] Sprite not found: {spritePath}"); return 0; }

            var sr = prefab.GetComponentInChildren<SpriteRenderer>();
            if (sr == null)
            {
                Debug.LogWarning($"[SpriteLinker] No SpriteRenderer on prefab: {prefabPath}");
                return 0;
            }

            sr.sprite = sprite;
            EditorUtility.SetDirty(prefab);
            PrefabUtility.SavePrefabAsset(prefab);
            Debug.Log($"[SpriteLinker] Prefab '{prefab.name}' SpriteRenderer linked to {spritePath}");
            return 1;
        }

        #endregion
    }
}
