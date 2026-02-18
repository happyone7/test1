using System.Collections;
using Tesseract.Core;
using UnityEngine;

namespace Soulspire.Core
{
    /// <summary>
    /// 도파민 연출 매니저. 카메라 셰이크, 슬로우모션, 데미지/Soul 텍스트 팝업을 관리합니다.
    /// Singleton으로 구현되며 DontDestroyOnLoad입니다.
    /// </summary>
    public class FeedbackManager : Singleton<FeedbackManager>
    {
        [Header("카메라 셰이크")]
        [Tooltip("셰이크 적용 대상 카메라. null이면 Camera.main 사용.")]
        public Camera targetCamera;

        [Header("텍스트 팝업")]
        [Tooltip("데미지 텍스트 프리팹 (FloatingText 컴포넌트 필요). null이면 런타임 자동 생성.")]
        public GameObject floatingTextPrefab;

        // 카메라 셰이크 상태
        Camera _cam;
        Vector3 _camOriginalPos;
        Coroutine _shakeCoroutine;

        // 슬로우모션 상태
        Coroutine _slowMotionCoroutine;
        float _originalFixedDeltaTime;

        protected override void Awake()
        {
            base.Awake();
            _originalFixedDeltaTime = Time.fixedDeltaTime;
        }

        void Start()
        {
            CacheCamera();
        }

        void CacheCamera()
        {
            if (targetCamera != null)
            {
                _cam = targetCamera;
            }
            else
            {
                _cam = Camera.main;
            }

            if (_cam != null)
            {
                _camOriginalPos = _cam.transform.position;
            }
            else
            {
                Debug.LogWarning("[FeedbackManager] Camera.main을 찾을 수 없음. 카메라 셰이크가 작동하지 않습니다.");
            }
        }

        // ═══════════════════════════════════════
        // 카메라 셰이크
        // ═══════════════════════════════════════

        /// <summary>
        /// 카메라를 intensity 크기로 duration 초 동안 흔듭니다.
        /// Transform 오프셋 기반으로 동작하며, 종료 후 원위치로 복귀합니다.
        /// </summary>
        /// <param name="intensity">흔들림 크기 (0.1 = 약간, 0.3 = 강하게)</param>
        /// <param name="duration">흔들림 지속 시간 (초)</param>
        public void ShakeCamera(float intensity, float duration)
        {
            if (_cam == null)
            {
                CacheCamera();
                if (_cam == null) return;
            }

            // 진행 중인 셰이크가 있으면 중단하고 원위치 복귀
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                _cam.transform.position = _camOriginalPos;
            }

            _camOriginalPos = new Vector3(
                _cam.transform.position.x,
                _cam.transform.position.y,
                _cam.transform.position.z
            );
            _shakeCoroutine = StartCoroutine(ShakeCameraCoroutine(intensity, duration));
        }

        IEnumerator ShakeCameraCoroutine(float intensity, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                // unscaledDeltaTime 사용 (슬로우모션 중에도 정상 동작)
                elapsed += Time.unscaledDeltaTime;

                // 감쇠: 시간이 지남에 따라 흔들림 줄어듦
                float currentIntensity = intensity * (1f - (elapsed / duration));

                float offsetX = Random.Range(-currentIntensity, currentIntensity);
                float offsetY = Random.Range(-currentIntensity, currentIntensity);

                _cam.transform.position = new Vector3(
                    _camOriginalPos.x + offsetX,
                    _camOriginalPos.y + offsetY,
                    _camOriginalPos.z
                );

                yield return null;
            }

