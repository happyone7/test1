using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 커맨드라인 또는 에디터 메뉴에서 빌드를 실행합니다.
/// 메뉴: Tools > Build Windows
/// CLI: Unity -batchmode -executeMethod BuildScript.BuildWindows -quit
/// </summary>
public class BuildScript
{
    [MenuItem("Tools/Build Windows")]
    public static void BuildWindows()
    {
        string buildPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "SteamBuild", "content");
        string exePath = Path.Combine(buildPath, "MyGame.exe");

        // EditorBuildSettings도 GameScene만 포함하도록 동기화
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity", true)
        };

        // content 폴더 정리
        if (Directory.Exists(buildPath))
            Directory.Delete(buildPath, true);
        Directory.CreateDirectory(buildPath);

        // 빌드 실행
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/GameScene.unity" },
            locationPathName = exePath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("=== 빌드 성공! 경로: " + exePath + " ===");
            Debug.Log("=== 이제 SteamBuild/upload.bat 으로 Steam에 업로드하세요. ===");
        }
        else
        {
            Debug.LogError("=== 빌드 실패 ===");
            foreach (var step in report.steps)
            {
                foreach (var msg in step.messages)
                {
                    if (msg.type == LogType.Error)
                        Debug.LogError(msg.content);
                }
            }
        }
    }
}
