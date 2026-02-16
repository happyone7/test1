using UnityEngine;
using UnityEngine.Serialization;

namespace Soulspire.Data
{
    public enum NodeType
    {
        Bit,
        Quick,
        Heavy,
        Shield,
        Swarm,
        Regen,
        Phase,
        Split,
        Boss
    }

    public enum BossAbilityType
    {
        None,
        Enrage,     // HP 50% 이하 시 속도 2배
        ArmorRegen  // 10초마다 defense +5 (최대 25), AoE 피격 시 리셋
    }

    [CreateAssetMenu(fileName = "Node_", menuName = "Soulspire/Node Data")]
    public class NodeData : ScriptableObject
    {
        public string nodeId;
        public string nodeName;
        public NodeType type;
        public Sprite icon;
        public GameObject prefab;

        [Header("기본 스탯")]
        public float hp = 30f;
        public float speed = 2f;
        [FormerlySerializedAs("bitDrop")]
        public int soulDrop = 5;
        public int damage = 1; // 기지에 주는 데미지
        public int defense = 0; // 방어력 (받는 데미지 차감, 최소 1)

        [Header("보스 능력")]
        public BossAbilityType bossAbilityType = BossAbilityType.None;
    }
}
