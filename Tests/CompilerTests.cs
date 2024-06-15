using System.Runtime.CompilerServices;
using TED;
using TED.Compiler;
using static TED.Language;
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
            R.ForceRecompute();
            var compiledResult = R.ToArray();
            CollectionAssert.AreEqual(interpretedResult, compiledResult);
        }

        [TestMethod]
        public void RowSet()
        {
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
            R.ForceRecompute();
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
            Mapped.ForceRecompute();
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
            var interpreted = Q.ToArray();
            new Compiler(program, "CompilerTests", CompilerOutputFolder()).GenerateSource();
            Compiler.Link(program);
            Q.ForceRecompute();
            var compiled = Q.ToArray();
            CollectionAssert.AreEqual(interpreted, compiled);
        }

        [TestMethod]
        public void EvalWrite()
        {
            var i = (Var<int>)"i";
            var j = (Var<int>)"j";
            var k = (Var<int>)"k";
            var program = new Simulation(nameof(EvalWrite));
            program.BeginPredicates();
            var P = Predicate("P", i, j);
            for (int a = 0; a < 10; a++)
            for (int b = 0; b < 20; b += 2)
                P.AddRow(a, b);
            var Q = Predicate("Q", k).If(P[i, j], k == i+j);
            program.EndPredicates();
            program.Update();
            var interpreted = Q.ToArray();
            new Compiler(program, "CompilerTests", CompilerOutputFolder()).GenerateSource();
            Compiler.Link(program);
            Q.ForceRecompute();
            var compiled = Q.ToArray();
            CollectionAssert.AreEqual(interpreted, compiled);
        }

        [TestMethod]
        public void EvalRead()
        {
            var i = (Var<int>)"i";
            var j = (Var<int>)"j";
            var k = (Var<int>)"k";
            var program = new Simulation(nameof(EvalRead));
            program.BeginPredicates();
            var P = Predicate("P", i, j);
            for (int a = 0; a < 10; a++)
            for (int b = 0; b < 20; b += 2)
                P.AddRow(a, b);
            var Q = Predicate("Q", i).If(P[i, j], i == j+1);
            program.EndPredicates();
            program.Update();
            var interpreted = Q.ToArray();
            new Compiler(program, "CompilerTests", CompilerOutputFolder()).GenerateSource();
            Compiler.Link(program);
            Q.ForceRecompute();
            var compiled = Q.ToArray();
            CollectionAssert.AreEqual(interpreted, compiled);
        }

        [TestMethod]
        public void Not()
        {
            Console.WriteLine("");
            var a = (Var<int>)"a";
            var b = (Var<int>)"b";
            var program = new Simulation(nameof(Not));
            program.BeginPredicates();
            var P = Predicate<int>("P", new int[] { 1, 2, 3, 4, 5, 6 }, "i");
            var Q = Predicate<int>("Q", new int[] { 2, 4, 6, 8, 10 }, "i");
            var R = Predicate("R", a).If(P[a], !Q[a]);
            program.EndPredicates();
            program.Update();
            var interpretedResult = R.ToArray();
            var comp = new Compiler(program, "CompilerTests", CompilerOutputFolder());
            comp.GenerateSource();
            Compiler.Link(program);
            R.ForceRecompute();
            var compiledResult = R.ToArray();
            CollectionAssert.AreEqual(interpretedResult, compiledResult);
        }

        [TestMethod]
        public void DualRule()
        {
            Console.WriteLine("");
            var a = (Var<int>)"a";
            var b = (Var<int>)"b";
            var program = new Simulation(nameof(DualRule));
            program.BeginPredicates();
            var P = Predicate<int>("P", new int[] { 1, 2, 3, 4, 5, 6 }, "i");
            var Q = Predicate<int>("Q", new int[] { 2, 4, 6, 8, 10 }, "i");
            var R = Predicate("R", a)
                .If(P[a], !Q[a])
                .If(Q[a], !P[a]);
            program.EndPredicates();
            program.Update();
            var interpretedResult = R.ToArray();
            var comp = new Compiler(program, "CompilerTests", CompilerOutputFolder());
            comp.GenerateSource();
            Compiler.Link(program);
            R.ForceRecompute();
            var compiledResult = R.ToArray();
            CollectionAssert.AreEqual(interpretedResult, compiledResult);
        }

        [TestMethod]
        public void LessThan()
        {
            var i = (Var<int>)"i";
            var j = (Var<int>)"j";
            var k = (Var<int>)"k";
            var program = new Simulation(nameof(LessThan));
            program.BeginPredicates();
            var P = Predicate("P", i, j);
            for (int a = 0; a < 10; a++)
            for (int b = 0; b < 20; b += 2)
                P.AddRow(a, b);
            var Q = Predicate("Q", i).If(P[i, j], i < j);
            program.EndPredicates();
            program.Update();
            var interpreted = Q.ToArray();
            new Compiler(program, "CompilerTests", CompilerOutputFolder()).GenerateSource();
            Compiler.Link(program);
            Q.ForceRecompute();
            var compiled = Q.ToArray();
            CollectionAssert.AreEqual(interpreted, compiled);
        }

        public static bool Odd(int n) => n % 2 == 1;

        [TestMethod]
        public void PrimitiveTestTest()
        {
            var a = (Var<int>)"a";
            var b = (Var<int>)"b";
            var OddP = TestMethod((Func<int, bool>)Odd);
            var program = new Simulation(nameof(PrimitiveTestTest));
            program.BeginPredicates();
            var P = Predicate<int>("P", new int[] { 1, 2, 3, 4, 5, 6 }, "i");
            var Q = Predicate("Q", a).If(P[a], OddP[a]);
            program.EndPredicates();
            var interpreted = Q.ToArray();
            new Compiler(program, "CompilerTests", CompilerOutputFolder()).GenerateSource();
            Compiler.Link(program);
            Q.ForceRecompute();
            var compiled = Q.ToArray();
            CollectionAssert.AreEqual(interpreted, compiled);
        }

        [TestMethod]
        public void FirstOfTest()
        {
            var a = (Var<int>)"a";
            var b = (Var<string>)"b";
            var OddP = TestMethod((Func<int, bool>)Odd);
            var program = new Simulation(nameof(FirstOfTest));
            program.BeginPredicates();
            var P = Predicate<int>("P", new int[] { 1, 2, 3, 4, 5, 6 }, "i");
            var Q = Predicate("Q", b).If(P[a], FirstOf[And[OddP[a], b=="odd"], b=="even"]);
            program.EndPredicates();
            var interpreted = Q.ToArray();
            CollectionAssert.AreEqual(new [] { "odd", "even", "odd", "even", "odd", "even" }, interpreted);
            new Compiler(program, "CompilerTests", CompilerOutputFolder()).GenerateSource();
            Compiler.Link(program);
            Q.ForceRecompute();
            var compiled = Q.ToArray();
            CollectionAssert.AreEqual(interpreted, compiled);
        }

        [TestMethod]
        public void InGenerateModeTest()
        {
            var c = new[] { 1, 2, 3, 4, 5 };
            var n = (Var<int>)"n";
            var program = new Program(nameof(InGenerateModeTest));
            program.BeginPredicates();
            var Test = Predicate("Test", n);
            Test[n].If(In<int>(n, c));  // Should just copy c into Test
            program.EndPredicates();
            var interpreted = Test.ToArray();
            CollectionAssert.AreEqual(c, interpreted);
            new Compiler(program, "CompilerTests", CompilerOutputFolder()).GenerateSource();
            Compiler.Link(program);
            Test.ForceRecompute();
            var compiled = Test.ToArray();
            CollectionAssert.AreEqual(interpreted, compiled);
        }

        [TestMethod]
        public void InTestModeTest()
        {
            var c = new[] { 1, 2, 3, 4, 5 };
            var n = (Var<int>)"n";
            var program = new Program(nameof(InTestModeTest));
            program.BeginPredicates();
            var Test = Predicate("Test", n);
            Test[0].If(In<int>(0, c));
            Test[1].If(In<int>(4, c));
            program.EndPredicates();
            var interpreted = Test.ToArray();
            CollectionAssert.AreEqual(new[]{1}, interpreted);
            new Compiler(program, "CompilerTests", CompilerOutputFolder()).GenerateSource();
            Compiler.Link(program);
            Test.ForceRecompute();
            var compiled = Test.ToArray();
            CollectionAssert.AreEqual(interpreted, compiled);
        }

        [TestMethod]
        public void OnceTest()
        {
            var program = new Program(nameof(OnceTest));
            program.BeginPredicates();
            var p = Predicate("p", new[] { 1, 2, 3 });
            var x = (Var<int>)"x";
            var q = Predicate("q", x).If(Once[p[x]]);
            program.EndPredicates();
            var interpreted = q.ToArray();
            CollectionAssert.AreEqual(new[] {1 }, interpreted);
            new Compiler(program, "CompilerTests", CompilerOutputFolder()).GenerateSource();
            Compiler.Link(program);
            q.ForceRecompute();
            var compiled = q.ToArray();
            CollectionAssert.AreEqual(interpreted, compiled);
        }

        string CompilerOutputFolder([CallerFilePath] string caller = null!) => Path.Combine(Path.GetDirectoryName(caller)!, "CompilerOutput");
    }
}