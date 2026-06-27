using StrategyDemo.Core;
using StrategyDemo.Grid;
using UnityEngine;

namespace StrategyDemo.Units
{
    /// <summary>
    /// Translates a right-click into a move order for the selected unit: paths it to the clicked
    /// cell (Brief #6). Right-clicks during placement belong to placement (cancel), not movement.
    /// </summary>
    public sealed class UnitCommandController : MonoBehaviour
    {
        [SerializeField] private InputReader _input;
        [SerializeField] private PlacementController _placement;

        private void OnEnable()
        {
            _input.SecondaryPressed += OnSecondaryPressed;
        }

        private void OnDisable()
        {
            _input.SecondaryPressed -= OnSecondaryPressed;
        }

        private void OnSecondaryPressed()
        {
            if (_placement != null && _placement.IsPlacing)
            {
                return;
            }

            if (!(SelectionManager.Instance.Current is UnitElement unit))
            {
                return;
            }

            GameElement target = EntityUnderPointer();
            var combat = unit.GetComponent<UnitCombat>();

            if (target != null && target != unit && target.Faction != unit.Faction)
            {
                combat?.Attack(target);
            }
            else
            {
                // Move order: cancel any ongoing attack, then walk to the clicked cell.
                combat?.StopCombat();
                Vector2Int targetCell = GridManager.Instance.WorldToCell(_input.PointerWorldPosition);
                unit.GetComponent<UnitMovement>()?.MoveTo(targetCell);
            }
        }

        private GameElement EntityUnderPointer()
        {
            Collider2D hit = Physics2D.OverlapPoint(_input.PointerWorldPosition);
            return hit != null ? hit.GetComponent<GameElement>() : null;
        }
    }
}
