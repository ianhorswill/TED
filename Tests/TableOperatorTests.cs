using TED;
using TED.Interpreter;
using static TED.Language;
using InvalidProgramException = TED.InvalidProgramException;

namespace Tests
{
    [TestClass]
    public class TableOperatorTests
    {
        [TestMethod]
        public void CountsByTest()
        {
            var s = (Var<string>)"s";
            var c = (Var<int>)"c";
            var b = Predicate("b", s.Indexed, c);
            foreach (var str in new [] { "x", "xx", "xxx" })
                for (var i = 0; i < str.Length; i++)
                    b.AddRow(str,0);
            var counts = CountsBy("counts", b, s, c);
            counts.EnsureUpToDate();
            var a = counts.ToArray();
            Assert.AreEqual(3, a.Length);
            foreach (var (str, cnt) in a)
                Assert.AreEqual(cnt, str.Length);
        }

        [TestMethod]
        public void AssignRandomlyTest()
        {
            var f = (Var<string>)"f";
            var m = (Var<string>)"m";
            var candidates = Predicate("candidates", f.Indexed, m);
            candidates.Unique = true;
            var assignments = AssignRandomly("assignments", candidates);
            candidates.AddRow("jenny", "bill");
            candidates.AddRow("jenny", "george");
            candidates.AddRow("jenny", "chris");
            candidates.AddRow("sally", "bill");
            candidates.AddRow("sally", "george");
            candidates.AddRow("sally", "pat");
            Assert.AreEqual(2, assignments.Count());
            foreach (var assignment in assignments)
                Assert.IsTrue(candidates.Table.ContainsRowUsingRowSet(assignment));
        }
    }
}