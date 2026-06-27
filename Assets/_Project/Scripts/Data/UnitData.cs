using UnityEngine;

namespace StrategyDemo.Data
{
    /// <summary>Definition of a soldier unit — combat and movement stats (Brief #7, #10).</summary>
    [CreateAssetMenu(menuName = "StrategyDemo/Data/Unit", fileName = "UnitData")]
    public sealed class UnitData : EntityData
    {
        [SerializeField, Min(0)] private int _attackDamage;
        [SerializeField, Min(0f)] private float _moveSpeed = 1f;
        [SerializeField, Min(0f)] private float _attackRange = 1f;

        public int AttackDamage => _attackDamage;
        public float MoveSpeed => _moveSpeed;
        public float AttackRange => _attackRange;
    }
}
