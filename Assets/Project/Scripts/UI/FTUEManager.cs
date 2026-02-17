using Tesseract.Core;
using UnityEngine;

namespace Soulspire.UI
{
    /// <summary>
    /// FTUE (First-Time User Experience) 가이드 텍스트 트리거 관리.
    /// 5가지 가이드를 조건 충족 시 한 번씩만 표시합니다.
    /// PlayerPrefs 대신 MetaManager(Tesseract Save)를 통해 세이브합니다.
    /// </summary>
    public class FTUEManager : Singleton<FTUEManager>
    {
        // 가이드 텍스트 정의
        public static readonly string GuideFirstWave = "타워가 적을 자동으로 공격합니다";
        public static readonly string GuideFirstTowerDrop = "타워를 획득했습니다! 드래그하여 배치하세요";
        public static readonly string GuideFirstDeath = "성장하면 더 강해집니다 — Sanctum에서 업그레이드!";
        public static readonly string GuideHubFirstEntry = "스킬을 눌러 영구 강화를 구매하세요";
        public static readonly string GuideBossWave = "보스가 나타났습니다! 처치하면 클리어!";
        public static readonly string GuideFirstTreasure = "보물상자에서 타워를 획득했습니다! 인벤토리에 추가됩니다";
        public static readonly string GuideFirstSortie = "터치하여 타워를 배치할 위치를 선택하세요";
        public static readonly string GuideTowerSell = "타워를 터치하면 판매할 수 있습니다 (70% Soul 환급)";

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
        /// MetaManager의 Debug_ResetAllFtueFlags를 통해 초기화합니다.
        /// </summary>
        public void ResetAll()
        {
            if (Singleton<Core.MetaManager>.HasInstance)
                Core.MetaManager.Instance.Debug_ResetAllFtueFlags();
        }

        private bool HasShown(string key)
        {
            if (!Singleton<Core.MetaManager>.HasInstance) return false;
            return Core.MetaManager.Instance.HasFtueShown(key);
        }

        private void MarkShown(string key)
        {
            if (!Singleton<Core.MetaManager>.HasInstance) return;
            Core.MetaManager.Instance.MarkFtueShown(key);
        }
    }
}
