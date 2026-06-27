namespace StrategyDemo.Core
{
    /// <summary>
    /// Tracks the single selected entity and broadcasts changes via
    /// <see cref="GameEvents.SelectionChanged"/> so the info panel can render it (Brief #5).
    /// Clears the selection automatically when the selected entity dies (Brief #11).
    /// </summary>
    public sealed class SelectionManager : Singleton<SelectionManager>
    {
        public ISelectable Current { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            if (Instance == this)
            {
                GameEvents.EntityDied += OnEntityDied;
            }
        }

        protected override void OnDestroy()
        {
            GameEvents.EntityDied -= OnEntityDied;
            base.OnDestroy();
        }

        /// <summary>Selects <paramref name="selectable"/>, deselecting any previous selection.</summary>
        public void Select(ISelectable selectable)
        {
            if (selectable == null || ReferenceEquals(selectable, Current))
            {
                return;
            }

            Current?.OnDeselected();
            Current = selectable;
            Current.OnSelected();
            GameEvents.RaiseSelectionChanged(Current);
        }

        /// <summary>Clears the current selection.</summary>
        public void Deselect()
        {
            if (Current == null)
            {
                return;
            }

            Current.OnDeselected();
            Current = null;
            GameEvents.RaiseSelectionChanged(null);
        }

        private void OnEntityDied(IDamageable entity)
        {
            if (ReferenceEquals(entity, Current))
            {
                Deselect();
            }
        }
    }
}
