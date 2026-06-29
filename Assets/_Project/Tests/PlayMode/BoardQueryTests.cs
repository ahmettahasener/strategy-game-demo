using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using StrategyDemo.Core;
using StrategyDemo.Data;
using StrategyDemo.Units;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace StrategyDemo.Tests.PlayMode
{
    public sealed class BoardQueryTests
    {
        private GameObject _gridRoot;
        private GameObject _productionRoot;
        private GameObject _unit;
        private Tile _tile;
        private UnitData _unitData;

        [SetUp]
        public void SetUp()
        {
            _tile = ScriptableObject.CreateInstance<Tile>();

            _gridRoot = new GameObject("GridRoot");
            _gridRoot.SetActive(false);
            _gridRoot.AddComponent<UnityEngine.Grid>();

            var ground = new GameObject("Ground");
            ground.transform.SetParent(_gridRoot.transform);
            var tilemap = ground.AddComponent<Tilemap>();
            ground.AddComponent<TilemapRenderer>();

            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), _tile);
                }
            }

            GridManager gridManager = _gridRoot.AddComponent<GridManager>();
            typeof(GridManager)
                .GetField("_groundTilemap", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(gridManager, tilemap);

            _gridRoot.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            if (_gridRoot != null)
            {
                Object.DestroyImmediate(_gridRoot);
            }

            if (_tile != null)
            {
                Object.DestroyImmediate(_tile);
            }

            if (_unit != null)
            {
                Object.DestroyImmediate(_unit);
            }

            if (_productionRoot != null)
            {
                Object.DestroyImmediate(_productionRoot);
            }

            if (_unitData != null)
            {
                Object.DestroyImmediate(_unitData);
            }
        }

        [Test]
        public void NearestOpenCell_WhenOriginContainsUnit_ReturnsAdjacentCell()
        {
            var origin = new Vector2Int(2, 2);
            _unit = SpawnUnitAt(origin);

            Vector2Int? result = BoardQuery.NearestOpenCell(origin);

            Assert.IsTrue(result.HasValue);
            Assert.AreNotEqual(origin, result.Value);
            Assert.IsTrue(GridManager.Instance.IsInBounds(result.Value));
            Assert.IsTrue(GridManager.Instance.IsAreaFree(result.Value, Vector2Int.one));
        }

        [Test]
        public void NearestOpenCellTo_WhenUnitAlreadyAdjacentToOccupiedCell_ReturnsCurrentCell()
        {
            var clicked = new Vector2Int(2, 2);
            var from = new Vector2Int(1, 2);
            GridManager.Instance.Occupy(clicked, Vector2Int.one);

            Vector2Int? result = BoardQuery.NearestOpenCellTo(clicked, from);

            Assert.AreEqual(from, result);
        }

        [Test]
        public void NearestOpenCellAround_WhenFootprintClicked_ReturnsReachableNearSideCell()
        {
            var from = new Vector2Int(0, 2);
            var footprint = new[]
            {
                new Vector2Int(2, 2),
                new Vector2Int(3, 2),
                new Vector2Int(2, 3),
                new Vector2Int(3, 3)
            };
            GridManager.Instance.Occupy(new Vector2Int(2, 2), new Vector2Int(2, 2));

            Vector2Int? result = BoardQuery.NearestOpenCellAround(footprint, from);

            Assert.AreEqual(new Vector2Int(1, 2), result);
        }

        [Test]
        public void NearestOpenCellAround_WhenNoAdjacentCellReachable_ReturnsNull()
        {
            var from = new Vector2Int(0, 0);
            var footprint = new[]
            {
                new Vector2Int(2, 2)
            };
            GridManager.Instance.Occupy(Vector2Int.one, new Vector2Int(3, 3));

            Vector2Int? result = BoardQuery.NearestOpenCellAround(footprint, from);

            Assert.IsFalse(result.HasValue);
        }

        [Test]
        public void NearestOpenCell_WhenEveryCellOccupied_ReturnsNull()
        {
            GridManager.Instance.Occupy(Vector2Int.zero, new Vector2Int(5, 5));

            Vector2Int? result = BoardQuery.NearestOpenCell(new Vector2Int(2, 2));

            Assert.IsFalse(result.HasValue);
        }

        [Test]
        public void Produce_WhenBoardHasNoOpenSpawnCell_ReturnsNullAndRaisesActionDenied()
        {
            _unitData = ScriptableObject.CreateInstance<UnitData>();
            _productionRoot = new GameObject("ProductionManager");
            ProductionManager production = _productionRoot.AddComponent<ProductionManager>();
            var producer = new TestProducer(new Vector2Int(2, 2), _unitData);
            GridManager.Instance.Occupy(Vector2Int.zero, new Vector2Int(5, 5));

            bool actionDenied = false;
            void OnActionDenied()
            {
                actionDenied = true;
            }

            UnitElement result;
            GameEvents.ActionDenied += OnActionDenied;
            try
            {
                result = production.Produce(producer, _unitData);
            }
            finally
            {
                GameEvents.ActionDenied -= OnActionDenied;
            }

            Assert.IsNull(result);
            Assert.IsTrue(actionDenied);
        }

        private static GameObject SpawnUnitAt(Vector2Int cell)
        {
            var unit = new GameObject("Unit");
            unit.transform.position = GridManager.Instance.CellToWorldCenter(cell);
            unit.AddComponent<BoxCollider2D>();
            unit.AddComponent<UnitElement>();
            return unit;
        }

        private sealed class TestProducer : IProducer
        {
            private readonly IReadOnlyList<UnitData> _producibleUnits;

            public TestProducer(Vector2Int spawnCell, UnitData unit)
            {
                SpawnCell = spawnCell;
                _producibleUnits = new[] { unit };
            }

            public bool CanProduce => true;
            public IReadOnlyList<UnitData> ProducibleUnits => _producibleUnits;
            public Vector2Int SpawnCell { get; }
        }
    }
}
