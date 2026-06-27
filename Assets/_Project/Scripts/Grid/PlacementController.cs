using System.Collections.Generic;
using StrategyDemo.Buildings;
using StrategyDemo.Core;
using StrategyDemo.Data;
using UnityEngine;

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

            Vector2Int origin = CurrentFootprintOrigin();
            if (GridManager.Instance.IsAreaFree(origin, _current.Size))
            {
                PlaceBuilding(_current, origin);
                CancelPlacement();
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
        /// Single point that creates a placed building. The Factory slice moves this body into a
        /// BuildingFactory; everything else in this controller stays unchanged.
        /// </summary>
        private void PlaceBuilding(BuildingData data, Vector2Int footprintOrigin)
        {
            GameObject instance = Instantiate(data.Prefab);
            instance.transform.position = FootprintCenterWorld(footprintOrigin, data.Size);

            BuildingElement building = instance.GetComponent<BuildingElement>();
            building.Initialize(data, footprintOrigin, Faction.Player);

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

        private Vector3 FootprintCenterWorld(Vector2Int origin, Vector2Int size)
        {
            Vector3 min = GridManager.Instance.CellToWorldCenter(origin);
            Vector3 max = GridManager.Instance.CellToWorldCenter(origin + size - Vector2Int.one);
            return (min + max) * 0.5f;
        }
    }
}
