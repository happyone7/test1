using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Nodebreaker.Editor
{
    /// <summary>
    /// Sprint 4 Phase 2 UI 셋업 스크립트.
    /// UI-3 ~ UI-6 의 Canvas 오브젝트와 컴포넌트를 씬에 생성한다.
    /// Tools > Nodebreaker > Setup Sprint4 Phase2 UI 로 실행.
    /// </summary>
    public class Sprint4Phase2UISetup
    {
        [MenuItem("Tools/Nodebreaker/Setup Sprint4 Phase2 UI (UI-3~6)")]
        static void Setup()
        {
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[Sprint4Phase2UISetup] Canvas 없음. 먼저 씬에 Canvas를 추가하세요.");
                return;
            }

            SetupTreasureChoiceUI(canvas.transform);
            SetupBossHpBarUI(canvas.transform);
            SetupStageClearUI(canvas.transform);
            SetupTowerDragGhostUI(canvas.transform);

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("[Sprint4Phase2UISetup] Sprint 4 Phase 2 UI 셋업 완료! (UI-3 ~ UI-6)");
        }

        // ===== UI-3: 보물상자 3택 선택 UI =====
        [MenuItem("Tools/Nodebreaker/Setup UI-3 Treasure Choice")]
        static void SetupTreasureChoice()
        {
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) { Debug.LogError("Canvas 없음"); return; }
            SetupTreasureChoiceUI(canvas.transform);
            MarkDirty();
        }

        static void SetupTreasureChoiceUI(Transform canvasTransform)
        {
            var obj = FindOrCreateChild(canvasTransform, "TreasureChoiceUI");
            var rect = EnsureRectTransform(obj);
            StretchFull(rect);

            var treasureUI = EnsureComponent<UI.TreasureChoiceUI>(obj);

            // UISprites 연결
            var uiSprites = FindUISprites();
            if (uiSprites != null)
            {
                var so = new SerializedObject(treasureUI);
                var prop = so.FindProperty("uiSprites");
                if (prop != null)
                    prop.objectReferenceValue = uiSprites;
                so.ApplyModifiedProperties();
            }

            obj.SetActive(false);
            EditorUtility.SetDirty(treasureUI);
            Debug.Log("[Sprint4Phase2UISetup] UI-3 TreasureChoiceUI 셋업 완료");
        }

        // ===== UI-4: 보스 HP바 UI =====
        [MenuItem("Tools/Nodebreaker/Setup UI-4 Boss HP Bar")]
        static void SetupBossHpBar()
        {
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) { Debug.LogError("Canvas 없음"); return; }
            SetupBossHpBarUI(canvas.transform);
            MarkDirty();
        }

        static void SetupBossHpBarUI(Transform canvasTransform)
        {
            var obj = FindOrCreateChild(canvasTransform, "BossHpBarUI");
            var rect = EnsureRectTransform(obj);
            StretchFull(rect);

            var bossUI = EnsureComponent<UI.BossHpBarUI>(obj);

            // UISprites 연결
            var uiSprites = FindUISprites();
            if (uiSprites != null)
            {
                var so = new SerializedObject(bossUI);
                var prop = so.FindProperty("uiSprites");
                if (prop != null)
                    prop.objectReferenceValue = uiSprites;
                so.ApplyModifiedProperties();
            }

            obj.SetActive(false);
            EditorUtility.SetDirty(bossUI);
            Debug.Log("[Sprint4Phase2UISetup] UI-4 BossHpBarUI 셋업 완료");
        }

        // ===== UI-5: Stage 클리어 화면 =====
        [MenuItem("Tools/Nodebreaker/Setup UI-5 Stage Clear")]
        static void SetupStageClear()
        {
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) { Debug.LogError("Canvas 없음"); return; }
            SetupStageClearUI(canvas.transform);
            MarkDirty();
        }

        static void SetupStageClearUI(Transform canvasTransform)
        {
            var obj = FindOrCreateChild(canvasTransform, "StageClearUI");
            var rect = EnsureRectTransform(obj);
            StretchFull(rect);

            var clearUI = EnsureComponent<UI.StageClearUI>(obj);

            // UISprites 연결
            var uiSprites = FindUISprites();
            if (uiSprites != null)
            {
                var so = new SerializedObject(clearUI);
                var prop = so.FindProperty("uiSprites");
                if (prop != null)
                    prop.objectReferenceValue = uiSprites;
                so.ApplyModifiedProperties();
            }

            obj.SetActive(false);
            EditorUtility.SetDirty(clearUI);
            Debug.Log("[Sprint4Phase2UISetup] UI-5 StageClearUI 셋업 완료");
        }

        // ===== UI-6: 타워 드래그 고스트 UI =====
        [MenuItem("Tools/Nodebreaker/Setup UI-6 Tower Drag Ghost")]
        static void SetupDragGhost()
        {
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) { Debug.LogError("Canvas 없음"); return; }
            SetupTowerDragGhostUI(canvas.transform);
            MarkDirty();
        }

        static void SetupTowerDragGhostUI(Transform canvasTransform)
        {
            var obj = FindOrCreateChild(canvasTransform, "TowerDragGhostUI");
            var rect = EnsureRectTransform(obj);
            StretchFull(rect);

            var ghostUI = EnsureComponent<UI.TowerDragGhostUI>(obj);

            // TowerDragController 자동 연결
            var dragController = Object.FindFirstObjectByType<Tower.TowerDragController>();
            if (dragController != null)
            {
                var so = new SerializedObject(ghostUI);
                var dragProp = so.FindProperty("dragController");
                if (dragProp != null)
                    dragProp.objectReferenceValue = dragController;

                var canvasProp = so.FindProperty("parentCanvas");
                if (canvasProp != null)
                    canvasProp.objectReferenceValue = canvasTransform.GetComponent<Canvas>();

                so.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(ghostUI);
            Debug.Log("[Sprint4Phase2UISetup] UI-6 TowerDragGhostUI 셋업 완료");
        }

        // === 유틸리티 ===

        static UI.UISprites FindUISprites()
        {
            // 프로젝트에서 UISprites 에셋 찾기
            var guids = AssetDatabase.FindAssets("t:UISprites");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<UI.UISprites>(path);
                if (asset != null) return asset;
            }

            // InGameUI에 이미 연결된 것 찾기
            var inGameUI = Object.FindFirstObjectByType<UI.InGameUI>(FindObjectsInactive.Include);
            if (inGameUI != null && inGameUI.uiSprites != null)
                return inGameUI.uiSprites;

            return null;
        }

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
            if (rt == null) rt = obj.AddComponent<RectTransform>();
            return rt;
        }

        static T EnsureComponent<T>(GameObject obj) where T : Component
        {
            var comp = obj.GetComponent<T>();
            if (comp == null) comp = obj.AddComponent<T>();
            return comp;
        }

        static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static void MarkDirty()
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }
}
