using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Nodebreaker.Data;
using Nodebreaker.UI;

namespace Nodebreaker.Editor
{
    /// <summary>
    /// Hub 씬에 스킬트리 UI를 구축하는 에디터 도구.
    /// 기획서(SkillTree_Spec.md)의 18개 노드를 그리드 좌표에 따라 배치하고,
    /// 줌/패닝/연결선/구매팝업까지 한 번에 생성한다.
    /// </summary>
    public class SkillTreeSetup
    {
        // 다크 판타지 색상 팔레트
        static readonly Color ColorMainBg       = new Color32(0x0B, 0x0F, 0x1A, 0xFF);
        static readonly Color ColorPanel        = new Color32(0x12, 0x1A, 0x2A, 0xFF);
        static readonly Color ColorBrightPanel  = new Color32(0x1A, 0x24, 0x3A, 0xFF);
        static readonly Color ColorNeonGreen    = new Color32(0x2B, 0xFF, 0x88, 0xFF);
        static readonly Color ColorNeonBlue     = new Color32(0x37, 0xB6, 0xFF, 0xFF);
        static readonly Color ColorNeonPurple   = new Color32(0xB8, 0x6C, 0xFF, 0xFF);
        static readonly Color ColorRed          = new Color32(0xFF, 0x4D, 0x5A, 0xFF);
        static readonly Color ColorYellowGold   = new Color32(0xFF, 0xD8, 0x4D, 0xFF);
        static readonly Color ColorTextPrimary  = new Color32(0xD8, 0xE4, 0xFF, 0xFF);
        static readonly Color ColorTextSecondary= new Color32(0xAF, 0xC3, 0xE8, 0xFF);
        static readonly Color ColorBorder       = new Color32(0x5B, 0x6B, 0x8A, 0xFF);
        static readonly Color ColorDarkGreen    = new Color32(0x15, 0x30, 0x25, 0xFF);
        static readonly Color ColorOverlay      = new Color32(0x05, 0x08, 0x12, 0xCC);

        // 그리드 셀 크기 (노드 + 여백)
        const float GridCellSize = 180f;

        // 노드 크기
        const float NodeWidth = 120f;
        const float NodeHeight = 140f;

