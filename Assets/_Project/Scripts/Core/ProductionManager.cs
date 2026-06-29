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
        [SerializeField] private Transform _unitsRoot; // parent for produced units (hierarchy tidiness)

        private readonly UnitFactory _unitFactory = new UnitFactory();

        /// <summary>
        /// Produces <paramref name="unit"/> from <paramref name="producer"/> if the producer can make
        /// it and its spawn cell is on the board. Returns the spawned unit, or null if rejected.
        /// Production is instant and unlimited: there is no cooldown or cost.
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
            // holding a unit (see BoardQuery: units are detected by physics, not grid occupancy).
            Vector2Int? openCell = BoardQuery.NearestOpenCell(spawnCell);
            if (openCell == null)
            {
                // Every cell is under a building or already holds a unit; reject the order and signal
                // feedback (denial SFX / UI) instead of stacking units invisibly on one cell.
                GameEvents.RaiseActionDenied();
                return null;
            }

            Vector3 spawnPosition = GridManager.Instance.CellToWorldCenter(openCell.Value);
            return _unitFactory.Create(unit, spawnPosition, Faction.Player, _unitsRoot);
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
