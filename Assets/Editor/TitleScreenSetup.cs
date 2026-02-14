using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Nodebreaker.Editor
{
    public class TitleScreenSetup
    {
        // 색상 팔레트
        static readonly Color MainBg = HexColor("#0B0F1A");
        static readonly Color PanelColor = HexColor("#121A2A");
        static readonly Color NeonGreen = HexColor("#2BFF88");
        static readonly Color NeonBlue = HexColor("#37B6FF");
        static readonly Color DangerRed = HexColor("#FF4D5A");
        static readonly Color SubText = HexColor("#AFC3E8");

        [MenuItem("Tools/Nodebreaker/Setup TitleScreen UI")]
        static void SetupTitleScreen()
        {
            // 기존 TitleScreenCanvas가 있으면 삭제
            var existing = GameObject.Find("TitleScreenCanvas");
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing);
            }

            // === 1. TitleScreen 전용 Canvas 생성 ===
            var canvasObj = new GameObject("TitleScreenCanvas");
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create TitleScreenCanvas");

            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // 다른 Canvas 위에 표시

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // === 2. TitleScreenPanel (전체 화면 배경) ===
            var panelObj = CreateUIElement("TitleScreenPanel", canvasObj.transform);
            var panelRect = panelObj.GetComponent<RectTransform>();
            StretchFull(panelRect);

            var panelImage = panelObj.AddComponent<Image>();
            panelImage.color = MainBg;

            // TitleScreenUI 컴포넌트 추가
            var titleScreenUI = panelObj.AddComponent<UI.TitleScreenUI>();

            // === 3. 게임 제목: "NODEBREAKER TD" ===
            // PPT: pos (460, 200), size 1000x100, 48pt 굵게, #2BFF88, Anchor=Top-Center
            var titleObj = CreateUIElement("TitleText", panelObj.transform);
            var titleRect = titleObj.GetComponent<RectTransform>();
            SetAnchorTopCenter(titleRect);
            titleRect.sizeDelta = new Vector2(1000, 100);
            // PPT Y=200 from top => anchoredPosition.y = -(200 + 50) = -250 (center of element)
            titleRect.anchoredPosition = new Vector2(0, -250);

            var titleText = titleObj.AddComponent<Text>();
            titleText.text = "NODEBREAKER TD";
            titleText.fontSize = 48;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = NeonGreen;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // === 4. 부제목: "네트워크를 지켜라. Node를 격파하라." ===
            // PPT: pos (460, 320), size 1000x40, 16pt, #AFC3E8, Anchor=Top-Center
            var subtitleObj = CreateUIElement("SubtitleText", panelObj.transform);
            var subtitleRect = subtitleObj.GetComponent<RectTransform>();
            SetAnchorTopCenter(subtitleRect);
            subtitleRect.sizeDelta = new Vector2(1000, 40);
            // PPT Y=320 from top => anchoredPosition.y = -(320 + 20) = -340
            subtitleRect.anchoredPosition = new Vector2(0, -340);

            var subtitleText = subtitleObj.AddComponent<Text>();
            subtitleText.text = "네트워크를 지켜라. Node를 격파하라.";
            subtitleText.fontSize = 16;
            subtitleText.color = SubText;
            subtitleText.alignment = TextAnchor.MiddleCenter;
            subtitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // === 5. 버튼들 (세로 배치, 가로 중앙) ===

            // 시작 버튼: PPT (760, 480), 400x60, 테두리 #2BFF88, 채우기 #121A2A, 20pt 굵게
            var startBtn = CreateButton("StartButton", panelObj.transform,
                "시작", 20, FontStyle.Bold,
                PanelColor, NeonGreen,
                new Vector2(400, 60),
                new Vector2(0, -510)); // -(480+30)

            // 설정 버튼: PPT (760, 560), 400x60, 테두리 #37B6FF, 채우기 #121A2A, 20pt 굵게
            var settingsBtn = CreateButton("SettingsButton", panelObj.transform,
                "설정", 20, FontStyle.Bold,
                PanelColor, NeonBlue,
                new Vector2(400, 60),
                new Vector2(0, -590)); // -(560+30)

            // 종료 버튼: PPT (760, 640), 400x60, 테두리 #FF4D5A, 채우기 #121A2A, 20pt 굵게
            var quitBtn = CreateButton("QuitButton", panelObj.transform,
                "종료", 20, FontStyle.Bold,
                PanelColor, DangerRed,
                new Vector2(400, 60),
                new Vector2(0, -670)); // -(640+30)

            // === 6. TitleScreenUI에 SerializeField 연결 ===
            var so = new SerializedObject(titleScreenUI);
            so.FindProperty("startButton").objectReferenceValue = startBtn.GetComponent<Button>();
            so.FindProperty("settingsButton").objectReferenceValue = settingsBtn.GetComponent<Button>();
            so.FindProperty("quitButton").objectReferenceValue = quitBtn.GetComponent<Button>();
            so.ApplyModifiedProperties();

            // Dirty 마킹
            EditorUtility.SetDirty(canvasObj);

            Debug.Log("[TitleScreenSetup] 타이틀 화면 UI가 성공적으로 생성되었습니다.");

            // 씬 자동 저장
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        }

        /// <summary>
        /// 테두리가 있는 버튼 생성. Outline 컴포넌트로 테두리 효과 구현.
        /// </summary>
        static GameObject CreateButton(string name, Transform parent,
            string label, int fontSize, FontStyle fontStyle,
            Color fillColor, Color borderColor,
            Vector2 size, Vector2 anchoredPos)
        {
            var btnObj = CreateUIElement(name, parent);
            var btnRect = btnObj.GetComponent<RectTransform>();
            SetAnchorTopCenter(btnRect);
            btnRect.sizeDelta = size;
            btnRect.anchoredPosition = anchoredPos;

            // 버튼 배경 이미지
            var btnImage = btnObj.AddComponent<Image>();
            btnImage.color = fillColor;

            // Outline으로 테두리 효과
            var outline = btnObj.AddComponent<Outline>();
            outline.effectColor = borderColor;
            outline.effectDistance = new Vector2(2, -2);

            // Button 컴포넌트
            var button = btnObj.AddComponent<Button>();
            // 트랜지션 None (커스텀 인터랙션은 TitleScreenUI에서 처리)
            button.transition = Selectable.Transition.None;

            // 버튼 텍스트
            var textObj = CreateUIElement("Text", btnObj.transform);
            var textRect = textObj.GetComponent<RectTransform>();
            StretchFull(textRect);

            var text = textObj.AddComponent<Text>();
            text.text = label;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            return btnObj;
        }

        static GameObject CreateUIElement(string name, Transform parent)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static void SetAnchorTopCenter(RectTransform rect)
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        static Color HexColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
                return color;
            return Color.white;
        }
    }
}
