using UnityEngine;

namespace Nodebreaker.Data
{
    public enum TowerType
    {
        Arrow,
        Cannon,
        Ice,
        Lightning,
        Laser,
        Void
    }

    [CreateAssetMenu(fileName = "Tower_", menuName = "Nodebreaker/Tower Data")]
    public class TowerData : ScriptableObject
    {
        public string towerId;
        public string towerName;
        public TowerType type;
        public Sprite icon;
        public GameObject prefab;
        public GameObject projectilePrefab;

        [Header("레벨별 스탯 (Lv1~4, 5번째는 미사용)")]
        public float[] damage = { 10f, 15f, 22f, 33f, 50f };
        public float[] attackSpeed = { 1f, 1.1f, 1.2f, 1.35f, 1.5f };
        public float[] range = { 3f, 3f, 3.5f, 3.5f, 4f };

        [Header("인게임 배치 비용 (Bit)")]
        public int placeCost = 50;

        public float GetDamage(int level) => damage[Mathf.Clamp(level - 1, 0, damage.Length - 1)];
        public float GetAttackSpeed(int level) => attackSpeed[Mathf.Clamp(level - 1, 0, attackSpeed.Length - 1)];
        public float GetRange(int level) => range[Mathf.Clamp(level - 1, 0, range.Length - 1)];
    }
}
