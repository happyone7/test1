using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Nodebreaker.Editor
{
    /// <summary>
    /// PPT 명세 슬라이드 5 기반 팝업/툴팁 UI 오브젝트 생성
    /// </summary>
    public class PopupSetup
    {
        // 색상 팔레트
        static readonly Color ColPanel      = new Color32(0x12, 0x1A, 0x2A, 0xFF);
        static readonly Color ColBrightPanel= new Color32(0x1A, 0x24, 0x3A, 0xFF);
        static readonly Color ColNeonGreen  = new Color32(0x2B, 0xFF, 0x88, 0xFF);
        static readonly Color ColNeonBlue   = new Color32(0x37, 0xB6, 0xFF, 0xFF);
        static readonly Color ColYellow     = new Color32(0xFF, 0xD8, 0x4D, 0xFF);
        static readonly Color ColOrange     = new Color32(0xFF, 0x9A, 0x3D, 0xFF);
        static readonly Color ColRed        = new Color32(0xFF, 0x4D, 0x5A, 0xFF);
        static readonly Color ColTextMain   = new Color32(0xD8, 0xE4, 0xFF, 0xFF);
        static readonly Color ColTextSub    = new Color32(0xAF, 0xC3, 0xE8, 0xFF);
        static readonly Color ColBorder     = new Color32(0x5B, 0x6B, 0x8A, 0xFF);
        static readonly Color ColBtnGreenBg = new Color32(0x15, 0x30, 0x20, 0xFF);
        static readonly Color ColBtnYellowBg= new Color32(0x30, 0x28, 0x10, 0xFF);
        static readonly Color ColOverlay    = new Color32(0x00, 0x00, 0x00, 0x80);

        [MenuItem("Tools/Nodebreaker/Setup Popups (PPT Slide 5)")]
        static void SetupPopups()
        {
            // InGameUI가 붙은 Canvas를 찾기 (TitleScreenCanvas가 아닌 메인 Canvas)
            var inGameUI = Object.FindFirstObjectByType<UI.InGameUI>(FindObjectsInactive.Include);
            Canvas canvas = inGameUI != null ? inGameUI.GetComponent<Canvas>() : null;
            if (canvas == null)
            {
                // 폴백: "Canvas" 이름으로 찾기
                var allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                foreach (var c in allCanvases)
                {
                    if (c.gameObject.name == "Canvas")
                    {
                        canvas = c;
                        break;
                    }
                }
            }
            if (canvas == null)
            {
                Debug.LogError("[PopupSetup] Main Canvas not found!");
                return;
            }

            // 0. 잘못된 위치에 생성된 오브젝트 정리
            CleanupMisplacedPopups(canvas.transform);

            // 1. 설정 팝업
            SetupSettingsPopup(canvas.transform);

            // 2. 스킬 구매 팝업 - 기존 DetailPanel 보강
            SetupSkillPurchasePopup(canvas.transform);

            // 3. 타워 구매 패널
            SetupTowerPurchasePanel(canvas.transform);

            // 4. 타워 정보 툴팁
            SetupTowerInfoTooltip(canvas.transform);

            // 5. 방치 Bit 수령 팝업
            SetupIdleBitPopup(canvas.transform);

            // HubUI 참조 연결
            ConnectHubUIReferences(canvas.transform);

            // InGameUI 참조 연결
            ConnectInGameUIReferences(canvas.transform);

            EditorUtility.SetDirty(canvas.gameObject);
            Debug.Log("[PopupSetup] All popups created successfully!");
        }

        // ============================================================
        // 1. 설정 팝업 (400x300, Center, 테두리 #5B6B8A)
        // ============================================================
        static void SetupSettingsPopup(Transform canvasT)
        {
            // 오버레이
            var overlay = FindOrCreate(canvasT, "SettingsOverlay");
            StretchFull(overlay);
            var overlayImg = EnsureComponent<Image>(overlay);
            overlayImg.color = ColOverlay;
            overlayImg.raycastTarget = true;

            // 메인 패널
            var panel = FindOrCreate(overlay.transform, "SettingsPopup");
            var panelRect = panel.GetComponent<RectTransform>();
            CenterAnchored(panelRect, 400, 300);
            var panelImg = EnsureComponent<Image>(panel);
            panelImg.color = ColPanel;
            var panelOutline = EnsureComponent<Outline>(panel);
            panelOutline.effectColor = ColBorder;
            panelOutline.effectDistance = new Vector2(2, 2);

            // 스크립트
            var settingsScript = EnsureComponent<UI.SettingsPopup>(panel);

            // 제목: "설정" 20pt 굵게 #D8E4FF
            var titleObj = FindOrCreate(panel.transform, "TitleText");
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0, -15);
            titleRect.sizeDelta = new Vector2(-20, 40);
            var titleText = EnsureComponent<Text>(titleObj);
            titleText.text = "설정";
            titleText.fontSize = 20;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = ColTextMain;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 구분선
            var divider = FindOrCreate(panel.transform, "Divider");
            var divRect = divider.GetComponent<RectTransform>();
            divRect.anchorMin = new Vector2(0.05f, 1);
            divRect.anchorMax = new Vector2(0.95f, 1);
            divRect.pivot = new Vector2(0.5f, 1f);
            divRect.anchoredPosition = new Vector2(0, -60);
            divRect.sizeDelta = new Vector2(0, 1);
            var divImg = EnsureComponent<Image>(divider);
            divImg.color = ColBorder;

            // BGM 슬라이더
            float sliderY = -90;
            var bgmLabel = CreateLabel(panel.transform, "BGMLabel", "BGM", 14, ColTextSub,
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(30, sliderY), new Vector2(60, 25));

            var bgmSlider = CreateSlider(panel.transform, "BGMSlider",
                new Vector2(0.25f, 1), new Vector2(0.85f, 1), new Vector2(0.5f, 1f),
                new Vector2(0, sliderY - 3), new Vector2(0, 20));
            settingsScript.bgmSlider = bgmSlider.GetComponent<Slider>();

            var bgmValObj = CreateLabel(panel.transform, "BGMValueText", "100%", 12, ColTextMain,
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, sliderY), new Vector2(50, 25));
            settingsScript.bgmValueText = bgmValObj.GetComponent<Text>();

            // SFX 슬라이더
            float sfxY = -135;
            var sfxLabel = CreateLabel(panel.transform, "SFXLabel", "SFX", 14, ColTextSub,
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(30, sfxY), new Vector2(60, 25));

            var sfxSlider = CreateSlider(panel.transform, "SFXSlider",
                new Vector2(0.25f, 1), new Vector2(0.85f, 1), new Vector2(0.5f, 1f),
                new Vector2(0, sfxY - 3), new Vector2(0, 20));
            settingsScript.sfxSlider = sfxSlider.GetComponent<Slider>();

            var sfxValObj = CreateLabel(panel.transform, "SFXValueText", "100%", 12, ColTextMain,
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, sfxY), new Vector2(50, 25));
            settingsScript.sfxValueText = sfxValObj.GetComponent<Text>();

            // 닫기 버튼 (160x45)
            var closeBtn = CreateButton(panel.transform, "CloseButton", "닫기",
                160, 45, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 30), ColBrightPanel, ColBorder, ColTextMain, 15);
            settingsScript.closeButton = closeBtn.GetComponent<Button>();

            overlay.SetActive(false);

            EditorUtility.SetDirty(settingsScript);
        }

        // ============================================================
        // 2. 스킬 구매 팝업 보강 (기존 DetailPanel에 닫기 X + 추가 텍스트)
        // ============================================================
        static void SetupSkillPurchasePopup(Transform canvasT)
        {
            var hubPanel = canvasT.Find("HubPanel");
            if (hubPanel == null) { Debug.LogWarning("[PopupSetup] HubPanel not found, skip skill popup"); return; }

            var skillArea = hubPanel.Find("SkillArea");
            if (skillArea == null) { Debug.LogWarning("[PopupSetup] SkillArea not found"); return; }

            var detailPanel = skillArea.Find("DetailPanel");
            if (detailPanel == null) { Debug.LogWarning("[PopupSetup] DetailPanel not found"); return; }

            // DetailPanel 크기 조정 (380x340)
            var dpRect = detailPanel.GetComponent<RectTransform>();
            dpRect.sizeDelta = new Vector2(380, 340);

            // Outline 색상 변경
            var dpOutline = detailPanel.GetComponent<Outline>();
            if (dpOutline != null)
            {
                dpOutline.effectColor = ColNeonGreen;
                dpOutline.effectDistance = new Vector2(2, 2);
            }

            // 배경 색상 변경
            var dpImage = detailPanel.GetComponent<Image>();
            if (dpImage != null)
                dpImage.color = ColPanel;

            // DetailNameText 스타일 변경 (16pt 굵게 #2BFF88)
            var nameText = detailPanel.Find("DetailNameText")?.GetComponent<Text>();
            if (nameText != null)
            {
                nameText.fontSize = 16;
                nameText.fontStyle = FontStyle.Bold;
                nameText.color = ColNeonGreen;
            }

            // DetailCostText 색상 (15pt 굵게 #FFD84D)
            var costText = detailPanel.Find("DetailCostText")?.GetComponent<Text>();
            if (costText != null)
            {
                costText.fontSize = 15;
                costText.fontStyle = FontStyle.Bold;
                costText.color = ColYellow;
            }

            // DetailEffectText (13pt #D8E4FF)
            var effectText = detailPanel.Find("DetailEffectText")?.GetComponent<Text>();
            if (effectText != null)
            {
                effectText.fontSize = 13;
                effectText.color = ColTextMain;
            }

            // DetailDescText (13pt #AFC3E8)
            var descText = detailPanel.Find("DetailDescText")?.GetComponent<Text>();
            if (descText != null)
            {
                descText.fontSize = 13;
                descText.color = ColTextSub;
            }

            // 구분선 추가
            var sepObj = FindOrCreate(detailPanel, "Separator");
            var sepRect = sepObj.GetComponent<RectTransform>();
            sepRect.anchorMin = new Vector2(0.05f, 1);
            sepRect.anchorMax = new Vector2(0.95f, 1);
            sepRect.pivot = new Vector2(0.5f, 1f);
            sepRect.anchoredPosition = new Vector2(0, -45);
            sepRect.sizeDelta = new Vector2(0, 1);
            var sepImg = EnsureComponent<Image>(sepObj);
            sepImg.color = ColBorder;

            // 변경 전/후 텍스트
            var changeBeforeObj = FindOrCreate(detailPanel, "ChangeBeforeText");
            var changeBeforeRect = changeBeforeObj.GetComponent<RectTransform>();
            changeBeforeRect.anchorMin = new Vector2(0, 1);
            changeBeforeRect.anchorMax = new Vector2(1, 1);
            changeBeforeRect.pivot = new Vector2(0.5f, 1f);
            changeBeforeRect.anchoredPosition = new Vector2(0, -175);
            changeBeforeRect.sizeDelta = new Vector2(-30, 22);
            var changeBeforeText = EnsureComponent<Text>(changeBeforeObj);
            changeBeforeText.fontSize = 13;
            changeBeforeText.color = ColTextSub;
            changeBeforeText.alignment = TextAnchor.MiddleLeft;
            changeBeforeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var changeAfterObj = FindOrCreate(detailPanel, "ChangeAfterText");
            var changeAfterRect = changeAfterObj.GetComponent<RectTransform>();
            changeAfterRect.anchorMin = new Vector2(0, 1);
            changeAfterRect.anchorMax = new Vector2(1, 1);
            changeAfterRect.pivot = new Vector2(0.5f, 1f);
            changeAfterRect.anchoredPosition = new Vector2(0, -200);
            changeAfterRect.sizeDelta = new Vector2(-30, 22);
            var changeAfterText = EnsureComponent<Text>(changeAfterObj);
            changeAfterText.fontSize = 13;
            changeAfterText.color = ColTextSub;
            changeAfterText.alignment = TextAnchor.MiddleLeft;
            changeAfterText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // PurchaseButton 스타일 변경 (260x50, 채우기=#153020, 테두리=#2BFF88)
            var purchaseBtn = detailPanel.Find("PurchaseButton");
            if (purchaseBtn != null)
            {
                var pbRect = purchaseBtn.GetComponent<RectTransform>();
                pbRect.sizeDelta = new Vector2(260, 50);

                var pbImg = purchaseBtn.GetComponent<Image>();
                if (pbImg != null)
                    pbImg.color = ColBtnGreenBg;

                var pbOutline = purchaseBtn.GetComponent<Outline>();
                if (pbOutline != null)
                {
                    pbOutline.effectColor = ColNeonGreen;
                    pbOutline.effectDistance = new Vector2(2, 2);
                }

                // 버튼 텍스트 찾기/생성
                var pbTextObj = purchaseBtn.Find("Text");
                if (pbTextObj == null) pbTextObj = purchaseBtn.Find("PurchaseText");
                if (pbTextObj != null)
                {
                    var pbText = pbTextObj.GetComponent<Text>();
                    if (pbText != null)
                    {
                        pbText.text = "구매";
                        pbText.fontSize = 15;
                        pbText.fontStyle = FontStyle.Bold;
                        pbText.color = ColNeonGreen;
                    }
                }
            }

            // 닫기 X 버튼 (30x25, 우측 상단)
            CreateCloseButton(detailPanel, new Vector2(-5, -5));
        }

        // ============================================================
        // 3. 타워 구매 패널 (500x340, +구매 버튼 위, 테두리 #FFD84D)
        // ============================================================
        static void SetupTowerPurchasePanel(Transform canvasT)
        {
            // BottomBar 아래 (인게임)
            var bottomBar = canvasT.Find("BottomBar");

            var panel = FindOrCreate(canvasT, "TowerPurchasePanel");
            var panelRect = panel.GetComponent<RectTransform>();
            // BottomBar 위에 배치
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(0, 0);
            panelRect.pivot = new Vector2(0, 0);
            panelRect.anchoredPosition = new Vector2(10, 65);
            panelRect.sizeDelta = new Vector2(500, 340);

            var panelImg = EnsureComponent<Image>(panel);
            panelImg.color = ColPanel;
            var panelOutline = EnsureComponent<Outline>(panel);
            panelOutline.effectColor = ColYellow;
            panelOutline.effectDistance = new Vector2(2, 2);

            var script = EnsureComponent<UI.TowerPurchasePanel>(panel);

            // 제목: "타워 구매" 18pt 굵게 #FFD84D
            var titleObj = FindOrCreate(panel.transform, "TitleText");
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0, -10);
            titleRect.sizeDelta = new Vector2(-20, 35);
            var titleText = EnsureComponent<Text>(titleObj);
            titleText.text = "타워 구매";
            titleText.fontSize = 18;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = ColYellow;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            script.titleText = titleText;

            // 구분선
            var sep = FindOrCreate(panel.transform, "Separator");
            var sepRect = sep.GetComponent<RectTransform>();
            sepRect.anchorMin = new Vector2(0.03f, 1);
            sepRect.anchorMax = new Vector2(0.97f, 1);
            sepRect.pivot = new Vector2(0.5f, 1f);
            sepRect.anchoredPosition = new Vector2(0, -50);
            sepRect.sizeDelta = new Vector2(0, 1);
            EnsureComponent<Image>(sep).color = ColBorder;

            // Content (세로 레이아웃)
            var content = FindOrCreate(panel.transform, "Content");
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = new Vector2(10, 10);
            contentRect.offsetMax = new Vector2(-10, -55);
            var vlg = EnsureComponent<VerticalLayoutGroup>(content);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.spacing = 5;
            vlg.padding = new RectOffset(5, 5, 5, 5);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            script.contentParent = content.transform;

            // 닫기 X 버튼
            script.closeButton = CreateCloseButton(panel.transform, new Vector2(-5, -5));

            panel.SetActive(false);
            EditorUtility.SetDirty(script);
        }

        // ============================================================
        // 4. 타워 정보 툴팁 (350x300, 테두리 #37B6FF)
        // ============================================================
        static void SetupTowerInfoTooltip(Transform canvasT)
        {
            var panel = FindOrCreate(canvasT, "TowerInfoTooltip");
            var panelRect = panel.GetComponent<RectTransform>();
            CenterAnchored(panelRect, 350, 300);
            var panelImg = EnsureComponent<Image>(panel);
            panelImg.color = ColPanel;
            var panelOutline = EnsureComponent<Outline>(panel);
            panelOutline.effectColor = ColNeonBlue;
            panelOutline.effectDistance = new Vector2(2, 2);

            var script = EnsureComponent<UI.TowerInfoTooltip>(panel);

            // 이름 (16pt 굵게 #37B6FF)
            var nameObj = FindOrCreate(panel.transform, "NameText");
            var nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 1);
            nameRect.anchorMax = new Vector2(0.7f, 1);
            nameRect.pivot = new Vector2(0, 1);
            nameRect.anchoredPosition = new Vector2(15, -10);
            nameRect.sizeDelta = new Vector2(0, 30);
            var nameText = EnsureComponent<Text>(nameObj);
            nameText.fontSize = 16;
            nameText.fontStyle = FontStyle.Bold;
            nameText.color = ColNeonBlue;
            nameText.alignment = TextAnchor.MiddleLeft;
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            script.nameText = nameText;

            // 레벨 (16pt 굵게 #37B6FF)
            var lvlObj = FindOrCreate(panel.transform, "LevelText");
            var lvlRect = lvlObj.GetComponent<RectTransform>();
            lvlRect.anchorMin = new Vector2(0.7f, 1);
            lvlRect.anchorMax = new Vector2(1, 1);
            lvlRect.pivot = new Vector2(1, 1);
            lvlRect.anchoredPosition = new Vector2(-15, -10);
            lvlRect.sizeDelta = new Vector2(0, 30);
            var lvlText = EnsureComponent<Text>(lvlObj);
            lvlText.fontSize = 16;
            lvlText.fontStyle = FontStyle.Bold;
            lvlText.color = ColNeonBlue;
            lvlText.alignment = TextAnchor.MiddleRight;
            lvlText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            script.levelText = lvlText;

            // 구분선
            var sep = FindOrCreate(panel.transform, "Separator");
            var sepRect = sep.GetComponent<RectTransform>();
            sepRect.anchorMin = new Vector2(0.05f, 1);
            sepRect.anchorMax = new Vector2(0.95f, 1);
            sepRect.pivot = new Vector2(0.5f, 1f);
            sepRect.anchoredPosition = new Vector2(0, -45);
            sepRect.sizeDelta = new Vector2(0, 1);
            EnsureComponent<Image>(sep).color = ColBorder;

            // 스탯 라벨 + 값
            float y = -60;
            var dmgObj = CreateStatRow(panel.transform, "DamageRow", "공격력", y); y -= 30;
            script.damageText = dmgObj.transform.Find("ValueText")?.GetComponent<Text>();

            var atkSpdObj = CreateStatRow(panel.transform, "AtkSpeedRow", "공격속도", y); y -= 30;
            script.attackSpeedText = atkSpdObj.transform.Find("ValueText")?.GetComponent<Text>();

            var rangeObj = CreateStatRow(panel.transform, "RangeRow", "사거리", y); y -= 30;
            script.rangeText = rangeObj.transform.Find("ValueText")?.GetComponent<Text>();

            // 구분선
            y -= 5;
            var sep2 = FindOrCreate(panel.transform, "Separator2");
            var sep2Rect = sep2.GetComponent<RectTransform>();
            sep2Rect.anchorMin = new Vector2(0.05f, 1);
            sep2Rect.anchorMax = new Vector2(0.95f, 1);
            sep2Rect.pivot = new Vector2(0.5f, 1f);
            sep2Rect.anchoredPosition = new Vector2(0, y);
            sep2Rect.sizeDelta = new Vector2(0, 1);
            EnsureComponent<Image>(sep2).color = ColBorder;
            y -= 10;

            // 판매 (11pt #FF9A3D)
            var sellObj = FindOrCreate(panel.transform, "SellText");
            var sellRect = sellObj.GetComponent<RectTransform>();
            sellRect.anchorMin = new Vector2(0, 1);
            sellRect.anchorMax = new Vector2(1, 1);
            sellRect.pivot = new Vector2(0.5f, 1f);
            sellRect.anchoredPosition = new Vector2(0, y - 5);
            sellRect.sizeDelta = new Vector2(-30, 22);
            var sellText = EnsureComponent<Text>(sellObj);
            sellText.fontSize = 11;
            sellText.color = ColOrange;
            sellText.alignment = TextAnchor.MiddleLeft;
            sellText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            script.sellText = sellText;

            // 판매 버튼
            var sellBtnObj = CreateButton(panel.transform, "SellButton", "판매",
                100, 35, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 20), ColBrightPanel, ColOrange, ColOrange, 13);
            script.sellButton = sellBtnObj.GetComponent<Button>();

            // 닫기 X
            script.closeButton = CreateCloseButton(panel.transform, new Vector2(-5, -5));

            panel.SetActive(false);
            EditorUtility.SetDirty(script);
        }

        // ============================================================
        // 5. 방치 Bit 수령 팝업 (420x250, 테두리 #FFD84D)
        // ============================================================
        static void SetupIdleBitPopup(Transform canvasT)
        {
            // 오버레이
            var overlay = FindOrCreate(canvasT, "IdleBitOverlay");
            StretchFull(overlay);
            var overlayImg = EnsureComponent<Image>(overlay);
            overlayImg.color = ColOverlay;
            overlayImg.raycastTarget = true;

            var panel = FindOrCreate(overlay.transform, "IdleBitPopup");
            var panelRect = panel.GetComponent<RectTransform>();
            CenterAnchored(panelRect, 420, 250);
            var panelImg = EnsureComponent<Image>(panel);
            panelImg.color = ColPanel;
            var panelOutline = EnsureComponent<Outline>(panel);
            panelOutline.effectColor = ColYellow;
            panelOutline.effectDistance = new Vector2(2, 2);

            var script = EnsureComponent<UI.IdleBitPopup>(panel);

            // 제목: "방치 보상" 20pt 굵게 #FFD84D
            var titleObj = FindOrCreate(panel.transform, "TitleText");
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0, -15);
            titleRect.sizeDelta = new Vector2(-20, 35);
            var titleText = EnsureComponent<Text>(titleObj);
            titleText.text = "방치 보상";
            titleText.fontSize = 20;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = ColYellow;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            script.titleText = titleText;

            // 구분선
            var sep = FindOrCreate(panel.transform, "Separator");
            var sepRect = sep.GetComponent<RectTransform>();
            sepRect.anchorMin = new Vector2(0.05f, 1);
            sepRect.anchorMax = new Vector2(0.95f, 1);
            sepRect.pivot = new Vector2(0.5f, 1f);
            sepRect.anchoredPosition = new Vector2(0, -55);
            sepRect.sizeDelta = new Vector2(0, 1);
            EnsureComponent<Image>(sep).color = ColBorder;

            // Bit 금액 (28pt 굵게 #2BFF88)
            var bitObj = FindOrCreate(panel.transform, "BitAmountText");
            var bitRect = bitObj.GetComponent<RectTransform>();
            bitRect.anchorMin = new Vector2(0, 1);
            bitRect.anchorMax = new Vector2(1, 1);
            bitRect.pivot = new Vector2(0.5f, 1f);
            bitRect.anchoredPosition = new Vector2(0, -75);
            bitRect.sizeDelta = new Vector2(-20, 40);
            var bitText = EnsureComponent<Text>(bitObj);
            bitText.text = "+0 Bit";
            bitText.fontSize = 28;
            bitText.fontStyle = FontStyle.Bold;
            bitText.color = ColNeonGreen;
            bitText.alignment = TextAnchor.MiddleCenter;
            bitText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            script.bitAmountText = bitText;

            // 부재 시간 (12pt #AFC3E8)
            var timeObj = FindOrCreate(panel.transform, "AbsenceTimeText");
            var timeRect = timeObj.GetComponent<RectTransform>();
            timeRect.anchorMin = new Vector2(0, 1);
            timeRect.anchorMax = new Vector2(1, 1);
            timeRect.pivot = new Vector2(0.5f, 1f);
            timeRect.anchoredPosition = new Vector2(0, -120);
            timeRect.sizeDelta = new Vector2(-20, 22);
            var timeText = EnsureComponent<Text>(timeObj);
            timeText.text = "부재 시간: 0분";
            timeText.fontSize = 12;
            timeText.color = ColTextSub;
            timeText.alignment = TextAnchor.MiddleCenter;
            timeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            script.absenceTimeText = timeText;

            // 수령 버튼 (220x50, 채우기=#302810, 테두리=#FFD84D)
            var claimBtn = CreateButton(panel.transform, "ClaimButton", "수령",
                220, 50, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 25), ColBtnYellowBg, ColYellow, ColYellow, 16);
            script.claimButton = claimBtn.GetComponent<Button>();
            // 버튼 텍스트 참조
            var claimBtnText = claimBtn.transform.Find("Text")?.GetComponent<Text>();
            script.claimButtonText = claimBtnText;

            overlay.SetActive(false);
            EditorUtility.SetDirty(script);
        }

        // ============================================================
        // HubUI 참조 연결
        // ============================================================
        static void ConnectHubUIReferences(Transform canvasT)
        {
            var hubPanel = canvasT.Find("HubPanel");
            if (hubPanel == null) return;

            var hubUI = hubPanel.GetComponent<UI.HubUI>();
            if (hubUI == null) return;

            // SettingsPopup 참조
            var settingsOverlay = canvasT.Find("SettingsOverlay");
            if (settingsOverlay != null)
            {
                var settingsPopup = settingsOverlay.Find("SettingsPopup")?.GetComponent<UI.SettingsPopup>();
                hubUI.settingsPopup = settingsPopup;
            }

            // IdleBitPopup 참조
            var idleBitOverlay = canvasT.Find("IdleBitOverlay");
            if (idleBitOverlay != null)
            {
                var idleBitPopup = idleBitOverlay.Find("IdleBitPopup")?.GetComponent<UI.IdleBitPopup>();
                hubUI.idleBitPopup = idleBitPopup;
            }

            // DetailPanel 추가 참조
            var skillArea = hubPanel.Find("SkillArea");
            if (skillArea != null)
            {
                var detailPanel = skillArea.Find("DetailPanel");
                if (detailPanel != null)
                {
                    hubUI.detailChangeBeforeText = detailPanel.Find("ChangeBeforeText")?.GetComponent<Text>();
                    hubUI.detailChangeAfterText = detailPanel.Find("ChangeAfterText")?.GetComponent<Text>();
                    hubUI.detailCloseButton = detailPanel.Find("CloseButton")?.GetComponent<Button>();

                    var purchaseBtn = detailPanel.Find("PurchaseButton");
                    if (purchaseBtn != null)
                    {
                        var ptObj = purchaseBtn.Find("Text");
                        if (ptObj == null) ptObj = purchaseBtn.Find("PurchaseText");
                        if (ptObj != null)
                            hubUI.purchaseButtonText = ptObj.GetComponent<Text>();
                    }
                }
            }

            EditorUtility.SetDirty(hubUI);
        }

        // ============================================================
        // InGameUI 참조 연결
        // ============================================================
        static void ConnectInGameUIReferences(Transform canvasT)
        {
            var inGameUI = canvasT.GetComponent<UI.InGameUI>();
            if (inGameUI == null) return;

            // TowerPurchasePanel
            var tpp = canvasT.Find("TowerPurchasePanel")?.GetComponent<UI.TowerPurchasePanel>();
            inGameUI.towerPurchasePanel = tpp;

            // TowerInfoTooltip
            var tit = canvasT.Find("TowerInfoTooltip")?.GetComponent<UI.TowerInfoTooltip>();
            inGameUI.towerInfoTooltip = tit;

            // +구매 버튼: BottomBar에 TowerInventory 옆에 추가
            var bottomBar = canvasT.Find("BottomBar");
            if (bottomBar != null)
            {
                var purchaseBtn = FindOrCreate(bottomBar, "TowerPurchaseButton");
                var pbRect = purchaseBtn.GetComponent<RectTransform>();
                pbRect.anchorMin = new Vector2(0, 0);
                pbRect.anchorMax = new Vector2(0, 1);
                pbRect.pivot = new Vector2(0, 0.5f);
                pbRect.anchoredPosition = new Vector2(5, 0);
                pbRect.sizeDelta = new Vector2(55, 0);

                var pbImg = EnsureComponent<Image>(purchaseBtn);
                pbImg.color = ColBtnGreenBg;
                var pbOutline = EnsureComponent<Outline>(purchaseBtn);
                pbOutline.effectColor = ColNeonGreen;
                pbOutline.effectDistance = new Vector2(1, 1);
                var btn = EnsureComponent<Button>(purchaseBtn);
                btn.targetGraphic = pbImg;

                var btnTextObj = FindOrCreate(purchaseBtn.transform, "Text");
                var btnTextRect = btnTextObj.GetComponent<RectTransform>();
                StretchFull(btnTextObj);
                var btnText = EnsureComponent<Text>(btnTextObj);
                btnText.text = "+";
                btnText.fontSize = 24;
                btnText.fontStyle = FontStyle.Bold;
                btnText.color = ColNeonGreen;
                btnText.alignment = TextAnchor.MiddleCenter;
                btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

                inGameUI.towerPurchaseButton = btn;
            }

            EditorUtility.SetDirty(inGameUI);
        }

        // ============================================================
        // Helper methods
        // ============================================================

        static void CleanupMisplacedPopups(Transform correctCanvas)
        {
            // TitleScreenCanvas 등 다른 Canvas에 잘못 생성된 팝업 제거
            string[] popupNames = { "SettingsOverlay", "TowerPurchasePanel", "TowerInfoTooltip", "IdleBitOverlay", "TowerPurchaseButton" };
            var allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var canvas in allCanvases)
            {
                if (canvas.transform == correctCanvas) continue;

                foreach (var name in popupNames)
                {
                    var child = canvas.transform.Find(name);
                    if (child != null)
                    {
                        Debug.Log($"[PopupSetup] Removing misplaced '{name}' from '{canvas.name}'");
                        Object.DestroyImmediate(child.gameObject);
                    }
                }
            }
        }

        static Button CreateCloseButton(Transform parent, Vector2 offset)
        {
            var closeObj = FindOrCreate(parent, "CloseButton");
            var closeRect = closeObj.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1, 1);
            closeRect.anchorMax = new Vector2(1, 1);
            closeRect.pivot = new Vector2(1, 1);
            closeRect.anchoredPosition = offset;
            closeRect.sizeDelta = new Vector2(30, 25);
            var closeImg = EnsureComponent<Image>(closeObj);
            closeImg.color = new Color(0, 0, 0, 0); // 투명 배경
            var btn = EnsureComponent<Button>(closeObj);
            btn.targetGraphic = closeImg;

            // 텍스트는 자식 오브젝트로 (Image + Text 동시 불가)
            var textObj = FindOrCreate(closeObj.transform, "Text");
            StretchFull(textObj);
            var t = EnsureComponent<Text>(textObj);
            t.text = "X";
            t.fontSize = 16;
            t.fontStyle = FontStyle.Bold;
            t.color = ColTextSub;
            t.alignment = TextAnchor.MiddleCenter;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.raycastTarget = false;

            return btn;
        }

        static GameObject FindOrCreate(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null) return existing.gameObject;

            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        static void StretchFull(GameObject go)
        {
            var rect = go.GetComponent<RectTransform>();
            if (rect == null) rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
        }

        static void CenterAnchored(RectTransform rect, float width, float height)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = Vector2.zero;
        }

        static T EnsureComponent<T>(GameObject go) where T : Component
        {
            var comp = go.GetComponent<T>();
            if (comp == null) comp = go.AddComponent<T>();
            return comp;
        }

        static GameObject CreateLabel(Transform parent, string name, string text,
            int fontSize, Color color,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var obj = FindOrCreate(parent, name);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;
            var t = EnsureComponent<Text>(obj);
            t.text = text;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = TextAnchor.MiddleLeft;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return obj;
        }

        static GameObject CreateSlider(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var obj = FindOrCreate(parent, name);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;

            var slider = EnsureComponent<Slider>(obj);

            // Background
            var bg = FindOrCreate(obj.transform, "Background");
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.25f);
            bgRect.anchorMax = new Vector2(1, 0.75f);
            bgRect.sizeDelta = Vector2.zero;
            bgRect.anchoredPosition = Vector2.zero;
            var bgImg = EnsureComponent<Image>(bg);
            bgImg.color = ColBrightPanel;

            // Fill Area
            var fillArea = FindOrCreate(obj.transform, "Fill Area");
            var faRect = fillArea.GetComponent<RectTransform>();
            faRect.anchorMin = new Vector2(0, 0.25f);
            faRect.anchorMax = new Vector2(1, 0.75f);
            faRect.sizeDelta = Vector2.zero;
            faRect.anchoredPosition = Vector2.zero;

            var fill = FindOrCreate(fillArea.transform, "Fill");
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0.5f, 1);
            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;
            var fillImg = EnsureComponent<Image>(fill);
            fillImg.color = ColNeonBlue;

            // Handle Slide Area
            var handleArea = FindOrCreate(obj.transform, "Handle Slide Area");
            var haRect = handleArea.GetComponent<RectTransform>();
            haRect.anchorMin = Vector2.zero;
            haRect.anchorMax = Vector2.one;
            haRect.sizeDelta = Vector2.zero;
            haRect.anchoredPosition = Vector2.zero;

            var handle = FindOrCreate(handleArea.transform, "Handle");
            var hRect = handle.GetComponent<RectTransform>();
            hRect.sizeDelta = new Vector2(16, 20);
            var hImg = EnsureComponent<Image>(handle);
            hImg.color = Color.white;

            slider.fillRect = fillRect;
            slider.handleRect = hRect;
            slider.targetGraphic = hImg;

            return obj;
        }

        static GameObject CreateButton(Transform parent, string name, string text,
            float width, float height,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Color bgColor, Color outlineColor, Color textColor, int fontSize)
        {
            var obj = FindOrCreate(parent, name);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(width, height);

            var img = EnsureComponent<Image>(obj);
            img.color = bgColor;
            var btn = EnsureComponent<Button>(obj);
            btn.targetGraphic = img;
            var outline = EnsureComponent<Outline>(obj);
            outline.effectColor = outlineColor;
            outline.effectDistance = new Vector2(2, 2);

            var textObj = FindOrCreate(obj.transform, "Text");
            StretchFull(textObj);
            var t = EnsureComponent<Text>(textObj);
            t.text = text;
            t.fontSize = fontSize;
            t.fontStyle = FontStyle.Bold;
            t.color = textColor;
            t.alignment = TextAnchor.MiddleCenter;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            return obj;
        }

        static GameObject CreateStatRow(Transform parent, string name, string label, float y)
        {
            var row = FindOrCreate(parent, name);
            var rowRect = row.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0, 1);
            rowRect.anchorMax = new Vector2(1, 1);
            rowRect.pivot = new Vector2(0.5f, 1f);
            rowRect.anchoredPosition = new Vector2(0, y);
            rowRect.sizeDelta = new Vector2(-30, 25);

            // 라벨 (12pt #AFC3E8)
            var labelObj = FindOrCreate(row.transform, "LabelText");
            var lblRect = labelObj.GetComponent<RectTransform>();
            lblRect.anchorMin = new Vector2(0, 0);
            lblRect.anchorMax = new Vector2(0.5f, 1);
            lblRect.sizeDelta = Vector2.zero;
            lblRect.anchoredPosition = Vector2.zero;
            var lblText = EnsureComponent<Text>(labelObj);
            lblText.text = label;
            lblText.fontSize = 12;
            lblText.color = ColTextSub;
            lblText.alignment = TextAnchor.MiddleLeft;
            lblText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 값 (12pt 굵게 #D8E4FF)
            var valObj = FindOrCreate(row.transform, "ValueText");
            var valRect = valObj.GetComponent<RectTransform>();
            valRect.anchorMin = new Vector2(0.5f, 0);
            valRect.anchorMax = new Vector2(1, 1);
            valRect.sizeDelta = Vector2.zero;
            valRect.anchoredPosition = Vector2.zero;
            var valText = EnsureComponent<Text>(valObj);
            valText.text = "-";
            valText.fontSize = 12;
            valText.fontStyle = FontStyle.Bold;
            valText.color = ColTextMain;
            valText.alignment = TextAnchor.MiddleRight;
            valText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            return row;
        }
    }
}
