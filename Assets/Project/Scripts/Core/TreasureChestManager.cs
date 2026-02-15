using System;
using System.Collections.Generic;
using Tesseract.Core;
using UnityEngine;
using Nodebreaker.Audio;

namespace Nodebreaker.Core
{
    /// <summary>
    /// 보물상자 드랍 및 3택 선택 시스템을 관리한다.
    /// 몬스터 처치 시 확률 드랍, 웨이브 클리어/보스 처치 시 보장 드랍.
    /// 드랍 시 게임을 일시정지하고 3택 선택지를 표시한다.
    /// 선택된 보상은 현재 런 동안 유지된다.
    /// </summary>
    public class TreasureChestManager : Singleton<TreasureChestManager>
    {
        [Header("보상 풀")]
        public Data.TreasureRewardData[] rewardPool;

        [Header("드랍 확률")]
        [Range(0f, 1f)]
        public float normalKillDropRate = 0.03f;    // 일반 몬스터 처치 시 3%

        [Range(0f, 1f)]
        public float waveClearDropRate = 0.20f;      // 웨이브 클리어 시 20%

        [Range(0f, 1f)]
        public float bossKillDropRate = 1.0f;        // 보스 처치 시 100%

        /// <summary>
        /// 보물상자가 드랍되어 3택 선택 대기 중일 때 발생.
        /// UI팀장이 이 이벤트를 구독하여 3택 UI를 표시한다.
        /// </summary>
        public event Action<Data.TreasureRewardData[]> OnChestDropped;

        /// <summary>
        /// 보상 선택 완료 시 발생 (UI에서 선택 후 호출).
        /// </summary>
        public event Action<Data.TreasureRewardData> OnRewardSelected;

        // 현재 런에서 적용 중인 보물상자 보상 목록
        private List<Data.TreasureRewardData> _activeRewards = new List<Data.TreasureRewardData>();
        public IReadOnlyList<Data.TreasureRewardData> ActiveRewards => _activeRewards;

        private bool _waitingForSelection;
        public bool WaitingForSelection => _waitingForSelection;

        /// <summary>
        /// 런 시작 시 호출하여 보상 목록을 초기화한다.
        /// </summary>
        public void ResetForNewRun()
        {
            _activeRewards.Clear();
            _waitingForSelection = false;
        }

        /// <summary>
        /// 일반 몬스터 처치 시 호출. 확률에 따라 보물상자 드랍을 시도한다.
        /// </summary>
        public void TryDropOnNormalKill()
        {
            if (_waitingForSelection) return;
            if (UnityEngine.Random.value <= normalKillDropRate)
                DropChest();
        }

        /// <summary>
        /// 웨이브 클리어 시 호출. 확률에 따라 보물상자 드랍을 시도한다.
        /// </summary>
        public void TryDropOnWaveClear()
        {
            if (_waitingForSelection) return;
            if (UnityEngine.Random.value <= waveClearDropRate)
                DropChest();
        }

        /// <summary>
        /// 보스 처치 시 호출. 100% 보물상자 드랍.
        /// </summary>
        public void TryDropOnBossKill()
        {
            if (_waitingForSelection) return;
            if (UnityEngine.Random.value <= bossKillDropRate)
                DropChest();
        }

        /// <summary>
        /// 보물상자를 드랍한다. 3개의 랜덤 보상을 선택하여 이벤트를 발생시킨다.
        /// </summary>
        private void DropChest()
        {
            if (rewardPool == null || rewardPool.Length == 0)
            {
                Debug.LogWarning("[TreasureChestManager] 보상 풀이 비어있습니다.");
                return;
            }

            var choices = PickRandomRewards(3);
            if (choices.Length == 0) return;

            _waitingForSelection = true;

            // 게임 일시정지 (선택 대기)
            Time.timeScale = 0f;

            SoundManager.Instance.PlaySfx(SoundKeys.TreasureChestDrop, 1f);

            Debug.Log($"[TreasureChestManager] 보물상자 드랍! 선택지: {choices.Length}개");
            OnChestDropped?.Invoke(choices);
        }

