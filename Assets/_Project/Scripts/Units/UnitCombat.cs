using System.Collections;
using System.Collections.Generic;
using StrategyDemo.Buildings;
using StrategyDemo.Core;
using UnityEngine;

namespace StrategyDemo.Units
{
    /// <summary>
    /// Orchestrates "approach then attack" (Brief #10) on the grid: the unit walks to the nearest free
    /// cell within its attack range (counted in cells) of the target's footprint, settles on that cell
    /// centre, then has the <see cref="AttackEffector"/> deal damage on cooldown until the target dies.
    /// Reasoning in whole cells (rather than raw world distance) keeps units centred on the grid and
    /// makes firing range predictable. Movement and damage stay in their own components; this only
    /// coordinates them.
    /// </summary>
    [RequireComponent(typeof(UnitElement))]
    [RequireComponent(typeof(UnitMovement))]
    [RequireComponent(typeof(AttackEffector))]
    public sealed class UnitCombat : MonoBehaviour
    {
        // Grid step costs, matching the A* pathfinder, so the chosen firing cell minimises real travel.
        private const int OrthogonalStepCost = 10;
        private const int DiagonalStepCost = 14;

        // Scratch lists reused across calls. The combat helpers run synchronously (no yields between
        // filling and reading them), and coroutines never interleave mid-helper, so sharing is safe.
        private static readonly List<Vector2Int> Footprint = new List<Vector2Int>();
        private static readonly List<Vector2Int> Candidates = new List<Vector2Int>();

        private UnitElement _unit;
        private UnitMovement _movement;
        private AttackEffector _attack;
        private Coroutine _combatRoutine;

        private void Awake()
        {
            _unit = GetComponent<UnitElement>();
            _movement = GetComponent<UnitMovement>();
            _attack = GetComponent<AttackEffector>();
        }

        // Clear combat state when pooled (deactivation already stops the coroutine).
        private void OnDisable()
        {
            StopCombat();
        }

        /// <summary>Orders this unit to engage <paramref name="target"/>.</summary>
        public void Attack(GameElement target)
        {
            if (!CanAttack(target))
            {
                return;
            }

            StopCombat();
            // Cancel any move order already in progress so the unit re-approaches the target instead of
            // finishing its previous destination first.
            _movement.Stop();
            _combatRoutine = StartCoroutine(CombatRoutine(target));
        }

        /// <summary>
        /// Enemy-only targeting is a combat-domain rule, not just an input concern (Brief #10): a unit
        /// may engage only a live entity of another faction — never a null/dead target, itself, or a
        /// same-faction ally. Pure and side-effect free so it can be unit-tested without running combat.
        /// </summary>
        public bool CanAttack(GameElement target)
        {
            return target != null && target != _unit && !target.IsDead && target.Faction != _unit.Faction;
        }

        /// <summary>Cancels any ongoing attack (e.g. on a new move order).</summary>
        public void StopCombat()
        {
            if (_combatRoutine != null)
            {
                StopCoroutine(_combatRoutine);
                _combatRoutine = null;
            }
        }

        private IEnumerator CombatRoutine(GameElement target)
        {
            int rangeCells = Mathf.Max(1, _unit.Data.AttackRangeCells);

            while (target != null && !target.IsDead)
            {
                // Never interrupt a walk mid-cell: let the unit reach the destination cell centre so it
                // doesn't stop on a grid line. We only act once it has settled.
                if (_movement.IsMoving)
                {
                    yield return null;
                    continue;
                }

                if (InRange(target, rangeCells))
                {
                    _attack.TryAttack(target);
                }
                else if (FindFiringCell(target, rangeCells) is Vector2Int firingCell
                    && _movement.MoveTo(firingCell))
                {
                    // Walking to the firing cell; the next iterations wait for arrival.
                }
                else
                {
                    break; // no reachable cell within range
                }

                yield return null;
            }

            _movement.Stop();
            _combatRoutine = null;
        }

        // True when the unit's cell is within rangeCells (Chebyshev, matching 8-direction movement) of
        // any cell of the target's footprint.
        private bool InRange(GameElement target, int rangeCells)
        {
            CollectFootprint(target);
            Vector2Int unitCell = GridManager.Instance.WorldToCell(transform.position);
            for (int i = 0; i < Footprint.Count; i++)
            {
                if (Chebyshev(unitCell, Footprint[i]) <= rangeCells)
                {
                    return true;
                }
            }

            return false;
        }

