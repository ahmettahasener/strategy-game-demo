using StrategyDemo.Core;
using StrategyDemo.Data;
using UnityEngine;

namespace StrategyDemo.Units
{
    /// <summary>
    /// A produced soldier on the board. Supplies its stats from <see cref="UnitData"/>. Movement and
    /// combat arrive in later slices; for now it spawns, takes damage and dies. No dynamic
    /// Rigidbody2D — movement will be transform-based, so overlapping spawns can't cause a physics
    /// explosion.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class UnitElement : GameElement
    {
        [SerializeField] private Color _highlightTint = Color.yellow;
        [SerializeField] private Color _enemyTint = new Color(1f, 0.4f, 0.4f);

        private SpriteRenderer _spriteRenderer;
        private Color _originalColor;
        private Color _baseColor;
        private UnitData _data;

        public UnitData Data => _data;

        public override EntityData Definition => _data;

        public override int MaxHp => _data != null ? _data.MaxHp : 0;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
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

            SetFaction(faction);
            _baseColor = faction == Faction.Enemy ? _enemyTint : _originalColor;
            OnDeselected(); // clears stale selection state and applies the base colour
            ResetHealth();
        }

        protected override void SetHighlight(bool isOn)
        {
            _spriteRenderer.color = isOn ? _highlightTint : _baseColor;
        }

        protected override void RemoveFromBoard()
        {
            PoolManager.Instance.Release(gameObject);
        }
    }
}
