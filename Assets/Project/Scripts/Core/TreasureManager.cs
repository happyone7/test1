using System;
using System.Collections.Generic;
using Tesseract.Core;
using UnityEngine;
using Soulspire.Data;

namespace Soulspire.Core
{
    /// <summary>
    /// 보물상자 시스템 매니저.
    /// 보스 처치 시 보물상자 획득 → Sanctum에서 오픈 → 타워 3택 → 인벤토리 추가.
    /// 기존 버프 효과 시스템도 API 호환을 위해 유지.
    /// </summary>
    public class TreasureManager : Singleton<TreasureManager>
    {
        [Header("보물 설정")]
        public TreasureConfig treasureConfig;

        [Header("타워 보물상자 설정")]
        [Tooltip("보물상자 오픈 시 제시할 타워 후보 수")]
        public int towerChoiceCount = 3;

        // ── UI 이벤트 ──
        /// <summary>
        /// 보물상자가 드랍되었을 때 발행. UI가 구독하여 3택 패널을 표시한다.
        /// 파라미터: 선택 가능한 TreasureChoiceData 리스트 (choicesPerDrop개)
        /// </summary>
        public event Action<List<TreasureChoiceData>> OnTreasureDropped;

        /// <summary>
        /// 플레이어가 보물 선택을 완료했을 때 발행. UI가 구독하여 패널을 닫는다.
        /// 파라미터: 선택된 TreasureChoiceData
        /// </summary>
        public event Action<TreasureChoiceData> OnTreasureChosen;

        // ── UI 이벤트 (타워 보물상자) ──

        /// <summary>
        /// 인게임 중 보물상자를 획득했을 때 발행.
        /// UI가 구독하여 "보물상자 +1" 알림을 표시한다.
        /// </summary>
        public event Action<int> OnTreasureChestAcquired;

        /// <summary>
        /// Sanctum에서 보물상자를 열어 타워 후보가 생성되었을 때 발행.
        /// UI가 구독하여 타워 3택 패널을 표시한다.
        /// </summary>
        public event Action<List<TowerData>> OnTowerTreasureDropped;

        /// <summary>
        /// 플레이어가 타워를 선택했을 때 발행.
        /// </summary>
        public event Action<TowerData> OnTowerTreasureChosen;

        // ── 현재 런의 활성 보물 효과 ──
        readonly List<TreasureChoiceData> _activeEffects = new List<TreasureChoiceData>();

        /// <summary>현재 런에서 선택된 모든 보물 효과 (읽기 전용).</summary>
        public IReadOnlyList<TreasureChoiceData> ActiveEffects => _activeEffects;

        // ── 보물 효과 배율 캐시 (Tower, RunManager 등에서 참조) ──
        float _damageMultiplier = 1f;
        float _attackSpeedMultiplier = 1f;
        float _rangeMultiplier = 1f;
        float _towerCostDiscount = 0f;
        float _critChance = 0f;
        float _slowEffectBonus = 0f;
        int _bonusMaxHp = 0;
        int _soulBonusPerKill = 0;

        public float DamageMultiplier => _damageMultiplier;
        public float AttackSpeedMultiplier => _attackSpeedMultiplier;
        public float RangeMultiplier => _rangeMultiplier;
        public float TowerCostDiscount => _towerCostDiscount;
        public float CritChance => _critChance;
        public float SlowEffectBonus => _slowEffectBonus;
        public int BonusMaxHp => _bonusMaxHp;
        public int SoulBonusPerKill => _soulBonusPerKill;

        // ── 대기 상태 (UI가 3택을 표시 중인지) ──
        bool _waitingForChoice;
        public bool IsWaitingForChoice => _waitingForChoice;

        // ═══════════════════════════════════════
        // 타워 보물상자 API
        // ═══════════════════════════════════════

        // ── 현재 오픈 중인 보물상자 후보 (UI가 표시 중) ──
        List<TowerData> _currentChoices;