        [MenuItem("Tools/Nodebreaker/Setup Skill Tree UI")]
        static void SetupSkillTreeUI()
        {
            // HubUI 찾기
            var hubUI = Object.FindFirstObjectByType<HubUI>(FindObjectsInactive.Include);
            if (hubUI == null)
            {
                Debug.LogError("[SkillTreeSetup] HubUI 컴포넌트를 찾을 수 없습니다.");
                return;
            }

            var hubPanel = hubUI.gameObject;

            // 기존 스킬트리 컴포넌트 제거
            RemoveExistingSkillTree(hubPanel.transform);

            // SkillNodeData SO 로드 또는 생성
            var skillDataMap = LoadOrCreateSkillData();

            // 1. 스킬트리 영역 생성 (기존 SkillArea 활용)
            var skillArea = hubPanel.transform.Find("SkillArea");
            if (skillArea == null)
            {
                Debug.LogError("[SkillTreeSetup] SkillArea를 찾을 수 없습니다. Rebuild Hub UI를 먼저 실행하세요.");
                return;
            }

            // 2. SkillTreeUI 컴포넌트 추가
            var skillTreeUI = skillArea.gameObject.GetComponent<SkillTreeUI>();
            if (skillTreeUI == null)
                skillTreeUI = skillArea.gameObject.AddComponent<SkillTreeUI>();

            // ScrollRect 참조
            var scrollRect = skillArea.GetComponent<ScrollRect>();

            // Viewport 찾기 또는 생성
            var viewportTransform = skillArea.Find("Viewport");
            if (viewportTransform == null)
            {
                // SkillArea 자체를 viewport로 사용
                var viewportObj = CreateChild(skillArea, "Viewport");
                SetupRect(viewportObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var viewportImg = viewportObj.AddComponent<Image>();
                viewportImg.color = Color.clear;
                var mask = viewportObj.AddComponent<Mask>();
                mask.showMaskGraphic = false;
                viewportTransform = viewportObj.transform;

                // ScrollRect viewport 연결
                if (scrollRect != null)
                    scrollRect.viewport = viewportObj.GetComponent<RectTransform>();
            }

            // Content RectTransform 찾기 또는 생성
            var contentTransform = skillArea.Find("Content");
            if (contentTransform == null)
            {
                contentTransform = skillArea.Find("Viewport/Content");
            }
            if (contentTransform == null)
            {
                var contentObj = CreateChild(viewportTransform, "Content");
                var contentRect = contentObj.GetComponent<RectTransform>();
                contentRect.anchorMin = new Vector2(0.5f, 0.5f);
                contentRect.anchorMax = new Vector2(0.5f, 0.5f);
                contentRect.pivot = new Vector2(0.5f, 0.5f);
                contentRect.sizeDelta = new Vector2(2400, 2400);  // 넓은 트리 맵
                contentRect.anchoredPosition = Vector2.zero;
                contentTransform = contentObj.transform;

                if (scrollRect != null)
                    scrollRect.content = contentRect;
            }
            else
            {
                // 기존 Content 크기 확장
                var contentRect = contentTransform.GetComponent<RectTransform>();
                contentRect.sizeDelta = new Vector2(2400, 2400);
            }

            // 기존 Content 자식 제거 (CoreNode, SkillNode_*, Line_* 등)
            DestroyChildrenAll(contentTransform);

            // 3. 연결선 컨테이너 생성
            var connectionContainer = CreateChild(contentTransform, "ConnectionLines");
            var connectionContainerRect = connectionContainer.GetComponent<RectTransform>();
            connectionContainerRect.anchorMin = new Vector2(0.5f, 0.5f);
            connectionContainerRect.anchorMax = new Vector2(0.5f, 0.5f);
            connectionContainerRect.pivot = new Vector2(0.5f, 0.5f);
            connectionContainerRect.sizeDelta = Vector2.zero;
            connectionContainerRect.anchoredPosition = Vector2.zero;

            // 4. 연결선 프리팹 생성
            var connectionLinePrefab = CreateConnectionLinePrefab();

            // 5. 노드 생성
            var nodeUIList = new List<SkillNodeUI>();
            foreach (var kvp in skillDataMap)
            {
                var skillData = kvp.Value;
                var nodeUI = CreateSkillNode(contentTransform, skillData);
                nodeUIList.Add(nodeUI);
            }

            // 6. SkillTreeUI 설정
            skillTreeUI.treeContent = contentTransform.GetComponent<RectTransform>();
            skillTreeUI.viewport = viewportTransform != null
                ? viewportTransform.GetComponent<RectTransform>()
                : skillArea.GetComponent<RectTransform>();
            skillTreeUI.skillNodes = nodeUIList.ToArray();
            skillTreeUI.connectionLinePrefab = connectionLinePrefab;
            skillTreeUI.connectionContainer = connectionContainerRect;
            skillTreeUI.gridCellSize = GridCellSize;

            // 7. HubUI에 SkillTreeUI 연결
            hubUI.skillTreeUI = skillTreeUI;

            // 8. 구매 팝업 생성
            var purchasePopup = CreatePurchasePopup(hubPanel.transform);
            hubUI.purchasePopup = purchasePopup;

            // ScrollRect의 기본 스크롤 비활성화 (SkillTreeUI가 직접 제어)
            if (scrollRect != null)
            {
                scrollRect.horizontal = false;
                scrollRect.vertical = false;
                scrollRect.enabled = false;
            }

            // 마무리
            EditorUtility.SetDirty(hubUI);
            EditorUtility.SetDirty(hubPanel);

            Debug.Log($"[SkillTreeSetup] 스킬트리 UI 생성 완료! 노드 {nodeUIList.Count}개 배치");
        }

        #region 노드 생성

        static SkillNodeUI CreateSkillNode(Transform parent, SkillNodeData data)
        {
            var nodeObj = CreateChild(parent, $"SkillNode_{data.skillId}");
            var nodeRect = nodeObj.GetComponent<RectTransform>();
            nodeRect.anchorMin = new Vector2(0.5f, 0.5f);
            nodeRect.anchorMax = new Vector2(0.5f, 0.5f);
            nodeRect.pivot = new Vector2(0.5f, 0.5f);
            nodeRect.sizeDelta = new Vector2(NodeWidth, NodeHeight);

            // 그리드 좌표 -> 픽셀 위치
            nodeRect.anchoredPosition = new Vector2(
                data.gridPosition.x * GridCellSize,
                data.gridPosition.y * GridCellSize
            );

            // 테두리 (Border) - 전체 노드 배경
            var borderImg = nodeObj.AddComponent<Image>();
            borderImg.color = ColorBorder;

            // 내부 배경
            var innerBg = CreateChild(nodeObj.transform, "InnerBg");
            SetupRect(innerBg, Vector2.zero, Vector2.one, new Vector2(2, 2), new Vector2(-2, -2));
            var innerBgImg = innerBg.AddComponent<Image>();
            innerBgImg.color = ColorBrightPanel;
            innerBgImg.raycastTarget = false;

            // 아이콘
            var iconObj = CreateChild(nodeObj.transform, "Icon");
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 1f);
            iconRect.anchorMax = new Vector2(0.5f, 1f);
            iconRect.pivot = new Vector2(0.5f, 1f);
            iconRect.anchoredPosition = new Vector2(0, -10);
            iconRect.sizeDelta = new Vector2(48, 48);
            var iconImg = iconObj.AddComponent<Image>();
            if (data.icon != null)
                iconImg.sprite = data.icon;
            iconImg.preserveAspect = true;
            iconImg.color = Color.white;
            iconImg.raycastTarget = false;

            // 이름 텍스트
            var nameObj = CreateChild(nodeObj.transform, "NameText");
            var nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.3f);
            nameRect.anchorMax = new Vector2(1, 0.5f);
            nameRect.offsetMin = new Vector2(4, 0);
            nameRect.offsetMax = new Vector2(-4, 0);
            var nameText = nameObj.AddComponent<Text>();
            nameText.text = data.skillName;
            nameText.fontSize = 11;
            nameText.fontStyle = FontStyle.Bold;
            nameText.color = ColorTextPrimary;
            nameText.alignment = TextAnchor.MiddleCenter;
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.raycastTarget = false;

