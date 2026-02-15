using System.Collections.Generic;
using Tesseract.Core;
using Tesseract.Save;
using UnityEngine;

namespace Nodebreaker.Core
{
    public class MetaManager : Singleton<MetaManager>
    {
        [Header("스킬 데이터")]
        public Data.SkillNodeData[] allSkills;

        SaveManager<PlayerSaveData> _saveManager;
        PlayerSaveData _data;
        Dictionary<string, int> _skillLevelCache = new Dictionary<string, int>();

        public int TotalBit => _data.totalBit;
        public int TotalCore => _data.totalCore;
        public int CurrentStageIndex => _data.currentStageIndex;
        public int TotalKills => _data.totalKills;
        public int TotalBitEarned => _data.totalBitEarned;

        protected override void Awake()
        {
            base.Awake();
            _saveManager = new SaveManager<PlayerSaveData>("nodebreaker_save.json");
            Load();
        }

        void Load()
        {
            _data = _saveManager.Load();
            RebuildCache();
            EnsureCoreNodeActive();
        }

        /// <summary>
        /// Core Node(N00)가 항상 활성 상태인지 확인하고, 미활성이면 자동 활성화
        /// </summary>
        void EnsureCoreNodeActive()
        {
            if (GetSkillLevel("N00") <= 0)
            {
                _skillLevelCache["N00"] = 1;
                Save();
                Debug.Log("[MetaManager] Core Node(N00) 자동 활성화");
            }
        }

        public void Save()
        {
            SyncFromCache();
            _saveManager.Save(_data);
        }

        public int GetSkillLevel(string skillId)
        {
            _skillLevelCache.TryGetValue(skillId, out int level);
            return level;
        }

