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

        [TestMethod]
        public void AssignGreedilyTest()
        {
            var f = (Var<string>)"f";
            var m = (Var<string>)"m";
            var desire = (Var<float>)"desire";
            var candidates = Predicate("candidates", f.Indexed, m, desire);
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

        [TestMethod]
        public void AssignGreedilyWithCapacities()
        {
            var p = (Var<string>)"p";
            var j = (Var<string>)"j";
            var c = (Var<int>)"c";
            var desire = (Var<float>)"desire";
            var candidates = Predicate("candidates", p.Indexed, j, desire);
            candidates.AddRow("jenny", "lawyer", 1);
            candidates.AddRow("jenny", "janitor",2);
            candidates.AddRow("jenny", "judge",0);
            candidates.AddRow("jenny", "dancer",10);
            candidates.AddRow("sally", "lawyer",2);
            candidates.AddRow("sally", "doctor",3);
            candidates.AddRow("sally", "teacher",2);
            candidates.AddRow("sally", "dancer",10);
            candidates.AddRow("bill", "teacher",2);
            candidates.AddRow("bill", "lawyer",1);
            candidates.AddRow("bill", "teacher",2);
            candidates.AddRow("bill", "doctor",0);
            candidates.AddRow("bill", "dancer",100);
            candidates.AddRow("george", "teacher",1);
            candidates.AddRow("george", "janitor",3);
            candidates.AddRow("george", "dancer",10);

            var capacities = Predicate("capacities", p, c);
            capacities.AddRow("lawyer",2);
            capacities.AddRow("doctor",2);
            capacities.AddRow("judge",1);
            capacities.AddRow("janitor",3);
            capacities.AddRow("teacher",3);
            capacities.AddRow("dancer",1);

            var assignments = AssignGreedily("assignments", candidates, capacities);

            var a = assignments.ToArray();
            Assert.AreEqual(4, assignments.Count());
            foreach (var (fa,ma) in assignments)
                switch (fa)
                {
                    case "jenny":
                        Assert.AreEqual("janitor", ma);
                        break;
                    case "sally":
                        Assert.AreEqual("doctor", ma);
                        break;
                    case "george":
                        Assert.AreEqual("janitor", ma);
                        break;
                    case "bill":
                        Assert.AreEqual("dancer", ma);
                        break;
                    default:
                        Assert.Fail($"Unknown assignee {fa}");
                        break;
                }
        }

        [TestMethod]
        public void MatchGreedilyTest()
        {
            var f = (Var<string>)"f";
            var m = (Var<string>)"m";
            var desire = (Var<float>)"desire";
            var candidates = Predicate("candidates", f.Indexed, m, desire);
            var assignments = MatchGreedily("assignments", candidates);
            candidates.AddRow("jenny", "bill", 1);
            candidates.AddRow("jenny", "george", 2);
            candidates.AddRow("jenny", "chris", 0);
            candidates.AddRow("jenny", "sally", 1);
            candidates.AddRow("sally", "bill", 2);
            candidates.AddRow("sally", "george", 3);
            candidates.AddRow("sally", "pat", 2);
            candidates.AddRow("bill", "pat", 4);
            var a = assignments.ToArray();
            Assert.AreEqual(3, assignments.Count());
            foreach (var (fa, ma) in assignments)
                switch (fa)
                {
                    case "jenny":
                        Assert.AreEqual("chris", ma);
                        break;
                    case "sally":
                        Assert.AreEqual("george", ma);
                        break;
                    case "bill":
                        Assert.AreEqual("pat", ma);
                        break;
                    default:
                        Assert.Fail($"Unexpected assignee {fa}");
                        break;
                }
        }

        [TestMethod]
        public void TransitiveClosure()
        {
            var sub = (Var<string>)"sub";
            var parent = (Var<string>)"parent";
            var subtypes = Predicate("subtypes", new (string,string)[]
            {
                ("cat", "mammal"),
                ("dog", "mammal"),
                ("mammal", "animal"),
                ("bird","animal")
            }, sub.Indexed, parent);

            var isa = Closure("isa", subtypes, false);
            Assert.AreEqual(6u, isa.Length);
            foreach (var pair in subtypes)
                Assert.IsTrue(isa.Table.ContainsRowUsingRowSet(pair));

            Assert.IsTrue(isa.Table.ContainsRowUsingRowSet(("cat", "animal")));
            Assert.IsTrue(isa.Table.ContainsRowUsingRowSet(("dog", "animal")));
        }
    }
}