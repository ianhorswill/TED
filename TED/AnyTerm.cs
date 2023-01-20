using System;

namespace TED
{
    /// <summary>
    /// Untyped base class of all Terms.  Terms are expressions representing arguments to predicates.
    /// This only has one direct subclass, Term[T], whose subclasses are variables (Var[T]), constants (Constant[T]),
    /// and functional expressions (FunctionalExpression[T]).
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
        public virtual bool IsVariable => false;

        /// <summary>
        /// True if this is a FunctionalExpression.
        /// We have to make this test available with a virtual property because the actual FunctionalExpression
        /// class is a generic class.  The *is* operator doesn't give you a way of asking "is this a functional
        /// expression" in general; you can only ask "is this a functional expression denoting an integer" or
        /// 'denoting a boolean", or what have you.
        /// </summary>
        public virtual bool IsFunctionalExpression => false;

        /// <summary>
        /// Return the information necessary to hoist a FunctionalExpression from a goal.
        /// This is only implemented for the FunctionalExpression class.
        /// </summary>
        /// <returns>The expression, a variable of the right type to hold it's value, and a call to the Eval primitive to compute it</returns>
        /// <exception cref="NotImplementedException">If this object isn't a FunctionalExpression</exception>
        internal virtual (AnyTerm Expression, AnyTerm Var, AnyGoal EvalGoal) HoistInfo()
        {
            throw new NotImplementedException("This shouldn't be called");
        }
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

        internal abstract Term<T> ApplySubstitution(Substitution s);

        public static AnyGoal operator ==(Var<T> v, Term<T> exp) => EvalPrimitive<T>.Singleton[v, exp];
        public static AnyGoal operator !=(Var<T> v, Term<T> exp) => Language.Not[EvalPrimitive<T>.Singleton[v, exp]];

        public static AnyGoal operator ==(Term<T> exp, Var<T> v) => EvalPrimitive<T>.Singleton[v, exp];
        public static AnyGoal operator !=(Term<T> exp, Var<T> v) => Language.Not[EvalPrimitive<T>.Singleton[v, exp]];

        public static AnyGoal operator ==(Constant<T> v, Term<T> exp) => EvalPrimitive<T>.Singleton[v, exp];
        public static AnyGoal operator !=(Constant<T> v, Term<T> exp) => Language.Not[EvalPrimitive<T>.Singleton[v, exp]];

        public static AnyGoal operator ==(Term<T> exp, Constant<T> v) => EvalPrimitive<T>.Singleton[v, exp];
        public static AnyGoal operator !=(Term<T> exp, Constant<T> v) => Language.Not[EvalPrimitive<T>.Singleton[v, exp]];

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
            => AdditionOperator<T>.Singleton[a,b];

        public static FunctionalExpression<T> operator -(Term<T> a, Term<T> b)
            => SubtractionOperator<T>.Singleton[a,b];

        public static FunctionalExpression<T> operator -(Term<T> a)
            => NegationOperator<T>.Singleton[a];

        public static FunctionalExpression<T> operator *(Term<T> a, Term<T> b)
            => MultiplicationOperator<T>.Singleton[a,b];

        public static FunctionalExpression<T> operator /(Term<T> a, Term<T> b)
            => DivisionOperator<T>.Singleton[a,b];

        public static FunctionalExpression<T> operator %(Term<T> a, Term<T> b)
            => ModulusOperator<T>.Singleton[a,b];

        public static FunctionalExpression<T> operator |(Term<T> a, Term<T> b)
            => BitwiseOrOperator<T>.Singleton[a,b];

        public static FunctionalExpression<T> operator &(Term<T> a, Term<T> b)
            => BitwiseAndOperator<T>.Singleton[a,b];

        public static FunctionalExpression<T> operator ^(Term<T> a, Term<T> b)
            => BitwiseXOrOperator<T>.Singleton[a,b];

        public static FunctionalExpression<T> operator ~(Term<T> a)
            => BitwiseNegationOperator<T>.Singleton[a];
    }
}
