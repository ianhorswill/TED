using System.Diagnostics;

namespace TED
{
    /// <summary>
    /// Untyped base class for all calls to predicates.
    /// These are essentially iterators for the call: they contain the state information used for
    /// backtracking.  Calls to different kinds of predicates or with different mode patterns may
    /// have different call objects because they need different state information
    /// </summary>
    [DebuggerDisplay("{DebugString}")]
    public abstract class AnyCall
    {
        public readonly AnyPredicate Predicate;

        protected AnyCall(AnyPredicate predicate)
        {
            Predicate = predicate;
        }

        public abstract IPattern ArgumentPattern { get; }

        /// <summary>
        /// Called before the start of a call.  Initializes any backtracking state for the call
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Find the first/next solution to the call, writing any variables that need to be bound by it
        /// </summary>
        /// <returns>True if it found another solution</returns>
        public abstract bool NextSolution();

        public override string ToString() => $"{Predicate.Name}{ArgumentPattern}";

        private string DebugString => ToString();
    }
}
