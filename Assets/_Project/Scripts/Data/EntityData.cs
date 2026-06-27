using UnityEngine;

namespace StrategyDemo.Data
{
    /// <summary>
    /// Shared immutable definition data for every entity (buildings and units): the fields the
    /// production menu / info panel display and the factory builds from. Holds <b>only</b> common
    /// definition data — no behaviour, no faction, no runtime state (current HP, target, path live
    /// on the spawned entity, never written back here).
    /// </summary>
    public abstract class EntityData : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private Sprite _boardSprite;
        [SerializeField] private GameObject _prefab;
        [SerializeField, Min(1)] private int _maxHp = 1;

        public string DisplayName => _displayName;

        /// <summary>Badge sprite for the production card / info panel (UI).</summary>
        public Sprite Icon => _icon;

        /// <summary>World sprite drawn on the board by the spawned entity.</summary>
        public Sprite BoardSprite => _boardSprite;

        public GameObject Prefab => _prefab;
        public int MaxHp => _maxHp;
    }
}
