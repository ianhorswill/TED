using TED;
using TED.Interpreter;
using static TED.Language;

namespace Tests
{
    [TestClass]
    public class IntensionalPredicateTests
    {
        [TestMethod]
        public void DynamicCreation()
        {
            Assert.IsInstanceOfType(TablePredicate.Create("test", new IVariable[]{ (Var<int>)"x", (Var<string>)"y"}),
                typeof(TablePredicate<int,string>));
        }

        [TestMethod]
        public void SingleSubgoal()
        {
            var x = (Var<int>)"x";
            var y = (Var<int>)"y";

            var t = new TablePredicate<int, int>("t", x, y);
            for (var i = 0; i < 10; i++)
            for (var j = 0; j < 10; j++)
                t.AddRow(i, j);

            var s = new TablePredicate<int>("s", x);


            s[x].If(t[x, x]);

            var hits = s.ToArray();
            for (var j = 0; j < 10; j++)
            {
                Assert.AreEqual(j, hits[j]);
            }
        }

        [TestMethod]
        public void DualSubgoal()
        {
            var x = (Var<int>)"x";
            var y = (Var<int>)"y";

            var t = new TablePredicate<int, int>("t", x, y);
            for (var i = 0; i < 10; i++)
            for (var j = 0; j < 10; j++)
                t.AddRow(i, j);

            var u = new TablePredicate<int>("u", x);
            u.AddRow(2);
            u.AddRow(4);
            u.AddRow(6);

            var s = new TablePredicate<int>("s", x);

            s[x].If(t[x, x], u[x]);

            var hits = s.ToArray();
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

        [TestMethod]
        public void KeyIndexedCallTest()
        {
            var d = (Var<string>)"d";
            var n = (Var<string>)"n";
            var Day = Predicate("Day",
                new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" }, d);
            var NextDay = Predicate("NextDay",
                new[]
                {
                    ("Monday", "Tuesday"), ("Tuesday", "Wednesday"), ("Wednesday", "Thursday"), ("Thursday", "Friday"),
                    ("Friday", "Saturday"), ("Saturday", "Sunday"), ("Sunday", "Monday")
                }, d, n);
            NextDay.IndexByKey(d);
            var Mapped = Predicate("Mapped", d, n).If(Day[d], NextDay[d, n]);
            var rule = Mapped.Rules![0];
            Assert.IsInstanceOfType(rule.Body[1], typeof(TableCallWithKey<string, string, string>));
            CollectionAssert.AreEqual(NextDay.ToArray(), Mapped.ToArray());
        }

        [TestMethod]
        public void KeyIndexedCallFailureTest()
        {
            // Like KeyIndexCallTest, but forces some of the key lookups to fail to make sure that doesn't crash
            var d = (Var<string>)"d";
            var n = (Var<string>)"n";
            var Day = Predicate("Day",
                new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" }, d);
            var NextDay = Predicate("NextDay",
                new[] { ("Monday", "Tuesday"), ("Wednesday", "Thursday"), ("Friday", "Saturday") }, d, n);
            NextDay.IndexByKey(d);
            var Mapped = Predicate("Mapped", d, n).If(Day[d], NextDay[d, n]);
            var rule = Mapped.Rules![0];
            Assert.IsInstanceOfType(rule.Body[1], typeof(TableCallWithKey<string, string, string>));
            CollectionAssert.AreEqual(
                new[] { ("Monday", "Tuesday"), ("Wednesday", "Thursday"), ("Friday", "Saturday") },
                Mapped.ToArray());
        }

        [TestMethod, ExpectedException(typeof(RuleExecutionException))]
        public void RuleExecutionExceptionTest()
        {
            var x = (Var<int>)"x";
            var y = (Var<int>)"y";

            var Number = Predicate("Number", new [] { 1, 2, 3, 0, 4, 5, 6 }, x);
            var Reciprocal = Predicate("Reciprocal", y).If(Number[x], y == 100 / x);
            Console.WriteLine(Reciprocal.ToArray());
        }
    }
}