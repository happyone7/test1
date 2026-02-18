using System.Collections.Generic;
using Tesseract.Core;
using Tesseract.ObjectPool;
using UnityEngine;
using Soulspire.Audio;

namespace Soulspire.Tower
{
    public class Tower : MonoBehaviour
    {
        [Header("데이터")]
        public Data.TowerData data;

        [Header("런타임")]
        public int Level { get; private set; } = 1;

        float _attackTimer;
        Node.Node _currentTarget;

        [Header("발사 위치")]
        public Transform firePoint;

        public void Initialize(Data.TowerData towerData, int level = 1)
        {
            data = towerData;
            Level = level;
            _attackTimer = 0f;
            EnsureCollider2D();
        }

        /// <summary>
        /// Physics2D 기반 감지를 위해 BoxCollider2D와 Layer 8(Tower)을 보장합니다.
        /// prefab에 BoxCollider2D가 없거나 Layer가 잘못 설정된 경우 런타임에서 보정합니다.
        /// </summary>
        private void EnsureCollider2D()
        {
            var col2d = GetComponent<BoxCollider2D>();
            if (col2d == null)
            {
                col2d = gameObject.AddComponent<BoxCollider2D>();
            }
            if (col2d != null)
            {
                // 3D BoxCollider 크기를 참조하여 2D 크기 설정
                var col3d = GetComponent<BoxCollider>();
                if (col3d != null)
                {
                    col2d.size = new Vector2(col3d.size.x, col3d.size.y);
                    col2d.offset = new Vector2(col3d.center.x, col3d.center.y);
                }
            }
            // Layer 8 = Tower (PlacementGrid.towerLayer가 기대하는 레이어)
            if (gameObject.layer != 8)
                gameObject.layer = 8;
        }

        void Update()
        {
            if (data == null) return;

            _attackTimer += Time.deltaTime;

            if (_currentTarget == null || !_currentTarget.IsAlive)
                _currentTarget = FindTarget();

            float attackSpeed = data.GetAttackSpeed(Level);
            if (Singleton<Core.RunManager>.HasInstance)
                attackSpeed *= Core.RunManager.Instance.CurrentModifiers.attackSpeedMultiplier;
            if (Singleton<Core.TreasureManager>.HasInstance)
                attackSpeed *= Core.TreasureManager.Instance.AttackSpeedMultiplier;

            if (_currentTarget != null && _attackTimer >= 1f / attackSpeed)
            {
                Attack();
                _attackTimer = 0f;
            }
        }

        Node.Node FindTarget()
        {
            float range = data.GetRange(Level);
            if (Singleton<Core.TreasureManager>.HasInstance)
                range *= Core.TreasureManager.Instance.RangeMultiplier;
            var colliders = Physics2D.OverlapCircleAll(transform.position, range);

            Node.Node closest = null;
            float minDist = float.MaxValue;

            foreach (var col in colliders)
            {
                var node = col.GetComponent<Node.Node>();
                if (node != null && node.IsAlive)
                {
                    float dist = Vector2.Distance(transform.position, node.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = node;
                    }
                }
            }
            return closest;
        }

        void Attack()
        {
            SoundManager.Instance.PlaySfx(SoundKeys.TowerAttack, 0.85f);

            float damage = data.GetDamage(Level);
            if (Singleton<Core.RunManager>.HasInstance)
                damage *= Core.RunManager.Instance.CurrentModifiers.attackDamageMultiplier;
            if (Singleton<Core.TreasureManager>.HasInstance)
                damage *= Core.TreasureManager.Instance.DamageMultiplier;

            // 보물 크리티컬 판정
            if (Singleton<Core.TreasureManager>.HasInstance)
            {
                float critChance = Core.TreasureManager.Instance.CritChance;
                if (critChance > 0f && Random.value < critChance)
                    damage *= 2f;
            }

            // Lightning Tower: 체인 공격 (투사체 없이 즉시 데미지)
            int chainCount = data.GetChainCount(Level);
            if (chainCount > 0)
            {
                ChainAttack(damage, chainCount);
                return;
            }

            if (data.projectilePrefab != null)
            {
                var spawnPos = firePoint != null ? firePoint : transform;
                var go = Poolable.TryGetPoolable(data.projectilePrefab, null);
                go.transform.position = spawnPos.position;

                var proj = go.GetComponent<Projectile.Projectile>();
                if (proj != null)
                {
                    proj.Initialize(_currentTarget, damage);

                    // Cannon Tower: AoE 폭발
                    float expRadius = data.GetExplosionRadius(Level);
                    if (expRadius > 0f)
                        proj.SetExplosion(expRadius);

                    // Ice Tower: 감속 디버프
                    float slowRate = data.GetSlowRate(Level);
                    float slowDur = data.GetSlowDuration(Level);
                    // 보물 SlowEffect 보너스 적용
                    if (Singleton<Core.TreasureManager>.HasInstance)
                        slowRate += Core.TreasureManager.Instance.SlowEffectBonus;
                    if (slowRate > 0f && slowDur > 0f)
                        proj.SetSlow(slowRate, slowDur);
                }
            }
            else
            {
                _currentTarget.TakeDamage(damage);
            }
        }

