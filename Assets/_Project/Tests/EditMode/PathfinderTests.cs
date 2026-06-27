using System.Collections.Generic;
using NUnit.Framework;
using StrategyDemo.Grid;
using StrategyDemo.Pathfinding;
using UnityEngine;

namespace StrategyDemo.Tests.EditMode
{
    public sealed class PathfinderTests
    {
        private readonly Pathfinder _pathfinder = new Pathfinder();

        private static GridModel NewGrid(int size)
        {
            return new GridModel(Vector2Int.zero, size, size);
        }

        private static void Block(GridModel grid, params Vector2Int[] cells)
        {
            foreach (Vector2Int cell in cells)
            {
                grid.Occupy(cell, Vector2Int.one);
            }
        }

        [Test]
        public void FindPath_StraightDiagonal_ReturnsInclusivePath()
        {
            GridModel grid = NewGrid(5);

            List<Vector2Int> path = _pathfinder.FindPath(grid, new Vector2Int(0, 0), new Vector2Int(4, 4));

            Assert.AreEqual(new Vector2Int(0, 0), path[0]);
            Assert.AreEqual(new Vector2Int(4, 4), path[path.Count - 1]);
            Assert.AreEqual(5, path.Count); // pure diagonal — one cell per step
        }

        [Test]
        public void FindPath_AroundObstacle_NeverEntersBlockedCell()
        {
            GridModel grid = NewGrid(5);
            // A wall on column x = 2 from y = 0..3, leaving a gap at y = 4 — forces a detour.
            Block(grid,
                new Vector2Int(2, 0), new Vector2Int(2, 1), new Vector2Int(2, 2), new Vector2Int(2, 3));

            List<Vector2Int> path = _pathfinder.FindPath(grid, new Vector2Int(0, 0), new Vector2Int(4, 0));

            Assert.IsNotEmpty(path);
            Assert.AreEqual(new Vector2Int(4, 0), path[path.Count - 1]);
            CollectionAssert.DoesNotContain(path, new Vector2Int(2, 0));
            CollectionAssert.DoesNotContain(path, new Vector2Int(2, 2));
        }

        [Test]
        public void FindPath_DoesNotCutBuildingCorner()
        {
            GridModel grid = NewGrid(3);
            Block(grid, new Vector2Int(1, 0)); // shared orthogonal cell of the (0,0)->(1,1) diagonal

            List<Vector2Int> path = _pathfinder.FindPath(grid, new Vector2Int(0, 0), new Vector2Int(1, 1));

            // Corner-cutting forbidden: must go around, (0,0)->(0,1)->(1,1) = 3 cells, not the
            // 2-cell diagonal that would clip the building corner.
            Assert.AreEqual(3, path.Count);
        }

        [Test]
        public void FindPath_UnreachableTarget_ReturnsEmpty()
        {
            GridModel grid = NewGrid(3);
            Block(grid, new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(1, 2)); // walls off (2,2)

            List<Vector2Int> path = _pathfinder.FindPath(grid, new Vector2Int(0, 0), new Vector2Int(2, 2));

            Assert.IsEmpty(path);
        }

        [Test]
        public void FindPath_TargetOutOfBounds_ReturnsEmpty()
        {
            GridModel grid = NewGrid(3);

            List<Vector2Int> path = _pathfinder.FindPath(grid, new Vector2Int(0, 0), new Vector2Int(9, 9));

            Assert.IsEmpty(path);
        }

        [Test]
        public void FindPath_StartEqualsTarget_ReturnsSingleCell()
        {
            GridModel grid = NewGrid(3);

            List<Vector2Int> path = _pathfinder.FindPath(grid, new Vector2Int(1, 1), new Vector2Int(1, 1));

            Assert.AreEqual(1, path.Count);
            Assert.AreEqual(new Vector2Int(1, 1), path[0]);
        }
    }
}
