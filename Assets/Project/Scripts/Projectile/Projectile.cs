using Tesseract.ObjectPool;
using UnityEngine;
using Soulspire.Audio;

namespace Soulspire.Projectile
{
    public class Projectile : Poolable
    {
        [Header("속도")]
        public float speed = 10f;

        [Header("최대 수명")]
        public float lifetime = 3f;

        Node.Node _target;
        float _damage;
        float _timer;

        // AoE (Cannon) 지원
        float _explosionRadius;

        // 감속 (Ice) 지원
        float _slowRate;
        float _slowDuration;

        public void Initialize(Node.Node target, float damage)
        {
            _target = target;
            _damage = damage;
            _timer = 0f;
            _explosionRadius = 0f;
            _slowRate = 0f;
            _slowDuration = 0f;
        }

        /// <summary>
        /// AoE 폭발 범위를 설정합니다 (Cannon Tower용).
        /// </summary>
        public void SetExplosion(float radius)
        {
            _explosionRadius = radius;
        }

        /// <summary>
        /// 감속 디버프를 설정합니다 (Ice Tower용).
        /// </summary>
        public void SetSlow(float rate, float duration)
        {
            _slowRate = rate;
            _slowDuration = duration;
        }

        void Update()
        {
            if (!IsUsing) return;

            _timer += Time.deltaTime;
            if (_timer >= lifetime)
            {
                Poolable.TryPool(gameObject);
                return;
            }

            if (_target == null || !_target.IsAlive)
            {
                Poolable.TryPool(gameObject);
                return;
            }

            var dir = (_target.transform.position - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;

            if (Vector2.Distance(transform.position, _target.transform.position) < 0.2f)
                Hit();
        }

        void Hit()
        {
            SoundManager.Instance.PlaySfx(SoundKeys.ProjectileHit, 0.8f);

            if (_explosionRadius > 0f)
            {
                // AoE: 폭발 범위 내 모든 Node에 데미지 (ArmorRegen 보스 방어력 리셋 포함)
                var hits = Physics2D.OverlapCircleAll(transform.position, _explosionRadius);
                foreach (var col in hits)
                {
                    var node = col.GetComponent<Node.Node>();
                    if (node != null && node.IsAlive)
                        node.TakeDamageAoE(_damage);
                }
            }
            else
            {
                // 단일 타겟 데미지
                _target.TakeDamage(_damage);
            }

            // 감속 디버프 적용
            if (_slowRate > 0f && _slowDuration > 0f)
            {
                if (_target != null && _target.IsAlive)
                    _target.ApplySlow(_slowRate, _slowDuration);
            }

            Poolable.TryPool(gameObject);
        }

        public override void OnPop()
        {
            base.OnPop();
            _timer = 0f;
        }
    }
}
