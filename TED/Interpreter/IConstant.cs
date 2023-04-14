using System;

namespace TED.Interpreter
{
    /// <summary>
    /// Untyped base interface to identify Terms as being Constant[T] for some T.
    /// THE ONLY CLASS THAT SHOULD IMPLEMENT THIS IS Constant[T]!  This only exists to give us a way of asking if a Term is a Constant[T]
    /// without knowing in advance what T is.
    /// </summary>
    public interface IConstant
    {
        /// <summary>
        /// The type of the constant's value
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// True if this and t are both Constants of the same type, with the same value.
        /// </summary>
        public bool IsSameConstant(Term t);

        /// <summary>
        /// The value of the constant
        /// </summary>
        public object? ValueUntyped { get; }
    }
}
