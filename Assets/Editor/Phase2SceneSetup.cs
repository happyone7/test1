using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Nodebreaker.Editor
{
    public class Phase2SceneSetup
    {
        [MenuItem("Tools/Nodebreaker/Setup Phase 2 (Hub + MetaManager)")]
        static void SetupPhase2()
        {
            // 1. MetaManager 싱글톤 추가
            var metaObj = CreateSingleton<Core.MetaManager>("MetaManager");
            var meta = metaObj.GetComponent<Core.MetaManager>();

            // 스킬 데이터 에셋 할당
            var skills = new Data.SkillNodeData[3];
            skills[0] = AssetDatabase.LoadAssetAtPath<Data.SkillNodeData>("Assets/Data/Skills/Skill_AttackDamage.asset");
            skills[1] = AssetDatabase.LoadAssetAtPath<Data.SkillNodeData>("Assets/Data/Skills/Skill_AttackSpeed.asset");
            skills[2] = AssetDatabase.LoadAssetAtPath<Data.SkillNodeData>("Assets/Data/Skills/Skill_BaseHp.asset");
            meta.allSkills = skills;

            // 2. GameManager의 autoStartRun 비활성화 (이미 제거됨, 확인용)
            var gm = Object.FindFirstObjectByType<Core.GameManager>();
            if (gm != null)
            {
                EditorUtility.SetDirty(gm);
            }

            // 3. Canvas 찾기
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // 4. HubPanel 생성
            var hubPanelObj = FindOrCreateChild(canvas.transform, "HubPanel");
            var hubUI = hubPanelObj.GetComponent<UI.HubUI>();
            if (hubUI == null)
                hubUI = hubPanelObj.AddComponent<UI.HubUI>();

            // HubPanel을 전체 화면으로 설정
            var hubRect = hubPanelObj.GetComponent<RectTransform>();
            hubRect.anchorMin = Vector2.zero;
            hubRect.anchorMax = Vector2.one;
            hubRect.offsetMin = Vector2.zero;
            hubRect.offsetMax = Vector2.zero;

            // 배경 이미지 (어두운 반투명)
            var hubBg = hubPanelObj.GetComponent<Image>();
            if (hubBg == null)
                hubBg = hubPanelObj.AddComponent<Image>();
            hubBg.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);

            // 5. 상단 재화 바 (컨테이너 + 배경)
            var topBarObj = FindOrCreateChild(hubPanelObj.transform, "TopBar");
            var topBarRect = topBarObj.GetComponent<RectTransform>();
            topBarRect.anchorMin = new Vector2(0, 1);
            topBarRect.anchorMax = new Vector2(1, 1);
            topBarRect.offsetMin = new Vector2(0, -60);
            topBarRect.offsetMax = new Vector2(0, 0);
            var topBarBg = topBarObj.GetComponent<Image>();
            if (topBarBg == null)
                topBarBg = topBarObj.AddComponent<Image>();
            topBarBg.color = new Color(0, 0, 0, 0.5f);

            var bitText = CreateUIText(topBarObj.transform, "TotalBitText", "Bit: 0",
                new Vector2(0, 0), new Vector2(0.3f, 1),
                Vector2.zero, Vector2.zero);
            bitText.alignment = TextAnchor.MiddleCenter;
            bitText.fontSize = 24;
            hubUI.totalBitText = bitText;

            var coreText = CreateUIText(topBarObj.transform, "TotalCoreText", "Core: 0",
                new Vector2(0.3f, 0), new Vector2(0.6f, 1),
                Vector2.zero, Vector2.zero);
            coreText.alignment = TextAnchor.MiddleCenter;
            coreText.fontSize = 24;
            hubUI.totalCoreText = coreText;

            // 스테이지 드롭다운
            var stageDropdownObj = FindOrCreateChild(topBarObj.transform, "StageDropdown");
            var stageDropdownRect = stageDropdownObj.GetComponent<RectTransform>();
            stageDropdownRect.anchorMin = new Vector2(0.6f, 0.1f);
            stageDropdownRect.anchorMax = new Vector2(0.95f, 0.9f);
            stageDropdownRect.offsetMin = Vector2.zero;
            stageDropdownRect.offsetMax = Vector2.zero;
            var stageDropdown = stageDropdownObj.GetComponent<Dropdown>();
            if (stageDropdown == null)
                stageDropdown = stageDropdownObj.AddComponent<Dropdown>();
            // 레이블 텍스트
            var dropdownLabel = CreateUIText(stageDropdownObj.transform, "Label", "Stage 1",
                Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-25, 0));
            dropdownLabel.alignment = TextAnchor.MiddleLeft;
            dropdownLabel.fontSize = 20;
            stageDropdown.captionText = dropdownLabel;
            // 배경 이미지
            var dropdownBg = stageDropdownObj.GetComponent<Image>();
            if (dropdownBg == null)
                dropdownBg = stageDropdownObj.AddComponent<Image>();
            dropdownBg.color = new Color(0.2f, 0.2f, 0.3f, 1f);
            hubUI.stageDropdown = stageDropdown;

            // 6. 스킬 노드 영역 (중앙)
            var skillArea = FindOrCreateChild(hubPanelObj.transform, "SkillArea");
            var skillAreaRect = skillArea.GetComponent<RectTransform>();
            skillAreaRect.anchorMin = new Vector2(0.1f, 0.25f);
            skillAreaRect.anchorMax = new Vector2(0.9f, 0.85f);
            skillAreaRect.offsetMin = Vector2.zero;
            skillAreaRect.offsetMax = Vector2.zero;

            // 타이틀
            var titleText = CreateUIText(skillArea.transform, "TitleText", "SKILL TREE",
                new Vector2(0, 0.85f), new Vector2(1, 1),
                Vector2.zero, Vector2.zero);
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.fontSize = 30;
            titleText.fontStyle = FontStyle.Bold;

            // 3개 스킬 노드 생성
            var skillNodes = new UI.SkillNodeUI[3];
            string[] skillNames = { "공격력 강화", "공격속도 강화", "기지 HP 강화" };
            Vector2[] nodeAnchors = {
                new Vector2(0.15f, 0.4f),  // 왼쪽
                new Vector2(0.5f, 0.4f),   // 중앙
                new Vector2(0.85f, 0.4f)   // 오른쪽
            };

            for (int i = 0; i < 3; i++)
            {
                var nodeObj = FindOrCreateChild(skillArea.transform, $"SkillNode_{i}");
                var nodeRect = nodeObj.GetComponent<RectTransform>();
                nodeRect.anchorMin = nodeAnchors[i] - new Vector2(0.12f, 0.15f);
                nodeRect.anchorMax = nodeAnchors[i] + new Vector2(0.12f, 0.15f);
                nodeRect.offsetMin = Vector2.zero;
                nodeRect.offsetMax = Vector2.zero;

                // 배경
                var nodeBg = nodeObj.GetComponent<Image>();
                if (nodeBg == null)
                    nodeBg = nodeObj.AddComponent<Image>();
                nodeBg.color = new Color(0.2f, 0.25f, 0.35f, 1f);

                // 버튼
                var nodeBtn = nodeObj.GetComponent<Button>();
                if (nodeBtn == null)
                    nodeBtn = nodeObj.AddComponent<Button>();

                // 레벨 텍스트
                var levelText = CreateUIText(nodeObj.transform, "LevelText", "Lv.0",
                    new Vector2(0, 0), new Vector2(1, 0.35f),
                    Vector2.zero, Vector2.zero);
                levelText.alignment = TextAnchor.MiddleCenter;
                levelText.fontSize = 18;

                // 이름 텍스트
                var nameText = CreateUIText(nodeObj.transform, "NameText", skillNames[i],
                    new Vector2(0, 0.35f), new Vector2(1, 0.7f),
                    Vector2.zero, Vector2.zero);
                nameText.alignment = TextAnchor.MiddleCenter;
                nameText.fontSize = 16;

                // SkillNodeUI 컴포넌트
                var skillNodeUI = nodeObj.GetComponent<UI.SkillNodeUI>();
                if (skillNodeUI == null)
                    skillNodeUI = nodeObj.AddComponent<UI.SkillNodeUI>();
                skillNodeUI.data = skills[i];
                skillNodeUI.levelText = levelText;
                skillNodeUI.button = nodeBtn;
                skillNodes[i] = skillNodeUI;
            }
            hubUI.skillNodes = skillNodes;

            // 7. 상세 패널 (하단, 출전 버튼 위에 배치)
            var detailPanel = FindOrCreateChild(hubPanelObj.transform, "DetailPanel");
            var detailRect = detailPanel.GetComponent<RectTransform>();
            detailRect.anchorMin = new Vector2(0.15f, 0.13f);
            detailRect.anchorMax = new Vector2(0.65f, 0.24f);
            detailRect.offsetMin = Vector2.zero;
            detailRect.offsetMax = Vector2.zero;
            var detailBg = detailPanel.GetComponent<Image>();
            if (detailBg == null)
                detailBg = detailPanel.AddComponent<Image>();
            detailBg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);
            hubUI.detailPanel = detailPanel;

            var dNameText = CreateUIText(detailPanel.transform, "DetailNameText", "스킬 이름",
                new Vector2(0.02f, 0.7f), new Vector2(0.5f, 1),
                Vector2.zero, Vector2.zero);
            dNameText.fontSize = 20;
            dNameText.fontStyle = FontStyle.Bold;
            hubUI.detailNameText = dNameText;

            var dLevelText = CreateUIText(detailPanel.transform, "DetailLevelText", "Lv.0/20",
                new Vector2(0.5f, 0.7f), new Vector2(1, 1),
                Vector2.zero, Vector2.zero);
            dLevelText.fontSize = 18;
            dLevelText.alignment = TextAnchor.MiddleRight;
            hubUI.detailLevelText = dLevelText;

            var dDescText = CreateUIText(detailPanel.transform, "DetailDescText", "설명",
                new Vector2(0.02f, 0.35f), new Vector2(0.98f, 0.7f),
                Vector2.zero, Vector2.zero);
            dDescText.fontSize = 14;
            hubUI.detailDescText = dDescText;

            var dEffectText = CreateUIText(detailPanel.transform, "DetailEffectText", "효과: +0%",
                new Vector2(0.02f, 0), new Vector2(0.5f, 0.35f),
                Vector2.zero, Vector2.zero);
            dEffectText.fontSize = 16;
            hubUI.detailEffectText = dEffectText;

            var dCostText = CreateUIText(detailPanel.transform, "DetailCostText", "비용: 50 Bit",
                new Vector2(0.5f, 0), new Vector2(1, 0.35f),
                Vector2.zero, Vector2.zero);
            dCostText.fontSize = 16;
            dCostText.alignment = TextAnchor.MiddleRight;
            hubUI.detailCostText = dCostText;

            // 8. 구매 버튼 (상세 패널 오른쪽, 작은 크기)
            var purchaseBtnObj = FindOrCreateChild(hubPanelObj.transform, "PurchaseButton");
            var purchaseRect = purchaseBtnObj.GetComponent<RectTransform>();
            purchaseRect.anchorMin = new Vector2(0.67f, 0.15f);
            purchaseRect.anchorMax = new Vector2(0.76f, 0.23f);
            purchaseRect.offsetMin = Vector2.zero;
            purchaseRect.offsetMax = Vector2.zero;
            var purchaseBg = purchaseBtnObj.GetComponent<Image>();
            if (purchaseBg == null)
                purchaseBg = purchaseBtnObj.AddComponent<Image>();
            purchaseBg.color = new Color(0.2f, 0.6f, 0.3f, 1f);
            var purchaseBtn = purchaseBtnObj.GetComponent<Button>();
            if (purchaseBtn == null)
                purchaseBtn = purchaseBtnObj.AddComponent<Button>();
            var purchaseBtnText = CreateUIText(purchaseBtnObj.transform, "Text", "구매",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            purchaseBtnText.alignment = TextAnchor.MiddleCenter;
            purchaseBtnText.fontSize = 22;
            purchaseBtnText.fontStyle = FontStyle.Bold;
            hubUI.purchaseButton = purchaseBtn;

            // 9. 출전 버튼 (하단 중앙, 크고 눈에 띄게 — DetailPanel 아래)
            var startBtnObj = FindOrCreateChild(hubPanelObj.transform, "StartRunButton");
            var startRect = startBtnObj.GetComponent<RectTransform>();
            startRect.anchorMin = new Vector2(0.25f, 0.02f);
            startRect.anchorMax = new Vector2(0.75f, 0.11f);
            startRect.offsetMin = Vector2.zero;
            startRect.offsetMax = Vector2.zero;
            var startBg = startBtnObj.GetComponent<Image>();
            if (startBg == null)
                startBg = startBtnObj.AddComponent<Image>();
            startBg.color = new Color(0.9f, 0.45f, 0.1f, 1f);
            var startBtn = startBtnObj.GetComponent<Button>();
            if (startBtn == null)
                startBtn = startBtnObj.AddComponent<Button>();
            var startBtnText = CreateUIText(startBtnObj.transform, "Text", "▶  출전!",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            startBtnText.alignment = TextAnchor.MiddleCenter;
            startBtnText.fontSize = 28;
            startBtnText.fontStyle = FontStyle.Bold;
            hubUI.startRunButton = startBtn;

            // 마킹
            EditorUtility.SetDirty(hubUI);
            EditorUtility.SetDirty(meta);

            Debug.Log("[Nodebreaker] Phase 2 씬 설정 완료! MetaManager + HubPanel 추가됨.");
        }

        static GameObject CreateSingleton<T>(string name) where T : Component
        {
            var existing = Object.FindFirstObjectByType<T>();
            if (existing != null) return existing.gameObject;

            var obj = new GameObject(name);
            obj.AddComponent<T>();
            return obj;
        }

        static GameObject FindOrCreateChild(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null) return existing.gameObject;

            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        static Text CreateUIText(Transform parent, string name, string defaultText,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var obj = FindOrCreateChild(parent, name);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            var text = obj.GetComponent<Text>();
            if (text == null)
                text = obj.AddComponent<Text>();
            text.text = defaultText;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return text;
        }
    }
}
