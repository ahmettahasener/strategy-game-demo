using System.Collections.Generic;
using NUnit.Framework;
using StrategyDemo.Core;
using StrategyDemo.Units;
using UnityEngine;

namespace StrategyDemo.Tests.PlayMode
{
    /// <summary>
    /// Covers the brief's enemy-only attack rule (Brief #10): a unit attacks live entities of another
    /// faction and never an ally, itself, or nothing. Builds real components (so the targeting rule is
    /// tested as the game runs it) but asserts the synchronous <see cref="UnitCombat.CanAttack"/> rule
    /// directly, keeping it stable — coroutine-driven approach/damage stays in manual QA.
    /// </summary>
    public sealed class UnitCombatTargetingTests
    {
        private readonly List<GameObject> _spawned = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject go in _spawned)
            {
                if (go != null)
                {
                    Object.DestroyImmediate(go);
                }
            }

            _spawned.Clear();
        }

        // RequireComponent pulls in UnitElement/UnitMovement/AttackEffector/SpriteRenderer at AddComponent.
        private UnitCombat NewUnit(Faction faction)
        {
            var go = new GameObject("Unit");
            _spawned.Add(go);
            UnitCombat combat = go.AddComponent<UnitCombat>();
            go.GetComponent<UnitElement>().SetFaction(faction);
            return combat;
        }

        [Test]
        public void CanAttack_EnemyFaction_ReturnsTrue()
        {
            UnitCombat player = NewUnit(Faction.Player);
            UnitElement enemy = NewUnit(Faction.Enemy).GetComponent<UnitElement>();

            Assert.IsTrue(player.CanAttack(enemy));
        }

        [Test]
        public void CanAttack_SameFaction_ReturnsFalse()
        {
            UnitCombat player = NewUnit(Faction.Player);
            UnitElement ally = NewUnit(Faction.Player).GetComponent<UnitElement>();

            Assert.IsFalse(player.CanAttack(ally));
        }

        [Test]
        public void CanAttack_Self_ReturnsFalse()
        {
            UnitCombat unit = NewUnit(Faction.Player);

            Assert.IsFalse(unit.CanAttack(unit.GetComponent<UnitElement>()));
        }

        [Test]
        public void CanAttack_NullTarget_ReturnsFalse()
        {
            UnitCombat unit = NewUnit(Faction.Player);

            Assert.IsFalse(unit.CanAttack(null));
        }
    }
}
