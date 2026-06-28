using StrategyDemo.Core;
using StrategyDemo.Data;
using UnityEngine;

namespace StrategyDemo.Units
{
    /// <summary>
    /// A produced soldier on the board. Supplies its stats from <see cref="UnitData"/>. No dynamic
    /// Rigidbody2D — movement is transform-based, so overlapping spawns can't cause a physics
    /// explosion.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class UnitElement : GameElement
    {
        private const float BoardSize = 0.8f; // on-board footprint of a unit, in cells

        [SerializeField] private Color _highlightTint = Color.yellow;
        [SerializeField] private Color _enemyTint = new Color(1f, 0.4f, 0.4f);

        // Optional selection-ring child. When assigned, selection is shown by toggling this ring instead
        // of tinting the sprite, so the unit art keeps its true colour. Falls back to the tint when null.
        // Its size/position are authored on the prefab (all unit sprites share size, so one setting fits all).
        [SerializeField] private GameObject _selectionRing;

        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _collider;
        private Color _originalColor;
        private Color _baseColor;
        private UnitData _data;

        public UnitData Data => _data;

        public override EntityData Definition => _data;

        public override int MaxHp => _data != null ? _data.MaxHp : 0;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<BoxCollider2D>();
            _originalColor = _spriteRenderer.color;
            _baseColor = _originalColor;
        }

        /// <summary>
        /// Configures the unit after a factory spawn or a pool reuse. Resets every per-instance bit of
        /// state — faction, colour, selection highlight and health — so a recycled instance starts clean.
        /// </summary>
        public void Initialize(UnitData data, Faction faction)
        {
            _data = data;
            if (data.BoardSprite != null)
            {
                _spriteRenderer.sprite = data.BoardSprite;
            }

            ApplyBoardScale();
            FitColliderToSprite();
            SetFaction(faction);
            _baseColor = faction == Faction.Enemy ? _enemyTint : _originalColor;
            _spriteRenderer.color = _baseColor; // apply faction colour directly: with a ring, SetHighlight no longer sets it
            OnDeselected(); // clears stale selection state (ring off / base colour)
            ResetHealth();
        }

        protected override void SetHighlight(bool isOn)
        {
            if (_selectionRing != null)
            {
                _selectionRing.SetActive(isOn);
                return;
            }

            _spriteRenderer.color = isOn ? _highlightTint : _baseColor;
        }

        // Scale to a consistent on-board size (uniform, aspect-preserving), independent of the art's
        // resolution / pixels-per-unit (sprite.bounds.size is the true unscaled world size).
        private void ApplyBoardScale()
        {
            if (_spriteRenderer.sprite == null)
            {
                return;
            }

            Vector2 native = _spriteRenderer.sprite.bounds.size;
            float scale = BoardSize / Mathf.Max(0.0001f, Mathf.Max(native.x, native.y));
            transform.localScale = new Vector3(scale, scale, 1f);
        }

        // Match the click/selection collider to the sprite so the whole unit is clickable.
        private void FitColliderToSprite()
        {
            if (_collider == null || _spriteRenderer.sprite == null)
            {
                return;
            }

            Bounds bounds = _spriteRenderer.sprite.bounds;
            _collider.size = bounds.size;
            _collider.offset = bounds.center;
        }

        protected override void RemoveFromBoard()
        {
            PoolManager.Instance.Release(gameObject);
        }
    }
}
