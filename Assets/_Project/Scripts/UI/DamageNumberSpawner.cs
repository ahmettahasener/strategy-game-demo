using System.Collections;
using System.Collections.Generic;
using StrategyDemo.Core;
using UnityEngine;

namespace StrategyDemo.UI
{
    /// <summary>
    /// Pops a floating "-N" number above an entity whenever it takes damage, then floats it up and
    /// fades it out. Listens on <see cref="GameEvents.DamageTaken"/> so it stays decoupled from combat.
    /// Uses world-space <see cref="TextMesh"/> (no TMP dependency) and pools the labels, so repeated
    /// hits allocate nothing and share a single font material.
    /// </summary>
    public sealed class DamageNumberSpawner : MonoBehaviour
    {
        [SerializeField] private Font _font; // falls back to the built-in font when left empty
        [SerializeField] private Color _color = new Color(1f, 0.25f, 0.2f, 1f);
        [SerializeField] private int _fontSize = 64;
        [SerializeField] private float _characterSize = 0.06f;
        [SerializeField] private float _riseDistance = 0.7f;
        [SerializeField] private float _duration = 0.6f;
        [SerializeField] private float _spawnYOffset = 0.15f;
        [SerializeField] private float _horizontalJitter = 0.15f;
        [SerializeField] private int _sortingOrder = 100;

        private readonly List<TextMesh> _pool = new List<TextMesh>();

        private void Awake()
        {
            if (_font == null)
            {
                // Built-in font name in Unity 2021 LTS (it became "LegacyRuntime.ttf" only in 2022.2+).
                _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
        }

        private void OnEnable()
        {
            GameEvents.DamageTaken += OnDamageTaken;
        }

        private void OnDisable()
        {
            GameEvents.DamageTaken -= OnDamageTaken;

            // Disabling stops the float coroutines mid-flight, so their labels never reach the
            // SetActive(false) that returns them to the pool. Retire them here, or they stay
            // visible forever and GetLabel can never recycle them.
            for (int i = 0; i < _pool.Count; i++)
            {
                if (_pool[i] != null)
                {
                    _pool[i].gameObject.SetActive(false);
                }
            }
        }

        private void OnDamageTaken(IDamageable entity, int amount)
        {
            if (amount <= 0 || !(entity is Component component))
            {
                return;
            }

            var renderer = component.GetComponent<SpriteRenderer>();
            Bounds bounds = renderer != null
                ? renderer.bounds
                : new Bounds(component.transform.position, Vector3.one);
            float jitter = Random.Range(-_horizontalJitter, _horizontalJitter);
            var origin = new Vector3(bounds.center.x + jitter, bounds.max.y + _spawnYOffset, 0f);

            TextMesh label = GetLabel();
            label.text = amount.ToString();
            label.transform.position = origin;
            label.gameObject.SetActive(true);
            StartCoroutine(FloatRoutine(label, origin));
        }

        // Rises with an ease-out and fades over the second half, then returns to the pool.
        private IEnumerator FloatRoutine(TextMesh label, Vector3 origin)
        {
            float elapsed = 0f;
            while (elapsed < _duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _duration);
                float eased = 1f - (1f - t) * (1f - t);

                Vector3 position = origin;
                position.y += _riseDistance * eased;
                label.transform.position = position;

                Color color = _color;
                color.a = _color.a * (1f - Mathf.InverseLerp(0.5f, 1f, t)); // hold, then fade
                label.color = color;
                yield return null;
            }

            label.gameObject.SetActive(false);
        }

        // Reuses an idle label or builds a new one. All labels share the font's material, so they
        // batch into a single draw call regardless of how many are on screen.
        private TextMesh GetLabel()
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                if (!_pool[i].gameObject.activeSelf)
                {
                    return _pool[i];
                }
            }

            var labelObject = new GameObject("DamageNumber");
            labelObject.transform.SetParent(transform, false);
            var label = labelObject.AddComponent<TextMesh>();
            if (_font != null)
            {
                label.font = _font;
            }

            label.fontSize = _fontSize;
            label.characterSize = _characterSize;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = _color;

            var meshRenderer = labelObject.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = _font != null ? _font.material : meshRenderer.sharedMaterial;
            meshRenderer.sortingOrder = _sortingOrder;

            labelObject.SetActive(false);
            _pool.Add(label);
            return label;
        }
    }
}
