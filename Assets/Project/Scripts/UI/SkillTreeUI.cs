using System.Collections.Generic;
using Tesseract.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nodebreaker.UI
{
    /// <summary>
    /// 스킬트리 줌/패닝 + 노드 배치 + 연결선 관리 컨트롤러.
    /// ScrollRect 위에 대형 RectTransform(Content)을 배치하여 패닝하고,
    /// Content의 localScale로 줌을 구현한다.
    /// </summary>
    public class SkillTreeUI : MonoBehaviour, IScrollHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [Header("컨테이너")]
        [Tooltip("줌/패닝 대상이 되는 트리 콘텐츠 RectTransform")]
        public RectTransform treeContent;

        [Tooltip("스크롤 뷰포트 (마스킹 영역)")]
        public RectTransform viewport;

        [Header("줌 설정")]
        public float minZoom = 0.4f;
        public float maxZoom = 1.5f;
        public float zoomStep = 0.1f;
        public float zoomSpeed = 8f;

        [Header("패닝 설정")]
        public float keyboardPanSpeed = 500f;
        public float panInertia = 0.3f;

        [Header("그리드 설정")]
        [Tooltip("그리드 1칸의 픽셀 크기 (노드 크기 + 여백)")]
        public float gridCellSize = 180f;

        [Header("노드")]
        public SkillNodeUI[] skillNodes;

        [Header("연결선 프리팹")]
        public GameObject connectionLinePrefab;

        [Header("연결선 컨테이너")]
        public RectTransform connectionContainer;

        private float _currentZoom = 1f;
        private float _targetZoom = 1f;
        private Vector2 _panVelocity;
        private Vector2 _dragStartPos;
        private bool _isDragging;
        private bool _wasDragSignificant; // 드래그 5px 이상 여부
        private Vector2 _lastDragDelta;
        private List<SkillTreeConnectionLine> _connectionLines = new List<SkillTreeConnectionLine>();

        void Start()
        {
            _currentZoom = 1f;
            _targetZoom = 1f;

            LayoutNodes();
            CreateConnectionLines();
            CenterOnNode("N00"); // Core Node 중앙 배치
        }

        void Update()
        {
            // 줌 보간
            if (!Mathf.Approximately(_currentZoom, _targetZoom))
            {
                _currentZoom = Mathf.Lerp(_currentZoom, _targetZoom, Time.unscaledDeltaTime * zoomSpeed);
                if (Mathf.Abs(_currentZoom - _targetZoom) < 0.001f)
                    _currentZoom = _targetZoom;
                ApplyZoom();
            }

            // 키보드 패닝 (WASD + 화살표)
            Vector2 keyInput = Vector2.zero;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) keyInput.y -= 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) keyInput.y += 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) keyInput.x += 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) keyInput.x -= 1f;

            if (keyInput != Vector2.zero)
            {
                var delta = keyInput * (keyboardPanSpeed * Time.unscaledDeltaTime / _currentZoom);
                treeContent.anchoredPosition += delta;
                _panVelocity = Vector2.zero; // 키보드 입력 시 관성 제거
            }

            // 키보드 줌 (+/-)
            if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
                SetTargetZoom(_targetZoom + zoomStep);
            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
                SetTargetZoom(_targetZoom - zoomStep);

            // 관성 패닝
            if (!_isDragging && _panVelocity.sqrMagnitude > 0.1f)
            {
                treeContent.anchoredPosition += _panVelocity * Time.unscaledDeltaTime;
                _panVelocity = Vector2.Lerp(_panVelocity, Vector2.zero, Time.unscaledDeltaTime / panInertia);
            }

            ClampPosition();
        }

        #region IScrollHandler - 줌

        public void OnScroll(PointerEventData eventData)
        {
            float scrollDelta = eventData.scrollDelta.y;
            if (scrollDelta > 0)
                SetTargetZoom(_targetZoom + zoomStep);
            else if (scrollDelta < 0)
                SetTargetZoom(_targetZoom - zoomStep);
        }

        #endregion

        #region IDragHandler - 패닝 (우클릭 또는 좌클릭 드래그)

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
            _wasDragSignificant = false;
            _dragStartPos = eventData.position;
            _panVelocity = Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // 좌클릭 또는 우클릭 모두 패닝 허용
            Vector2 delta = eventData.delta / _currentZoom;
            treeContent.anchoredPosition += delta;
            _lastDragDelta = delta;

            // 드래그 거리 5px 이상이면 '의미 있는 드래그'
            if (Vector2.Distance(eventData.position, _dragStartPos) > 5f)
                _wasDragSignificant = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            // 관성 계산
            _panVelocity = _lastDragDelta / Time.unscaledDeltaTime * 0.5f;
        }

        #endregion

        /// <summary>
        /// 드래그가 5px 미만이었는지 여부 (노드 클릭 판정용)
        /// </summary>
        public bool WasDragSignificant => _wasDragSignificant;

        private void SetTargetZoom(float zoom)
        {
            _targetZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        }

        private void ApplyZoom()
        {
            if (treeContent != null)
                treeContent.localScale = Vector3.one * _currentZoom;
        }

        private void ClampPosition()
        {
            if (treeContent == null || viewport == null) return;

            // 트리 콘텐츠의 바운더리 계산 후 뷰포트 내 유지
            Vector2 contentSize = treeContent.sizeDelta * _currentZoom;
            Vector2 viewSize = viewport.rect.size;
            float margin = 200f * _currentZoom;

            Vector2 pos = treeContent.anchoredPosition;
            float maxX = contentSize.x * 0.5f + margin;
            float maxY = contentSize.y * 0.5f + margin;

            pos.x = Mathf.Clamp(pos.x, -maxX, maxX);
            pos.y = Mathf.Clamp(pos.y, -maxY, maxY);
            treeContent.anchoredPosition = pos;
        }

        /// <summary>
        /// 기획서 그리드 좌표에 따라 모든 노드를 배치한다.
        /// </summary>
        public void LayoutNodes()
        {
            if (skillNodes == null) return;

            foreach (var node in skillNodes)
            {
                if (node == null || node.data == null) continue;

                var rt = node.GetComponent<RectTransform>();
                if (rt == null) continue;

                // 그리드 좌표 -> 픽셀 위치 (Y축 반전: 위가 양수이므로 Y에 -1 곱하지 않음)
                Vector2 pos = new Vector2(
                    node.data.gridPosition.x * gridCellSize,
                    node.data.gridPosition.y * gridCellSize
                );
                rt.anchoredPosition = pos;
            }
        }

        /// <summary>
        /// SkillNodeData의 connectedNodeIds에 따라 연결선을 생성한다.
        /// </summary>
        public void CreateConnectionLines()
        {
            // 기존 연결선 제거
            foreach (var line in _connectionLines)
            {
                if (line != null)
                    Destroy(line.gameObject);
            }
            _connectionLines.Clear();

            if (connectionLinePrefab == null || connectionContainer == null) return;

            // 노드 ID -> SkillNodeUI 매핑
            var nodeMap = new Dictionary<string, SkillNodeUI>();
            foreach (var node in skillNodes)
            {
                if (node != null && node.data != null)
                    nodeMap[node.data.skillId] = node;
            }

            // 각 노드의 connectedNodeIds에 따라 연결선 생성
            foreach (var node in skillNodes)
            {
                if (node == null || node.data == null) continue;
                if (node.data.connectedNodeIds == null) continue;

                foreach (var targetId in node.data.connectedNodeIds)
                {
                    if (string.IsNullOrEmpty(targetId)) continue;
                    if (!nodeMap.ContainsKey(targetId)) continue;

                    var targetNode = nodeMap[targetId];
                    var lineGo = Instantiate(connectionLinePrefab, connectionContainer);
                    var line = lineGo.GetComponent<SkillTreeConnectionLine>();
                    if (line != null)
                    {
                        line.Setup(node, targetNode);
                        _connectionLines.Add(line);
                    }
                }
            }
        }

        /// <summary>
        /// 특정 노드 중심으로 뷰 이동
        /// </summary>
        public void CenterOnNode(string nodeId)
        {
            foreach (var node in skillNodes)
            {
                if (node == null || node.data == null) continue;
                if (node.data.skillId == nodeId)
                {
                    var rt = node.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        treeContent.anchoredPosition = -rt.anchoredPosition;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// 모든 노드와 연결선의 상태를 갱신한다.
        /// </summary>
        public void RefreshAll()
        {
            if (skillNodes != null)
            {
                foreach (var node in skillNodes)
                {
                    if (node != null)
                        node.Refresh();
                }
            }

            foreach (var line in _connectionLines)
            {
                if (line != null)
                    line.Refresh();
            }
        }
    }
}
