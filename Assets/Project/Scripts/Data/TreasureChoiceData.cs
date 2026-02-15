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
        public string displayName;

        [TextArea(2, 4)]
        public string description;

        public Sprite icon;

        [Header("효과")]
        public TreasureEffectType effectType;
        public float effectValue;
        [Tooltip("0이면 영구 지속")]
        public float duration;

        [Header("희귀도")]
        public TreasureRarity rarity = TreasureRarity.Common;
    }

    public enum TreasureEffectType
    {
        AddTower,           // 무료 타워 1기 추가
        UpgradeRandomTower, // 랜덤 타워 1기 업그레이드
        HealBase,           // 기지 HP 회복
        GainBit,            // 즉시 Bit 획득
        AttackSpeedBoost,   // 전체 타워 공속 증가
        DamageBoost,        // 전체 타워 공격력 증가
        RangeBoost,         // 전체 타워 사거리 증가
        SlowAll,            // 모든 적 이동속도 감소
        ExtraProjectile,    // 추가 투사체 발사
        ChainLightning,     // 연쇄 번개 공격
        LifeSteal,          // 공격 시 HP 회복
        CritChance,         // 치명타 확률 증가
        ShieldBuff,         // 기지 보호막
        SpeedBuff           // 타워 공격/이동 전체 버프
    }

    public enum TreasureRarity
    {
        Common,
        Rare,
        Epic
    }
}
