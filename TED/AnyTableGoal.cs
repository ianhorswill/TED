using System.Diagnostics;
using System.Text;

namespace TED
{
    /// <summary>
    /// A TableGoal represents a table predicate applied to arguments, e.g. p["a"], p[variable], etc.
    /// Untyped base class for all goals involving TablePredicates
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebugName) + "}")]
    public abstract class AnyTableGoal : AnyGoal
    {
        /// <summary>
        /// Predicate being called
        /// </summary>
        public readonly TablePredicate TablePredicate;

        /// <summary>
        /// Make a new goal object
        /// </summary>
        protected AnyTableGoal(TablePredicate predicate, AnyTerm[] args) : base(args)
        {
            TablePredicate = predicate;
        }

        /// <summary>
        /// Add a rule for inferring this predicate from other predicates
        /// You cannot add rules to a table you add rows to using AddRow.
        /// </summary>

        public abstract void If(params AnyGoal[] subgoals);

        /// <summary>
        /// Add a "fact" (rule with no subgoals) to the predicate
        /// IMPORTANT: this is different from adding the data directly using AddRow!
        /// A TablePredicate can either either rules (including facts) or you can add data manually
        /// using AddRow, but not both.
        /// </summary>
        public void Fact() => If();

        /// <inheritdoc />
        public override AnyPredicate Predicate => TablePredicate;
    }
}
