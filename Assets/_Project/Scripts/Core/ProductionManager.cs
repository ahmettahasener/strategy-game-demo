namespace StrategyDemo.Core
{
    /// <summary>
    /// Drives instant, unlimited unit production from the selected producer via the unit factory
    /// (Brief #4, #9). Production logic lives under <c>Scripts/Production</c>; behaviour is added
    /// in later slices.
    /// </summary>
    public sealed class ProductionManager : Singleton<ProductionManager>
    {
    }
}
