using UnityEngine;

namespace Nodebreaker.Data
{
    public enum SkillEffectType
    {
        AttackDamage,
        AttackSpeed,
        BaseHp
    }

    [CreateAssetMenu(fileName = "Skill_", menuName = "Nodebreaker/Skill Node Data")]
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
