using UnityEngine;

namespace StrategyDemo.Core
{
    /// <summary>
    /// Swaps the OS pointer for the game's themed cursor. Uses a hardware cursor
    /// (<see cref="CursorMode.Auto"/>), so the art is drawn by the OS at zero render cost — no extra
    /// draw call and no input lag, unlike a UI-image cursor. The hotspot is the click point inside the
    /// texture (pixels from the top-left); set it to the tip of the pointer art so clicks land where the
    /// player expects.
    /// </summary>
    public sealed class CursorController : MonoBehaviour
    {
        [SerializeField] private Texture2D _cursor;
        [SerializeField] private Vector2 _hotspot = Vector2.zero;

        private void OnEnable()
        {
            if (_cursor != null)
            {
                Cursor.SetCursor(_cursor, _hotspot, CursorMode.Auto);
            }
        }

        private void OnDisable()
        {
            // Restore the default OS cursor so a custom pointer doesn't linger if this object is disabled.
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}
