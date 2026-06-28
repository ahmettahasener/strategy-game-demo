using UnityEngine;
using UnityEngine.Tilemaps;

namespace StrategyDemo.Grid
{
    /// <summary>
    /// Applies subtle deterministic colour variation to the board Tilemap so the repeated grass tile
    /// reads less flat without adding sprites, materials, or draw calls.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GroundColorVariation : MonoBehaviour
    {
        [SerializeField] private Tilemap _tilemap;
        [SerializeField] private Color _baseColor = Color.white;
        [SerializeField] private Color _coolVariant = new Color(0.88f, 1f, 0.88f, 1f);
        [SerializeField] private Color _warmVariant = new Color(1f, 0.96f, 0.86f, 1f);
        [SerializeField, Range(0f, 1f)] private float _strength = 0.22f;

        private void Awake()
        {
            Apply();
        }

        private void Apply()
        {
            if (_tilemap == null)
            {
                return;
            }

            BoundsInt bounds = _tilemap.cellBounds;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    var cell = new Vector3Int(x, y, 0);
                    if (!_tilemap.HasTile(cell))
                    {
                        continue;
                    }

                    _tilemap.SetTileFlags(cell, TileFlags.None);
                    _tilemap.SetColor(cell, VariationForCell(x, y));
                }
            }
        }

        private Color VariationForCell(int x, int y)
        {
            int hash = Mathf.Abs((x * 73856093) ^ (y * 19349663));
            float t = (hash % 1000) / 999f;
            Color variant = t < 0.5f
                ? Color.Lerp(_baseColor, _coolVariant, t * 2f)
                : Color.Lerp(_baseColor, _warmVariant, (t - 0.5f) * 2f);
            return Color.Lerp(_baseColor, variant, _strength);
        }
    }
}
