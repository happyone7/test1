using UnityEditor;
using UnityEngine;

/// <summary>
/// Sprint 2 TA: 게임 에셋 스프라이트 임포트 설정 일괄 조정.
/// 1024x1024 스프라이트를 PPU 1024로 설정하여 1 unit = 1 sprite가 되도록 함.
/// </summary>
public static class SpriteImportFixer
{
    [MenuItem("Tools/TA/Fix Sprite Import Settings")]
    public static void FixAllSpriteImports()
    {
        string[] spritePaths = new string[]
        {
            // Tower sprites (Final)
            "Assets/Art/Sprites/Towers/ArrowTower/Final/ArrowTower_Level1_Minimal.png",
            "Assets/Art/Sprites/Towers/ArrowTower/Final/ArrowTower_Level2_Basic.png",
            "Assets/Art/Sprites/Towers/ArrowTower/Final/ArrowTower_Level3_Ornate.png",
            "Assets/Art/Sprites/Towers/ArrowTower/Final/ArrowTower_Level4_Dark.png",

            // Monster sprites
            "Assets/Art/Sprites/Monsters/Soul/Soul_Concept_A.png",
            "Assets/Art/Sprites/Monsters/Soul/Soul_Concept_B.png",
            "Assets/Art/Sprites/Monsters/Charger/Charger_Concept_A.png",
            "Assets/Art/Sprites/Monsters/Charger/Charger_Concept_B.png",
            "Assets/Art/Sprites/Monsters/Brute/Brute_Concept_A.png",
            "Assets/Art/Sprites/Monsters/Brute/Brute_Concept_B.png",

            // Tile sprites (Final)
            "Assets/Art/Sprites/Tiles/Path/Final/Tile_Path.png",
            "Assets/Art/Sprites/Tiles/Buildable/Final/Tile_Buildable.png",
            "Assets/Art/Sprites/Tiles/Decoration/Final/Tile_Decoration.png",
            "Assets/Art/Sprites/Tiles/Special/Final/Tile_Entrance.png",
            "Assets/Art/Sprites/Tiles/Special/Final/Tile_Base.png",
        };

        int fixedCount = 0;

        foreach (var path in spritePaths)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"[TA] 임포터 없음 (파일 미존재?): {path}");
                continue;
            }

            bool changed = false;

            // Sprite 모드
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            // PPU: 1024 (1024x1024 이미지 → 1 unit)
            if (Mathf.Abs(importer.spritePixelsPerUnit - 1024f) > 0.1f)
            {
                importer.spritePixelsPerUnit = 1024f;
                changed = true;
            }

            // Filter Mode: Bilinear (다크 판타지 스타일은 픽셀아트가 아님)
            if (importer.filterMode != FilterMode.Bilinear)
            {
                importer.filterMode = FilterMode.Bilinear;
                changed = true;
            }

            // Compression: None (품질 우선, PC 타겟)
            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                changed = true;
            }

            // Mipmap: Off (2D 게임)
            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }

            // Alpha is Transparency
            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                changed = true;
            }

            // Max Texture Size
            if (importer.maxTextureSize != 1024)
            {
                importer.maxTextureSize = 1024;
                changed = true;
            }

            // Standalone platform override
            var standaloneSettings = importer.GetPlatformTextureSettings("Standalone");
            if (!standaloneSettings.overridden || standaloneSettings.textureCompression != TextureImporterCompression.Uncompressed)
            {
                standaloneSettings.overridden = true;
                standaloneSettings.maxTextureSize = 1024;
                standaloneSettings.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SetPlatformTextureSettings(standaloneSettings);
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
                fixedCount++;
                Debug.Log($"[TA] 임포트 설정 수정: {path}");
            }
        }

        Debug.Log($"[TA] 스프라이트 임포트 설정 완료: {fixedCount}개 수정됨");
    }
}
