namespace StrategyDemo.Core
{
    /// <summary>
    /// Top-level coordinator and bootstrap for the session. Owns game-wide state and wires the
    /// other manager services together. Behaviour is added in later slices.
    /// </summary>
    public sealed class GameManager : Singleton<GameManager>
    {
    }
}
