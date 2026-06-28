using UnityEngine;

namespace StrategyDemo.UI
{
    /// <summary>
    /// Softly fades a UI panel as the board camera zooms in, so panels stop blocking the view — but
    /// snaps it back to full opacity while the pointer is over the panel, so it is always readable
    /// when you reach for it. Fades alpha only (raycasts/interaction stay on), so the panel keeps
    /// working while dimmed and the fade adds no draw calls.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UIZoomFader : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Camera _boardCamera;
        [SerializeField] private RectTransform _hoverArea; // pointer here keeps the panel opaque
        [SerializeField, Range(0f, 1f)] private float _fadedAlpha = 0.35f;
        [SerializeField] private float _opaqueSize = 7f;   // ortho size at/above which the panel is full
        [SerializeField] private float _fadedSize = 4f;    // ortho size at/below which the panel is dimmest
        [SerializeField] private float _fadeSpeed = 8f;

        private Canvas _canvas;
        private bool _draggingFromPanel; // a press that began over the panel is still held (e.g. scrolling)

        private void Awake()
        {
            if (_group == null)
            {
                _group = GetComponent<CanvasGroup>();
            }

            if (_hoverArea == null)
            {
                _hoverArea = transform as RectTransform;
            }

            if (_boardCamera == null)
            {
                _boardCamera = Camera.main;
            }

            _canvas = GetComponentInParent<Canvas>();
        }

        private void Update()
        {
            UpdateDragState();
            _group.alpha = Mathf.MoveTowards(_group.alpha, TargetAlpha(), _fadeSpeed * Time.deltaTime);
        }

        // A drag that starts on the panel (e.g. flick-scrolling the build menu) keeps it opaque even
        // when the cursor leaves the panel rect, until the button is released.
        private void UpdateDragState()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _draggingFromPanel = IsPointerOver();
            }
            else if (!Input.GetMouseButton(0))
            {
                _draggingFromPanel = false;
            }
        }

        private float TargetAlpha()
        {
            if (_draggingFromPanel || IsPointerOver())
            {
                return 1f;
            }

            if (_boardCamera == null || !_boardCamera.orthographic)
            {
                return 1f;
            }

            // 1 when zoomed out (size >= opaque), down to _fadedAlpha when zoomed in (size <= faded).
            float t = Mathf.InverseLerp(_fadedSize, _opaqueSize, _boardCamera.orthographicSize);
            return Mathf.Lerp(_fadedAlpha, 1f, t);
        }

        private bool IsPointerOver()
        {
            if (_hoverArea == null)
            {
                return false;
            }

            // Overlay canvases use a null camera for the screen-point test; others use their camera.
            Camera uiCamera = _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? _canvas.worldCamera
                : null;
            return RectTransformUtility.RectangleContainsScreenPoint(_hoverArea, Input.mousePosition, uiCamera);
        }
    }
}
