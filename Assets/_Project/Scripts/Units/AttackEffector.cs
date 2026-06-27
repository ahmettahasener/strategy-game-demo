using StrategyDemo.Core;
using UnityEngine;
using UnityEngine.Events;

namespace StrategyDemo.Units
{
    /// <summary>
    /// Deals the unit's attack damage on a fixed cooldown — combat timing lives here, not baked into
    /// the unit (composition over inheritance). Damage comes from the unit's data; the cooldown is a
    /// tuning value. A pre-attack <see cref="UnityEvent"/> hook lets a designer wire SFX/VFX in the
    /// inspector without code.
    /// </summary>
    [RequireComponent(typeof(UnitElement))]
    public sealed class AttackEffector : MonoBehaviour
    {
        [SerializeField, Min(0.05f)] private float _interval = 0.8f;
        [SerializeField] private UnityEvent _onAttack;

        private UnitElement _unit;
        private float _nextAttackTime;

        private void Awake()
        {
            _unit = GetComponent<UnitElement>();
        }

        // Reset the cooldown on spawn / pool reuse so a recycled unit can attack immediately.
        private void OnEnable()
        {
            _nextAttackTime = 0f;
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
            return true;
        }
    }
}
