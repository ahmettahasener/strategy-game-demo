using StrategyDemo.Data;
using StrategyDemo.Grid;
using StrategyDemo.UI;
using UnityEngine;

namespace StrategyDemo.Buildings
{
    /// <summary>
    /// Feeds the building roster to the recycling menu and turns a card click into a placement order.
    /// A thin controller: the View renders/recycles, this only routes the "build this" intent to the
    /// <see cref="PlacementController"/> (no game rules in the View).
    /// </summary>
    public sealed class BuildMenuController : MonoBehaviour
    {
        [SerializeField] private RecyclingScrollView _scrollView;
        [SerializeField] private PlacementController _placement;
        [SerializeField] private BuildingData[] _roster;

        private void Start()
        {
            _scrollView.Initialize(_roster, OnBuildingSelected);
        }

        private void OnBuildingSelected(BuildingData building)
        {
            _placement.EnterPlacement(building);
        }
    }
}
