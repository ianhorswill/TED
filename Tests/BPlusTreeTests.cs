using System;
using System.Collections.Generic;
using System.Linq;
using TED.Tables;

namespace Tests
{
    [TestClass]
    public class BPlusTreeTests
    {
        #region Helpers
        /// <summary>
        /// Build a tree whose keys are indices into <paramref name="values"/> and which is ordered by the int at
        /// that index.  The keys 0..values.Length-1 are all inserted.
        /// </summary>
        private static BPlusTree<int> Build(int[] values, int order = 32)
        {
            var tree = new BPlusTree<int>(k => values[(int)k], Comparer<int>.Default, order);
            for (var k = 0u; k < values.Length; k++) tree.Insert(k);
            return tree;
        }

        /// <summary>The keys 0..values.Length-1 sorted the way the tree should sort them: by value, ties by key.</summary>
        private static List<uint> ExpectedOrder(int[] values, Func<int, bool>? predicate = null) =>
            Enumerable.Range(0, values.Length).Select(i => (uint)i)
                .Where(k => predicate == null || predicate(values[(int)k]))
                .OrderBy(k => values[(int)k]).ThenBy(k => k)
                .ToList();

        private static void AssertSequence(IEnumerable<uint> expected, IEnumerable<uint> actual) =>
            CollectionAssert.AreEqual(expected.ToList(), actual.ToList());
        #endregion

        [TestMethod]
        public void EmptyTree()
        {
            var tree = Build(Array.Empty<int>());
            Assert.AreEqual(0, tree.Count);
            Assert.IsFalse(tree.Contains(0));
            Assert.IsFalse(tree.Remove(0));
            Assert.IsFalse(tree.TryGetMinimum(out _));
            Assert.IsFalse(tree.TryGetMaximum(out _));
            Assert.IsFalse(tree.Any());
            Assert.AreEqual(0, tree.GreaterThan(0).Count());
            Assert.AreEqual(0, tree.GreaterThanOrEqual(0).Count());
            Assert.AreEqual(0, tree.LessThan(0).Count());
            Assert.AreEqual(0, tree.LessThanOrEqual(0).Count());
        }

        [TestMethod]
        public void InsertAndContains()
        {
            // Key 10 has a defined value but is deliberately not inserted, so Contains can be probed for a
            // valid-but-absent key.  (Contains must be able to look up the value of any key it is asked about.)
            var values = new[] { 5, 3, 8, 1, 9, 2, 7, 4, 6, 0, 42 };
            var tree = new BPlusTree<int>(k => values[(int)k], Comparer<int>.Default);
            for (var k = 0u; k < 10; k++) tree.Insert(k);

            Assert.AreEqual(10, tree.Count);
            for (var k = 0u; k < 10; k++)
                Assert.IsTrue(tree.Contains(k), $"missing key {k}");
            Assert.IsFalse(tree.Contains(10));
        }

        [TestMethod]
        public void DuplicateInsertIsNoOp()
        {
            var values = new[] { 10, 20, 30 };
            var tree = Build(values);
            Assert.AreEqual(3, tree.Count);
            tree.Insert(1);
            tree.Insert(1);
            Assert.AreEqual(3, tree.Count);
            AssertSequence(ExpectedOrder(values), tree);
        }

        [TestMethod]
        public void InOrderTraversalIsSortedByValue()
        {
            // Values chosen so that key order and value order differ.
            var values = new[] { 40, 10, 30, 50, 20, 0 };
            var tree = Build(values, order: 4);   // small order to force multiple internal levels
            AssertSequence(ExpectedOrder(values), tree);
        }

        [TestMethod]
        public void TiesAreBrokenByKey()
        {
            // Every key maps to the same value, so ordering must fall back to the key itself.
            var values = Enumerable.Repeat(7, 20).ToArray();
            var tree = Build(values, order: 4);
            AssertSequence(Enumerable.Range(0, 20).Select(i => (uint)i), tree);
        }

        [TestMethod]
        public void MinimumAndMaximum()
        {
            var values = new[] { 5, 3, 8, 1, 9, 2 };
            var tree = Build(values);
            Assert.IsTrue(tree.TryGetMinimum(out var min));
            Assert.IsTrue(tree.TryGetMaximum(out var max));
            Assert.AreEqual(3u, min);   // value 1 is smallest
            Assert.AreEqual(4u, max);   // value 9 is largest
        }

