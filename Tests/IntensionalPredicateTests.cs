using TED;

namespace Tests
{
    [TestClass]
    public class IntensionalPredicateTests
    {
        [TestMethod]
        public void SingleSubgoal()
        {
            var t = new TablePredicate<int, int>("t");
            for (var i = 0; i < 10; i++)
                for (var j = 0; j < 10; j++)
                    t.AddRow(i, j);

            var s = new TablePredicate<int>("s");
            var x = (Var<int>)"x";

            s[x].If(t[x,x]);

            var hits = s.Rows.ToArray();
            for (var j = 0; j < 10; j++)
            {
                Assert.AreEqual(j, hits[j]);
            }
        }

        [TestMethod]
        public void DualSubgoal()
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

            s[x].If(t[x,x], u[x]);

            var hits = s.Rows.ToArray();
            Assert.AreEqual(3, hits.Length);
            Assert.AreEqual(2, hits[0]);
            Assert.AreEqual(4, hits[1]);
            Assert.AreEqual(6, hits[2]);
        }

        [TestMethod, ExpectedException(typeof(InstantiationException))]
        public void UninstantiatedRuleTest()
        {
            var n = (Var<int>)"n";
            var p = new TablePredicate<int>("test", n);
            p[n].Fact();
        }
    }
}