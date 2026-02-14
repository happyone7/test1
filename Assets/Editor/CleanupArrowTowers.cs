using UnityEditor;
using UnityEngine;

namespace Nodebreaker.Editor
{
    public static class CleanupArrowTowers
    {
        [MenuItem("Tools/Nodebreaker/Cleanup Pre-placed ArrowTowers")]
        static void Cleanup()
        {
            string[] names = { "ArrowTower_0", "ArrowTower_1", "ArrowTower_2" };
            int deleted = 0;

            foreach (var name in names)
            {
                // FindObjectsByType으로는 inactive를 못 찾으므로, 씬 루트부터 탐색
                var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                foreach (var root in roots)
                {
                    var found = FindInChildren(root.transform, name);
                    if (found != null)
                    {
                        Undo.DestroyObjectImmediate(found.gameObject);
                        deleted++;
                        Debug.Log($"[Cleanup] Deleted {name}");
                    }
                }
            }

            if (deleted > 0)
            {
                EditorUtility.SetDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()[0]);
                Debug.Log($"[Cleanup] Total deleted: {deleted} pre-placed ArrowTower(s)");
            }
            else
            {
                Debug.Log("[Cleanup] No pre-placed ArrowTowers found.");
            }
        }

        static Transform FindInChildren(Transform parent, string name)
        {
            if (parent.name == name)
                return parent;

            for (int i = 0; i < parent.childCount; i++)
            {
                var result = FindInChildren(parent.GetChild(i), name);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
