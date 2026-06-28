using UnityEngine;

namespace StrategyDemo.Data
{
    /// <summary>Definition of a soldier unit — combat and movement stats (Brief #7, #10).</summary>
    [CreateAssetMenu(menuName = "StrategyDemo/Data/Unit", fileName = "UnitData")]
    public sealed class UnitData : EntityData
    {
        [SerializeField, Min(0)] private int _attackDamage;
        [SerializeField, Min(0f)] private float _moveSpeed = 1f;
        [Tooltip("Attack reach in cells (Chebyshev): 1 = adjacent (melee), 2 = one cell farther (ranged).")]
        [SerializeField, Min(1)] private int _attackRangeCells = 1;

        public int AttackDamage => _attackDamage;
        public float MoveSpeed => _moveSpeed;

        /// <summary>How far (in cells, Chebyshev) the unit can attack from.</summary>
        public int AttackRangeCells => _attackRangeCells;
    }
}
