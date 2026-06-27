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

        private SpriteRenderer _spriteRenderer;
        private Color _baseColor;
        private UnitData _data;

        public UnitData Data => _data;

        public override EntityData Definition => _data;

        public override int MaxHp => _data != null ? _data.MaxHp : 0;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _baseColor = _spriteRenderer.color;
        }

        /// <summary>Configures the unit after instantiation (called by the unit factory).</summary>
        public void Initialize(UnitData data, Faction faction)
        {
            _data = data;
            SetFaction(faction);
            ResetHealth();
        }

        protected override void SetHighlight(bool isOn)
        {
            _spriteRenderer.color = isOn ? _highlightTint : _baseColor;
        }
    }
}
