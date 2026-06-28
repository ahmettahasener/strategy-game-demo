using System.Collections;
using UnityEngine;

namespace StrategyDemo.Core
{
    /// <summary>
    /// A short positional "punch" — eases the transform out in a direction and back, for combat impact
    /// (an attacker's lunge, a target's knockback). It only ever adds and removes <i>its own</i> offset
    /// on top of the current position each frame, so it composes with movement that is driving the same
    /// transform instead of fighting it. Transform-only and hand-written (no tween library), so it adds
    /// no draw calls. Added on demand via <see cref="GetOrAdd"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HitImpact : MonoBehaviour
    {
        private Coroutine _routine;
        private Vector3 _appliedOffset;

        /// <summary>Returns the entity's <see cref="HitImpact"/>, adding one if it has none.</summary>
        public static HitImpact GetOrAdd(GameObject target)
        {
            HitImpact impact = target.GetComponent<HitImpact>();
            return impact != null ? impact : target.AddComponent<HitImpact>();
        }

        /// <summary>Punches the transform <paramref name="distance"/> units along <paramref name="direction"/> and back.</summary>
        public void Nudge(Vector2 direction, float distance, float duration)
        {
            if (distance <= 0f || duration <= 0f || direction.sqrMagnitude < 0.0001f || !isActiveAndEnabled)
            {
                return;
            }

            if (_routine != null)
            {
                StopCoroutine(_routine);
            }

            _routine = StartCoroutine(NudgeRoutine(direction.normalized, distance, duration));
        }

        // Leave the transform where it logically belongs if the entity is pooled mid-punch.
        private void OnDisable()
        {
            if (_appliedOffset != Vector3.zero)
            {
                transform.position -= _appliedOffset;
                _appliedOffset = Vector3.zero;
            }

            _routine = null;
        }

        private IEnumerator NudgeRoutine(Vector2 direction, float distance, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curve = Mathf.Sin(t * Mathf.PI); // 0 → 1 → 0: out and back
                Vector3 desired = (Vector3)(direction * (distance * curve));

                // Replace only the offset we previously applied, preserving any movement this frame.
                transform.position += desired - _appliedOffset;
                _appliedOffset = desired;
                yield return null;
            }

            transform.position -= _appliedOffset;
            _appliedOffset = Vector3.zero;
            _routine = null;
        }
    }
}
