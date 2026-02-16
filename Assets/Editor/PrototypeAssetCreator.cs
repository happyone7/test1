using UnityEditor;
using UnityEngine;
using Soulspire.Data;

namespace Soulspire.Editor
{
    public class PrototypeAssetCreator
    {
        [MenuItem("Tools/Soulspire/Create Prototype Assets")]
        static void CreatePrototypeAssets()
        {
            try
            {
                Debug.Log("[Soulspire] 프로토타입 에셋 생성 시작...");
                CreateFolders();
                var arrowProjectile = CreateProjectilePrefab();
                var bitNodePrefab = CreateNodePrefab("BitNode", Color.cyan);
                var arrowTowerPrefab = CreateTowerPrefab("ArrowTower", Color.green);

                var bitNodeData = CreateNodeData("Bit", bitNodePrefab);
                var arrowTowerData = CreateTowerData("Arrow", arrowTowerPrefab, arrowProjectile);
                var waveData = CreateWaveData(bitNodeData);
                var stageData = CreateStageData(waveData);

                // GameManager에 스테이지 데이터 연결
                var gm = Object.FindFirstObjectByType<Soulspire.Core.GameManager>();
                if (gm != null)
                {
                    gm.stages = new StageData[] { stageData };
                    EditorUtility.SetDirty(gm);
                    Debug.Log("[Soulspire] GameManager에 스테이지 데이터 연결 완료");
                }
                else
                {
                    Debug.LogWarning("[Soulspire] GameManager를 씬에서 찾을 수 없습니다. 먼저 Setup Game Scene을 실행하세요.");
                }

                // TowerManager에 타워 데이터 연결
                var tm = Object.FindFirstObjectByType<Soulspire.Tower.TowerManager>();
                if (tm != null)
                {
                    tm.availableTowers = new TowerData[] { arrowTowerData };
                    EditorUtility.SetDirty(tm);
                    Debug.Log("[Soulspire] TowerManager에 타워 데이터 연결 완료");
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[Soulspire] 프로토타입 에셋 생성 완료!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Soulspire] 에셋 생성 실패: {e.Message}\n{e.StackTrace}");
            }
        }

        static void CreateFolders()
        {
            CreateFolderIfNeeded("Assets/Data");
            CreateFolderIfNeeded("Assets/Data/Towers");
            CreateFolderIfNeeded("Assets/Data/Nodes");
            CreateFolderIfNeeded("Assets/Data/Waves");
            CreateFolderIfNeeded("Assets/Data/Stages");
            CreateFolderIfNeeded("Assets/Prefabs");
            CreateFolderIfNeeded("Assets/Prefabs/Towers");
            CreateFolderIfNeeded("Assets/Prefabs/Nodes");
            CreateFolderIfNeeded("Assets/Prefabs/Projectiles");
        }

        static void CreateFolderIfNeeded(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parts = path.Split('/');
                var current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    var next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }
        }

        static Material CreateMaterial(string name, Color color, string savePath)
        {
            // URP Lit 셰이더 찾기 (여러 이름 시도)
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null)
            {
                Debug.LogWarning($"[Soulspire] 셰이더를 찾을 수 없어 기본 Material 사용: {name}");
                var defaultMat = new Material(Shader.Find("Hidden/InternalErrorShader"));
                return defaultMat;
            }

            var mat = new Material(shader);
            // URP에서는 _BaseColor, Standard에서는 _Color
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
            else
                mat.color = color;

