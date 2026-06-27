using StrategyDemo.Core;
using StrategyDemo.Data;
using UnityEngine;

namespace StrategyDemo.Buildings
{
    /// <summary>
    /// Creates buildings from <see cref="BuildingData"/> at a grid footprint. Intentionally a plain
    /// C# class (not a MonoBehaviour/Singleton): stateless and owned by its caller, keeping building
    /// creation in one place without a scene object or lifecycle overhead. The pooling slice swaps
    /// the Instantiate call for a pool fetch.
    /// </summary>
    public sealed class BuildingFactory
    {
        public BuildingElement Create(BuildingData data, Vector2Int footprintOrigin, Faction faction)
        {
            Vector3 worldPosition = FootprintCenterWorld(footprintOrigin, data.Size);
            GameObject instance = Object.Instantiate(data.Prefab, worldPosition, Quaternion.identity);
            BuildingElement building = instance.GetComponent<BuildingElement>();
            building.Initialize(data, footprintOrigin, faction);
            return building;
        }

        private static Vector3 FootprintCenterWorld(Vector2Int origin, Vector2Int size)
        {
            Vector3 min = GridManager.Instance.CellToWorldCenter(origin);
            Vector3 max = GridManager.Instance.CellToWorldCenter(origin + size - Vector2Int.one);
            return (min + max) * 0.5f;
        }
    }
}
