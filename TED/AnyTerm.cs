namespace TED
{
    /// <summary>
    /// Base class of all Terms.  Terms are expressions representing arguments to predicates.
    /// </summary>
    public abstract class AnyTerm
    {
        /// <summary>
        /// We use this rather than an "is" test because there's no way given the C# type system
        /// to have a base class for all terms and also a base class for just the variables, but
        /// regardless of the type of the variable.  So we have to test whether something is a variable
        /// using a virtual function.  And unless microsoft has optimized their JIT compilation of
        /// type testing, this is faster anyway.
        /// </summary>
        /// <value></value>
        public abstract bool IsVariable { get; }
    }

    /// <summary>
    /// Base class for terms whose values are type T.
    /// </summary>
    /// <typeparam name="T">Type of the value of the variable</typeparam>
    public abstract class Term<T> : AnyTerm
    {
        /// <summary>
        /// Automatically convert C# constant of type T to Constant terms
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Term<T>(T value) => new Constant<T>(value);

        /// <summary>
        /// Compare the magnitudes of two values
        /// </summary>
        public static AnyGoal operator <(Term<T> a, Term<T> b) => Language.LessThan<T>(a, b);
        /// <summary>
        /// Compare the magnitudes of two values
        /// </summary>
        public static AnyGoal operator >(Term<T> a, Term<T> b) => Language.GreaterThan<T>(a, b);
        /// <summary>
        /// Compare the magnitudes of two values
        /// </summary>
        public static AnyGoal operator <=(Term<T> a, Term<T> b) => Language.LessThanEqual<T>(a, b);
        /// <summary>
        /// Compare the magnitudes of two values
        /// </summary>
        public static AnyGoal operator >=(Term<T> a, Term<T> b) => Language.GreaterThanEqual<T>(a, b);
    }
}
