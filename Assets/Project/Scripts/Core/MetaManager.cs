using System.Collections.Generic;
using Tesseract.Core;
using Tesseract.Save;
using UnityEngine;

namespace Soulspire.Core
{
    public class MetaManager : Singleton<MetaManager>
    {
        [Header("스킬 데이터")]
        public Data.SkillNodeData[] allSkills;

        SaveManager<PlayerSaveData> _saveManager;
        PlayerSaveData _data;
        Dictionary<string, int> _skillLevelCache = new Dictionary<string, int>();

        public int TotalSoul => _data.totalSoul;
        public int TotalCoreFragment => _data.totalCoreFragment;
        public int CurrentStageIndex => _data.currentStageIndex;
        public int TotalNodesKilled => _data.totalNodesKilled;

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
        }

        public void Save()
        {
            if (_saveManager == null || _data == null) return;
            SyncFromCache();
            _saveManager.Save(_data);
        }

        public int GetSkillLevel(string skillId)
        {
            _skillLevelCache.TryGetValue(skillId, out int level);
            return level;
        }

        public bool CanPurchaseSkill(string skillId)
        {
            var skillData = FindSkillData(skillId);
            if (skillData == null) return false;

            int currentLevel = GetSkillLevel(skillId);
            if (currentLevel >= skillData.maxLevel) return false;

            int cost = skillData.GetCost(currentLevel);
            return _data.totalSoul >= cost;
        }

        public bool TryPurchaseSkill(string skillId)
        {
            if (!CanPurchaseSkill(skillId)) return false;

            var skillData = FindSkillData(skillId);
            int currentLevel = GetSkillLevel(skillId);
            int cost = skillData.GetCost(currentLevel);

            _data.totalSoul -= cost;
            _skillLevelCache[skillId] = currentLevel + 1;
            Save();
            return true;
        }

        public void AddRunRewards(int soul, int coreFragment, bool cleared, int stageIdx, int nodesKilled = 0)
        {
            _data.totalSoul += soul;
            _data.totalCoreFragment += coreFragment;
            _data.totalNodesKilled += nodesKilled;
            if (cleared)
                _data.currentStageIndex = Mathf.Max(_data.currentStageIndex, stageIdx + 1);
            Save();
        }

        /// <summary>
        /// 스테이지 해금 조건을 확인합니다.
        /// Stage 1: 항상 해금
        /// Stage 2: totalSoul >= 500 AND totalNodesKilled >= 100
        /// Stage 3: totalSoul >= 3000 AND totalNodesKilled >= 500
        /// </summary>
        public bool IsStageUnlocked(int stageIndex)
        {
            switch (stageIndex)
            {
                case 0: // Stage 1: 항상 해금
                    return true;
                case 1: // Stage 2
                    return _data.totalSoul >= 500 && _data.totalNodesKilled >= 100;
                case 2: // Stage 3
                    return _data.totalSoul >= 3000 && _data.totalNodesKilled >= 500;
                default: // 이후 스테이지: currentStageIndex 기반 진행도 체크
                    return _data.currentStageIndex >= stageIndex;
            }
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
                        mods.attackDamageMultiplier += value;
                        break;
                    case Data.SkillEffectType.AttackSpeed:
                        mods.attackSpeedMultiplier += value;
                        break;
                    case Data.SkillEffectType.BaseHp:
                        mods.bonusBaseHp += Mathf.RoundToInt(value);
                        break;
                }
            }

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

        // ── FTUE 시스템 ──

        /// <summary>
        /// FTUE 플래그 인덱스를 가져옵니다.
        /// </summary>
        public bool GetFtueFlag(int index)
        {
            if (_data.ftueFlags == null || index < 0 || index >= _data.ftueFlags.Length)
                return false;
            return _data.ftueFlags[index];
        }

        /// <summary>
        /// FTUE 플래그를 설정하고 저장합니다.
        /// </summary>
        public void SetFtueFlag(int index, bool value = true)
        {
            if (_data.ftueFlags == null)
                _data.ftueFlags = new bool[5];
            if (index < 0 || index >= _data.ftueFlags.Length) return;
            _data.ftueFlags[index] = value;
            Save();
        }

        /// <summary>
        /// 첫 플레이 여부 (ftueFlags[0]이 false이면 첫 플레이).
        /// </summary>
        public bool IsFirstPlay => !GetFtueFlag(0);

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
            if (_saveManager != null && _data != null)
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
