using Tesseract.ObjectPool;
using UnityEngine;

namespace Nodebreaker.Node
{
    public class Node : Poolable
    {
        [Header("런타임 상태")]
        public Data.NodeData Data { get; private set; }
        public float CurrentHp { get; private set; }
        public bool IsAlive => CurrentHp > 0f;

        float _scaledHp;
        float _scaledSpeed;
        int _scaledBitDrop;

        Transform[] _waypoints;
        int _waypointIndex;

        public void Initialize(Data.NodeData data, Transform[] waypoints, float hpMul, float speedMul, float bitMul)
        {
            Data = data;
            _waypoints = waypoints;
            _waypointIndex = 0;

            _scaledHp = data.hp * hpMul;
            _scaledSpeed = data.speed * speedMul;
            _scaledBitDrop = Mathf.RoundToInt(data.bitDrop * bitMul);

            CurrentHp = _scaledHp;
            transform.position = _waypoints[0].position;
        }

        void Update()
        {
            if (!IsAlive || !IsUsing) return;
            Move();
        }

        void Move()
        {
            if (_waypointIndex >= _waypoints.Length)
            {
                ReachEnd();
                return;
            }

            var target = _waypoints[_waypointIndex].position;
            transform.position = Vector3.MoveTowards(transform.position, target, _scaledSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.1f)
                _waypointIndex++;
        }

        public void TakeDamage(float damage)
        {
            if (!IsAlive) return;
            CurrentHp -= damage;
            if (CurrentHp <= 0f)
                Die();
        }

        void Die()
        {
            CurrentHp = 0f;
            if (Tesseract.Core.Singleton<Core.RunManager>.HasInstance)
                Core.RunManager.Instance.AddBit(_scaledBitDrop);
            RemoveFromWave();
            Poolable.TryPool(gameObject);
        }

        void ReachEnd()
        {
            if (Tesseract.Core.Singleton<Core.RunManager>.HasInstance)
                Core.RunManager.Instance.TakeDamage(Data.damage);
            RemoveFromWave();
            Poolable.TryPool(gameObject);
        }

        void RemoveFromWave()
        {
            if (Tesseract.Core.Singleton<WaveSpawner>.HasInstance)
                WaveSpawner.Instance.OnNodeRemoved();
        }

        public override void OnPop()
        {
            base.OnPop();
            _waypointIndex = 0;
        }
    }
}
