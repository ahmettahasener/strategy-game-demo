using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace StrategyDemo.Grid
{
    /// <summary>
    /// View that paints a building's footprint on a dedicated preview Tilemap: green where
    /// placement is valid, red where it is not (Brief #3). Holds no placement rules.
    /// </summary>
    public sealed class PlacementPreview : MonoBehaviour
    {
        [SerializeField] private Tilemap _previewTilemap;
        [SerializeField] private TileBase _cellTile;
        [SerializeField] private Color _validColor = new Color(0.3f, 1f, 0.3f, 0.5f);
        [SerializeField] private Color _invalidColor = new Color(1f, 0.3f, 0.3f, 0.5f);

        /// <summary>Paints the given cells in the valid/invalid colour.</summary>
        public void Show(IReadOnlyList<Vector2Int> cells, bool isValid)
        {
            _previewTilemap.ClearAllTiles();
            Color color = isValid ? _validColor : _invalidColor;

            for (int i = 0; i < cells.Count; i++)
            {
                var cell = new Vector3Int(cells[i].x, cells[i].y, 0);
                _previewTilemap.SetTile(cell, _cellTile);
                _previewTilemap.SetTileFlags(cell, TileFlags.None);
                _previewTilemap.SetColor(cell, color);
            }
        }

        /// <summary>Removes the preview (on cancel / exit placement).</summary>
        public void Clear()
        {
            _previewTilemap.ClearAllTiles();
        }
    }
}
