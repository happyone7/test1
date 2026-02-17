using Tesseract.Core;
using UnityEngine;

namespace Soulspire.Core
{
    /// <summary>
    /// GDD InGameCombat_v0.1.md 6.4절 기반 콤보 시스템.
    /// 3초 이내 연속 처치 시 콤보 카운트를 올리고, 구간별 보너스 Soul을 지급합니다.
    /// 5+ 콤보: +50%, 10+ 콤보: +100%, 20+ 콤보: +200%.
    /// </summary>
    public class ComboSystem : Singleton<ComboSystem>
    {
        const float COMBO_WINDOW = 3.0f; // 3초 이내 연속 처치

        float _comboTimer;
        int _comboCount;

        /// <summary>
        /// 현재 콤보 카운트 (외부 UI 표시용).
        /// </summary>
        public int ComboCount => _comboCount;

        /// <summary>
        /// Node가 처치될 때 호출합니다.
        /// baseSoulDrop에 대해 콤보 보너스 Soul을 계산하여 반환합니다.
        /// </summary>
        /// <param name="baseSoulDrop">기본 Soul 드롭량 (TreasureManager 보너스 포함)</param>
        /// <returns>콤보 보너스로 추가 지급할 Soul 양 (보너스 없으면 0)</returns>
        public int OnNodeKilled(int baseSoulDrop)
        {
            _comboTimer = COMBO_WINDOW;
            _comboCount++;

            int bonus = 0;
            float bonusRate = GetBonusRate(_comboCount);
            if (bonusRate > 0f)
                bonus = Mathf.RoundToInt(baseSoulDrop * bonusRate);

            // 콤보 텍스트 표시 (FeedbackManager 사용)
            if (_comboCount >= 5 && Singleton<FeedbackManager>.HasInstance)
            {
                string comboText;
                if (_comboCount >= 20)
                    comboText = "MEGA COMBO!";
                else
                    comboText = $"COMBO x{_comboCount}!";

                FeedbackManager.Instance.SpawnComboText(comboText);
            }

            // 10 콤보 도달 시 카메라 셰이크
            if (_comboCount == 10 && Singleton<FeedbackManager>.HasInstance)
                FeedbackManager.Instance.ShakeCamera(0.15f, 0.2f);

            return bonus;
        }

        /// <summary>
        /// 콤보 구간별 보너스 비율을 반환합니다.
        /// </summary>
        float GetBonusRate(int combo)
        {
            if (combo >= 20) return 2.0f;  // +200%
            if (combo >= 10) return 1.0f;  // +100%
            if (combo >= 5) return 0.5f;   // +50%
            return 0f;
        }

        void Update()
        {
            if (_comboTimer > 0f)
            {
                _comboTimer -= Time.deltaTime;
                if (_comboTimer <= 0f)
                {
                    _comboCount = 0;
                }
            }
        }

        /// <summary>
        /// 런 시작 시 콤보 상태를 리셋합니다.
        /// </summary>
        public void ResetCombo()
        {
            _comboCount = 0;
            _comboTimer = 0f;
        }
    }
}
