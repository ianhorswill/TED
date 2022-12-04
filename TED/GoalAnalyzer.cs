using System.Collections.Generic;
using System.Linq;

namespace TED
{
    /// <summary>
    /// Keeps the state involved in scanning the body of a rule:
    /// - What TablePredicates does this rule depend on (reference)
    /// - What variables appear in the rule?
    /// - What ValueCells do they correspond to?
    /// - Have they been bound yet?
    /// </summary>
    internal class GoalAnalyzer
    {
        private readonly Dictionary<AnyTerm, object> variableValueCells = new Dictionary<AnyTerm, object>();
        private readonly HashSet<TablePredicate> Tables = new HashSet<TablePredicate>();

        public void AddDependency(TablePredicate p) => Tables.Add(p);

        public TablePredicate[] Dependencies => Tables.ToArray();

        public MatchOperation<T> Emit<T>(Term<T> term)
        {
            if (term is Constant<T> c)
                return MatchOperation<T>.Constant(c.Value);
            // it's a variable
            var v = (Var<T>)term;
            if (variableValueCells.TryGetValue(v, out var cell))
                return MatchOperation<T>.Read((ValueCell<T>)cell);
            else
            {
                var vc = ValueCell<T>.MakeVariable(v.Name);
                variableValueCells[v] = vc;
                return MatchOperation<T>.Write(vc);
            }
        }
    }
}
