using UnityEditor;
using UnityEngine;

namespace Soulspire.Editor
{
    public class GameSceneSetup
    {
        [MenuItem("Tools/Soulspire/Setup Game Scene")]
        static void SetupGameScene()
        {
            // 카메라 설정 (표준 2D: XY 평면, -Z 방향)
            var cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 8f;
                cam.transform.position = new Vector3(0, 0, -10);
                cam.transform.rotation = Quaternion.identity;
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

            // 웨이포인트 경로 (좌→우 S자 형태, XY 평면)
            var waypointParent = CreateOrFind("Waypoints");
            Vector3[] positions = {
                new Vector3(-6, 0, 0),
                new Vector3(-2, 3, 0),
                new Vector3(2, -3, 0),
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

            // 바닥 (XY 평면에서 보이도록 Z=0.1 뒤쪽 배치, X축 90도 회전)
            var ground = CreateOrFind("Ground");
            ground.transform.position = new Vector3(0, 0, 0.1f);
            ground.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            ground.transform.localScale = new Vector3(20, 12, 0.1f);
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

            Debug.Log("[Soulspire] Game Scene 설정 완료!");
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
