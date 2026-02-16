using Tesseract.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Soulspire.UI
{
    /// <summary>
    /// 방치 Bit 수령 팝업 (PPT 명세 슬라이드 5-4)
    /// 420x250px, 테두리 #FFD84D 2px
    /// 제목: "방치 보상", 20pt 굵게, #FFD84D
    /// Bit 금액: 28pt 굵게, #2BFF88
    /// 부재 시간: 12pt, #AFC3E8
    /// 수령 버튼: 220x50px, 채우기=#302810, 테두리=#FFD84D
    /// </summary>
    public class IdleSoulPopup : MonoBehaviour
    {
        [Header("텍스트")]
        public Text titleText;
        public Text soulAmountText;
        public Text absenceTimeText;

        [Header("버튼")]
        public Button claimButton;
        public Text claimButtonText;

        [Header("UI 스프라이트")]
        public UISprites uiSprites;

        private int _pendingSoul;
        private float _absenceSeconds;

        void Start()
        {
            if (claimButton != null)
                claimButton.onClick.AddListener(OnClaim);

            // UI 스프라이트 적용
            if (uiSprites != null)
            {
                var bgImage = GetComponent<Image>();
                uiSprites.ApplyPanelFrame(bgImage);
                uiSprites.ApplyAccentButton(claimButton);
            }

            gameObject.SetActive(false);
        }

        public void Show(int soulAmount, float absenceSeconds)
        {
            _pendingSoul = soulAmount;
            _absenceSeconds = absenceSeconds;

            if (titleText != null)
                titleText.text = "방치 보상";

            if (soulAmountText != null)
                soulAmountText.text = $"+{soulAmount:N0} Bit";

            if (absenceTimeText != null)
            {
                int hours = Mathf.FloorToInt(absenceSeconds / 3600f);
                int minutes = Mathf.FloorToInt((absenceSeconds % 3600f) / 60f);
                absenceTimeText.text = hours > 0
                    ? $"부재 시간: {hours}시간 {minutes}분"
                    : $"부재 시간: {minutes}분";
            }

            if (claimButtonText != null)
                claimButtonText.text = "수령";

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnClaim()
        {
            if (_pendingSoul > 0 && Singleton<Core.MetaManager>.HasInstance)
            {
                Core.MetaManager.Instance.AddRunRewards(_pendingSoul, 0, false, 0);
                Debug.Log($"[IdleSoulPopup] 방치 보상 수령: {_pendingSoul} Bit");
            }

            _pendingSoul = 0;
            Hide();

            // 허브 UI 갱신
            var hubUI = Object.FindFirstObjectByType<HubUI>();
            if (hubUI != null)
                hubUI.RefreshAll();
        }
    }
}
