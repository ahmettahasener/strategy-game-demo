using StrategyDemo.Core;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] private float _opaqueSize = 7f;   // fallback opaque size if no CameraController
        [SerializeField] private float _fadedSize = 4f;    // ortho size at/below which the panel is dimmest
        [SerializeField] private float _fadeSpeed = 8f;

        private Canvas _canvas;
        private ScrollRect _scrollRect; // disabled during a drag-pan so the list can't scroll under it
        private CameraController _cameraController; // its HomeSize is the "fully opaque" point, so the
                                                    // panels are solid at the default view and only fade in
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

            if (_boardCamera != null)
            {
                _cameraController = _boardCamera.GetComponent<CameraController>();
            }

            _canvas = GetComponentInParent<Canvas>();
            _scrollRect = GetComponentInChildren<ScrollRect>(true);
        }

        private void Update()
        {
            UpdateDragState();
            bool panning = Input.GetMouseButton(2);
            // Disable the panel's controls while drag-panning so a click that lands on it mid-pan can't
            // accidentally pick a building card; restored as soon as the pan is released.
            _group.interactable = !panning;

            // interactable only gates Selectables (buttons), not the ScrollRect's drag/wheel scrolling,
            // so disable the scroll view too — keeping blocksRaycasts on, so the click is still absorbed
            // rather than leaking through to the board.
            if (_scrollRect != null)
            {
                _scrollRect.enabled = !panning;
            }
            _group.alpha = Mathf.MoveTowards(_group.alpha, TargetAlpha(panning), _fadeSpeed * Time.deltaTime);
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

        private float TargetAlpha(bool panning)
        {
            // While drag-panning the camera (middle mouse held), don't pop the panel back to opaque just
            // because the cursor swept over it — keep it faded until the pan is released, so it stays out
            // of the way during the drag.
            if (!panning && (_draggingFromPanel || IsPointerOver()))
            {
                return 1f;
            }

            if (_boardCamera == null || !_boardCamera.orthographic)
            {
                return 1f;
            }

            // Fully opaque at the default framing (and zoomed further out), fading in only as the camera
            // zooms past it. Using the camera's home size as the opaque point means the default view is
            // always solid, regardless of the panel's authored fade values.
            float opaqueSize = _cameraController != null ? _cameraController.HomeSize : _opaqueSize;
            float t = Mathf.InverseLerp(_fadedSize, opaqueSize, _boardCamera.orthographicSize);
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
