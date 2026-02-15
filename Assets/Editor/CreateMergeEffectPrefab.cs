using UnityEngine;
using UnityEditor;

/// <summary>
/// 합성 이펙트 테스트용 에디터 도구.
/// Tools > Test Merge Effect 메뉴로 Game 뷰에서 이펙트 미리보기.
/// </summary>
public static class CreateMergeEffectPrefab
{
    [MenuItem("Tools/Test Merge Effect")]
    private static void TestMergeEffect()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[MergeVFX] Play 모드에서만 테스트할 수 있습니다.");
            return;
        }
        Nodebreaker.Tower.MergeVFX.Play(Vector3.zero, 2);
        Debug.Log("[MergeVFX] 테스트 이펙트를 (0,0,0)에 재생했습니다.");
    }
}
