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
        public List<SkillLevelEntry> skillLevels = new List<SkillLevelEntry>();

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
