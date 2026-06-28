using StrategyDemo.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StrategyDemo.Grid
{
    /// <summary>
    /// Highlights the board cell under the pointer for a bit of tactile RTS feel. Hidden while placing
    /// (the placement preview owns the board then) and while the pointer is over UI. Uses one
    /// gameplay-atlas sprite moved around, so it adds no draw call.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CellHoverHighlight : MonoBehaviour
    {
        [SerializeField] private InputReader _input;
        [SerializeField] private PlacementController _placement;
        [SerializeField] private Sprite _sprite;
        [SerializeField] private Color _color = new Color(1f, 0.95f, 0.6f, 0.35f);
        [SerializeField] private int _sortingOrder = 1;

        private SpriteRenderer _renderer;

        private void Awake()
        {
            if (_sprite == null)
            {
                return;
            }

            var child = new GameObject("CellHighlight");
            _renderer = child.AddComponent<SpriteRenderer>();
            _renderer.transform.SetParent(transform, false);
            _renderer.sprite = _sprite;
            _renderer.color = _color;
            _renderer.sortingOrder = _sortingOrder;

            float native = Mathf.Max(0.0001f, _sprite.bounds.size.x);
            _renderer.transform.localScale = Vector3.one / native; // one sprite spans one cell
            _renderer.enabled = false;
        }

        private void Update()
        {
            if (_renderer == null || _input == null || GridManager.Instance == null)
            {
                return;
            }

            bool blocked = (_placement != null && _placement.IsPlacing)
                || (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject());
            if (blocked)
            {
                _renderer.enabled = false;
                return;
            }

            Vector2Int cell = GridManager.Instance.WorldToCell(_input.PointerWorldPosition);
            if (!GridManager.Instance.IsInBounds(cell))
            {
                _renderer.enabled = false;
                return;
            }

            _renderer.enabled = true;
            _renderer.transform.position = GridManager.Instance.CellToWorldCenter(cell);
        }
    }
}
