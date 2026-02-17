using UnityEngine;

namespace Soulspire.Data
{
    [CreateAssetMenu(fileName = "TreasureConfig", menuName = "Soulspire/Treasure Config")]
    public class TreasureConfig : ScriptableObject
    {
        [Header("드롭 설정")]
        [Range(0f, 1f)]
        public float dropChance = 0.3f;
        public int choicesPerDrop = 3;

        [Header("선택지 풀")]
        public TreasureChoiceData[] allChoices;

        [Header("희귀도 가중치")]
        public float commonWeight = 0.6f;
        public float rareWeight = 0.3f;
        public float epicWeight = 0.1f;
    }
}
