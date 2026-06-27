using System.Collections.Generic;
using StrategyDemo.Data;
using StrategyDemo.Units;
using UnityEngine;

namespace StrategyDemo.Core
{
    /// <summary>
    /// Drives instant, unlimited unit production (Brief #4, #9): given a producer and a unit it can
    /// make, spawns the unit at the producer's spawn cell via the <see cref="UnitFactory"/>. The
    /// producer is supplied by the caller — a debug trigger now, the selected building once the
    /// Selection/Info-Panel slice exists.
    /// </summary>
    public sealed class ProductionManager : Singleton<ProductionManager>
    {
        private readonly UnitFactory _unitFactory = new UnitFactory();

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

            Vector3 spawnPosition = GridManager.Instance.CellToWorldCenter(spawnCell);
            return _unitFactory.Create(unit, spawnPosition, Faction.Player);
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
