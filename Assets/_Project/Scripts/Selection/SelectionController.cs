using StrategyDemo.Core;
using StrategyDemo.Grid;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StrategyDemo.Selection
{
    /// <summary>
    /// Turns a left-click into a selection: picks the entity under the pointer and asks the
    /// <see cref="SelectionManager"/> to select it, or clears the selection when clicking empty
    /// ground. Ignores clicks that land on UI or happen during building placement.
    /// </summary>
    public sealed class SelectionController : MonoBehaviour
    {
        [SerializeField] private InputReader _input;
        [SerializeField] private PlacementController _placement;

        private void OnEnable()
        {
            _input.PrimaryPressed += OnPrimaryPressed;
        }

        private void OnDisable()
        {
            _input.PrimaryPressed -= OnPrimaryPressed;
        }

        private void OnPrimaryPressed()
        {
            // Clicks on UI (e.g. the info-panel unit cards) must not select/deselect the world.
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            // While placing a building the left-click belongs to placement, not selection.
            if (_placement != null && _placement.IsPlacing)
            {
                return;
            }

            Collider2D hit = Physics2D.OverlapPoint(_input.PointerWorldPosition);
            ISelectable selectable = hit != null ? hit.GetComponent<ISelectable>() : null;

            if (selectable != null)
            {
                SelectionManager.Instance.Select(selectable);
            }
            else
            {
                SelectionManager.Instance.Deselect();
            }
        }
    }
}
