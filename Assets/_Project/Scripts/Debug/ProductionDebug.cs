using StrategyDemo.Core;
using UnityEngine;

namespace StrategyDemo.DebugTools
{
    /// <summary>
    /// Temporary reviewer/testing helper: point at a producer building and press Q/W/E to produce
    /// its 1st/2nd/3rd unit, until the Selection/Info-Panel slice drives production from the selected
    /// building. The component always compiles; only the cheat input runs in the Editor and
    /// Development builds. Picks the building under the pointer with a physics query (no scene scan).
    /// </summary>
    public sealed class ProductionDebug : MonoBehaviour
    {
        [SerializeField] private InputReader _input;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static readonly KeyCode[] ProduceKeys = { KeyCode.Q, KeyCode.W, KeyCode.E };

        private void Update()
        {
            for (int i = 0; i < ProduceKeys.Length; i++)
            {
                if (Input.GetKeyDown(ProduceKeys[i]))
                {
                    TryProduce(i);
                }
            }
        }

        private void TryProduce(int unitIndex)
        {
            Collider2D hit = Physics2D.OverlapPoint(_input.PointerWorldPosition);
            if (hit == null)
            {
                return;
            }

            IProducer producer = hit.GetComponent<IProducer>();
            if (producer == null || !producer.CanProduce || unitIndex >= producer.ProducibleUnits.Count)
            {
                return;
            }

            ProductionManager.Instance.Produce(producer, producer.ProducibleUnits[unitIndex]);
        }
#endif
    }
}
