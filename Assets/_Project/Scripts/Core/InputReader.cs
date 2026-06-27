using System;
using UnityEngine;

namespace StrategyDemo.Core
{
    /// <summary>
    /// Single point of input for the game. Wraps Unity's legacy Input so controllers depend on
    /// this instead of raw input — keeping input handling in one place and swappable later
    /// (e.g. to the Input System) without touching the controllers.
    /// </summary>
    public sealed class InputReader : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        /// <summary>Left mouse button pressed this frame (place / select).</summary>
        public event Action PrimaryPressed;

        /// <summary>Right mouse button pressed this frame (cancel / move-attack).</summary>
        public event Action SecondaryPressed;

        /// <summary>Pointer position projected into world space (2D plane).</summary>
        public Vector2 PointerWorldPosition { get; private set; }

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        private void Update()
        {
            Vector3 screenPosition = Input.mousePosition;
            if (_camera != null && IsFinite(screenPosition))
            {
                PointerWorldPosition = _camera.ScreenToWorldPoint(screenPosition);
            }

            if (Input.GetMouseButtonDown(0))
            {
                PrimaryPressed?.Invoke();
            }

            if (Input.GetMouseButtonDown(1))
            {
                SecondaryPressed?.Invoke();
            }
        }

        private static bool IsFinite(Vector3 value)
        {
            return !float.IsInfinity(value.x) && !float.IsInfinity(value.y)
                && !float.IsNaN(value.x) && !float.IsNaN(value.y);
        }
    }
}
