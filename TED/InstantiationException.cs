using System;

namespace TED
{
    /// <summary>
    /// Exception that gets thrown when an bound value is passed into a predicate that needs an unbound variable, or vice-versa
    /// </summary>
    public class InstantiationException : ArgumentException
    {
        public InstantiationException(string message) : base(message)
        { }
        public InstantiationException(Predicate p, Term t)
            : base($"{p.Name} called with improperly instantiated argument {t}")
        { }
    }
}
