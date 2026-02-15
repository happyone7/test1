namespace Nodebreaker.Core
{
    public struct RunModifiers
    {
        public float attackDamageMultiplier;
        public float attackSpeedMultiplier;
        public int bonusBaseHp;
        public float rangeBonus;
        public float bitGainMultiplier;
        public int startBitBonus;
        public float spawnRateMultiplier;
        public float hpRegenPerSec;
        public bool hasCritical;
        public bool hasIdleCollector;
        public bool hasSpeedControl;

        public static RunModifiers Default => new RunModifiers
        {
            attackDamageMultiplier = 1f,
            attackSpeedMultiplier = 1f,
            bonusBaseHp = 0,
            rangeBonus = 0f,
            bitGainMultiplier = 1f,
            startBitBonus = 0,
            spawnRateMultiplier = 1f,
            hpRegenPerSec = 0f,
            hasCritical = false,
            hasIdleCollector = false,
            hasSpeedControl = false
        };
    }
}
