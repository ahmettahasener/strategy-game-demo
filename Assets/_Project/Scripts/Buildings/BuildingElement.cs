using StrategyDemo.Core;
using StrategyDemo.Data;
using UnityEngine;

namespace StrategyDemo.Buildings
{
    /// <summary>
    /// A placed building on the board. Supplies its stats from <see cref="BuildingData"/>, remembers
    /// the grid footprint it occupies, and releases those cells when destroyed.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class BuildingElement : GameElement
    {
        [SerializeField] private Color _highlightTint = Color.cyan;

        private SpriteRenderer _spriteRenderer;
        private Color _baseColor;
        private BuildingData _data;
        private Vector2Int _footprintOrigin;

        public BuildingData Data => _data;

        public override int MaxHp => _data != null ? _data.MaxHp : 0;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _baseColor = _spriteRenderer.color;
        }

        /// <summary>Configures the building after instantiation (called by the placement flow).</summary>
        public void Initialize(BuildingData data, Vector2Int footprintOrigin, Faction faction)
        {
            _data = data;
            _footprintOrigin = footprintOrigin;
            SetFaction(faction);
            ResetHealth();
        }

        protected override void SetHighlight(bool isOn)
        {
            _spriteRenderer.color = isOn ? _highlightTint : _baseColor;
        }

        protected override void OnDied()
        {
            if (GridManager.Instance != null && _data != null)
            {
                GridManager.Instance.Free(_footprintOrigin, _data.Size);
            }
        }
    }
}
