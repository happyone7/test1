using Tesseract.ObjectPool;
using UnityEngine;
using Nodebreaker.Audio;

namespace Nodebreaker.Projectile
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

        public void Initialize(Node.Node target, float damage)
        {
            _target = target;
            _damage = damage;
            _timer = 0f;
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
            _target.TakeDamage(_damage);
            Poolable.TryPool(gameObject);
        }

        public override void OnPop()
        {
            base.OnPop();
            _timer = 0f;
        }
    }
}
