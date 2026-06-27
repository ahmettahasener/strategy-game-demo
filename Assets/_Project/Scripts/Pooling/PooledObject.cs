using UnityEngine;

namespace StrategyDemo.Pooling
{
    /// <summary>
    /// Tags a pooled instance with the prefab it was created from, so <c>PoolManager</c> can return
    /// it to the correct pool on release.
    /// </summary>
    public sealed class PooledObject : MonoBehaviour
    {
        public GameObject SourcePrefab { get; set; }
    }
}
