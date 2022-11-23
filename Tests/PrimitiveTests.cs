using TED;

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
    }
}
