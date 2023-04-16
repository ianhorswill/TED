using TED;
using TED.Tables;
using static TED.Language;

namespace Tests
{
    [TestClass]
    public class TableTests
    {
        [TestMethod]
        public void TableAdd()
        {
            var t = new Table<uint>();
            for (var i = 0u; i < 100; i++)
                t.Add(i);
            for (var i = 0u; i < 100; i++)
            {
                Assert.AreEqual(i, t[i]);
                Assert.AreEqual(i, t.PositionReference(i));
            }
        }

        [TestMethod]
        public void SuppressDuplicatesTest1()
        {
            var t = new Table<string>() { Unique = true };
            t.Add("foo");
            Assert.AreEqual("foo", t[0]);
            Assert.AreEqual(1u, t.Length);
            t.Add("foo");
            Assert.AreEqual(1u, t.Length);
            t.Add("bar");
            Assert.AreEqual(2u, t.Length);
            Assert.AreEqual("bar", t[1]);
        }

        [TestMethod]
        public void SuppressDuplicatesTest2()
        {
            // Left shift by j places; this forces clustering for higher values of j
            // thus exercising the search
            for (int j = 0; j < 5; j++)
            {
                var t = new Table<uint>() { Unique = true };
                for (var i = 0u; i < 50; i++)
                    t.Add(i<<j);
                Assert.AreEqual(50u, t.Length);
                for (var i = 0u; i < 100; i++)
                    t.Add(i<<j);
                Assert.AreEqual(100u, t.Length);
                for (var i = 0u; i < 100; i++)
                {
                    Assert.AreEqual(i<<j, t[i]);
                    Assert.AreEqual(i<<j, t.PositionReference(i));
                }
            }
        }

        [TestMethod]
        public void ProbeTest()
        {
            var t = new Table<int>() { Unique = true };
            for (var i = 0; i < 100; i++)
                t.Add(i<<2);
            for (var i = 0; i < 400; i++)
            {
                Assert.AreEqual((i&3)==0, t.ContainsRowUsingRowSet(i));
            }
        }

        [TestMethod]
        public void FullTableProbeTest()
        {
            var t = new Table<int>() { Unique = true };
            for (var i = 0; i < 1025; i++)
            {
                t.Add(i<<2);
                Assert.IsFalse(t.ContainsRowUsingRowSet(-1));
                Assert.IsTrue(t.ContainsRowUsingRowSet(i<<2));
            }
        }

        [TestMethod]
        public void KeyIndexTest()
        {
            var t = new Table<int>();
            var index = new KeyIndex<int, int>(null!, t, 0, (in int n)=>n);
            t.AddIndex(index);
            for (var i = 0; i < 1025; i++)
            {
                var value = i << 2;
                t.Add(value);
            }
            for (var i = 0u; i < 1025; i++)
            {
                var value = i << 2;
                Assert.AreEqual(i, index.RowWithKey((int)value));
            }
            Assert.AreEqual(Table.NoRow, index.RowWithKey(-1));
        }

        [TestMethod, ExpectedException(typeof(DuplicateKeyException))]
        public void DuplicateKeyTest()
        {
            var t = new Table<int>();
            var index = new KeyIndex<int, int>(null!, t, 0, (in int n)=>n);
            t.AddIndex(index);
            t.Add(0);
            t.Add(0);
        }

        [TestMethod]
        public void GeneralIndexTest()
        {
            var t = new Table<int>();
            var index = new GeneralIndex<int, int>(null!, t, 0, (in int n)=>n);
            t.AddIndex(index);
            for (var i = 0; i < 1025; i++)
            {
                var value = i << 2;
                t.Add(value);
            }
            for (var i = 0u; i < 1025; i++)
            {
                var value = i << 2;
                Assert.AreEqual(i, index.FirstRowWithValue((int)value));
                Assert.AreEqual(Table.NoRow, index.NextRowWithValue(i));
            }
            Assert.AreEqual(Table.NoRow, index.FirstRowWithValue(-1));
            for (var i = 0; i < 1025; i++)
            {
                var value = i << 2;
                t.Add(value);
            }
            for (var i = 0u; i < 1025; i++)
            {
                var value = i << 2;
                var first = index.FirstRowWithValue((int)value);
                Assert.AreEqual(i+1025, first);
                var next = index.NextRowWithValue(first);
                Assert.AreEqual(i, next);
                Assert.AreEqual(Table.NoRow, index.NextRowWithValue(next));
            }
        }