            AssetDatabase.CreateAsset(mat, savePath);
            return mat;
        }

        static GameObject CreateNodePrefab(string name, Color color)
        {
            var path = $"Assets/Prefabs/Nodes/{name}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.localScale = Vector3.one * 0.5f;

            var renderer = go.GetComponent<MeshRenderer>();
            var mat = CreateMaterial(name, color, $"Assets/Prefabs/Nodes/{name}_Mat.mat");
            renderer.sharedMaterial = mat;

            // 3D Collider 제거, 2D 추가
            Object.DestroyImmediate(go.GetComponent<SphereCollider>());
            go.AddComponent<CircleCollider2D>().radius = 0.25f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            go.AddComponent<Soulspire.Node.Node>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log($"[Soulspire] 프리팹 생성: {path}");
            return prefab;
        }

        static GameObject CreateTowerPrefab(string name, Color color)
        {
            var path = $"Assets/Prefabs/Towers/{name}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.localScale = new Vector3(0.6f, 0.8f, 0.6f);

            var renderer = go.GetComponent<MeshRenderer>();
            var mat = CreateMaterial(name, color, $"Assets/Prefabs/Towers/{name}_Mat.mat");
            renderer.sharedMaterial = mat;

            var firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(go.transform);
            firePoint.transform.localPosition = new Vector3(0, 0.5f, 0);

            go.AddComponent<Soulspire.Tower.Tower>();
            var tower = go.GetComponent<Soulspire.Tower.Tower>();
            tower.firePoint = firePoint.transform;

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log($"[Soulspire] 프리팹 생성: {path}");
            return prefab;
        }

        static GameObject CreateProjectilePrefab()
        {
            var path = "Assets/Project/Prefabs/Projectiles/Projectile_Arrow.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "ArrowProjectile";
            go.transform.localScale = new Vector3(0.1f, 0.2f, 0.1f);

            var renderer = go.GetComponent<MeshRenderer>();
            var mat = CreateMaterial("ArrowProjectile", Color.yellow, "Assets/Prefabs/Projectiles/ArrowProjectile_Mat.mat");
            renderer.sharedMaterial = mat;

            Object.DestroyImmediate(go.GetComponent<CapsuleCollider>());
            go.AddComponent<Soulspire.Projectile.Projectile>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log($"[Soulspire] 프리팹 생성: {path}");
            return prefab;
        }

        static NodeData CreateNodeData(string name, GameObject prefab)
        {
            var path = $"Assets/Data/Nodes/Node_{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<NodeData>(path);
            if (existing != null) return existing;

            var data = ScriptableObject.CreateInstance<NodeData>();
            data.nodeId = name.ToLower();
            data.nodeName = name;
            data.type = NodeType.Bit;
            data.prefab = prefab;
            data.hp = 30f;
            data.speed = 2f;
            data.bitDrop = 5;
            data.damage = 1;

            AssetDatabase.CreateAsset(data, path);
            return data;
        }

        static TowerData CreateTowerData(string name, GameObject prefab, GameObject projectile)
        {
            var path = $"Assets/Data/Towers/Tower_{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<TowerData>(path);
            if (existing != null) return existing;

            var data = ScriptableObject.CreateInstance<TowerData>();
            data.towerId = name.ToLower();
            data.towerName = name;
            data.type = TowerType.Arrow;
            data.prefab = prefab;
            data.projectilePrefab = projectile;
            data.damage = new float[] { 10f, 15f, 22f, 33f, 50f };
            data.attackSpeed = new float[] { 1f, 1.1f, 1.2f, 1.35f, 1.5f };
            data.range = new float[] { 3f, 3f, 3.5f, 3.5f, 4f };
            data.placeCost = 50;

            AssetDatabase.CreateAsset(data, path);
            return data;
        }

        static WaveData CreateWaveData(NodeData bitNode)
        {
            var path = "Assets/Data/Waves/Wave_01.asset";
            var existing = AssetDatabase.LoadAssetAtPath<WaveData>(path);
            if (existing != null) return existing;

            var data = ScriptableObject.CreateInstance<WaveData>();
            data.spawnGroups = new WaveData.SpawnGroup[]
            {
                new WaveData.SpawnGroup
                {
                    nodeData = bitNode,
                    count = 8,
                    spawnInterval = 0.8f
                }
            };
            data.delayBeforeWave = 2f;

            AssetDatabase.CreateAsset(data, path);
            return data;
        }

        static StageData CreateStageData(WaveData wave)
        {
            var path = "Assets/Data/Stages/Stage_01.asset";
            var existing = AssetDatabase.LoadAssetAtPath<StageData>(path);
            if (existing != null) return existing;

            var data = ScriptableObject.CreateInstance<StageData>();
            data.stageId = "stage_01";
            data.stageName = "시작의 회로";
            data.stageIndex = 0;
            data.waves = new WaveData[] { wave };
            data.hpMultiplier = 1f;
            data.speedMultiplier = 1f;
            data.bitDropMultiplier = 1f;
            data.coreReward = 2;
            data.baseHp = 10;

            AssetDatabase.CreateAsset(data, path);
            return data;
        }
    }
}
