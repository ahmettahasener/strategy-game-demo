using System.Collections;
using StrategyDemo.Core;
using UnityEngine;

namespace StrategyDemo.Units
{
    /// <summary>
    /// Orchestrates "approach then attack" (Brief #10): walks the unit toward a target until it is in
    /// range (<see cref="Collider2D.ClosestPoint"/> distance ≤ the unit's attack range, so building
    /// size is handled), then has the <see cref="AttackEffector"/> deal damage on cooldown until the
    /// target dies. Movement and damage stay in their own components; this only coordinates them.
    /// </summary>
    [RequireComponent(typeof(UnitElement))]
    [RequireComponent(typeof(UnitMovement))]
    [RequireComponent(typeof(AttackEffector))]
    public sealed class UnitCombat : MonoBehaviour
    {
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

        /// <summary>Orders this unit to engage <paramref name="target"/>.</summary>
        public void Attack(GameElement target)
        {
            // Enemy-only targeting is a combat-domain rule, not just an input concern: never engage
            // a null/dead target, ourselves, or a same-faction ally.
            if (target == null || target == _unit || target.IsDead || target.Faction == _unit.Faction)
            {
                return;
            }

            StopCombat();
            _combatRoutine = StartCoroutine(CombatRoutine(target));
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
            Collider2D targetCollider = target.GetComponent<Collider2D>();

            while (target != null && !target.IsDead)
            {
                if (InRange(target, targetCollider))
                {
                    _movement.Stop();
                    _attack.TryAttack(target);
                }
                else if (!_movement.IsMoving && !Approach(target))
                {
                    break; // no walkable cell reaches the target
                }

                yield return null;
            }

            _movement.Stop();
            _combatRoutine = null;
        }

        private bool Approach(GameElement target)
        {
            Vector2Int targetCell = GridManager.Instance.WorldToCell(target.transform.position);
            Vector2Int? approachCell = GridManager.Instance.NearestFreeCell(targetCell);
            return approachCell.HasValue && _movement.MoveTo(approachCell.Value);
        }

        private bool InRange(GameElement target, Collider2D targetCollider)
        {
            Vector2 position = transform.position;
            Vector2 closest = targetCollider != null
                ? targetCollider.ClosestPoint(position)
                : (Vector2)target.transform.position;
            return Vector2.Distance(position, closest) <= _unit.Data.AttackRange;
        }
    }
}
