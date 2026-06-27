using System;
using StrategyDemo.Data;
using UnityEngine;
using UnityEngine.UI;

namespace StrategyDemo.UI
{
    /// <summary>
    /// One recycled cell in the build menu: shows a building's icon and name, and reports clicks.
    /// Pure View — it is re-bound to different data as it is recycled and knows no game rules.
    /// </summary>
    public sealed class BuildingCardView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Text _label;
        [SerializeField] private Button _button;

        private BuildingData _data;
        private Action<BuildingData> _onClick;

        public RectTransform RectTransform => (RectTransform)transform;

        private void Awake()
        {
            // Bound once: HandleClick always reads the current _data/_onClick, so re-binding the cell
            // during a scroll never re-allocates a delegate (zero-alloc recycling).
            if (_button != null)
            {
                _button.onClick.AddListener(HandleClick);
            }
        }

        public void Bind(BuildingData data, Action<BuildingData> onClick)
        {
            _data = data;
            _onClick = onClick;

            if (_icon != null)
            {
                _icon.sprite = data.Icon;
            }

            if (_label != null)
            {
                _label.text = data.DisplayName;
            }
        }

        private void HandleClick()
        {
            _onClick?.Invoke(_data);
        }
    }
}
