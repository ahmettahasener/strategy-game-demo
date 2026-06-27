using System.Collections.Generic;
using StrategyDemo.Data;
using UnityEngine;

namespace StrategyDemo.Core
{
    /// <summary>
    /// Implemented by entities that can produce units — the Barracks (Brief #5, #9). Every building
    /// implements it, but non-producers (e.g. the Mana Well) report <c>CanProduce == false</c>.
    /// </summary>
    public interface IProducer
    {
        bool CanProduce { get; }
        IReadOnlyList<UnitData> ProducibleUnits { get; }

        /// <summary>Board cell where produced units spawn (Brief #6).</summary>
        Vector2Int SpawnCell { get; }
    }
}
