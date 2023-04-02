using System;

namespace TED
{
    /// <summary>
    /// Exception that gets thrown when an bound value is passed into a predicate that needs an unbound variable, or vice-versa
    /// </summary>
    public class InstantiationException : ArgumentException
    {
        /// <summary>
        /// Make an exception signaling that a predicate is being called with an unbound variable that must be bound or vice-versa.
        /// </summary>
        public InstantiationException(string message) : base(message)
        { }

        /// <summary>
        /// Make an exception stating that the specified predicate was called with the specified term as an argument
        /// and that term either wasn't bound when it should have been was was bound when it shouldn't have.
        /// </summary>
        public InstantiationException(Predicate p, Term t)
            : base($"{p.Name} called with improperly instantiated argument {t}")
        { }
    }
}
