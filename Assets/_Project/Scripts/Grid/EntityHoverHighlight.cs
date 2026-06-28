using StrategyDemo.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StrategyDemo.Grid
{
    /// <summary>
    /// Shows a faint ring under the entity beneath the pointer (when it isn't already selected), for a
    /// bit of "this is clickable" RTS feel. One shared, atlas-sprite renderer is moved around — no
    /// draw call, no per-entity state. Hidden while placing or over UI. GetComponent runs only when the
    /// hovered collider changes, not every frame.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EntityHoverHighlight : MonoBehaviour
    {
        [SerializeField] private InputReader _input;
        [SerializeField] private PlacementController _placement;
        [SerializeField] private Sprite _ringSprite;
        [SerializeField] private Color _color = new Color(1f, 1f, 1f, 0.28f);
        [SerializeField] private float _widthFactor = 1.05f;
        [SerializeField] private float _flatten = 0.5f;
        [SerializeField] private int _sortingOrder = 1;

        private SpriteRenderer _ring;
        private Collider2D _lastCollider;
        private GameElement _hoveredElement;
        private SpriteRenderer _hoveredRenderer;

        private void Awake()
        {
            if (_ringSprite == null)
            {
                return;
            }

            var child = new GameObject("EntityHoverRing");
            _ring = child.AddComponent<SpriteRenderer>();
            _ring.transform.SetParent(transform, false);
            _ring.sprite = _ringSprite;
            _ring.color = _color;
            _ring.sortingOrder = _sortingOrder;
            _ring.enabled = false;
        }

        private void Update()
        {
            if (_ring == null || _input == null)
            {
                return;
            }

            bool blocked = (_placement != null && _placement.IsPlacing)
                || (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject());
            if (blocked)
            {
                _ring.enabled = false;
                return;
            }

            Collider2D hit = Physics2D.OverlapPoint(_input.PointerWorldPosition);
            if (hit != _lastCollider)
            {
                _lastCollider = hit;
                _hoveredElement = hit != null ? hit.GetComponent<GameElement>() : null;
                _hoveredRenderer = hit != null ? hit.GetComponent<SpriteRenderer>() : null;
            }

            if (_hoveredElement == null || _hoveredElement.IsSelected || _hoveredElement.IsDead
                || _hoveredRenderer == null)
            {
                _ring.enabled = false;
                return;
            }

            Bounds bounds = _hoveredRenderer.bounds;
            float width = bounds.size.x * _widthFactor;
            float height = width * _flatten;
            Vector2 native = _ringSprite.bounds.size;
            _ring.transform.localScale = new Vector3(
                width / Mathf.Max(0.0001f, native.x), height / Mathf.Max(0.0001f, native.y), 1f);
            _ring.transform.position = new Vector3(bounds.center.x, bounds.min.y + height * 0.5f, 0f);
            _ring.enabled = true;
        }
    }
}
