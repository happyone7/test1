using UnityEngine;
using UnityEngine.UI;
using Nodebreaker.Audio;

namespace Nodebreaker.UI
{
    /// <summary>
    /// 설정 팝업 (PPT 명세 슬라이드 5-3)
    /// 400x300px, Center-Center, 테두리 #5B6B8A 2px
    /// BGM/SFX 슬라이더 + 닫기 버튼
    /// </summary>
    public class SettingsPopup : MonoBehaviour
    {
        [Header("슬라이더")]
        public Slider bgmSlider;
        public Slider sfxSlider;
        public Text bgmValueText;
        public Text sfxValueText;

        [Header("버튼")]
        public Button closeButton;

        void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (bgmSlider != null)
            {
                bgmSlider.minValue = 0f;
                bgmSlider.maxValue = 1f;
                bgmSlider.value = SoundManager.Instance.GetBgmVolume();
                bgmSlider.onValueChanged.AddListener(OnBGMChanged);
                UpdateBGMText(bgmSlider.value);
            }

            if (sfxSlider != null)
            {
                sfxSlider.minValue = 0f;
                sfxSlider.maxValue = 1f;
                sfxSlider.value = SoundManager.Instance.GetSfxVolume();
                sfxSlider.onValueChanged.AddListener(OnSFXChanged);
                UpdateSFXText(sfxSlider.value);
            }

            gameObject.SetActive(false);
        }

        public void Show()
        {
            SoundManager.Instance.PlaySfx(SoundKeys.UiClick, 0.8f);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            SoundManager.Instance.PlaySfx(SoundKeys.UiClick, 0.8f);
            gameObject.SetActive(false);
        }

        private void OnBGMChanged(float value)
        {
            SoundManager.Instance.SetBgmVolume(value);
            UpdateBGMText(value);
        }

        private void OnSFXChanged(float value)
        {
            SoundManager.Instance.SetSfxVolume(value);
            UpdateSFXText(value);
        }

        private void UpdateBGMText(float value)
        {
            if (bgmValueText != null)
                bgmValueText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }

        private void UpdateSFXText(float value)
        {
            if (sfxValueText != null)
                sfxValueText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }
    }
}
