using System.Runtime.CompilerServices;
using TED;
using TED.Compiler;
using static TED.Language;
using CompilerTests;
using TED.Interpreter;
// ReSharper disable InconsistentNaming

namespace Tests
{
    [TestClass]
    public class CompilerTests
    {
        [TestMethod]
        public void Exhaustive()
        {
            Console.WriteLine("");
            var a = (Var<int>)"a";
            var b = (Var<int>)"b";
            var program = new Simulation(nameof(Exhaustive));
            program.BeginPredicates();
            var P = Predicate<int>("P", new int[] { 1, 2, 3, 4, 5, 6 }, "i");
            var Q = Predicate<int>("Q", new int[] { 2, 4, 6, 8, 10 }, "i");
            var R = Predicate("R", a).If(P[a], Q[a]);
            program.EndPredicates();
            program.Update();
            var interpretedResult = R.ToArray();
            var comp = new Compiler(program, "CompilerTests", CompilerOutputFolder());
            comp.GenerateSource();
            Compiler.Link(program);
            program.Update();
            var compiledResult = R.ToArray();
            CollectionAssert.AreEqual(interpretedResult, compiledResult);
        }

        [TestMethod]
        public void RowSet()
        {
            Console.WriteLine("");
            var a = (Var<int>)"a";
            var b = (Var<int>)"b";
            var program = new Simulation(nameof(RowSet));
            program.BeginPredicates();
            var P = Predicate<int>("P", new int[] { 1, 2, 3, 4, 5, 6 }, "i");
            var Q = Predicate<int>("Q", new int[] { 2, 4, 6, 8, 10 }, "i");
            Q.Unique = true;
            var R = Predicate("R", a).If(P[a], Q[a]);
            program.EndPredicates();
            program.Update();
            var interpretedResult = R.ToArray();
            var comp = new Compiler(program, "CompilerTests", CompilerOutputFolder());
            comp.GenerateSource();
            Compiler.Link(program);
            program.Update();
            var compiledResult = R.ToArray();
            CollectionAssert.AreEqual(interpretedResult, compiledResult);
        }

        [TestMethod]
        public void KeyIndexed()
        {
            var d = (Var<string>)"d";
            var n = (Var<string>)"n";
            var program = new Simulation(nameof(KeyIndexed));
            program.BeginPredicates();
            var Day = Predicate("Day",
                new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" }, d);
            var NextDay = Predicate("NextDay",
                new[]
                {
                    ("Monday", "Tuesday"), ("Tuesday", "Wednesday"), ("Wednesday", "Thursday"), ("Thursday", "Friday"),
                    ("Friday", "Saturday"), ("Saturday", "Sunday"), ("Sunday", "Monday")
                }, d, n);
            NextDay.IndexByKey(d);
            NextDay.IndexBy(n);
            var Mapped = Predicate("Mapped", d, n).If(Day[d], NextDay[d, n]);
            program.EndPredicates();
            program.Update();
            var interpreted = Mapped.ToArray();
            CollectionAssert.AreEqual(NextDay.ToArray(), interpreted);
            new Compiler(program, "CompilerTests", CompilerOutputFolder()).GenerateSource();
            Compiler.Link(program);
            CollectionAssert.AreEqual(NextDay.ToArray(), Mapped.ToArray());
        }

        [TestMethod]
        public void GeneralIndexed()
        {
            var i = (Var<int>)"i";
            var j = (Var<int>)"j";
            var program = new Simulation(nameof(GeneralIndexed));
            program.BeginPredicates();
            var P = Predicate("P", i, j);
            for (int a = 0; a < 10; a++)
            for (int b = 0; b < 20; b += 2)
                P.AddRow(a, b);
            var Q = Predicate("Q", i, j).If(P[i, j], P[j, i]);
            program.EndPredicates();
            program.Update();
            program.Update();
            var interpreted = Q.ToArray();
            new Compiler(program, "CompilerTests", CompilerOutputFolder()).GenerateSource();
            Compiler.Link(program);
            program.Update();
            var compiled = Q.ToArray();
            CollectionAssert.AreEqual(interpreted, compiled);
        }

        string CompilerOutputFolder([CallerFilePath] string caller = null!) => Path.Combine(Path.GetDirectoryName(caller)!, "CompilerOutput");
    }
}