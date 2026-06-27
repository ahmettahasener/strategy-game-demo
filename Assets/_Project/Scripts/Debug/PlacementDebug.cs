using StrategyDemo.Data;
using StrategyDemo.Grid;
using UnityEngine;

namespace StrategyDemo.DebugTools
{
    /// <summary>
    /// Temporary reviewer/testing helper: number keys 1-9 enter placement mode for the matching
    /// building, until the production menu exists. The component always compiles; only the cheat
    /// input runs in the Editor and Development builds, so no cheat ships in a non-development build.
    /// </summary>
    public sealed class PlacementDebug : MonoBehaviour
    {
        [SerializeField] private PlacementController _placement;
        [SerializeField] private BuildingData[] _buildings;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void Update()
        {
            int count = Mathf.Min(_buildings.Length, 9);
            for (int i = 0; i < count; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    _placement.EnterPlacement(_buildings[i]);
                }
            }
        }
#endif
    }
}
