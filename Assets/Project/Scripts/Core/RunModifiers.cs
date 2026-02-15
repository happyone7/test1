namespace Nodebreaker.Core
{
    public struct RunModifiers
    {
        public float attackDamageMultiplier;
        public float attackSpeedMultiplier;
        public int bonusBaseHp;

        public static RunModifiers Default => new RunModifiers
        {
            attackDamageMultiplier = 1f,
            attackSpeedMultiplier = 1f,
            bonusBaseHp = 0
        };
    }
}
