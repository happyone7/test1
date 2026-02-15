using System;
using Tesseract.Core;
using UnityEngine;

namespace Nodebreaker.Core
{
    /// <summary>
    /// 게임 배속 제어 (x1/x2/x3).
    /// UI나 다른 시스템에서 일시정지(TimeScale=0)를 요청할 수 있으며,
    /// 일시정지 해제 시 이전 배속으로 자동 복귀합니다.
    /// </summary>
    public class SpeedController : Singleton<SpeedController>
    {
        private static readonly int[] SpeedLevels = { 1, 2, 3 };

        private int _speedIndex; // 0=x1, 1=x2, 2=x3
        private bool _isPaused;

        /// <summary>현재 배속 (1, 2, 3)</summary>
        public int CurrentSpeed => SpeedLevels[_speedIndex];

        /// <summary>현재 일시정지 여부</summary>
        public bool IsPaused => _isPaused;

        /// <summary>배속 변경 시 발생. 인자: 새 배속 값 (1, 2, 3)</summary>
        public event Action<int> OnSpeedChanged;

        /// <summary>일시정지 상태 변경 시 발생. 인자: isPaused</summary>
        public event Action<bool> OnPauseChanged;

        protected override void Awake()
        {
            base.Awake();
            _speedIndex = 0;
            _isPaused = false;
        }

        /// <summary>
        /// 다음 배속으로 순환 토글 (1 -> 2 -> 3 -> 1).
        /// 일시정지 중이면 일시정지를 해제하고 다음 배속을 적용합니다.
        /// </summary>
        public void ToggleSpeed()
        {
            _speedIndex = (_speedIndex + 1) % SpeedLevels.Length;

            if (_isPaused)
            {
                _isPaused = false;
                OnPauseChanged?.Invoke(false);
            }

            ApplyTimeScale();
            OnSpeedChanged?.Invoke(CurrentSpeed);
        }

        /// <summary>
        /// 특정 배속으로 직접 설정 (1, 2, 3).
        /// 일시정지 중이면 일시정지를 해제합니다.
        /// </summary>
        public void SetSpeed(int speed)
        {
            int index = Array.IndexOf(SpeedLevels, speed);
            if (index < 0)
            {
                Debug.LogWarning($"[SpeedController] 유효하지 않은 배속: {speed}. 무시합니다.");
                return;
            }

            _speedIndex = index;

            if (_isPaused)
            {
                _isPaused = false;
                OnPauseChanged?.Invoke(false);
            }

            ApplyTimeScale();
            OnSpeedChanged?.Invoke(CurrentSpeed);
        }

        /// <summary>
        /// 일시정지 토글. 보물상자 선택 등 UI 표시 중 호출.
        /// </summary>
        public void TogglePause()
        {
            _isPaused = !_isPaused;
            ApplyTimeScale();
            OnPauseChanged?.Invoke(_isPaused);
        }

        /// <summary>
        /// 강제 일시정지 설정 (보물상자 UI 등에서 사용).
        /// </summary>
        public void SetPaused(bool paused)
        {
            if (_isPaused == paused) return;
            _isPaused = paused;
            ApplyTimeScale();
            OnPauseChanged?.Invoke(_isPaused);
        }

        /// <summary>
        /// 배속을 x1로 리셋하고 일시정지 해제.
        /// 런 종료/허브 복귀 시 호출.
        /// </summary>
        public void ResetToDefault()
        {
            _speedIndex = 0;
            _isPaused = false;
            Time.timeScale = 1f;
            OnSpeedChanged?.Invoke(CurrentSpeed);
            OnPauseChanged?.Invoke(false);
        }

        private void ApplyTimeScale()
        {
            Time.timeScale = _isPaused ? 0f : SpeedLevels[_speedIndex];
        }
    }
}
