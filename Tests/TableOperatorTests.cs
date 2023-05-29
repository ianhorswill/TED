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
            var desire = (Var<float>)"desire";
            var candidates = Predicate("candidates", f.Indexed, m, desire);
            candidates.Unique = true;
            var assignments = AssignGreedily("assignments", candidates);
            candidates.AddRow("jenny", "bill", 1);
            candidates.AddRow("jenny", "george",2);
            candidates.AddRow("jenny", "chris",0);
            candidates.AddRow("sally", "bill",2);
            candidates.AddRow("sally", "george",3);
            candidates.AddRow("sally", "pat",2);
            var a = assignments.ToArray();
            Assert.AreEqual(2, assignments.Count());
            foreach (var (fa,ma) in assignments)
                switch (fa)
                {
                    case "jenny":
                        Assert.AreEqual("bill", ma);
                        break;
                    case "sally":
                        Assert.AreEqual("george", ma);
                        break;
                    default:
                        Assert.Fail($"Unknown assignee {fa}");
                        break;
                }
        }
    }
}