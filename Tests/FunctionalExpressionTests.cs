using TED;
using static TED.Language;
// ReSharper disable InconsistentNaming

namespace Tests
{
    [TestClass]
    public class FunctionalExpressionTests
    {
        [TestMethod]
        public void EvaluationTest()
        {
            var Negative = Function<int,int>("Negative", n => -n);
            var e = Negative[1].MakeEvaluator(new GoalAnalyzer());
            Assert.AreEqual(-1, e());
        }

        [TestMethod]
        public void SetTest()
        {
            var Negative = Function<int,int>("Negative", n => -n);
            var n = (Var<int>)"n";
            var T = Predicate("T", n).If(Eval(n, Negative[1]));
            var r = T.ToArray();
            Assert.AreEqual(1, r.Length);
            Assert.AreEqual(-1, r[0]);
        }

        [TestMethod]
        public void EqualityTest()
        {
            var Negative = Function<int,int>("Negative", n => -n);
            var n = (Var<int>)"n";
            var T = Predicate("T", n).If(n == Negative[1]);
            var r = T.ToArray();
            Assert.AreEqual(1, r.Length);
            Assert.AreEqual(-1, r[0]);
        }

        [TestMethod]
        public void IntAdditionTest()
        {
            var n = (Var<int>)"n";
            var T = Predicate("t", n).If(n == 1 + 2);
            var r = T.ToArray();
            Assert.AreEqual(1, r.Length);
            Assert.AreEqual(3, r[0]);
        }

        [TestMethod]
        public void FloatAdditionTest()
        {
            var n = (Var<float>)"n";
            var T = Predicate("t", n).If(n == 1f + 2f);
            var r = T.ToArray();
            Assert.AreEqual(1, r.Length);
            Assert.AreEqual(3, r[0]);
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
        public void CustomClassAdditionTest()
        {
            var n = (Var<Vector2Int>)"n";
            var T = Predicate("t", n).If(n == Constant(Vector2Int.UnitX) + Constant(Vector2Int.UnitY));
            var r = T.ToArray();
            Assert.AreEqual(1, r.Length);
            Assert.AreEqual(new Vector2Int(1,1), r[0]);
        }

        [TestMethod, ExpectedException(typeof(MissingMethodException))]
        public void CustomClassMissingOperatorTest()
        {
            var n = (Var<Vector2Int>)"n";
            var T = Predicate("t", n).If(n == Constant(Vector2Int.UnitX) * Constant(Vector2Int.UnitY));
            var r = T.ToArray();
            Assert.AreEqual(1, r.Length);
            Assert.AreEqual(new Vector2Int(1,1), r[0]);
        }

        [TestMethod]
        public void NegationTest()
        {
            var n = (Var<int>)"n";
            var m = (Var<int>)"m";
            var T = Predicate("t", n).If(m == 1, n == -m);
            var r = T.ToArray();
            Assert.AreEqual(1, r.Length);
            Assert.AreEqual(-1, r[0]);
        }

        
        [TestMethod]
        public void Modulus()
        {
            var n = (Var<int>)"n";
            var m = (Var<int>)"m";
            var P = Predicate("P", new[] { 1, 2, 3, 4, 5, 6 }, n);
            var Q = Predicate("Q", n).If(P[n], n%2 == Constant(0));
            var answers = Q.ToArray();
            Assert.AreEqual(3, answers.Length);
            Assert.AreEqual(2, answers[0]);
            Assert.AreEqual(4, answers[1]);
            Assert.AreEqual(6, answers[2]);
        }

        [TestMethod]
        public void CountTest()
        {
            var n = (Var<int>)"n";
            var m = (Var<int>)"m";
            var P = Predicate("P", new[] { 1, 2, 3, 4, 5, 6 }, n);
            var Q = Predicate("Q", n).If(n == Count(And[P[m], m%2 == Constant(0)]));
            var answers = Q.ToArray();
            Assert.AreEqual(1, answers.Length);
            Assert.AreEqual(3, answers[0]);
        }

        [TestMethod]
        public void Summation()
        {
            var n = (Var<int>)"n";
            var m = (Var<int>)"m";
            var P = Predicate("P", new[] { 1, 2, 3, 4, 5, 6 }, n);
            var Q = Predicate("Q", n).If(n == SumInt(m, And[P[m], m%2 == Constant(0)]));
            var answers = Q.ToArray();
            Assert.AreEqual(1, answers.Length);
            Assert.AreEqual(12, answers[0]);
        }

        [TestMethod]
        public void CellCountTest()
        {
            
            var neighborhood = new[] { new Vector2Int(0, 1), new Vector2Int(1, 1),
                new Vector2Int(1, 0), new Vector2Int(1, -1),
                new Vector2Int(0, -1), new Vector2Int(-1, -1),
                new Vector2Int(-1, 0), new Vector2Int(-1, 1) };
            var state = (Var<bool>)"state";
            var cell = (Var<Vector2Int>)"cell";
            var neighbor = (Var<Vector2Int>)"neighbor";
            var t = (Var<Vector2Int>)"t";
            var where = (Var<Vector2Int>)"where";
            var count = (Var<int>)"count";
            var b = (Var<bool>)"b";
            var tileTable = Predicate("tileTable", cell, state);
            var Neighbor = Definition("Neighbor", cell, neighbor).IfAndOnlyIf(In(t, neighborhood), neighbor == cell + t);
            var NeighborCount = Predicate("NeighborCount", cell, count);

            NeighborCount[where, count].If(tileTable[where, b], count == Count(And[Neighbor[where, neighbor], tileTable[neighbor, true]]));
        }

        [TestMethod]
        public void CellCountTest2()
        {
            
            var neighborhood = new[] { new Vector2Int(0, 1), new Vector2Int(1, 1),
                new Vector2Int(1, 0), new Vector2Int(1, -1),
                new Vector2Int(0, -1), new Vector2Int(-1, -1),
                new Vector2Int(-1, 0), new Vector2Int(-1, 1) };
            var state = (Var<bool>)"state";
            var cell = (Var<Vector2Int>)"cell";
            var neighbor = (Var<Vector2Int>)"neighbor";
            var t = (Var<Vector2Int>)"t";
            var where = (Var<Vector2Int>)"where";
            var count = (Var<int>)"count";
            var b = (Var<bool>)"b";
            var tileTable = Predicate("tileTable", cell, state);
            var Neighbor = Definition("Neighbor", cell, neighbor).IfAndOnlyIf();
            var NeighborCount = Predicate("NeighborCount", cell, count);

            NeighborCount[cell, count].If(tileTable[cell, b], count == Count(And[In(t, neighborhood), neighbor == cell + t, tileTable[neighbor, true]]));
        }
    }
}
