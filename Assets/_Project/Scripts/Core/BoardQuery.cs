using System.Collections.Generic;
using StrategyDemo.Units;
using UnityEngine;

namespace StrategyDemo.Core
{
    /// <summary>
    /// Board-cell queries shared by spawning and movement: finds the nearest "open" cell - one that is
    /// on the board, free of buildings, and not already holding a unit. Units deliberately don't occupy
    /// the grid (so they never become pathfinding obstacles), so their presence is detected with a light
    /// physics probe rather than grid occupancy, keeping the grid model buildings-only.
    /// </summary>
    public static class BoardQuery
    {
        // Reused probe buffer so the occupancy check stays allocation-free.
        private static readonly Collider2D[] OverlapBuffer = new Collider2D[8];
        private static readonly List<Vector2Int> RingScratch = new List<Vector2Int>();

        /// <summary>
        /// The first open cell at, or spiralling outward (nearest first) from, <paramref name="origin"/>,
        /// or null when the whole board is occupied.
        /// </summary>
        public static Vector2Int? NearestOpenCell(Vector2Int origin)
        {
            GridManager grid = GridManager.Instance;
            // A burst of spawns/orders can move colliders within the frame; sync so the probe is current.
            Physics2D.SyncTransforms();
            float probeRadius = CellWorldSize() * 0.35f; // < half a cell, so a neighbour never reads as full

            if (IsCellOpen(origin, probeRadius))
            {
                return origin;
            }

            // Search outward across the whole board so every reachable cell is considered before it is
            // declared full; max(Width, Height) rings reach the farthest corner from any origin.
            int maxRadius = Mathf.Max(grid.Width, grid.Height);
            for (int radius = 1; radius <= maxRadius; radius++)
            {
                // Top and bottom edges of the ring (corners included here).
                for (int x = origin.x - radius; x <= origin.x + radius; x++)
                {
                    var top = new Vector2Int(x, origin.y + radius);
                    if (IsCellOpen(top, probeRadius))
                    {
                        return top;
                    }

                    var bottom = new Vector2Int(x, origin.y - radius);
                    if (IsCellOpen(bottom, probeRadius))
                    {
                        return bottom;
                    }
                }

                // Left and right edges (corners already covered above).
                for (int y = origin.y - radius + 1; y <= origin.y + radius - 1; y++)
                {
                    var left = new Vector2Int(origin.x - radius, y);
                    if (IsCellOpen(left, probeRadius))
                    {
                        return left;
                    }

                    var right = new Vector2Int(origin.x + radius, y);
                    if (IsCellOpen(right, probeRadius))
                    {
                        return right;
                    }
                }
            }

            return null; // no open cell anywhere; the board is full
        }

        /// <summary>
        /// The open cell at, or hugging, <paramref name="clicked"/> that <paramref name="from"/> can
        /// reach with the least travel, so a unit ordered onto an occupied cell (e.g. a friendly
        /// building) walks to the near side instead of circling to the far edge. Returns
        /// <paramref name="clicked"/> when it is itself open, or null when nothing nearby is reachable.
        /// Unlike <see cref="NearestOpenCell"/> (nearest to the cell), this picks nearest by path.
        /// </summary>
        public static Vector2Int? NearestOpenCellTo(Vector2Int clicked, Vector2Int from)
        {
            GridManager grid = GridManager.Instance;
            Physics2D.SyncTransforms();
            float probeRadius = CellWorldSize() * 0.35f;

            if (IsCellOpen(clicked, probeRadius))
            {
                return clicked;
            }

            int maxRadius = Mathf.Max(grid.Width, grid.Height);
            for (int radius = 1; radius <= maxRadius; radius++)
            {
                // Prefer the closest ring around the clicked cell, but do not stop at cells that are
                // open yet unreachable from the unit's side of the board.
                RingScratch.Clear();
                CollectOpenRing(clicked, radius, probeRadius, RingScratch);

                // Of this ring, take the one the unit reaches with the lowest path cost (diagonals cost
                // more), so it approaches from the side facing it.
                Vector2Int? best = null;
                int bestCost = int.MaxValue;
                for (int i = 0; i < RingScratch.Count; i++)
                {
                    int cost = PathCost(grid.FindPath(from, RingScratch[i]));
                    if ((cost > 0 || RingScratch[i] == from) && cost < bestCost)
                    {
                        bestCost = cost;
                        best = RingScratch[i];
                    }
                }

                if (best != null)
                {
                    return best;
                }
            }

            return null;
        }

