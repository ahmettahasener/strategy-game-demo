namespace StrategyDemo.Core
{
    /// <summary>
    /// Something the player can select with left-click. The info panel renders the selected
    /// entity; the selection highlight is toggled through this contract.
    /// </summary>
    public interface ISelectable
    {
        bool IsSelected { get; }
        void OnSelected();
        void OnDeselected();
    }
}
