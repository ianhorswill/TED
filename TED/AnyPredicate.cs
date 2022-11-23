using System;
using System.Collections.Generic;
using System.Text;

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
    public abstract class AnyPredicate
    {
        /// <summary>
        /// Human-readable name for this predicate
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Make a new predicate
        /// </summary>
        protected AnyPredicate(string name)
        {
            Name = name;
        }
    }
}
