using UnityEngine;

namespace StrategyDemo.Core
{
    /// <summary>
    /// Board camera control: smooth mouse-wheel zoom (toward the cursor) and middle-mouse drag-pan.
    /// All input comes from the central <see cref="InputReader"/>. Zoom eases the orthographic size
    /// with <see cref="Mathf.SmoothDamp"/>; drag-pan grabs the world point under the cursor so the map
    /// follows the pointer. After either, the camera is clamped so its view never leaves the board
    /// (<see cref="GridManager.WorldBounds"/>); on an axis the board is smaller than the view, it
    /// centres there.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CameraController : MonoBehaviour
    {
        [SerializeField] private InputReader _input;
        [SerializeField] private Camera _camera;

        [Header("Zoom")]
        // Min is intentionally well below the board's half-height: only when the view is narrower than
        // the board (which, on a wide screen, needs a fairly close zoom) does horizontal panning open
        // up. Without this the whole board width always fits and the camera is forced to centre on X.
        [SerializeField] private float _minSize = 2f;   // most zoomed-in
        [SerializeField] private float _maxSize = 7f;    // most zoomed-out (a bit past the full board)
        [SerializeField] private float _stepPerNotch = 0.75f;
        [SerializeField] private float _smoothTime = 0.12f;
        [SerializeField] private bool _zoomToCursor = true;

        [Header("Pan")]
        [SerializeField] private bool _dragPan = true;
        [Tooltip("Extra horizontal pan past the board, as a fraction of the view half-width, so the player " +
                 "can see board hidden behind the side panels. Only applies while zoomed in enough to pan.")]
        [SerializeField, Range(0f, 1f)] private float _horizontalOverscroll = 0.5f;

        private float _targetSize;
        private float _homeSize; // the starting (default) framing, restored by ResetView
        private float _velocity;
        private bool _isPanning;
        private Vector2 _panGrabWorld; // world point grabbed when the drag started

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = GetComponent<Camera>();
            }

            _targetSize = Mathf.Clamp(_camera.orthographicSize, _minSize, _maxSize);
            _homeSize = _targetSize;
        }

        /// <summary>The default (starting) orthographic size — the framing <see cref="ResetView"/> returns to.</summary>
        public float HomeSize => _homeSize;

        /// <summary>
        /// Eases the camera back to the default framing (and, via the clamp, recentres on the board).
        /// Bound to the Space key so a reviewer who pans/zooms away can always get back to a clean view.
        /// </summary>
        public void ResetView()
        {
            _targetSize = _homeSize;
        }

        private void Update()
        {
            if (_input != null && _input.ResetViewPressed)
            {
                ResetView();
            }

            if (_input != null && !Mathf.Approximately(_input.ZoomDelta, 0f))
            {
                // Scroll up (positive) zooms in, which means a smaller orthographic size.
                _targetSize = Mathf.Clamp(_targetSize - _input.ZoomDelta * _stepPerNotch, _minSize, _maxSize);
            }

            float oldSize = _camera.orthographicSize;
            float newSize = Mathf.SmoothDamp(oldSize, _targetSize, ref _velocity, _smoothTime);
            _camera.orthographicSize = newSize;

            Vector3 position = transform.position;
            position = ApplyZoomToCursor(position, oldSize, newSize);
            position = ApplyDragPan(position);
            transform.position = ClampToBoard(position, newSize);
        }

        // Keep the world point under the cursor fixed as the size changes: solving
        // cursor + k*(camera - cursor) == newCamera gives camera = lerp(cursor, camera, k).
        private Vector3 ApplyZoomToCursor(Vector3 position, float oldSize, float newSize)
        {
            if (!_zoomToCursor || _input == null || oldSize <= 0.0001f)
            {
                return position;
            }

            float k = newSize / oldSize;
            Vector2 panned = Vector2.Lerp(_input.PointerWorldPosition, position, k);
            position.x = panned.x;
            position.y = panned.y;
            return position;
        }

        // Grab-pan: hold the middle button and the world point under the cursor follows the pointer.
        private Vector3 ApplyDragPan(Vector3 position)
        {
            if (!_dragPan || _input == null || !_input.PanHeld)
            {
                _isPanning = false;
                return position;
            }

            if (!_isPanning)
            {
                _isPanning = true;
                _panGrabWorld = _input.PointerWorldPosition;
                return position;
            }

            // Move so the originally grabbed point sits back under the (possibly moved) cursor.
            Vector2 diff = _panGrabWorld - _input.PointerWorldPosition;
            position.x += diff.x;
            position.y += diff.y;
            return position;
        }

        // Keep the camera's view rectangle inside the board; centre on an axis the board is smaller than.
        private Vector3 ClampToBoard(Vector3 position, float size)
        {
            if (GridManager.Instance == null)
            {
                return position;
            }

            Bounds board = GridManager.Instance.WorldBounds;
            float halfHeight = size;
            float halfWidth = size * _camera.aspect;

            // Horizontal gets an overscroll margin so the player can pull board out from behind the side
            // panels; vertical stays hard-clamped to the board (no margin).
            position.x = ClampAxis(
                position.x, board.min.x, board.max.x, halfWidth, board.center.x, halfWidth * _horizontalOverscroll);
            position.y = ClampAxis(position.y, board.min.y, board.max.y, halfHeight, board.center.y, 0f);
            return position;
        }

        private static float ClampAxis(
            float value, float min, float max, float halfExtent, float center, float overscroll)
        {
            // How far the camera centre may sit from the board centre: the slack the board gives over the
            // view, plus the overscroll margin. Folding overscroll into one continuous value (instead of
            // a separate "centre when the view is wider" branch) keeps panning smooth across the point
            // where the view grows past the board — no sudden snap back to centre while zooming/panning.
            float halfRange = (max - min) * 0.5f - halfExtent + overscroll;
            return halfRange > 0f ? Mathf.Clamp(value, center - halfRange, center + halfRange) : center;
        }
    }
}
