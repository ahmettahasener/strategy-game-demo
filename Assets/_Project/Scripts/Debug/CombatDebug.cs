using StrategyDemo.Buildings;
using StrategyDemo.Core;
using StrategyDemo.Data;
using StrategyDemo.Grid;
using StrategyDemo.Units;
using UnityEngine;

namespace StrategyDemo.DebugTools
{
    /// <summary>
    /// Development/reviewer helper hotkeys (Editor and Development builds only). Spawns enemy units
    /// (X) and buildings (C) under the pointer; on the selected entity deals lethal damage (K) or
    /// heals to full (H); and toggles the in-game grid overlay (G) so pathfinding stays visible in
    /// the build, where Gizmos do not draw. The component always compiles; only the input runs.
    /// </summary>
    public sealed class CombatDebug : MonoBehaviour
    {
        [SerializeField] private InputReader _input;
        [SerializeField] private UnitData _enemyUnit;
        [SerializeField] private BuildingData _enemyBuilding;
        [SerializeField] private BoardGridOverlay _gridOverlay;

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
            else if (Input.GetKeyDown(KeyCode.K))
            {
                KillSelected();
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                HealSelected();
            }
            else if (Input.GetKeyDown(KeyCode.G))
            {
                ToggleGridOverlay();
            }
        }

        /// <summary>Deals lethal damage to the selected entity to test destroy-at-0-HP (Brief #11).</summary>
        private static void KillSelected()
        {
            if (SelectionManager.Instance.Current is GameElement element)
            {
                element.TakeDamage(element.MaxHp);
            }
        }

        /// <summary>Restores the selected entity to full HP and refreshes its health-bar view.</summary>
        private static void HealSelected()
        {
            if (SelectionManager.Instance.Current is GameElement element)
            {
                element.ResetHealth();
            }
        }

        /// <summary>Shows/hides the in-game pathfinding grid overlay (works in the build).</summary>
        private void ToggleGridOverlay()
        {
            if (_gridOverlay != null)
            {
                GameObject overlay = _gridOverlay.gameObject;
                overlay.SetActive(!overlay.activeSelf);
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
