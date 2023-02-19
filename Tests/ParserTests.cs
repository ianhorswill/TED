using TED;
using static TED.Language;
using TED.Repl;

namespace Tests
{
    [TestClass]
    public class ParserTests
    {
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
            var _ = new Simulation("s");
            var n = (Var<int>)"n";
            var m = (Var<int>)"m";
            var o = (Var<int>)"o";
            var a = (Var<int>)"a";
            var str = (Var<string>)"str";
            // ReSharper disable once InconsistentNaming
            var Foo = Predicate("Foo", n, m, o, a, str);

            Goal g = null!;

            Assert.IsTrue(Parser.Goal(new ParserState("Foo[x, y, x, 1, \"String\"]"), new Parser.SymbolTable(),
                (s, goal) =>
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
    }
}