using Tesseract.ObjectPool;
using UnityEngine;
using Nodebreaker.Audio;

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
        int _defense;

        // 감속 디버프
        float _slowRate;       // 0~1 (0.25 = 25% 감속)
        float _slowTimer;      // 남은 감속 시간

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
            _defense = data.defense;

            CurrentHp = _scaledHp;
            _slowRate = 0f;
            _slowTimer = 0f;
            IsUsing = true;
            transform.position = _waypoints[0].position;
        }

        void Update()
        {
            if (!IsAlive || !IsUsing) return;

            // 감속 타이머 갱신
            if (_slowTimer > 0f)
                _slowTimer -= Time.deltaTime;

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
            float speed = _scaledSpeed;
            if (_slowTimer > 0f)
                speed *= (1f - _slowRate);
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.1f)
                _waypointIndex++;
        }

        public void TakeDamage(float damage)
        {
            if (!IsAlive) return;
            // defense 차감: 최소 데미지 1 보장
            float actualDamage = Mathf.Max(1f, damage - _defense);
            CurrentHp -= actualDamage;
            if (CurrentHp <= 0f)
                Die();
        }

        /// <summary>
        /// 감속 디버프 적용. 더 강한 감속률이면 갱신, 아니면 지속시간만 연장.
        /// </summary>
        public void ApplySlow(float rate, float duration)
        {
            if (rate > _slowRate)
            {
                _slowRate = Mathf.Clamp01(rate);
                _slowTimer = duration;
            }
            else if (Mathf.Approximately(rate, _slowRate))
            {
                // 같은 감속률이면 지속시간만 갱신 (더 긴 쪽)
                _slowTimer = Mathf.Max(_slowTimer, duration);
            }
        }

        void Die()
        {
            CurrentHp = 0f;
            SoundManager.Instance.PlaySfx(SoundKeys.NodeDie, 0.85f);
            if (Tesseract.Core.Singleton<Core.RunManager>.HasInstance)
            {
                int bitDrop = _scaledBitDrop;
                float bitMul = Core.RunManager.Instance.CurrentModifiers.bitGainMultiplier;
                if (bitMul > 0f)
                    bitDrop = Mathf.RoundToInt(bitDrop * bitMul);
                Core.RunManager.Instance.AddBit(bitDrop);
                Core.RunManager.Instance.OnNodeKilled();
            }
            RemoveFromWave();
            Poolable.TryPool(gameObject);
        }

        void ReachEnd()
        {
            Debug.Log($"[Node] ReachEnd: damage={Data.damage}, RunManager.HasInstance={Tesseract.Core.Singleton<Core.RunManager>.HasInstance}");
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
