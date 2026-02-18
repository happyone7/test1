using UnityEngine;

namespace Soulspire.Data
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

    [CreateAssetMenu(fileName = "Tower_", menuName = "Soulspire/Tower Data")]
    public class TowerData : ScriptableObject
    {
        public string towerId;
        public string towerName;
        public TowerType type;
        public Sprite icon;
        public GameObject prefab;
        public GameObject projectilePrefab;

        [Header("레벨별 스탯 (Lv1~5)")]
        public float[] damage = { 10f, 15f, 22f, 33f, 50f };
        public float[] attackSpeed = { 1f, 1.1f, 1.2f, 1.35f, 1.5f };
        public float[] range = { 3f, 3f, 3.5f, 3.5f, 4f };

        [Header("인게임 배치 비용 (Soul)")]
        public int placeCost = 50;

        [Header("Cannon 전용 - AoE 폭발 범위 (레벨별)")]
        public float[] explosionRadius = { 0f, 0f, 0f, 0f, 0f };

        [Header("Ice 전용 - 감속 (레벨별)")]
        public float[] slowRate = { 0f, 0f, 0f, 0f, 0f };        // 0~1 (0.25 = 25% 감속)
        public float[] slowDuration = { 0f, 0f, 0f, 0f, 0f };    // 초

        [Header("Lightning 전용 - 체인 공격 (레벨별)")]
        public int[] chainCount = { 0, 0, 0, 0, 0 };             // 체인 횟수 (0이면 체인 없음)
        public float[] chainDamageDecay = { 0.3f, 0.3f, 0.3f, 0.3f, 0.3f }; // 체인 데미지 감소율
        public float chainRange = 3f;                              // 체인 탐색 범위

        public float GetDamage(int level) => damage[Mathf.Clamp(level - 1, 0, damage.Length - 1)];
        public float GetAttackSpeed(int level) => attackSpeed[Mathf.Clamp(level - 1, 0, attackSpeed.Length - 1)];
        public float GetRange(int level) => range[Mathf.Clamp(level - 1, 0, range.Length - 1)];
        public float GetExplosionRadius(int level) => explosionRadius[Mathf.Clamp(level - 1, 0, explosionRadius.Length - 1)];
        public float GetSlowRate(int level) => slowRate[Mathf.Clamp(level - 1, 0, slowRate.Length - 1)];
        public float GetSlowDuration(int level) => slowDuration[Mathf.Clamp(level - 1, 0, slowDuration.Length - 1)];
        public int GetChainCount(int level) => chainCount[Mathf.Clamp(level - 1, 0, chainCount.Length - 1)];
        public float GetChainDamageDecay(int level) => chainDamageDecay[Mathf.Clamp(level - 1, 0, chainDamageDecay.Length - 1)];

        [Header("업그레이드 비용 (Soul, 레벨별: Lv1->2, Lv2->3, Lv3->4, Lv4->5)")]
        public int[] upgradeCost = { 30, 60, 120, 240 };

        /// <summary>
        /// 현재 레벨에서 다음 레벨로 업그레이드하는 비용. 최대 레벨이면 -1 반환.
        /// </summary>
        public int GetUpgradeCost(int currentLevel)
        {
            int idx = currentLevel - 1; // Lv1->idx0, Lv2->idx1, Lv3->idx2
            if (idx < 0 || idx >= upgradeCost.Length) return -1;
            return upgradeCost[idx];
        }

        /// <summary>
        /// 최대 업그레이드 가능 레벨 (upgradeCost 배열 길이 + 1).
        /// </summary>
        public int MaxLevel => upgradeCost.Length + 1;
    }
}
