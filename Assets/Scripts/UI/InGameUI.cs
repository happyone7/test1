using Tesseract.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Nodebreaker.UI
{
    public class InGameUI : MonoBehaviour
    {
        [Header("상단 HUD")]
        public Text waveText;
        public Text bitText;
        public Text baseHpText;
        public Slider baseHpSlider;

        [Header("하단 타워 인벤토리")]
        public Transform towerListParent;

        [Header("버튼")]
        public Button startRunButton;
        public Button speedButton;

        [Header("런 종료 패널")]
        public GameObject runEndPanel;
        public Text runEndTitleText;
        public Text runEndBitText;
        public Button hubButton;
        public Button retryButton;

        void Start()
        {
            if (startRunButton != null)
                startRunButton.onClick.AddListener(OnStartRun);
            if (hubButton != null)
                hubButton.onClick.AddListener(OnGoToHub);
            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetry);

            if (runEndPanel != null)
                runEndPanel.SetActive(false);
        }

        void Update()
        {
            if (!Singleton<Core.RunManager>.HasInstance) return;
            var run = Core.RunManager.Instance;

            if (waveText != null)
            {
                int total = run.CurrentStage != null ? run.CurrentStage.waves.Length : 0;
                waveText.text = $"Wave {run.CurrentWaveIndex + 1}/{total}";
            }

            if (bitText != null)
                bitText.text = $"Bit: {run.BitEarned}";

            if (baseHpText != null)
                baseHpText.text = $"HP: {run.BaseHp}/{run.BaseMaxHp}";

            if (baseHpSlider != null)
            {
                baseHpSlider.maxValue = run.BaseMaxHp;
                baseHpSlider.value = run.BaseHp;
            }
        }

        public void ShowRunEnd(bool cleared, int bitEarned)
        {
            if (runEndPanel != null) runEndPanel.SetActive(true);
            if (runEndTitleText != null)
                runEndTitleText.text = cleared ? "STAGE CLEAR!" : "DEFEATED";
            if (runEndBitText != null)
                runEndBitText.text = $"+{bitEarned} Bit";
        }

        void OnStartRun()
        {
            if (Singleton<Core.GameManager>.HasInstance)
                Core.GameManager.Instance.StartRun();
        }

        void OnGoToHub()
        {
            if (runEndPanel != null) runEndPanel.SetActive(false);
            if (Singleton<Core.GameManager>.HasInstance)
                Core.GameManager.Instance.GoToHub();
        }

        void OnRetry()
        {
            if (runEndPanel != null) runEndPanel.SetActive(false);
            if (Singleton<Core.GameManager>.HasInstance)
                Core.GameManager.Instance.StartRun();
        }
    }
}
