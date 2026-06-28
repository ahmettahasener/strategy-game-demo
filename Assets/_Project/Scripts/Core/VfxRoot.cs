using UnityEngine;

namespace StrategyDemo.Core
{
    /// <summary>
    /// An authored scene container that transient world VFX (one-shot bursts, hit sparks) parent
    /// themselves under at runtime, so they stay grouped instead of littering the scene root. It sits
    /// at identity, so a child's world position and scale are unaffected. Optional: the VFX fall back
    /// to a lazily-created root if no <see cref="VfxRoot"/> is present in the scene.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VfxRoot : MonoBehaviour
    {
        /// <summary>The active VFX container, or null if the scene has none.</summary>
        public static Transform Current { get; private set; }

        private void Awake()
        {
            Current = transform;
        }

        private void OnDestroy()
        {
            if (Current == transform)
            {
                Current = null;
            }
        }
    }
}
