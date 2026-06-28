using System;
using StrategyDemo.Data;
using UnityEngine;
using UnityEngine.UI;

namespace StrategyDemo.UI
{
    /// <summary>
    /// One producible-unit card in the info panel: shows the unit icon and reports clicks through a
    /// callback. Pure View — it knows nothing about how production happens.
    /// </summary>
    public sealed class UnitCardView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Button _button;

        private UnitData _data;
        private Action<UnitData> _onClick;

        private void Awake()
        {
            // Bound once: HandleClick always reads the current _data/_onClick, so re-binding the card
            // never re-allocates a delegate (mirrors BuildingCardView).
            if (_button != null)
            {
                _button.onClick.AddListener(HandleClick);
            }
        }

        public void Bind(UnitData data, Action<UnitData> onClick)
        {
            _data = data;
            _onClick = onClick;
            if (_icon != null)
            {
                _icon.sprite = data.Icon;
            }
        }

        private void HandleClick()
        {
            _onClick?.Invoke(_data);
        }
    }
}
