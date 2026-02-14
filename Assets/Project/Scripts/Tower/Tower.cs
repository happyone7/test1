using Tesseract.Core;
using Tesseract.ObjectPool;
using UnityEngine;
using Nodebreaker.Audio;

namespace Nodebreaker.Tower
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

            if (_currentTarget != null && _attackTimer >= 1f / attackSpeed)
            {
                Attack();
                _attackTimer = 0f;
            }
        }

        Node.Node FindTarget()
        {
            float range = data.GetRange(Level);
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
                    if (slowRate > 0f && slowDur > 0f)
                        proj.SetSlow(slowRate, slowDur);
                }
            }
            else
            {
                _currentTarget.TakeDamage(damage);
            }
        }

        public bool TryMerge(Tower other)
        {
            if (other.data.type != data.type) return false;
            if (other.Level != Level) return false; // 같은 레벨끼리만 합성
            if (Level >= 4) return false; // 최대 Lv4
            Level++;
            Destroy(other.gameObject);
            return true;
        }

        /// <summary>
        /// 드래그 합성 시 사용: 레벨 1 증가 (소스 타워는 인벤토리에서 별도 제거).
        /// </summary>
        public void LevelUp()
        {
            if (Level < 4)
                Level++;
        }

        void OnDrawGizmosSelected()
        {
            if (data == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, data.GetRange(Level));
        }
    }
}
