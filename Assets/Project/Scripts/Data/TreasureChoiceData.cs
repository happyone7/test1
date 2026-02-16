using UnityEngine;

namespace Soulspire.Data
{
    public enum TreasureChoiceType
    {
        DamageBoost,
        AttackSpeedBoost,
        RangeBoost,
        BitBonus,
        MaxHpBoost,
        TowerCostDiscount,
        CritChance,
        SlowEffect,
        TowerReward
    }

    public enum TreasureRarity
    {
        Common,
        Rare,
        Epic
    }

    [CreateAssetMenu(fileName = "TreasureChoice_", menuName = "Soulspire/Treasure Choice Data")]
    public class TreasureChoiceData : ScriptableObject
    {
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("효과")]
        public TreasureChoiceType choiceType;
        public float value;

        [Header("타워 보상 (choiceType == TowerReward 시 사용)")]
        public TowerData rewardTower;

        [Header("희귀도")]
        public TreasureRarity rarity;
    }
}