            _cam.transform.position = _camOriginalPos;
            _shakeCoroutine = null;
        }

        // ═══════════════════════════════════════
        // TimeScale 슬로우모션
        // ═══════════════════════════════════════

        /// <summary>
        /// TimeScale을 일시적으로 낮춥니다. duration(실시간 초) 후 1.0으로 복원됩니다.
        /// 이미 슬로우 중이면 새 요청이 덮어씁니다.
        /// </summary>
        /// <param name="timeScale">목표 timeScale (예: 0.2)</param>
        /// <param name="duration">실시간 지속 시간 (초)</param>
        public void SlowMotion(float timeScale, float duration)
        {
            // 진행 중인 슬로우가 있으면 중단
            if (_slowMotionCoroutine != null)
            {
                StopCoroutine(_slowMotionCoroutine);
                RestoreTimeScale();
            }

            _slowMotionCoroutine = StartCoroutine(SlowMotionCoroutine(timeScale, duration));
        }

        IEnumerator SlowMotionCoroutine(float targetTimeScale, float duration)
        {
            Time.timeScale = Mathf.Clamp(targetTimeScale, 0.01f, 1f);
            Time.fixedDeltaTime = _originalFixedDeltaTime * Time.timeScale;

            yield return new WaitForSecondsRealtime(duration);

            RestoreTimeScale();
            _slowMotionCoroutine = null;
        }

        void RestoreTimeScale()
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = _originalFixedDeltaTime;
        }

        // ═══════════════════════════════════════
        // 데미지/Soul 텍스트 팝업
        // ═══════════════════════════════════════

        /// <summary>
        /// 데미지 텍스트를 월드 좌표에 생성합니다. 위로 떠오르며 페이드아웃됩니다.
        /// </summary>
        public void SpawnDamageText(Vector3 position, int damage, Color color)
        {
            SpawnFloatingText(position, damage.ToString(), color);
        }

        /// <summary>
        /// Soul 획득 텍스트를 월드 좌표에 생성합니다. 노란색으로 표시됩니다.
        /// </summary>
        public void SpawnSoulText(Vector3 position, int amount)
        {
            Color soulColor = new Color(1f, 0.85f, 0.2f, 1f); // 황금색
            SpawnFloatingText(position, $"+{amount} Soul", soulColor);
        }

        /// <summary>
        /// 콤보 텍스트를 화면 중앙 상단에 표시합니다. 크기 1.2배, 노란색.
        /// ComboSystem에서 5+ 콤보 시 호출됩니다.
        /// </summary>
        public void SpawnComboText(string text)
        {
            var cam = Camera.main;
            if (cam == null) return;
            Vector3 worldPos = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.85f, 10f));
            SpawnFloatingText(worldPos, text, Color.yellow, 1.2f);
        }

        /// <summary>
        /// 플로팅 텍스트를 월드 좌표에 생성합니다. 내부 공용 메서드.
        /// SpawnDamageText, SpawnSoulText, SpawnComboText가 공유합니다.
        /// </summary>
        /// <param name="worldPosition">텍스트가 표시될 월드 좌표</param>
        /// <param name="text">표시할 텍스트</param>
        /// <param name="color">텍스트 색상</param>
        /// <param name="scale">텍스트 크기 배율 (기본 1.0)</param>
        void SpawnFloatingText(Vector3 worldPosition, string text, Color color, float scale = 1f)
        {
            if (floatingTextPrefab != null)
            {
                // 프리팹이 할당된 경우 프리팹에서 생성
                var go = Instantiate(floatingTextPrefab, worldPosition, Quaternion.identity);
                if (scale != 1f)
                    go.transform.localScale *= scale;
                var ft = go.GetComponent<FloatingText>();
                if (ft != null)
                {
                    ft.Initialize(text, color);
                }
            }
            else
            {
                // 프리팹이 없으면 런타임에서 동적으로 생성
                CreateRuntimeFloatingText(worldPosition, text, color, scale);
            }
        }

        /// <summary>
        /// 프리팹 없이 런타임에서 World Space Canvas + TextMesh를 생성합니다.
        /// 프로토타입/개발 단계에서 프리팹 없이도 동작하도록 합니다.
        /// </summary>
        void CreateRuntimeFloatingText(Vector3 worldPosition, string text, Color color, float scale = 1f)
        {
            var go = new GameObject("[FloatingText]");
            go.transform.position = worldPosition;

            var textMesh = go.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.color = color;
            textMesh.characterSize = 0.15f * scale;
            textMesh.fontSize = 48;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;

            // 정렬 순서를 높여서 스프라이트 위에 표시
            var meshRenderer = go.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.sortingOrder = 100;
            }

            var ft = go.AddComponent<FloatingText>();
            ft.Initialize(text, color);
        }
    }
}
