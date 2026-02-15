using UnityEngine;

namespace Nodebreaker.Tower
{
    /// <summary>
    /// 타워 합성(레벨업) 시 파티클 burst + 레벨업 텍스트 팝업을 재생하는 VFX 컨트롤러.
    /// 모든 컴포넌트를 런타임에 동적 생성하므로 프리팹 불필요 (코드 전용).
    /// static Play(Vector3, int) 메서드로 호출.
    /// </summary>
    public class MergeVFX : MonoBehaviour
    {
        // 다크 판타지 색상
        private static readonly Color Emerald = new Color(0.314f, 0.784f, 0.471f, 1f); // #50C878
        private static readonly Color Gold    = new Color(1f, 0.843f, 0f, 1f);          // #FFD700

        private TextMesh _levelText;
        private float _floatSpeed = 1.2f;
        private float _fadeDuration = 0.8f;
        private float _elapsed;
        private Color _startColor;
        private Vector3 _startPos;
        private bool _animating;

        // ---------------------------------------------------------------
        // Public static API
        // ---------------------------------------------------------------

        /// <summary>
        /// 타워 합성 이펙트를 지정 위치에 재생합니다.
        /// </summary>
        /// <param name="position">타워 월드 좌표</param>
        /// <param name="newLevel">합성 후 새 레벨 (표시용)</param>
        public static void Play(Vector3 position, int newLevel)
        {
            var go = new GameObject("MergeVFX");
            go.transform.position = position;
            var vfx = go.AddComponent<MergeVFX>();
            vfx.Init(newLevel);
        }

        // ---------------------------------------------------------------
        // Instance methods
        // ---------------------------------------------------------------

        private void Init(int newLevel)
        {
            // ── 파티클 시스템 생성 ──────────────────────────────
            CreateBurstParticle();

            // ── 레벨업 텍스트 팝업 ──────────────────────────────
            CreateLevelText(newLevel);

            // 2초 후 자동 파괴
            Destroy(gameObject, 2f);
        }

        private void CreateBurstParticle()
        {
            var psGO = new GameObject("BurstParticle");
            psGO.transform.SetParent(transform, false);

            var ps = psGO.AddComponent<ParticleSystem>();

            // Main module
            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.3f;
            main.maxParticles = 40;
            main.playOnAwake = false;

            // 에메랄드 ~ 골드 그라디언트
            var colorGrad = new Gradient();
            colorGrad.SetKeys(
                new[] { new GradientColorKey(Emerald, 0f), new GradientColorKey(Gold, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
            );
            main.startColor = new ParticleSystem.MinMaxGradient(colorGrad);

            // Emission: burst
            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20, 30) });

            // Shape: Circle (2D 방사)
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.15f;

            // Size over Lifetime: 축소
            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            var sizeCurve = new AnimationCurve(
                new Keyframe(0f, 1f), new Keyframe(1f, 0f));
            sol.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            // Color over Lifetime: 페이드아웃
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var fadeGrad = new Gradient();
            fadeGrad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = new ParticleSystem.MinMaxGradient(fadeGrad);

            // Renderer 설정
            var psRenderer = psGO.GetComponent<ParticleSystemRenderer>();
            psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            psRenderer.material = new Material(Shader.Find("Sprites/Default"));
            psRenderer.sortingOrder = 100;

            ps.Play();
        }

        private void CreateLevelText(int newLevel)
        {
            var textGO = new GameObject("LevelText");
            textGO.transform.SetParent(transform, false);
            textGO.transform.localPosition = new Vector3(0f, 0.5f, 0f);

            _levelText = textGO.AddComponent<TextMesh>();
            _levelText.text = $"Lv.{newLevel}!";
            _levelText.fontSize = 48;
            _levelText.characterSize = 0.1f;
            _levelText.anchor = TextAnchor.MiddleCenter;
            _levelText.alignment = TextAlignment.Center;
            _levelText.color = Gold;
            _levelText.fontStyle = FontStyle.Bold;

            // MeshRenderer 소팅
            var textRenderer = textGO.GetComponent<MeshRenderer>();
            if (textRenderer != null)
            {
                textRenderer.sortingOrder = 101;
            }

            _startColor = _levelText.color;
            _startPos = _levelText.transform.localPosition;
            _animating = true;
        }

        private void Update()
        {
            if (!_animating || _levelText == null) return;

            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _fadeDuration);

            // 위로 떠오르기
            _levelText.transform.localPosition = _startPos + Vector3.up * (_floatSpeed * t);

            // 페이드아웃
            Color c = _startColor;
            c.a = Mathf.Lerp(1f, 0f, t);
            _levelText.color = c;

            if (t >= 1f)
            {
                _animating = false;
            }
        }
    }
}
