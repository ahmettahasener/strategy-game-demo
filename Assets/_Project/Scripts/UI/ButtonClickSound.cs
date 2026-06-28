using StrategyDemo.Core;
using UnityEngine;
using UnityEngine.UI;

namespace StrategyDemo.UI
{
    /// <summary>
    /// Plays the UI click sound for the <see cref="Button"/> it sits on by raising
    /// <see cref="GameEvents.UiClicked"/>. Hooking the button's own <c>onClick</c> means it respects
    /// interactability (a disabled button stays silent) and keeps audio decoupled — the button knows
    /// nothing about the <c>AudioManager</c>. One reusable component for every button.
    /// </summary>
    [RequireComponent(typeof(Button))]
    [DisallowMultipleComponent]
    public sealed class ButtonClickSound : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(GameEvents.RaiseUiClicked);
        }
    }
}
