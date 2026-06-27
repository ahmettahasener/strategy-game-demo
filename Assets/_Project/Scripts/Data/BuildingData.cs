using System.Collections.Generic;
using UnityEngine;

namespace StrategyDemo.Data
{
    /// <summary>
    /// Definition of a placeable building: grid footprint, spawn point, and optional production
    /// (Brief #2, #3, #8, #9). Only producers expose a non-empty <see cref="ProducibleUnits"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "StrategyDemo/Data/Building", fileName = "BuildingData")]
    public sealed class BuildingData : EntityData
    {
        [Header("Placement")]
        [SerializeField] private Vector2Int _size = Vector2Int.one;
        [SerializeField] private Vector2Int _spawnPointOffset;

        [Header("Production")]
        [SerializeField] private bool _isProducer;
        [SerializeField] private List<UnitData> _producibleUnits = new List<UnitData>();

        /// <summary>Footprint in grid cells (Brief #3).</summary>
        public Vector2Int Size => _size;

        /// <summary>Spawn cell relative to the building origin; produced units emerge here (Brief #6).</summary>
        public Vector2Int SpawnPointOffset => _spawnPointOffset;

        public bool IsProducer => _isProducer;

        /// <summary>Units this building can produce (Brief #5); empty for non-producers.</summary>
        public IReadOnlyList<UnitData> ProducibleUnits => _producibleUnits;
    }
}
