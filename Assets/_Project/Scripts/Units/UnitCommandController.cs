using System.Collections;
using StrategyDemo.Core;
using StrategyDemo.Grid;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StrategyDemo.Units
{
    /// <summary>
    /// Translates a right-click into a move order for the selected unit: paths it to the clicked
    /// cell (Brief #6). Right-clicks during placement belong to placement (cancel), not movement.
    /// </summary>
    public sealed class UnitCommandController : MonoBehaviour
    {
        [SerializeField] private InputReader _input;
        [SerializeField] private PlacementController _placement;
        [SerializeField] private Sprite _commandMarkerSprite;
        [SerializeField] private Color _commandMarkerColor = new Color(0.35f, 0.85f, 1f, 0.9f);
        [SerializeField] private float _commandMarkerDuration = 0.35f;
        [SerializeField] private float _commandMarkerStartScale = 0.55f;
        [SerializeField] private float _commandMarkerEndScale = 0.9f;
        [SerializeField] private int _commandMarkerSortingOrder = 4;

        private SpriteRenderer _commandMarker;
        private Coroutine _commandMarkerRoutine;

        private void OnEnable()
        {
            _input.SecondaryPressed += OnSecondaryPressed;
        }

        private void OnDisable()
        {
            _input.SecondaryPressed -= OnSecondaryPressed;
            if (_commandMarkerRoutine != null)
            {
                StopCoroutine(_commandMarkerRoutine);
                _commandMarkerRoutine = null;
            }

            if (_commandMarker != null)
            {
                _commandMarker.gameObject.SetActive(false);
            }
        }

        private void OnSecondaryPressed()
        {
            // A right-click on a panel (build menu, production buttons) must not leak a world
            // move/attack order to the cell hidden behind the UI.
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (_placement != null && _placement.IsPlacing)
            {
                return;
            }

            if (!(SelectionManager.Instance.Current is UnitElement unit))
            {
                return;
            }

            GameElement target = EntityUnderPointer();
            var combat = unit.GetComponent<UnitCombat>();

            if (combat != null && combat.CanAttack(target))
            {
                combat.Attack(target);
                ShowCommandMarker(target.transform.position);
            }
            else
            {
                // Move order: cancel any ongoing attack, then walk to the clicked cell.
                combat?.StopCombat();
                Vector2Int targetCell = GridManager.Instance.WorldToCell(_input.PointerWorldPosition);
                if (unit.GetComponent<UnitMovement>()?.MoveTo(targetCell) == true)
                {
                    ShowCommandMarker(GridManager.Instance.CellToWorldCenter(targetCell));
                }
            }
        }

        private GameElement EntityUnderPointer()
        {
            Collider2D hit = Physics2D.OverlapPoint(_input.PointerWorldPosition);
            return hit != null ? hit.GetComponent<GameElement>() : null;
        }

        private void ShowCommandMarker(Vector3 position)
        {
            if (_commandMarkerSprite == null)
            {
                return;
            }

            EnsureCommandMarker();
            if (_commandMarkerRoutine != null)
            {
                StopCoroutine(_commandMarkerRoutine);
            }

            _commandMarker.transform.position = new Vector3(position.x, position.y, 0f);
            _commandMarker.gameObject.SetActive(true);
            _commandMarkerRoutine = StartCoroutine(CommandMarkerRoutine());
        }

        private void EnsureCommandMarker()
        {
            if (_commandMarker != null)
            {
                return;
            }

            var markerObject = new GameObject("CommandMarker");
            markerObject.transform.SetParent(transform, false);
            _commandMarker = markerObject.AddComponent<SpriteRenderer>();
            _commandMarker.sprite = _commandMarkerSprite;
            _commandMarker.sortingOrder = _commandMarkerSortingOrder;
            _commandMarker.gameObject.SetActive(false);
        }

        private IEnumerator CommandMarkerRoutine()
        {
            float elapsed = 0f;
            while (elapsed < _commandMarkerDuration)
            {
                elapsed += Time.deltaTime;
                float ratio = Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, _commandMarkerDuration));
                float eased = 1f - Mathf.Pow(1f - ratio, 2f);
                float scale = Mathf.Lerp(_commandMarkerStartScale, _commandMarkerEndScale, eased);

                Color color = _commandMarkerColor;
                color.a *= 1f - ratio;
                _commandMarker.color = color;
                _commandMarker.transform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            _commandMarker.gameObject.SetActive(false);
            _commandMarkerRoutine = null;
        }
    }
}
