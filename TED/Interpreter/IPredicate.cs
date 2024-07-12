namespace TED.Interpreter
{
    /// <summary>
    /// Interface to abstract across predicates and definitions
    /// </summary>
    public interface IPredicate<T1>
    {
        /// <summary>
        /// Name of the underlying predicate
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Call the predicate with the specified arguments
        /// </summary>
        Goal this[Term<T1> arg1] { get; }
    }

    /// <summary>
    /// Interface to abstract across predicates and definitions
    /// </summary>
    public interface IPredicate<T1,T2>
    {
        /// <summary>
        /// Name of the underlying predicate
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Call the predicate with the specified arguments
        /// </summary>
        Goal this[Term<T1> arg1, Term<T2> arg2] { get; }
    }

    /// <summary>
    /// Interface to abstract across predicates and definitions
    /// </summary>
    public interface IPredicate<T1,T2,T3>
    {
        /// <summary>
        /// Name of the underlying predicate
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Call the predicate with the specified arguments
        /// </summary>
        Goal this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3] { get; }
    }

    /// <summary>
    /// Interface to abstract across predicates and definitions
    /// </summary>
    public interface IPredicate<T1,T2,T3,T4>
    {
        /// <summary>
        /// Name of the underlying predicate
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Call the predicate with the specified arguments
        /// </summary>
        Goal this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4] { get; }
    }

    /// <summary>
    /// Interface to abstract across predicates and definitions
    /// </summary>
    public interface IPredicate<T1,T2,T3,T4,T5>
    {
        /// <summary>
        /// Name of the underlying predicate
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Call the predicate with the specified arguments
        /// </summary>
        Goal this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5] { get; }
    }

    /// <summary>
    /// Interface to abstract across predicates and definitions
    /// </summary>
    public interface IPredicate<T1,T2,T3,T4,T5,T6>
    {
        /// <summary>
        /// Name of the underlying predicate
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Call the predicate with the specified arguments
        /// </summary>
        Goal this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5, Term<T6> arg6] { get; }
    }

    /// <summary>
    /// Interface to abstract across predicates and definitions
    /// </summary>
    public interface IPredicate<T1,T2,T3,T4,T5,T6,T7>
    {
        /// <summary>
        /// Name of the underlying predicate
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Call the predicate with the specified arguments
        /// </summary>
        Goal this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5, Term<T6> arg6, Term<T7> arg7] { get; }
    }

    /// <summary>
    /// Interface to abstract across predicates and definitions
    /// </summary>
    public interface IPredicate<T1,T2,T3,T4,T5,T6,T7,T8>
    {
        /// <summary>
        /// Name of the underlying predicate
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Call the predicate with the specified arguments
        /// </summary>
        Goal this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5, Term<T6> arg6, Term<T7> arg7, Term<T8> arg8] { get; }
    }
}
