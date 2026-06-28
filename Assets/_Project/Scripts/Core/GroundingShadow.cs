using UnityEngine;

namespace StrategyDemo.Core
{
    /// <summary>
    /// Drops a soft elliptical shadow under an entity so it reads as planted on the board instead of a
    /// sticker floating on the grass. Builds a child <see cref="SpriteRenderer"/> at runtime, sorted
    /// just below the owner, sized to the owner's current width — so it tracks the spawn-pop / death
    /// scaling and matches a building's footprint or a unit's small size automatically. The sprite
    /// comes from the gameplay atlas, so it adds no draw call.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class GroundingShadow : MonoBehaviour
    {
        [SerializeField] private Sprite _shadowSprite;
        [SerializeField, Range(0.1f, 2f)] private float _widthFactor = 0.82f; // shadow width vs owner width
        [SerializeField, Range(0.05f, 1f)] private float _flatten = 0.4f;      // ellipse height vs width
        [SerializeField] private float _feetOffset = 0f;                       // extra world Y at the base
        [SerializeField, Range(0f, 1f)] private float _opacity = 0.33f;
        [SerializeField] private int _sortingOffset = -1;

        private SpriteRenderer _owner;
        private SpriteRenderer _shadow;

        private void Awake()
        {
            _owner = GetComponent<SpriteRenderer>();
            if (_shadowSprite != null)
            {
                Build();
            }
        }

        // Track the owner's size/position so the shadow stays correct as it scales (spawn pop, death).
        private void LateUpdate()
        {
            if (_shadow != null)
            {
                Apply();
            }
        }

        private void Build()
        {
            var child = new GameObject("Shadow");
            _shadow = child.AddComponent<SpriteRenderer>();
            _shadow.transform.SetParent(transform, false);
            _shadow.sprite = _shadowSprite;
            _shadow.color = new Color(0f, 0f, 0f, _opacity);
            _shadow.sortingLayerID = _owner.sortingLayerID;
            _shadow.sortingOrder = _owner.sortingOrder + _sortingOffset;
            Apply();
        }

        private void Apply()
        {
            if (_owner.sprite == null)
            {
                _shadow.enabled = false;
                return;
            }

            _shadow.enabled = true;
            Bounds bounds = _owner.bounds; // world-space, already includes scale and flip
            float width = bounds.size.x * _widthFactor;
            float height = width * _flatten;

            Vector3 lossy = transform.lossyScale;
            Vector2 native = _shadowSprite.bounds.size;
            _shadow.transform.localScale = new Vector3(
                width / (native.x * Mathf.Max(0.0001f, Mathf.Abs(lossy.x))),
                height / (native.y * Mathf.Max(0.0001f, Mathf.Abs(lossy.y))),
                1f);

            // Centre the ellipse on the owner's base (a touch above the very bottom reads best).
            _shadow.transform.position = new Vector3(
                bounds.center.x,
                bounds.min.y + height * 0.5f + _feetOffset,
                transform.position.z);
        }
    }
}
