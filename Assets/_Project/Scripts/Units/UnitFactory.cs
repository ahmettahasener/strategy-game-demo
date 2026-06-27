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
        public UnitElement Create(UnitData data, Vector3 worldPosition, Faction faction)
        {
            GameObject instance = PoolManager.Instance.Get(data.Prefab);
            instance.transform.SetPositionAndRotation(worldPosition, Quaternion.identity);
            UnitElement unit = instance.GetComponent<UnitElement>();
            unit.Initialize(data, faction);
            return unit;
        }
    }
}
