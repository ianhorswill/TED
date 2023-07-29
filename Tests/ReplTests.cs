using TED;
using TED.Interpreter;
using TED.Repl;

namespace Tests
{
    [TestClass]
    public class ReplTests
    {
        [TestMethod]
        public void QueryTest()
        {
            var p = new Program(nameof(QueryTest));
            p.BeginPredicates();
            TablePredicate<string, int>.FromCsv("test", "../../../TestTable.csv", (Var<string>)"name", (Var<int>)"age");
            p.EndPredicates();

            var q = p.Repl.Query("TestQuery", "test[\"Tamika\", age]");
            Assert.IsInstanceOfType(q,typeof(TablePredicate<int>));
            Assert.AreEqual(1u, q.Length);
            CollectionAssert.AreEqual(new[] { 11 }, ((IEnumerable<int>)q).ToArray());
        }

        [TestMethod]
        public void PredicateFieldTest()
        {
            var p = new Program(nameof(QueryTest));
            p.BeginPredicates();
            var age = (Var<int>)"age";
            var name = (Var<string>)"name";

            var ti = TablePredicate<string, int>.FromCsv("testInit", "../../../TestTable.csv", name, age);
            var test = TED.Language.Predicate("test", name,age);
            test.Initially.If(ti);
            p.EndPredicates();

            var q = p.Repl.Query("TestQuery", "test.Initially[\"Tamika\", age]");
            Assert.IsInstanceOfType(q,typeof(TablePredicate<int>));
            Assert.AreEqual(1u, q.Length);
            CollectionAssert.AreEqual(new[] { 11 }, ((IEnumerable<int>)q).ToArray());
        }

        [TestMethod]
        public void FullToken()
        {
            string result = null!;
            Assert.IsTrue(new ParserState("Foo").ReadToken(char.IsAscii, (_, str) =>
            {
                result = str;
                return true;
            }));
            Assert.AreEqual("Foo", result);
        }

        [TestMethod]
        public void PartialToken()
        {
            string result = null!;
            Assert.IsTrue(new ParserState("Foo  ").ReadToken(char.IsLetter, (_, str) =>
            {
                result = str;
                return true;
            }));
            Assert.AreEqual("Foo", result);
        }

        [TestMethod]
        public void TokenSequence()
        {
            IList<string> result = null!;

            bool ReadToken(ParserState s, ParserState.Continuation<string> k)
                => s.ReadToken(char.IsLetter, (s2, str) => s2.SkipWhitespace(s3 => k(s3, str)));

            Assert.IsTrue(new ParserState("Foo Bar Baz ").Star<string>(ReadToken, (s, strList) =>
            {
                result = strList;
                return s.End;
            }));

            CollectionAssert.AreEqual(new[] {"Foo", "Bar", "Baz"}, result!.ToArray());
        }

        [TestMethod]
        public void DelimitedTokenSequence()
        {
            IList<string> result = null!;

            Assert.IsTrue(new ParserState("Foo, Bar ,Baz").DelimitedList<string>(Parser.Identifier, ",", (s, strList) =>
            {
                result = strList;
                return s.End;
            }));

            CollectionAssert.AreEqual(new[] {"Foo", "Bar", "Baz"}, result!.ToArray());
        }

        [TestMethod]
        public void Goal()
        {
            var sim = new Simulation("s");
            var n = (Var<int>)"n";
            var m = (Var<int>)"m";
            var o = (Var<int>)"o";
            var a = (Var<int>)"a";
            var str = (Var<string>)"str";
            sim.BeginPredicates();
            // ReSharper disable once InconsistentNaming
            var Foo = TED.Language.Predicate("Foo", n, m, o, a, str);
            sim.EndPredicates();

            Goal g = null!;

            Assert.IsTrue(sim.Repl.Parser.Goal(new ParserState("Foo[x, y, x, 1, \"String\"]"), (s, goal) =>
                {
                    g = goal;
                    return s.End;
                }));

            Assert.AreEqual(Foo, g.Predicate);
            Assert.AreEqual(5, g.Arguments.Length);

            var t = (Var<int>)g.Arguments[0];
            Assert.AreEqual("x", t.Name);

            t = (Var<int>)g.Arguments[1];
            Assert.AreEqual("y", t.Name);

            Assert.AreEqual(g.Arguments[0], g.Arguments[2]);

            var t4 = (Constant<int>)g.Arguments[3];
            Assert.AreEqual(1, t4.Value);

            var t5 = (Constant<string>)g.Arguments[4];
            Assert.AreEqual("String", t5.Value);
        }

        [TestMethod]
        public void DualGoal()
        {
            var p = new Program(nameof(DualGoal));
            p.BeginPredicates();
            var i = (Var<int>)"i";
            var j = (Var<int>)"j";
            var T = TED.Language.Predicate("T", i, j);
            p.EndPredicates();
            for (var k = 0; k < 5; k++)
                T.AddRow(k, 1);
            var q = (TablePredicate<int,int>)p.Repl.Query("q", "T[a,b], T[b, a]");
            Assert.AreEqual(1u,q.Length);
            Assert.AreEqual((1,1), q.ToArray()[0]);
        }
    }
}