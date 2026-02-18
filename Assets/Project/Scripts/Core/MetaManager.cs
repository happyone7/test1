using System;
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

        /// <summary>스킬 구매 완료 시 발생. 인자: 구매된 skillId</summary>
        public event Action<string> OnSkillPurchased;

        SaveManager<PlayerSaveData> _saveManager;
        PlayerSaveData _data;
        Dictionary<string, int> _skillLevelCache = new Dictionary<string, int>();

        public int TotalSoul => _data.totalSoul;
        public int TotalCoreFragment => _data.totalCoreFragment;
        public int CurrentStageIndex => _data.currentStageIndex;
        public int TotalNodesKilled => _data.totalNodesKilled;
        public int TreasureChestCount => _data.treasureChestCount;

        protected override void Awake()
        {
            base.Awake();
            _saveManager = new SaveManager<PlayerSaveData>("soulspire_save.json");
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

            if (skillData.isCoreNode)
                return _data.totalCoreFragment >= cost;
            else
                return _data.totalSoul >= cost;
        }

        public bool TryPurchaseSkill(string skillId)
        {
            if (!CanPurchaseSkill(skillId)) return false;

            var skillData = FindSkillData(skillId);
            int currentLevel = GetSkillLevel(skillId);
            int cost = skillData.GetCost(currentLevel);

            if (skillData.isCoreNode)
                _data.totalCoreFragment -= cost;
            else
                _data.totalSoul -= cost;

            _skillLevelCache[skillId] = currentLevel + 1;
            Save();
            OnSkillPurchased?.Invoke(skillId);
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
        /// 특정 타워가 해금되었는지 확인합니다.
        /// Arrow는 항상 해금. 다른 타워는 스킬트리에서 해당 해금 노드를 구매해야 합니다.
        /// </summary>
        public bool IsTowerUnlocked(string towerId)
        {
            if (towerId == "arrow") return true; // Arrow는 항상 사용 가능

            if (allSkills == null) return false;
            foreach (var skill in allSkills)
            {
                if (skill == null || !skill.isCoreNode) continue;
                if (skill.unlockTargetId == towerId && GetSkillLevel(skill.skillId) > 0)
                    return true;
            }
            return false;
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

        /// <summary>
        /// speed_mode(SpeedControl 효과) 스킬이 하나라도 구매되어 있는지 확인.
        /// UI에서 x2/x3 버튼 interactable 여부 판단에 사용.
        /// CalculateModifiers()보다 가볍게 단일 여부만 반환.
        /// </summary>
        public bool HasSpeedControl()
        {
            if (allSkills == null) return false;

            foreach (var skill in allSkills)
            {
                if (skill == null) continue;
                if (skill.effectType != Data.SkillEffectType.SpeedControl) continue;
                if (GetSkillLevel(skill.skillId) > 0) return true;
            }
            return false;
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
                    case Data.SkillEffectType.Range:
                        mods.rangeBonus += value;
                        break;
                    case Data.SkillEffectType.SoulGain:
                        mods.soulGainMultiplier += value;
                        break;
                    case Data.SkillEffectType.StartSoul:
                        mods.startSoul += Mathf.RoundToInt(value);
                        break;
                    case Data.SkillEffectType.SpawnRate:
                        mods.spawnRateMultiplier += value;
                        break;
                    case Data.SkillEffectType.HpRegen:
                        mods.hpRegenPerSecond += value;
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


        // ── FTUE 시스템 (string key 기반 — FTUEManager 가이드용) ──

        /// <summary>
        /// FTUE 가이드가 이미 표시되었는지 확인합니다.
        /// </summary>
        public bool HasFtueShown(string key)
        {
            if (_data.ftueShownKeys == null) return false;
            return _data.ftueShownKeys.Contains(key);
        }

        /// <summary>
        /// FTUE 가이드 표시 기록을 저장합니다.
        /// </summary>
        public void MarkFtueShown(string key)
        {
            if (_data.ftueShownKeys == null)
                _data.ftueShownKeys = new System.Collections.Generic.List<string>();
            if (!_data.ftueShownKeys.Contains(key))
            {
                _data.ftueShownKeys.Add(key);
                Save();
            }
        }

        /// <summary>
        /// 모든 FTUE 플래그와 가이드 표시 기록을 초기화합니다 (디버그용).
        /// </summary>
        public void Debug_ResetAllFtueFlags()
        {
            if (_data.ftueFlags != null)
            {
                for (int i = 0; i < _data.ftueFlags.Length; i++)
                    _data.ftueFlags[i] = false;
            }
            if (_data.ftueShownKeys != null)
                _data.ftueShownKeys.Clear();
            Save();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // ── 디버그 전용 메서드 ──

        /// <summary>
        /// CoreFragment 재화를 지정량만큼 추가합니다 (디버그용).
        /// </summary>
        public void Debug_AddCoreFragment(int amount)
        {
            _data.totalCoreFragment += amount;
            Save();
        }

        /// <summary>
        /// Soul 재화를 지정량만큼 추가합니다 (디버그용).
        /// </summary>
        public void Debug_AddSoul(int amount)
        {
            _data.totalSoul += amount;
            Save();
        }

        /// <summary>
        /// 지정 스킬의 레벨을 강제 설정합니다 (디버그용).
        /// </summary>
        public void Debug_SetSkillLevel(string skillId, int level)
        {
            if (string.IsNullOrEmpty(skillId)) return;
            _skillLevelCache[skillId] = Mathf.Max(0, level);
            Save();
        }

        /// <summary>
        /// totalNodesKilled 값을 직접 설정합니다 (디버그용).
        /// </summary>
        public void Debug_SetNodesKilled(int count)
        {
            _data.totalNodesKilled = Mathf.Max(0, count);
            Save();
        }
#endif

        // ── 보물상자 ──

        /// <summary>
        /// 보물상자를 count개 추가하고 저장합니다.
        /// 보스 처치 시 TreasureManager.OnBossKilled()에서 호출.
        /// </summary>
        public void AddTreasureChest(int count = 1)
        {
            _data.treasureChestCount += count;
            Save();
            Debug.Log($"[MetaManager] 보물상자 +{count} (보유: {_data.treasureChestCount})");
        }

        /// <summary>
        /// 보물상자를 1개 소비합니다. 보유 수가 0이면 false 반환.
        /// Sanctum에서 보물상자 오픈 시 TreasureManager.OpenTreasureChest()에서 호출.
        /// </summary>
        public bool SpendTreasureChest()
        {
            if (_data.treasureChestCount <= 0) return false;
            _data.treasureChestCount--;
            Save();
            return true;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>
        /// 보물상자 보유 수를 강제 설정합니다 (디버그용).
        /// </summary>
        public void Debug_SetTreasureChestCount(int count)
        {
            _data.treasureChestCount = Mathf.Max(0, count);
            Save();
        }
#endif

        // ── Idle Soul 수집기 ──

        const float IDLE_SOUL_PER_MINUTE = 0.5f;
        const float MAX_IDLE_HOURS = 8f;

        public int CalculateIdleSoul()
        {
            if (_data.lastPlayTimeTicks <= 0) return 0;

            var lastPlay = new DateTime(_data.lastPlayTimeTicks, DateTimeKind.Utc);
            var elapsed = DateTime.UtcNow - lastPlay;
            if (elapsed.TotalMinutes < 1) return 0;

            float minutes = (float)System.Math.Min(elapsed.TotalMinutes, MAX_IDLE_HOURS * 60f);
            return Mathf.FloorToInt(minutes * IDLE_SOUL_PER_MINUTE);
        }

        public void CollectIdleSoul()
        {
            int amount = CalculateIdleSoul();
            if (amount > 0)
            {
                _data.totalSoul += amount;
                UpdateLastPlayTime();
                Save();
            }
        }

        public void UpdateLastPlayTime()
        {
            _data.lastPlayTimeTicks = DateTime.UtcNow.Ticks;
            Save();
        }

        // ── 오디오 설정 ──

        /// <summary>
        /// 저장된 BGM 볼륨을 반환합니다 (0~1).
        /// </summary>
        public float GetSavedBgmVolume() => _data.bgmVolume;

        /// <summary>
        /// 저장된 SFX 볼륨을 반환합니다 (0~1).
        /// </summary>
        public float GetSavedSfxVolume() => _data.sfxVolume;

        /// <summary>
        /// BGM 볼륨을 세이브 데이터에 저장합니다.
        /// </summary>
        public void SetSavedBgmVolume(float value)
        {
            _data.bgmVolume = Mathf.Clamp01(value);
            Save();
        }

        /// <summary>
        /// SFX 볼륨을 세이브 데이터에 저장합니다.
        /// </summary>
        public void SetSavedSfxVolume(float value)
        {
            _data.sfxVolume = Mathf.Clamp01(value);
            Save();
        }

        // ── 타워 배치 유지 ──

        /// <summary>
        /// 현재 배치된 타워 목록을 세이브 데이터에 저장합니다.
        /// 해금된 타워만 저장하며, 미해금 타워는 제외합니다.
        /// </summary>
        public void SaveTowerPlacements()
        {
            _data.savedTowerPlacements.Clear();

            if (!Singleton<Tower.TowerManager>.HasInstance) return;

            var placedTowers = Tower.TowerManager.Instance.PlacedTowers;
            foreach (var tower in placedTowers)
            {
                if (tower == null || tower.data == null) continue;

                // 해금 여부 확인 (미해금 타워는 저장하지 않음)
                if (!IsTowerUnlocked(tower.data.towerId)) continue;

                _data.savedTowerPlacements.Add(new TowerPlacement
                {
                    towerId = tower.data.towerId,
                    level = tower.Level,
                    posX = tower.transform.position.x,
                    posY = tower.transform.position.y
                });
            }

            Save();
            Debug.Log($"[MetaManager] 타워 배치 저장 완료: {_data.savedTowerPlacements.Count}개");
        }

        /// <summary>
        /// 저장된 타워 배치 목록을 반환합니다 (읽기 전용).
        /// </summary>
        public IReadOnlyList<TowerPlacement> GetSavedTowerPlacements()
        {
            return _data.savedTowerPlacements;
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
            if (_saveManager != null && _data != null)
            {
                UpdateLastPlayTime();
                Save();
            }
            _saveManager?.Dispose();
            base.OnApplicationQuit();
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                UpdateLastPlayTime();
                Save();
            }
        }
    }
}