        [TestMethod]
        public void RemoveMaintainsOrderAndCount()
        {
            var values = Enumerable.Range(0, 200).Select(i => (i * 37) % 200).ToArray();
            var tree = Build(values, order: 4);
            var present = new HashSet<uint>(Enumerable.Range(0, 200).Select(i => (uint)i));

            // Remove keys in a scrambled order.
            foreach (var k in Enumerable.Range(0, 200).Select(i => (uint)((i * 91) % 200)))
            {
                Assert.IsTrue(tree.Remove(k));
                present.Remove(k);
                Assert.AreEqual(present.Count, tree.Count);
                var expected = present.OrderBy(x => values[(int)x]).ThenBy(x => x);
                AssertSequence(expected, tree);
            }
            Assert.AreEqual(0, tree.Count);
        }

        [TestMethod]
        public void RemoveNonexistentReturnsFalse()
        {
            // Key 3 has a defined value but is not inserted.
            var values = new[] { 1, 2, 3, 4 };
            var tree = new BPlusTree<int>(k => values[(int)k], Comparer<int>.Default);
            tree.Insert(0);
            tree.Insert(1);
            tree.Insert(2);
            Assert.IsFalse(tree.Remove(3));
            Assert.IsTrue(tree.Remove(0));
            Assert.IsFalse(tree.Remove(0));
            Assert.AreEqual(2, tree.Count);
        }

        [TestMethod]
        public void RangeQueriesExactValues()
        {
            // value[k] == k, so keys and values coincide and the answers are easy to state.
            var values = Enumerable.Range(0, 10).ToArray();
            var tree = Build(values, order: 4);

            AssertSequence(new uint[] { 6, 7, 8, 9 }, tree.GreaterThan(5));
            AssertSequence(new uint[] { 5, 6, 7, 8, 9 }, tree.GreaterThanOrEqual(5));
            AssertSequence(new uint[] { 0, 1, 2, 3, 4 }, tree.LessThan(5));
            AssertSequence(new uint[] { 0, 1, 2, 3, 4, 5 }, tree.LessThanOrEqual(5));

            // Bounds outside the populated range.
            AssertSequence(Enumerable.Range(0, 10).Select(i => (uint)i), tree.GreaterThanOrEqual(-1));
            AssertSequence(Array.Empty<uint>(), tree.GreaterThan(9));
            AssertSequence(Array.Empty<uint>(), tree.LessThan(0));
            AssertSequence(Enumerable.Range(0, 10).Select(i => (uint)i), tree.LessThanOrEqual(100));
        }

        [TestMethod]
        public void RangeQueriesWithTiesUseValueBound()
        {
            // Keys 0,1,2 -> value 10; keys 3,4 -> value 20; keys 5,6,7 -> value 30.
            var values = new[] { 10, 10, 10, 20, 20, 30, 30, 30 };
            var tree = Build(values, order: 4);

            // Everything with value strictly greater than 10 == the 20s and 30s, ties broken by key.
            AssertSequence(new uint[] { 3, 4, 5, 6, 7 }, tree.GreaterThan(10));
            // value >= 20
            AssertSequence(new uint[] { 3, 4, 5, 6, 7 }, tree.GreaterThanOrEqual(20));
            // value < 20 == the 10s
            AssertSequence(new uint[] { 0, 1, 2 }, tree.LessThan(20));
            // value <= 20 == 10s and 20s
            AssertSequence(new uint[] { 0, 1, 2, 3, 4 }, tree.LessThanOrEqual(20));
            // A bound that matches no value at all.
            AssertSequence(new uint[] { 5, 6, 7 }, tree.GreaterThan(25));
            AssertSequence(new uint[] { 0, 1, 2, 3, 4 }, tree.LessThan(25));
        }

        [TestMethod]
        public void ClearEmptiesAndTreeStillUsable()
        {
            var values = Enumerable.Range(0, 100).Select(i => 99 - i).ToArray();
            var tree = Build(values, order: 4);
            Assert.AreEqual(100, tree.Count);

            tree.Clear();
            Assert.AreEqual(0, tree.Count);
            Assert.IsFalse(tree.Any());
            Assert.IsFalse(tree.Contains(0));

            // Reusing after Clear exercises the node pool; results must still be correct.
            for (var k = 0u; k < values.Length; k++) tree.Insert(k);
            Assert.AreEqual(100, tree.Count);
            AssertSequence(ExpectedOrder(values), tree);
        }

