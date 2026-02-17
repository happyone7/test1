#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using Tesseract.Core;

namespace Soulspire.Debugging
{
    /// <summary>
    /// F12 키로 디버그 패널을 토글하고,
    /// 콘솔 에러/워닝 카운터 오버레이를 항상 표시하는 매니저.
    /// </summary>
    public class DebugManager : Singleton<DebugManager>
    {
        // ── 패널 토글 ──
        bool _panelVisible;
        public bool PanelVisible => _panelVisible;

        // ── 콘솔 카운터 오버레이 ──
        int _errorCount;
        int _warningCount;
        int _logCount;
        float _flashTimer;
        bool _flashOn;
        int _lastErrorCount;

        // 오버레이 스타일
        GUIStyle _overlayStyle;
        GUIStyle _overlayFlashStyle;
        bool _stylesInitialized;

        // 오버레이 위치 (우상단)
        const float OverlayWidth = 220f;
        const float OverlayHeight = 60f;
        const float OverlayMargin = 10f;

        protected override void Awake()
        {
            base.Awake();
            _errorCount = 0;
            _warningCount = 0;
            _logCount = 0;
            _lastErrorCount = 0;
        }

        void OnEnable()
        {
            Application.logMessageReceived += OnLogMessageReceived;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }

        void Update()
        {
            // F12 토글
            if (Input.GetKeyDown(KeyCode.F12))
            {
                _panelVisible = !_panelVisible;
                Debug.Log($"[DebugCmd] 디버그 패널 {(_panelVisible ? "열림" : "닫힘")}");
            }

            // 에러 깜빡임 처리
            if (_errorCount > _lastErrorCount)
            {
                _flashTimer = 2f; // 새 에러 발생 시 2초간 깜빡임
                _lastErrorCount = _errorCount;
            }

            if (_flashTimer > 0f)
            {
                _flashTimer -= Time.unscaledDeltaTime;
                // 0.3초 간격으로 깜빡임
                _flashOn = (Mathf.FloorToInt(_flashTimer / 0.3f) % 2) == 0;
            }
            else
            {
                _flashOn = false;
            }
        }

        void OnGUI()
        {
            InitStyles();

            // 콘솔 카운터 오버레이 (패널 닫혀도 항상 표시)
            DrawConsoleOverlay();
        }

        // ── 콘솔 카운터 오버레이 ──

        void DrawConsoleOverlay()
        {
            float x = Screen.width - OverlayWidth - OverlayMargin;
            float y = OverlayMargin;
            Rect rect = new Rect(x, y, OverlayWidth, OverlayHeight);

            // 배경
            GUI.Box(rect, "");

            // 에러가 있고 깜빡임 중이면 빨간 배경
            if (_errorCount > 0 && _flashOn)
            {
                var flashTex = new Texture2D(1, 1);
                flashTex.SetPixel(0, 0, new Color(1f, 0f, 0f, 0.4f));
                flashTex.Apply();
                GUI.DrawTexture(rect, flashTex);
            }

            GUILayout.BeginArea(rect);
            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            GUILayout.Space(8);

            // Error
            var prevColor = GUI.color;
            GUI.color = _errorCount > 0 ? Color.red : Color.white;
            GUILayout.Label($"E:{_errorCount}", _overlayStyle, GUILayout.Width(60));

            // Warning
            GUI.color = _warningCount > 0 ? Color.yellow : Color.white;
            GUILayout.Label($"W:{_warningCount}", _overlayStyle, GUILayout.Width(60));

            // Log
            GUI.color = Color.white;
            GUILayout.Label($"L:{_logCount}", _overlayStyle, GUILayout.Width(60));

            GUI.color = prevColor;
            GUILayout.EndHorizontal();

            // 리셋 버튼
            GUILayout.BeginHorizontal();
            GUILayout.Space(8);
            if (GUILayout.Button("Reset Counts", GUILayout.Width(100)))
            {
                _errorCount = 0;
                _warningCount = 0;
                _logCount = 0;
                _lastErrorCount = 0;
                _flashTimer = 0f;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                    _errorCount++;
                    break;
                case LogType.Warning:
                    _warningCount++;
                    break;
                case LogType.Log:
                    _logCount++;
                    break;
            }
        }

        void InitStyles()
        {
            if (_stylesInitialized) return;
            _stylesInitialized = true;

            _overlayStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };

            _overlayFlashStyle = new GUIStyle(_overlayStyle);
            _overlayFlashStyle.normal.textColor = Color.red;
        }

        // ── 공개 API ──

        public void TogglePanel()
        {
            _panelVisible = !_panelVisible;
        }

        public int ErrorCount => _errorCount;
        public int WarningCount => _warningCount;
        public int LogCount => _logCount;
    }
}
#endif
