using System;
using Tesseract.Core;
using UnityEngine;

namespace Soulspire.Core
{
    /// <summary>
    /// 게임 배속 및 일시정지를 관리하는 싱글톤.
    /// InGameUI에서 참조하며, 스킬트리 SpeedControl 해금 후 x2/x3 사용 가능.
    /// </summary>
    public class SpeedController : Singleton<SpeedController>
    {
        private static readonly int[] SpeedValues = { 1, 2, 3 };

        private int _speedIndex; // 0=x1, 1=x2, 2=x3
        private bool _isPaused;

        /// <summary>현재 배속 (1, 2, 3)</summary>
        public int CurrentSpeed => SpeedValues[_speedIndex];

        /// <summary>일시정지 여부</summary>
        public bool IsPaused => _isPaused;

        /// <summary>배속 변경 시 발생. 인자: 새 배속 값</summary>
        public event Action<int> OnSpeedChanged;

        /// <summary>일시정지 상태 변경 시 발생. 인자: isPaused</summary>
        public event Action<bool> OnPauseChanged;

        /// <summary>
        /// 지정 배속으로 설정합니다.
        /// </summary>
        public void SetSpeed(int speed)
        {
            int index = System.Array.IndexOf(SpeedValues, speed);
            if (index < 0) return;

            _speedIndex = index;
            _isPaused = false;
            ApplyTimeScale();
            OnSpeedChanged?.Invoke(CurrentSpeed);
            OnPauseChanged?.Invoke(_isPaused);
        }

        /// <summary>
        /// 배속을 순환합니다 (x1 -> x2 -> x3 -> x1).
        /// </summary>
        public void ToggleSpeed()
        {
            _speedIndex = (_speedIndex + 1) % SpeedValues.Length;
            _isPaused = false;
            ApplyTimeScale();
            OnSpeedChanged?.Invoke(CurrentSpeed);
            OnPauseChanged?.Invoke(_isPaused);
        }

        /// <summary>
        /// 일시정지를 토글합니다.
        /// </summary>
        public void TogglePause()
        {
            _isPaused = !_isPaused;
            ApplyTimeScale();
            OnPauseChanged?.Invoke(_isPaused);
        }

        /// <summary>
        /// 배속을 x1으로 리셋합니다.
        /// </summary>
        public void ResetToDefault()
        {
            _speedIndex = 0;
            _isPaused = false;
            ApplyTimeScale();
            OnSpeedChanged?.Invoke(CurrentSpeed);
            OnPauseChanged?.Invoke(_isPaused);
        }

        private void ApplyTimeScale()
        {
            Time.timeScale = _isPaused ? 0f : CurrentSpeed;
        }
    }
}
