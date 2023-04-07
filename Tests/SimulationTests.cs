using TED;
using static TED.Language;
// ReSharper disable InconsistentNaming

namespace Tests
{
    [TestClass]
    public class SimulationTests
    {
        [TestMethod]
        public void AddTest()
        {
            var counter = 0;
            var NextCounter = Function("NextCounter", () => counter++);

            var n = (Var<int>)"n";

            var s = new Simulation(nameof(AddTest));
            s.BeginPredicates();
            var BaseTable = Predicate("BaseTable", n);
            BaseTable.Add[n].If(n == NextCounter);
            s.EndPredicates();

            s.Update();
            s.Update();
            s.Update();
            CollectionAssert.AreEqual(new [] { 0, 1, 2}, BaseTable.ToArray());
        }
    }
}