        /// <summary>현재 3택으로 제시된 타워 후보 (읽기 전용). 선택 대기 중이 아니면 null.</summary>
        public IReadOnlyList<TowerData> CurrentChoices => _currentChoices;

        /// <summary>
        /// 보스 처치 시 WaveSpawner에서 호출.
        /// 보물상자 보유 수를 +1 증가시키고 저장한다.
        /// GDD 3.5.2: 보물상자는 인게임 중에는 열 수 없으며, Sanctum에서만 오픈 가능.
        /// </summary>
        public void OnBossKilled()
        {
            if (!Singleton<MetaManager>.HasInstance)
            {
                Debug.LogWarning("[TreasureManager] OnBossKilled: MetaManager 인스턴스 없음");
                return;
            }

            MetaManager.Instance.AddTreasureChest(1);
            int chestCount = MetaManager.Instance.TreasureChestCount;

            Debug.Log($"[TreasureManager] 보스 처치! 보물상자 +1 (보유: {chestCount})");
            OnTreasureChestAcquired?.Invoke(chestCount);
        }

        /// <summary>
        /// Sanctum에서 보물상자를 오픈한다.
        /// 보물상자 보유 수가 0이면 false를 반환하고 아무 일도 하지 않는다.
        /// 성공 시 해금된 타워 풀에서 towerChoiceCount개의 후보를 뽑아
        /// OnTowerTreasureDropped 이벤트를 발행한다.
        /// </summary>
        /// <returns>보물상자를 소비하고 후보를 제시했으면 true, 실패하면 false.</returns>
        public bool OpenTreasureChest()
        {
            if (!Singleton<MetaManager>.HasInstance)
            {
                Debug.LogWarning("[TreasureManager] OpenTreasureChest: MetaManager 인스턴스 없음");
                return false;
            }

            if (MetaManager.Instance.TreasureChestCount <= 0)
            {
                Debug.Log("[TreasureManager] OpenTreasureChest: 보물상자 없음");
                return false;
            }

            var choices = GetTreasureChoices(towerChoiceCount);
            if (choices == null || choices.Count == 0)
            {
                Debug.LogWarning("[TreasureManager] OpenTreasureChest: 타워 후보를 생성할 수 없음");
                return false;
            }

            // 보물상자 1개 소비
            MetaManager.Instance.SpendTreasureChest();

            _currentChoices = choices;
            _waitingForChoice = true;

            Debug.Log($"[TreasureManager] 보물상자 오픈! 타워 후보 {choices.Count}개 (남은 상자: {MetaManager.Instance.TreasureChestCount})");
            OnTowerTreasureDropped?.Invoke(choices);
            return true;
        }

        /// <summary>
        /// 해금된 타워 풀에서 count개의 타워 후보를 랜덤 추출한다.
        /// 보물상자를 소비하지 않으며 이벤트도 발행하지 않는 순수 데이터 메서드.
        /// GDD 3.5.3: 같은 종류의 타워가 중복으로 등장할 수 있음.
        /// </summary>
        public List<TowerData> GetTreasureChoices(int count = 3)
        {
            if (!Singleton<Tower.TowerManager>.HasInstance)
            {
                Debug.LogWarning("[TreasureManager] GetTreasureChoices: TowerManager 인스턴스 없음");
                return new List<TowerData>();
            }

            var available = Tower.TowerManager.Instance.availableTowers;
            if (available == null || available.Length == 0)
            {
                Debug.LogWarning("[TreasureManager] GetTreasureChoices: 사용 가능한 타워가 없음");
                return new List<TowerData>();
            }

            return PickRandomTowers(count, available);
        }

