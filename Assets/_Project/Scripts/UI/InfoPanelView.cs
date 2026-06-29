using System;
using System.Collections;
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
        [SerializeField] private Color _enemyTint = new Color(1f, 0.4f, 0.4f); // matches the board enemy tint
        [SerializeField] private float _showScaleFrom = 0.92f;
        [SerializeField] private float _showDuration = 0.14f;

        private GameElement _current;
        private Coroutine _showRoutine;

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
            bool wasHidden = !_panelRoot.activeSelf;
            _panelRoot.SetActive(true);
            _iconImage.sprite = element.Definition.Icon;
            // Tint the icon to match the entity's faction colour on the board (red for enemies), so the
            // info panel reads as the same unit you clicked rather than a neutral player-coloured one.
            _iconImage.color = element.Faction == Faction.Enemy ? _enemyTint : Color.white;
            _nameText.text = element.Definition.DisplayName;
            UpdateHp(element);
            BuildCards(element);

            if (wasHidden)
            {
                PlayShowTransition();
            }
        }

        // A small scale-in when the panel first appears (alpha is left to the zoom-fader).
        private void PlayShowTransition()
        {
            if (_showRoutine != null)
            {
                StopCoroutine(_showRoutine);
            }

            _showRoutine = StartCoroutine(ShowTransition());
        }

        private IEnumerator ShowTransition()
        {
            Transform panel = _panelRoot.transform;
            float elapsed = 0f;
            while (elapsed < _showDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, _showDuration));
                float eased = 1f - (1f - t) * (1f - t);
                float scale = Mathf.Lerp(_showScaleFrom, 1f, eased);
                panel.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            panel.localScale = Vector3.one;
            _showRoutine = null;
        }

        private void Hide()
        {
            if (_showRoutine != null)
            {
                StopCoroutine(_showRoutine);
                _showRoutine = null;
            }

            ClearCards();
            _current = null;
            _panelRoot.transform.localScale = Vector3.one;
            _panelRoot.SetActive(false);
        }

        private void UpdateHp(GameElement element)
        {
            _hpText.text = $"HP {element.CurrentHp}/{element.MaxHp}";
        }

        private void BuildCards(GameElement element)
        {
            ClearCards();
            // Only the player's own producers offer production; an enemy building still shows its
            // identity and HP, but no produce cards (you can't make units from the enemy's barracks).
            if (!(element is IProducer producer) || !producer.CanProduce || element.Faction != Faction.Player)
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
