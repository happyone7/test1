using Tesseract.ObjectPool;
using UnityEngine;

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
        }

        void Update()
        {
            if (data == null) return;

            _attackTimer += Time.deltaTime;

            if (_currentTarget == null || !_currentTarget.IsAlive)
                _currentTarget = FindTarget();

            if (_currentTarget != null && _attackTimer >= 1f / data.GetAttackSpeed(Level))
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
            if (data.projectilePrefab != null)
            {
                var spawnPos = firePoint != null ? firePoint : transform;
                var go = Poolable.TryGetPoolable(data.projectilePrefab, null);
                go.transform.position = spawnPos.position;

                var proj = go.GetComponent<Projectile.Projectile>();
                if (proj != null)
                    proj.Initialize(_currentTarget, data.GetDamage(Level));
            }
            else
            {
                // 투사체 없이 즉시 타격
                _currentTarget.TakeDamage(data.GetDamage(Level));
            }
        }

        public bool TryMerge(Tower other)
        {
            if (other.data.type != data.type) return false;
            if (Level >= 5) return false;
            Level++;
            Destroy(other.gameObject);
            return true;
        }

        void OnDrawGizmosSelected()
        {
            if (data == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, data.GetRange(Level));
        }
    }
}
