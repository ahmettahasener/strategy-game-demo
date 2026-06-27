using StrategyDemo.Core;
using StrategyDemo.Data;
using UnityEngine;

namespace StrategyDemo.Units
{
    /// <summary>
    /// Creates units from <see cref="UnitData"/>. Intentionally a plain C# class (not a
    /// MonoBehaviour/Singleton): it is stateless and owned by its caller, keeping unit creation in
    /// one place without adding a scene object or lifecycle overhead. The pooling slice swaps the
    /// Instantiate call for a pool fetch.
    /// </summary>
    public sealed class UnitFactory
    {
        public UnitElement Create(UnitData data, Vector3 worldPosition, Faction faction)
        {
            GameObject instance = Object.Instantiate(data.Prefab, worldPosition, Quaternion.identity);
            UnitElement unit = instance.GetComponent<UnitElement>();
            unit.Initialize(data, faction);
            return unit;
        }
    }
}
