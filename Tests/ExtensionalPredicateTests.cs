using System.Reflection;
using TED;
using static TED.Language;

namespace Tests
{
    [TestClass]
    public class ExtensionalPredicateTests
    {
        [TestMethod]
        public void MatchConstant()
        {
            var t = new TablePredicate<int>("t");
            var target = 49;
            for (var i = 0; i < 100; i++)
                t.AddRow(i);
            var p = new Pattern<int>(MatchOperation<int>.Constant(target));

            var gotOne = false;
            foreach (var hit in t.Match(p))
            {
                Assert.AreEqual(target, hit);
                Assert.IsFalse(gotOne, "Got multiple hits, when there should only have been one");
                gotOne = true;
            }

            Assert.IsTrue(gotOne);
        }

        [TestMethod]
        public void MatchWrite()
        {
            var t = new TablePredicate<int>("t");
            for (var i = 0; i < 100; i++)
                t.AddRow(i);

            var c = ValueCell<int>.MakeVariable("c");
            var p = new Pattern<int>(MatchOperation<int>.Write(c));

            var j = 0;
            foreach (var hit in t.Match(p))
            {
                Assert.AreEqual(j, hit);
                Assert.AreEqual(j, c.Value);
                j++;
            }
            Assert.AreEqual(100,j);
        }

        [TestMethod]
        public void MatchRead()
        {
            var t = new TablePredicate<int>("t");
            for (var i = 0; i < 100; i++)
                t.AddRow(i);

            var c = ValueCell<int>.MakeVariable("c");
            var p = new Pattern<int>(MatchOperation<int>.Read(c));

            for (var j = 0; j < 100; j++)
            {
                c.Value = j;
                var hits = t.Match(p).ToArray();
                Assert.AreEqual(1, hits.Length);
                Assert.AreEqual(j, hits[0]);
            }
        }

        [TestMethod]
        public void MatchWriteRead()
        {
            var t = new TablePredicate<int, int>("t");
            for (var i = 0; i < 10; i++)
                for (var j = 0; j < 10; j++)
                    t.AddRow(i, j);

            var c = ValueCell<int>.MakeVariable("c");
            var p = new Pattern<int,int>(MatchOperation<int>.Write(c), MatchOperation<int>.Read(c));

            var hits = t.Match(p).ToArray();
            Assert.AreEqual(10, hits.Length);

            for (var j = 0; j < 10; j++)
            {
                Assert.AreEqual((j,j), hits[j]);
            }
        }

        [TestMethod]
        public void CsvReaderTest()
        {
            var t = TablePredicate<string, int>.FromCsv("test", "../../../TestTable.csv");
            Assert.AreEqual("test", t.Name);
            Assert.AreEqual("Name", t.ColumnHeadings[0]);
            Assert.AreEqual("Age", t.ColumnHeadings[1]);
            var rows = t.Rows.ToArray();
            Assert.AreEqual(4, rows.Length);
            Assert.AreEqual("Fred", rows[0].Item1);
            Assert.AreEqual(10, rows[0].Item2);
            Assert.AreEqual("Elroy", rows[3].Item1);
            Assert.AreEqual(9, rows[3].Item2);
        }

        [TestMethod]
        public void GeneratorTest1()
        {
            var types = Predicate("types", typeof(ExtensionalPredicateTests).Assembly.DefinedTypes);

            Assert.IsTrue(types.Rows.Contains(typeof(ExtensionalPredicateTests)));
            Assert.IsFalse(types.Rows.Contains(typeof(int)));
        }

        [TestMethod]
        public void GeneratorTest2()
        {
            var t = (Var<TypeInfo>)"t";
            var m = (Var<MethodInfo>)"m";
            var rt = (Var<Type>)"rt";
            var Methods = Predicate("methods",
                typeof(AnyTerm).Assembly.DefinedTypes.SelectMany(t => t.GetMethods().Select(m => (t, m, m.ReturnType))),
                t, m, rt);

            var HasBoolMethod = Predicate("HasBoolMethods", t).If(Methods[t, m, typeof(bool)]);
            Assert.IsTrue(HasBoolMethod.Length > 0); ;
        }
    }
}