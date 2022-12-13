using System;

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
#pragma warning disable CS0660, CS0661
    public abstract class Term<T> : AnyTerm
#pragma warning restore CS0660, CS0661
    {
        /// <summary>
        /// Automatically convert C# constant of type T to Constant terms
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Term<T>(T value) => new Constant<T>(value);

        internal abstract Func<T> MakeEvaluator(GoalAnalyzer ga);

        public static AnyGoal operator ==(Var<T> v, Term<T> exp) => MatchPrimitive<T>.Singleton[v, exp];
        public static AnyGoal operator !=(Var<T> v, Term<T> exp) => Language.Not[MatchPrimitive<T>.Singleton[v, exp]];

        public static AnyGoal operator ==(Term<T> exp, Var<T> v) => MatchPrimitive<T>.Singleton[v, exp];
        public static AnyGoal operator !=(Term<T> exp, Var<T> v) => Language.Not[MatchPrimitive<T>.Singleton[v, exp]];

        /// <summary>
        /// Compare the magnitudes of two values
        /// </summary>
        public static AnyGoal operator <(Term<T> a, Term<T> b) => ComparisonPrimitive<T>.LessThan[a, b];
        /// <summary>
        /// Compare the magnitudes of two values
        /// </summary>
        public static AnyGoal operator >(Term<T> a, Term<T> b) => ComparisonPrimitive<T>.GreaterThan[a, b];

        /// <summary>
        /// Compare the magnitudes of two values
        /// </summary>
        public static AnyGoal operator <=(Term<T> a, Term<T> b) => ComparisonPrimitive<T>.LessThanEq[a, b];

        /// <summary>
        /// Compare the magnitudes of two values
        /// </summary>
        public static AnyGoal operator >=(Term<T> a, Term<T> b) => ComparisonPrimitive<T>.GreaterThanEq[a, b];

        public static FunctionalExpression<T> operator +(Term<T> a, Term<T> b)
            => BinaryArithmeticOperator<T>.Add[a,b];

        public static FunctionalExpression<T> operator -(Term<T> a, Term<T> b)
            => BinaryArithmeticOperator<T>.Subtract[a,b];

        public static FunctionalExpression<T> operator -(Term<T> a)
            => UnaryArithmeticOperator<T>.Negate[a];

        public static FunctionalExpression<T> operator *(Term<T> a, Term<T> b)
            => BinaryArithmeticOperator<T>.Multiply[a,b];

        public static FunctionalExpression<T> operator /(Term<T> a, Term<T> b)
            => BinaryArithmeticOperator<T>.Divide[a,b];

        public static FunctionalExpression<T> operator %(Term<T> a, Term<T> b)
            => BinaryArithmeticOperator<T>.Modulus[a,b];

        public static FunctionalExpression<T> operator |(Term<T> a, Term<T> b)
            => BinaryArithmeticOperator<T>.BitwiseOr[a,b];

        public static FunctionalExpression<T> operator &(Term<T> a, Term<T> b)
            => BinaryArithmeticOperator<T>.BitwiseAnd[a,b];

        public static FunctionalExpression<T> operator ^(Term<T> a, Term<T> b)
            => BinaryArithmeticOperator<T>.BitwiseXor[a,b];

        public static FunctionalExpression<T> operator ~(Term<T> a)
            => UnaryArithmeticOperator<T>.BitwiseNot[a];
    }
}