        [TestMethod]
        public void CustomComparerControlsOrdering()
        {
            var values = new[] { 5, 3, 8, 1, 9, 2 };
            var descending = Comparer<int>.Create((a, b) => b.CompareTo(a));
            var tree = new BPlusTree<int>(k => values[(int)k], descending, order: 4);
            for (var k = 0u; k < values.Length; k++) tree.Insert(k);

            // Descending by value, ties broken by key ascending (the tie-break is always on the key).
            var expected = Enumerable.Range(0, values.Length).Select(i => (uint)i)
                .OrderByDescending(k => values[(int)k]).ThenBy(k => k);
            AssertSequence(expected, tree);

            Assert.IsTrue(tree.TryGetMinimum(out var min));   // "minimum" under this comparer == largest value
            Assert.AreEqual(4u, min);
        }

        [TestMethod]
        public void ReferenceTypeKeysWork()
        {
            var values = new[] { "delta", "alpha", "charlie", "bravo", "echo" };
            var tree = new BPlusTree<string>(k => values[(int)k], StringComparer.Ordinal, order: 4);
            for (var k = 0u; k < values.Length; k++) tree.Insert(k);

            var expected = Enumerable.Range(0, values.Length).Select(i => (uint)i)
                .OrderBy(k => values[(int)k], StringComparer.Ordinal).ThenBy(k => k);
            AssertSequence(expected, tree);
            AssertSequence(new uint[] { 0, 4 }, tree.GreaterThan("charlie")); // delta, echo
        }

        [TestMethod]
        public void MaxOrderBoundaryWorks()
        {
            // order 255 -> maxKeys 254; filling a node triggers a transient Count of 255, the byte maximum, before
            // it splits.  This exercises the edge of the byte-sized count field.
            var values = Enumerable.Range(0, 400).Select(i => 400 - i).ToArray();
            var tree = Build(values, order: 255);
            Assert.AreEqual(400, tree.Count);
            AssertSequence(ExpectedOrder(values), tree);
        }

        [TestMethod]
        public void InvalidOrderThrows()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => new BPlusTree<int>(k => 0, Comparer<int>.Default, 3));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => new BPlusTree<int>(k => 0, Comparer<int>.Default, 256));
        }

        [TestMethod]
        public void RangeQueriesRandomizedAgainstOracle()
        {
            foreach (var order in new[] { 4, 5, 8, 32 })
            {
                var rand = new Random(1000 + order);
                const int n = 300;
                var values = new int[n];
                for (var i = 0; i < n; i++) values[i] = rand.Next(0, 40);   // lots of ties
                var tree = Build(values, order);

                for (var v = -1; v <= 41; v++)
                {
                    var bound = v;
                    AssertSequence(ExpectedOrder(values, x => x > bound), tree.GreaterThan(bound));
                    AssertSequence(ExpectedOrder(values, x => x >= bound), tree.GreaterThanOrEqual(bound));
                    AssertSequence(ExpectedOrder(values, x => x < bound), tree.LessThan(bound));
                    AssertSequence(ExpectedOrder(values, x => x <= bound), tree.LessThanOrEqual(bound));
                }
            }
        }

        [TestMethod]
        public void RandomizedInsertRemoveAgainstOracle()
        {
            foreach (var order in new[] { 4, 5, 8, 16 })
            {
                var rand = new Random(500 + order);
                const int n = 400;
                var values = new int[n];
                for (var i = 0; i < n; i++) values[i] = rand.Next(0, 60);   // ties are common
                var tree = new BPlusTree<int>(k => values[(int)k], Comparer<int>.Default, order);
                var present = new HashSet<uint>();

                for (var step = 0; step < 6000; step++)
                {
                    var k = (uint)rand.Next(n);
                    if (present.Contains(k))
                    {
                        Assert.IsTrue(tree.Contains(k));
                        if (rand.Next(2) == 0)
                        {
                            Assert.IsTrue(tree.Remove(k));
                            present.Remove(k);
                        }
                    }
                    else
                    {
                        Assert.IsFalse(tree.Contains(k));
                        tree.Insert(k);
                        present.Add(k);
                    }

                    if (step % 300 == 0)
                    {
                        Assert.AreEqual(present.Count, tree.Count);
                        var expected = present.OrderBy(x => values[(int)x]).ThenBy(x => x);
                        AssertSequence(expected, tree);
                    }
                }

                Assert.AreEqual(present.Count, tree.Count);
                AssertSequence(present.OrderBy(x => values[(int)x]).ThenBy(x => x), tree);
            }
        }
    }
}
