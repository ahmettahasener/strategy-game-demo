using System.Collections;
using StrategyDemo.Data;
using UnityEngine;

namespace StrategyDemo.Core
{
    /// <summary>
    /// Base for every on-board entity (buildings and units): shared health, damage, death and
    /// selection. Concrete entities derive and supply their stats (from data) and visuals.
    /// </summary>
    public abstract class GameElement : MonoBehaviour, IDamageable, ISelectable
    {
        private int _currentHp;
        private bool _isDead;
        private bool _isSelected;

        /// <summary>Full health for this entity — supplied by the concrete type from its data.</summary>
        public abstract int MaxHp { get; }

        /// <summary>The ScriptableObject definition (icon, name, …) the info panel renders.</summary>
        public abstract EntityData Definition { get; }

        public int CurrentHp => _currentHp;
        public bool IsDead => _isDead;
        public bool IsSelected => _isSelected;
        public Faction Faction { get; private set; } = Faction.Player;

        /// <summary>Sets HP to full. Called by the factory once the entity's data is injected.</summary>
        public void ResetHealth()
        {
            _currentHp = MaxHp;
            _isDead = false;
            GameEvents.RaiseHealthChanged(this);
        }

        /// <summary>Assigns runtime allegiance (called by the factory at spawn/placement).</summary>
        public void SetFaction(Faction faction)
        {
            Faction = faction;
        }

        public void TakeDamage(int amount)
        {
            if (_isDead || amount <= 0)
            {
                return;
            }

            _currentHp = Mathf.Max(0, _currentHp - amount);
            GameEvents.RaiseHealthChanged(this);
            OnDamaged();

            if (_currentHp == 0)
            {
                Die();
            }
        }

        public void OnSelected()
        {
            _isSelected = true;
            SetHighlight(true);
        }

        public void OnDeselected()
        {
            _isSelected = false;
            SetHighlight(false);
        }

        /// <summary>Visual selection feedback; concrete entities decide how to render it.</summary>
        protected abstract void SetHighlight(bool isOn);

        /// <summary>Visual damage feedback; concrete entities decide how to render it.</summary>
        protected virtual void OnDamaged()
        {
        }

        /// <summary>Hook for death visuals/SFX, run just before the object is removed.</summary>
        protected virtual void OnDied()
        {
        }

        /// <summary>Optional death animation; removal waits until this routine completes.</summary>
        protected virtual IEnumerator DeathAnimationRoutine()
        {
            yield break;
        }

        /// <summary>
        /// Removes the entity from the board. Pooled entities can override this to release
        /// themselves instead of being destroyed.
        /// </summary>
        protected virtual void RemoveFromBoard()
        {
            Destroy(gameObject);
        }

        private void Die()
        {
            _isDead = true;
            GameEvents.RaiseEntityDied(this);
            OnDied();
            StartCoroutine(DieRoutine());
        }

        private IEnumerator DieRoutine()
        {
            yield return DeathAnimationRoutine();
            RemoveFromBoard();
        }
    }
}
