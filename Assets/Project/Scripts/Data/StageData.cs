using UnityEngine;

namespace Nodebreaker.Data
{
    [CreateAssetMenu(fileName = "Stage_", menuName = "Nodebreaker/Stage Data")]
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
        public float bitDropMultiplier = 1f;

        [Header("클리어 보상")]
        public int coreReward = 2;

        [Header("기지")]
        public int baseHp = 10;
    }
}
