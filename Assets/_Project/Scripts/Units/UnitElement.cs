using System.Collections;
using StrategyDemo.Core;
using StrategyDemo.Data;
using UnityEngine;

namespace StrategyDemo.Units
{
    /// <summary>
    /// A produced soldier on the board. Supplies its stats from <see cref="UnitData"/>. No dynamic
    /// Rigidbody2D — movement is transform-based, so overlapping spawns can't cause a physics
    /// explosion.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class UnitElement : GameElement
    {
        private const float BoardSize = 0.8f; // on-board footprint of a unit, in cells

        [SerializeField] private Color _highlightTint = Color.yellow;
        [SerializeField] private Color _enemyTint = new Color(1f, 0.4f, 0.4f);
        [SerializeField] private Color _damageFlashTint = new Color(1f, 0.2f, 0.15f);
        [SerializeField] private float _damageFlashDuration = 0.08f;
        [SerializeField] private float _deathAnimationDuration = 0.2f;
        [SerializeField] private float _spawnPopDuration = 0.16f;

        // Flip the sprite to face its travel direction. Off for art that is drawn symmetric/front-on.
        [SerializeField] private bool _faceMovementDirection = true;

        // Optional selection-ring child. When assigned, selection is shown by toggling this ring instead
        // of tinting the sprite, so the unit art keeps its true colour. Falls back to the tint when null.
        // Its size/position are authored on the prefab (all unit sprites share size, so one setting fits all).
        [SerializeField] private GameObject _selectionRing;

        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _collider;
        private Color _originalColor;
        private Color _baseColor;
        private UnitData _data;
        private Coroutine _damageFlashRoutine;
        private Vector3 _lastBoardScale = Vector3.one;

        public UnitData Data => _data;

        public override EntityData Definition => _data;

        public override int MaxHp => _data != null ? _data.MaxHp : 0;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<BoxCollider2D>();
            _originalColor = _spriteRenderer.color;
            _baseColor = _originalColor;
        }

        private void OnDisable()
        {
            StopDamageFlash();
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _baseColor;
            }
        }

        /// <summary>
        /// Configures the unit after a factory spawn or a pool reuse. Resets every per-instance bit of
        /// state — faction, colour, selection highlight and health — so a recycled instance starts clean.
        /// </summary>
        public void Initialize(UnitData data, Faction faction)
        {
            _data = data;
            if (data.BoardSprite != null)
            {
                _spriteRenderer.sprite = data.BoardSprite;
            }

            ApplyBoardScale();
            FitColliderToSprite();
            SetFaction(faction);
            _baseColor = faction == Faction.Enemy ? _enemyTint : _originalColor;
            _spriteRenderer.color = _baseColor; // apply faction colour directly: with a ring, SetHighlight no longer sets it
            _spriteRenderer.flipX = false; // reset facing for a recycled instance
            OnDeselected(); // clears stale selection state (ring off / base colour)
            ResetHealth();
            PlaySpawnPop(_spawnPopDuration); // after ApplyBoardScale set the final scale
        }

        /// <summary>
        /// Flips the sprite to face horizontal travel. Called by movement each leg; ignored for tiny
        /// or purely vertical deltas so the unit doesn't flicker. No-op when facing is disabled.
        /// </summary>
        public void FaceMovement(float deltaX)
        {
            if (!_faceMovementDirection || Mathf.Abs(deltaX) < 0.01f)
            {
                return;
            }

            _spriteRenderer.flipX = deltaX < 0f;
        }

        protected override void SetHighlight(bool isOn)
        {
            if (_selectionRing != null)
            {
                _selectionRing.SetActive(isOn);
                return;
            }

            _spriteRenderer.color = isOn ? _highlightTint : _baseColor;
        }

        protected override void OnDamaged()
        {
            if (_damageFlashRoutine != null)
            {
                StopCoroutine(_damageFlashRoutine);
            }

            _damageFlashRoutine = StartCoroutine(DamageFlashRoutine());
        }

        // Scale to a consistent on-board size (uniform, aspect-preserving), independent of the art's
        // resolution / pixels-per-unit (sprite.bounds.size is the true unscaled world size).
        private void ApplyBoardScale()
        {
            if (_spriteRenderer.sprite == null)
            {
                return;
            }

            Vector2 native = _spriteRenderer.sprite.bounds.size;
            float scale = BoardSize / Mathf.Max(0.0001f, Mathf.Max(native.x, native.y));
            transform.localScale = new Vector3(scale, scale, 1f);
            _lastBoardScale = transform.localScale;
        }

        // Match the click/selection collider to the sprite so the whole unit is clickable.
        private void FitColliderToSprite()
        {
            if (_collider == null || _spriteRenderer.sprite == null)
            {
                return;
            }

            Bounds bounds = _spriteRenderer.sprite.bounds;
            _collider.size = bounds.size;
            _collider.offset = bounds.center;
        }

        protected override void RemoveFromBoard()
        {
            PoolManager.Instance.Release(gameObject);
        }

        protected override IEnumerator DeathAnimationRoutine()
        {
            yield return ScaleFadeOutRoutine();
        }

        private IEnumerator DamageFlashRoutine()
        {
            _spriteRenderer.color = _damageFlashTint;
            yield return new WaitForSeconds(_damageFlashDuration);
            _damageFlashRoutine = null;
            RestoreSpriteColor();
        }

        private void StopDamageFlash()
        {
            if (_damageFlashRoutine == null)
            {
                return;
            }

            StopCoroutine(_damageFlashRoutine);
            _damageFlashRoutine = null;
        }

        private void RestoreSpriteColor()
        {
            if (_selectionRing != null || !IsSelected)
            {
                _spriteRenderer.color = _baseColor;
                return;
            }

            _spriteRenderer.color = _highlightTint;
        }

        private IEnumerator ScaleFadeOutRoutine()
        {
            StopDamageFlash();
            StopSpawnPop();

            Vector3 startScale = transform.localScale;
            Color startColor = _spriteRenderer.color;
            float elapsed = 0f;

            if (_selectionRing != null)
            {
                _selectionRing.SetActive(false);
            }

            _spriteRenderer.color = _damageFlashTint;

            while (elapsed < _deathAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float ratio = Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, _deathAnimationDuration));
                float eased = 1f - Mathf.Pow(1f - ratio, 2f);

                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, eased);
                Color current = Color.Lerp(_damageFlashTint, startColor, ratio);
                current.a = 1f - ratio;
                _spriteRenderer.color = current;
                yield return null;
            }

            transform.localScale = _lastBoardScale;
            _spriteRenderer.color = _baseColor;
        }
    }
}