        /// <summary>
        /// 플레이어가 타워 3택 중 하나를 선택했을 때 UI에서 호출.
        /// 선택된 타워를 인벤토리에 추가한다.
        /// </summary>
        public void ConfirmTreasureChoice(TowerData towerData)
        {
            if (towerData == null)
            {
                Debug.LogWarning("[TreasureManager] ConfirmTreasureChoice: null towerData");
                _waitingForChoice = false;
                _currentChoices = null;
                return;
            }

            _waitingForChoice = false;
            _currentChoices = null;

            // FTUE: 보물상자 첫 획득 가이드
            if (Singleton<UI.FTUEManager>.HasInstance)
                UI.FTUEManager.Instance.TriggerInGame("FirstTreasure", UI.FTUEManager.GuideFirstTreasure);

            // 인벤토리에 타워 추가
            if (Singleton<Tower.TowerInventory>.HasInstance)
            {
                bool added = Tower.TowerInventory.Instance.TryAddTower(towerData, 1);
                if (added)
                    Debug.Log($"[TreasureManager] 타워 획득: {towerData.towerName} Lv1 인벤토리 추가");
                else
                    Debug.LogWarning($"[TreasureManager] 인벤토리 가득 참 — {towerData.towerName} 획득 실패");
            }
            else
            {
                Debug.LogWarning("[TreasureManager] ConfirmTreasureChoice: TowerInventory 인스턴스 없음");
            }

            OnTowerTreasureChosen?.Invoke(towerData);
        }

        /// <summary>
        /// 인덱스(0~2)로 타워를 선택한다. UI에서 버튼 인덱스를 전달할 때 사용.
        /// CurrentChoices가 null이거나 인덱스가 범위 밖이면 무시한다.
        /// </summary>
        public void ConfirmTreasureChoiceByIndex(int index)
        {
            if (_currentChoices == null || index < 0 || index >= _currentChoices.Count)
            {
                Debug.LogWarning($"[TreasureManager] ConfirmTreasureChoiceByIndex: 유효하지 않은 인덱스 {index}");
                return;
            }

            ConfirmTreasureChoice(_currentChoices[index]);
        }

        /// <summary>
        /// [하위 호환] 기존 ApplyTowerChoice 호출을 ConfirmTreasureChoice로 전달.
        /// </summary>
        public void ApplyTowerChoice(TowerData towerData)
        {
            ConfirmTreasureChoice(towerData);
        }

        /// <summary>
        /// availableTowers에서 해금된 타워만 대상으로 count개를 랜덤 추출한다.
        /// GDD 3.5.3: 같은 종류의 타워가 중복 등장 가능 (Arrow 2 + Cannon 1 등).
        /// </summary>
        List<TowerData> PickRandomTowers(int count, TowerData[] available)
        {
            // 해금된 타워만 필터링
            var unlocked = new List<TowerData>();
            bool hasMetaManager = Singleton<MetaManager>.HasInstance;
            foreach (var tower in available)
            {
                if (tower == null) continue;
                if (hasMetaManager && !MetaManager.Instance.IsTowerUnlocked(tower.towerId))
                    continue;
                unlocked.Add(tower);
            }

            var result = new List<TowerData>();
            if (unlocked.Count == 0) return result;

            // GDD 3.5.3: 같은 종류 중복 허용 — 단순 랜덤 추출
            for (int i = 0; i < count; i++)
            {
                int idx = UnityEngine.Random.Range(0, unlocked.Count);
                result.Add(unlocked[idx]);
            }

            return result;
        }

        // ═══════════════════════════════════════
        // 기존 버프 보물 API (호환용 유지)
        // ═══════════════════════════════════════

        /// <summary>
        /// 웨이브 클리어 시 호출 (현재 비활성 — Phase 2에서 보스 처치 트리거로 대체).
        /// WaveSpawner에서 더 이상 호출하지 않지만 API 호환을 위해 유지.
        /// </summary>
        public void OnWaveCleared()
        {
            if (treasureConfig == null)
            {
                Debug.LogWarning("[TreasureManager] TreasureConfig가 할당되지 않음");
                return;
            }

            float roll = UnityEngine.Random.value;
            if (roll > treasureConfig.dropChance)
            {
                Debug.Log($"[TreasureManager] 보물상자 드랍 실패 (roll={roll:F2}, chance={treasureConfig.dropChance:F2})");
                return;
            }

            var choices = PickRandomChoices(treasureConfig.choicesPerDrop);
            if (choices.Count == 0)
            {
                Debug.LogWarning("[TreasureManager] 선택 가능한 보물이 없음");
                return;
            }

            Debug.Log($"[TreasureManager] 보물상자 드랍! 선택지 {choices.Count}개");
            _waitingForChoice = true;
            OnTreasureDropped?.Invoke(choices);
        }

