using System;
using System.Collections.Generic;

namespace Nodebreaker.Core
{
    [Serializable]
    public class PlayerSaveData
    {
        public int totalBit;
        public int totalCore;
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
    }

    [Serializable]
    public struct SkillLevelEntry
    {
        public string id;
        public int level;
    }
}
