using UnityEngine;

namespace StrategyDemo.Core
{
    /// <summary>
    /// Gently breathes an object's scale with a sine wave while it is active — used on selection rings
    /// so a selected entity reads as "live" rather than statically highlighted. Captures the scale it
    /// is enabled at as the baseline, so it composes with rings sized at runtime (buildings).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PulseScale : MonoBehaviour
    {
        [SerializeField, Range(0f, 0.5f)] private float _amplitude = 0.06f;
        [SerializeField] private float _frequency = 2.2f; // pulses per second

        private Vector3 _baseScale;
        private float _time;

        private void OnEnable()
        {
            _baseScale = transform.localScale;
            _time = 0f;
        }

        private void OnDisable()
        {
            transform.localScale = _baseScale;
        }

        private void Update()
        {
            _time += Time.deltaTime;
            float scale = 1f + Mathf.Sin(_time * _frequency * 2f * Mathf.PI) * _amplitude;
            transform.localScale = _baseScale * scale;
        }
    }
}
