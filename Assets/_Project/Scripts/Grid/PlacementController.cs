using System.Collections.Generic;
using StrategyDemo.Buildings;
using StrategyDemo.Core;
using StrategyDemo.Data;
using StrategyDemo.Units;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StrategyDemo.Grid
{
    /// <summary>
    /// Drives building placement (Brief #3): follows the pointer, validates the footprint against
    /// the grid, shows the green/red preview, and commits on left-click or cancels on right-click.
    /// No UI logic — placement is entered by a caller (debug hotkey now, production menu later).
    /// </summary>
    public sealed class PlacementController : MonoBehaviour
    {
        [SerializeField] private InputReader _input;
        [SerializeField] private PlacementPreview _preview;
        [SerializeField] private Transform _buildingsRoot; // parent for placed buildings (hierarchy tidiness)

        private readonly List<Vector2Int> _footprintBuffer = new List<Vector2Int>();
        private readonly BuildingFactory _buildingFactory = new BuildingFactory();
        private static readonly Collider2D[] OverlapBuffer = new Collider2D[8];
        private BuildingData _current;
        private bool _isPlacing;

        public bool IsPlacing => _isPlacing;

        /// <summary>Raised when placement mode is entered (e.g. to emphasise the grid).</summary>
        public event System.Action PlacementStarted;

        /// <summary>Raised when placement mode ends (placed-and-exited or cancelled).</summary>
        public event System.Action PlacementEnded;

        private void OnEnable()
        {
            _input.PrimaryPressed += OnPrimaryPressed;
            _input.SecondaryPressed += OnSecondaryPressed;
        }

        private void OnDisable()
        {
            _input.PrimaryPressed -= OnPrimaryPressed;
            _input.SecondaryPressed -= OnSecondaryPressed;
        }

        /// <summary>Enters placement mode for the given building.</summary>
        public void EnterPlacement(BuildingData data)
        {
            if (data == null)
            {
                return;
            }

            // Entering build mode clears any unit selection so a right-click (which cancels
            // placement) can't also issue a move order to a still-selected unit.
            SelectionManager.Instance?.Deselect();
            _current = data;
            bool wasPlacing = _isPlacing;
            _isPlacing = true;
            if (!wasPlacing)
            {
                PlacementStarted?.Invoke();
            }
        }

        public void CancelPlacement()
        {
            bool wasPlacing = _isPlacing;
            _isPlacing = false;
            _current = null;
            _preview.Clear();
            if (wasPlacing)
            {
                PlacementEnded?.Invoke();
            }
        }

        private void Update()
        {
            if (!_isPlacing)
            {
                return;
            }

            // Don't paint a footprint on the cell hidden behind a UI panel the pointer is over;
            // commit (OnPrimaryPressed) already ignores those clicks, so the preview shouldn't show.
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                _preview.Clear();
                return;
            }

            Vector2Int origin = CurrentFootprintOrigin();
            UpdateFootprintBuffer(origin, _current.Size);
            _preview.Show(_footprintBuffer, IsPlaceable(origin, _current.Size));
        }

        private void OnPrimaryPressed()
        {
            if (!_isPlacing)
            {
                return;
            }

            // Clicking a UI panel (e.g. another build-menu card) must route through that card,
            // not commit a building on the cell hidden behind the panel.
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Vector2Int origin = CurrentFootprintOrigin();
            if (IsPlaceable(origin, _current.Size))
            {
                PlaceBuilding(_current, origin);
                // Stay in placement mode so several buildings can be placed; right-click exits.
                // This also keeps selection from firing on the same click that placed a building.
            }
        }

        private void OnSecondaryPressed()
        {
            if (_isPlacing)
            {
                CancelPlacement();
            }
        }

        /// <summary>
        /// Commits a building at the footprint: the factory creates it, then the grid marks the
        /// cells occupied (the placement-side effect that stays the controller's responsibility).
        /// </summary>
        private void PlaceBuilding(BuildingData data, Vector2Int footprintOrigin)
        {
            _buildingFactory.Create(data, footprintOrigin, Faction.Player, _buildingsRoot);
            GridManager.Instance.Occupy(footprintOrigin, data.Size);
        }

        private Vector2Int CurrentFootprintOrigin()
        {
            Vector2Int pointerCell = GridManager.Instance.WorldToCell(_input.PointerWorldPosition);
            return pointerCell - new Vector2Int(_current.Size.x / 2, _current.Size.y / 2);
        }

        // A footprint is placeable when the grid cells are free of buildings AND no unit is standing on
        // them. Units never occupy the grid (so they don't become pathfinding obstacles), so we probe
        // for them with physics instead of grid occupancy — keeping the grid model buildings-only.
        private bool IsPlaceable(Vector2Int origin, Vector2Int size)
        {
            return GridManager.Instance.IsAreaFree(origin, size) && !FootprintHasUnit(origin, size);
        }

        private bool FootprintHasUnit(Vector2Int origin, Vector2Int size)
        {
            float radius = CellWorldSize() * 0.45f; // just under half a cell: only a unit on the cell counts
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector2 center =
                        GridManager.Instance.CellToWorldCenter(new Vector2Int(origin.x + x, origin.y + y));
                    int count = Physics2D.OverlapCircleNonAlloc(center, radius, OverlapBuffer);
                    for (int i = 0; i < count; i++)
                    {
                        if (OverlapBuffer[i].GetComponentInParent<UnitElement>() != null)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static float CellWorldSize()
        {
            Vector3 origin = GridManager.Instance.CellToWorldCenter(Vector2Int.zero);
            Vector3 neighbour = GridManager.Instance.CellToWorldCenter(Vector2Int.right);
            return Mathf.Abs(neighbour.x - origin.x);
        }

        private void UpdateFootprintBuffer(Vector2Int origin, Vector2Int size)
        {
            _footprintBuffer.Clear();
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    _footprintBuffer.Add(new Vector2Int(origin.x + x, origin.y + y));
                }
            }
        }
    }
}
