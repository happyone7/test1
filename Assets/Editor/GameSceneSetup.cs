using UnityEditor;
using UnityEngine;

namespace Nodebreaker.Editor
{
    public class GameSceneSetup
    {
        [MenuItem("Tools/Nodebreaker/Setup Game Scene")]
        static void SetupGameScene()
        {
            // 카메라 설정
            var cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 8f;
                cam.transform.position = new Vector3(0, 10, 0);
                cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);

                if (cam.GetComponent<Core.GameCamera>() == null)
                    cam.gameObject.AddComponent<Core.GameCamera>();
            }

            // GameManager
            CreateSingleton<Core.GameManager>("GameManager");
            CreateSingleton<Core.RunManager>("RunManager");
            CreateSingleton<Tower.TowerManager>("TowerManager");
            CreateSingleton<Tesseract.ObjectPool.PoolManager>("PoolManager");

            // WaveSpawner + 웨이포인트
            var spawnerObj = CreateSingleton<Node.WaveSpawner>("WaveSpawner");
            var spawner = spawnerObj.GetComponent<Node.WaveSpawner>();

            // 스폰 포인트
            var spawnPoint = CreateOrFind("SpawnPoint");
            spawnPoint.transform.position = new Vector3(-8, 0, 0);
            spawner.spawnPoint = spawnPoint.transform;

            // 웨이포인트 경로 (좌→우 S자 형태)
            var waypointParent = CreateOrFind("Waypoints");
            Vector3[] positions = {
                new Vector3(-6, 0, 0),
                new Vector3(-2, 0, 3),
                new Vector3(2, 0, -3),
                new Vector3(6, 0, 0),
                new Vector3(8, 0, 0)
            };

            Transform[] waypoints = new Transform[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                var wp = CreateOrFind($"WP_{i}", waypointParent.transform);
                wp.transform.position = positions[i];
                waypoints[i] = wp.transform;
            }
            spawner.waypoints = waypoints;

            // 기지 (경로 끝)
            var baseObj = CreateOrFind("Base");
            baseObj.transform.position = new Vector3(9, 0, 0);

            // 바닥
            var ground = CreateOrFind("Ground");
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(20, 0.1f, 12);
            if (ground.GetComponent<MeshRenderer>() == null)
            {
                var filter = ground.AddComponent<MeshFilter>();
                filter.sharedMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
                var renderer = ground.AddComponent<MeshRenderer>();
            }

            // UI Canvas
            if (Object.FindFirstObjectByType<Canvas>() == null)
            {
                var canvasObj = new GameObject("Canvas");
                var canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            Debug.Log("[Nodebreaker] Game Scene 설정 완료!");
        }

        static GameObject CreateSingleton<T>(string name) where T : Component
        {
            var existing = Object.FindFirstObjectByType<T>();
            if (existing != null) return existing.gameObject;

            var obj = new GameObject(name);
            obj.AddComponent<T>();
            return obj;
        }

        static GameObject CreateOrFind(string name, Transform parent = null)
        {
            var existing = GameObject.Find(name);
            if (existing != null) return existing;

            var obj = new GameObject(name);
            if (parent != null)
                obj.transform.SetParent(parent);
            return obj;
        }
    }
}