        // The reachable, building-free cell within range of the footprint reachable for the least travel
        // cost — measured in real path cost (diagonals cost more than orthogonals), not step count, so a
        // unit lined up with its target closes straight in instead of drifting diagonally for the same
        // number of steps.
        private Vector2Int? FindFiringCell(GameElement target, int rangeCells)
        {
            Vector2Int unitCell = GridManager.Instance.WorldToCell(transform.position);
            CollectFootprint(target);
            CollectCandidates(rangeCells);
            Candidates.Sort((a, b) => Octile(unitCell, a).CompareTo(Octile(unitCell, b)));

            Vector2Int? best = null;
            int bestCost = int.MaxValue;
            for (int i = 0; i < Candidates.Count; i++)
            {
                Vector2Int candidate = Candidates[i];
                // Octile distance lower-bounds the true path cost, so once a candidate's distance reaches
                // the best cost found, no later (farther) candidate can beat it.
                if (Octile(unitCell, candidate) >= bestCost)
                {
                    break;
                }

                List<Vector2Int> path = GridManager.Instance.FindPath(unitCell, candidate);
                if (path.Count <= 1)
                {
                    continue; // unreachable
                }

                int cost = PathCost(path);
                if (cost < bestCost)
                {
                    bestCost = cost;
                    best = candidate;
                }
            }

            return best;
        }

        private static int PathCost(List<Vector2Int> path)
        {
            int cost = 0;
            for (int i = 1; i < path.Count; i++)
            {
                Vector2Int step = path[i] - path[i - 1];
                cost += step.x != 0 && step.y != 0 ? DiagonalStepCost : OrthogonalStepCost;
            }

            return cost;
        }

        // Diagonal-aware grid distance (same cost model as the pathfinder); a lower bound on path cost.
        private static int Octile(Vector2Int a, Vector2Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            int min = Mathf.Min(dx, dy);
            int max = Mathf.Max(dx, dy);
            return DiagonalStepCost * min + OrthogonalStepCost * (max - min);
        }

        private void CollectFootprint(GameElement target)
        {
            Footprint.Clear();
            if (target is BuildingElement building)
            {
                Vector2Int origin = building.FootprintOrigin;
                Vector2Int size = building.FootprintSize;
                for (int x = 0; x < size.x; x++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        Footprint.Add(new Vector2Int(origin.x + x, origin.y + y));
                    }
                }
            }
            else
            {
                Footprint.Add(GridManager.Instance.WorldToCell(target.transform.position));
            }
        }

        // Building-free, in-bounds cells outside the footprint but within rangeCells of it.
        private void CollectCandidates(int rangeCells)
        {
            Candidates.Clear();
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            for (int i = 0; i < Footprint.Count; i++)
            {
                minX = Mathf.Min(minX, Footprint[i].x);
                minY = Mathf.Min(minY, Footprint[i].y);
                maxX = Mathf.Max(maxX, Footprint[i].x);
                maxY = Mathf.Max(maxY, Footprint[i].y);
            }

            for (int x = minX - rangeCells; x <= maxX + rangeCells; x++)
            {
                for (int y = minY - rangeCells; y <= maxY + rangeCells; y++)
                {
                    var cell = new Vector2Int(x, y);
                    int distance = ChebyshevToFootprint(cell);
                    if (distance >= 1 && distance <= rangeCells
                        && GridManager.Instance.IsInBounds(cell)
                        && GridManager.Instance.IsAreaFree(cell, Vector2Int.one))
                    {
                        Candidates.Add(cell);
                    }
                }
            }
        }

        private int ChebyshevToFootprint(Vector2Int cell)
        {
            int min = int.MaxValue;
            for (int i = 0; i < Footprint.Count; i++)
            {
                min = Mathf.Min(min, Chebyshev(cell, Footprint[i]));
            }

            return min;
        }

        private static int Chebyshev(Vector2Int a, Vector2Int b)
        {
            return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
        }
    }
}