        [TestMethod]
        public void IndexDeletionTest()
        {

            var t = new Table<int>();
            var index = new GeneralIndex<int, int>(null!, t, 0, (in int n)=>n);
            t.AddIndex(index);
            index.EnableMutation();

            IEnumerable<uint> RowsWithValue(int v)
            {
                for (var row = index.FirstRowWithValue(v); Table.ValidRow(row); row = index.NextRowWithValue(row))
                    yield return row;
            }

            for (var i = 0; i < 10; i++)
                t.Add(0);

            var rows = RowsWithValue(0).ToArray();
            CollectionAssert.AreEqual(new uint[] { 9,8,7,6,5,4,3,2,1,0 }, rows);

            // Test removing the first element
            index.Remove(9);
            rows = RowsWithValue(0).ToArray();
            CollectionAssert.AreEqual(new uint[] { 8,7,6,5,4,3,2,1,0 }, rows);

            // Test removing the last element
            index.Remove(0);
            rows = RowsWithValue(0).ToArray();
            CollectionAssert.AreEqual(new uint[] { 8,7,6,5,4,3,2,1 }, rows);

            // Test removing a middle element
            index.Remove(5);
            rows = RowsWithValue(0).ToArray();
            CollectionAssert.AreEqual(new uint[] { 8,7,6,4,3,2,1 }, rows);

            // Test removing all elements
            foreach (var row in rows) index.Remove(row);
            Assert.AreEqual(0, RowsWithValue(0).Count());

            // Test adding rows back in
            for (var i = 0u; i < 10u; i++)
                index.Add(i);

            rows = RowsWithValue(0).ToArray();
            CollectionAssert.AreEqual(new uint[] { 9,8,7,6,5,4,3,2,1,0 }, rows);
        }

        [TestMethod]
        public void MutatorUnindexed()
        {
            var s = (Var<string>)"s";
            var n = (Var<int>)"n";

            var T = Predicate("T", s.Key, n);
            T.AddRow("foo", 1);
            T.AddRow("bar", 2);
            var a = T.Accessor(s, n);
            Assert.AreEqual(1, a["foo"]);
            Assert.AreEqual(2, a["bar"]);
            a["foo"] = 3;
            Assert.AreEqual(3, a["foo"]);
        }

        [TestMethod]
        public void MutatorIndexed()
        {
            var s = (Var<string>)"s";
            var n = (Var<int>)"n";

            var T = Predicate("T", s.Key, n.Indexed);
            var index = (GeneralIndex<(string,int), int>)T.IndexFor(1, false)!;

            IEnumerable<uint> RowsWithValue(int v)
            {
                for (var row = index.FirstRowWithValue(v); Table.ValidRow(row); row = index.NextRowWithValue(row))
                    yield return row;
            }
            T.AddRow("foo", 1);
            T.AddRow("bar", 2);
            var a = T.Accessor(s, n);
            Assert.AreEqual(1, a["foo"]);
            Assert.AreEqual(2, a["bar"]);
            a["foo"] = 3;
            Assert.AreEqual(3, a["foo"]);
            CollectionAssert.AreEqual(new uint[] {0}, RowsWithValue(3).ToArray());

            a["foo"] = 2;
            CollectionAssert.AreEqual(new uint[] {0, 1}, RowsWithValue(2).ToArray());
            Assert.AreEqual(0, RowsWithValue(3).Count());
        }

        [TestMethod]
        public void UserKeyIndexTest()
        {
            var n = (Var<int>)"n";
            var m = (Var<int>)"m";
            var nums = new[] { 1, 2, 3, 4, 5, 6 };
            var Table = Predicate("Table", nums.Select(i => (i, i+1)), n.Key, m);
            var nKey = Table.KeyIndex(n);
            foreach (var i in nums)
                Assert.AreEqual(i+1, nKey[i].Item2);
        }
    }
}