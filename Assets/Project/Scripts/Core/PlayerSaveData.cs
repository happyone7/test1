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
    }

    [Serializable]
    public struct SkillLevelEntry
    {
        public string id;
        public int level;
    }
}