        /// <summary>
        /// UI에서 보상을 선택했을 때 호출한다.
        /// </summary>
        public void SelectReward(Data.TreasureRewardData reward)
        {
            if (!_waitingForSelection) return;

            _waitingForSelection = false;
            _activeRewards.Add(reward);
            ApplyReward(reward);

            SoundManager.Instance.PlaySfx(SoundKeys.TreasureRewardSelect, 1f);

            Debug.Log($"[TreasureChestManager] 보상 선택: {reward.rewardName} ({reward.rewardType} +{reward.value})");
            OnRewardSelected?.Invoke(reward);

            // 게임 재개
            if (Singleton<RunManager>.HasInstance && RunManager.Instance.IsRunning)
            {
                // InGameUI의 현재 배속을 복원하는 것이 이상적이지만,
                // 간단하게 x1으로 복원
                Time.timeScale = 1f;
            }
        }

        /// <summary>
        /// 선택된 보상 효과를 런 모디파이어에 적용한다.
        /// </summary>
        private void ApplyReward(Data.TreasureRewardData reward)
        {
            if (!Singleton<RunManager>.HasInstance) return;
            var run = RunManager.Instance;

            switch (reward.rewardType)
            {
                case Data.TreasureRewardType.AttackDamageUp:
                    var mods1 = run.CurrentModifiers;
                    mods1.attackDamageMultiplier += reward.value * 0.01f;
                    run.SetModifiers(mods1);
                    break;

                case Data.TreasureRewardType.AttackSpeedUp:
                    var mods2 = run.CurrentModifiers;
                    mods2.attackSpeedMultiplier += reward.value * 0.01f;
                    run.SetModifiers(mods2);
                    break;

                case Data.TreasureRewardType.RangeUp:
                    var mods3 = run.CurrentModifiers;
                    mods3.rangeBonus += reward.value;
                    run.SetModifiers(mods3);
                    break;

                case Data.TreasureRewardType.BitGainUp:
                    var mods4 = run.CurrentModifiers;
                    mods4.bitGainMultiplier += reward.value * 0.01f;
                    run.SetModifiers(mods4);
                    break;

                case Data.TreasureRewardType.BaseHpRestore:
                    run.HealBase(Mathf.RoundToInt(reward.value));
                    break;

                case Data.TreasureRewardType.BaseMaxHpUp:
                    run.IncreaseMaxHp(Mathf.RoundToInt(reward.value));
                    break;

                case Data.TreasureRewardType.RandomTower:
                    GrantRandomTower();
                    break;

                case Data.TreasureRewardType.GlobalAttackSpeedBurst:
                    var mods5 = run.CurrentModifiers;
                    mods5.attackSpeedMultiplier += reward.value * 0.01f;
                    run.SetModifiers(mods5);
                    break;
            }
        }

        /// <summary>
        /// 랜덤 타워를 인벤토리에 추가한다.
        /// </summary>
        private void GrantRandomTower()
        {
            if (!Singleton<Tower.TowerManager>.HasInstance) return;
            if (!Singleton<Tower.TowerInventory>.HasInstance) return;

            var available = Tower.TowerManager.Instance.availableTowers;
            if (available == null || available.Length == 0) return;

            var inventory = Tower.TowerInventory.Instance;
            if (inventory.IsFull)
            {
                Debug.Log("[TreasureChestManager] 인벤토리 가득 참 - 타워 보상 불가");
                return;
            }

            var randomTower = available[UnityEngine.Random.Range(0, available.Length)];
            inventory.TryAddTower(randomTower, 1);
            Debug.Log($"[TreasureChestManager] 랜덤 타워 획득: {randomTower.towerName}");
        }

        /// <summary>
        /// 가중치 기반으로 N개의 중복 없는 보상을 선택한다.
        /// </summary>
        private Data.TreasureRewardData[] PickRandomRewards(int count)
        {
            if (rewardPool == null || rewardPool.Length == 0)
                return Array.Empty<Data.TreasureRewardData>();

            count = Mathf.Min(count, rewardPool.Length);
            var result = new List<Data.TreasureRewardData>();
            var pool = new List<Data.TreasureRewardData>(rewardPool);

            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                int totalWeight = 0;
                foreach (var r in pool)
                    totalWeight += Mathf.Max(1, r.weight);

                int roll = UnityEngine.Random.Range(0, totalWeight);
                int cumulative = 0;

                for (int j = 0; j < pool.Count; j++)
                {
                    cumulative += Mathf.Max(1, pool[j].weight);
                    if (roll < cumulative)
                    {
                        result.Add(pool[j]);
                        pool.RemoveAt(j);
                        break;
                    }
                }
            }

            return result.ToArray();
        }
    }
}