        void ChainAttack(float baseDamage, int maxChains)
        {
            float decay = data.GetChainDamageDecay(Level);
            float chainRange = data.chainRange;
            var hitNodes = new List<Node.Node> { _currentTarget };

            // VFX 경로 수집: 타워(발사 위치) → 첫 번째 타겟 → 체인 타겟들
            var spawnPos = firePoint != null ? firePoint : transform;
            var chainPath = new List<Vector3> { spawnPos.position };

            // 첫 번째 타격
            _currentTarget.TakeDamage(baseDamage);
            chainPath.Add(_currentTarget.transform.position);

            float currentDamage = baseDamage;
            Node.Node lastHit = _currentTarget;

            for (int i = 0; i < maxChains; i++)
            {
                currentDamage *= (1f - decay);

                // 체인 범위 내 가장 가까운 미타격 적 탐색
                var colliders = Physics2D.OverlapCircleAll(lastHit.transform.position, chainRange);
                Node.Node nextTarget = null;
                float minDist = float.MaxValue;

                foreach (var col in colliders)
                {
                    var node = col.GetComponent<Node.Node>();
                    if (node != null && node.IsAlive && !hitNodes.Contains(node))
                    {
                        float dist = Vector2.Distance(lastHit.transform.position, node.transform.position);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            nextTarget = node;
                        }
                    }
                }

                if (nextTarget == null) break;

                nextTarget.TakeDamage(currentDamage);
                hitNodes.Add(nextTarget);
                chainPath.Add(nextTarget.transform.position);
                lastHit = nextTarget;
            }

            // 체인 번개 VFX 생성 (경로가 2개 이상일 때만)
            if (chainPath.Count >= 2)
                SpawnChainLightningVFX(chainPath);
        }

        /// <summary>
        /// 체인 번개 VFX 오브젝트를 생성합니다.
        /// 프로토타입 단계이므로 Instantiate + 자동 Destroy 방식 사용.
        /// </summary>
        void SpawnChainLightningVFX(List<Vector3> chainPath)
        {
            var vfxGo = new GameObject("ChainLightningVFX");
            var vfx = vfxGo.AddComponent<ChainLightningVFX>();
            vfx.Play(chainPath);
        }

        public bool TryMerge(Tower other)
        {
            if (other.data.type != data.type) return false;
            if (other.Level != Level) return false; // 같은 레벨끼리만 합성
            if (Level >= data.MaxLevel) return false; // TowerData 기반 동적 레벨 캡
            Level++;
            Destroy(other.gameObject);
            return true;
        }

        /// <summary>
        /// 드래그 합성 시 사용: 레벨 1 증가 (소스 타워는 인벤토리에서 별도 제거).
        /// </summary>
        public void LevelUp()
        {
            if (data != null && Level < data.MaxLevel)
                Level++;
        }

        /// <summary>
        /// Soul을 소모하여 타워를 업그레이드합니다.
        /// 성공 시 true 반환, Soul 부족 또는 최대 레벨이면 false 반환.
        /// </summary>
        public bool UpgradeWithSoul()
        {
            if (data == null) return false;
            int cost = data.GetUpgradeCost(Level);
            if (cost < 0) return false; // 최대 레벨

            // 보물 TowerCostDiscount 효과 적용
            if (Singleton<Core.TreasureManager>.HasInstance)
            {
                float discount = Core.TreasureManager.Instance.TowerCostDiscount;
                if (discount > 0f)
                    cost = Mathf.Max(1, Mathf.RoundToInt(cost * (1f - discount)));
            }

            if (!Singleton<Core.RunManager>.HasInstance) return false;
            if (Core.RunManager.Instance.SoulEarned < cost) return false;

            Core.RunManager.Instance.SpendSoul(cost);
            Level++;
            Debug.Log($"[Tower] 업그레이드 완료: {data.towerName} Lv{Level}, 비용 {cost} Soul");
            return true;
        }

        /// <summary>
        /// 타워를 판매합니다. 배치 비용의 70%를 Soul로 환급합니다.
        /// TowerManager.SellTower()를 통해 호출하세요.
        /// </summary>
        public int GetSellValue()
        {
            if (data == null) return 0;
            return Mathf.RoundToInt(data.placeCost * 0.7f);
        }

        void OnDrawGizmosSelected()
        {
            if (data == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, data.GetRange(Level));
        }
    }
}
