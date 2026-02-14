using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Nodebreaker.Editor
{
    public class PlayableSceneSetup
    {
        [MenuItem("Tools/Nodebreaker/Make Scene Playable")]
        static void MakePlayable()
        {
            try
            {
                PlaceTowers();
                SetupUI();
                EditorUtility.SetDirty(Object.FindFirstObjectByType<Canvas>());
                Debug.Log("[Nodebreaker] 씬 플레이 가능 상태 설정 완료!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Nodebreaker] 설정 실패: {e.Message}\n{e.StackTrace}");
            }
        }

        static void PlaceTowers()
        {
            // 타워 데이터 로드
            var towerData = AssetDatabase.LoadAssetAtPath<Data.TowerData>("Assets/Data/Towers/Tower_Arrow.asset");
            var towerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Project/Prefabs/Towers/Tower_Arrow.prefab");
            if (towerData == null || towerPrefab == null)
            {
                Debug.LogWarning("[Nodebreaker] Tower 에셋이 없습니다. Create Prototype Assets를 먼저 실행하세요.");
                return;
            }

            // 경로 근처에 타워 3개 배치
            Vector3[] towerPositions = {
                new Vector3(-2, 1, 0),   // 경로 중간 위쪽
                new Vector3(2, -1, 0),   // 경로 중간 아래쪽
                new Vector3(5, 2, 0),    // 경로 후반
            };

            var towerParent = GetOrCreateParent("Towers");

            for (int i = 0; i < towerPositions.Length; i++)
            {
                var name = $"ArrowTower_{i}";
                var existing = GameObject.Find(name);
                if (existing != null) continue;

                var go = (GameObject)PrefabUtility.InstantiatePrefab(towerPrefab);
                go.name = name;
                go.transform.position = towerPositions[i];
                go.transform.SetParent(towerParent.transform);

                var tower = go.GetComponent<Tower.Tower>();
                if (tower != null)
                    tower.Initialize(towerData, 1);

                // Tower의 data 필드 직접 설정 (Initialize는 런타임용)
                var so = new SerializedObject(tower);
                var dataProp = so.FindProperty("data");
                if (dataProp != null)
                {
                    dataProp.objectReferenceValue = towerData;
                    so.ApplyModifiedProperties();
                }

                EditorUtility.SetDirty(go);
                Debug.Log($"[Nodebreaker] 타워 배치: {name} at {towerPositions[i]}");
            }
        }

        static void SetupUI()
        {
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[Nodebreaker] Canvas가 없습니다. Setup Game Scene을 먼저 실행하세요.");
                return;
            }

            var canvasObj = canvas.gameObject;

            // CanvasScaler 설정
            var scaler = canvasObj.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
            }

            // 상단 HUD 패널
            var topPanel = CreateUIElement<RectTransform>("TopPanel", canvasObj.transform);
            SetAnchors(topPanel, new Vector2(0, 1), new Vector2(1, 1));
            topPanel.pivot = new Vector2(0.5f, 1);
            topPanel.anchoredPosition = Vector2.zero;
            topPanel.sizeDelta = new Vector2(0, 60);
            var topBg = topPanel.gameObject.AddComponent<Image>();
            topBg.color = new Color(0, 0, 0, 0.5f);

            // Wave 텍스트
            var waveText = CreateText("WaveText", topPanel, "Wave 1/1",
                new Vector2(20, -10), new Vector2(200, 40), TextAnchor.MiddleLeft);

            // Bit 텍스트
            var bitText = CreateText("BitText", topPanel, "Bit: 0",
                new Vector2(250, -10), new Vector2(200, 40), TextAnchor.MiddleLeft);

            // HP 텍스트
            var hpText = CreateText("HPText", topPanel, "HP: 10/10",
                new Vector2(480, -10), new Vector2(200, 40), TextAnchor.MiddleLeft);

            // HP 슬라이더
            var hpSlider = CreateSlider("HPSlider", topPanel,
                new Vector2(700, -15), new Vector2(300, 30));

            // 런 종료 패널
            var runEndPanel = CreateUIElement<RectTransform>("RunEndPanel", canvasObj.transform);
            SetAnchors(runEndPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            runEndPanel.sizeDelta = new Vector2(400, 300);
            var endBg = runEndPanel.gameObject.AddComponent<Image>();
            endBg.color = new Color(0, 0, 0, 0.8f);

            var titleText = CreateText("TitleText", runEndPanel, "DEFEATED",
                new Vector2(0, 80), new Vector2(300, 50), TextAnchor.MiddleCenter);
            titleText.fontSize = 36;

            var endBitText = CreateText("BitEarnedText", runEndPanel, "+0 Bit",
                new Vector2(0, 20), new Vector2(300, 40), TextAnchor.MiddleCenter);

            var retryBtn = CreateButton("RetryButton", runEndPanel, "재시도",
                new Vector2(-80, -60), new Vector2(120, 40));

            var hubBtn = CreateButton("HubButton", runEndPanel, "Hub",
                new Vector2(80, -60), new Vector2(120, 40));

            runEndPanel.gameObject.SetActive(false);

            // InGameUI 컴포넌트 연결
            var inGameUI = canvasObj.GetComponent<UI.InGameUI>();
            if (inGameUI == null)
                inGameUI = canvasObj.AddComponent<UI.InGameUI>();

            var so = new SerializedObject(inGameUI);
            SetSerializedField(so, "waveText", waveText);
            SetSerializedField(so, "bitText", bitText);
            SetSerializedField(so, "baseHpText", hpText);
            SetSerializedField(so, "baseHpSlider", hpSlider);
            SetSerializedField(so, "runEndPanel", runEndPanel.gameObject);
            SetSerializedField(so, "runEndTitleText", titleText);
            SetSerializedField(so, "runEndBitText", endBitText);
            SetSerializedField(so, "hubButton", hubBtn);
            SetSerializedField(so, "retryButton", retryBtn);
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(inGameUI);
            Debug.Log("[Nodebreaker] InGame UI 설정 완료");
        }

        static T CreateUIElement<T>(string name, Transform parent) where T : Component
        {
            var existing = parent.Find(name);
            if (existing != null) return existing.GetComponent<T>();

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var comp = go.AddComponent<T>();
            return comp;
        }

        static Text CreateText(string name, RectTransform parent, string text,
            Vector2 anchoredPos, Vector2 size, TextAnchor alignment)
        {
            var existing = parent.Find(name);
            if (existing != null) return existing.GetComponent<Text>();

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            SetAnchors(rt, new Vector2(0, 1), new Vector2(0, 1));
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var t = go.AddComponent<Text>();
            t.text = text;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 20;
            t.color = Color.white;
            t.alignment = alignment;
            return t;
        }

        static Slider CreateSlider(string name, RectTransform parent,
            Vector2 anchoredPos, Vector2 size)
        {
            var existing = parent.Find(name);
            if (existing != null) return existing.GetComponent<Slider>();

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            SetAnchors(rt, new Vector2(0, 1), new Vector2(0, 1));
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            // Background
            var bg = new GameObject("Background");
            bg.transform.SetParent(go.transform, false);
            var bgRt = bg.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.3f, 0, 0);

            // Fill Area
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            var faRt = fillArea.AddComponent<RectTransform>();
            faRt.anchorMin = Vector2.zero; faRt.anchorMax = Vector2.one;
            faRt.sizeDelta = Vector2.zero;

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillRt = fill.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.8f, 0.1f, 0.1f);

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillRt;
            slider.interactable = false;
            slider.maxValue = 10;
            slider.value = 10;

            return slider;
        }

        static Button CreateButton(string name, RectTransform parent,
            string label, Vector2 anchoredPos, Vector2 size)
        {
            var existing = parent.Find(name);
            if (existing != null) return existing.GetComponent<Button>();

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.6f, 0.2f);

            var btn = go.AddComponent<Button>();

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(go.transform, false);
            var textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero; textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            var t = textObj.AddComponent<Text>();
            t.text = label;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 18;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;

            return btn;
        }

        static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
        }

        static void SetSerializedField(SerializedObject so, string fieldName, Object value)
        {
            var prop = so.FindProperty(fieldName);
            if (prop != null)
                prop.objectReferenceValue = value;
        }

        static GameObject GetOrCreateParent(string name)
        {
            var existing = GameObject.Find(name);
            if (existing != null) return existing;
            return new GameObject(name);
        }
    }
}