        /// <summary>
        /// The open cell adjacent to the whole <paramref name="footprint"/> (a building's rectangle, or
        /// a single unit cell) that <paramref name="from"/> reaches with the least path cost — so a unit
        /// ordered onto a multi-cell building approaches the side facing it, no matter which footprint
        /// cell was clicked. Null when no adjacent cell is reachable.
        /// </summary>
        public static Vector2Int? NearestOpenCellAround(IReadOnlyList<Vector2Int> footprint, Vector2Int from)
        {
            if (footprint == null || footprint.Count == 0)
            {
                return null;
            }

            Physics2D.SyncTransforms();
            float probeRadius = CellWorldSize() * 0.35f;

            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            for (int i = 0; i < footprint.Count; i++)
            {
                minX = Mathf.Min(minX, footprint[i].x);
                minY = Mathf.Min(minY, footprint[i].y);
                maxX = Mathf.Max(maxX, footprint[i].x);
                maxY = Mathf.Max(maxY, footprint[i].y);
            }

            // The ring one cell out from the footprint rectangle — symmetric around the whole building,
            // so the choice below isn't biased toward the clicked corner.
            RingScratch.Clear();
            for (int x = minX - 1; x <= maxX + 1; x++)
            {
                for (int y = minY - 1; y <= maxY + 1; y++)
                {
                    bool insideFootprint = x >= minX && x <= maxX && y >= minY && y <= maxY;
                    if (!insideFootprint)
                    {
                        AddIfOpen(new Vector2Int(x, y), probeRadius, RingScratch);
                    }
                }
            }

            GridManager grid = GridManager.Instance;
            Vector2Int? best = null;
            int bestCost = int.MaxValue;
            for (int i = 0; i < RingScratch.Count; i++)
            {
                int cost = PathCost(grid.FindPath(from, RingScratch[i]));
                if (cost > 0 && cost < bestCost)
                {
                    bestCost = cost;
                    best = RingScratch[i];
                }
            }

            return best;
        }

        private static void CollectOpenRing(Vector2Int center, int radius, float probeRadius, List<Vector2Int> into)
        {
            for (int x = center.x - radius; x <= center.x + radius; x++)
            {
                AddIfOpen(new Vector2Int(x, center.y + radius), probeRadius, into);
                AddIfOpen(new Vector2Int(x, center.y - radius), probeRadius, into);
            }

            for (int y = center.y - radius + 1; y <= center.y + radius - 1; y++)
            {
                AddIfOpen(new Vector2Int(center.x - radius, y), probeRadius, into);
                AddIfOpen(new Vector2Int(center.x + radius, y), probeRadius, into);
            }
        }

        private static void AddIfOpen(Vector2Int cell, float probeRadius, List<Vector2Int> into)
        {
            if (IsCellOpen(cell, probeRadius))
            {
                into.Add(cell);
            }
        }

        // Real path cost (orthogonal 10, diagonal 14, matching the pathfinder); 0 when unreachable.
        private static int PathCost(List<Vector2Int> path)
        {
            if (path.Count <= 1)
            {
                return 0;
            }

            int cost = 0;
            for (int i = 1; i < path.Count; i++)
            {
                Vector2Int step = path[i] - path[i - 1];
                cost += step.x != 0 && step.y != 0 ? 14 : 10;
            }

            return cost;
        }

        private static bool IsCellOpen(Vector2Int cell, float probeRadius)
        {
            GridManager grid = GridManager.Instance;
            if (!grid.IsInBounds(cell) || !grid.IsAreaFree(cell, Vector2Int.one))
            {
                return false; // off-board or under a building
            }

            return !HasUnit(grid.CellToWorldCenter(cell), probeRadius);
        }

        private static bool HasUnit(Vector3 worldCenter, float probeRadius)
        {
            int count = Physics2D.OverlapCircleNonAlloc(worldCenter, probeRadius, OverlapBuffer);
            for (int i = 0; i < count; i++)
            {
                if (OverlapBuffer[i] != null && OverlapBuffer[i].GetComponentInParent<UnitElement>() != null)
                {
                    return true;
                }
            }

            return false;
        }

        // World distance between two adjacent cell centres: the board's cell size, whatever the tilemap
        // is set to, so the probe radius scales with the grid rather than assuming 1 unit.
        private static float CellWorldSize()
        {
            GridManager grid = GridManager.Instance;
            return Vector3.Distance(
                grid.CellToWorldCenter(Vector2Int.zero),
                grid.CellToWorldCenter(Vector2Int.right));
        }
    }
}
