using UnityEngine;

namespace Nodebreaker.Core
{
    public class GameCamera : MonoBehaviour
    {
        [Header("2D 탑다운 설정")]
        public float height = 10f;
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
            // 위에서 내려다보는 시점
            transform.position = new Vector3(0, height, 0);
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
