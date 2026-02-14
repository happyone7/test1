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

            // === 2-1. 배경 이미지 (전체 화면, TitleBG_01) ===
            var bgObj = CreateUIElement("BackgroundImage", panelObj.transform);
            var bgRect = bgObj.GetComponent<RectTransform>();
            StretchFull(bgRect);
            var bgImage = bgObj.AddComponent<Image>();
            bgImage.color = Color.white;
            bgImage.preserveAspect = false;
            bgImage.raycastTarget = false;
            var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Backgrounds/TitleBG_01.png");
            if (bgSprite != null)
            {
                bgImage.sprite = bgSprite;
                Debug.Log("[TitleScreenSetup] 타이틀 배경 이미지 적용: TitleBG_01.png");
            }
            else
            {
                Debug.LogWarning("[TitleScreenSetup] TitleBG_01.png 스프라이트를 찾을 수 없습니다. 텍스처 임포트 설정을 확인하세요.");
            }

            // === 2-2. 로고 이미지 (상단 중앙, SoulspireLogo_02) ===
            var logoObj = CreateUIElement("LogoImage", panelObj.transform);
            var logoRect = logoObj.GetComponent<RectTransform>();
            SetAnchorTopCenter(logoRect);
            logoRect.sizeDelta = new Vector2(600, 300);
            logoRect.anchoredPosition = new Vector2(0, -200);
            var logoImage = logoObj.AddComponent<Image>();
            logoImage.color = Color.white;
            logoImage.preserveAspect = true;
            logoImage.raycastTarget = false;
            var logoSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Logo/SoulspireLogo_02.png");
            if (logoSprite != null)
            {
                logoImage.sprite = logoSprite;
                Debug.Log("[TitleScreenSetup] 타이틀 로고 이미지 적용: SoulspireLogo_02.png");
            }
            else
            {
                Debug.LogWarning("[TitleScreenSetup] SoulspireLogo_02.png 스프라이트를 찾을 수 없습니다. 텍스처 임포트 설정을 확인하세요.");
            }

            // === 3. 부제목: "영혼의 첨탑을 지켜라" ===
            // PPT: pos (460, 320), size 1000x40, 16pt, #AFC3E8, Anchor=Top-Center
            var subtitleObj = CreateUIElement("SubtitleText", panelObj.transform);
            var subtitleRect = subtitleObj.GetComponent<RectTransform>();
            SetAnchorTopCenter(subtitleRect);
            subtitleRect.sizeDelta = new Vector2(1000, 40);
            subtitleRect.anchoredPosition = new Vector2(0, -380);

            var subtitleText = subtitleObj.AddComponent<Text>();
            subtitleText.text = "영혼의 첨탑을 지켜라";
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
            so.FindProperty("backgroundImage").objectReferenceValue = bgImage;
            so.FindProperty("logoImage").objectReferenceValue = logoImage;
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
