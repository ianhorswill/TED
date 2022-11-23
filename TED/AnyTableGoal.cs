using System.Diagnostics;
using System.Text;

namespace TED
{
    /// <summary>
    /// A Goal represents a table predicate applied to arguments, e.g. p["a"], p[variable], etc.
    /// Goals are used as the arguments to Prover.Prove, but also as the Head and Body of Rules.
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

        /// <inheritdoc />
        public override AnyPredicate Predicate => TablePredicate;
    }
}
