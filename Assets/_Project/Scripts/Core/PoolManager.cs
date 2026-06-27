using System;
using System.Collections.Generic;
using StrategyDemo.Pooling;
using UnityEngine;

namespace StrategyDemo.Core
{
    /// <summary>
    /// Central object-pooling service — recycles instances by prefab to cut instantiate/destroy churn
    /// and support the draw-call budget (Brief #12). A from-scratch prefab-keyed pool: <see cref="Get"/>
    /// reuses or creates an instance, <see cref="Release"/> deactivates it and returns it to its pool.
    /// </summary>
    public sealed class PoolManager : Singleton<PoolManager>
    {
        private readonly Dictionary<GameObject, Queue<GameObject>> _pools =
            new Dictionary<GameObject, Queue<GameObject>>();

        /// <summary>An active instance of <paramref name="prefab"/> from its pool, creating one if empty.</summary>
        public GameObject Get(GameObject prefab)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            Queue<GameObject> pool = GetPool(prefab);
            GameObject instance = pool.Count > 0 ? pool.Dequeue() : CreateInstance(prefab);
            instance.SetActive(true);
            return instance;
        }

        /// <summary>
        /// Deactivates <paramref name="instance"/> and returns it to its pool. Instances not created by
        /// this pool are destroyed instead (fail-soft).
        /// </summary>
        public void Release(GameObject instance)
        {
            // Fail-soft on null; ignore double-release (an already-released instance is inactive,
            // so re-queuing it would hand the same object to two callers).
            if (instance == null || !instance.activeSelf)
            {
                return;
            }

            var pooled = instance.GetComponent<PooledObject>();
            if (pooled == null || pooled.SourcePrefab == null)
            {
                Destroy(instance);
                return;
            }

            instance.SetActive(false);
            GetPool(pooled.SourcePrefab).Enqueue(instance);
        }

        private GameObject CreateInstance(GameObject prefab)
        {
            GameObject instance = Instantiate(prefab, transform);
            instance.AddComponent<PooledObject>().SourcePrefab = prefab;
            return instance;
        }

        private Queue<GameObject> GetPool(GameObject prefab)
        {
            if (!_pools.TryGetValue(prefab, out Queue<GameObject> pool))
            {
                pool = new Queue<GameObject>();
                _pools[prefab] = pool;
            }

            return pool;
        }
    }
}
