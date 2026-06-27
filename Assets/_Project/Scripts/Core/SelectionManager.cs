namespace StrategyDemo.Core
{
    /// <summary>
    /// Tracks the currently selected entity and broadcasts changes via
    /// <see cref="GameEvents.SelectionChanged"/> for the info panel to render (Brief #5).
    /// Behaviour is added in later slices.
    /// </summary>
    public sealed class SelectionManager : Singleton<SelectionManager>
    {
    }
}
