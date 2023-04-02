using TED;
using static TED.Language;

namespace Tests
{
    [TestClass]
    public class DefinitionTests
    {
        [TestMethod]
        public void SubstitutionTest()
        {
            var s = new Substitution(true);
            var i = (Var<int>)"i";
            var j = (Var<int>)"j";
            var k = (Var<int>)"k";
            s.ReplaceWith(i,j);
            Assert.AreEqual(5, ((Constant<int>)s.Substitute(new Constant<int>(5))).Value);
            Assert.AreEqual(j, s.Substitute(i));
            var newK = (Var<int>)s.Substitute(k);
            Assert.AreNotEqual(k, newK);
            Assert.AreEqual(k.Name, newK.Name);
            Assert.AreEqual(newK, s.Substitute(k));
        }

        [TestMethod]
        public void SingleGoalExpansion()
        {
            var i = (Var<int>)"i";
            var p = Predicate("p", i);
            var d = Definition("d", i).Is(Not[p[i]]);
            var g = (Definition.DefinitionGoal)d[5];
            var e = g.Expand().ToArray();
            Assert.AreEqual(1, e.Length);
            var newGoal = e[0];
            Assert.AreEqual(Language.Not, newGoal.Predicate);
            var subgoal = ((Constant<Goal>)newGoal.Arguments[0]).Value;
            Assert.AreEqual(p, subgoal.Predicate);
            var arg = subgoal.Arguments[0];
            Assert.AreEqual(5, ((Constant<int>)arg).Value);
        }

        [TestMethod]
        public void DualGoalExpansion()
        {
            var i = (Var<int>)"i";
            var p = Predicate("p", i);
            var q = Predicate("q", i);
            var d = Definition("d", i).Is(Not[p[i]], q[i]);
            var g = (Definition.DefinitionGoal)d[5];
            var e = g.Expand().ToArray();
            Assert.AreEqual(2, e.Length);
            var newGoal = e[0];
            Assert.AreEqual(Language.Not, newGoal.Predicate);
            var subgoal = ((Constant<Goal>)newGoal.Arguments[0]).Value;
            Assert.AreEqual(p, subgoal.Predicate);
            var arg = subgoal.Arguments[0];
            Assert.AreEqual(5, ((Constant<int>)arg).Value);
            newGoal = e[1];
            Assert.AreEqual(q, newGoal.Predicate);
            Assert.AreEqual(5, ((Constant<int>)newGoal.Arguments[0]).Value);
        }

        [TestMethod]
        public void ComplexExpansion()
        {
            var i = (Var<int>)"i";
            var j = (Var<int>)"j";
            var k = (Var<int>)"k";
            var p = Predicate("p", i, j);
            var q = Predicate("q", i, j);
            var d = Definition("d", i, j).Is(p[i,k], q[k, j]);
            var g = (Definition.DefinitionGoal)d[5, k];
            var e = g.Expand().ToArray();
            Assert.AreEqual(2, e.Length);
            var newGoal = e[1];
            Assert.AreEqual(q, newGoal.Predicate);
            Assert.IsInstanceOfType(newGoal.Arguments[0], typeof(Var<int>));
            Assert.AreEqual("k", ((Var<int>)newGoal.Arguments[0]).Name);
            Assert.IsInstanceOfType(newGoal.Arguments[1], typeof(Var<int>));
            Assert.AreEqual("k", ((Var<int>)newGoal.Arguments[1]).Name);
            Assert.AreNotEqual(newGoal.Arguments[0], newGoal.Arguments[1]);
        }
    }
}