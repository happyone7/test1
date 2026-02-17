using UnityEngine;

namespace Soulspire.Data
{
    public enum SkillEffectType
    {
        AttackDamage,   // 0
        AttackSpeed,    // 1
        BaseHp,         // 2
        Range,          // 3 - 타워 사거리 +0.2/lv
        SoulGain,       // 4 - Soul 획득량 +15%/lv
        StartSoul,      // 5 - 런 시작 Soul +30/lv
        SpawnRate,      // 6 - Node 스폰율 +10%/lv
        HpRegen         // 7 - 기지 HP 초당 +0.5/lv
    }

    [CreateAssetMenu(fileName = "Skill_", menuName = "Soulspire/Skill Node Data")]
    public class SkillNodeData : ScriptableObject
    {
        public string skillId;
        public string skillName;
        [TextArea] public string description;
        public Sprite icon;

        public SkillEffectType effectType;
        public float valuePerLevel;
        public int maxLevel;

        [Header("Cost")]
        public int baseCost;
        public float growthRate;

        [Header("Tree Position")]
        public Vector2 position;
        public string[] prerequisiteIds;

        [Header("Prerequisite Mode")]
        public bool prerequisiteIsOr; // true = OR (하나만 충족), false = AND (모두 충족, 기본값)

        public int GetCost(int currentLevel)
        {
            return Mathf.RoundToInt(baseCost * Mathf.Pow(growthRate, currentLevel));
        }

        public float GetTotalValue(int level)
        {
            return valuePerLevel * level;
        }
    }
}
