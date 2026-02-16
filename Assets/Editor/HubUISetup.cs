using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Soulspire.Editor
{
    /// <summary>
    /// Hub UI를 PPT 명세에 맞게 재구성하는 에디터 도구
    /// 레이아웃: 상단 바(50px) + 스킬 트리(나머지) + 하단 바(55px)
    /// </summary>
    public class HubUISetup
    {
        // PPT 색상 팔레트
        static readonly Color ColorMainBg      = new Color32(0x0B, 0x0F, 0x1A, 0xFF);
        static readonly Color ColorPanel       = new Color32(0x12, 0x1A, 0x2A, 0xFF);
        static readonly Color ColorBrightPanel = new Color32(0x1A, 0x24, 0x3A, 0xFF);
        static readonly Color ColorNeonGreen   = new Color32(0x2B, 0xFF, 0x88, 0xFF);
        static readonly Color ColorNeonBlue    = new Color32(0x37, 0xB6, 0xFF, 0xFF);
        static readonly Color ColorNeonPurple  = new Color32(0xB8, 0x6C, 0xFF, 0xFF);
        static readonly Color ColorRed         = new Color32(0xFF, 0x4D, 0x5A, 0xFF);
        static readonly Color ColorYellowGold  = new Color32(0xFF, 0xD8, 0x4D, 0xFF);
        static readonly Color ColorTextPrimary = new Color32(0xD8, 0xE4, 0xFF, 0xFF);
        static readonly Color ColorTextSecondary = new Color32(0xAF, 0xC3, 0xE8, 0xFF);
        static readonly Color ColorBorder      = new Color32(0x5B, 0x6B, 0x8A, 0xFF);
        static readonly Color ColorDarkGreen   = new Color32(0x15, 0x30, 0x25, 0xFF);

        [MenuItem("Tools/Soulspire/Rebuild Hub UI (PPT Spec)")]
        static void RebuildHubUI()
        {
            // HubPanel 찾기
            var hubUI = Object.FindFirstObjectByType<UI.HubUI>(FindObjectsInactive.Include);
            if (hubUI == null)
            {
                Debug.LogError("[HubUISetup] HubUI 컴포넌트를 찾을 수 없습니다. Phase 2 Setup을 먼저 실행하세요.");
                return;
            }

            var hubPanel = hubUI.gameObject;
            var hubRect = hubPanel.GetComponent<RectTransform>();

            // HubPanel 배경색 -> 메인 배경
            var hubBg = hubPanel.GetComponent<Image>();
            if (hubBg != null) hubBg.color = ColorMainBg;

            // =========================================
            // 기존 자식 정리 (TopBar, SkillArea 등 삭제 후 재생성)
            // =========================================
            DestroyChildrenExcept(hubPanel.transform, new string[] { });

            // =========================================
            // 0. 배경 이미지 (전체 화면, HubBG_03_dimmed)
            // =========================================
            var hubBgImgObj = CreateChild(hubPanel.transform, "BackgroundImage");
            var hubBgImgRect = SetupRect(hubBgImgObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var hubBgImg = hubBgImgObj.AddComponent<Image>();
            hubBgImg.color = Color.white;
            hubBgImg.preserveAspect = false;
            hubBgImg.raycastTarget = false;
            var hubBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Backgrounds/HubBG_03_dimmed.png");
            if (hubBgSprite != null)
            {
                hubBgImg.sprite = hubBgSprite;
                Debug.Log("[HubUISetup] 허브 배경 이미지 적용: HubBG_03_dimmed.png");
            }
            else
            {
                Debug.LogWarning("[HubUISetup] HubBG_03_dimmed.png 스프라이트를 찾을 수 없습니다. 텍스처 임포트 설정을 확인하세요.");
            }
            hubUI.backgroundImage = hubBgImg;

            // =========================================
            // 1. 상단 바 (0,0) 1920x50px
            // =========================================
            var topBar = CreateChild(hubPanel.transform, "TopBar");
            var topBarRect = SetupRect(topBar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -50), Vector2.zero);
            var topBarImg = topBar.AddComponent<Image>();
            topBarImg.color = ColorPanel;
            // 하단 테두리 효과용 (1px 라인)
            var topBarBorderObj = CreateChild(topBar.transform, "BottomBorder");
            var topBarBorderRect = SetupRect(topBarBorderObj, new Vector2(0, 0), new Vector2(1, 0), Vector2.zero, new Vector2(0, 1));
            var topBarBorderImg = topBarBorderObj.AddComponent<Image>();
            topBarBorderImg.color = ColorBorder;

            // Bit 카운터 (20,8) 200x35px
            var bitTextObj = CreateChild(topBar.transform, "TotalBitText");
            var bitRect = bitTextObj.GetComponent<RectTransform>();
            bitRect.anchorMin = new Vector2(0, 0.5f);
            bitRect.anchorMax = new Vector2(0, 0.5f);
            bitRect.pivot = new Vector2(0, 0.5f);
            bitRect.anchoredPosition = new Vector2(20, 0);
            bitRect.sizeDelta = new Vector2(200, 35);
            var bitText = bitTextObj.AddComponent<Text>();
            bitText.text = "Bit: 1,250";
            bitText.fontSize = 16;
            bitText.fontStyle = FontStyle.Bold;
            bitText.color = ColorNeonGreen;
            bitText.alignment = TextAnchor.MiddleLeft;
            bitText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hubUI.totalBitText = bitText;

            // Core 카운터 (240,8) 200x35px
            var coreTextObj = CreateChild(topBar.transform, "TotalCoreText");
            var coreRect = coreTextObj.GetComponent<RectTransform>();
            coreRect.anchorMin = new Vector2(0, 0.5f);
            coreRect.anchorMax = new Vector2(0, 0.5f);
            coreRect.pivot = new Vector2(0, 0.5f);
            coreRect.anchoredPosition = new Vector2(240, 0);
            coreRect.sizeDelta = new Vector2(200, 35);
            var coreText = coreTextObj.AddComponent<Text>();
            coreText.text = "Core: 5";
            coreText.fontSize = 16;
            coreText.fontStyle = FontStyle.Bold;
            coreText.color = ColorNeonPurple;
            coreText.alignment = TextAnchor.MiddleLeft;
            coreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hubUI.totalCoreText = coreText;

            // 방치 Bit 알림 (우측) (1500,6) 400x38px
            var idleBitPanel = CreateChild(topBar.transform, "IdleBitPanel");
            var idleBitRect = idleBitPanel.GetComponent<RectTransform>();
            idleBitRect.anchorMin = new Vector2(1, 0.5f);
            idleBitRect.anchorMax = new Vector2(1, 0.5f);
            idleBitRect.pivot = new Vector2(1, 0.5f);
            idleBitRect.anchoredPosition = new Vector2(-20, 0);
            idleBitRect.sizeDelta = new Vector2(400, 38);
            var idleBitBg = idleBitPanel.AddComponent<Image>();
            idleBitBg.color = ColorBrightPanel;
            // 테두리 효과 (Outline 컴포넌트)
            var idleBitOutline = idleBitPanel.AddComponent<Outline>();
            idleBitOutline.effectColor = ColorYellowGold;
            idleBitOutline.effectDistance = new Vector2(1, -1);
            hubUI.idleBitPanel = idleBitPanel;

            // 방치 Bit 텍스트
            var idleBitTextObj = CreateChild(idleBitPanel.transform, "IdleBitText");
            SetupRect(idleBitTextObj, new Vector2(0, 0), new Vector2(0.7f, 1), new Vector2(10, 0), new Vector2(-5, 0));
            var idleBitText = idleBitTextObj.AddComponent<Text>();
            idleBitText.text = "방치 Bit: 150";
            idleBitText.fontSize = 14;
            idleBitText.color = ColorYellowGold;
            idleBitText.alignment = TextAnchor.MiddleLeft;
            idleBitText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hubUI.idleBitText = idleBitText;

            // 수령 버튼
            var claimBtnObj = CreateChild(idleBitPanel.transform, "ClaimButton");
            var claimBtnRect = SetupRect(claimBtnObj, new Vector2(0.7f, 0.1f), new Vector2(0.95f, 0.9f), Vector2.zero, Vector2.zero);
            var claimBtnBg = claimBtnObj.AddComponent<Image>();
            claimBtnBg.color = ColorYellowGold;
            var claimBtn = claimBtnObj.AddComponent<Button>();
            var claimBtnTextObj = CreateChild(claimBtnObj.transform, "Text");
            SetupRect(claimBtnTextObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var claimBtnText = claimBtnTextObj.AddComponent<Text>();
            claimBtnText.text = "수령!";
            claimBtnText.fontSize = 13;
            claimBtnText.fontStyle = FontStyle.Bold;
            claimBtnText.color = ColorMainBg;
            claimBtnText.alignment = TextAnchor.MiddleCenter;
            claimBtnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hubUI.idleBitClaimButton = claimBtn;

            idleBitPanel.SetActive(false); // 기본 비활성

            // =========================================
            // 2. 스킬 트리 영역 (0,50) ~ (1920, 1080-55)
            // =========================================
            var skillArea = CreateChild(hubPanel.transform, "SkillArea");
            var skillAreaRect = SetupRect(skillArea, Vector2.zero, Vector2.one, new Vector2(0, 55), new Vector2(0, -50));

            // ScrollRect 설정
            var scrollRect = skillArea.AddComponent<ScrollRect>();
            scrollRect.horizontal = true;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
            scrollRect.scrollSensitivity = 30f;
            hubUI.skillScrollRect = scrollRect;

            // Content (스크롤 내부 콘텐츠 - 스킬 노드가 들어감)
            var content = CreateChild(skillArea.transform, "Content");
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax = new Vector2(0.5f, 0.5f);
            contentRect.pivot = new Vector2(0.5f, 0.5f);
            contentRect.sizeDelta = new Vector2(1920, 1080);
            contentRect.anchoredPosition = Vector2.zero;
            scrollRect.content = contentRect;

            // 범례 (좌측 상단) 280x160px
            var legendObj = CreateChild(skillArea.transform, "Legend");
            var legendRect = legendObj.GetComponent<RectTransform>();
            legendRect.anchorMin = new Vector2(0, 1);
            legendRect.anchorMax = new Vector2(0, 1);
            legendRect.pivot = new Vector2(0, 1);
            legendRect.anchoredPosition = new Vector2(10, -10);
            legendRect.sizeDelta = new Vector2(280, 160);
            var legendBg = legendObj.AddComponent<Image>();
            legendBg.color = new Color(ColorPanel.r, ColorPanel.g, ColorPanel.b, 0.85f);
            var legendOutline = legendObj.AddComponent<Outline>();
            legendOutline.effectColor = ColorBorder;
            legendOutline.effectDistance = new Vector2(1, -1);

            // 범례 텍스트
            var legendTextObj = CreateChild(legendObj.transform, "LegendText");
            SetupRect(legendTextObj, Vector2.zero, Vector2.one, new Vector2(10, 5), new Vector2(-10, -5));
            var legendText = legendTextObj.AddComponent<Text>();
            legendText.text = "[ 범례 ]\n" +
                              "<color=#2BFF88>\u25cf</color> 활성 (밝은 채우기+발광)\n" +
                              "<color=#37B6FF>\u25cb</color> 구매 가능 (깜빡이는 테두리)\n" +
                              "<color=#5B6B8A>\u25cf</color> 잠김 (흐릿함)\n" +
                              "<color=#1A243A>\u25cb</color> 숨김";
            legendText.fontSize = 13;
            legendText.color = ColorTextSecondary;
            legendText.alignment = TextAnchor.UpperLeft;
            legendText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            legendText.supportRichText = true;
            legendText.lineSpacing = 1.3f;

            // Core 노드 (화면 중앙, 70x70px)
            var coreNode = CreateChild(content.transform, "CoreNode");
            var coreNodeRect = coreNode.GetComponent<RectTransform>();
            coreNodeRect.anchorMin = new Vector2(0.5f, 0.5f);
            coreNodeRect.anchorMax = new Vector2(0.5f, 0.5f);
            coreNodeRect.pivot = new Vector2(0.5f, 0.5f);
            coreNodeRect.sizeDelta = new Vector2(70, 70);
            coreNodeRect.anchoredPosition = Vector2.zero;
            var coreNodeBg = coreNode.AddComponent<Image>();
            coreNodeBg.color = ColorNeonGreen;
            var coreNodeTextObj = CreateChild(coreNode.transform, "Text");
            SetupRect(coreNodeTextObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var coreNodeText = coreNodeTextObj.AddComponent<Text>();
            coreNodeText.text = "CORE";
            coreNodeText.fontSize = 14;
            coreNodeText.fontStyle = FontStyle.Bold;
            coreNodeText.color = ColorMainBg;
            coreNodeText.alignment = TextAnchor.MiddleCenter;
            coreNodeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 스킬 노드 3개 (Core 노드 주변, 각 60x60px, 중심 간격 80px)
            var skillDataAssets = new Data.SkillNodeData[3];
            skillDataAssets[0] = AssetDatabase.LoadAssetAtPath<Data.SkillNodeData>("Assets/Data/Skills/Skill_AttackDamage.asset");
            skillDataAssets[1] = AssetDatabase.LoadAssetAtPath<Data.SkillNodeData>("Assets/Data/Skills/Skill_AttackSpeed.asset");
            skillDataAssets[2] = AssetDatabase.LoadAssetAtPath<Data.SkillNodeData>("Assets/Data/Skills/Skill_BaseHp.asset");

            Vector2[] nodeOffsets = {
                new Vector2(-120, 80),   // 좌상
                new Vector2(120, 80),    // 우상
                new Vector2(0, -120)     // 하단
            };

            var skillNodes = new UI.SkillNodeUI[3];
            for (int i = 0; i < 3; i++)
            {
                // 연결선 (Core -> 노드)
                var lineObj = CreateChild(content.transform, $"Line_{i}");
                var lineRect = lineObj.GetComponent<RectTransform>();
                lineRect.anchorMin = new Vector2(0.5f, 0.5f);
                lineRect.anchorMax = new Vector2(0.5f, 0.5f);
                lineRect.pivot = new Vector2(0.5f, 0.5f);
                // 연결선: 2px 두께, 방향에 맞게 배치
                float lineLen = nodeOffsets[i].magnitude;
                float angle = Mathf.Atan2(nodeOffsets[i].y, nodeOffsets[i].x) * Mathf.Rad2Deg;
                lineRect.sizeDelta = new Vector2(lineLen, 2);
                lineRect.anchoredPosition = nodeOffsets[i] * 0.5f;
                lineRect.localRotation = Quaternion.Euler(0, 0, angle);
                var lineImg = lineObj.AddComponent<Image>();
                lineImg.color = ColorNeonGreen;

                // 노드
                var nodeObj = CreateChild(content.transform, $"SkillNode_{i}");
                var nodeRect = nodeObj.GetComponent<RectTransform>();
                nodeRect.anchorMin = new Vector2(0.5f, 0.5f);
                nodeRect.anchorMax = new Vector2(0.5f, 0.5f);
                nodeRect.pivot = new Vector2(0.5f, 0.5f);
                nodeRect.sizeDelta = new Vector2(60, 60);
                nodeRect.anchoredPosition = nodeOffsets[i];

                var nodeBg = nodeObj.AddComponent<Image>();
                nodeBg.color = ColorBrightPanel;
                var nodeOutline = nodeObj.AddComponent<Outline>();
                nodeOutline.effectColor = ColorNeonBlue;
                nodeOutline.effectDistance = new Vector2(2, -2);

                var nodeBtn = nodeObj.AddComponent<Button>();
                var btnColors = nodeBtn.colors;
                btnColors.normalColor = Color.white;
                btnColors.highlightedColor = new Color(1, 1, 1, 0.9f);
                btnColors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
                nodeBtn.colors = btnColors;

                // 노드 레벨 텍스트
                var nodeLevelTextObj = CreateChild(nodeObj.transform, "LevelText");
                SetupRect(nodeLevelTextObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var nodeLevelText = nodeLevelTextObj.AddComponent<Text>();
                nodeLevelText.text = "Lv.0";
                nodeLevelText.fontSize = 12;
                nodeLevelText.color = ColorTextPrimary;
                nodeLevelText.alignment = TextAnchor.MiddleCenter;
                nodeLevelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

                // 노드 이름 텍스트 (아래)
                var nodeNameObj = CreateChild(nodeObj.transform, "NameText");
                var nodeNameRect = nodeNameObj.GetComponent<RectTransform>();
                nodeNameRect.anchorMin = new Vector2(0.5f, 0);
                nodeNameRect.anchorMax = new Vector2(0.5f, 0);
                nodeNameRect.pivot = new Vector2(0.5f, 1);
                nodeNameRect.anchoredPosition = new Vector2(0, -4);
                nodeNameRect.sizeDelta = new Vector2(100, 20);
                var nodeNameText = nodeNameObj.AddComponent<Text>();
                string[] nodeNames = { "공격력", "공격속도", "기지 HP" };
                nodeNameText.text = nodeNames[i];
                nodeNameText.fontSize = 11;
                nodeNameText.color = ColorTextSecondary;
                nodeNameText.alignment = TextAnchor.UpperCenter;
                nodeNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

                // SkillNodeUI 컴포넌트
                var skillNodeUI = nodeObj.AddComponent<UI.SkillNodeUI>();
                skillNodeUI.data = skillDataAssets[i];
                skillNodeUI.levelText = nodeLevelText;
                skillNodeUI.button = nodeBtn;
                skillNodes[i] = skillNodeUI;
            }
            hubUI.skillNodes = skillNodes;

            // 상세 패널 (스킬 영역 우하단)
            var detailPanel = CreateChild(skillArea.transform, "DetailPanel");
            var detailRect = detailPanel.GetComponent<RectTransform>();
            detailRect.anchorMin = new Vector2(1, 0);
            detailRect.anchorMax = new Vector2(1, 0);
            detailRect.pivot = new Vector2(1, 0);
            detailRect.anchoredPosition = new Vector2(-10, 10);
            detailRect.sizeDelta = new Vector2(350, 200);
            var detailBg = detailPanel.AddComponent<Image>();
            detailBg.color = new Color(ColorPanel.r, ColorPanel.g, ColorPanel.b, 0.95f);
            var detailOutline = detailPanel.AddComponent<Outline>();
            detailOutline.effectColor = ColorBorder;
            detailOutline.effectDistance = new Vector2(1, -1);
            hubUI.detailPanel = detailPanel;

            // 상세 패널 내용
            var dNameObj = CreateChild(detailPanel.transform, "DetailNameText");
            var dNameRect = SetupRect(dNameObj, new Vector2(0, 0.75f), new Vector2(0.6f, 1), new Vector2(10, 0), new Vector2(-5, -5));
            var dNameText = dNameObj.AddComponent<Text>();
            dNameText.text = "스킬 이름";
            dNameText.fontSize = 16;
            dNameText.fontStyle = FontStyle.Bold;
            dNameText.color = ColorTextPrimary;
            dNameText.alignment = TextAnchor.MiddleLeft;
            dNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hubUI.detailNameText = dNameText;

            var dLevelObj = CreateChild(detailPanel.transform, "DetailLevelText");
            SetupRect(dLevelObj, new Vector2(0.6f, 0.75f), new Vector2(1, 1), new Vector2(5, 0), new Vector2(-10, -5));
            var dLevelText = dLevelObj.AddComponent<Text>();
            dLevelText.text = "Lv.0/20";
            dLevelText.fontSize = 14;
            dLevelText.color = ColorNeonBlue;
            dLevelText.alignment = TextAnchor.MiddleRight;
            dLevelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hubUI.detailLevelText = dLevelText;

            var dDescObj = CreateChild(detailPanel.transform, "DetailDescText");
            SetupRect(dDescObj, new Vector2(0, 0.4f), new Vector2(1, 0.75f), new Vector2(10, 0), new Vector2(-10, 0));
            var dDescText = dDescObj.AddComponent<Text>();
            dDescText.text = "설명";
            dDescText.fontSize = 12;
            dDescText.color = ColorTextSecondary;
            dDescText.alignment = TextAnchor.UpperLeft;
            dDescText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hubUI.detailDescText = dDescText;

            var dEffectObj = CreateChild(detailPanel.transform, "DetailEffectText");
            SetupRect(dEffectObj, new Vector2(0, 0.15f), new Vector2(1, 0.4f), new Vector2(10, 0), new Vector2(-10, 0));
            var dEffectText = dEffectObj.AddComponent<Text>();
            dEffectText.text = "효과: +0%";
            dEffectText.fontSize = 13;
            dEffectText.color = ColorNeonGreen;
            dEffectText.alignment = TextAnchor.MiddleLeft;
            dEffectText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hubUI.detailEffectText = dEffectText;

            var dCostObj = CreateChild(detailPanel.transform, "DetailCostText");
            SetupRect(dCostObj, new Vector2(0, 0), new Vector2(0.5f, 0.15f), new Vector2(10, 5), new Vector2(-5, 0));
            var dCostText = dCostObj.AddComponent<Text>();
            dCostText.text = "비용: 50 Bit";
            dCostText.fontSize = 13;
            dCostText.color = ColorYellowGold;
            dCostText.alignment = TextAnchor.MiddleLeft;
            dCostText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hubUI.detailCostText = dCostText;

            // 구매 버튼 (상세 패널 우하단)
            var purchaseBtnObj = CreateChild(detailPanel.transform, "PurchaseButton");
            var purchaseRect = purchaseBtnObj.GetComponent<RectTransform>();
            purchaseRect.anchorMin = new Vector2(0.55f, 0);
            purchaseRect.anchorMax = new Vector2(1, 0.18f);
            purchaseRect.offsetMin = new Vector2(5, 3);
            purchaseRect.offsetMax = new Vector2(-10, -2);
            var purchaseBg = purchaseBtnObj.AddComponent<Image>();
            purchaseBg.color = ColorDarkGreen;
            var purchaseOutline = purchaseBtnObj.AddComponent<Outline>();
            purchaseOutline.effectColor = ColorNeonGreen;
            purchaseOutline.effectDistance = new Vector2(1, -1);
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
            hubUI.purchaseButton = purchaseBtn;

            detailPanel.SetActive(false); // 기본 비활성

            // =========================================
            // 3. 하단 바 (0,1025) 1920x55px
            // =========================================
            var bottomBar = CreateChild(hubPanel.transform, "BottomBar");
            var bottomBarRect = SetupRect(bottomBar, new Vector2(0, 0), new Vector2(1, 0), Vector2.zero, new Vector2(0, 55));
            var bottomBarImg = bottomBar.AddComponent<Image>();
            bottomBarImg.color = ColorPanel;

            // 스테이지 드롭다운 (20,8) 300x38px
            var stageDropdownObj = CreateChild(bottomBar.transform, "StageDropdown");
            var stageDropdownRect = stageDropdownObj.GetComponent<RectTransform>();
            stageDropdownRect.anchorMin = new Vector2(0, 0.5f);
            stageDropdownRect.anchorMax = new Vector2(0, 0.5f);
            stageDropdownRect.pivot = new Vector2(0, 0.5f);
            stageDropdownRect.anchoredPosition = new Vector2(20, 0);
            stageDropdownRect.sizeDelta = new Vector2(300, 38);
            var stageBg = stageDropdownObj.AddComponent<Image>();
            stageBg.color = ColorBrightPanel;
            var stageOutline = stageDropdownObj.AddComponent<Outline>();
            stageOutline.effectColor = ColorNeonBlue;
            stageOutline.effectDistance = new Vector2(1, -1);
            var stageDropdown = stageDropdownObj.AddComponent<Dropdown>();
            // 드롭다운 Label
            var dropdownLabelObj = CreateChild(stageDropdownObj.transform, "Label");
            SetupRect(dropdownLabelObj, Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-30, 0));
            var dropdownLabel = dropdownLabelObj.AddComponent<Text>();
            dropdownLabel.text = "Stage 1";
            dropdownLabel.fontSize = 14;
            dropdownLabel.color = ColorNeonBlue;
            dropdownLabel.alignment = TextAnchor.MiddleLeft;
            dropdownLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            stageDropdown.captionText = dropdownLabel;
            // 드롭다운 템플릿 (최소 구성)
            var templateObj = CreateChild(stageDropdownObj.transform, "Template");
            var templateRect = templateObj.GetComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0, 0);
            templateRect.anchorMax = new Vector2(1, 0);
            templateRect.pivot = new Vector2(0.5f, 1);
            templateRect.anchoredPosition = Vector2.zero;
            templateRect.sizeDelta = new Vector2(0, 150);
            var templateBg = templateObj.AddComponent<Image>();
            templateBg.color = ColorBrightPanel;
            var templateScrollRect = templateObj.AddComponent<ScrollRect>();

            var viewportObj = CreateChild(templateObj.transform, "Viewport");
            SetupRect(viewportObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            viewportObj.AddComponent<Image>().color = Color.white;
            viewportObj.AddComponent<Mask>().showMaskGraphic = false;
            templateScrollRect.viewport = viewportObj.GetComponent<RectTransform>();

            var templateContentObj = CreateChild(viewportObj.transform, "Content");
            var templateContentRect = templateContentObj.GetComponent<RectTransform>();
            templateContentRect.anchorMin = new Vector2(0, 1);
            templateContentRect.anchorMax = new Vector2(1, 1);
            templateContentRect.pivot = new Vector2(0.5f, 1);
            templateContentRect.sizeDelta = new Vector2(0, 28);
            templateScrollRect.content = templateContentRect;

            var itemObj = CreateChild(templateContentObj.transform, "Item");
            var itemRect = itemObj.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0, 0.5f);
            itemRect.anchorMax = new Vector2(1, 0.5f);
            itemRect.sizeDelta = new Vector2(0, 28);
            var itemToggle = itemObj.AddComponent<Toggle>();

            var itemBgObj = CreateChild(itemObj.transform, "Item Background");
            SetupRect(itemBgObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var itemBgImg = itemBgObj.AddComponent<Image>();
            itemBgImg.color = ColorBrightPanel;

            var itemCheckObj = CreateChild(itemObj.transform, "Item Checkmark");
            SetupRect(itemCheckObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var itemCheckImg = itemCheckObj.AddComponent<Image>();
            itemCheckImg.color = new Color(ColorNeonBlue.r, ColorNeonBlue.g, ColorNeonBlue.b, 0.3f);
            itemToggle.graphic = itemCheckImg;

            var itemLabelObj = CreateChild(itemObj.transform, "Item Label");
            SetupRect(itemLabelObj, Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-10, 0));
            var itemLabelText = itemLabelObj.AddComponent<Text>();
            itemLabelText.text = "Stage";
            itemLabelText.fontSize = 14;
            itemLabelText.color = ColorTextPrimary;
            itemLabelText.alignment = TextAnchor.MiddleLeft;
            itemLabelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            stageDropdown.itemText = itemLabelText;

            stageDropdown.template = templateRect;
            templateObj.SetActive(false);
            hubUI.stageDropdown = stageDropdown;

            // 출격 버튼 (810,5) 300x44px (가운데)
            var startBtnObj = CreateChild(bottomBar.transform, "StartRunButton");
            var startBtnRect = startBtnObj.GetComponent<RectTransform>();
            startBtnRect.anchorMin = new Vector2(0.5f, 0.5f);
            startBtnRect.anchorMax = new Vector2(0.5f, 0.5f);
            startBtnRect.pivot = new Vector2(0.5f, 0.5f);
            startBtnRect.anchoredPosition = Vector2.zero;
            startBtnRect.sizeDelta = new Vector2(300, 44);
            var startBtnBg = startBtnObj.AddComponent<Image>();
            startBtnBg.color = ColorDarkGreen;
            var startBtnOutline = startBtnObj.AddComponent<Outline>();
            startBtnOutline.effectColor = ColorNeonGreen;
            startBtnOutline.effectDistance = new Vector2(2, -2);
            var startBtn = startBtnObj.AddComponent<Button>();
            var startBtnTextObj = CreateChild(startBtnObj.transform, "Text");
            SetupRect(startBtnTextObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var startBtnText = startBtnTextObj.AddComponent<Text>();
            startBtnText.text = "[ \uCD9C\uACA9 ]";
            startBtnText.fontSize = 18;
            startBtnText.fontStyle = FontStyle.Bold;
            startBtnText.color = ColorNeonGreen;
            startBtnText.alignment = TextAnchor.MiddleCenter;
            startBtnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hubUI.startRunButton = startBtn;

            // 설정 버튼 (1700,8) 100x38px (우측)
            var settingsBtnObj = CreateChild(bottomBar.transform, "SettingsButton");
            var settingsBtnRect = settingsBtnObj.GetComponent<RectTransform>();
            settingsBtnRect.anchorMin = new Vector2(1, 0.5f);
            settingsBtnRect.anchorMax = new Vector2(1, 0.5f);
            settingsBtnRect.pivot = new Vector2(1, 0.5f);
            settingsBtnRect.anchoredPosition = new Vector2(-120, 0);
            settingsBtnRect.sizeDelta = new Vector2(100, 38);
            var settingsBg = settingsBtnObj.AddComponent<Image>();
            settingsBg.color = ColorBrightPanel;
            var settingsOutline = settingsBtnObj.AddComponent<Outline>();
            settingsOutline.effectColor = ColorNeonBlue;
            settingsOutline.effectDistance = new Vector2(1, -1);
            var settingsBtn = settingsBtnObj.AddComponent<Button>();
            var settingsBtnTextObj = CreateChild(settingsBtnObj.transform, "Text");
            SetupRect(settingsBtnTextObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var settingsBtnText = settingsBtnTextObj.AddComponent<Text>();
            settingsBtnText.text = "설정";
            settingsBtnText.fontSize = 14;
            settingsBtnText.color = ColorNeonBlue;
            settingsBtnText.alignment = TextAnchor.MiddleCenter;
            settingsBtnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hubUI.settingsButton = settingsBtn;

            // 종료 버튼 (1810,8) 100x38px
            var quitBtnObj = CreateChild(bottomBar.transform, "QuitButton");
            var quitBtnRect = quitBtnObj.GetComponent<RectTransform>();
            quitBtnRect.anchorMin = new Vector2(1, 0.5f);
            quitBtnRect.anchorMax = new Vector2(1, 0.5f);
            quitBtnRect.pivot = new Vector2(1, 0.5f);
            quitBtnRect.anchoredPosition = new Vector2(-10, 0);
            quitBtnRect.sizeDelta = new Vector2(100, 38);
            var quitBg = quitBtnObj.AddComponent<Image>();
            quitBg.color = ColorBrightPanel;
            var quitOutline = quitBtnObj.AddComponent<Outline>();
            quitOutline.effectColor = ColorRed;
            quitOutline.effectDistance = new Vector2(1, -1);
            var quitBtn = quitBtnObj.AddComponent<Button>();
            var quitBtnTextObj = CreateChild(quitBtnObj.transform, "Text");
            SetupRect(quitBtnTextObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var quitBtnText = quitBtnTextObj.AddComponent<Text>();
            quitBtnText.text = "종료";
            quitBtnText.fontSize = 14;
            quitBtnText.color = ColorRed;
            quitBtnText.alignment = TextAnchor.MiddleCenter;
            quitBtnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hubUI.quitButton = quitBtn;

            // =========================================
            // 마무리
            // =========================================
            EditorUtility.SetDirty(hubUI);
            EditorUtility.SetDirty(hubPanel);

            Debug.Log("[HubUISetup] Hub UI PPT 명세 기반 재구성 완료!");
            Debug.Log("[HubUISetup] 상단 바: Bit/Core 카운터 + 방치 Bit 알림");
            Debug.Log("[HubUISetup] 스킬 트리: ScrollRect + Core 노드 + 3 스킬 노드 + 연결선 + 범례 + 상세 패널");
            Debug.Log("[HubUISetup] 하단 바: 스테이지 드롭다운 + 출격 + 설정 + 종료");
        }

        #region 헬퍼 메서드

        static void DestroyChildrenExcept(Transform parent, string[] keepNames)
        {
            var toDestroy = new System.Collections.Generic.List<GameObject>();
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                var child = parent.GetChild(i);
                bool keep = false;
                foreach (var name in keepNames)
                {
                    if (child.name == name) { keep = true; break; }
                }
                if (!keep) toDestroy.Add(child.gameObject);
            }
            foreach (var obj in toDestroy)
                Object.DestroyImmediate(obj);
        }

        static GameObject CreateChild(Transform parent, string name)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        static RectTransform SetupRect(GameObject obj, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
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
