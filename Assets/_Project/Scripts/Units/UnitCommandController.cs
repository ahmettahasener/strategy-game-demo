using System.Collections;
using System.Collections.Generic;
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

        [Header("Path trail")]
        [SerializeField] private Sprite _pathDotSprite;
        [SerializeField] private Color _pathDotColor = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField] private float _pathDotSize = 0.28f;
        [SerializeField] private int _pathDotSortingOrder = 3;
        [SerializeField] private float _pathFadeDuration = 0.3f;
        [SerializeField] private float _dotReachRadius = 0.45f;

        private SpriteRenderer _commandMarker;
        private Coroutine _commandMarkerRoutine;

        private readonly List<SpriteRenderer> _pathDots = new List<SpriteRenderer>();
        private Coroutine _pathRoutine;
        private int _activeDotCount;

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

            if (_pathRoutine != null)
            {
                StopCoroutine(_pathRoutine);
                _pathRoutine = null;
            }

            ClearPathTrail();
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
                ClearPathTrail();
                ShowCommandMarker(target.transform.position, 0f);
            }
            else
            {
                // Move order: cancel any ongoing attack, then walk to the clicked cell.
                combat?.StopCombat();
                Vector2Int targetCell = GridManager.Instance.WorldToCell(_input.PointerWorldPosition);
                var movement = unit.GetComponent<UnitMovement>();
                if (movement != null && movement.MoveTo(targetCell))
                {
                    IReadOnlyList<Vector2Int> path = movement.LastPath;
                    ShowCommandMarker(GridManager.Instance.CellToWorldCenter(targetCell), EntryAngle(path));
                    ShowPathTrail(path, movement);
                }
            }
        }

        // Angle (degrees) of the unit's final step into the target, so the marker — a sprite that
        // points +X natively — faces the way the unit arrives (e.g. enters from below → points up).
        private static float EntryAngle(IReadOnlyList<Vector2Int> path)
        {
            if (path == null || path.Count < 2)
            {
                return 0f;
            }

            Vector2Int step = path[path.Count - 1] - path[path.Count - 2];
            return Mathf.Atan2(step.y, step.x) * Mathf.Rad2Deg;
        }

        private GameElement EntityUnderPointer()
        {
            Collider2D hit = Physics2D.OverlapPoint(_input.PointerWorldPosition);
            return hit != null ? hit.GetComponent<GameElement>() : null;
        }

        private void ShowCommandMarker(Vector3 position, float rotationZ)
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
            _commandMarker.transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
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

        // Lays a dot on each cell between the unit and its target, so the player can see the route the
        // unit will walk. Dots stay lit while the unit travels, then fade out (see PathTrailRoutine).
        // One pooled renderer per cell, all using the same atlas dot sprite — no per-order allocation.
        private void ShowPathTrail(IReadOnlyList<Vector2Int> path, UnitMovement movement)
        {
            if (_pathRoutine != null)
            {
                StopCoroutine(_pathRoutine);
                _pathRoutine = null;
            }

            // path[0] is the cell the unit stands on and the last cell gets the command marker, so the
            // dots cover only the cells strictly in between.
            int dotCount = _pathDotSprite != null && path != null ? Mathf.Max(0, path.Count - 2) : 0;
            for (int i = 0; i < dotCount; i++)
            {
                SpriteRenderer dot = GetPathDot(i);
                dot.transform.position = GridManager.Instance.CellToWorldCenter(path[i + 1]);
                dot.color = _pathDotColor;
                dot.enabled = true;
            }

            for (int i = dotCount; i < _pathDots.Count; i++)
            {
                _pathDots[i].enabled = false;
            }

            _activeDotCount = dotCount;
            if (dotCount > 0)
            {
                _pathRoutine = StartCoroutine(PathTrailRoutine(movement));
            }
        }

        // Keeps only the dots ahead of the unit lit: as the unit reaches each dot (front of the trail),
        // that dot switches off, so the breadcrumb shrinks toward the target. Whatever is still ahead
        // when the unit stops fades out together.
        private IEnumerator PathTrailRoutine(UnitMovement movement)
        {
            int passed = 0;
            float reachSqr = _dotReachRadius * _dotReachRadius;
            while (movement != null && movement.IsMoving)
            {
                Vector3 unitPosition = movement.transform.position;
                while (passed < _activeDotCount
                    && (unitPosition - _pathDots[passed].transform.position).sqrMagnitude <= reachSqr)
                {
                    _pathDots[passed].enabled = false;
                    passed++;
                }

                yield return null;
            }

            float elapsed = 0f;
            while (elapsed < _pathFadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = _pathDotColor.a * (1f - Mathf.Clamp01(elapsed / _pathFadeDuration));
                for (int i = 0; i < _pathDots.Count; i++)
                {
                    if (!_pathDots[i].enabled)
                    {
                        continue;
                    }

                    Color color = _pathDotColor;
                    color.a = alpha;
                    _pathDots[i].color = color;
                }

                yield return null;
            }

            ClearPathTrail();
            _pathRoutine = null;
        }

        private SpriteRenderer GetPathDot(int index)
        {
            if (index < _pathDots.Count)
            {
                return _pathDots[index];
            }

            var dotObject = new GameObject("PathDot");
            dotObject.transform.SetParent(transform, false);
            var dot = dotObject.AddComponent<SpriteRenderer>();
            dot.sprite = _pathDotSprite;
            dot.sortingOrder = _pathDotSortingOrder;
            float native = Mathf.Max(0.0001f, _pathDotSprite.bounds.size.x);
            dot.transform.localScale = Vector3.one * (_pathDotSize / native);
            _pathDots.Add(dot);
            return dot;
        }

        private void ClearPathTrail()
        {
            for (int i = 0; i < _pathDots.Count; i++)
            {
                _pathDots[i].enabled = false;
            }
        }
    }
}
