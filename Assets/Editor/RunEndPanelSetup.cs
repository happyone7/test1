using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Nodebreaker.Editor
{
    /// <summary>
    /// Sprint 4 UI-1: RunEnd 패널을 PPT 명세에 맞게 재구성.
    /// 기존 RunEndPanel의 자식을 정리하고 새 구조로 재빌드합니다.
    /// Tools > Nodebreaker > Setup RunEnd Panel (PPT Spec) 으로 실행.
    /// </summary>
    public class RunEndPanelSetup
    {
        // === PPT 명세 색상 팔레트 ===
        static readonly Color ColorPanel = new Color32(0x12, 0x1A, 0x2A, 0xFF);
        static readonly Color ColorBrightPanel = new Color32(0x1A, 0x24, 0x3A, 0xFF);
        static readonly Color ColorNeonGreen = new Color32(0x2B, 0xFF, 0x88, 0xFF);
        static readonly Color ColorNeonBlue = new Color32(0x37, 0xB6, 0xFF, 0xFF);
        static readonly Color ColorTextMain = new Color32(0xD8, 0xE4, 0xFF, 0xFF);
        static readonly Color ColorTextSub = new Color32(0xAF, 0xC3, 0xE8, 0xFF);
        static readonly Color ColorBorder = new Color32(0x5B, 0x6B, 0x8A, 0xFF);
        static readonly Color ColorOverlay = new Color32(0x05, 0x08, 0x12, 0xCC);

        [MenuItem("Tools/Nodebreaker/Setup RunEnd Panel (PPT Spec)")]
        static void SetupRunEndPanel()
        {
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[RunEndPanelSetup] Canvas 없음");
                return;
            }

            var inGameUI = canvas.GetComponent<UI.InGameUI>();
            if (inGameUI == null)
            {
                Debug.LogError("[RunEndPanelSetup] InGameUI 컴포넌트 없음");
                return;
            }

            var so = new SerializedObject(inGameUI);

            // ===== 1. 오버레이 (전체 화면 반투명) =====
            var overlayObj = FindOrCreateChild(canvas.transform, "RunEndOverlay");
            var overlayRect = EnsureRectTransform(overlayObj);
            StretchFull(overlayRect);
            var overlayImage = EnsureComponent<Image>(overlayObj);
            overlayImage.color = ColorOverlay;
            overlayImage.raycastTarget = true;
            overlayObj.SetActive(false);
            SetField(so, "runEndOverlay", overlayObj);

            // ===== 2. RunEndPanel (메인 패널 컨테이너) =====
            var panelObj = FindOrCreateChild(canvas.transform, "RunEndPanel");
            var panelRect = EnsureRectTransform(panelObj);
            // 기존 자식 정리
            ClearChildren(panelObj.transform);
            // 화면 중앙 고정, 800x700
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(800, 700);
            panelRect.anchoredPosition = Vector2.zero;
            panelObj.SetActive(false);

            // RunEndPanel의 Image 제거 (PanelBody가 대신)
            var oldPanelImage = panelObj.GetComponent<Image>();
            if (oldPanelImage != null)
                Object.DestroyImmediate(oldPanelImage);
            var oldCanvasRenderer = panelObj.GetComponent<CanvasRenderer>();
            if (oldCanvasRenderer != null)
                Object.DestroyImmediate(oldCanvasRenderer);

            SetField(so, "runEndPanel", panelObj);

            // ===== 3. PanelBody (배경 이미지 + Outline) =====
            var bodyObj = FindOrCreateChild(panelObj.transform, "PanelBody");
            var bodyRect = EnsureRectTransform(bodyObj);
            StretchFull(bodyRect);
            var bodyImage = EnsureComponent<Image>(bodyObj);
            bodyImage.color = ColorPanel;
            bodyImage.raycastTarget = true;
            var bodyOutline = EnsureComponent<Outline>(bodyObj);
            bodyOutline.effectColor = ColorBorder;
            bodyOutline.effectDistance = new Vector2(2, 2);
            SetField(so, "runEndPanelBody", bodyImage);
            SetField(so, "runEndPanelOutline", bodyOutline);

            // ===== 4. 제목 텍스트 (상단) =====
            var titleObj = FindOrCreateChild(panelObj.transform, "TitleText");
            var titleRect = EnsureRectTransform(titleObj);
            titleRect.anchorMin = new Vector2(0, 0.82f);
            titleRect.anchorMax = new Vector2(1, 0.95f);
            titleRect.offsetMin = new Vector2(20, 0);
            titleRect.offsetMax = new Vector2(-20, 0);
            var titleText = EnsureComponent<Text>(titleObj);
            EnsureComponent<CanvasRenderer>(titleObj);
            titleText.text = "\ud328\ubc30";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 36;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = ColorTextMain;
            SetField(so, "runEndTitleText", titleText);

            // ===== 5. 스테이지 이름 텍스트 (제목 아래) =====
            var stageNameObj = FindOrCreateChild(panelObj.transform, "StageNameText");
            var stageNameRect = EnsureRectTransform(stageNameObj);
            stageNameRect.anchorMin = new Vector2(0, 0.74f);
            stageNameRect.anchorMax = new Vector2(1, 0.82f);
            stageNameRect.offsetMin = new Vector2(20, 0);
            stageNameRect.offsetMax = new Vector2(-20, 0);
            var stageNameText = EnsureComponent<Text>(stageNameObj);
            EnsureComponent<CanvasRenderer>(stageNameObj);
            stageNameText.text = "";
            stageNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            stageNameText.fontSize = 14;
            stageNameText.alignment = TextAnchor.MiddleCenter;
            stageNameText.color = ColorTextSub;
            stageNameObj.SetActive(false);
            SetField(so, "runEndStageNameText", stageNameText);

            // ===== 6. 구분선 =====
            var dividerObj = FindOrCreateChild(panelObj.transform, "Divider");
            var dividerRect = EnsureRectTransform(dividerObj);
            dividerRect.anchorMin = new Vector2(0.1f, 0.72f);
            dividerRect.anchorMax = new Vector2(0.9f, 0.72f);
            dividerRect.sizeDelta = new Vector2(0, 2);
            var dividerImage = EnsureComponent<Image>(dividerObj);
            EnsureComponent<CanvasRenderer>(dividerObj);
            dividerImage.color = ColorBorder;

            // ===== 7. 정보 영역 (웨이브/처치/Bit/Core) =====
            // 웨이브 도달
            var waveObj = FindOrCreateChild(panelObj.transform, "WaveText");
            var waveRect = EnsureRectTransform(waveObj);
            waveRect.anchorMin = new Vector2(0.1f, 0.58f);
            waveRect.anchorMax = new Vector2(0.9f, 0.68f);
            waveRect.offsetMin = Vector2.zero;
            waveRect.offsetMax = Vector2.zero;
            var waveText = EnsureComponent<Text>(waveObj);
            EnsureComponent<CanvasRenderer>(waveObj);
            waveText.text = "\ub3c4\ub2ec \uc6e8\uc774\ube0c:  0/0";
            waveText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            waveText.fontSize = 22;
            waveText.alignment = TextAnchor.MiddleCenter;
            waveText.color = ColorTextMain;
            SetField(so, "runEndWaveText", waveText);

            // 처치 Node
            var nodesObj = FindOrCreateChild(panelObj.transform, "NodesText");
            var nodesRect = EnsureRectTransform(nodesObj);
            nodesRect.anchorMin = new Vector2(0.1f, 0.47f);
            nodesRect.anchorMax = new Vector2(0.9f, 0.57f);
            nodesRect.offsetMin = Vector2.zero;
            nodesRect.offsetMax = Vector2.zero;
            var nodesText = EnsureComponent<Text>(nodesObj);
            EnsureComponent<CanvasRenderer>(nodesObj);
            nodesText.text = "\ucc98\uce58 Node:  0";
            nodesText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nodesText.fontSize = 22;
            nodesText.alignment = TextAnchor.MiddleCenter;
            nodesText.color = ColorTextMain;
            SetField(so, "runEndNodesText", nodesText);

            // 획득 Bit
            var bitObj = FindOrCreateChild(panelObj.transform, "BitText");
            var bitRect = EnsureRectTransform(bitObj);
            bitRect.anchorMin = new Vector2(0.1f, 0.36f);
            bitRect.anchorMax = new Vector2(0.9f, 0.46f);
            bitRect.offsetMin = Vector2.zero;
            bitRect.offsetMax = Vector2.zero;
            var bitText = EnsureComponent<Text>(bitObj);
            EnsureComponent<CanvasRenderer>(bitObj);
            bitText.text = "\ud68d\ub4dd Bit:  +0";
            bitText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            bitText.fontSize = 22;
            bitText.alignment = TextAnchor.MiddleCenter;
            bitText.color = ColorTextMain;
            SetField(so, "runEndBitText", bitText);

            // 획득 Core
            var coreObj = FindOrCreateChild(panelObj.transform, "CoreText");
            var coreRect = EnsureRectTransform(coreObj);
            coreRect.anchorMin = new Vector2(0.1f, 0.25f);
            coreRect.anchorMax = new Vector2(0.9f, 0.35f);
            coreRect.offsetMin = Vector2.zero;
            coreRect.offsetMax = Vector2.zero;
            var coreText = EnsureComponent<Text>(coreObj);
            EnsureComponent<CanvasRenderer>(coreObj);
            coreText.text = "\ud68d\ub4dd Core:  0";
            coreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            coreText.fontSize = 22;
            coreText.alignment = TextAnchor.MiddleCenter;
            coreText.color = ColorTextMain;
            coreObj.SetActive(false);
            SetField(so, "runEndCoreText", coreText);

            // ===== 8. 해금 알림 (하단 정보 영역) =====
            var unlockObj = FindOrCreateChild(panelObj.transform, "UnlockNotice");
            var unlockRect = EnsureRectTransform(unlockObj);
            unlockRect.anchorMin = new Vector2(0.1f, 0.17f);
            unlockRect.anchorMax = new Vector2(0.9f, 0.25f);
            unlockRect.offsetMin = Vector2.zero;
            unlockRect.offsetMax = Vector2.zero;
            var unlockText = EnsureComponent<Text>(unlockObj);
            EnsureComponent<CanvasRenderer>(unlockObj);
            unlockText.text = "\uc0c8 \uc2a4\ud14c\uc774\uc9c0 \ud574\uae08!";
            unlockText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            unlockText.fontSize = 18;
            unlockText.alignment = TextAnchor.MiddleCenter;
            unlockText.color = ColorNeonGreen;
            unlockObj.SetActive(false);
            SetField(so, "runEndUnlockNotice", unlockObj);
            SetField(so, "runEndUnlockText", unlockText);

            // ===== 9. 하단 버튼 영역 =====
            var buttonArea = FindOrCreateChild(panelObj.transform, "ButtonArea");
            var btnAreaRect = EnsureRectTransform(buttonArea);
            btnAreaRect.anchorMin = new Vector2(0.05f, 0.03f);
            btnAreaRect.anchorMax = new Vector2(0.95f, 0.15f);
            btnAreaRect.offsetMin = Vector2.zero;
            btnAreaRect.offsetMax = Vector2.zero;

            // Hub 버튼 (왼쪽)
            var hubBtnObj = FindOrCreateChild(buttonArea.transform, "HubButton");
            var hubBtnRect = EnsureRectTransform(hubBtnObj);
            hubBtnRect.anchorMin = new Vector2(0.05f, 0.1f);
            hubBtnRect.anchorMax = new Vector2(0.45f, 0.9f);
            hubBtnRect.offsetMin = Vector2.zero;
            hubBtnRect.offsetMax = Vector2.zero;
            var hubBtnImage = EnsureComponent<Image>(hubBtnObj);
            EnsureComponent<CanvasRenderer>(hubBtnObj);
            hubBtnImage.color = ColorBrightPanel;
            var hubBtn = EnsureComponent<Button>(hubBtnObj);
            var hubBtnOutline = EnsureComponent<Outline>(hubBtnObj);
            hubBtnOutline.effectColor = ColorNeonBlue;
            hubBtnOutline.effectDistance = new Vector2(2, 2);
            SetField(so, "hubButton", hubBtn);
            SetField(so, "hubButtonOutline", hubBtnOutline);

            // Hub 버튼 텍스트
            var hubTextObj = FindOrCreateChild(hubBtnObj.transform, "Text");
            var hubTextRect = EnsureRectTransform(hubTextObj);
            StretchFull(hubTextRect);
            var hubBtnText = EnsureComponent<Text>(hubTextObj);
            EnsureComponent<CanvasRenderer>(hubTextObj);
            hubBtnText.text = "[ Hub ]";
            hubBtnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hubBtnText.fontSize = 22;
            hubBtnText.fontStyle = FontStyle.Bold;
            hubBtnText.alignment = TextAnchor.MiddleCenter;
            hubBtnText.color = ColorTextMain;
            SetField(so, "hubButtonText", hubBtnText);

            // Retry 버튼 (오른쪽)
            var retryBtnObj = FindOrCreateChild(buttonArea.transform, "RetryButton");
            var retryBtnRect = EnsureRectTransform(retryBtnObj);
            retryBtnRect.anchorMin = new Vector2(0.55f, 0.1f);
            retryBtnRect.anchorMax = new Vector2(0.95f, 0.9f);
            retryBtnRect.offsetMin = Vector2.zero;
            retryBtnRect.offsetMax = Vector2.zero;
            var retryBtnImage = EnsureComponent<Image>(retryBtnObj);
            EnsureComponent<CanvasRenderer>(retryBtnObj);
            retryBtnImage.color = ColorBrightPanel;
            var retryBtn = EnsureComponent<Button>(retryBtnObj);
            var retryBtnOutline = EnsureComponent<Outline>(retryBtnObj);
            retryBtnOutline.effectColor = ColorNeonGreen;
            retryBtnOutline.effectDistance = new Vector2(2, 2);
            SetField(so, "retryButton", retryBtn);
            SetField(so, "retryButtonOutline", retryBtnOutline);

            // Retry 버튼 텍스트
            var retryTextObj = FindOrCreateChild(retryBtnObj.transform, "Text");
            var retryTextRect = EnsureRectTransform(retryTextObj);
            StretchFull(retryTextRect);
            var retryBtnText = EnsureComponent<Text>(retryTextObj);
            EnsureComponent<CanvasRenderer>(retryTextObj);
            retryBtnText.text = "[ \uc989\uc2dc \uc7ac\ub3c4\uc804 ]";
            retryBtnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            retryBtnText.fontSize = 22;
            retryBtnText.fontStyle = FontStyle.Bold;
            retryBtnText.alignment = TextAnchor.MiddleCenter;
            retryBtnText.color = ColorTextMain;
            SetField(so, "retryButtonText", retryBtnText);

            // ===== 직렬화 적용 =====
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(inGameUI);
            EditorUtility.SetDirty(canvas);

            // RunEndOverlay를 RunEndPanel보다 앞(낮은 sibling)에 배치
            overlayObj.transform.SetSiblingIndex(panelObj.transform.GetSiblingIndex());

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("[RunEndPanelSetup] RunEnd 패널 PPT 명세 재구성 완료! 씬 저장 필요.");
        }

        // === 유틸리티 ===

        static GameObject FindOrCreateChild(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null) return existing.gameObject;

            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        static RectTransform EnsureRectTransform(GameObject obj)
        {
            var rt = obj.GetComponent<RectTransform>();
            if (rt == null)
                rt = obj.AddComponent<RectTransform>();
            return rt;
        }

        static T EnsureComponent<T>(GameObject obj) where T : Component
        {
            var comp = obj.GetComponent<T>();
            if (comp == null)
                comp = obj.AddComponent<T>();
            return comp;
        }

        static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static void SetField(SerializedObject so, string fieldName, Object value)
        {
            var prop = so.FindProperty(fieldName);
            if (prop != null)
                prop.objectReferenceValue = value;
            else
                Debug.LogWarning($"[RunEndPanelSetup] '{fieldName}' 필드를 찾을 수 없음");
        }

        static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(parent.GetChild(i).gameObject);
            }
        }
    }
}
