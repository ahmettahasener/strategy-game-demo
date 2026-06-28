using System;
using System.Collections.Generic;
using StrategyDemo.Data;
using UnityEngine;
using UnityEngine.UI;

namespace StrategyDemo.UI
{
    /// <summary>
    /// Virtualized vertical list (Brief #1): instead of one cell per item, it keeps a fixed pool of
    /// cells sized to the viewport and recycles them as the user scrolls — a cell that leaves one edge
    /// is repositioned and re-bound to the item that scrolled into view. Re-binding happens only when
    /// the first visible index changes (a boundary crossing), not every frame, so scrolling is alloc-free.
    /// </summary>
    public sealed class RecyclingScrollView : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _content;
        [SerializeField] private BuildingCardView _cellPrefab;
        [SerializeField] private float _cellHeight = 100f;
        [SerializeField, Min(0)] private int _maxPooledCells = 6;

        private readonly List<BuildingCardView> _cells = new List<BuildingCardView>();
        private IReadOnlyList<BuildingData> _items;
        private Action<BuildingData> _onSelect;
        private int _firstIndex = -1;

        public void Initialize(IReadOnlyList<BuildingData> items, Action<BuildingData> onSelect)
        {
            _items = items ?? Array.Empty<BuildingData>();
            _onSelect = onSelect;

            // The content represents ALL items, so the scrollbar reflects the full list.
            _content.sizeDelta = new Vector2(_content.sizeDelta.x, _items.Count * _cellHeight);

            if (_items.Count == 0)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            float viewportHeight = _scrollRect.viewport.rect.height;
            // Cells needed to fill the viewport plus a one-cell buffer at each edge for the recycle.
            int required = Mathf.Max(4, Mathf.CeilToInt(viewportHeight / _cellHeight) + 2);
            // _maxPooledCells caps the pool, but it can never drop below the count that covers the
            // viewport — capping lower would leave the bottom rows permanently blank when scrolled.
            int visibleCount = _maxPooledCells > 0 ? Mathf.Max(required, _maxPooledCells) : required;

            EnsureCells(Mathf.Min(visibleCount, _items.Count));

            _scrollRect.onValueChanged.RemoveListener(OnScrolled);
            _scrollRect.onValueChanged.AddListener(OnScrolled);

            _firstIndex = -1;
            Refresh();
        }

        private void EnsureCells(int count)
        {
            while (_cells.Count < count)
            {
                _cells.Add(Instantiate(_cellPrefab, _content));
            }
        }

        private void OnScrolled(Vector2 _)
        {
            Refresh();
        }

        private void Refresh()
        {
            int maxFirst = Mathf.Max(0, _items.Count - _cells.Count);
            int firstIndex = Mathf.Clamp(Mathf.FloorToInt(_content.anchoredPosition.y / _cellHeight), 0, maxFirst);
            if (firstIndex == _firstIndex)
            {
                return; // no boundary crossed — nothing to reposition or rebind
            }

            _firstIndex = firstIndex;
            for (int i = 0; i < _cells.Count; i++)
            {
                int itemIndex = firstIndex + i;
                BuildingCardView cell = _cells[i];
                bool inRange = itemIndex < _items.Count;
                cell.gameObject.SetActive(inRange);
                if (inRange)
                {
                    cell.RectTransform.anchoredPosition = new Vector2(0f, -itemIndex * _cellHeight);
                    cell.Bind(_items[itemIndex], _onSelect);
                }
            }
        }
    }
}
