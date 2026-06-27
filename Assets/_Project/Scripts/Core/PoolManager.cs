namespace StrategyDemo.Core
{
    /// <summary>
    /// Central object-pooling service — recycles units, placement overlays, scroll cells and path
    /// markers to cut instantiate/destroy churn and support the draw-call budget (Brief #12).
    /// Pool implementation lives under <c>Scripts/Pooling</c>; behaviour is added in later slices.
    /// </summary>
    public sealed class PoolManager : Singleton<PoolManager>
    {
    }
}