            // 레벨 텍스트
            var levelObj = CreateChild(nodeObj.transform, "LevelText");
            var levelRect = levelObj.GetComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0, 0.15f);
            levelRect.anchorMax = new Vector2(1, 0.3f);
            levelRect.offsetMin = new Vector2(4, 0);
            levelRect.offsetMax = new Vector2(-4, 0);
            var levelText = levelObj.AddComponent<Text>();
            levelText.text = data.IsRepeatable ? "Lv.0" : "";
            levelText.fontSize = 10;
            levelText.color = data.resourceType == SkillResourceType.Core
                ? ColorYellowGold : ColorNeonBlue;
            levelText.alignment = TextAnchor.MiddleCenter;
            levelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            levelText.raycastTarget = false;

            // 비용 텍스트
            var costObj = CreateChild(nodeObj.transform, "CostText");
            var costRect = costObj.GetComponent<RectTransform>();
            costRect.anchorMin = new Vector2(0, 0);
            costRect.anchorMax = new Vector2(1, 0.15f);
            costRect.offsetMin = new Vector2(4, 2);
            costRect.offsetMax = new Vector2(-4, 0);
            var costText = costObj.AddComponent<Text>();
            string unit = data.resourceType == SkillResourceType.Core ? "Core" : "Bit";
            costText.text = $"{data.baseCost} {unit}";
            costText.fontSize = 9;
            costText.color = ColorTextSecondary;
            costText.alignment = TextAnchor.MiddleCenter;
            costText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            costText.raycastTarget = false;

            // 잠금 오버레이
            var lockOverlay = CreateChild(nodeObj.transform, "LockedOverlay");
            SetupRect(lockOverlay, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var lockBg = lockOverlay.AddComponent<Image>();
            lockBg.color = new Color(0.05f, 0.05f, 0.1f, 0.7f);
            lockBg.raycastTarget = false;

            var lockIconObj = CreateChild(lockOverlay.transform, "LockIcon");
            var lockIconRect = lockIconObj.GetComponent<RectTransform>();
            lockIconRect.anchorMin = new Vector2(0.5f, 0.5f);
            lockIconRect.anchorMax = new Vector2(0.5f, 0.5f);
            lockIconRect.sizeDelta = new Vector2(32, 32);
            var lockIconText = lockIconObj.AddComponent<Text>();
            lockIconText.text = "\u26BF"; // Lock icon placeholder
            lockIconText.fontSize = 24;
            lockIconText.color = ColorBorder;
            lockIconText.alignment = TextAnchor.MiddleCenter;
            lockIconText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            lockIconText.raycastTarget = false;

            lockOverlay.SetActive(false);

            // 버튼
            var button = nodeObj.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.9f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            button.colors = colors;

            // SkillNodeUI 컴포넌트
            var skillNodeUI = nodeObj.AddComponent<SkillNodeUI>();
            skillNodeUI.data = data;
            skillNodeUI.iconImage = iconImg;
            skillNodeUI.borderImage = borderImg;
            skillNodeUI.nameText = nameText;
            skillNodeUI.levelText = levelText;
            skillNodeUI.costText = costText;
            skillNodeUI.lockedOverlay = lockOverlay;
            skillNodeUI.button = button;

            return skillNodeUI;
        }

        #endregion

        #region 연결선 프리팹

        static GameObject CreateConnectionLinePrefab()
        {
            var lineObj = new GameObject("ConnectionLine", typeof(RectTransform));
            var lineRect = lineObj.GetComponent<RectTransform>();
            lineRect.anchorMin = new Vector2(0.5f, 0.5f);
            lineRect.anchorMax = new Vector2(0.5f, 0.5f);
            lineRect.pivot = new Vector2(0.5f, 0.5f);
            lineRect.sizeDelta = new Vector2(100, 2);

            var lineImg = lineObj.AddComponent<Image>();
            lineImg.color = new Color(0f, 1f, 0.8f, 0.4f);
            lineImg.raycastTarget = false;

            var connectionLine = lineObj.AddComponent<SkillTreeConnectionLine>();

            return lineObj;
        }

        #endregion

        #region 구매 팝업

