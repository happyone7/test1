using Tesseract.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Nodebreaker.UI
{
    /// <summary>
    /// 타이틀 화면 UI 컨트롤러.
    /// 시작/설정/종료 버튼 처리 및 호버/클릭 인터랙션.
    /// </summary>
    public class TitleScreenUI : MonoBehaviour
    {
        [Header("버튼")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        // 버튼 테두리 색상 (호버 시 발광 강도 +50%)
        private static readonly Color StartBorderNormal = HexColor("#2BFF88");
        private static readonly Color StartBorderHover = HexColor("#5EFFA6"); // +50% brightness
        private static readonly Color SettingsBorderNormal = HexColor("#37B6FF");
        private static readonly Color SettingsBorderHover = HexColor("#6ED0FF");
        private static readonly Color QuitBorderNormal = HexColor("#FF4D5A");
        private static readonly Color QuitBorderHover = HexColor("#FF8088");

        private static readonly Color FillNormal = HexColor("#121A2A");
        private static readonly Color FillHover = HexColor("#1A243A");

        void Start()
        {
            if (startButton != null)
                startButton.onClick.AddListener(OnStartClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            // 버튼 인터랙션 이벤트 설정
            SetupButtonInteraction(startButton, StartBorderNormal, StartBorderHover);
            SetupButtonInteraction(settingsButton, SettingsBorderNormal, SettingsBorderHover);
            SetupButtonInteraction(quitButton, QuitBorderNormal, QuitBorderHover);
        }

        private void OnStartClicked()
        {
            if (Singleton<Core.GameManager>.HasInstance)
            {
                Core.GameManager.Instance.GoToHub();
                Hide();
            }
            else
            {
                Debug.LogWarning("[TitleScreenUI] GameManager not found.");
            }
        }

        private void OnSettingsClicked()
        {
            // TODO: 설정 팝업 구현
            Debug.Log("[TitleScreenUI] Settings button clicked - not yet implemented.");
        }

        private void OnQuitClicked()
        {
            Debug.Log("[TitleScreenUI] Quit button clicked.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 버튼에 호버/클릭 인터랙션 이벤트 설정.
        /// 호버 시 테두리 발광 +50%, 채우기 밝아짐.
        /// 클릭 시 0.1초간 0.95배 축소.
        /// </summary>
        private void SetupButtonInteraction(Button button, Color borderNormal, Color borderHover)
        {
            if (button == null) return;

            var trigger = button.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = button.gameObject.AddComponent<EventTrigger>();

            // 버튼의 Outline (테두리) 참조
            var outline = button.GetComponent<Outline>();
            var buttonImage = button.GetComponent<Image>();

            // Pointer Enter (호버)
            var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enterEntry.callback.AddListener((_) =>
            {
                if (outline != null) outline.effectColor = borderHover;
                if (buttonImage != null) buttonImage.color = FillHover;
            });
            trigger.triggers.Add(enterEntry);

            // Pointer Exit
            var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exitEntry.callback.AddListener((_) =>
            {
                if (outline != null) outline.effectColor = borderNormal;
                if (buttonImage != null) buttonImage.color = FillNormal;
            });
            trigger.triggers.Add(exitEntry);

            // Pointer Down (클릭 축소)
            var downEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            downEntry.callback.AddListener((_) =>
            {
                button.transform.localScale = Vector3.one * 0.95f;
            });
            trigger.triggers.Add(downEntry);

            // Pointer Up (복원)
            var upEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            upEntry.callback.AddListener((_) =>
            {
                button.transform.localScale = Vector3.one;
            });
            trigger.triggers.Add(upEntry);
        }

        /// <summary>
        /// 헥스 색상 문자열을 Color로 변환.
        /// </summary>
        private static Color HexColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
                return color;
            return Color.white;
        }
    }
}
