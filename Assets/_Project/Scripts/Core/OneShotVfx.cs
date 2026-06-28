using System.Collections;
using UnityEngine;

namespace StrategyDemo.Core
{
    /// <summary>
    /// A fire-and-forget visual burst: spawns an independent sprite that expands and fades, then
    /// destroys itself. Independent (not parented) so it outlives the entity that triggered it — e.g.
    /// a death "poof" while the dying entity is being pooled/destroyed. Hand-written Coroutine.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class OneShotVfx : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        private Color _color;
        private float _size;
        private float _duration;
        private float _flatten;

        /// <summary>
        /// Spawns a one-shot burst at <paramref name="position"/> using an atlased sprite.
        /// <paramref name="flatten"/> squashes the vertical axis (1 = round, &lt;1 = a ground-hugging
        /// ellipse) for effects that read as lying on the floor, like a placement dust ring.
        /// </summary>
        public static void Play(
            Sprite sprite, Vector3 position, Color color, float size, float duration, int sortingOrder,
            float flatten = 1f)
        {
            if (sprite == null || size <= 0f || duration <= 0f)
            {
                return;
            }

            var go = new GameObject("Vfx") { transform = { position = position } };
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;

            var vfx = go.AddComponent<OneShotVfx>();
            vfx._renderer = renderer;
            vfx._color = color;
            vfx._size = size;
            vfx._duration = duration;
            vfx._flatten = flatten;
        }

        private IEnumerator Start()
        {
            float native = Mathf.Max(0.0001f, _renderer.sprite.bounds.size.x);
            float elapsed = 0f;
            while (elapsed < _duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _duration);
                float eased = 1f - (1f - t) * (1f - t);
                float worldSize = _size * Mathf.Lerp(0.4f, 1.3f, eased);
                float scale = worldSize / native;
                transform.localScale = new Vector3(scale, scale * _flatten, 1f);

                Color c = _color;
                c.a = _color.a * (1f - t);
                _renderer.color = c;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