        static SkillPurchasePopup CreatePurchasePopup(Transform parent)
        {
            // 오버레이 (전체 화면)
            var overlayObj = CreateChild(parent, "PurchasePopupOverlay");
            SetupRect(overlayObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var overlayImg = overlayObj.AddComponent<Image>();
            overlayImg.color = ColorOverlay;
            var overlayBtn = overlayObj.AddComponent<Button>();
            overlayBtn.transition = Selectable.Transition.None;

            // 팝업 루트 (360x380, 화면 중앙)
            var popupObj = CreateChild(overlayObj.transform, "PopupRoot");
            var popupRect = popupObj.GetComponent<RectTransform>();
            popupRect.anchorMin = new Vector2(0.5f, 0.5f);
            popupRect.anchorMax = new Vector2(0.5f, 0.5f);
            popupRect.pivot = new Vector2(0.5f, 0.5f);
            popupRect.sizeDelta = new Vector2(360, 380);
            var popupBg = popupObj.AddComponent<Image>();
            popupBg.color = ColorPanel;
            var popupOutline = popupObj.AddComponent<Outline>();
            popupOutline.effectColor = ColorBorder;
            popupOutline.effectDistance = new Vector2(2, -2);

            // 닫기 버튼 (우상단 X)
            var closeObj = CreateChild(popupObj.transform, "CloseButton");
            var closeRect = closeObj.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1, 1);
            closeRect.anchorMax = new Vector2(1, 1);
            closeRect.pivot = new Vector2(1, 1);
            closeRect.anchoredPosition = new Vector2(-5, -5);
            closeRect.sizeDelta = new Vector2(30, 30);
            var closeBg = closeObj.AddComponent<Image>();
            closeBg.color = ColorBrightPanel;
            var closeBtn = closeObj.AddComponent<Button>();
            var closeTextObj = CreateChild(closeObj.transform, "Text");
            SetupRect(closeTextObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var closeText = closeTextObj.AddComponent<Text>();
            closeText.text = "X";
            closeText.fontSize = 16;
            closeText.fontStyle = FontStyle.Bold;
            closeText.color = ColorTextPrimary;
            closeText.alignment = TextAnchor.MiddleCenter;
            closeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 아이콘 (좌상단)
            var iconObj = CreateChild(popupObj.transform, "NodeIcon");
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 1);
            iconRect.anchorMax = new Vector2(0, 1);
            iconRect.pivot = new Vector2(0, 1);
            iconRect.anchoredPosition = new Vector2(15, -15);
            iconRect.sizeDelta = new Vector2(48, 48);
            var iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            // 이름 텍스트
            var nameObj = CreateChild(popupObj.transform, "NodeNameText");
            var nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 1);
            nameRect.anchorMax = new Vector2(1, 1);
            nameRect.pivot = new Vector2(0, 1);
            nameRect.anchoredPosition = new Vector2(75, -15);
            nameRect.sizeDelta = new Vector2(-120, 25);
            var nameText = nameObj.AddComponent<Text>();
            nameText.text = "스킬 이름";
            nameText.fontSize = 16;
            nameText.fontStyle = FontStyle.Bold;
            nameText.color = ColorTextPrimary;
            nameText.alignment = TextAnchor.MiddleLeft;
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.raycastTarget = false;

