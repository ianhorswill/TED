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
    }
}
