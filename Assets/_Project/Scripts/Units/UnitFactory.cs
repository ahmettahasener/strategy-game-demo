using StrategyDemo.Core;
using StrategyDemo.Data;
using UnityEngine;

namespace StrategyDemo.Units
{
    /// <summary>
    /// Creates units from <see cref="UnitData"/>. Intentionally a plain C# class (not a
    /// MonoBehaviour/Singleton): it is stateless and owned by its caller, keeping unit creation in
    /// one place without adding a scene object or lifecycle overhead. Instances come from the shared
    /// <see cref="PoolManager"/> so units are recycled, not destroyed.
    /// </summary>
    public sealed class UnitFactory
    {
        public UnitElement Create(
            UnitData data, Vector3 worldPosition, Faction faction, Transform parent = null)
        {
            GameObject instance = PoolManager.Instance.Get(data.Prefab, parent);
            instance.transform.SetPositionAndRotation(worldPosition, Quaternion.identity);
            instance.name = $"{faction}_{data.DisplayName}";
            UnitElement unit = instance.GetComponent<UnitElement>();
            unit.Initialize(data, faction);
            return unit;
        }
    }
}
