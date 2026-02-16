using UnityEngine;

namespace Nodebreaker.Data
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
        SlowEffect
    }

    public enum TreasureRarity
    {
        Common,
        Rare,
        Epic
    }

    [CreateAssetMenu(fileName = "TreasureChoice_", menuName = "Nodebreaker/Treasure Choice Data")]
    public class TreasureChoiceData : ScriptableObject
    {
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("효과")]
        public TreasureChoiceType choiceType;
        public float value;

        [Header("희귀도")]
        public TreasureRarity rarity;
    }
}
