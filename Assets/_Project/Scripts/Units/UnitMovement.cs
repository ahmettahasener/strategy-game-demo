using System.Collections;
using System.Collections.Generic;
using StrategyDemo.Core;
using UnityEngine;

namespace StrategyDemo.Units
{
    /// <summary>
    /// Moves the unit along an A* path to a target cell, executed over frames with a Coroutine
    /// (Brief #6). Reads the path from <see cref="GridManager"/> and the speed from the unit's data.
    /// </summary>
    [RequireComponent(typeof(UnitElement))]
    public sealed class UnitMovement : MonoBehaviour
    {
        private UnitElement _unit;
        private Coroutine _moveRoutine;

        private void Awake()
        {
            _unit = GetComponent<UnitElement>();
        }

        /// <summary>Paths to <paramref name="targetCell"/> and starts moving; ignores unreachable targets.</summary>
        public void MoveTo(Vector2Int targetCell)
        {
            Vector2Int startCell = GridManager.Instance.WorldToCell(transform.position);
            List<Vector2Int> path = GridManager.Instance.FindPath(startCell, targetCell);
            if (path.Count <= 1)
            {
                return;
            }

            if (_moveRoutine != null)
            {
                StopCoroutine(_moveRoutine);
            }

            _moveRoutine = StartCoroutine(FollowPath(Simplify(path)));
        }

        private IEnumerator FollowPath(List<Vector2Int> waypoints)
        {
            float speed = Mathf.Max(0.01f, _unit.Data.MoveSpeed);
            for (int i = 1; i < waypoints.Count; i++)
            {
                Vector3 destination = GridManager.Instance.CellToWorldCenter(waypoints[i]);
                while ((transform.position - destination).sqrMagnitude > 0.0001f)
                {
                    transform.position =
                        Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
                    yield return null;
                }

                transform.position = destination;
            }

            _moveRoutine = null;
        }

        // Collapse straight runs into direction-change waypoints so movement is smooth.
        private static List<Vector2Int> Simplify(List<Vector2Int> path)
        {
            if (path.Count < 3)
            {
                return path;
            }

            var waypoints = new List<Vector2Int> { path[0] };
            for (int i = 1; i < path.Count - 1; i++)
            {
                Vector2Int previous = path[i] - path[i - 1];
                Vector2Int next = path[i + 1] - path[i];
                if (previous != next)
                {
                    waypoints.Add(path[i]);
                }
            }

            waypoints.Add(path[path.Count - 1]);
            return waypoints;
        }
    }
}
