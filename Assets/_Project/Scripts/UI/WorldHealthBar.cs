using StrategyDemo.Core;
using UnityEngine;

namespace StrategyDemo.UI
{
    /// <summary>
    /// Small world-space HP bar for an entity. It builds two child SpriteRenderers at runtime so
    /// buildings and units can share the same prefab component without extra scene plumbing.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WorldHealthBar : MonoBehaviour
    {
        [SerializeField] private Sprite _frameSprite;
        [SerializeField] private Sprite _fillSprite;
        [SerializeField] private Vector2 _worldSize = new Vector2(0.8f, 0.14f);
        [SerializeField] private Vector2 _worldOffset = new Vector2(0f, 0.55f);
        [SerializeField] private int _sortingOrder = 20;
        [SerializeField] private Color _healthyColor = new Color(0.15f, 0.95f, 0.25f, 1f);
        [SerializeField] private Color _criticalColor = new Color(1f, 0.15f, 0.08f, 1f);

        private GameElement _owner;
        private Transform _root;
        private SpriteRenderer _frame;
        private SpriteRenderer _fill;
        private bool _isSelected;

        private void Awake()
        {
            _owner = GetComponent<GameElement>();
            BuildRenderers();
        }

        private void OnEnable()
        {
            GameEvents.HealthChanged += OnHealthChanged;
            GameEvents.SelectionChanged += OnSelectionChanged;
        }

        private void Start()
        {
            UpdateBar();
        }

        private void OnDisable()
        {
            GameEvents.HealthChanged -= OnHealthChanged;
            GameEvents.SelectionChanged -= OnSelectionChanged;
        }

        private void OnHealthChanged(IDamageable entity)
        {
            if (ReferenceEquals(entity, _owner))
            {
                UpdateBar();
            }
        }

        private void OnSelectionChanged(ISelectable selectable)
        {
            _isSelected = ReferenceEquals(selectable, _owner);
            UpdateBar();
        }

        private void BuildRenderers()
        {
            _root = new GameObject("HealthBar").transform;
            _root.SetParent(transform, false);

            _frame = CreateRenderer("Frame", _root, _frameSprite, _sortingOrder);
            _fill = CreateRenderer("Fill", _root, _fillSprite, _sortingOrder + 1);
        }

        private static SpriteRenderer CreateRenderer(
            string name, Transform parent, Sprite sprite, int sortingOrder)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private void UpdateBar()
        {
            if (_owner == null || _owner.MaxHp <= 0)
            {
                SetVisible(false);
                return;
            }

            float ratio = Mathf.Clamp01((float)_owner.CurrentHp / _owner.MaxHp);
            bool shouldShow = !_owner.IsDead && (_isSelected || ratio < 0.999f);
            SetVisible(shouldShow);

            if (!shouldShow)
            {
                return;
            }

            Vector3 parentScale = transform.lossyScale;
            float invX = 1f / Mathf.Max(0.0001f, Mathf.Abs(parentScale.x));
            float invY = 1f / Mathf.Max(0.0001f, Mathf.Abs(parentScale.y));

            _root.localPosition = new Vector3(_worldOffset.x * invX, _worldOffset.y * invY, 0f);
            SetWorldSize(_frame, _worldSize);
            SetWorldSize(_fill, new Vector2(_worldSize.x * ratio, _worldSize.y * 0.65f));
            _fill.color = Color.Lerp(_criticalColor, _healthyColor, ratio);
            _fill.transform.localPosition =
                new Vector3(-_worldSize.x * (1f - ratio) * 0.5f * invX, 0f, 0f);
        }

        private void SetWorldSize(SpriteRenderer renderer, Vector2 worldSize)
        {
            if (renderer == null || renderer.sprite == null)
            {
                return;
            }

            Vector3 parentScale = transform.lossyScale;
            Vector2 native = renderer.sprite.bounds.size;
            renderer.transform.localScale = new Vector3(
                worldSize.x / Mathf.Max(0.0001f, native.x) / Mathf.Max(0.0001f, Mathf.Abs(parentScale.x)),
                worldSize.y / Mathf.Max(0.0001f, native.y) / Mathf.Max(0.0001f, Mathf.Abs(parentScale.y)),
                1f);
        }

        private void SetVisible(bool isVisible)
        {
            if (_root != null && _root.gameObject.activeSelf != isVisible)
            {
                _root.gameObject.SetActive(isVisible);
            }
        }
    }
}
