using System.Collections.Generic;
using StrategyDemo.Grid;
using StrategyDemo.Pathfinding;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace StrategyDemo.Core
{
    /// <summary>
    /// Owns the logical board grid and bridges it to the scene Tilemap: builds a rectangular
    /// <see cref="GridModel"/> from the painted ground tiles and serves cell/occupancy queries to
    /// placement and pathfinding (Brief #3). Paint the board as a solid rectangle (no holes).
    /// </summary>
    public sealed class GridManager : Singleton<GridManager>
    {
        [SerializeField] private Tilemap _groundTilemap;

        private readonly Pathfinder _pathfinder = new Pathfinder();
        private GridModel _model;
        private Bounds _worldBounds;

        /// <summary>World-space rectangle the board occupies — used to keep the camera over the board.</summary>
        public Bounds WorldBounds => _worldBounds;

        /// <summary>Board width in cells.</summary>
        public int Width => _model.Width;

        /// <summary>Board height in cells.</summary>
        public int Height => _model.Height;

        protected override void Awake()
        {
            base.Awake();
            if (Instance == this)
            {
                BuildModel();
            }
        }

        /// <summary>Converts a world position to its board cell.</summary>
        public Vector2Int WorldToCell(Vector3 worldPosition)
        {
            Vector3Int cell = _groundTilemap.WorldToCell(worldPosition);
            return new Vector2Int(cell.x, cell.y);
        }

        /// <summary>World-space center of a cell (where a 1x1 entity sits).</summary>
        public Vector3 CellToWorldCenter(Vector2Int cell)
        {
            return _groundTilemap.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
        }

        /// <summary>True if the cell lies inside the board rectangle.</summary>
        public bool IsInBounds(Vector2Int cell)
        {
            return _model.IsInBounds(cell);
        }

        public bool IsAreaFree(Vector2Int footprintOrigin, Vector2Int size)
        {
            return _model.IsAreaFree(footprintOrigin, size);
        }

        public void Occupy(Vector2Int footprintOrigin, Vector2Int size)
        {
            _model.Occupy(footprintOrigin, size);
        }

        public void Free(Vector2Int footprintOrigin, Vector2Int size)
        {
            _model.Free(footprintOrigin, size);
        }

        /// <summary>Shortest walkable path between two cells; buildings are obstacles (Brief #6).</summary>
        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
        {
            return _pathfinder.FindPath(_model, start, target);
        }

        /// <summary>Nearest walkable cell to <paramref name="cell"/> (e.g. a building's perimeter).</summary>
        public Vector2Int? NearestFreeCell(Vector2Int cell)
        {
            return _model.NearestFreeCell(cell, Mathf.Max(_model.Width, _model.Height));
        }

        private void BuildModel()
        {
            _groundTilemap.CompressBounds();
            BoundsInt bounds = _groundTilemap.cellBounds;
            _model = new GridModel(
                new Vector2Int(bounds.xMin, bounds.yMin), bounds.size.x, bounds.size.y);

            // Board rectangle in world space: cellBounds.max is one past the last cell, so its world
            // position is the board's far corner.
            Vector3 min = _groundTilemap.CellToWorld(bounds.min);
            Vector3 max = _groundTilemap.CellToWorld(bounds.max);
            _worldBounds = new Bounds();
            _worldBounds.SetMinMax(min, max);
        }
    }
}
