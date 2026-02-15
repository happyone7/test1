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

        /// <summary>클리어한 스테이지 인덱스 목록 (보스 처치 기록)</summary>
        public List<int> clearedStages = new List<int>();

        /// <summary>누적 킬 수 (모든 런 합산)</summary>
        public int totalKills;

        /// <summary>누적 획득 Bit (모든 런 합산, 소비와 별개)</summary>
        public int totalBitEarned;
    }

    [Serializable]
    public struct SkillLevelEntry
    {
        public string id;
        public int level;
    }
}
