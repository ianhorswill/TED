using System.Diagnostics;

namespace TED
{
    /// <summary>
    /// Untyped base class for all predicates
    /// Predicates are either TablePredicates or PrimitivePredicates
    /// TablePredicates are backed by an explicit table containing the extension of the predicate
    /// - These can either be "extensional predicates", where you manually specify the data for the table
    /// - or "intensional predicates", where you specify rules for the computing the extension from
    /// - the extensions of other predicates
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public abstract class Predicate
    {
        /// <summary>
        /// Human-readable name for this predicate
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Make a new predicate
        /// </summary>
        protected Predicate(string name)
        {
            Name = name;
        }

        /// <summary>
        /// True if the predicate is a table predicate or is a primitive that acts as a pure function,
        /// i.e. it always has the same result for the same inputs and has no side-effects.
        /// </summary>
        public virtual bool IsPure => true;
    }
}
