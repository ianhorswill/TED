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
            var T = TPredicate("T", n).If(Set(n, Negative[1]));
            var r = T.Rows.ToArray();
            Assert.AreEqual(1, r.Length);
            Assert.AreEqual(-1, r[0]);
        }

        [TestMethod]
        public void EqualityTest()
        {
            var Negative = Function<int,int>("Negative", n => -n);
            var n = (Var<int>)"n";
            var T = TPredicate("T", n).If(n == Negative[1]);
            var r = T.Rows.ToArray();
            Assert.AreEqual(1, r.Length);
            Assert.AreEqual(-1, r[0]);
        }

        [TestMethod]
        public void AdditionTest()
        {
            var n = (Var<int>)"n";
            var T = TPredicate("t", n).If(n == 1 + 2);
            var r = T.Rows.ToArray();
            Assert.AreEqual(1, r.Length);
            Assert.AreEqual(3, r[0]);
        }

        
        [TestMethod]
        public void NegationTest()
        {
            var n = (Var<int>)"n";
            var m = (Var<int>)"m";
            var T = TPredicate("t", n).If(m == 1, n == -m);
            var r = T.Rows.ToArray();
            Assert.AreEqual(1, r.Length);
            Assert.AreEqual(-1, r[0]);
        }
    }
}
