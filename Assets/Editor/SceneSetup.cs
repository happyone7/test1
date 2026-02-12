using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// 메뉴에서 Tools > Setup Game Scene 을 클릭하면
/// 기본 도형 + 플레이어 + 카메라가 포함된 씬을 자동 생성합니다.
/// </summary>
public class SceneSetup
{
    [MenuItem("Tools/Setup Game Scene")]
    public static void SetupScene()
    {
        // 새 씬 생성
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // === 바닥 (Ground) ===
        var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0f, -0.5f, 0f);
        ground.transform.localScale = new Vector3(30f, 1f, 30f);
        var groundRenderer = ground.GetComponent<Renderer>();
        var groundMat = new Material(Shader.Find("Standard"));
        groundMat.color = new Color(0.3f, 0.3f, 0.3f);
        groundRenderer.material = groundMat;
        AssetDatabase.CreateAsset(groundMat, "Assets/GroundMaterial.mat");

        // === 플레이어 (Cube) ===
        var player = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.name = "Player";
        player.transform.position = new Vector3(0f, 1f, 0f);
        player.transform.localScale = Vector3.one;
        var playerRb = player.AddComponent<Rigidbody>();
        playerRb.constraints = RigidbodyConstraints.FreezeRotation;
        player.AddComponent<PlayerController>();
        var playerMat = new Material(Shader.Find("Standard"));
        playerMat.color = new Color(0.2f, 0.6f, 1f); // 파란색
        player.GetComponent<Renderer>().material = playerMat;
        AssetDatabase.CreateAsset(playerMat, "Assets/PlayerMaterial.mat");

        // === 장애물들 ===
        CreateObstacle("Obstacle_1", new Vector3(5f, 0.75f, 3f), new Vector3(2f, 1.5f, 2f), new Color(1f, 0.3f, 0.3f));
        CreateObstacle("Obstacle_2", new Vector3(-4f, 0.5f, 6f), new Vector3(3f, 1f, 1f), new Color(0.3f, 1f, 0.3f));
        CreateObstacle("Obstacle_3", new Vector3(2f, 1f, -5f), new Vector3(1.5f, 2f, 1.5f), new Color(1f, 1f, 0.3f));

        // === 구체 장애물 ===
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "Sphere_Obstacle";
        sphere.transform.position = new Vector3(-3f, 1f, -4f);
        sphere.transform.localScale = Vector3.one * 2f;
        var sphereMat = new Material(Shader.Find("Standard"));
        sphereMat.color = new Color(1f, 0.5f, 0f); // 주황색
        sphere.GetComponent<Renderer>().material = sphereMat;
        AssetDatabase.CreateAsset(sphereMat, "Assets/SphereMaterial.mat");

        // === 카메라 설정 ===
        var mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(0f, 8f, -6f);
            mainCam.transform.LookAt(player.transform);
            var camFollow = mainCam.gameObject.AddComponent<CameraFollow>();
            camFollow.target = player.transform;
        }

        // === SteamManager ===
        var steamObj = new GameObject("SteamManager");
        steamObj.AddComponent<SteamManager>();

        // === 조명 강화 ===
        var dirLight = GameObject.Find("Directional Light");
        if (dirLight != null)
        {
            dirLight.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
            var light = dirLight.GetComponent<Light>();
            light.intensity = 1.2f;
        }

        // 씬 저장
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainScene.unity");
        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainScene.unity", true)
        };

        Debug.Log("=== Game Scene 생성 완료! Play 버튼을 눌러 테스트하세요. ===");
    }

    static void CreateObstacle(string name, Vector3 pos, Vector3 scale, Color color)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.position = pos;
        obj.transform.localScale = scale;
        var mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        obj.GetComponent<Renderer>().material = mat;
        AssetDatabase.CreateAsset(mat, "Assets/" + name + "_Material.mat");
    }
}
