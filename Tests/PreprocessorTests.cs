using TED;
using static TED.Language;
// ReSharper disable InconsistentNaming

namespace Tests
{
    [TestClass]
    public class PreprocessorTests
    {
        [TestMethod]
        public void FunctionalExpressionsTest()
        {
            var n = (Var<int>)"n";
            var m = (Var<int>)"m";
            var P = Predicate("P", n, m);
            var sum = Constant(1) + Constant(2);
            var g = P[1, sum];
            CollectionAssert.AreEqual(new[] { sum }, Preprocessor.FunctionalExpressions(g).ToArray());
        }

        [TestMethod]
        public void HoistGoal()
        {
            var n = (Var<int>)"n";
            var m = (Var<int>)"m";
            var P = Predicate("P", n, m);
            var sum = Constant(1) + Constant(2);
            var g = P[1, sum];
            var hoisted = Preprocessor.HoistFunctionalExpressions(g).ToArray();
            Assert.AreEqual(2, hoisted.Length);
            var match = hoisted[0];
            Assert.IsInstanceOfType(match.Predicate, typeof(MatchPrimitive<int>));
            var v = (Var<int>)match.Arguments[0];
            Assert.AreEqual(sum, match.Arguments[1]);
            var transformed = hoisted[1];
            Assert.AreEqual(g.Predicate, transformed.Predicate);
            Assert.AreEqual(g.Arguments[0], transformed.Arguments[0]);
            Assert.AreEqual(v, transformed.Arguments[1]);

            // Check that something with no functional expressions doesn't get rehoisted.
            var rehoisted = Preprocessor.HoistFunctionalExpressions(transformed).ToArray();
            Assert.AreEqual(1, rehoisted.Length);
            Assert.AreEqual(transformed, rehoisted[0]);

            // Check that Match expressions don't get hoisted
            rehoisted = Preprocessor.HoistFunctionalExpressions(match).ToArray();
            Assert.AreEqual(1, rehoisted.Length);
            Assert.AreEqual(match, rehoisted[0]);
        }

        [TestMethod]
        public void HoistRule()
        {
            var d = (Var<string>)"d";
            var next = (Var<string>)"next";
            var i = (Var<int>)"i";
            var DayNumber = Predicate("DayNumber",
                new[] { ("Monday", 0), ("Tuesday", 1), ("Wednesday",2), ("Thursday", 3), ("Friday", 4), ("Saturday", 5), ("Sunday", 6) },
                d, i);
            // This should have no effect whatsoever, but exercising the indexing system a little more
            DayNumber.IndexByKey(d);
            // This isn't really necessary, but again, exercising the indexing system
            DayNumber.IndexByKey(i);

            var NextDay = Predicate("NextDay", d, next).If(DayNumber[d, i], DayNumber[next, (i + 1) % 7]);

            var rightAnswer = new[]
            {
                ("Monday", "Tuesday"), ("Tuesday", "Wednesday"), ("Wednesday", "Thursday"), ("Thursday", "Friday"),
                ("Friday", "Saturday"), ("Saturday", "Sunday"), ("Sunday", "Monday")
            };
            CollectionAssert.AreEqual(rightAnswer, NextDay.Rows.ToArray());
        }
    }
}