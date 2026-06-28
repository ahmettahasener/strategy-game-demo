using System;

namespace StrategyDemo.Pathfinding
{
    /// <summary>
    /// An item storable in a <see cref="Heap{T}"/>. The heap maintains <see cref="HeapIndex"/> so it
    /// can re-sort an item in O(log n) when its priority changes and test membership in O(1).
    /// </summary>
    public interface IHeapItem<T> : IComparable<T>
    {
        int HeapIndex { get; set; }
    }

    /// <summary>
    /// Binary min-heap: the item that compares lowest (via <see cref="IComparable{T}.CompareTo"/>) is
    /// always first out. Backs the A* open set with O(log n) Add / RemoveFirst / UpdateItem and O(1)
    /// Contains.
    /// </summary>
    public sealed class Heap<T> where T : IHeapItem<T>
    {
        private readonly T[] _items;
        private int _count;

        public Heap(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Heap capacity must be positive.");
            }

            _items = new T[capacity];
        }

        public int Count => _count;

        /// <summary>Backing-array size — the most items the heap can hold before it must be re-created.</summary>
        public int Capacity => _items.Length;

        /// <summary>
        /// Empties the heap for reuse without re-allocating its backing array. Clears the slots so
        /// stored references can be collected rather than pinned for the heap's lifetime.
        /// </summary>
        public void Clear()
        {
            Array.Clear(_items, 0, _count);
            _count = 0;
        }

        public void Add(T item)
        {
            if (_count >= _items.Length)
            {
                throw new InvalidOperationException("Heap capacity exceeded.");
            }

            item.HeapIndex = _count;
            _items[_count] = item;
            _count++;
            SortUp(item);
        }

        public T RemoveFirst()
        {
            if (_count == 0)
            {
                throw new InvalidOperationException("Cannot remove from an empty heap.");
            }

            T first = _items[0];
            _count--;
            _items[0] = _items[_count];
            _items[0].HeapIndex = 0;
            SortDown(_items[0]);
            return first;
        }

        /// <summary>Re-sorts an item whose priority decreased (A* found a cheaper path to it).</summary>
        public void UpdateItem(T item)
        {
            SortUp(item);
        }

        public bool Contains(T item)
        {
            return item.HeapIndex < _count && Equals(_items[item.HeapIndex], item);
        }

        private void SortUp(T item)
        {
            int parentIndex = (item.HeapIndex - 1) / 2;
            while (item.HeapIndex > 0 && item.CompareTo(_items[parentIndex]) < 0)
            {
                Swap(item, _items[parentIndex]);
                parentIndex = (item.HeapIndex - 1) / 2;
            }
        }

        private void SortDown(T item)
        {
            while (true)
            {
                int leftChild = item.HeapIndex * 2 + 1;
                int rightChild = item.HeapIndex * 2 + 2;

                if (leftChild >= _count)
                {
                    return;
                }

                int smallest = leftChild;
                if (rightChild < _count && _items[rightChild].CompareTo(_items[leftChild]) < 0)
                {
                    smallest = rightChild;
                }

                if (_items[smallest].CompareTo(item) >= 0)
                {
                    return;
                }

                Swap(item, _items[smallest]);
            }
        }

        private void Swap(T a, T b)
        {
            _items[a.HeapIndex] = b;
            _items[b.HeapIndex] = a;
            int temp = a.HeapIndex;
            a.HeapIndex = b.HeapIndex;
            b.HeapIndex = temp;
        }
    }
}
