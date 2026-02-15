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

        [Header("해금 조건")]
        public long unlockBitRequired;
        public int unlockKillRequired;

        /// <summary>
        /// 누적 Bit와 처치 수를 기준으로 해금 가능 여부를 판정한다.
        /// </summary>
        public bool IsUnlockable(long totalBitEarned, int totalKills)
        {
            return totalBitEarned >= unlockBitRequired && totalKills >= unlockKillRequired;
        }
    }
}
