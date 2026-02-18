namespace Soulspire.Core
{
    public struct RunModifiers
    {
        public float attackDamageMultiplier;
        public float attackSpeedMultiplier;
        public int bonusBaseHp;
        public float rangeBonus;           // 사거리 추가 (0에서 시작)
        public float soulGainMultiplier;   // Soul 획득 배율 (1에서 시작)
        public int startSoul;              // 런 시작 시 보너스 Soul
        public float spawnRateMultiplier;  // Node 스폰율 배율 (1에서 시작)
        public float hpRegenPerSecond;     // 초당 HP 회복량

        public static RunModifiers Default => new RunModifiers
        {
            attackDamageMultiplier = 1f,
            attackSpeedMultiplier = 1f,
            bonusBaseHp = 0,
            rangeBonus = 0f,
            soulGainMultiplier = 1f,
            startSoul = 0,
            spawnRateMultiplier = 1f,
            hpRegenPerSecond = 0f
        };
    }
}
