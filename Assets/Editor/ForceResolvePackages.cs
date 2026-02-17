using UnityEditor;

public static class ForceResolvePackages
{
    [MenuItem("Tools/Force Resolve Packages")]
    public static void Resolve()
    {
        UnityEditor.PackageManager.Client.Resolve();
        UnityEngine.Debug.Log("[ForceResolve] Package resolve triggered!");
    }
}