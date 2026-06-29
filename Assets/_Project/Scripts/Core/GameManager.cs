using UnityEngine;

namespace StrategyDemo.Core
{
    /// <summary>
    /// Top-level coordinator and bootstrap for the session. Owns game-wide state and wires the other
    /// manager services together.
    /// </summary>
    public sealed class GameManager : Singleton<GameManager>
    {
        private void Update()
        {
            // Esc quits the built application so the reviewer can close the windowed/fullscreen build
            // cleanly (Application.Quit is a no-op in the editor, so this does nothing during Play mode).
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }
}
