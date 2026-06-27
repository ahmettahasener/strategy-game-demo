namespace StrategyDemo.Core
{
    /// <summary>
    /// Anything with health that can be damaged and destroyed — buildings and units alike.
    /// Guarantees uniform 0-HP death across the game (Brief #11).
    /// </summary>
    public interface IDamageable
    {
        int CurrentHp { get; }
        int MaxHp { get; }
        bool IsDead { get; }

        /// <summary>Applies damage; clamps HP at 0 and triggers death when it reaches 0.</summary>
        void TakeDamage(int amount);
    }
}
