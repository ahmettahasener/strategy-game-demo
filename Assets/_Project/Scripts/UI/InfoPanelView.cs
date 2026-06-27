using System;
using System.Collections.Generic;
using StrategyDemo.Core;
using StrategyDemo.Data;
using UnityEngine;
using UnityEngine.UI;

namespace StrategyDemo.UI
{
    /// <summary>
    /// Renders the currently selected entity (icon, name, HP) and, for a producer, its producible
    /// unit cards (Brief #5). Render-only: it raises <see cref="ProduceRequested"/> when a card is
    /// clicked but runs no game rules — a controller turns that intent into production.
    /// </summary>
    public sealed class InfoPanelView : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _hpText;
        [SerializeField] private Transform _cardContainer;
        [SerializeField] private UnitCardView _unitCardPrefab;

        private GameElement _current;

        /// <summary>Raised when a producible-unit card is clicked.</summary>
        public event Action<UnitData> ProduceRequested;

        private void OnEnable()
        {
            GameEvents.SelectionChanged += OnSelectionChanged;
            GameEvents.HealthChanged += OnHealthChanged;
            Hide();
        }

        private void OnDisable()
        {
            GameEvents.SelectionChanged -= OnSelectionChanged;
            GameEvents.HealthChanged -= OnHealthChanged;
        }

        private void OnSelectionChanged(ISelectable selectable)
        {
            _current = selectable as GameElement;
            if (_current == null)
            {
                Hide();
                return;
            }

            Show(_current);
        }

        private void OnHealthChanged(IDamageable entity)
        {
            if (_current != null && ReferenceEquals(entity, _current))
            {
                UpdateHp(_current);
            }
        }

        private void Show(GameElement element)
        {
            _panelRoot.SetActive(true);
            _iconImage.sprite = element.Definition.Icon;
            _nameText.text = element.Definition.DisplayName;
            UpdateHp(element);
            BuildCards(element);
        }

        private void Hide()
        {
            ClearCards();
            _current = null;
            _panelRoot.SetActive(false);
        }

        private void UpdateHp(GameElement element)
        {
            _hpText.text = $"HP {element.CurrentHp}/{element.MaxHp}";
        }

        private void BuildCards(GameElement element)
        {
            ClearCards();
            if (!(element is IProducer producer) || !producer.CanProduce)
            {
                return;
            }

            IReadOnlyList<UnitData> units = producer.ProducibleUnits;
            for (int i = 0; i < units.Count; i++)
            {
                UnitCardView card = Instantiate(_unitCardPrefab, _cardContainer);
                card.Bind(units[i], OnCardClicked);
            }
        }

        private void ClearCards()
        {
            for (int i = _cardContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_cardContainer.GetChild(i).gameObject);
            }
        }

        private void OnCardClicked(UnitData unit)
        {
            ProduceRequested?.Invoke(unit);
        }
    }
}
