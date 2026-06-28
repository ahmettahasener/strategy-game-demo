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
        [SerializeField] private Sprite _deathPoofSprite;
        [SerializeField] private Color _deathPoofColor = new Color(1f, 0.95f, 0.8f, 0.9f);
        [SerializeField, Range(0.2f, 3f)] private float _deathPoofScale = 1.1f;

        [SerializeField] private Sprite _spawnDustSprite;
        [SerializeField] private Color _spawnDustColor = new Color(1f, 0.97f, 0.85f, 0.8f);
        [SerializeField, Range(0.2f, 3f)] private float _spawnDustScale = 1.15f;
        [SerializeField, Range(0.1f, 1f)] private float _spawnDustFlatten = 0.45f;

        private int _currentHp;
        private bool _isDead;
        private bool _isSelected;
        private Coroutine _spawnPopRoutine;

        /// <summary>Full health for this entity — supplied by the concrete type from its data.</summary>
        public abstract int MaxHp { get; }

        /// <summary>The ScriptableObject definition (icon, name, …) the info panel renders.</summary>
        public abstract EntityData Definition { get; }

        public int CurrentHp => _currentHp;
        public bool IsDead => _isDead;
        public bool IsSelected => _isSelected;

        /// <summary>True while the spawn-pop animation owns the transform scale.</summary>
        protected bool IsSpawnPopping => _spawnPopRoutine != null;
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

            int applied = Mathf.Min(amount, _currentHp);
            _currentHp = Mathf.Max(0, _currentHp - amount);
            GameEvents.RaiseHealthChanged(this);
            GameEvents.RaiseDamageTaken(this, applied);
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

        /// <summary>
        /// Plays a short "pop" on appearance — scales from zero up to the entity's current scale with
        /// an overshoot, so spawns/placements feel alive instead of snapping in. Transform-only, so it
        /// adds no draw calls. Call <b>after</b> the final scale is set; concrete types pass their own
        /// duration. A hand-written Coroutine (no tween library), per the brief.
        /// </summary>
        protected void PlaySpawnPop(float duration)
        {
            StopSpawnPop();
            if (duration > 0f && isActiveAndEnabled)
            {
                _spawnPopRoutine = StartCoroutine(SpawnPopRoutine(duration));
            }
        }

        /// <summary>
        /// A flat dust ring that expands at the entity's base on appearance, so spawns/placements feel
        /// like they land on the ground. Independent of the entity (an <see cref="OneShotVfx"/>), so it
        /// is unaffected by the spawn-pop scaling. No-op until a sprite is assigned.
        /// </summary>
        protected void PlaySpawnDust()
        {
            if (_spawnDustSprite == null)
            {
                return;
            }

            var renderer = GetComponent<SpriteRenderer>();
            Bounds bounds = renderer != null ? renderer.bounds : new Bounds(transform.position, Vector3.one);
            Vector3 ground = new Vector3(bounds.center.x, bounds.min.y, 0f);
            float size = bounds.size.x * _spawnDustScale;
            OneShotVfx.Play(_spawnDustSprite, ground, _spawnDustColor, size, 0.35f, 2, _spawnDustFlatten);
        }

        /// <summary>Cancels an in-flight spawn pop (e.g. the entity dies mid-pop).</summary>
        protected void StopSpawnPop()
        {
            if (_spawnPopRoutine != null)
            {
                StopCoroutine(_spawnPopRoutine);
                _spawnPopRoutine = null;
            }
        }

        private IEnumerator SpawnPopRoutine(float duration)
        {
            Vector3 finalScale = transform.localScale;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.localScale = finalScale * EaseOutBack(t);
                yield return null;
            }

            transform.localScale = finalScale;
            _spawnPopRoutine = null;
        }

        // Ease-out with a slight overshoot past 1 before settling — the classic "pop" feel.
        private static float EaseOutBack(float t)
        {
            const float overshoot = 1.70158f;
            float p = t - 1f;
            return 1f + (overshoot + 1f) * p * p * p + overshoot * p * p;
        }

        private void Die()
        {
            _isDead = true;
            GameEvents.RaiseEntityDied(this);
            OnDied();
            PlayDeathPoof();
            StartCoroutine(DieRoutine());
        }

        // An independent burst at the entity's centre, sized to its sprite so a barracks pops bigger
        // than a soldier. Outlives this object (which is about to be pooled/destroyed).
        private void PlayDeathPoof()
        {
            if (_deathPoofSprite == null)
            {
                return;
            }

            var renderer = GetComponent<SpriteRenderer>();
            Vector3 position = renderer != null ? renderer.bounds.center : transform.position;
            float size = renderer != null ? renderer.bounds.size.x * _deathPoofScale : _deathPoofScale;
            OneShotVfx.Play(_deathPoofSprite, position, _deathPoofColor, size, 0.3f, 25);
        }

        private IEnumerator DieRoutine()
        {
            yield return DeathAnimationRoutine();
            RemoveFromBoard();
        }
    }
}
