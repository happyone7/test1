using System.Collections.Generic;
using UnityEngine;

namespace Soulspire.Tower
{
    /// <summary>
    /// LineRenderer 기반 체인 번개 VFX.
    /// 타워 → 첫 번째 타겟 → 체인 타겟들 경로를 지그재그 번개로 시각화합니다.
    /// 짧은 시간(0.15~0.25초) 동안 표시 후 알파 페이드아웃으로 사라집니다.
    /// </summary>
    public class ChainLightningVFX : MonoBehaviour
    {
        [Header("번개 시각 설정")]
        [SerializeField] float _displayDuration = 0.2f;
        [SerializeField] float _startWidth = 0.08f;
        [SerializeField] float _endWidth = 0.03f;
        [SerializeField] int _zigzagSegmentsPerChain = 4;
        [SerializeField] float _zigzagOffset = 0.15f;
        [SerializeField] Color _baseColor = Color.white;
        [SerializeField] int _sortingOrder = 50;

        LineRenderer _lineRenderer;
        float _timer;
        float _duration;
        bool _active;

        void Awake()
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.startWidth = _startWidth;
            _lineRenderer.endWidth = _endWidth;
            _lineRenderer.numCapVertices = 2;
            _lineRenderer.numCornerVertices = 2;
            _lineRenderer.sortingOrder = _sortingOrder;

            // Sprites-Default 머터리얼 사용 (알파 페이드 지원)
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _lineRenderer.startColor = _baseColor;
            _lineRenderer.endColor = _baseColor;

            _lineRenderer.enabled = false;
        }

        /// <summary>
        /// 체인 번개 VFX를 재생합니다.
        /// </summary>
        /// <param name="chainPath">타워 위치 + 체인 타겟 위치 리스트 (최소 2개)</param>
        public void Play(List<Vector3> chainPath)
        {
            if (chainPath == null || chainPath.Count < 2)
            {
                Destroy(gameObject);
                return;
            }

            // 지그재그 포인트 생성
            var points = GenerateZigzagPoints(chainPath);

            _lineRenderer.positionCount = points.Count;
            _lineRenderer.SetPositions(points.ToArray());

            // 색상 그라디언트: 밝은 파란/흰색 → 끝부분 살짝 파랗게
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(0.6f, 0.85f, 1f), 1f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.7f, 1f)
                }
            );
            _lineRenderer.colorGradient = gradient;

            _lineRenderer.enabled = true;
            _duration = Random.Range(0.15f, 0.25f);
            _timer = 0f;
            _active = true;
        }

        /// <summary>
        /// 체인 경로의 각 구간에 지그재그 중간 포인트를 삽입하여 번개 느낌을 줍니다.
        /// </summary>
        List<Vector3> GenerateZigzagPoints(List<Vector3> chainPath)
        {
            var result = new List<Vector3>();

            for (int i = 0; i < chainPath.Count - 1; i++)
            {
                Vector3 start = chainPath[i];
                Vector3 end = chainPath[i + 1];

                result.Add(start);

                Vector3 direction = end - start;
                Vector3 perpendicular = Vector3.Cross(direction.normalized, Vector3.forward).normalized;

                for (int s = 1; s <= _zigzagSegmentsPerChain; s++)
                {
                    float t = (float)s / (_zigzagSegmentsPerChain + 1);
                    Vector3 midPoint = Vector3.Lerp(start, end, t);

                    // 수직 방향으로 랜덤 오프셋 (번개 지그재그)
                    float offset = Random.Range(-_zigzagOffset, _zigzagOffset);
                    midPoint += perpendicular * offset;

                    result.Add(midPoint);
                }
            }

            // 마지막 포인트 추가
            result.Add(chainPath[chainPath.Count - 1]);

            return result;
        }

        void Update()
        {
            if (!_active) return;

            _timer += Time.deltaTime;
            float progress = _timer / _duration;

            if (progress >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            // 알파 페이드아웃: 1 → 0
            float alpha = 1f - progress;

            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(0.6f, 0.85f, 1f), 1f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(alpha, 0f),
                    new GradientAlphaKey(alpha * 0.7f, 1f)
                }
            );
            _lineRenderer.colorGradient = gradient;

            // 폭도 점진적으로 줄이기
            _lineRenderer.startWidth = _startWidth * alpha;
            _lineRenderer.endWidth = _endWidth * alpha;
        }
    }
}
