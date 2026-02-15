using System;
using UnityEngine;

namespace Nodebreaker.Data
{
    public enum UnlockConditionType
    {
        /// <summary>이전 스테이지 보스 처치 (기본)</summary>
        BossKill,
        /// <summary>누적 Bit 달성</summary>
        TotalBit,
        /// <summary>누적 킬 수 달성</summary>
        TotalKills,
        /// <summary>조건 없음 (항상 해금)</summary>
        None
    }

    [Serializable]
    public class StageUnlockCondition
    {
        public UnlockConditionType type = UnlockConditionType.BossKill;

        [Tooltip("BossKill: 이전 스테이지 인덱스 (기본 -1 = 자동), TotalBit/TotalKills: 필요 수치")]
        public int requiredValue;

        [Tooltip("해금 조건 설명 (UI 표시용)")]
        public string description;
    }

    [CreateAssetMenu(fileName = "Stage_", menuName = "Nodebreaker/Stage Data")]
    public class StageData : ScriptableObject
    {
        public string stageId;
        public string stageName;
        public int stageIndex;

        [Header("웨이브")]
        public WaveData[] waves;

        [Header("스케일링 배율")]
        public float hpMultiplier = 1f;
        public float speedMultiplier = 1f;
        public float bitDropMultiplier = 1f;

        [Header("클리어 보상")]
        public int coreReward = 2;

        [Header("기지")]
        public int baseHp = 10;

        [Header("해금 조건 (Stage 1은 비어있으면 항상 해금)")]
        public StageUnlockCondition[] unlockConditions;
    }
}
