using Tesseract.Core;
using UnityEngine;

namespace Soulspire.UI
{
    /// <summary>
    /// FTUE (First-Time User Experience) 가이드 텍스트 트리거 관리.
    /// 5가지 가이드를 조건 충족 시 한 번씩만 표시합니다.
    /// </summary>
    public class FTUEManager : Singleton<FTUEManager>
    {
        private const string PrefKey = "FTUE_Shown_";

        // 가이드 텍스트 정의
        public static readonly string GuideFirstWave = "타워가 적을 자동으로 공격합니다";
        public static readonly string GuideFirstTowerDrop = "타워를 획득했습니다! 드래그하여 배치하세요";
        public static readonly string GuideFirstDeath = "성장하면 더 강해집니다 — Hub에서 업그레이드!";
        public static readonly string GuideHubFirstEntry = "스킬을 눌러 영구 강화를 구매하세요";
        public static readonly string GuideBossWave = "보스가 나타났습니다! 처치하면 클리어!";

        /// <summary>
        /// 가이드 텍스트를 InGameUI에 표시합니다 (중복 표시 방지).
        /// </summary>
        public void TriggerInGame(string guideKey, string text)
        {
            if (HasShown(guideKey)) return;
            MarkShown(guideKey);

            var ui = Object.FindFirstObjectByType<InGameUI>(FindObjectsInactive.Include);
            if (ui != null)
                ui.ShowGuideText(text);
        }

        /// <summary>
        /// 가이드 텍스트를 HubUI에 표시합니다 (중복 표시 방지).
        /// </summary>
        public void TriggerHub(string guideKey, string text)
        {
            if (HasShown(guideKey)) return;
            MarkShown(guideKey);

            var ui = Object.FindFirstObjectByType<HubUI>(FindObjectsInactive.Include);
            if (ui != null)
                ui.ShowGuideText(text);
        }

        /// <summary>
        /// 모든 FTUE 기록을 초기화합니다 (디버그/테스트용).
        /// </summary>
        public void ResetAll()
        {
            PlayerPrefs.DeleteKey(PrefKey + "FirstWave");
            PlayerPrefs.DeleteKey(PrefKey + "FirstTowerDrop");
            PlayerPrefs.DeleteKey(PrefKey + "FirstDeath");
            PlayerPrefs.DeleteKey(PrefKey + "HubFirstEntry");
            PlayerPrefs.DeleteKey(PrefKey + "BossWave");
            PlayerPrefs.Save();
        }

        private bool HasShown(string key) => PlayerPrefs.GetInt(PrefKey + key, 0) == 1;

        private void MarkShown(string key)
        {
            PlayerPrefs.SetInt(PrefKey + key, 1);
            PlayerPrefs.Save();
        }
    }
}
