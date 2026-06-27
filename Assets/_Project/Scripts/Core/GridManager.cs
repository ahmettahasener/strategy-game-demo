namespace StrategyDemo.Core
{
    /// <summary>
    /// Owns the logical board grid and placement validation (Brief #3) and serves cell/occupancy
    /// queries to placement and pathfinding. The grid model lives under <c>Scripts/Grid</c>;
    /// behaviour is added in later slices.
    /// </summary>
    public sealed class GridManager : Singleton<GridManager>
    {
    }
}
