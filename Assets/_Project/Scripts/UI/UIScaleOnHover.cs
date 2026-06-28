using UnityEngine;
using UnityEngine.EventSystems;

namespace StrategyDemo.UI
{
    /// <summary>
    /// Lightweight hover/press feedback for a UI element: scales up slightly on hover and dips on
    /// press, easing toward the target each frame. Pure visual — it doesn't consume the click, so it
    /// composes with the card's existing button behaviour. Stays within the same canvas/atlas, so it
    /// adds no draw call.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UIScaleOnHover : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float _hoverScale = 1.05f;
        [SerializeField] private float _pressScale = 0.96f;
        [SerializeField] private float _speed = 12f;

        private Vector3 _baseScale = Vector3.one;
        private float _target = 1f;
        private bool _hovering;

        private void OnEnable()
        {
            _baseScale = transform.localScale;
            _target = 1f;
        }

        private void OnDisable()
        {
            _hovering = false;
            _target = 1f;
            transform.localScale = _baseScale;
        }

        private void Update()
        {
            float current = transform.localScale.x / Mathf.Max(0.0001f, _baseScale.x);
            float next = Mathf.MoveTowards(current, _target, _speed * Time.deltaTime);
            transform.localScale = _baseScale * next;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovering = true;
            _target = _hoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovering = false;
            _target = 1f;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _target = _pressScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _target = _hovering ? _hoverScale : 1f;
        }
    }
}
