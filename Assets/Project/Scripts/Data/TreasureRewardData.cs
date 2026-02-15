using UnityEngine;

namespace Nodebreaker.Data
{
    public enum TreasureRewardType
    {
        /// <summary>공격력 증가 (%)</summary>
        AttackDamageUp,
        /// <summary>공격 속도 증가 (%)</summary>
        AttackSpeedUp,
        /// <summary>사거리 증가</summary>
        RangeUp,
        /// <summary>Bit 획득량 증가 (%)</summary>
        BitGainUp,
        /// <summary>기지 HP 회복</summary>
        BaseHpRestore,
        /// <summary>기지 최대 HP 증가</summary>
        BaseMaxHpUp,
        /// <summary>랜덤 타워 획득</summary>
        RandomTower,
        /// <summary>모든 타워 공격속도 일시 증가</summary>
        GlobalAttackSpeedBurst
    }

    [CreateAssetMenu(fileName = "TreasureReward_", menuName = "Nodebreaker/Treasure Reward")]
    public class TreasureRewardData : ScriptableObject
    {
        public string rewardId;
        public string rewardName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;

        [Header("효과")]
        public TreasureRewardType rewardType;

        [Tooltip("효과 수치 (%, 절대값 등 rewardType에 따라 해석)")]
        public float value = 10f;

        [Header("가중치 (드랍 풀에서의 등장 빈도)")]
        public int weight = 10;

        [Header("희귀도 (0=일반, 1=레어, 2=에픽)")]
        public int rarity;
    }
}