        /// <summary>
        /// 전제 조건 충족 여부 확인 (OR/AND 모드 지원)
        /// </summary>
        public bool ArePrerequisitesMet(string skillId)
        {
            var skillData = FindSkillData(skillId);
            if (skillData == null) return false;

            // 전제 조건 없는 노드(Core Node 등)는 항상 충족
            if (skillData.prerequisiteIds == null || skillData.prerequisiteIds.Length == 0)
                return true;

            if (skillData.prerequisiteMode == Data.PrerequisiteMode.And)
            {
                // AND: 모든 선행 노드가 1레벨 이상
                foreach (var prereqId in skillData.prerequisiteIds)
                {
                    if (string.IsNullOrEmpty(prereqId)) continue;
                    if (GetSkillLevel(prereqId) <= 0) return false;
                }
                return true;
            }
            else
            {
                // OR: 선행 노드 중 하나라도 1레벨 이상
                foreach (var prereqId in skillData.prerequisiteIds)
                {
                    if (string.IsNullOrEmpty(prereqId)) continue;
                    if (GetSkillLevel(prereqId) > 0) return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 노드가 보이는지 여부 (선행 노드 중 최소 1개가 구매됨)
        /// </summary>
        public bool IsNodeVisible(string skillId)
        {
            var skillData = FindSkillData(skillId);
            if (skillData == null) return false;

            // 전제 조건 없는 노드(Core Node)는 항상 보임
            if (skillData.prerequisiteIds == null || skillData.prerequisiteIds.Length == 0)
                return true;

            // 선행 노드 중 최소 1개가 구매되어 있으면 보임
            foreach (var prereqId in skillData.prerequisiteIds)
            {
                if (string.IsNullOrEmpty(prereqId)) continue;
                if (GetSkillLevel(prereqId) > 0) return true;
            }
            return false;
        }

        /// <summary>
        /// 자원 충분 여부 확인 (Bit 또는 Core)
        /// </summary>
        public bool CanAfford(string skillId)
        {
            var skillData = FindSkillData(skillId);
            if (skillData == null) return false;

            int currentLevel = GetSkillLevel(skillId);
            if (currentLevel >= skillData.maxLevel) return false;

            int cost = skillData.GetCost(currentLevel);
            if (skillData.resourceType == Data.SkillResourceType.Core)
                return _data.totalCore >= cost;
            else
                return _data.totalBit >= cost;
        }

        public bool CanPurchaseSkill(string skillId)
        {
            var skillData = FindSkillData(skillId);
            if (skillData == null) return false;

            int currentLevel = GetSkillLevel(skillId);
            if (currentLevel >= skillData.maxLevel) return false;

            // 전제 조건 확인
            if (!ArePrerequisitesMet(skillId)) return false;

            // 자원 확인
            return CanAfford(skillId);
        }

        public bool TryPurchaseSkill(string skillId)
        {
            if (!CanPurchaseSkill(skillId)) return false;

            var skillData = FindSkillData(skillId);
            int currentLevel = GetSkillLevel(skillId);
            int cost = skillData.GetCost(currentLevel);

            // 자원 타입에 따라 차감
            if (skillData.resourceType == Data.SkillResourceType.Core)
                _data.totalCore -= cost;
            else
                _data.totalBit -= cost;

            _skillLevelCache[skillId] = currentLevel + 1;
            Debug.Log($"[MetaManager] TryPurchaseSkill: '{skillId}' Lv{currentLevel}->Lv{currentLevel + 1}, 비용={cost} {skillData.resourceType}");
            Save();
            return true;
        }

        public void AddRunRewards(int bit, int core, bool cleared, int stageIdx)
        {
            AddRunRewards(bit, core, cleared, stageIdx, 0);
        }

        public void AddRunRewards(int bit, int core, bool cleared, int stageIdx, int nodesKilled)
        {
            _data.totalBit += bit;
            _data.totalCore += core;
            _data.totalBitEarned += bit;
            _data.totalKills += nodesKilled;

            if (cleared)
            {
                _data.currentStageIndex = Mathf.Max(_data.currentStageIndex, stageIdx + 1);

                // 클리어한 스테이지 기록
                if (!_data.clearedStages.Contains(stageIdx))
                    _data.clearedStages.Add(stageIdx);
            }
            Save();
        }

        /// <summary>해당 스테이지를 클리어했는지 확인</summary>
        public bool IsStageCleared(int stageIndex)
        {
            return _data.clearedStages != null && _data.clearedStages.Contains(stageIndex);
        }

        /// <summary>스테이지 해금 조건 충족 여부 확인</summary>
        public bool IsStageUnlocked(Data.StageData stage)
        {
            if (stage == null) return false;

            // 해금 조건이 없으면 항상 해금 (Stage 1)
            if (stage.unlockConditions == null || stage.unlockConditions.Length == 0)
                return true;

            // 모든 조건을 충족해야 해금
            foreach (var cond in stage.unlockConditions)
            {
                if (cond == null) continue;

                switch (cond.type)
                {
                    case Data.UnlockConditionType.None:
                        continue;

                    case Data.UnlockConditionType.BossKill:
                        // 이전 스테이지 보스 처치 (requiredValue < 0이면 자동으로 stageIndex - 1)
                        int prevIdx = cond.requiredValue >= 0 ? cond.requiredValue : stage.stageIndex - 1;
                        if (prevIdx >= 0 && !IsStageCleared(prevIdx))
                            return false;
                        break;

                    case Data.UnlockConditionType.TotalBit:
                        if (_data.totalBitEarned < cond.requiredValue)
                            return false;
                        break;

                    case Data.UnlockConditionType.TotalKills:
                        if (_data.totalKills < cond.requiredValue)
                            return false;
                        break;
                }
            }

            return true;
        }

        public void SetCurrentStageIndex(int index)
        {
            _data.currentStageIndex = index;
        }

        public RunModifiers CalculateModifiers()
        {
            var mods = RunModifiers.Default;

            if (allSkills == null) return mods;

            foreach (var skill in allSkills)
            {
                if (skill == null || string.IsNullOrEmpty(skill.skillId)) continue;
                int level = GetSkillLevel(skill.skillId);
                if (level <= 0) continue;

                float value = skill.GetTotalValue(level);

                switch (skill.effectType)
                {
                    case Data.SkillEffectType.AttackDamage:
                        mods.attackDamageMultiplier += value * 0.01f;
                        break;
                    case Data.SkillEffectType.AttackSpeed:
                        mods.attackSpeedMultiplier += value * 0.01f;
                        break;
                    case Data.SkillEffectType.BaseHp:
                        mods.bonusBaseHp += Mathf.RoundToInt(value);
                        break;
                    case Data.SkillEffectType.Range:
                        mods.rangeBonus += value;
                        break;
                    case Data.SkillEffectType.BitGain:
                        mods.bitGainMultiplier += value * 0.01f;
                        break;
                    case Data.SkillEffectType.StartBit:
                        mods.startBitBonus += Mathf.RoundToInt(value);
                        break;
                    case Data.SkillEffectType.SpawnRate:
                        mods.spawnRateMultiplier += value * 0.01f;
                        break;
                    case Data.SkillEffectType.HpRegen:
                        mods.hpRegenPerSec += value;
                        break;
                    case Data.SkillEffectType.Critical:
                        mods.hasCritical = true;
                        break;
                    case Data.SkillEffectType.Idle:
                        mods.hasIdleCollector = true;
                        break;
                    case Data.SkillEffectType.SpeedControl:
                        mods.hasSpeedControl = true;
                        break;
                    case Data.SkillEffectType.TowerUnlock:
                    case Data.SkillEffectType.None:
                        break;
                }
            }

            Debug.Log($"[MetaManager] CalculateModifiers: atkDmg={mods.attackDamageMultiplier:F2}, " +
                      $"atkSpd={mods.attackSpeedMultiplier:F2}, bonusHp={mods.bonusBaseHp}, " +
                      $"range={mods.rangeBonus:F2}, bitGain={mods.bitGainMultiplier:F2}, " +
                      $"startBit={mods.startBitBonus}, spawnRate={mods.spawnRateMultiplier:F2}, " +
                      $"hpRegen={mods.hpRegenPerSec:F2}");

            return mods;
        }

        void RebuildCache()
        {
            _skillLevelCache.Clear();
            if (_data.skillLevels == null) return;

            foreach (var entry in _data.skillLevels)
            {
                _skillLevelCache[entry.id] = entry.level;
            }
        }

        void SyncFromCache()
        {
            _data.skillLevels.Clear();
            foreach (var kvp in _skillLevelCache)
            {
                _data.skillLevels.Add(new SkillLevelEntry
                {
                    id = kvp.Key,
                    level = kvp.Value
                });
            }
        }

        Data.SkillNodeData FindSkillData(string skillId)
        {
            if (allSkills == null) return null;
            foreach (var skill in allSkills)
            {
                if (skill == null) continue;
                if (skill.skillId == skillId)
                    return skill;
            }
            return null;
        }

        protected override void OnApplicationQuit()
        {
            Save();
            _saveManager?.Dispose();
            base.OnApplicationQuit();
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                Save();
        }
    }
}
