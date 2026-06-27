namespace StrategyDemo.Core
{
    /// <summary>
    /// Runtime allegiance of an entity. Assigned at placement/spawn, never baked into
    /// ScriptableObject data. Player units may only attack <see cref="Enemy"/> entities.
    /// </summary>
    public enum Faction
    {
        Player,
        Enemy
    }
}
