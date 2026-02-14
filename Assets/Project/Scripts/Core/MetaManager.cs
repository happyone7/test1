using System.Collections.Generic;
using System.IO;
using Tesseract.Core;
using UnityEngine;

namespace Nodebreaker.Core
{
    public class MetaManager : Singleton<MetaManager>
    {
        [Header("스킬 데이터")]
        public Data.SkillNodeData[] allSkills;

        // TODO: Unity 재시작 후 Tesseract.Save.SaveManager로 교체
        PlayerSaveData _data;
        Dictionary<string, int> _skillLevelCache = new Dictionary<string, int>();

        string SavePath => Path.Combine(Application.persistentDataPath, "nodebreaker_save.json");

        public int TotalBit => _data.totalBit;
        public int TotalCore => _data.totalCore;
        public int CurrentStageIndex => _data.currentStageIndex;

        protected override void Awake()
        {
            base.Awake();
            Load();
        }

        void Load()
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                _data = JsonUtility.FromJson<PlayerSaveData>(json) ?? new PlayerSaveData();
            }
            else
            {
                _data = new PlayerSaveData();
            }
            RebuildCache();
        }

        public void Save()
        {
            SyncFromCache();
            string json = JsonUtility.ToJson(_data, true);
            File.WriteAllText(SavePath, json);
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
            return _data.totalBit >= cost;
        }

        public bool TryPurchaseSkill(string skillId)
        {
            if (!CanPurchaseSkill(skillId)) return false;

            var skillData = FindSkillData(skillId);
            int currentLevel = GetSkillLevel(skillId);
            int cost = skillData.GetCost(currentLevel);

            _data.totalBit -= cost;
            _skillLevelCache[skillId] = currentLevel + 1;
            Save();
            return true;
        }

        public void AddRunRewards(int bit, int core, bool cleared, int stageIdx)
        {
            _data.totalBit += bit;
            _data.totalCore += core;
            if (cleared)
                _data.currentStageIndex = Mathf.Max(_data.currentStageIndex, stageIdx + 1);
            Save();
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
            base.OnApplicationQuit();
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                Save();
        }
    }
}
