using StrategyDemo.Buildings;
using StrategyDemo.Core;
using StrategyDemo.Data;
using StrategyDemo.Units;
using UnityEngine;

namespace StrategyDemo.Scenario
{
    /// <summary>
    /// Builds the fixed reviewer-facing demo battleground at runtime. This is intentionally not a
    /// level editor or a reusable level format; it only makes the default play loop exercise
    /// placement, production, movement, combat, and 0-HP death without debug hotkeys.
    /// </summary>
    public sealed class ScenarioSetup : MonoBehaviour
    {
        [SerializeField] private BuildingSpawn[] _enemyBuildings;
        [SerializeField] private UnitSpawn[] _enemyUnits;

        private readonly BuildingFactory _buildingFactory = new BuildingFactory();
        private readonly UnitFactory _unitFactory = new UnitFactory();

        private void Start()
        {
            SpawnEnemyBuildings();
            SpawnEnemyUnits();
        }

        private void SpawnEnemyBuildings()
        {
            if (_enemyBuildings == null)
            {
                return;
            }

            for (int i = 0; i < _enemyBuildings.Length; i++)
            {
                SpawnBuilding(_enemyBuildings[i], i);
            }
        }

        private void SpawnEnemyUnits()
        {
            if (_enemyUnits == null)
            {
                return;
            }

            for (int i = 0; i < _enemyUnits.Length; i++)
            {
                SpawnUnit(_enemyUnits[i], i);
            }
        }

        private void SpawnBuilding(BuildingSpawn spawn, int index)
        {
            if (spawn.Data == null)
            {
                Debug.LogWarning($"{nameof(ScenarioSetup)} skipped enemy building {index}: data is missing.", this);
                return;
            }

            if (!GridManager.Instance.IsAreaFree(spawn.FootprintOrigin, spawn.Data.Size))
            {
                Debug.LogWarning(
                    $"{nameof(ScenarioSetup)} skipped enemy building {spawn.Data.DisplayName}: footprint is blocked or out of bounds.",
                    this);
                return;
            }

            _buildingFactory.Create(spawn.Data, spawn.FootprintOrigin, Faction.Enemy);
            GridManager.Instance.Occupy(spawn.FootprintOrigin, spawn.Data.Size);
        }

        private void SpawnUnit(UnitSpawn spawn, int index)
        {
            if (spawn.Data == null)
            {
                Debug.LogWarning($"{nameof(ScenarioSetup)} skipped enemy unit {index}: data is missing.", this);
                return;
            }

            if (!GridManager.Instance.IsInBounds(spawn.Cell))
            {
                Debug.LogWarning(
                    $"{nameof(ScenarioSetup)} skipped enemy unit {spawn.Data.DisplayName}: cell is out of bounds.",
                    this);
                return;
            }

            if (!GridManager.Instance.IsAreaFree(spawn.Cell, Vector2Int.one))
            {
                Debug.LogWarning(
                    $"{nameof(ScenarioSetup)} skipped enemy unit {spawn.Data.DisplayName}: cell is blocked.",
                    this);
                return;
            }

            Vector3 worldPosition = GridManager.Instance.CellToWorldCenter(spawn.Cell);
            _unitFactory.Create(spawn.Data, worldPosition, Faction.Enemy);
        }

        [System.Serializable]
        public struct BuildingSpawn
        {
            [SerializeField] private BuildingData _data;
            [SerializeField] private Vector2Int _footprintOrigin;

            public BuildingData Data => _data;
            public Vector2Int FootprintOrigin => _footprintOrigin;
        }

        [System.Serializable]
        public struct UnitSpawn
        {
            [SerializeField] private UnitData _data;
            [SerializeField] private Vector2Int _cell;

            public UnitData Data => _data;
            public Vector2Int Cell => _cell;
        }
    }
}
