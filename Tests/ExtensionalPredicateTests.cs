using System.Reflection;
using TED;
using TED.Interpreter;
using TED.Utilities;
using static TED.Language;
// ReSharper disable InconsistentNaming

namespace Tests
{
    [TestClass]
    public class ExtensionalPredicateTests
    {
        [TestMethod]
        public void MatchConstant()
        {
            var t = new TablePredicate<int>("t", (Var<int>)"x");
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
            var t = new TablePredicate<int>("t", (Var<int>)"x");
            for (var i = 0; i < 100; i++)
                t.AddRow(i);

            var c = ValueCell<int>.MakeVariable(null!);
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
            var t = new TablePredicate<int>("t", (Var<int>)"x");
            for (var i = 0; i < 100; i++)
                t.AddRow(i);

            var c = ValueCell<int>.MakeVariable(null!);
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
            var t = new TablePredicate<int, int>("t", (Var<int>)"x", (Var<int>)"y");
            for (var i = 0; i < 10; i++)
                for (var j = 0; j < 10; j++)
                    t.AddRow(i, j);

            var c = ValueCell<int>.MakeVariable(null!);
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
            var t = TablePredicate<string, int>.FromCsv("test", "../../../TestTable.csv", (Var<string>)"name", (Var<int>)"age");
            Assert.AreEqual("test", t.Name);
            Assert.AreEqual("name", t.ColumnHeadings[0]);
            Assert.AreEqual("age", t.ColumnHeadings[1]);
            var rows = t.ToArray();
            Assert.AreEqual(4, rows.Length);
            Assert.AreEqual("Fred", rows[0].Item1);
            Assert.AreEqual(10, rows[0].Item2);
            Assert.AreEqual("Elroy", rows[3].Item1);
            Assert.AreEqual(9, rows[3].Item2);

            CsvReader.DeclareParser(typeof(byte), s =>byte.Parse(s));
            Assert.AreEqual(42, CsvReader.ConvertCell<byte>("42"));
        }

        [TestMethod]
        public void GeneratorTest1()
        {
            var types = Predicate("types", typeof(ExtensionalPredicateTests).Assembly.DefinedTypes);

            Assert.IsTrue(types.Contains(typeof(ExtensionalPredicateTests)));
            Assert.IsFalse(types.Contains(typeof(int)));
        }

        [TestMethod]
        public void GeneratorTest2()
        {
            var t = (Var<TypeInfo>)"t";
            var m = (Var<MethodInfo>)"m";
            var rt = (Var<Type>)"rt";
            var Methods = Predicate("methods",
                typeof(Term).Assembly.DefinedTypes.SelectMany(t2 => t2.GetMethods().Select(m2 => (t: t2, m: m2, m2.ReturnType))),
                t, m, rt);

            var HasBoolMethod = Predicate("HasBoolMethods", t).If(Methods[t, m, typeof(bool)]);
            Assert.IsTrue(HasBoolMethod.Length > 0);
        }

        [TestMethod]
        public void Set()
        {
            var name = (Var<string>)"name";
            var age = (Var<int>)"age";
            var older = (Var<int>)"older";
            var s = new Simulation(nameof(Set));
            s.BeginPredicates();
            var T = Language.Predicate("T", name.Key, age);
            T.Set(name, age, older).If(T[name, age],older == age+1);
            s.EndPredicates();
            T.AddRow("Jane",1);
            T.AddRow("Bruce", 2);
            s.Update();
            CollectionAssert.AreEqual(new [] { ("Jane", 2), ("Bruce", 3) }, T.ToArray());
        }

        [TestMethod]
        public void SetDualKey()
        {
            var firstName = (Var<string>)"firstName";
            var lastName = (Var<string>)"lastName";
            var age = (Var<int>)"age";
            var older = (Var<int>)"older";
            var s = new Simulation(nameof(Set));
            s.BeginPredicates();
            var T = Language.Predicate("T", firstName, lastName, age);
            T.IndexByKey(firstName, lastName);
            T.Set((firstName, lastName), age, older).If(T[firstName, lastName, age],older == age+1);
            s.EndPredicates();
            T.AddRow("Jane", "Byrne", 1);
            T.AddRow("Bruce", "Byrne", 2);
            s.Update();
            CollectionAssert.AreEqual(new [] { ("Jane", "Byrne", 2), ("Bruce", "Byrne", 3) }, T.ToArray());
        }

        [TestMethod]
        public void OverwriteModeTest()
        {
            var i = (Var<int>)"i";
            var j = (Var<int>)"j";
            var input = new TablePredicate<int, int>("input", i, j);
            for (int k = 0; k < 100; k++)
                input.AddRow(k % 10, k);
            var p = new TablePredicate<int, int>("p", i.Key, j) { Overwrite = true };
            p.Append(input);
            Assert.AreEqual(10u, p.Length);
            var counter = 90;
            foreach (var row in p)
            {
                Assert.AreEqual(counter%10, row.Item1);
                Assert.AreEqual(counter, row.Item2);
                counter++;
            }
        }

        [TestMethod]
        public void OverwriteModeIndexedTest()
        {
            var i = (Var<int>)"i";
            var j = (Var<int>)"j";
            var input = new TablePredicate<int, int>("input", i, j);
            for (int k = 0; k < 100; k++)
                input.AddRow(k % 10, k);
            var p = new TablePredicate<int, int>("p", i.Key, j.Indexed) { Overwrite = true };
            p.Append(input);
            Assert.AreEqual(10u, p.Length);
            var counter = 90;
            foreach (var row in p)
            {
                Assert.AreEqual(counter%10, row.Item1);
                Assert.AreEqual(counter, row.Item2);
                counter++;
            }
        }
    }
}