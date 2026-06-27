using UnityEngine;

namespace StrategyDemo.Grid
{
    /// <summary>
    /// Pure logical model of the rectangular board: cell bounds and per-cell occupancy.
    /// Unity object-free (no MonoBehaviour, no Tilemap — only the <see cref="Vector2Int"/> value
    /// type) so it is fully EditMode-testable; the <c>GridManager</c> bridges it to the scene
    /// Tilemap. Placement and pathfinding both
    /// read occupancy from here. The board is a solid rectangle — every in-bounds cell is a
    /// valid cell unless occupied.
    /// </summary>
    public sealed class GridModel
    {
        private readonly Vector2Int _origin;
        private readonly int _width;
        private readonly int _height;
        private readonly bool[,] _occupied;

        /// <param name="origin">Lowest (x, y) cell of the board.</param>
        /// <param name="width">Cell count along x (must be &gt; 0).</param>
        /// <param name="height">Cell count along y (must be &gt; 0).</param>
        public GridModel(Vector2Int origin, int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(width), $"Grid size must be positive (got {width}x{height}).");
            }

            _origin = origin;
            _width = width;
            _height = height;
            _occupied = new bool[width, height];
        }

        public Vector2Int Origin => _origin;
        public int Width => _width;
        public int Height => _height;

        /// <summary>True if the cell lies inside the board rectangle.</summary>
        public bool IsInBounds(Vector2Int cell)
        {
            return cell.x >= _origin.x && cell.x < _origin.x + _width
                && cell.y >= _origin.y && cell.y < _origin.y + _height;
        }

        /// <summary>True if the cell is in bounds and not occupied.</summary>
        public bool IsFree(Vector2Int cell)
        {
            return IsInBounds(cell) && !_occupied[cell.x - _origin.x, cell.y - _origin.y];
        }

        /// <summary>
        /// True if every cell of the <paramref name="size"/> footprint anchored at
        /// <paramref name="footprintOrigin"/> is free (in bounds and unoccupied).
        /// A non-positive footprint is never valid.
        /// </summary>
        public bool IsAreaFree(Vector2Int footprintOrigin, Vector2Int size)
        {
            if (!IsPositiveSize(size))
            {
                return false;
            }

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    if (!IsFree(new Vector2Int(footprintOrigin.x + x, footprintOrigin.y + y)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>Marks the footprint occupied.</summary>
        public void Occupy(Vector2Int footprintOrigin, Vector2Int size)
        {
            SetArea(footprintOrigin, size, true);
        }

        /// <summary>Clears the footprint (e.g. when a building is destroyed).</summary>
        public void Free(Vector2Int footprintOrigin, Vector2Int size)
        {
            SetArea(footprintOrigin, size, false);
        }

        private static bool IsPositiveSize(Vector2Int size)
        {
            return size.x > 0 && size.y > 0;
        }

        private void SetArea(Vector2Int footprintOrigin, Vector2Int size, bool occupied)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector2Int cell = new Vector2Int(footprintOrigin.x + x, footprintOrigin.y + y);
                    if (IsInBounds(cell))
                    {
                        _occupied[cell.x - _origin.x, cell.y - _origin.y] = occupied;
                    }
                }
            }
        }
    }
}
