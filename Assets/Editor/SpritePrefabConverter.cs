using UnityEditor;
using UnityEngine;

/// <summary>
/// Sprint 2 TA: 3D 프리팹을 2D SpriteRenderer 기반으로 변환하고
/// 다크 판타지 스타일 락 스프라이트를 적용하는 에디터 유틸리티.
/// </summary>
public static class SpritePrefabConverter
{
    [MenuItem("Tools/TA/Convert Prefabs to 2D Sprites")]
    public static void ConvertAllPrefabs()
    {
        ConvertArrowTower();
        ConvertMonsterBit();
        CreateMonsterPrefab("Soul", "Assets/Art/Sprites/Monsters/Soul/Soul_Concept_A.png");
        CreateMonsterPrefab("Charger", "Assets/Art/Sprites/Monsters/Charger/Charger_Concept_A.png");
        CreateMonsterPrefab("Brute", "Assets/Art/Sprites/Monsters/Brute/Brute_Concept_A.png");
        ConvertArrowProjectile();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[TA] 모든 프리팹 2D 변환 완료!");
    }

    static void ConvertArrowTower()
    {
        string prefabPath = "Assets/Project/Prefabs/Towers/Tower_Arrow.prefab";
        string spritePath = "Assets/Art/Sprites/Towers/ArrowTower/Final/ArrowTower_Level1_Minimal.png";

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) { Debug.LogError("[TA] prefab not found: " + prefabPath); return; }

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null) { Debug.LogError("[TA] sprite not found: " + spritePath); return; }

        using (var editScope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
        {
            var root = editScope.prefabContentsRoot;

            RemoveComponent<MeshFilter>(root);
            RemoveComponent<MeshRenderer>(root);
            RemoveComponent<BoxCollider>(root);

            var sr = root.GetComponent<SpriteRenderer>();
            if (sr == null) sr = root.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 10;

            var col2d = root.GetComponent<BoxCollider2D>();
            if (col2d == null) col2d = root.AddComponent<BoxCollider2D>();
            col2d.size = new Vector2(0.8f, 0.8f);

            root.transform.localScale = new Vector3(1f, 1f, 1f);

            var firePoint = root.transform.Find("FirePoint");
            if (firePoint != null)
            {
                firePoint.localPosition = new Vector3(0f, 0.4f, 0f);
                firePoint.localScale = Vector3.one;
            }
        }

        Debug.Log("[TA] Tower_Arrow 2D 변환 완료");
    }

    static void ConvertMonsterBit()
    {
        string prefabPath = "Assets/Project/Prefabs/Monsters/Monster_Bit.prefab";
        string spritePath = "Assets/Art/Sprites/Monsters/Soul/Soul_Concept_A.png";

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) { Debug.LogError("[TA] prefab not found: " + prefabPath); return; }

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null) { Debug.LogError("[TA] sprite not found: " + spritePath); return; }

        using (var editScope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
        {
            var root = editScope.prefabContentsRoot;

            RemoveComponent<MeshFilter>(root);
            RemoveComponent<MeshRenderer>(root);

            var sr = root.GetComponent<SpriteRenderer>();
            if (sr == null) sr = root.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 5;

            root.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        Debug.Log("[TA] Monster_Bit 2D 변환 완료 (Soul 스프라이트 적용)");
    }

    static void CreateMonsterPrefab(string monsterName, string spritePath)
    {
        string prefabDir = "Assets/Project/Prefabs/Monsters";
        string prefabPath = prefabDir + "/Monster_" + monsterName + ".prefab";

        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            Debug.Log("[TA] Monster_" + monsterName + " already exists, updating sprite");
            UpdateMonsterSprite(prefabPath, spritePath);
            return;
        }

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null) { Debug.LogError("[TA] sprite not found: " + spritePath); return; }

        string templatePath = "Assets/Project/Prefabs/Monsters/Monster_Bit.prefab";
        var template = AssetDatabase.LoadAssetAtPath<GameObject>(templatePath);
        if (template == null) { Debug.LogError("[TA] template not found: " + templatePath); return; }

        AssetDatabase.CopyAsset(templatePath, prefabPath);

        using (var editScope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
        {
            var root = editScope.prefabContentsRoot;
            root.name = "Monster_" + monsterName;

            var sr = root.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = sprite;
        }

        Debug.Log("[TA] Monster_" + monsterName + " prefab created: " + spritePath);
    }

    static void UpdateMonsterSprite(string prefabPath, string spritePath)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null) return;

        using (var editScope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
        {
            var root = editScope.prefabContentsRoot;
            var sr = root.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = sprite;
        }
    }

    static void ConvertArrowProjectile()
    {
        string prefabPath = "Assets/Project/Prefabs/Projectiles/Projectile_Arrow.prefab";

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) { Debug.LogError("[TA] prefab not found: " + prefabPath); return; }

        using (var editScope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
        {
            var root = editScope.prefabContentsRoot;

            RemoveComponent<MeshFilter>(root);
            RemoveComponent<MeshRenderer>(root);

            var sr = root.GetComponent<SpriteRenderer>();
            if (sr == null) sr = root.AddComponent<SpriteRenderer>();
            sr.sprite = null;
            sr.color = new Color(1f, 0.85f, 0.3f, 1f);
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 15;

            root.transform.localScale = new Vector3(0.15f, 0.3f, 1f);
        }

        Debug.Log("[TA] Projectile_Arrow 2D 변환 완료");
    }

    static void RemoveComponent<T>(GameObject go) where T : Component
    {
        var comp = go.GetComponent<T>();
        if (comp != null)
            Object.DestroyImmediate(comp);
    }
}
