using System;
using System.Collections.Generic;
using Tesseract.Core;
using UnityEngine;
using Nodebreaker.Data;

namespace Nodebreaker.Core
{
    /// <summary>
    /// 보물상자 시스템 매니저.
    /// 웨이브 클리어 시 확률적으로 보물상자를 드랍하고,
    /// 플레이어가 3택 중 하나를 선택하면 해당 효과를 현재 런에 적용한다.
    /// </summary>
    public class TreasureManager : Singleton<TreasureManager>
    {
        [Header("보물 설정")]
        public TreasureConfig treasureConfig;

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
        int _bitBonusPerKill = 0;

        public float DamageMultiplier => _damageMultiplier;
        public float AttackSpeedMultiplier => _attackSpeedMultiplier;
        public float RangeMultiplier => _rangeMultiplier;
        public float TowerCostDiscount => _towerCostDiscount;
        public float CritChance => _critChance;
        public float SlowEffectBonus => _slowEffectBonus;
        public int BonusMaxHp => _bonusMaxHp;
        public int BitBonusPerKill => _bitBonusPerKill;

        // ── 대기 상태 (UI가 3택을 표시 중인지) ──
        bool _waitingForChoice;
        public bool IsWaitingForChoice => _waitingForChoice;

        // ── 공개 API ──

        /// <summary>
        /// 웨이브 클리어 시 WaveSpawner에서 호출.
        /// dropChance 확률로 보물상자를 드랍한다.
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
            _bitBonusPerKill = 0;
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

                case TreasureChoiceType.BitBonus:
                    // value는 노드 처치 시 추가 Bit (예: 2 = +2 Bit per kill)
                    _bitBonusPerKill += Mathf.RoundToInt(choice.value);
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

                default:
                    Debug.LogWarning($"[TreasureManager] 알 수 없는 choiceType: {choice.choiceType}");
                    break;
            }
        }
    }
}