        /// <summary>
        /// 플레이어가 3택 중 하나를 선택했을 때 UI에서 호출.
        /// </summary>
        public void ApplyChoice(TreasureChoiceData choice)
        {
            if (choice == null)
            {
                Debug.LogWarning("[TreasureManager] ApplyChoice: null choice");
                _waitingForChoice = false;
                return;
            }

            _activeEffects.Add(choice);
            ApplyEffect(choice);
            _waitingForChoice = false;

            Debug.Log($"[TreasureManager] 보물 선택: {choice.displayName} ({choice.choiceType}, +{choice.value})");
            OnTreasureChosen?.Invoke(choice);
        }

        /// <summary>
        /// 런 종료 시 모든 보물 효과를 초기화한다.
        /// RunManager.ResetRun()에서 호출.
        /// </summary>
        public void ResetEffects()
        {
            _activeEffects.Clear();
            _damageMultiplier = 1f;
            _attackSpeedMultiplier = 1f;
            _rangeMultiplier = 1f;
            _towerCostDiscount = 0f;
            _critChance = 0f;
            _slowEffectBonus = 0f;
            _bonusMaxHp = 0;
            _soulBonusPerKill = 0;
            _waitingForChoice = false;

            Debug.Log("[TreasureManager] 보물 효과 초기화 완료");
        }

        // ── 내부 로직 ──

        /// <summary>
        /// rarityWeights 기반으로 중복 없이 N개의 선택지를 추출한다.
        /// </summary>
        List<TreasureChoiceData> PickRandomChoices(int count)
        {
            var result = new List<TreasureChoiceData>();
            if (treasureConfig.allChoices == null || treasureConfig.allChoices.Length == 0)
                return result;

            // 가중치 맵 구성
            float totalWeight = treasureConfig.commonWeight + treasureConfig.rareWeight + treasureConfig.epicWeight;
            if (totalWeight <= 0f) totalWeight = 1f;

            // 사용 가능한 선택지를 희귀도별로 분류
            var byRarity = new Dictionary<TreasureRarity, List<TreasureChoiceData>>();
            byRarity[TreasureRarity.Common] = new List<TreasureChoiceData>();
            byRarity[TreasureRarity.Rare] = new List<TreasureChoiceData>();
            byRarity[TreasureRarity.Epic] = new List<TreasureChoiceData>();

            foreach (var choice in treasureConfig.allChoices)
            {
                if (choice == null) continue;
                byRarity[choice.rarity].Add(choice);
            }

            // 이미 선택된 인덱스 추적 (중복 방지)
            var usedIndices = new HashSet<int>();
            int maxAttempts = count * 10; // 무한루프 방지
            int attempts = 0;

            while (result.Count < count && attempts < maxAttempts)
            {
                attempts++;

                // 1단계: 희귀도 결정 (가중치 기반)
                TreasureRarity selectedRarity = RollRarity(totalWeight);

                // 해당 희귀도에 선택지가 없으면 다른 희귀도에서 폴백
                var pool = byRarity[selectedRarity];
                if (pool.Count == 0)
                {
                    // 폴백: 아무 희귀도에서든 가져옴
                    pool = GetAnyAvailablePool(byRarity);
                    if (pool == null || pool.Count == 0) break;
                }

                // 2단계: 해당 희귀도 풀에서 랜덤 선택 (중복 제외)
                int idx = UnityEngine.Random.Range(0, pool.Count);
                var candidate = pool[idx];

                // allChoices 배열 내 글로벌 인덱스로 중복 체크
                int globalIdx = System.Array.IndexOf(treasureConfig.allChoices, candidate);
                if (globalIdx >= 0 && !usedIndices.Contains(globalIdx))
                {
                    usedIndices.Add(globalIdx);
                    result.Add(candidate);
                }
            }

            return result;
        }

