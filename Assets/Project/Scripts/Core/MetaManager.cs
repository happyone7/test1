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

        [Header("신규 세이브 초기값")]
        [Tooltip("새 세이브 파일 생성 시 지급할 초기 Bit (테스트/튜토리얼용)")]
        public int initialBit = 500;

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
                Debug.Log($"[MetaManager] 세이브 로드 완료 — Bit: {_data.totalBit}, Core: {_data.totalCore}, Skills: {_data.skillLevels.Count}");
            }
            else
            {
                _data = new PlayerSaveData();
                _data.totalBit = initialBit;
                Debug.Log($"[MetaManager] 신규 세이브 생성 — 초기 Bit: {initialBit}");
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
            if (skillData == null)
            {
                Debug.LogWarning($"[MetaManager] CanPurchaseSkill: skillData not found for '{skillId}'");
                return false;
            }

            int currentLevel = GetSkillLevel(skillId);
            if (currentLevel >= skillData.maxLevel)
            {
                Debug.Log($"[MetaManager] CanPurchaseSkill: '{skillId}' already max level ({currentLevel}/{skillData.maxLevel})");
                return false;
            }

            int cost = skillData.GetCost(currentLevel);
            bool canAfford = _data.totalBit >= cost;
            if (!canAfford)
                Debug.Log($"[MetaManager] CanPurchaseSkill: '{skillId}' 자금 부족 (보유: {_data.totalBit}, 필요: {cost})");
            return canAfford;
        }

        public bool TryPurchaseSkill(string skillId)
        {
            if (!CanPurchaseSkill(skillId))
            {
                Debug.LogWarning($"[MetaManager] TryPurchaseSkill: '{skillId}' 구매 실패");
                return false;
            }

            var skillData = FindSkillData(skillId);
            int currentLevel = GetSkillLevel(skillId);
            int cost = skillData.GetCost(currentLevel);

            _data.totalBit -= cost;
            _skillLevelCache[skillId] = currentLevel + 1;
            Debug.Log($"[MetaManager] TryPurchaseSkill: '{skillId}' Lv{currentLevel}->Lv{currentLevel + 1}, 비용={cost}, 잔액={_data.totalBit}");
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
