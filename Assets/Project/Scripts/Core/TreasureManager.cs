using System;
using System.Collections.Generic;
using Tesseract.Core;
using UnityEngine;
using Soulspire.Data;

namespace Soulspire.Core
{
    /// <summary>
    /// 보물상자 시스템 매니저.
    /// Phase 2: 보스 처치 시 보물상자 드롭 → 타워 3택 → 인벤토리 추가.
    /// 기존 버프 효과 시스템도 API 호환을 위해 유지.
    /// </summary>
    public class TreasureManager : Singleton<TreasureManager>
    {
        [Header("보물 설정")]
        public TreasureConfig treasureConfig;

        [Header("타워 보물상자 설정")]
        [Tooltip("보스 처치 시 제시할 타워 후보 수")]
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

        // ── UI 이벤트 (타워 보물상자 - Phase 2) ──
        /// <summary>
        /// 보스 처치 시 타워 보물상자가 드롭되었을 때 발행.
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
        // 타워 보물상자 API (Phase 2)
        // ═══════════════════════════════════════

        /// <summary>
        /// 보스 처치 시 WaveSpawner에서 호출.
        /// TowerManager.availableTowers에서 랜덤으로 타워 후보를 뽑아 UI에 제시한다.
        /// </summary>
        public void OnBossKilled()
        {
            if (!Singleton<Tower.TowerManager>.HasInstance)
            {
                Debug.LogWarning("[TreasureManager] OnBossKilled: TowerManager 인스턴스 없음");
                return;
            }

            var available = Tower.TowerManager.Instance.availableTowers;
            if (available == null || available.Length == 0)
            {
                Debug.LogWarning("[TreasureManager] OnBossKilled: 사용 가능한 타워가 없음");
                return;
            }

            var candidates = PickRandomTowers(towerChoiceCount, available);
            if (candidates.Count == 0)
            {
                Debug.LogWarning("[TreasureManager] OnBossKilled: 타워 후보를 생성할 수 없음");
                return;
            }

            Debug.Log($"[TreasureManager] 보스 처치! 타워 보물상자 드롭 — 후보 {candidates.Count}개");
            _waitingForChoice = true;
            OnTowerTreasureDropped?.Invoke(candidates);
        }

        /// <summary>
        /// 플레이어가 타워 3택 중 하나를 선택했을 때 UI에서 호출.
        /// 선택된 타워를 인벤토리에 추가한다.
        /// </summary>
        public void ApplyTowerChoice(TowerData towerData)
        {
            if (towerData == null)
            {
                Debug.LogWarning("[TreasureManager] ApplyTowerChoice: null towerData");
                _waitingForChoice = false;
                return;
            }

            _waitingForChoice = false;

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
                Debug.LogWarning("[TreasureManager] ApplyTowerChoice: TowerInventory 인스턴스 없음");
            }

            OnTowerTreasureChosen?.Invoke(towerData);
        }

        /// <summary>
        /// availableTowers에서 해금된 타워만 대상으로 중복 없이 count개를 랜덤 추출한다.
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
            var usedIndices = new HashSet<int>();
            int maxAttempts = count * 10;
            int attempts = 0;

            int actualCount = Mathf.Min(count, unlocked.Count);

            while (result.Count < actualCount && attempts < maxAttempts)
            {
                attempts++;
                int idx = UnityEngine.Random.Range(0, unlocked.Count);
                if (!usedIndices.Contains(idx))
                {
                    usedIndices.Add(idx);
                    result.Add(unlocked[idx]);
                }
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
        /// 보물상자를 확률 무시하고 강제 드롭합니다 (디버그용).
        /// Phase 2 타워 보물상자(OnBossKilled)를 트리거합니다.
        /// </summary>
        public void Debug_ForceDrop()
        {
            OnBossKilled();
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
