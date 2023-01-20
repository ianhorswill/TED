using TED;
using static TED.Language;

namespace Tests
{
    [TestClass]
    public class PrimitiveTests
    {
        [TestMethod]
        public void LessThanTest()
        {
            var t = new TablePredicate<int, int>("t");
            for (var i = 0; i < 10; i++)
            for (var j = 0; j < 10; j++)
                t.AddRow(i, j);

            var s = new TablePredicate<int, int>("s");
            var x = (Var<int>)"x";
            var y = (Var<int>)"y";

            s[x, y].If(t[x, y], x < y);

            var hits = s.Rows.ToArray();
            for (var j = 0; j < 10; j++)
            {
                Assert.AreEqual(45, hits.Length);
                foreach (var (a, b) in hits)
                    Assert.IsTrue(a < b);
            }
        }

        readonly struct Vector2Int
        {
            public readonly int X;
            public readonly int Y;

            public static Vector2Int Zero = new Vector2Int(0, 0);
            public static Vector2Int UnitX = new Vector2Int(1, 0);
            public static Vector2Int UnitY = new Vector2Int(0, 1);

            public Vector2Int(int x, int y)
            {
                X = x;
                Y = y;
            }

            public static Vector2Int operator +(Vector2Int a, Vector2Int b) => new Vector2Int(a.X + b.X, a.Y+b.Y);
        }

        [TestMethod]
        public void SamTest()
        {
            var neighborhood = new[] { new Vector2Int(0, 1), new Vector2Int(1, 1),
                new Vector2Int(1, 0), new Vector2Int(1, -1),
                new Vector2Int(0, -1), new Vector2Int(-1, -1),
                new Vector2Int(-1, 0), new Vector2Int(-1, 1) };
            var cell =     (Var<Vector2Int>)"cell";
            var neighbor = (Var<Vector2Int>)"neighbor";
            var temp =     (Var<Vector2Int>)"temp";
            //var where = (Var<Vector2Int>)"where";
            var count = (Var<int>)"count";
            var b = (Var<bool>)"b";
            //var Neighbor = Definition("Neighbor", cell, neighbor).IfAndOnlyIf(In(temp, neighborhood), neighbor == cell + temp);

            var tileTable = Predicate("tileTable", cell, b);
            tileTable.IndexByKey(cell);
            var index = tileTable.KeyIndex(cell);
            tileTable.AddRow(Vector2Int.Zero, true);

            var NeighborCount = Predicate("NeighborCount", cell, count);
            NeighborCount[cell, count].If(tileTable[cell, b], count == Count(And[In(temp, neighborhood), neighbor == cell + temp, tileTable[neighbor, true]]));
            //NeighborCount[where, count].If(tileTable[cell, b], count == Count(And[Neighbor[where, neighbor], tileTable[neighbor, true]]));
            //NeighborCount[where, Count(And[Neighbor[where, neighbor], tileTable[neighbor, true]])].If(tileTable[where, b]);

            var s = new TablePredicate<Vector2Int>("s");
            var x = (Var<Vector2Int>)"x";
            s[x].If(tileTable[x, true], Constant(1) <= Constant(2));

            s[x].If(tileTable[x, true], NeighborCount[x, count], count < 2);
            Assert.IsTrue(s.Rows.Any());
        }

        [TestMethod]
        public void NegationTest()
        {
            var t = new TablePredicate<int, int>("t");
            for (var i = 0; i < 10; i++)
            for (var j = 0; j < 10; j++)
                t.AddRow(i, j);

            var s = new TablePredicate<int, int>("s");
            var x = (Var<int>)"x";
            var y = (Var<int>)"y";

            s[x, y].If(t[x, y], x < y);

            var u = new TablePredicate<int, int>("u");
            var v = new TablePredicate<int, int>("v");

            u[x,y].If(t[x,y], !s[x,y]);
            v[x,y].If(t[x,y], !(x < y));

            var hits = u.Rows.ToArray();
            for (var j = 0; j < 10; j++)
            {
                Assert.AreEqual(55, hits.Length);
                foreach (var (a, b) in hits)
                    Assert.IsTrue(a >= b);
            }
        }

        [TestMethod]
        public void AndTest()
        {
            var t = new TablePredicate<int, int>("t");
            for (var i = 0; i < 10; i++)
            for (var j = 0; j < 10; j++)
                t.AddRow(i, j);

            var u = new TablePredicate<int>("u");
            u.AddRow(2);
            u.AddRow(4);
            u.AddRow(6);

            var s = new TablePredicate<int>("s");
            var x = (Var<int>)"x";

            s[x].If(And[t[x,x], And[u[x]]]);

            var hits = s.Rows.ToArray();
            Assert.AreEqual(3, hits.Length);
            Assert.AreEqual(2, hits[0]);
            Assert.AreEqual(4, hits[1]);
            Assert.AreEqual(6, hits[2]);
        }

        [TestMethod]
        public void NegatedDefinitionTest()
        {
            var n = (Var<int>)"n";
            var A = Predicate("A", new[] { 1, 2, 3, 4, 5, 6 }, n);
            var B = Predicate("B", new[] { 1, 2, 3, 4 }, n);
            var C = Predicate("C", new[] { 3, 4, 5, 6 }, n);
            var D = Definition("D", n).IfAndOnlyIf(B[n], C[n]);
            var E = Predicate("E", n).If(A[n], !D[n]);
            var results = E.Rows.ToArray();
            Assert.AreEqual(4, results.Length);
            Assert.AreEqual(1, results[0]);
            Assert.AreEqual(2, results[1]);
            Assert.AreEqual(5, results[2]);
            Assert.AreEqual(6, results[3]);
        }

        [TestMethod]
        public void InTestModeTest()
        {
            var c = new[] { 1, 2, 3, 4, 5 };
            var n = (Var<int>)"n";
            var Test = Predicate("Test", n);
            Test[0].If(In<int>(0, c));
            Test[1].If(In<int>(4, c));
            CollectionAssert.AreEqual(new[]{1}, Test.Rows.ToArray());
        }

        [TestMethod]
        public void InGenerateModeTest()
        {
            var c = new[] { 1, 2, 3, 4, 5 };
            var n = (Var<int>)"n";
            var Test = Predicate("Test", n);
            Test[n].If(In<int>(n, c));  // Should just copy c into Test
            CollectionAssert.AreEqual(c, Test.Rows.ToArray());
        }
    }
}
