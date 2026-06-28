using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace StrategyDemo.Grid
{
    /// <summary>
    /// View that paints a building's footprint on a dedicated preview Tilemap using distinct
    /// valid/invalid art. Holds no placement rules.
    /// </summary>
    public sealed class PlacementPreview : MonoBehaviour
    {
        [SerializeField] private Tilemap _previewTilemap;
        [SerializeField] private TileBase _validTile;
        [SerializeField] private TileBase _invalidTile;
        [SerializeField] private Color _validColor = Color.white;
        [SerializeField] private Color _invalidColor = Color.white;
        [SerializeField, Min(0.1f)] private float _tileVisualScale = 1.2f;

        /// <summary>Paints the given cells in the valid/invalid colour.</summary>
        public void Show(IReadOnlyList<Vector2Int> cells, bool isValid)
        {
            _previewTilemap.ClearAllTiles();
            TileBase tile = isValid ? _validTile : _invalidTile;
            Color color = isValid ? _validColor : _invalidColor;

            for (int i = 0; i < cells.Count; i++)
            {
                var cell = new Vector3Int(cells[i].x, cells[i].y, 0);
                _previewTilemap.SetTile(cell, tile);
                _previewTilemap.SetTileFlags(cell, TileFlags.None);
                _previewTilemap.SetColor(cell, color);
                _previewTilemap.SetTransformMatrix(
                    cell,
                    Matrix4x4.TRS(
                        Vector3.zero,
                        Quaternion.identity,
                        new Vector3(_tileVisualScale, _tileVisualScale, 1f)));
            }
        }

        /// <summary>Removes the preview (on cancel / exit placement).</summary>
        public void Clear()
        {
            _previewTilemap.ClearAllTiles();
        }
    }
}
