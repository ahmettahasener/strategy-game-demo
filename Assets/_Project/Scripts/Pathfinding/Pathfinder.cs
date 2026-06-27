using System.Collections.Generic;
using StrategyDemo.Grid;
using UnityEngine;

namespace StrategyDemo.Pathfinding
{
    /// <summary>
    /// Custom 8-directional A* over a <see cref="GridModel"/> — buildings are obstacles, units are
    /// not. Built from scratch (no NavMesh): a binary <see cref="Heap{T}"/> open set, octile
    /// heuristic, and corner-cutting forbidden so diagonals never clip a building corner (Brief #16).
    /// Pure logic (Unity value types only) so it is fully EditMode-testable.
    /// </summary>
    public sealed class Pathfinder
    {
        private const int OrthogonalCost = 10;
        private const int DiagonalCost = 14;

        /// <summary>
        /// Shortest walkable path from <paramref name="start"/> to <paramref name="target"/>,
        /// inclusive of both. Returns a single-cell list when they are equal, or an empty list when
        /// the target is unreachable or not walkable.
        /// </summary>
        public List<Vector2Int> FindPath(GridModel grid, Vector2Int start, Vector2Int target)
        {
            var path = new List<Vector2Int>();
            if (!grid.IsFree(start) || !grid.IsFree(target))
            {
                return path;
            }

            if (start == target)
            {
                path.Add(start);
                return path;
            }

            var open = new Heap<Node>(grid.Width * grid.Height);
            var openLookup = new Dictionary<Vector2Int, Node>();
            var closed = new HashSet<Vector2Int>();

            var startNode = new Node(start) { GCost = 0, HCost = Heuristic(start, target) };
            open.Add(startNode);
            openLookup[start] = startNode;

            while (open.Count > 0)
            {
                Node current = open.RemoveFirst();
                openLookup.Remove(current.Cell);
                closed.Add(current.Cell);

                if (current.Cell == target)
                {
                    return Reconstruct(current);
                }

                ExpandNeighbours(grid, current, target, open, openLookup, closed);
            }

            return path;
        }

        private static void ExpandNeighbours(
            GridModel grid, Node current, Vector2Int target,
            Heap<Node> open, Dictionary<Vector2Int, Node> openLookup, HashSet<Vector2Int> closed)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    var neighbour = new Vector2Int(current.Cell.x + dx, current.Cell.y + dy);
                    if (!grid.IsFree(neighbour) || closed.Contains(neighbour))
                    {
                        continue;
                    }

                    bool diagonal = dx != 0 && dy != 0;
                    if (diagonal && !CanMoveDiagonally(grid, current.Cell, dx, dy))
                    {
                        continue;
                    }

                    int newG = current.GCost + (diagonal ? DiagonalCost : OrthogonalCost);
                    if (openLookup.TryGetValue(neighbour, out Node existing))
                    {
                        if (newG < existing.GCost)
                        {
                            existing.GCost = newG;
                            existing.Parent = current;
                            open.UpdateItem(existing);
                        }
                    }
                    else
                    {
                        var node = new Node(neighbour)
                        {
                            GCost = newG,
                            HCost = Heuristic(neighbour, target),
                            Parent = current
                        };
                        open.Add(node);
                        openLookup[neighbour] = node;
                    }
                }
            }
        }

        // Forbid cutting between two blocked cells: a diagonal is allowed only when both shared
        // orthogonal cells are walkable, so a unit never clips the corner of a solid building.
        private static bool CanMoveDiagonally(GridModel grid, Vector2Int from, int dx, int dy)
        {
            return grid.IsFree(new Vector2Int(from.x + dx, from.y))
                && grid.IsFree(new Vector2Int(from.x, from.y + dy));
        }

        private static int Heuristic(Vector2Int a, Vector2Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            int min = Mathf.Min(dx, dy);
            int max = Mathf.Max(dx, dy);
            return DiagonalCost * min + OrthogonalCost * (max - min);
        }

        private static List<Vector2Int> Reconstruct(Node target)
        {
            var path = new List<Vector2Int>();
            Node node = target;
            while (node != null)
            {
                path.Add(node.Cell);
                node = node.Parent;
            }

            path.Reverse();
            return path;
        }

        private sealed class Node : IHeapItem<Node>
        {
            public Node(Vector2Int cell)
            {
                Cell = cell;
            }

            public Vector2Int Cell { get; }
            public int GCost { get; set; }
            public int HCost { get; set; }
            public Node Parent { get; set; }
            public int HeapIndex { get; set; }

            public int FCost => GCost + HCost;

            public int CompareTo(Node other)
            {
                int compare = FCost.CompareTo(other.FCost);
                if (compare == 0)
                {
                    compare = HCost.CompareTo(other.HCost);
                }

                return compare;
            }
        }
    }
}