        TreasureRarity RollRarity(float totalWeight)
        {
            float roll = UnityEngine.Random.value * totalWeight;

            if (roll < treasureConfig.commonWeight)
                return TreasureRarity.Common;
            if (roll < treasureConfig.commonWeight + treasureConfig.rareWeight)
                return TreasureRarity.Rare;
            return TreasureRarity.Epic;
        }

        List<TreasureChoiceData> GetAnyAvailablePool(Dictionary<TreasureRarity, List<TreasureChoiceData>> byRarity)
        {
            // Common -> Rare -> Epic 순서로 폴백
            if (byRarity[TreasureRarity.Common].Count > 0) return byRarity[TreasureRarity.Common];
            if (byRarity[TreasureRarity.Rare].Count > 0) return byRarity[TreasureRarity.Rare];
            if (byRarity[TreasureRarity.Epic].Count > 0) return byRarity[TreasureRarity.Epic];
            return null;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // ── 디버그 전용 메서드 ──

        /// <summary>
        /// 보물상자를 1개 강제 추가합니다 (디버그용).
        /// </summary>
        public void Debug_AddChest()
        {
            OnBossKilled();
        }

        /// <summary>
        /// 보물상자를 즉시 오픈합니다 (디버그용).
        /// 보유 수가 0이면 먼저 1개를 추가한 후 오픈합니다.
        /// </summary>
        public void Debug_ForceDrop()
        {
            if (Singleton<MetaManager>.HasInstance && MetaManager.Instance.TreasureChestCount <= 0)
                MetaManager.Instance.AddTreasureChest(1);
            OpenTreasureChest();
        }
#endif

        void ApplyEffect(TreasureChoiceData choice)
        {
            switch (choice.choiceType)
            {
                case TreasureChoiceType.DamageBoost:
                    // value는 배율 증가분 (예: 0.15 = +15% 데미지)
                    _damageMultiplier += choice.value;
                    break;

                case TreasureChoiceType.AttackSpeedBoost:
                    // value는 배율 증가분 (예: 0.1 = +10% 공격속도)
                    _attackSpeedMultiplier += choice.value;
                    break;

                case TreasureChoiceType.RangeBoost:
                    // value는 배율 증가분 (예: 0.1 = +10% 사거리)
                    _rangeMultiplier += choice.value;
                    break;

                case TreasureChoiceType.SoulBonus:
                    // value는 노드 처치 시 추가 Soul (예: 2 = +2 Soul per kill)
                    _soulBonusPerKill += Mathf.RoundToInt(choice.value);
                    break;

                case TreasureChoiceType.MaxHpBoost:
                    // value는 추가 최대 HP (예: 20 = +20 HP)
                    _bonusMaxHp += Mathf.RoundToInt(choice.value);
                    // RunManager에 즉시 적용
                    if (Singleton<RunManager>.HasInstance)
                        RunManager.Instance.ApplyBonusMaxHp(Mathf.RoundToInt(choice.value));
                    break;

                case TreasureChoiceType.TowerCostDiscount:
                    // value는 할인 비율 (예: 0.1 = 10% 할인)
                    _towerCostDiscount += choice.value;
                    break;

                case TreasureChoiceType.CritChance:
                    // value는 크리티컬 확률 (예: 0.05 = 5% 크리티컬)
                    _critChance += choice.value;
                    break;

                case TreasureChoiceType.SlowEffect:
                    // value는 추가 감속률 (예: 0.1 = +10% 감속 효과 강화)
                    _slowEffectBonus += choice.value;
                    break;
                case TreasureChoiceType.TowerReward:
                    // 타워 보상: 실제 타워 배치는 이벤트 구독자가 수행
                    Debug.Log($"[TreasureManager] 타워 보상 선택: {choice.rewardTower?.towerName ?? "null"}");
                    break;

                default:
                    Debug.LogWarning($"[TreasureManager] 알 수 없는 choiceType: {choice.choiceType}");
                    break;
            }
        }
    }
}
