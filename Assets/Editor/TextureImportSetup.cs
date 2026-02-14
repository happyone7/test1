using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Nodebreaker.Editor
{
    /// <summary>
    /// UI 에셋 텍스처 임포트 설정 도구.
    /// Sprite(2D and UI), PPU=16, Point filter, No compression, 9-slice border 자동 적용.
    /// </summary>
    public static class TextureImportSetup
    {
        // 9-slice border 정의: (Left, Bottom, Right, Top) = spriteBorder(x, y, z, w)
        // Unity spriteBorder: x=Left, y=Bottom, z=Right, w=Top
        private static readonly Dictionary<string, Vector4> NineSliceBorders = new()
        {
            // Frames
            { "panel_frame",    new Vector4(8, 8, 8, 8) },
            { "hp_bar_frame",   new Vector4(4, 2, 4, 2) },
            { "hp_bar_fill",    new Vector4(0, 0, 0, 0) },   // fill은 9-slice 불필요
            { "tower_slot",     new Vector4(6, 6, 6, 6) },
            { "tooltip_frame",  new Vector4(6, 6, 6, 6) },
            { "dropdown_frame", new Vector4(6, 6, 6, 6) },

            // Buttons (모두 border 6px)
            { "btn_basic_idle",     new Vector4(6, 6, 6, 6) },
            { "btn_basic_hover",    new Vector4(6, 6, 6, 6) },
            { "btn_basic_pressed",  new Vector4(6, 6, 6, 6) },
            { "btn_basic_disabled", new Vector4(6, 6, 6, 6) },
            { "btn_accent_idle",     new Vector4(6, 6, 6, 6) },
            { "btn_accent_hover",    new Vector4(6, 6, 6, 6) },
            { "btn_accent_pressed",  new Vector4(6, 6, 6, 6) },
            { "btn_accent_disabled", new Vector4(6, 6, 6, 6) },
        };

        /// <summary>
        /// 새 UI 에셋 폴더 (Assets/Art/UI/Frames, Buttons, Icons) 임포트 설정.
        /// PPU=16, Point filter, No compression, 9-slice border 적용.
        /// </summary>
        [MenuItem("Tools/Nodebreaker/Setup UI Art Import Settings")]
        public static void SetupUIArtTextures()
        {
            string[] uiArtFolders = new[]
            {
                "Assets/Art/UI/Frames",
                "Assets/Art/UI/Buttons",
                "Assets/Art/UI/Icons",
                "Assets/Art/UI/Backgrounds",
                "Assets/Art/UI/Logo",
            };

            int count = 0;
            foreach (var folder in uiArtFolders)
            {
                string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null) continue;

                    bool dirty = false;

                    // 배경/로고도 Point 필터 통일 (픽셀아트 스타일 일관성)
                    bool isLargeImage = folder.Contains("Backgrounds") || folder.Contains("Logo");
                    float targetPPU = isLargeImage ? 100f : 16f;
                    FilterMode targetFilter = FilterMode.Point;
                    TextureImporterCompression targetCompression = isLargeImage
                        ? TextureImporterCompression.CompressedHQ
                        : TextureImporterCompression.Uncompressed;

                    // 기본 설정: Sprite, Single
                    if (importer.textureType != TextureImporterType.Sprite)
                    { importer.textureType = TextureImporterType.Sprite; dirty = true; }

                    if (importer.spriteImportMode != SpriteImportMode.Single)
                    { importer.spriteImportMode = SpriteImportMode.Single; dirty = true; }

                    if (!Mathf.Approximately(importer.spritePixelsPerUnit, targetPPU))
                    { importer.spritePixelsPerUnit = targetPPU; dirty = true; }

                    if (importer.filterMode != targetFilter)
                    { importer.filterMode = targetFilter; dirty = true; }

                    if (importer.textureCompression != targetCompression)
                    { importer.textureCompression = targetCompression; dirty = true; }

                    if (importer.mipmapEnabled)
                    { importer.mipmapEnabled = false; dirty = true; }

                    if (!importer.alphaIsTransparency)
                    { importer.alphaIsTransparency = true; dirty = true; }

                    // 9-slice border 적용
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                    if (NineSliceBorders.TryGetValue(fileName, out Vector4 border))
                    {
                        if (importer.spriteBorder != border)
                        {
                            importer.spriteBorder = border;
                            dirty = true;
                        }
                    }

                    if (dirty)
                    {
                        importer.SaveAndReimport();
                        count++;
                        Debug.Log($"[TextureImportSetup] Configured: {path}" +
                                  (NineSliceBorders.ContainsKey(fileName) ? $" (9-slice: {NineSliceBorders[fileName]})" : ""));
                    }
                }
            }

            Debug.Log($"[TextureImportSetup] UI Art import setup complete. Updated {count} textures.");
        }

        /// <summary>
        /// 기존 에셋 폴더 (Towers, Nodes, Hub, Logo) 임포트 설정.
        /// PPU=100 (기존 값 유지).
        /// </summary>
        [MenuItem("Tools/Setup Texture Imports")]
        public static void SetupAllTextures()
        {
            string[] folders = new[]
            {
                "Assets/Art/Sprites/Towers",
                "Assets/Art/Sprites/Monsters",
                "Assets/Art/Sprites/Projectiles",
                "Assets/Art/Sprites/Tiles",
                "Assets/Art/UI/Hub",
                "Assets/Art/UI/Logo",
                "Assets/Art/UI/Backgrounds"
            };

            int count = 0;
            foreach (var folder in folders)
            {
                string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.Contains("_variants")) continue;

                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null) continue;

                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 100;
                    importer.filterMode = FilterMode.Point;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.mipmapEnabled = false;

                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    count++;
                }
            }

            Debug.Log($"[TextureImportSetup] Set sprite import settings for {count} textures.");
        }

        /// <summary>
        /// 전체 UI 에셋 임포트 상태 검증 (QA용).
        /// </summary>
        [MenuItem("Tools/Nodebreaker/Verify UI Art Import Settings")]
        public static void VerifyUIArtImportSettings()
        {
            string[] uiArtFolders = new[]
            {
                "Assets/Art/UI/Frames",
                "Assets/Art/UI/Buttons",
                "Assets/Art/UI/Icons",
                "Assets/Art/UI/Backgrounds",
                "Assets/Art/UI/Logo",
            };

            int total = 0;
            int passed = 0;
            int failed = 0;

            foreach (var folder in uiArtFolders)
            {
                string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null) continue;

                    total++;
                    var errors = new List<string>();

                    bool isLargeImage = folder.Contains("Backgrounds") || folder.Contains("Logo");
                    float expectedPPU = isLargeImage ? 100f : 16f;
                    FilterMode expectedFilter = FilterMode.Point;
                    TextureImporterCompression expectedCompression = isLargeImage
                        ? TextureImporterCompression.CompressedHQ
                        : TextureImporterCompression.Uncompressed;

                    if (importer.textureType != TextureImporterType.Sprite)
                        errors.Add($"textureType={importer.textureType} (expected Sprite)");
                    if (importer.spriteImportMode != SpriteImportMode.Single)
                        errors.Add($"spriteMode={importer.spriteImportMode} (expected Single)");
                    if (!Mathf.Approximately(importer.spritePixelsPerUnit, expectedPPU))
                        errors.Add($"PPU={importer.spritePixelsPerUnit} (expected {expectedPPU})");
                    if (importer.filterMode != expectedFilter)
                        errors.Add($"filterMode={importer.filterMode} (expected {expectedFilter})");
                    if (importer.textureCompression != expectedCompression)
                        errors.Add($"compression={importer.textureCompression} (expected {expectedCompression})");
                    if (importer.mipmapEnabled)
                        errors.Add("mipmap enabled (expected disabled)");

                    string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                    if (NineSliceBorders.TryGetValue(fileName, out Vector4 expectedBorder))
                    {
                        if (importer.spriteBorder != expectedBorder)
                            errors.Add($"border={importer.spriteBorder} (expected {expectedBorder})");
                    }

                    if (errors.Count == 0)
                    {
                        passed++;
                    }
                    else
                    {
                        failed++;
                        Debug.LogWarning($"[Verify] FAIL: {path}\n  - {string.Join("\n  - ", errors)}");
                    }
                }
            }

            if (failed == 0)
                Debug.Log($"[Verify] ALL PASS: {passed}/{total} UI art textures configured correctly.");
            else
                Debug.LogError($"[Verify] {failed}/{total} textures have incorrect settings. Run 'Tools/Nodebreaker/Setup UI Art Import Settings' to fix.");
        }
    }
}
