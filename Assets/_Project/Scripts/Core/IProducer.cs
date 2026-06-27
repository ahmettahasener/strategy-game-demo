using System.Collections.Generic;
using StrategyDemo.Data;

namespace StrategyDemo.Core
{
    /// <summary>
    /// Implemented by entities that can produce units — the Barracks (Brief #5, #9).
    /// Non-producers (e.g. the Mana Well) do not implement this.
    /// </summary>
    public interface IProducer
    {
        bool CanProduce { get; }
        IReadOnlyList<UnitData> ProducibleUnits { get; }
    }
}
