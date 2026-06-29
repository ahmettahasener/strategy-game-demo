using NUnit.Framework;
using StrategyDemo.Core;
using UnityEngine;

namespace StrategyDemo.Tests.PlayMode
{
    public sealed class PoolManagerTests
    {
        private PoolManager _pool;
        private GameObject _prefab;
        private GameObject _otherPrefab;
        private GameObject _activeRoot;

        [SetUp]
        public void SetUp()
        {
            _pool = new GameObject("PoolManager").AddComponent<PoolManager>();
            _prefab = new GameObject("Prefab");
            _otherPrefab = new GameObject("OtherPrefab");
            _activeRoot = new GameObject("UnitsRoot");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_pool.gameObject);
            Object.DestroyImmediate(_prefab);
            Object.DestroyImmediate(_otherPrefab);
            Object.DestroyImmediate(_activeRoot);
        }

        [Test]
        public void Get_FromEmptyPool_ReturnsActiveInstance()
        {
            GameObject instance = _pool.Get(_prefab);

            Assert.IsNotNull(instance);
            Assert.IsTrue(instance.activeSelf);
        }

        [Test]
        public void Release_DeactivatesInstance()
        {
            GameObject instance = _pool.Get(_prefab);

            _pool.Release(instance);

            Assert.IsFalse(instance.activeSelf);
        }

        [Test]
        public void Get_AfterRelease_ReusesSameInstance()
        {
            GameObject first = _pool.Get(_prefab);
            _pool.Release(first);

            GameObject second = _pool.Get(_prefab);

            Assert.AreSame(first, second);
            Assert.IsTrue(second.activeSelf);
        }

        [Test]
        public void Get_DifferentPrefabs_DoNotShareInstances()
        {
            GameObject fromFirst = _pool.Get(_prefab);
            _pool.Release(fromFirst);

            GameObject fromSecond = _pool.Get(_otherPrefab);

            Assert.AreNotSame(fromFirst, fromSecond);
        }

        [Test]
        public void Release_CalledTwice_DoesNotHandOutTheSameInstanceTwice()
        {
            GameObject first = _pool.Get(_prefab);
            _pool.Release(first);
            _pool.Release(first); // double-release must not queue it twice

            GameObject reused = _pool.Get(_prefab); // the one released instance
            GameObject fresh = _pool.Get(_prefab);  // pool now empty -> new instance

            Assert.AreSame(first, reused);
            Assert.AreNotSame(reused, fresh);
        }

        [Test]
        public void Get_WithActiveParent_MovesInstanceUnderThatParent()
        {
            GameObject instance = _pool.Get(_prefab, _activeRoot.transform);

            Assert.AreSame(_activeRoot.transform, instance.transform.parent);
        }

        [Test]
        public void Release_AfterActiveParent_ReturnsInstanceUnderPool()
        {
            GameObject instance = _pool.Get(_prefab, _activeRoot.transform);

            _pool.Release(instance);

            Assert.AreSame(_pool.transform, instance.transform.parent);
        }
    }
}
