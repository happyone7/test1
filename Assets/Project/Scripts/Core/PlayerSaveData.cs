using System;
using UnityEngine.Serialization;
using System.Collections.Generic;

namespace Soulspire.Core
{
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

        // ── 오디오 설정 ──

        /// <summary>BGM 볼륨 (0~1). 기본값 1.</summary>
        public float bgmVolume = 1f;

        /// <summary>SFX 볼륨 (0~1). 기본값 1.</summary>
        public float sfxVolume = 1f;
    }

    [Serializable]
    public struct SkillLevelEntry
    {
        public string id;
        public int level;
    }
}
