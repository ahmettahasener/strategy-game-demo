using NUnit.Framework;
using StrategyDemo.Grid;
using UnityEngine;

namespace StrategyDemo.Tests.EditMode
{
    public sealed class GridModelTests
    {
        private static GridModel NewGrid(int width = 5, int height = 5)
        {
            return new GridModel(Vector2Int.zero, width, height);
        }

        [Test]
        public void IsInBounds_TrueInsideRectangle_FalseOutside()
        {
            GridModel grid = NewGrid(4, 3);

            Assert.IsTrue(grid.IsInBounds(new Vector2Int(0, 0)));
            Assert.IsTrue(grid.IsInBounds(new Vector2Int(3, 2)));
            Assert.IsFalse(grid.IsInBounds(new Vector2Int(4, 2)));
            Assert.IsFalse(grid.IsInBounds(new Vector2Int(0, -1)));
        }

        [Test]
        public void IsInBounds_RespectsNonZeroOrigin()
        {
            GridModel grid = new GridModel(new Vector2Int(-2, -2), 4, 4);

            Assert.IsTrue(grid.IsInBounds(new Vector2Int(-2, -2)));
            Assert.IsTrue(grid.IsInBounds(new Vector2Int(1, 1)));
            Assert.IsFalse(grid.IsInBounds(new Vector2Int(2, 0)));
        }

        [Test]
        public void IsFree_TrueWhenEmpty_FalseWhenOccupiedOrOutOfBounds()
        {
            GridModel grid = NewGrid();

            Assert.IsTrue(grid.IsFree(new Vector2Int(1, 1)));

            grid.Occupy(new Vector2Int(1, 1), Vector2Int.one);
            Assert.IsFalse(grid.IsFree(new Vector2Int(1, 1)));

            Assert.IsFalse(grid.IsFree(new Vector2Int(99, 99)));
        }

        [Test]
        public void IsAreaFree_FalseWhenAnyCellOccupied()
        {
            GridModel grid = NewGrid();
            grid.Occupy(new Vector2Int(2, 2), Vector2Int.one);

            // A 2x2 footprint overlapping the occupied cell is not free...
            Assert.IsFalse(grid.IsAreaFree(new Vector2Int(1, 1), new Vector2Int(2, 2)));
            // ...but one clear of it is.
            Assert.IsTrue(grid.IsAreaFree(new Vector2Int(0, 0), new Vector2Int(2, 2)));
        }

        [Test]
        public void IsAreaFree_FalseForNonPositiveSize()
        {
            GridModel grid = NewGrid();

            Assert.IsFalse(grid.IsAreaFree(Vector2Int.zero, Vector2Int.zero));
            Assert.IsFalse(grid.IsAreaFree(Vector2Int.zero, new Vector2Int(-1, 1)));
        }

        [Test]
        public void IsAreaFree_FalseWhenFootprintCrossesBoundary()
        {
            GridModel grid = NewGrid(3, 3);

            Assert.IsFalse(grid.IsAreaFree(new Vector2Int(2, 2), new Vector2Int(2, 2)));
        }

        [Test]
        public void OccupyThenFree_RestoresCells()
        {
            GridModel grid = NewGrid();
            Vector2Int origin = new Vector2Int(1, 1);
            Vector2Int size = new Vector2Int(2, 2);

            grid.Occupy(origin, size);
            Assert.IsFalse(grid.IsAreaFree(origin, size));

            grid.Free(origin, size);
            Assert.IsTrue(grid.IsAreaFree(origin, size));
        }

        [Test]
        public void Constructor_ThrowsOnNonPositiveSize()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(
                () => new GridModel(Vector2Int.zero, 0, 5));
        }
    }
}
