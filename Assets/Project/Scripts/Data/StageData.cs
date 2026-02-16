using UnityEngine;
using UnityEngine.Serialization;

namespace Soulspire.Data
{
    [CreateAssetMenu(fileName = "Stage_", menuName = "Soulspire/Stage Data")]
    public class StageData : ScriptableObject
    {
        public string stageId;
        public string stageName;
        public int stageIndex;

        [Header("웨이브")]
        public WaveData[] waves;

        [Header("스케일링 배율")]
        public float hpMultiplier = 1f;
        public float speedMultiplier = 1f;
        [FormerlySerializedAs("bitDropMultiplier")]
        public float soulDropMultiplier = 1f;

        [Header("클리어 보상")]
        [FormerlySerializedAs("coreReward")]
        public int coreFragmentReward = 2;

        [Header("기지")]
        public int baseHp = 10;
    }
}
