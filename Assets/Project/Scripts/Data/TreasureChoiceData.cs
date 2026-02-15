using UnityEngine;

namespace Nodebreaker.Data
{
    /// <summary>
    /// 보물상자 3택 선택지 데이터.
    /// 웨이브 클리어 보상으로 3장의 카드 중 1장을 선택하여 런 버프를 획득한다.
    /// </summary>
    [CreateAssetMenu(fileName = "Treasure_", menuName = "Nodebreaker/Treasure Choice Data")]
    public class TreasureChoiceData : ScriptableObject
    {
        public string choiceId;
        public string choiceName;

        [TextArea(2, 4)]
        public string description;

        public Sprite icon;

        [Header("효과")]
        public TreasureEffectType effectType;
        public float effectValue;

        [Header("희귀도")]
        public TreasureRarity rarity = TreasureRarity.Common;
    }

    public enum TreasureEffectType
    {
        AttackDamage,       // 전체 타워 공격력 +%
        AttackSpeed,        // 전체 타워 공속 +%
        Range,              // 전체 타워 사거리 +%
        AoERange,           // Cannon AoE 범위 +%
        CooldownReduction,  // 쿨타임 감소
        BaseHp,             // 기지 HP 즉시 회복
        Critical,           // 치명타 확률 추가
        ChainExplosion,     // 연쇄 폭발 확률
        BitMagnet,          // Bit 획득량 +%
        Berserk,            // HP 낮을수록 공격력 증가
        GiantSlayer,        // 대형 몬스터 추가 데미지
        Overcharge,         // 일정 확률로 2배 공격
        TimeWarp,           // 일시적 슬로우 필드
        TreasureHunter      // 추가 보물상자 확률
    }

    public enum TreasureRarity
    {
        Common,
        Rare,
        Epic
    }
}
