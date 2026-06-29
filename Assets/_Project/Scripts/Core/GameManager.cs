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
            // Esc quits: in a build it closes the app cleanly; in the editor Application.Quit is a no-op,
            // so we stop Play mode instead, giving the same "the game exited" result while testing.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Quit();
            }
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
