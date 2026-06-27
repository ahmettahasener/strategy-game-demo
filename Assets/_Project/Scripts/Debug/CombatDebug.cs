using StrategyDemo.Buildings;
using StrategyDemo.Core;
using StrategyDemo.Data;
using StrategyDemo.Units;
using UnityEngine;

namespace StrategyDemo.DebugTools
{
    /// <summary>
    /// Development/reviewer helper: spawns extra enemy units (X) and enemy buildings (C) under the
    /// pointer. The default demo loop is owned by ScenarioSetup; this remains a fast way to stress
    /// combat and death cases. The component always compiles; only the cheat input runs in the
    /// Editor and Development builds.
    /// </summary>
    public sealed class CombatDebug : MonoBehaviour
    {
        [SerializeField] private InputReader _input;
        [SerializeField] private UnitData _enemyUnit;
        [SerializeField] private BuildingData _enemyBuilding;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private readonly UnitFactory _unitFactory = new UnitFactory();
        private readonly BuildingFactory _buildingFactory = new BuildingFactory();

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                SpawnEnemyUnit();
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                SpawnEnemyBuilding();
            }
        }

        private void SpawnEnemyUnit()
        {
            if (_enemyUnit == null)
            {
                return;
            }

            Vector2Int cell = GridManager.Instance.WorldToCell(_input.PointerWorldPosition);
            Vector3 world = GridManager.Instance.CellToWorldCenter(cell);
            _unitFactory.Create(_enemyUnit, world, Faction.Enemy);
        }

        private void SpawnEnemyBuilding()
        {
            if (_enemyBuilding == null)
            {
                return;
            }

            Vector2Int pointerCell = GridManager.Instance.WorldToCell(_input.PointerWorldPosition);
            Vector2Int origin = pointerCell
                - new Vector2Int(_enemyBuilding.Size.x / 2, _enemyBuilding.Size.y / 2);
            if (!GridManager.Instance.IsAreaFree(origin, _enemyBuilding.Size))
            {
                return;
            }

            _buildingFactory.Create(_enemyBuilding, origin, Faction.Enemy);
            GridManager.Instance.Occupy(origin, _enemyBuilding.Size);
        }
#endif
    }
}
