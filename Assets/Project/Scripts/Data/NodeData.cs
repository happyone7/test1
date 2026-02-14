using UnityEngine;

namespace Nodebreaker.Data
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

    [CreateAssetMenu(fileName = "Node_", menuName = "Nodebreaker/Node Data")]
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
        public int bitDrop = 5;
        public int damage = 1; // 기지에 주는 데미지
    }
}
