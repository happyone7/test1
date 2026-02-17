using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Soulspire.Editor
{
    /// <summary>
    /// Sprint 6 자동 셋업 스크립트.
    /// 메뉴: Soulspire > Sprint 6 Setup
    /// 1) MetaManager.allSkills 자동 할당 (12개 SkillNodeData SO)
    /// 2) FeedbackManager 씬 오브젝트 추가
    /// 3) SkillNodeUI 프리팹 생성 + HubUI 연결
    /// </summary>
    public static class Sprint6Setup
    {
        private const string SkillsFolder = "Assets/Project/ScriptableObjects/Skills";
        private const string PrefabPath = "Assets/Project/Prefabs/UI/SkillNodeUI.prefab";

        [MenuItem("Soulspire/Sprint 6 Setup")]
        static void Run()
        {
            int successCount = 0;

            if (SetupMetaManagerSkills()) successCount++;
            if (SetupFeedbackManager()) successCount++;
            if (SetupSkillNodeUIPrefab()) successCount++;

            EditorUtility.DisplayDialog(
                "Sprint 6 Setup",
                $"셋업 완료: {successCount}/3 단계 성공.\n자세한 내용은 Console 로그를 확인하세요.",
                "확인"
            );
        }

        // ═══════════════════════════════════════════════════════════
        // 1. MetaManager.allSkills 자동 할당
        // ═══════════════════════════════════════════════════════════

        static bool SetupMetaManagerSkills()
        {
            var meta = Object.FindFirstObjectByType<Core.MetaManager>();
            if (meta == null)
            {
                Debug.LogError("[Sprint6Setup] MetaManager를 씬에서 찾을 수 없습니다.");
                return false;
            }

            // Skills 폴더에서 모든 SkillNodeData SO 로드
            string[] guids = AssetDatabase.FindAssets("t:SkillNodeData", new[] { SkillsFolder });
            if (guids.Length == 0)
            {
                Debug.LogError($"[Sprint6Setup] {SkillsFolder} 에서 SkillNodeData를 찾을 수 없습니다.");
                return false;
            }

            var skills = new Data.SkillNodeData[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                skills[i] = AssetDatabase.LoadAssetAtPath<Data.SkillNodeData>(path);
            }

            meta.allSkills = skills;

            EditorUtility.SetDirty(meta);
            MarkActiveSceneDirty();

            Debug.Log($"[Sprint6Setup] MetaManager.allSkills 할당 완료: {skills.Length}개 SkillNodeData");
            return true;
        }

        // ═══════════════════════════════════════════════════════════
        // 2. FeedbackManager 씬 추가
        // ═══════════════════════════════════════════════════════════

        static bool SetupFeedbackManager()
        {
            var existing = Object.FindFirstObjectByType<Core.FeedbackManager>();
            if (existing != null)
            {
                Debug.Log("[Sprint6Setup] FeedbackManager가 이미 씬에 존재합니다. 건너뜁니다.");
                return true;
            }

            var go = new GameObject("[FeedbackManager]");
            go.AddComponent<Core.FeedbackManager>();

            EditorUtility.SetDirty(go);
            MarkActiveSceneDirty();

            Debug.Log("[Sprint6Setup] [FeedbackManager] GameObject 생성 및 FeedbackManager 컴포넌트 추가 완료.");
            return true;
        }

        // ═══════════════════════════════════════════════════════════
        // 3. SkillNodeUI 프리팹 생성
        // ═══════════════════════════════════════════════════════════

        static bool SetupSkillNodeUIPrefab()
        {
            // 이미 프리팹이 존재하면 건너뜀
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
            {
                Debug.Log($"[Sprint6Setup] {PrefabPath} 가 이미 존재합니다. 건너뜁니다.");
                return true;
            }

            // 폴더 확인/생성
            string directory = Path.GetDirectoryName(PrefabPath);
            if (!AssetDatabase.IsValidFolder(directory))
            {
                // Assets/Project/Prefabs/UI 경로를 단계별로 생성
                EnsureFolder("Assets/Project", "Prefabs");
                EnsureFolder("Assets/Project/Prefabs", "UI");
            }

            // ── Root 오브젝트 ──
            var root = new GameObject("SkillNodeUI");
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(100f, 100f);

            // Root: Image (배경)
            var bgImage = root.AddComponent<Image>();
            bgImage.color = new Color32(0x12, 0x1A, 0x2A, 0xFF);

            // Root: Button
            var button = root.AddComponent<Button>();
            button.targetGraphic = bgImage;

            // Root: SkillNodeUI 컴포넌트
            var skillNodeUI = root.AddComponent<UI.SkillNodeUI>();

            // ── Child: Border ──
            var borderGo = CreateChildWithRect(root.transform, "Border", Vector2.zero, rootRect.sizeDelta);
            var borderImage = borderGo.AddComponent<Image>();
            borderImage.color = new Color32(0x5B, 0x6B, 0x8A, 0xFF);
            borderImage.raycastTarget = false;

            // ── Child: Icon ──
            var iconGo = CreateChildWithRect(root.transform, "Icon", Vector2.zero, new Vector2(60f, 60f));
            var iconImage = iconGo.AddComponent<Image>();
            iconImage.color = Color.white;
            iconImage.raycastTarget = false;

            // ── Child: LevelText ──
            var levelGo = CreateChildWithRect(root.transform, "LevelText", Vector2.zero, new Vector2(100f, 20f));
            var levelRect = levelGo.GetComponent<RectTransform>();
            // Bottom-anchored
            levelRect.anchorMin = new Vector2(0f, 0f);
            levelRect.anchorMax = new Vector2(1f, 0f);
            levelRect.pivot = new Vector2(0.5f, 0f);
            levelRect.anchoredPosition = Vector2.zero;
            levelRect.sizeDelta = new Vector2(0f, 20f);

            var levelText = levelGo.AddComponent<Text>();
            levelText.text = "Lv.0/5";
            levelText.fontSize = 12;
            levelText.alignment = TextAnchor.MiddleCenter;
            levelText.color = Color.white;
            levelText.raycastTarget = false;
            // 기본 폰트 할당
            levelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // ── Child: LockedOverlay ──
            var lockedGo = new GameObject("LockedOverlay");
            var lockedRect = lockedGo.AddComponent<RectTransform>();
            lockedRect.SetParent(root.transform, false);
            // Stretch to fill parent
            lockedRect.anchorMin = Vector2.zero;
            lockedRect.anchorMax = Vector2.one;
            lockedRect.sizeDelta = Vector2.zero;
            lockedRect.anchoredPosition = Vector2.zero;

            // LockedOverlay: Image (반투명 검정)
            var lockedImage = lockedGo.AddComponent<Image>();
            lockedImage.color = new Color(0f, 0f, 0f, 0.6f);
            lockedImage.raycastTarget = false;

            // LockedOverlay > Text "잠금"
            var lockedTextGo = CreateChildWithRect(lockedGo.transform, "LockedText", Vector2.zero, Vector2.zero);
            var lockedTextRect = lockedTextGo.GetComponent<RectTransform>();
            lockedTextRect.anchorMin = Vector2.zero;
            lockedTextRect.anchorMax = Vector2.one;
            lockedTextRect.sizeDelta = Vector2.zero;
            lockedTextRect.anchoredPosition = Vector2.zero;

            var lockedText = lockedTextGo.AddComponent<Text>();
            lockedText.text = "잠금";
            lockedText.fontSize = 14;
            lockedText.alignment = TextAnchor.MiddleCenter;
            lockedText.color = new Color32(0xFF, 0x4D, 0x5A, 0xFF);
            lockedText.raycastTarget = false;
            lockedText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 기본 비활성
            lockedGo.SetActive(false);

            // ── SkillNodeUI 필드 자동 연결 ──
            skillNodeUI.iconImage = iconImage;
            skillNodeUI.levelText = levelText;
            skillNodeUI.button = button;
            skillNodeUI.lockedOverlay = lockedGo;
            skillNodeUI.backgroundImage = bgImage;
            skillNodeUI.borderImage = borderImage;
            skillNodeUI.lockedText = lockedText;

            // ── 프리팹으로 저장 ──
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);

            if (prefab == null)
            {
                Debug.LogError($"[Sprint6Setup] 프리팹 저장 실패: {PrefabPath}");
                return false;
            }

            Debug.Log($"[Sprint6Setup] SkillNodeUI 프리팹 생성 완료: {PrefabPath}");

            // ── HubUI.skillNodePrefab 연결 시도 ──
            AssignPrefabToHubUI(prefab);

            return true;
        }

        /// <summary>
        /// HubUI 컴포넌트를 씬에서 찾아 skillNodePrefab 필드에 프리팹을 할당합니다.
        /// 리플렉션으로 필드 존재 여부를 확인하여 필드가 없어도 에러 없이 처리합니다.
        /// </summary>
        static void AssignPrefabToHubUI(GameObject prefab)
        {
            var hubUI = Object.FindFirstObjectByType<UI.HubUI>();
            if (hubUI == null)
            {
                Debug.LogWarning("[Sprint6Setup] HubUI를 씬에서 찾을 수 없습니다. skillNodePrefab 할당을 건너뜁니다.");
                return;
            }

            // 리플렉션으로 skillNodePrefab 필드 존재 확인
            var field = typeof(UI.HubUI).GetField("skillNodePrefab",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (field == null)
            {
                Debug.LogWarning("[Sprint6Setup] HubUI에 skillNodePrefab 필드가 없습니다. HubUI 코드에 필드 추가 후 재실행하세요.");
                return;
            }

            var skillNodeUI = prefab.GetComponent<UI.SkillNodeUI>();
            if (skillNodeUI != null)
            {
                field.SetValue(hubUI, skillNodeUI);
                EditorUtility.SetDirty(hubUI);
                MarkActiveSceneDirty();
                Debug.Log("[Sprint6Setup] HubUI.skillNodePrefab에 SkillNodeUI 프리팹 할당 완료.");
            }
            else
            {
                Debug.LogWarning("[Sprint6Setup] 프리팹에서 SkillNodeUI 컴포넌트를 찾을 수 없습니다.");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // 헬퍼
        // ═══════════════════════════════════════════════════════════

        static GameObject CreateChildWithRect(Transform parent, string name, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject(name);
            var rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;
            return go;
        }

        static void EnsureFolder(string parentPath, string folderName)
        {
            string fullPath = $"{parentPath}/{folderName}";
            if (!AssetDatabase.IsValidFolder(fullPath))
            {
                AssetDatabase.CreateFolder(parentPath, folderName);
            }
        }

        static void MarkActiveSceneDirty()
        {
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
        }
    }
}
