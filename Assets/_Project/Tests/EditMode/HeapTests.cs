using System;
using NUnit.Framework;
using StrategyDemo.Pathfinding;

namespace StrategyDemo.Tests.EditMode
{
    public sealed class HeapTests
    {
        private sealed class Item : IHeapItem<Item>
        {
            public int Priority;

            public Item(int priority)
            {
                Priority = priority;
            }

            public int HeapIndex { get; set; }

            public int CompareTo(Item other)
            {
                return Priority.CompareTo(other.Priority);
            }
        }

        [Test]
        public void RemoveFirst_ReturnsItemsInAscendingPriority()
        {
            var heap = new Heap<Item>(8);
            heap.Add(new Item(5));
            heap.Add(new Item(1));
            heap.Add(new Item(3));
            heap.Add(new Item(4));
            heap.Add(new Item(2));

            Assert.AreEqual(1, heap.RemoveFirst().Priority);
            Assert.AreEqual(2, heap.RemoveFirst().Priority);
            Assert.AreEqual(3, heap.RemoveFirst().Priority);
            Assert.AreEqual(4, heap.RemoveFirst().Priority);
            Assert.AreEqual(5, heap.RemoveFirst().Priority);
        }

        [Test]
        public void Count_TracksAddsAndRemoves()
        {
            var heap = new Heap<Item>(8);
            Assert.AreEqual(0, heap.Count);

            heap.Add(new Item(1));
            heap.Add(new Item(2));
            Assert.AreEqual(2, heap.Count);

            heap.RemoveFirst();
            Assert.AreEqual(1, heap.Count);
        }

        [Test]
        public void Contains_TrueForAddedItem_FalseForOther()
        {
            var heap = new Heap<Item>(8);
            var added = new Item(1);
            var other = new Item(2);
            heap.Add(added);

            Assert.IsTrue(heap.Contains(added));
            Assert.IsFalse(heap.Contains(other));
        }

        [Test]
        public void UpdateItem_AfterPriorityDecrease_MovesItemToFront()
        {
            var heap = new Heap<Item>(8);
            heap.Add(new Item(1));
            heap.Add(new Item(2));
            var improved = new Item(10);
            heap.Add(improved);

            improved.Priority = 0;
            heap.UpdateItem(improved);

            Assert.AreSame(improved, heap.RemoveFirst());
        }

        [Test]
        public void RemoveFirst_OnEmptyHeap_Throws()
        {
            var heap = new Heap<Item>(4);

            Assert.Throws<InvalidOperationException>(() => heap.RemoveFirst());
        }

        [Test]
        public void Add_BeyondCapacity_Throws()
        {
            var heap = new Heap<Item>(1);
            heap.Add(new Item(1));

            Assert.Throws<InvalidOperationException>(() => heap.Add(new Item(2)));
        }
    }
}
