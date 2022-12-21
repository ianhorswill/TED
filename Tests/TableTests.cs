using TED;

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
            var index = new KeyIndex<int, int>(null, t, 0, n=>n);
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
            Assert.AreEqual(KeyIndex<int,int>.NotFound, index.RowWithKey(-1));
        }

        [TestMethod, ExpectedException(typeof(DuplicateKeyException))]
        public void DuplicateKeyTest()
        {
            var t = new Table<int>();
            var index = new KeyIndex<int, int>(null, t, 0, n=>n);
            t.AddIndex(index);
            t.Add(0);
            t.Add(0);
        }
    }
}