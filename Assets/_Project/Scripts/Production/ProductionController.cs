using StrategyDemo.Core;
using StrategyDemo.Data;
using StrategyDemo.UI;
using UnityEngine;

namespace StrategyDemo.Production
{
    /// <summary>
    /// Bridges the info panel's "produce this unit" intent to the production system: resolves the
    /// currently selected producer and asks the <see cref="ProductionManager"/> to make the unit.
    /// Keeps the production rule out of the View (MVC) while staying a thin translator.
    /// </summary>
    public sealed class ProductionController : MonoBehaviour
    {
        [SerializeField] private InfoPanelView _infoPanel;

        private void OnEnable()
        {
            _infoPanel.ProduceRequested += OnProduceRequested;
        }

        private void OnDisable()
        {
            _infoPanel.ProduceRequested -= OnProduceRequested;
        }

        private void OnProduceRequested(UnitData unit)
        {
            // Production belongs to the player's own buildings only (defence in depth: the info panel
            // already hides produce cards for enemy producers).
            if (SelectionManager.Instance.Current is IProducer producer
                && producer is GameElement element && element.Faction == Faction.Player)
            {
                ProductionManager.Instance.Produce(producer, unit);
            }
        }
    }
}
