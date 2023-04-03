using System.Diagnostics;

namespace TED.Interpreter
{
    /// <summary>
    /// Untyped base class for all calls to predicates.
    /// These are essentially iterators for the call: they contain the state information used for
    /// backtracking.  Calls to different kinds of predicates or with different mode patterns may
    /// have different call objects because they need different state information
    /// </summary>
    [DebuggerDisplay("{DebugString}")]
    public abstract class Call
    {
        /// <summary>
        /// The predicate being called
        /// </summary>
        public readonly Predicate Predicate;

        /// <summary>
        /// Make an object to interpret a particular goal in a particular rule.
        /// </summary>
        /// <param name="predicate"></param>
        protected Call(Predicate predicate)
        {
            Predicate = predicate;
        }

        /// <summary>
        /// The formal arguments in the call - the variables and constants for each argument
        /// </summary>
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

        /// <inheritdoc />
        public override string ToString() => $"{Predicate.Name}{ArgumentPattern}";

        private string DebugString => ToString();
    }
}
