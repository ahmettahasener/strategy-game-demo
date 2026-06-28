using System.Collections;
using System.Collections.Generic;
using StrategyDemo.Core;
using StrategyDemo.Data;
using UnityEngine;

namespace StrategyDemo.Buildings
{
    /// <summary>
    /// A placed building on the board. Supplies its stats from <see cref="BuildingData"/>, remembers
    /// the grid footprint it occupies, and releases those cells when destroyed. Implements
    /// <see cref="IProducer"/> data-driven — only buildings whose data is a producer can make units.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class BuildingElement : GameElement, IProducer
    {
        [SerializeField] private Color _highlightTint = Color.cyan;
        [SerializeField] private Color _enemyTint = new Color(1f, 0.4f, 0.4f);
        [SerializeField] private Color _damageFlashTint = new Color(1f, 0.2f, 0.15f);
        [SerializeField] private float _damageFlashDuration = 0.08f;
        [SerializeField] private float _deathAnimationDuration = 0.24f;
        [SerializeField] private float _spawnPopDuration = 0.18f;

        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _collider;
        private Color _baseColor;
        private BuildingData _data;
        private Vector2Int _footprintOrigin;
        private Coroutine _damageFlashRoutine;

        public BuildingData Data => _data;

        public override EntityData Definition => _data;

        public override int MaxHp => _data != null ? _data.MaxHp : 0;

        public bool CanProduce => _data != null && _data.IsProducer;

        public IReadOnlyList<UnitData> ProducibleUnits =>
            _data != null ? _data.ProducibleUnits : System.Array.Empty<UnitData>();

        public Vector2Int SpawnCell =>
            _footprintOrigin + (_data != null ? _data.SpawnPointOffset : Vector2Int.zero);

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<BoxCollider2D>();
            _baseColor = _spriteRenderer.color;
        }

        /// <summary>Configures the building after instantiation (called by the placement flow).</summary>
        public void Initialize(BuildingData data, Vector2Int footprintOrigin, Faction faction)
        {
            _data = data;
            _footprintOrigin = footprintOrigin;
            if (data.BoardSprite != null)
            {
                _spriteRenderer.sprite = data.BoardSprite;
            }

            ApplyFootprintScale(data.Size);
            FitColliderToSprite();
            SetFaction(faction);
            if (faction == Faction.Enemy)
            {
                _baseColor = _enemyTint;
                _spriteRenderer.color = _baseColor;
            }

            ResetHealth();
            PlaySpawnPop(_spawnPopDuration); // after ApplyFootprintScale set the final scale
        }

        protected override void SetHighlight(bool isOn)
        {
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

        // Scale so the sprite spans its grid footprint exactly, independent of the art's
        // resolution / pixels-per-unit (sprite.bounds.size is the true unscaled world size).
        private void ApplyFootprintScale(Vector2Int size)
        {
            Vector2 native = _spriteRenderer.sprite != null
                ? (Vector2)_spriteRenderer.sprite.bounds.size
                : Vector2.one;
            transform.localScale = new Vector3(
                size.x / Mathf.Max(0.0001f, native.x),
                size.y / Mathf.Max(0.0001f, native.y),
                1f);
        }

        // Match the click/selection collider to the sprite so the whole building is clickable
        // (the placeholder collider was sized for the 1x1 square and is wrong once scaled).
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

        protected override void OnDied()
        {
            StopDamageFlash();
            StopSpawnPop();
            if (GridManager.Instance != null && _data != null)
            {
                GridManager.Instance.Free(_footprintOrigin, _data.Size);
            }
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
            _spriteRenderer.color = IsSelected ? _highlightTint : _baseColor;
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

        private IEnumerator ScaleFadeOutRoutine()
        {
            Vector3 startScale = transform.localScale;
            Color startColor = _spriteRenderer.color;
            float elapsed = 0f;

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
        }
    }
}
