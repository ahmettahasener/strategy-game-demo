using UnityEngine;

namespace StrategyDemo.Core
{
    /// <summary>
    /// Base for app-wide manager services that need a single, globally reachable instance.
    /// Used <b>only</b> for genuine coordination points — not as a default for every class.
    /// Single-scene demo, so no <c>DontDestroyOnLoad</c>: the instance lives and dies with the scene.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = (T)this;
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
