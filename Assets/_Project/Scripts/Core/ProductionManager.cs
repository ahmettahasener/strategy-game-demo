using System.Collections.Generic;
using StrategyDemo.Data;
using StrategyDemo.Units;
using UnityEngine;

namespace StrategyDemo.Core
{
    /// <summary>
    /// Drives instant, unlimited unit production (Brief #4, #9): given a producer and a unit it can
    /// make, spawns the unit near the producer's spawn cell via the <see cref="UnitFactory"/>. The
    /// producer is supplied by the caller (the selected building).
    /// </summary>
    public sealed class ProductionManager : Singleton<ProductionManager>
    {
        // Rings searched outward from the spawn cell before giving up and stacking on it.
        private const int MaxSpawnSearchRadius = 6;

        [SerializeField] private Transform _unitsRoot; // parent for produced units (hierarchy tidiness)

        private readonly UnitFactory _unitFactory = new UnitFactory();

        // Reused probe buffer so the spawn-cell occupancy check stays allocation-free.
        private static readonly Collider2D[] OverlapBuffer = new Collider2D[8];

        /// <summary>
        /// Produces <paramref name="unit"/> from <paramref name="producer"/> if the producer can make
        /// it and its spawn cell is on the board. Returns the spawned unit, or null if rejected.
        /// Production is instant and unlimited — there is no cooldown or cost.
        /// </summary>
        public UnitElement Produce(IProducer producer, UnitData unit)
        {
            if (producer == null || unit == null || !producer.CanProduce)
            {
                return null;
            }

            if (!IsProducible(producer, unit))
            {
                return null;
            }

            Vector2Int spawnCell = producer.SpawnCell;
            if (!GridManager.Instance.IsInBounds(spawnCell))
            {
                return null;
            }

            // Spread produced units instead of stacking them on one cell: spawn on the nearest cell to
            // the producer's spawn point that is on the board, clear of buildings, and not already
            // holding a unit. Units don't occupy the grid (that would turn them into pathfinding
            // obstacles), so a light physics probe — not grid occupancy — decides if a cell is taken.
            Vector2Int openCell = FindOpenSpawnCell(spawnCell);
            Vector3 spawnPosition = GridManager.Instance.CellToWorldCenter(openCell);
            return _unitFactory.Create(unit, spawnPosition, Faction.Player, _unitsRoot);
        }

        // First open cell at or around the spawn point, by expanding Chebyshev rings (nearest first).
        private Vector2Int FindOpenSpawnCell(Vector2Int center)
        {
            // A burst of production can spawn several units in one frame; sync so the probe sees the
            // colliders of units placed earlier this frame.
            Physics2D.SyncTransforms();
            float probeRadius = CellWorldSize() * 0.35f; // < half a cell, so a neighbour never reads as full

            if (IsCellOpen(center, probeRadius))
            {
                return center;
            }

            for (int radius = 1; radius <= MaxSpawnSearchRadius; radius++)
            {
                // Top and bottom edges of the ring (corners included here).
                for (int x = center.x - radius; x <= center.x + radius; x++)
                {
                    var top = new Vector2Int(x, center.y + radius);
                    if (IsCellOpen(top, probeRadius))
                    {
                        return top;
                    }

                    var bottom = new Vector2Int(x, center.y - radius);
                    if (IsCellOpen(bottom, probeRadius))
                    {
                        return bottom;
                    }
                }

                // Left and right edges (corners already covered above).
                for (int y = center.y - radius + 1; y <= center.y + radius - 1; y++)
                {
                    var left = new Vector2Int(center.x - radius, y);
                    if (IsCellOpen(left, probeRadius))
                    {
                        return left;
                    }

                    var right = new Vector2Int(center.x + radius, y);
                    if (IsCellOpen(right, probeRadius))
                    {
                        return right;
                    }
                }
            }

            // Board packed near the producer — fall back to the spawn cell rather than refusing to
            // produce; instant/unlimited production must still hand back a unit.
            return center;
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

        // World distance between two adjacent cell centres — the board's cell size, whatever the
        // tilemap is set to — so the probe radius scales with the grid rather than assuming 1 unit.
        private static float CellWorldSize()
        {
            GridManager grid = GridManager.Instance;
            return Vector3.Distance(
                grid.CellToWorldCenter(Vector2Int.zero),
                grid.CellToWorldCenter(Vector2Int.right));
        }

        // Index loop instead of foreach: avoids the per-call enumerator allocation that foreach over
        // an IReadOnlyList<T> interface incurs.
        private static bool IsProducible(IProducer producer, UnitData unit)
        {
            IReadOnlyList<UnitData> producible = producer.ProducibleUnits;
            for (int i = 0; i < producible.Count; i++)
            {
                if (producible[i] == unit)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
