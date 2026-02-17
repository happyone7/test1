using UnityEngine;

namespace Soulspire.Core
{
    public class GameCamera : MonoBehaviour
    {
        [Header("2D 설정")]
        public float depth = -10f;
        public float orthographicSize = 8f;

        Camera _cam;

        void Awake()
        {
            _cam = GetComponent<Camera>();
            if (_cam != null)
            {
                _cam.orthographic = true;
                _cam.orthographicSize = orthographicSize;
            }
            // 표준 2D 카메라: XY 평면을 바라봄 (-Z 방향)
            transform.position = new Vector3(0, 0, depth);
            transform.rotation = Quaternion.identity;
        }
    }
}
