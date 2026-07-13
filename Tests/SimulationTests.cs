using TED;
using static TED.Language;
// ReSharper disable InconsistentNaming

namespace Tests
{
    [TestClass]
    public class SimulationTests
    {
        [TestMethod]
        public void Add()
        {
            var counter = 0;
            var NextCounter = Function("NextCounter", () => counter++);

            var n = (Var<int>)"n";

            var s = new Simulation(nameof(Add));
            s.BeginPredicates();
            var BaseTable = Predicate("BaseTable", n);
            BaseTable.Add[n].If(n == NextCounter);
            s.EndPredicates();

            s.Update();
            s.Update();
            s.Update();
            CollectionAssert.AreEqual(new [] { 0, 1, 2}, BaseTable.ToArray());
        }

        [TestMethod]
        public void AddLots()
        {
            var counter = 0;
            var NextCounter = Function("NextCounter", () => counter++);

            var n = (Var<int>)"n";

            var s = new Simulation(nameof(AddLots));
            s.BeginPredicates();
            var Static = Predicate("Static", n);
            for (var i = 0; i < 100; i++)
            {    
                Static[i].Fact();
            }
            var BaseTable = Predicate("BaseTable", n);
            BaseTable.Add[n].If(Static[n]);
            s.EndPredicates();

            s.Update();
            s.Update();

            var expected = Enumerable.Range(0, 100).Concat(Enumerable.Range(0, 100)).ToArray();
            var actual = BaseTable.ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AddUnique()
        {
            var counter = 0;
            var NextCounter = Function("NextCounter", () => counter++ % 3);

            var n = (Var<int>)"n";

            var s = new Simulation(nameof(AddUnique));
            s.BeginPredicates();
            var BaseTable = Predicate("BaseTable", n).UniqueRows();
            BaseTable.Add[n].If(n == NextCounter);
            s.EndPredicates();

            for (var i = 0; i < 10; i++) s.Update();
            var collection = BaseTable.ToArray();
            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, collection);
        }

        [TestMethod]
        public void AddOverwrite()
        {
            var smallCounter = 0;
            var SmallCounter = Function("SmallCounter", () => smallCounter++ % 3);
            var bigCounter = 0;
            var BigCounter = Function("BigCounter", () => bigCounter++);

            var n = (Var<int>)"n";
            var m = (Var<int>)"m";

            var s = new Simulation(nameof(AddUnique));
            s.BeginPredicates();
            var BaseTable = Predicate("BaseTable", n.Key, m);
            BaseTable.Overwrite = true;
            BaseTable.Add.If(n == SmallCounter, m == BigCounter);
            s.EndPredicates();

            for (var i = 0; i < 10; i++) s.Update();
            var collection = BaseTable.ToArray();
            CollectionAssert.AreEqual(new[] { (0,9), (1,7), (2,8) }, collection);
        }

        [TestMethod]
        public void InitializationOnly()
        {
            var s = new Simulation(nameof(InitializationOnly));
            var n = (Var<int>)"n";
            s.BeginPredicates();
            var BaseTable = Predicate("BaseTable", n);
            BaseTable.Initially[n].If(n == 1);
            s.EndPredicates();
            Assert.IsFalse(BaseTable.InitializationOnly);
            Assert.IsTrue(BaseTable.Initially.InitializationOnly);

        }

        [TestMethod]
        public void DynamicTableDetection()
        {
            var s = new Simulation(nameof(DynamicTableDetection));
            var n = (Var<int>)"n";
            s.BeginPredicates();
            var BaseTableStatic = Predicate("BaseTableStatic", n);
            BaseTableStatic.Initially[n].If(n == 1);
            BaseTableStatic.Initially[n].If(n == 2);
            var derivedStatic = Predicate("derivedStatic", n).If(BaseTableStatic[n]);
            var derivedDynamic = Predicate("derivedDynamic", n).If(RandomElement(BaseTableStatic, n));
            var BaseTableDynamic = Predicate("BaseTableDynamic", n);
            BaseTableDynamic.Add[n].If(PickRandomly(n, 1, 2, 3, 4));
            var derivedStatic2 = Predicate("derivedStatic2", n).If(BaseTableDynamic[n]);
            var derivedDynamic2 = Predicate("derivedDynamic2", n).If(RandomElement(BaseTableDynamic, n));
            s.EndPredicates();
            Assert.IsTrue(BaseTableStatic.IsStatic);
            Assert.IsTrue(derivedStatic.IsStatic);
            Assert.IsTrue(derivedDynamic.IsDynamic);
            Assert.IsTrue(derivedStatic2.IsDynamic);
            Assert.IsTrue(derivedDynamic2.IsDynamic);
            Assert.IsTrue(BaseTableDynamic.Add.IsDynamic);
        }

        [TestMethod]
        public void ImpurePredicate()
        {
            var s = new Simulation(nameof(ImpurePredicate));
            var n = (Var<int>)"n";
            var T = Test<int>("T",_ => true, false);
            s.BeginPredicates();
            var BaseTableStatic = Predicate("BaseTableStatic", n);
            BaseTableStatic.Initially[n].If(n == 1);
            BaseTableStatic.Initially[n].If(n == 2);
            var derivedDynamic = Predicate("derivedDynamic", n).If(BaseTableStatic[n], T[n]);
            s.EndPredicates();
            Assert.IsTrue(derivedDynamic.IsDynamic);
        }

        [TestMethod]
        public void ImpureFunction()
        {
            var s = new Simulation(nameof(ImpureFunction));
            var n = (Var<int>)"n";
            var m = (Var<int>)"m";
            var impure = Function("impure", (int i) => i, false);
            var pure = Function("pure", (int i) => i, true);
            s.BeginPredicates();
            var BaseTableStatic = Predicate("BaseTableStatic", n);
            BaseTableStatic.Initially[n].If(n == 1);
            BaseTableStatic.Initially[n].If(n == 2);
            var derivedStatic= Predicate("derivedStatic", n).If(BaseTableStatic[m], n == pure[m]);
            var derivedDynamic = Predicate("derivedDynamic", n).If(BaseTableStatic[m], n == impure[m]);
            s.EndPredicates();

            Assert.IsTrue(derivedStatic.IsStatic);
            Assert.IsTrue(derivedDynamic.IsDynamic);
        }
    }
}