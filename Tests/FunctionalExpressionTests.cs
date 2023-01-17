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
            var r = T.Rows.ToArray();
            Assert.AreEqual(1, r.Length);
            Assert.AreEqual(-1, r[0]);
        }

        [TestMethod]
        public void EqualityTest()
        {
            var Negative = Function<int,int>("Negative", n => -n);
            var n = (Var<int>)"n";
            var T = Predicate("T", n).If(n == Negative[1]);
            var r = T.Rows.ToArray();
            Assert.AreEqual(1, r.Length);
            Assert.AreEqual(-1, r[0]);
        }

        [TestMethod]
        public void AdditionTest()
        {
            var n = (Var<int>)"n";
            var T = Predicate("t", n).If(n == 1 + 2);
            var r = T.Rows.ToArray();
            Assert.AreEqual(1, r.Length);
            Assert.AreEqual(3, r[0]);
        }

        readonly struct V2Int
        {
            public readonly int X;
            public readonly int Y;

            public static V2Int Zero = new V2Int(0, 0);
            public static V2Int UnitX = new V2Int(1, 0);
            public static V2Int UnitY = new V2Int(0, 1);

            public V2Int(int x, int y)
            {
                X = x;
                Y = y;
            }

            public static V2Int operator +(V2Int a, V2Int b) => new V2Int(a.X + b.X, a.Y+b.Y);
        }

        [TestMethod]
        public void CustomClassAdditionTest()
        {
            var n = (Var<V2Int>)"n";
            var T = Predicate("t", n).If(n == V2Int.UnitX + V2Int.UnitY);
            var r = T.Rows.ToArray();
            Assert.AreEqual(1, r.Length);
            Assert.AreEqual(new V2Int(1,1), r[0]);
        }

        [TestMethod]
        public void NegationTest()
        {
            var n = (Var<int>)"n";
            var m = (Var<int>)"m";
            var T = Predicate("t", n).If(m == 1, n == -m);
            var r = T.Rows.ToArray();
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
            var answers = Q.Rows.ToArray();
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
            var answers = Q.Rows.ToArray();
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
            var answers = Q.Rows.ToArray();
            Assert.AreEqual(1, answers.Length);
            Assert.AreEqual(12, answers[0]);
        }
    }
}
