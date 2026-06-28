using System;

namespace StrategyDemo.Core
{
    /// <summary>
    /// Lightweight global broadcast hub. Systems publish here instead of calling each other
    /// directly, keeping the UI (View) decoupled from the game logic (Model). Payloads are
    /// contracts (interfaces), never concrete types, so the hub depends on no concrete system.
    /// </summary>
    public static class GameEvents
    {
        /// <summary>Selected entity changed; payload is <c>null</c> when nothing is selected.</summary>
        public static event Action<ISelectable> SelectionChanged;

        /// <summary>An entity's health changed (e.g. for health-bar views).</summary>
        public static event Action<IDamageable> HealthChanged;

        /// <summary>An entity reached 0 HP and was destroyed.</summary>
        public static event Action<IDamageable> EntityDied;

        /// <summary>An entity just took a hit; payload carries the entity and the damage amount.</summary>
        public static event Action<IDamageable, int> DamageTaken;

        public static void RaiseSelectionChanged(ISelectable selected)
        {
            SelectionChanged?.Invoke(selected);
        }

        public static void RaiseHealthChanged(IDamageable entity)
        {
            HealthChanged?.Invoke(entity);
        }

        public static void RaiseEntityDied(IDamageable entity)
        {
            EntityDied?.Invoke(entity);
        }

        public static void RaiseDamageTaken(IDamageable entity, int amount)
        {
            DamageTaken?.Invoke(entity, amount);
        }
    }
}
