using UnityEditor;
using UnityEngine;
using System.IO;

namespace Soulspire.Editor
{
    public static class AssetReorganizer
    {
        [MenuItem("Tools/Soulspire/Reorganize Assets")]
        public static void Reorganize()
        {
            // Ensure target folders exist
            EnsureFolder("Assets/Project");
            EnsureFolder("Assets/Project/ScriptableObjects");
            EnsureFolder("Assets/Project/Prefabs");
            EnsureFolder("Assets/Project/Scenes");
            EnsureFolder("Assets/Plugins");

            int moved = 0;

            // 1. Data/ → Project/ScriptableObjects/
            moved += MoveContents("Assets/Data", "Assets/Project/ScriptableObjects");

            // 2. Prefabs/ → Project/Prefabs/
            moved += MoveContents("Assets/Prefabs", "Assets/Project/Prefabs");

            // 3. Scenes/ → Project/Scenes/
            moved += MoveContents("Assets/Scenes", "Assets/Project/Scenes");

            // 4. MobileDependencyResolver/ → Plugins/MobileDependencyResolver/
            if (AssetDatabase.IsValidFolder("Assets/MobileDependencyResolver"))
            {
                string result = AssetDatabase.MoveAsset("Assets/MobileDependencyResolver", "Assets/Plugins/MobileDependencyResolver");
                if (string.IsNullOrEmpty(result))
                {
                    moved++;
                    Debug.Log("[Reorganize] Moved MobileDependencyResolver → Plugins/");
                }
                else
                {
                    Debug.LogWarning($"[Reorganize] Failed to move MobileDependencyResolver: {result}");
                }
            }

            // 5. Root .mat files → remove or move (check if any exist at root)
            var rootMats = AssetDatabase.FindAssets("t:Material", new[] { "Assets" });
            foreach (var guid in rootMats)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                // Only move materials directly in Assets/ (not in subfolders already under Project/)
                if (Path.GetDirectoryName(path).Replace("\\", "/") == "Assets")
                {
                    string dest = "Assets/Project/Prefabs/" + Path.GetFileName(path);
                    string r = AssetDatabase.MoveAsset(path, dest);
                    if (string.IsNullOrEmpty(r))
                    {
                        moved++;
                        Debug.Log($"[Reorganize] Moved root material {path} → {dest}");
                    }
                }
            }

            // Clean up empty folders
            DeleteIfEmpty("Assets/Data");
            DeleteIfEmpty("Assets/Prefabs");
            DeleteIfEmpty("Assets/Scenes");

            AssetDatabase.Refresh();
            Debug.Log($"[Soulspire] Asset reorganization complete! {moved} items moved.");
        }

        static int MoveContents(string source, string destination)
        {
            if (!AssetDatabase.IsValidFolder(source)) return 0;

            int count = 0;
            // Move subfolders
            foreach (string subfolder in AssetDatabase.GetSubFolders(source))
            {
                string folderName = Path.GetFileName(subfolder);
                string destPath = destination + "/" + folderName;

                if (AssetDatabase.IsValidFolder(destPath))
                {
                    // Destination subfolder already exists, move contents recursively
                    count += MoveContents(subfolder, destPath);
                    DeleteIfEmpty(subfolder);
                }
                else
                {
                    string result = AssetDatabase.MoveAsset(subfolder, destPath);
                    if (string.IsNullOrEmpty(result))
                    {
                        count++;
                        Debug.Log($"[Reorganize] Moved {subfolder} → {destPath}");
                    }
                    else
                    {
                        Debug.LogWarning($"[Reorganize] Failed to move {subfolder}: {result}");
                    }
                }
            }

            // Move files
            var guids = AssetDatabase.FindAssets("", new[] { source });
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                // Only direct children
                if (Path.GetDirectoryName(path).Replace("\\", "/") == source)
                {
                    string destPath = destination + "/" + Path.GetFileName(path);
                    string result = AssetDatabase.MoveAsset(path, destPath);
                    if (string.IsNullOrEmpty(result))
                    {
                        count++;
                        Debug.Log($"[Reorganize] Moved {path} → {destPath}");
                    }
                }
            }

            return count;
        }

        static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace("\\", "/");
                string name = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        static void DeleteIfEmpty(string path)
        {
            if (!AssetDatabase.IsValidFolder(path)) return;
            if (AssetDatabase.GetSubFolders(path).Length > 0) return;
            var assets = AssetDatabase.FindAssets("", new[] { path });
            if (assets.Length == 0)
            {
                AssetDatabase.DeleteAsset(path);
                Debug.Log($"[Reorganize] Deleted empty folder: {path}");
            }
        }
    }
}
