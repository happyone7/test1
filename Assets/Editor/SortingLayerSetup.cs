using UnityEditor;
using UnityEngine;

namespace Soulspire.Editor
{
    /// <summary>
    /// ArtDirection_v0.1.md 기준 Sorting Layer를 자동 등록하는 에디터 스크립트.
    /// Unity가 TagManager.asset을 직접 관리하므로, SerializedObject API를 통해 안전하게 추가합니다.
    /// </summary>
    public class SortingLayerSetup
    {
        // 아트 디렉션 문서 기준 Sorting Layer 목록 (순서대로)
        private static readonly string[] RequiredSortingLayers = new string[]
        {
            "Background",   // 0  - 배경 단색/패턴
            "Tilemap",      // 10 - 타일맵 레이어
            "Decoration",   // 20 - 장식 오브젝트
            "Shadows",      // 25 - 타워/몬스터 그림자
            "Units",        // 30 - 몬스터
            "Towers",       // 35 - 타워
            "Projectiles",  // 40 - 투사체
            "Effects",      // 50 - 이펙트/파티클
            "UI_World"      // 60 - 월드 스페이스 UI
        };

        [MenuItem("Tools/Soulspire/Setup Sorting Layers")]
        static void SetupSortingLayers()
        {
            var tagManager = new SerializedObject(
                AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));

            var sortingLayers = tagManager.FindProperty("m_SortingLayers");

            int addedCount = 0;
            foreach (var layerName in RequiredSortingLayers)
            {
                if (!SortingLayerExists(sortingLayers, layerName))
                {
                    AddSortingLayer(sortingLayers, layerName);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                tagManager.ApplyModifiedProperties();
                Debug.Log($"[Soulspire] Sorting Layer {addedCount}개 추가 완료: {string.Join(", ", RequiredSortingLayers)}");
            }
            else
            {
                Debug.Log("[Soulspire] 모든 Sorting Layer가 이미 존재합니다.");
            }
        }

        static bool SortingLayerExists(SerializedProperty sortingLayers, string layerName)
        {
            for (int i = 0; i < sortingLayers.arraySize; i++)
            {
                var element = sortingLayers.GetArrayElementAtIndex(i);
                var nameProperty = element.FindPropertyRelative("name");
                if (nameProperty != null && nameProperty.stringValue == layerName)
                    return true;
            }
            return false;
        }

        static void AddSortingLayer(SerializedProperty sortingLayers, string layerName)
        {
            sortingLayers.InsertArrayElementAtIndex(sortingLayers.arraySize);
            var newLayer = sortingLayers.GetArrayElementAtIndex(sortingLayers.arraySize - 1);
            newLayer.FindPropertyRelative("name").stringValue = layerName;
            newLayer.FindPropertyRelative("uniqueID").intValue = GenerateUniqueID(sortingLayers);
            newLayer.FindPropertyRelative("locked").intValue = 0;
        }

        static int GenerateUniqueID(SerializedProperty sortingLayers)
        {
            // Unity는 양수 uniqueID를 사용. 기존 ID와 겹치지 않는 값 생성.
            int maxId = 0;
            for (int i = 0; i < sortingLayers.arraySize; i++)
            {
                var element = sortingLayers.GetArrayElementAtIndex(i);
                var idProp = element.FindPropertyRelative("uniqueID");
                if (idProp != null && idProp.intValue > maxId)
                    maxId = idProp.intValue;
            }
            return maxId + 1;
        }
    }
}
