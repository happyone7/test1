using UnityEngine;

namespace Nodebreaker.Data
{
    public enum SkillEffectType
    {
        AttackDamage,
        AttackSpeed,
        BaseHp,
        Range,
        BitGain,
        StartBit,
        SpawnRate,
        HpRegen,
        Critical,
        Idle,
        SpeedControl,
        TowerUnlock,
        None
    }

    /// <summary>
    /// 스킬 노드의 자원 타입 (Bit 또는 Core)
    /// </summary>
    public enum SkillResourceType
    {
        Bit,
        Core
    }

    /// <summary>
    /// 전제 조건 모드: OR(하나만 충족) 또는 AND(모두 충족)
    /// </summary>
    public enum PrerequisiteMode
    {
        /// <summary>선행 노드 중 하나만 구매하면 활성화</summary>
        Or,
        /// <summary>선행 노드 전부 구매해야 활성화</summary>
        And
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
        public SkillResourceType resourceType = SkillResourceType.Bit;
        public int baseCost;
        public float growthRate = 1.3f;

        [Header("Tree Position (Grid Coordinates)")]
        [Tooltip("그리드 좌표 (0,0)=Core Node. X: 우측 양수, Y: 위 양수")]
        public Vector2Int gridPosition;

        [Header("Prerequisites")]
        public string[] prerequisiteIds;
        public PrerequisiteMode prerequisiteMode = PrerequisiteMode.Or;

        [Header("Connections (후속 노드 ID)")]
        [Tooltip("이 노드에서 연결선이 뻗어나가는 후속 노드 ID 목록")]
        public string[] connectedNodeIds;

        // --- 레거시 호환 ---
        [HideInInspector] public Vector2 position;

        public int GetCost(int currentLevel)
        {
            if (maxLevel <= 1)
                return baseCost; // 1회 구매형은 고정 비용
            return Mathf.RoundToInt(baseCost * Mathf.Pow(growthRate, currentLevel));
        }

        public float GetTotalValue(int level)
        {
            return valuePerLevel * level;
        }

        /// <summary>
        /// 반복 구매형(Bit 노드)인지 여부
        /// </summary>
        public bool IsRepeatable => maxLevel > 1;
    }
}