            // 레벨 변화 텍스트
            var levelObj = CreateChild(popupObj.transform, "LevelChangeText");
            var levelRect = levelObj.GetComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0, 1);
            levelRect.anchorMax = new Vector2(1, 1);
            levelRect.pivot = new Vector2(0, 1);
            levelRect.anchoredPosition = new Vector2(75, -42);
            levelRect.sizeDelta = new Vector2(-120, 20);
            var levelText = levelObj.AddComponent<Text>();
            levelText.text = "Lv 0 -> Lv 1";
            levelText.fontSize = 14;
            levelText.color = ColorNeonGreen;
            levelText.alignment = TextAnchor.MiddleLeft;
            levelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            levelText.raycastTarget = false;

            // Before/After 패널
            var beforeAfterPanel = CreateChild(popupObj.transform, "BeforeAfterPanel");
            var baRect = beforeAfterPanel.GetComponent<RectTransform>();
            baRect.anchorMin = new Vector2(0, 0.35f);
            baRect.anchorMax = new Vector2(1, 0.75f);
            baRect.offsetMin = new Vector2(15, 0);
            baRect.offsetMax = new Vector2(-15, 0);

            var beforeObj = CreateChild(beforeAfterPanel.transform, "BeforeText");
            SetupRect(beforeObj, new Vector2(0, 0), new Vector2(0.5f, 1), new Vector2(5, 5), new Vector2(-5, -5));
            var beforeText = beforeObj.AddComponent<Text>();
            beforeText.text = "현재: +0";
            beforeText.fontSize = 13;
            beforeText.color = ColorTextSecondary;
            beforeText.alignment = TextAnchor.MiddleCenter;
            beforeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            beforeText.raycastTarget = false;

            var afterObj = CreateChild(beforeAfterPanel.transform, "AfterText");
            SetupRect(afterObj, new Vector2(0.5f, 0), new Vector2(1, 1), new Vector2(5, 5), new Vector2(-5, -5));
            var afterText = afterObj.AddComponent<Text>();
            afterText.text = "변경 후: +10";
            afterText.fontSize = 13;
            afterText.color = ColorNeonGreen;
            afterText.alignment = TextAnchor.MiddleCenter;
            afterText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            afterText.raycastTarget = false;

            // 효과 설명 패널 (Core 노드용)
            var effectPanel = CreateChild(popupObj.transform, "EffectPanel");
            var effectRect = effectPanel.GetComponent<RectTransform>();
            effectRect.anchorMin = new Vector2(0, 0.35f);
            effectRect.anchorMax = new Vector2(1, 0.75f);
            effectRect.offsetMin = new Vector2(15, 0);
            effectRect.offsetMax = new Vector2(-15, 0);
            var effectDescObj = CreateChild(effectPanel.transform, "EffectDescText");
            SetupRect(effectDescObj, Vector2.zero, Vector2.one, new Vector2(5, 5), new Vector2(-5, -5));
            var effectDescText = effectDescObj.AddComponent<Text>();
            effectDescText.text = "효과 설명";
            effectDescText.fontSize = 13;
            effectDescText.color = ColorTextPrimary;
            effectDescText.alignment = TextAnchor.UpperLeft;
            effectDescText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            effectDescText.raycastTarget = false;
            effectPanel.SetActive(false);

            // 비용 텍스트
            var costObj = CreateChild(popupObj.transform, "CostText");
            var costRect = costObj.GetComponent<RectTransform>();
            costRect.anchorMin = new Vector2(0, 0.18f);
            costRect.anchorMax = new Vector2(1, 0.3f);
            costRect.offsetMin = new Vector2(15, 0);
            costRect.offsetMax = new Vector2(-15, 0);
            var costText = costObj.AddComponent<Text>();
            costText.text = "비용: 50 Bit";
            costText.fontSize = 14;
            costText.color = ColorYellowGold;
            costText.alignment = TextAnchor.MiddleLeft;
            costText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            costText.raycastTarget = false;

            // 잔액 텍스트
            var balanceObj = CreateChild(popupObj.transform, "BalanceText");
            var balanceRect = balanceObj.GetComponent<RectTransform>();
            balanceRect.anchorMin = new Vector2(0, 0.08f);
            balanceRect.anchorMax = new Vector2(1, 0.18f);
            balanceRect.offsetMin = new Vector2(15, 0);
            balanceRect.offsetMax = new Vector2(-15, 0);
            var balanceText = balanceObj.AddComponent<Text>();
            balanceText.text = "보유: 250 Bit (구매 후 200 Bit)";
            balanceText.fontSize = 12;
            balanceText.color = ColorTextSecondary;
            balanceText.alignment = TextAnchor.MiddleLeft;
            balanceText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            balanceText.raycastTarget = false;

            // 구매 버튼
            var purchaseBtnObj = CreateChild(popupObj.transform, "PurchaseButton");
            var purchaseBtnRect = purchaseBtnObj.GetComponent<RectTransform>();
            purchaseBtnRect.anchorMin = new Vector2(0.05f, 0);
            purchaseBtnRect.anchorMax = new Vector2(0.45f, 0.08f);
            purchaseBtnRect.offsetMin = new Vector2(0, 8);
            purchaseBtnRect.offsetMax = new Vector2(0, -2);
            // Expand the button to be more clickable
            purchaseBtnRect.anchorMax = new Vector2(0.45f, 0.12f);
            var purchaseBtnBg = purchaseBtnObj.AddComponent<Image>();
            purchaseBtnBg.color = ColorDarkGreen;
            var purchaseBtnOutline = purchaseBtnObj.AddComponent<Outline>();
            purchaseBtnOutline.effectColor = ColorNeonGreen;
            var purchaseBtn = purchaseBtnObj.AddComponent<Button>();

            var purchaseBtnTextObj = CreateChild(purchaseBtnObj.transform, "Text");
            SetupRect(purchaseBtnTextObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var purchaseBtnText = purchaseBtnTextObj.AddComponent<Text>();
            purchaseBtnText.text = "구매";
            purchaseBtnText.fontSize = 14;
            purchaseBtnText.fontStyle = FontStyle.Bold;
            purchaseBtnText.color = ColorNeonGreen;
            purchaseBtnText.alignment = TextAnchor.MiddleCenter;
            purchaseBtnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 잠김 안내 패널
            var lockedPanel = CreateChild(popupObj.transform, "LockedInfoPanel");
            var lockedRect = lockedPanel.GetComponent<RectTransform>();
            lockedRect.anchorMin = new Vector2(0, 0.1f);
            lockedRect.anchorMax = new Vector2(1, 0.75f);
            lockedRect.offsetMin = new Vector2(15, 0);
            lockedRect.offsetMax = new Vector2(-15, 0);

            var lockedReasonObj = CreateChild(lockedPanel.transform, "LockedReasonText");
            SetupRect(lockedReasonObj, new Vector2(0, 0.3f), Vector2.one, new Vector2(5, 5), new Vector2(-5, -5));
            var lockedReasonText = lockedReasonObj.AddComponent<Text>();
            lockedReasonText.text = "이 노드를 해금하려면:\n- ???";
            lockedReasonText.fontSize = 13;
            lockedReasonText.color = ColorTextSecondary;
            lockedReasonText.alignment = TextAnchor.UpperLeft;
            lockedReasonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            lockedReasonText.raycastTarget = false;

            var goToPrereqObj = CreateChild(lockedPanel.transform, "GoToPrereqButton");
            var goToPrereqRect = goToPrereqObj.GetComponent<RectTransform>();
            goToPrereqRect.anchorMin = new Vector2(0.1f, 0);
            goToPrereqRect.anchorMax = new Vector2(0.9f, 0.25f);
            goToPrereqRect.offsetMin = Vector2.zero;
            goToPrereqRect.offsetMax = Vector2.zero;
            var goToPrereqBg = goToPrereqObj.AddComponent<Image>();
            goToPrereqBg.color = ColorBrightPanel;
            var goToPrereqOutline = goToPrereqObj.AddComponent<Outline>();
            goToPrereqOutline.effectColor = ColorNeonBlue;
            var goToPrereqBtn = goToPrereqObj.AddComponent<Button>();

            var goToPrereqTextObj = CreateChild(goToPrereqObj.transform, "Text");
            SetupRect(goToPrereqTextObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var goToPrereqText = goToPrereqTextObj.AddComponent<Text>();
            goToPrereqText.text = "해당 노드로 이동";
            goToPrereqText.fontSize = 12;
            goToPrereqText.color = ColorNeonBlue;
            goToPrereqText.alignment = TextAnchor.MiddleCenter;
            goToPrereqText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            lockedPanel.SetActive(false);

            // SkillPurchasePopup 컴포넌트
            var popup = overlayObj.AddComponent<SkillPurchasePopup>();
            popup.popupRoot = popupObj;
            popup.overlayBackground = overlayImg;
            popup.overlayCloseButton = overlayBtn;
            popup.closeButton = closeBtn;
            popup.nodeIcon = iconImg;
            popup.nodeNameText = nameText;
            popup.levelChangeText = levelText;
            popup.beforeAfterPanel = beforeAfterPanel;
            popup.beforeValueText = beforeText;
            popup.afterValueText = afterText;
            popup.effectPanel = effectPanel;
            popup.effectDescText = effectDescText;
            popup.costText = costText;
            popup.balanceText = balanceText;
            popup.purchaseButton = purchaseBtn;
            popup.purchaseButtonText = purchaseBtnText;
            popup.lockedInfoPanel = lockedPanel;
            popup.lockedReasonText = lockedReasonText;
            popup.goToPrereqButton = goToPrereqBtn;
            popup.goToPrereqButtonText = goToPrereqText;
            popup.popupBackground = popupBg;

            overlayObj.SetActive(false);

            return popup;
        }

        #endregion

        #region 스킬 데이터 로드/생성

        static Dictionary<string, SkillNodeData> LoadOrCreateSkillData()
        {
            var result = new Dictionary<string, SkillNodeData>();

            // 기획서 기반 18개 노드 정의
            var nodeDefinitions = GetNodeDefinitions();

            // SO 저장 경로
            string soFolder = "Assets/Project/ScriptableObjects/Skills";
            if (!AssetDatabase.IsValidFolder(soFolder))
            {
                // 폴더 순차 생성
                if (!AssetDatabase.IsValidFolder("Assets/Project/ScriptableObjects"))
                    AssetDatabase.CreateFolder("Assets/Project", "ScriptableObjects");
                AssetDatabase.CreateFolder("Assets/Project/ScriptableObjects", "Skills");
            }

            foreach (var def in nodeDefinitions)
            {
                string assetPath = $"{soFolder}/Skill_{def.skillId}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<SkillNodeData>(assetPath);

                if (existing != null)
                {
                    // 기존 SO 업데이트 (그리드 좌표, 연결 정보 등)
                    UpdateSkillData(existing, def);
                    result[def.skillId] = existing;
                }
                else
                {
                    // 새 SO 생성
                    var newData = ScriptableObject.CreateInstance<SkillNodeData>();
                    ApplyDefinition(newData, def);
                    AssetDatabase.CreateAsset(newData, assetPath);
                    result[def.skillId] = newData;
                }
            }

            // 기존 Skills 폴더의 SO도 확인 (레거시 경로)
            string legacyFolder = "Assets/Data/Skills";
            if (AssetDatabase.IsValidFolder(legacyFolder))
            {
                var guids = AssetDatabase.FindAssets("t:SkillNodeData", new[] { legacyFolder });
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var data = AssetDatabase.LoadAssetAtPath<SkillNodeData>(path);
                    if (data != null && !result.ContainsKey(data.skillId))
                    {
                        // 레거시 SO를 새 폴더로 복사하지 않고 참조만 추가
                        // (이미 새 SO가 생성되었으므로 스킵)
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return result;
        }

        struct NodeDef
        {
            public string skillId;
            public string skillName;
            public string description;
            public SkillEffectType effectType;
            public float valuePerLevel;
            public int maxLevel;
            public SkillResourceType resourceType;
            public int baseCost;
            public float growthRate;
            public Vector2Int gridPosition;
            public string[] prerequisiteIds;
            public PrerequisiteMode prerequisiteMode;
            public string[] connectedNodeIds;
        }

        static NodeDef[] GetNodeDefinitions()
        {
            return new NodeDef[]
            {
                // N00: Core Node (시작)
                new NodeDef {
                    skillId = "N00", skillName = "Core Node",
                    description = "스킬트리의 시작점",
                    effectType = SkillEffectType.None, valuePerLevel = 0, maxLevel = 1,
                    resourceType = SkillResourceType.Bit, baseCost = 0, growthRate = 1,
                    gridPosition = new Vector2Int(0, 0),
                    prerequisiteIds = new string[0],
                    prerequisiteMode = PrerequisiteMode.Or,
                    connectedNodeIds = new[] { "N01", "N03", "N04", "N06" }
                },
                // N01: 공격력 강화
                new NodeDef {
                    skillId = "N01", skillName = "공격력 강화",
                    description = "타워 데미지 +10%/Lv",
                    effectType = SkillEffectType.AttackDamage, valuePerLevel = 10f, maxLevel = 20,
                    resourceType = SkillResourceType.Bit, baseCost = 50, growthRate = 1.3f,
                    gridPosition = new Vector2Int(0, -1),
                    prerequisiteIds = new[] { "N00" },
                    prerequisiteMode = PrerequisiteMode.Or,
                    connectedNodeIds = new[] { "N05", "N15" }
                },
                // N03: 기지 HP 강화
                new NodeDef {
                    skillId = "N03", skillName = "기지 HP 강화",
                    description = "기지 HP +5/Lv",
                    effectType = SkillEffectType.BaseHp, valuePerLevel = 5f, maxLevel = 20,
                    resourceType = SkillResourceType.Bit, baseCost = 40, growthRate = 1.25f,
                    gridPosition = new Vector2Int(1, -1),
                    prerequisiteIds = new[] { "N00" },
                    prerequisiteMode = PrerequisiteMode.Or,
                    connectedNodeIds = new[] { "N07", "N17" }
                },
                // N04: 사거리 강화
                new NodeDef {
                    skillId = "N04", skillName = "사거리 강화",
                    description = "타워 사거리 +0.2/Lv",
                    effectType = SkillEffectType.Range, valuePerLevel = 0.2f, maxLevel = 10,
                    resourceType = SkillResourceType.Bit, baseCost = 60, growthRate = 1.3f,
                    gridPosition = new Vector2Int(0, 1),
                    prerequisiteIds = new[] { "N00" },
                    prerequisiteMode = PrerequisiteMode.Or,
                    connectedNodeIds = new[] { "N05", "N12", "N13" }
                },
                // N05: Bit 획득량
                new NodeDef {
                    skillId = "N05", skillName = "Bit 획득량",
                    description = "Node 처치 Bit +15%/Lv",
                    effectType = SkillEffectType.BitGain, valuePerLevel = 15f, maxLevel = 15,
                    resourceType = SkillResourceType.Bit, baseCost = 100, growthRate = 1.35f,
                    gridPosition = new Vector2Int(-1, 1),
                    prerequisiteIds = new[] { "N01", "N04" },
                    prerequisiteMode = PrerequisiteMode.Or,
                    connectedNodeIds = new[] { "N09" }
                },
                // N06: 공격속도 강화
                new NodeDef {
                    skillId = "N06", skillName = "공격속도 강화",
                    description = "타워 공속 +5%/Lv",
                    effectType = SkillEffectType.AttackSpeed, valuePerLevel = 5f, maxLevel = 15,
                    resourceType = SkillResourceType.Bit, baseCost = 80, growthRate = 1.3f,
                    gridPosition = new Vector2Int(1, 1),
                    prerequisiteIds = new[] { "N00" },
                    prerequisiteMode = PrerequisiteMode.Or,
                    connectedNodeIds = new[] { "N09", "N13" }
                },
                // N07: 시작 Bit
                new NodeDef {
                    skillId = "N07", skillName = "시작 Bit",
                    description = "런 시작 Bit +30/Lv",
                    effectType = SkillEffectType.StartBit, valuePerLevel = 30f, maxLevel = 10,
                    resourceType = SkillResourceType.Bit, baseCost = 120, growthRate = 1.4f,
                    gridPosition = new Vector2Int(2, -1),
                    prerequisiteIds = new[] { "N03" },
                    prerequisiteMode = PrerequisiteMode.Or,
                    connectedNodeIds = new[] { "N10", "N15" }
                },
                // N09: 스폰율 증가
                new NodeDef {
                    skillId = "N09", skillName = "스폰율 증가",
                    description = "Node 스폰 +10%/Lv",
                    effectType = SkillEffectType.SpawnRate, valuePerLevel = 10f, maxLevel = 10,
                    resourceType = SkillResourceType.Bit, baseCost = 90, growthRate = 1.3f,
                    gridPosition = new Vector2Int(0, 2),
                    prerequisiteIds = new[] { "N05", "N06" },
                    prerequisiteMode = PrerequisiteMode.Or,
                    connectedNodeIds = new string[0]
                },
                // N10: Lightning 해금
                new NodeDef {
                    skillId = "N10", skillName = "Lightning 해금",
                    description = "Lightning Tower 사용 가능",
                    effectType = SkillEffectType.TowerUnlock, valuePerLevel = 1f, maxLevel = 1,
                    resourceType = SkillResourceType.Core, baseCost = 8, growthRate = 1f,
                    gridPosition = new Vector2Int(2, -3),
                    prerequisiteIds = new[] { "N14", "N15" },
                    prerequisiteMode = PrerequisiteMode.And,
                    connectedNodeIds = new[] { "N11" }
                },
                // N11: Laser 해금
                new NodeDef {
                    skillId = "N11", skillName = "Laser 해금",
                    description = "Laser Tower 사용 가능",
                    effectType = SkillEffectType.TowerUnlock, valuePerLevel = 1f, maxLevel = 1,
                    resourceType = SkillResourceType.Core, baseCost = 12, growthRate = 1f,
                    gridPosition = new Vector2Int(2, -4),
                    prerequisiteIds = new[] { "N10" },
                    prerequisiteMode = PrerequisiteMode.Or,
                    connectedNodeIds = new[] { "N18" }
                },
                // N12: HP 회복
                new NodeDef {
                    skillId = "N12", skillName = "HP 회복",
                    description = "기지 HP 초당 0.5 회복/Lv",
                    effectType = SkillEffectType.HpRegen, valuePerLevel = 0.5f, maxLevel = 5,
                    resourceType = SkillResourceType.Bit, baseCost = 200, growthRate = 1.4f,
                    gridPosition = new Vector2Int(-2, 1),
                    prerequisiteIds = new[] { "N04" },
                    prerequisiteMode = PrerequisiteMode.Or,
                    connectedNodeIds = new[] { "N16" }
                },
                // N13: 크리티컬 해금
                new NodeDef {
                    skillId = "N13", skillName = "크리티컬 해금",
                    description = "15% 확률로 2배 데미지",
                    effectType = SkillEffectType.Critical, valuePerLevel = 1f, maxLevel = 1,
                    resourceType = SkillResourceType.Core, baseCost = 2, growthRate = 1f,
                    gridPosition = new Vector2Int(2, 1),
                    prerequisiteIds = new[] { "N04", "N06" },
                    prerequisiteMode = PrerequisiteMode.Or,
                    connectedNodeIds = new[] { "N14" }
                },
                // N14: Cannon 해금
                new NodeDef {
                    skillId = "N14", skillName = "Cannon 해금",
                    description = "Cannon Tower 사용 가능\nAoE 폭발 공격",
                    effectType = SkillEffectType.TowerUnlock, valuePerLevel = 1f, maxLevel = 1,
                    resourceType = SkillResourceType.Core, baseCost = 3, growthRate = 1f,
                    gridPosition = new Vector2Int(3, 1),
                    prerequisiteIds = new[] { "N13" },
                    prerequisiteMode = PrerequisiteMode.Or,
                    connectedNodeIds = new[] { "N10" }
                },
                // N15: Ice 해금
                new NodeDef {
                    skillId = "N15", skillName = "Ice 해금",
                    description = "Ice Tower 사용 가능\n적 이동속도 감소",
                    effectType = SkillEffectType.TowerUnlock, valuePerLevel = 1f, maxLevel = 1,
                    resourceType = SkillResourceType.Core, baseCost = 5, growthRate = 1f,
                    gridPosition = new Vector2Int(1, -2),
                    prerequisiteIds = new[] { "N01", "N07" },
                    prerequisiteMode = PrerequisiteMode.Or,
                    connectedNodeIds = new[] { "N10" }
                },
                // N16: Idle 수집기
                new NodeDef {
                    skillId = "N16", skillName = "Idle 수집기",
                    description = "오프라인 Bit 생성 (100 Bit/시간)",
                    effectType = SkillEffectType.Idle, valuePerLevel = 1f, maxLevel = 1,
                    resourceType = SkillResourceType.Core, baseCost = 3, growthRate = 1f,
                    gridPosition = new Vector2Int(-3, 1),
                    prerequisiteIds = new[] { "N12" },
                    prerequisiteMode = PrerequisiteMode.Or,
                    connectedNodeIds = new string[0]
                },
                // N17: 배속 해금
                new NodeDef {
                    skillId = "N17", skillName = "배속 해금",
                    description = "2배속/3배속 모드 사용 가능",
                    effectType = SkillEffectType.SpeedControl, valuePerLevel = 1f, maxLevel = 1,
                    resourceType = SkillResourceType.Core, baseCost = 2, growthRate = 1f,
                    gridPosition = new Vector2Int(2, -2),
                    prerequisiteIds = new[] { "N03" },
                    prerequisiteMode = PrerequisiteMode.Or,
                    connectedNodeIds = new string[0]
                },
                // N18: Void 해금
                new NodeDef {
                    skillId = "N18", skillName = "Void 해금",
                    description = "Void Tower 사용 가능\n최강 타워",
                    effectType = SkillEffectType.TowerUnlock, valuePerLevel = 1f, maxLevel = 1,
                    resourceType = SkillResourceType.Core, baseCost = 18, growthRate = 1f,
                    gridPosition = new Vector2Int(2, -5),
                    prerequisiteIds = new[] { "N11" },
                    prerequisiteMode = PrerequisiteMode.Or,
                    connectedNodeIds = new string[0]
                },
            };
        }

        static void ApplyDefinition(SkillNodeData data, NodeDef def)
        {
            data.skillId = def.skillId;
            data.skillName = def.skillName;
            data.description = def.description;
            data.effectType = def.effectType;
            data.valuePerLevel = def.valuePerLevel;
            data.maxLevel = def.maxLevel;
            data.resourceType = def.resourceType;
            data.baseCost = def.baseCost;
            data.growthRate = def.growthRate;
            data.gridPosition = def.gridPosition;
            data.prerequisiteIds = def.prerequisiteIds;
            data.prerequisiteMode = def.prerequisiteMode;
            data.connectedNodeIds = def.connectedNodeIds;
            // 레거시 position 필드도 설정
            data.position = new Vector2(def.gridPosition.x, def.gridPosition.y);
        }

        static void UpdateSkillData(SkillNodeData data, NodeDef def)
        {
            // 기존 SO의 그리드 좌표/연결 정보/리소스 타입 등 업데이트
            data.gridPosition = def.gridPosition;
            data.prerequisiteIds = def.prerequisiteIds;
            data.prerequisiteMode = def.prerequisiteMode;
            data.connectedNodeIds = def.connectedNodeIds;
            data.resourceType = def.resourceType;
            data.effectType = def.effectType;
            data.maxLevel = def.maxLevel;
            data.baseCost = def.baseCost;
            data.growthRate = def.growthRate;
            data.skillName = def.skillName;
            data.description = def.description;
            data.valuePerLevel = def.valuePerLevel;
            data.position = new Vector2(def.gridPosition.x, def.gridPosition.y);
            EditorUtility.SetDirty(data);
        }

        #endregion

        #region 헬퍼

        static void RemoveExistingSkillTree(Transform parent)
        {
            // PurchasePopupOverlay 제거
            var overlay = parent.Find("PurchasePopupOverlay");
            if (overlay != null)
                Object.DestroyImmediate(overlay.gameObject);
        }

        static void DestroyChildrenAll(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(parent.GetChild(i).gameObject);
            }
        }

        static GameObject CreateChild(Transform parent, string name)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        static RectTransform SetupRect(GameObject obj, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var rect = obj.GetComponent<RectTransform>();
            if (rect == null) rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return rect;
        }

        #endregion
    }
}
