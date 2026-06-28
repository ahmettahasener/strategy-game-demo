using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StrategyDemo.Core
{
    /// <summary>
    /// A fire-and-forget visual burst: an independent sprite that expands and fades (e.g. a death
    /// "poof" or a placement dust ring). It is unparented from the entity that triggers it, so it
    /// outlives that entity being pooled/destroyed. Instances are **recycled through a small internal
    /// pool** rather than Instantiated/Destroyed per play, so frequent spawns/deaths add no GC churn —
    /// the same pooling discipline used for units, menu cells, damage numbers and path dots. Hand-written
    /// Coroutine, no tween library.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class OneShotVfx : MonoBehaviour
    {
        private static readonly Stack<OneShotVfx> Pool = new Stack<OneShotVfx>();
        private static Transform _fallbackContainer;

        private SpriteRenderer _renderer;
        private Color _color;
        private float _size;
        private float _duration;
        private float _flatten;

        /// <summary>
        /// Plays a one-shot burst at <paramref name="position"/> using an atlased sprite, reusing a
        /// pooled instance when one is free. <paramref name="flatten"/> squashes the vertical axis
        /// (1 = round, &lt;1 = a ground-hugging ellipse) for effects that lie on the floor, like dust.
        /// </summary>
        public static void Play(
            Sprite sprite, Vector3 position, Color color, float size, float duration, int sortingOrder,
            float flatten = 1f)
        {
            if (sprite == null || size <= 0f || duration <= 0f)
            {
                return;
            }

            OneShotVfx vfx = Rent();
            vfx.transform.position = position;
            vfx._renderer.sprite = sprite;
            vfx._renderer.color = color;
            vfx._renderer.sortingOrder = sortingOrder;
            vfx._color = color;
            vfx._size = size;
            vfx._duration = duration;
            vfx._flatten = flatten;
            vfx.gameObject.SetActive(true);
            vfx.StartCoroutine(vfx.Run());
        }

        // Reuse a free instance (skipping any destroyed by a domain reload), else build a new one.
        private static OneShotVfx Rent()
        {
            while (Pool.Count > 0)
            {
                OneShotVfx pooled = Pool.Pop();
                if (pooled != null)
                {
                    return pooled;
                }
            }

            var go = new GameObject("Vfx");
            go.transform.SetParent(Container(), false);
            var vfx = go.AddComponent<OneShotVfx>();
            vfx._renderer = go.AddComponent<SpriteRenderer>();
            return vfx;
        }

        // The authored VfxRoot when present, else a lazily-created root (so VFX still work without it).
        private static Transform Container()
        {
            if (VfxRoot.Current != null)
            {
                return VfxRoot.Current;
            }

            if (_fallbackContainer == null)
            {
                _fallbackContainer = new GameObject("OneShotVfxPool").transform;
            }

            return _fallbackContainer;
        }

        private IEnumerator Run()
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

            gameObject.SetActive(false);
            Pool.Push(this);
        }
    }
}
