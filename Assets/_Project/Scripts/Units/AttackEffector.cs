using System.Collections;
using StrategyDemo.Core;
using UnityEngine;
using UnityEngine.Events;

namespace StrategyDemo.Units
{
    /// <summary>
    /// Deals the unit's attack damage on a fixed cooldown — combat timing lives here, not baked into
    /// the unit (composition over inheritance). Damage comes from the unit's data; the cooldown is a
    /// tuning value. A pre-attack <see cref="UnityEvent"/> hook lets a designer wire SFX/VFX in the
    /// inspector without code. On a landed hit it also pops a short "spark" at the target so combat
    /// reads at a glance; the spark reuses a gameplay-atlas sprite, so it adds no draw call.
    /// </summary>
    [RequireComponent(typeof(UnitElement))]
    public sealed class AttackEffector : MonoBehaviour
    {
        [SerializeField, Min(0.05f)] private float _interval = 0.8f;
        [SerializeField] private UnityEvent _onAttack;

        [Header("Hit spark")]
        [SerializeField] private Sprite _hitSparkSprite;
        [SerializeField] private Color _hitSparkColor = new Color(1f, 0.85f, 0.45f, 0.9f);
        [SerializeField] private float _hitSparkSize = 0.55f;
        [SerializeField] private float _hitSparkDuration = 0.18f;

        private UnitElement _unit;
        private float _nextAttackTime;
        private SpriteRenderer _spark;
        private Coroutine _sparkRoutine;

        private void Awake()
        {
            _unit = GetComponent<UnitElement>();
        }

        // Reset the cooldown on spawn / pool reuse so a recycled unit can attack immediately.
        private void OnEnable()
        {
            _nextAttackTime = 0f;
        }

        private void OnDisable()
        {
            if (_sparkRoutine != null)
            {
                StopCoroutine(_sparkRoutine);
                _sparkRoutine = null;
            }

            if (_spark != null)
            {
                _spark.gameObject.SetActive(false);
            }
        }

        /// <summary>Deals damage if the cooldown has elapsed; returns whether an attack happened.</summary>
        public bool TryAttack(IDamageable target)
        {
            if (target == null || target.IsDead || Time.time < _nextAttackTime)
            {
                return false;
            }

            _onAttack?.Invoke();
            target.TakeDamage(_unit.Data.AttackDamage);
            _nextAttackTime = Time.time + _interval;

            if (_hitSparkSprite != null && target is Component component)
            {
                PlaySpark(component.transform.position);
            }

            return true;
        }

        private void PlaySpark(Vector3 position)
        {
            EnsureSpark();
            _spark.transform.position = new Vector3(position.x, position.y, position.z);
            _spark.gameObject.SetActive(true);
            if (_sparkRoutine != null)
            {
                StopCoroutine(_sparkRoutine);
            }

            _sparkRoutine = StartCoroutine(SparkRoutine());
        }

        // A standalone (unparented) renderer so the spark stays at the target and isn't scaled by the
        // attacker. Reused across hits to avoid per-hit instantiate/destroy churn.
        private void EnsureSpark()
        {
            if (_spark != null)
            {
                return;
            }

            var go = new GameObject("HitSpark");
            _spark = go.AddComponent<SpriteRenderer>();
            _spark.sprite = _hitSparkSprite;
            _spark.color = _hitSparkColor;
            var ownerRenderer = GetComponent<SpriteRenderer>();
            _spark.sortingLayerID = ownerRenderer != null ? ownerRenderer.sortingLayerID : 0;
            _spark.sortingOrder = (ownerRenderer != null ? ownerRenderer.sortingOrder : 0) + 5;
        }

        private IEnumerator SparkRoutine()
        {
            float native = Mathf.Max(0.0001f, _hitSparkSprite.bounds.size.x);
            float elapsed = 0f;
            while (elapsed < _hitSparkDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _hitSparkDuration);
                float eased = 1f - (1f - t) * (1f - t);
                float worldSize = _hitSparkSize * Mathf.Lerp(0.5f, 1.15f, eased);
                _spark.transform.localScale = Vector3.one * (worldSize / native);

                Color c = _hitSparkColor;
                c.a = _hitSparkColor.a * (1f - t);
                _spark.color = c;
                yield return null;
            }

            _spark.gameObject.SetActive(false);
            _sparkRoutine = null;
        }
    }
}
