using System.Collections;
using Tesseract.ObjectPool;
using UnityEngine;
using Soulspire.Audio;

namespace Soulspire.Node
{
    public class Node : Poolable
    {
        [Header("런타임 상태")]
        public Data.NodeData Data { get; private set; }
        public float CurrentHp { get; private set; }
        public bool IsAlive => CurrentHp > 0f;

        float _scaledHp;
        float _scaledSpeed;
        int _scaledSoulDrop;
        int _defense;

        // 감속 디버프
        float _slowRate;       // 0~1 (0.25 = 25% 감속)
        float _slowTimer;      // 남은 감속 시간

        // 보스 능력: Enrage
        bool _enraged;

        // 보스 능력: ArmorRegen
        const float ARMOR_REGEN_INTERVAL = 10f;
        const int ARMOR_REGEN_AMOUNT = 5;
        const int ARMOR_REGEN_MAX = 25;
        float _armorRegenTimer;
        int _baseDefense; // 원래 방어력 (리젠 기준)

        Transform[] _waypoints;
        int _waypointIndex;

        public void Initialize(Data.NodeData data, Transform[] waypoints, float hpMul, float speedMul, float soulMul)
        {
            Data = data;
            _waypoints = waypoints;
            _waypointIndex = 0;

            _scaledHp = data.hp * hpMul;
            _scaledSpeed = data.speed * speedMul;
            _scaledSoulDrop = Mathf.RoundToInt(data.soulDrop * soulMul);
            _defense = data.defense;
            _baseDefense = data.defense;

            CurrentHp = _scaledHp;
            _slowRate = 0f;
            _slowTimer = 0f;
            _enraged = false;
            _armorRegenTimer = 0f;
            IsUsing = true;
            transform.position = _waypoints[0].position;
        }

        void Update()
        {
            if (!IsAlive || !IsUsing) return;

            // 감속 타이머 갱신
            if (_slowTimer > 0f)
                _slowTimer -= Time.deltaTime;

            // 보스 능력: ArmorRegen - 10초마다 defense +5 (최대 25)
            if (Data != null && Data.bossAbilityType == Soulspire.Data.BossAbilityType.ArmorRegen)
            {
                _armorRegenTimer += Time.deltaTime;
                if (_armorRegenTimer >= ARMOR_REGEN_INTERVAL)
                {
                    _armorRegenTimer = 0f;
                    if (_defense < ARMOR_REGEN_MAX)
                    {
                        _defense = Mathf.Min(_defense + ARMOR_REGEN_AMOUNT, ARMOR_REGEN_MAX);
                        Debug.Log($"[Node] ArmorRegen: defense={_defense}");
                    }
                }
            }

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

            // 보스 능력: Enrage - HP 50% 이하 시 속도 2배
            if (!_enraged && Data != null && Data.bossAbilityType == Soulspire.Data.BossAbilityType.Enrage)
            {
                if (CurrentHp <= _scaledHp * 0.5f)
                {
                    _enraged = true;
                    Debug.Log($"[Node] Enrage 발동! 속도 2배");
                }
            }
            if (_enraged)
                speed *= 2f;

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

            // 보스 HP 바 실시간 업데이트
            if (Data != null && Data.type == Soulspire.Data.NodeType.Boss)
            {
                var inGameUI = Object.FindFirstObjectByType<UI.InGameUI>(FindObjectsInactive.Include);
                if (inGameUI != null)
                    inGameUI.UpdateBossHp(CurrentHp);
            }

            if (CurrentHp <= 0f)
                Die();
        }

        /// <summary>
        /// AoE 피격 시 호출. ArmorRegen 보스의 방어력 리젠 타이머를 리셋합니다.
        /// </summary>
        public void TakeDamageAoE(float damage)
        {
            if (!IsAlive) return;

            // ArmorRegen 보스: AoE 피격 시 방어력 리젠 리셋
            if (Data != null && Data.bossAbilityType == Soulspire.Data.BossAbilityType.ArmorRegen)
            {
                _defense = _baseDefense;
                _armorRegenTimer = 0f;
                Debug.Log($"[Node] ArmorRegen 리셋: defense={_defense}");
            }

            TakeDamage(damage);
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

            // 보스 처치 슬로우모션 연출
            bool isBoss = Data != null && Data.type == Soulspire.Data.NodeType.Boss;
            if (isBoss)
            {
                Time.timeScale = 0.2f;
                // 코루틴 대신 정적 헬퍼 사용 (풀링 후에도 동작하도록)
                var helper = new GameObject("[BossSlowMo]").AddComponent<BossSlowMotionHelper>();
                helper.StartRestore(1f); // 실시간 1초 후 복귀
            }

            SoundManager.Instance.PlaySfx(SoundKeys.NodeDie, 0.85f);
            if (Tesseract.Core.Singleton<Core.RunManager>.HasInstance)
            {
                int soulDrop = _scaledSoulDrop;
                // 보물 SoulBonus 효과 적용
                if (Tesseract.Core.Singleton<Core.TreasureManager>.HasInstance)
                    soulDrop += Core.TreasureManager.Instance.SoulBonusPerKill;
                Core.RunManager.Instance.AddSoul(soulDrop);
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

    /// <summary>
    /// 보스 처치 슬로우모션 복귀를 위한 일회용 헬퍼.
    /// Node가 풀에 반환된 후에도 timeScale 복귀가 보장됩니다.
    /// </summary>
    public class BossSlowMotionHelper : MonoBehaviour
    {
        public void StartRestore(float realTimeDelay)
        {
            StartCoroutine(RestoreTimeScale(realTimeDelay));
        }

        IEnumerator RestoreTimeScale(float realTimeDelay)
        {
            yield return new WaitForSecondsRealtime(realTimeDelay);
            Time.timeScale = 1f;
            Destroy(gameObject);
        }
    }
}
