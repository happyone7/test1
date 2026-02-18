using System;
using UnityEngine.Serialization;
using System.Collections.Generic;

namespace Soulspire.Core
{
    /// <summary>
    /// 런 간 유지되는 타워 배치 정보.
    /// 런 종료 시 저장, 다음 런 시작 시 복원.
    /// </summary>
    [Serializable]
    public class TowerPlacement
    {
        public string towerId;
        public int level;
        public float posX;
        public float posY;
    }

    [Serializable]
    public class PlayerSaveData
    {
        [FormerlySerializedAs("totalBit")]
        public int totalSoul;
        [FormerlySerializedAs("totalCore")]
        public int totalCoreFragment;
        public int currentStageIndex;
        public int totalNodesKilled;
        public List<SkillLevelEntry> skillLevels = new List<SkillLevelEntry>();

        /// <summary>
        /// FTUE 플래그 (5개):
        /// [0] 첫 플레이 완료 여부
        /// [1] 첫 타워 배치 완료
        /// [2] 첫 업그레이드 완료
        /// [3] 첫 스테이지 클리어
        /// [4] Hub 최초 방문 완료
        /// </summary>
        public bool[] ftueFlags = new bool[5];

        /// <summary>
        /// FTUE 가이드 표시 기록 (string key 기반).
        /// FTUEManager가 사용하는 가이드별 표시 여부.
        /// </summary>
        public List<string> ftueShownKeys = new List<string>();

        // ── Idle Soul ──

        /// <summary>마지막 플레이 시각 (DateTime.UtcNow.Ticks).</summary>
        public long lastPlayTimeTicks;

        // ── 오디오 설정 ──

        /// <summary>BGM 볼륨 (0~1). 기본값 1.</summary>
        public float bgmVolume = 1f;

        /// <summary>SFX 볼륨 (0~1). 기본값 1.</summary>
        public float sfxVolume = 1f;

        // ── 보물상자 ──

        /// <summary>보유 중인 보물상자 수 (보스 처치 시 획득, Sanctum에서 오픈).</summary>
        public int treasureChestCount;

        // ── 타워 배치 유지 ──

        /// <summary>
        /// 런 종료 시 저장된 타워 배치 목록.
        /// 다음 런 시작 시 이 목록을 기반으로 타워를 복원한다.
        /// </summary>
        public List<TowerPlacement> savedTowerPlacements = new List<TowerPlacement>();
    }

    [Serializable]
    public struct SkillLevelEntry
    {
        public string id;
        public int level;
    }
}
