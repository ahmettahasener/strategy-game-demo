using System.Collections.Generic;
using StrategyDemo.Buildings;
using StrategyDemo.Core;
using StrategyDemo.Data;
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

        private readonly List<Vector2Int> _footprintBuffer = new List<Vector2Int>();
        private readonly BuildingFactory _buildingFactory = new BuildingFactory();
        private BuildingData _current;
        private bool _isPlacing;

        public bool IsPlacing => _isPlacing;

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
            _isPlacing = true;
        }

        public void CancelPlacement()
        {
            _isPlacing = false;
            _current = null;
            _preview.Clear();
        }

        private void Update()
        {
            if (!_isPlacing)
            {
                return;
            }

            Vector2Int origin = CurrentFootprintOrigin();
            UpdateFootprintBuffer(origin, _current.Size);
            bool isValid = GridManager.Instance.IsAreaFree(origin, _current.Size);
            _preview.Show(_footprintBuffer, isValid);
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
            if (GridManager.Instance.IsAreaFree(origin, _current.Size))
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
            _buildingFactory.Create(data, footprintOrigin, Faction.Player);
            GridManager.Instance.Occupy(footprintOrigin, data.Size);
        }

        private Vector2Int CurrentFootprintOrigin()
        {
            Vector2Int pointerCell = GridManager.Instance.WorldToCell(_input.PointerWorldPosition);
            return pointerCell - new Vector2Int(_current.Size.x / 2, _current.Size.y / 2);
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
