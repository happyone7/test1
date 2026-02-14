using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace Nodebreaker.Editor
{
    public static class AIUIAssetPipeline
    {
        private const string SelectedHubDir = "Assets/Art/Source/AI/Selected/HubIcons";
        private const string SelectedLogoPath = "Assets/Art/Source/AI/Selected/Brand/NB_TitleLogo_Selected.png";
        private const string UiHubDir = "Assets/Art/UI/Hub";
        private const string UiLogoDir = "Assets/Art/UI/Logo";
        private const string HubAtlasPath = "Assets/Art/UI/Hub/NB_HubIcons.spriteatlas";

        private static readonly Dictionary<string, string> HubIconMap = new()
        {
            { "NB_HubIcon_AttackPower.png", "NB_Hub_AttackPower.png" },
            { "NB_HubIcon_AttackSpeed.png", "NB_Hub_AttackSpeed.png" },
            { "NB_HubIcon_BaseHP.png", "NB_Hub_BaseHP.png" },
            { "NB_HubIcon_BitGain.png", "NB_Hub_BitGain.png" },
            { "NB_HubIcon_CoreNode.png", "NB_Hub_CoreNode.png" },
            { "NB_HubIcon_Critical.png", "NB_Hub_Critical.png" },
            { "NB_HubIcon_HPRegen.png", "NB_Hub_HPRegen.png" },
            { "NB_HubIcon_IdleCollector.png", "NB_Hub_IdleCollector.png" },
            { "NB_HubIcon_Range.png", "NB_Hub_Range.png" },
            { "NB_HubIcon_SpawnRate.png", "NB_Hub_SpawnRate.png" },
            { "NB_HubIcon_SpeedMode.png", "NB_Hub_SpeedMode.png" },
            { "NB_HubIcon_StartBit.png", "NB_Hub_StartBit.png" },
            { "NB_HubIcon_TowerSlot.png", "NB_Hub_TowerSlot.png" },
            { "NB_HubIcon_UnlockCannon.png", "NB_Hub_UnlockCannon.png" },
            { "NB_HubIcon_UnlockIce.png", "NB_Hub_UnlockIce.png" },
            { "NB_HubIcon_UnlockLaser.png", "NB_Hub_UnlockLaser.png" },
            { "NB_HubIcon_UnlockLightning.png", "NB_Hub_UnlockLightning.png" },
            { "NB_HubIcon_UnlockVoid.png", "NB_Hub_UnlockVoid.png" },
        };

        [MenuItem("Tools/Nodebreaker/AI Assets/Run All (Sync + Import + Atlas)")]
        public static void RunAll()
        {
            ApplySelectedUIAssets();
            ApplyUISpriteImportSettings();
            BuildHubSpriteAtlas();
        }

        [MenuItem("Tools/Nodebreaker/AI Assets/Apply Selected UI Assets")]
        public static void ApplySelectedUIAssets()
        {
            Directory.CreateDirectory(ToAbsolutePath(UiHubDir));
            Directory.CreateDirectory(ToAbsolutePath(UiLogoDir));

            int copied = 0;
            int skipped = 0;
            bool hasError = false;

            string selectedLogoAbs = ToAbsolutePath(SelectedLogoPath);
            if (!File.Exists(selectedLogoAbs))
            {
                Debug.LogError($"[AIUIAssetPipeline] Missing selected logo: {SelectedLogoPath}");
                hasError = true;
            }
            else
            {
                CopyIfChanged(
                    selectedLogoAbs,
                    ToAbsolutePath(Path.Combine(UiLogoDir, "NB_Logo_003.png")),
                    ref copied,
                    ref skipped);
                CopyIfChanged(
                    selectedLogoAbs,
                    ToAbsolutePath(Path.Combine(UiLogoDir, "NB_Logo_Selected.png")),
                    ref copied,
                    ref skipped);
            }

            string selectedHubAbs = ToAbsolutePath(SelectedHubDir);
            foreach (var pair in HubIconMap)
            {
                string srcAbs = Path.Combine(selectedHubAbs, pair.Key);
                string dstAbs = ToAbsolutePath(Path.Combine(UiHubDir, pair.Value));

                if (!File.Exists(srcAbs))
                {
                    Debug.LogError($"[AIUIAssetPipeline] Missing selected icon: {ToAssetPath(srcAbs)}");
                    hasError = true;
                    continue;
                }

                CopyIfChanged(srcAbs, dstAbs, ref copied, ref skipped);
            }

            AssetDatabase.Refresh();
            Debug.Log($"[AIUIAssetPipeline] Sync complete. copied={copied}, skipped={skipped}");

            if (hasError)
            {
                Debug.LogError("[AIUIAssetPipeline] Some selected files were missing. Check logs.");
            }
        }

        [MenuItem("Tools/Nodebreaker/AI Assets/Apply UI Sprite Import Settings")]
        public static void ApplyUISpriteImportSettings()
        {
            int updated = 0;

            updated += ApplyImportSettingsInFolder(UiHubDir);
            updated += ApplyImportSettingsInFolder(UiLogoDir);

            Debug.Log($"[AIUIAssetPipeline] Applied sprite import settings to {updated} textures.");
        }

        [MenuItem("Tools/Nodebreaker/AI Assets/Build Hub SpriteAtlas")]
        public static void BuildHubSpriteAtlas()
        {
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(HubAtlasPath);
            if (atlas == null)
            {
                atlas = new SpriteAtlas();
                AssetDatabase.CreateAsset(atlas, HubAtlasPath);
            }

            var packing = atlas.GetPackingSettings();
            packing.enableRotation = false;
            packing.enableTightPacking = false;
            packing.padding = 2;
            atlas.SetPackingSettings(packing);

            var texture = atlas.GetTextureSettings();
            texture.filterMode = FilterMode.Point;
            texture.generateMipMaps = false;
            texture.readable = false;
            texture.sRGB = true;
            atlas.SetTextureSettings(texture);

            var defaultSettings = atlas.GetPlatformSettings("DefaultTexturePlatform");
            defaultSettings.overridden = true;
            defaultSettings.maxTextureSize = 2048;
            defaultSettings.textureCompression = TextureImporterCompression.Uncompressed;
            atlas.SetPlatformSettings(defaultSettings);

            var standaloneSettings = atlas.GetPlatformSettings("Standalone");
            standaloneSettings.overridden = true;
            standaloneSettings.maxTextureSize = 2048;
            standaloneSettings.textureCompression = TextureImporterCompression.Uncompressed;
            atlas.SetPlatformSettings(standaloneSettings);

            var oldPackables = SpriteAtlasExtensions.GetPackables(atlas);
            if (oldPackables != null && oldPackables.Length > 0)
            {
                SpriteAtlasExtensions.Remove(atlas, oldPackables);
            }

            var hubFolder = AssetDatabase.LoadMainAssetAtPath(UiHubDir);
            if (hubFolder == null)
            {
                Debug.LogError($"[AIUIAssetPipeline] Missing folder for atlas: {UiHubDir}");
                return;
            }

            SpriteAtlasExtensions.Add(atlas, new Object[] { hubFolder });
            EditorUtility.SetDirty(atlas);
            AssetDatabase.SaveAssets();

            SpriteAtlasUtility.PackAtlases(new[] { atlas }, EditorUserBuildSettings.activeBuildTarget, false);
            Debug.Log($"[AIUIAssetPipeline] Hub atlas ready: {HubAtlasPath}");
        }

        private static int ApplyImportSettingsInFolder(string folderPath)
        {
            int changed = 0;
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                bool dirty = false;
                if (importer.textureType != TextureImporterType.Sprite)
                { importer.textureType = TextureImporterType.Sprite; dirty = true; }
                if (importer.spriteImportMode != SpriteImportMode.Single)
                { importer.spriteImportMode = SpriteImportMode.Single; dirty = true; }
                if (!Mathf.Approximately(importer.spritePixelsPerUnit, 100f))
                { importer.spritePixelsPerUnit = 100f; dirty = true; }
                if (importer.filterMode != FilterMode.Point)
                { importer.filterMode = FilterMode.Point; dirty = true; }
                if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                { importer.textureCompression = TextureImporterCompression.Uncompressed; dirty = true; }
                if (importer.mipmapEnabled != false)
                { importer.mipmapEnabled = false; dirty = true; }
                if (importer.alphaIsTransparency != true)
                { importer.alphaIsTransparency = true; dirty = true; }

                if (dirty)
                {
                    importer.SaveAndReimport();
                    changed++;
                }
            }

            return changed;
        }

        private static void CopyIfChanged(string srcAbsPath, string dstAbsPath, ref int copied, ref int skipped)
        {
            if (File.Exists(dstAbsPath))
            {
                byte[] src = File.ReadAllBytes(srcAbsPath);
                byte[] dst = File.ReadAllBytes(dstAbsPath);
                if (src.SequenceEqual(dst))
                {
                    skipped++;
                    return;
                }
            }

            File.Copy(srcAbsPath, dstAbsPath, true);
            copied++;
        }


        private static string ToAbsolutePath(string assetPath)
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string normalized = assetPath.Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(projectRoot, normalized);
        }

        private static string ToAssetPath(string absolutePath)
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            if (absolutePath.StartsWith(projectRoot))
            {
                string local = absolutePath.Substring(projectRoot.Length).TrimStart(Path.DirectorySeparatorChar);
                return local.Replace(Path.DirectorySeparatorChar, '/');
            }

            return absolutePath;
        }
    }
}
