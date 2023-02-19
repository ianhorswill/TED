namespace TED
{
    /// <summary>
    /// Untyped base interface to identify Terms as being Var[T] for some T.
    /// THE ONLY CLASS THAT SHOULD IMPLEMENT THIS IS Var[T]!  This only exists to give us a way of asking if a Term is a Var[T]
    /// without knowing in advance what T is.
    /// </summary>
    public interface IVariable
    {
        /// <summary>
        /// Name of the variable
        /// </summary>
        public string VariableName { get;  }
    }
}
