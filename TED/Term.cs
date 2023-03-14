﻿using System;

namespace TED {
    /// <summary>
    /// Untyped base class of all Terms.  Terms are expressions representing arguments to predicates.
    /// This only has one direct subclass, Term[T], whose subclasses are variables (Var[T]), constants (Constant[T]),
    /// and functional expressions (FunctionalExpression[T]).
    /// </summary>
    public abstract class Term
    {
        /// <summary>
        /// Type of this term
        /// </summary>
        public abstract Type Type { get; }

        /// <summary>
        /// Make a variable of the same type as this term
        /// </summary>
        internal abstract Term MakeVariable(string name);

        protected static Goal CatchComparisonTypeInitializerProblem(Func<Goal> thunk, string operation) {
            try {
                return thunk();
            }
            catch (TypeInitializationException e) {
                throw new MissingMethodException($"There is no {operation} overload defined for type {e.TypeName}");
            }
        }

        protected static FunctionalExpression<T> CatchFunctionalExpressionTypeInitializerProblem<T>(Func<FunctionalExpression<T>> thunk, string operation) {
            try {
                return thunk();
            }
            catch (TypeInitializationException e) {
                throw new MissingMethodException($"There is no {operation} overload defined for type {e.TypeName}");
            }
        }
    }

    /// <summary>
    /// Base class for terms whose values are type T.
    /// </summary>
    /// <typeparam name="T">Type of the value of the variable</typeparam>
#pragma warning disable CS0660, CS0661
    public abstract class Term<T> : Term
#pragma warning restore CS0660, CS0661
    {
        /// <summary>
        /// Automatically convert C# constant of type T to Constant terms
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Term<T>(T value) => new Constant<T>(value);

        /// <summary>
        /// Call a 0-arity TedFunction with return type T by name and return Term of type T from the function Call
        /// </summary>
        /// <param name="func"></param>
        public static implicit operator Term<T>(TedFunction<T> func) => func.Call();

        internal abstract Func<T> MakeEvaluator(GoalAnalyzer ga);

        internal abstract Term<T> ApplySubstitution(Substitution s);

        /// <inheritdoc />
        public override Type Type => Type;

        internal override Term MakeVariable(string name) => (Var<T>)name;

        public static Goal operator ==(Var<T> v, Term<T> exp) => EvalPrimitive<T>.Singleton[v, exp];
        public static Goal operator !=(Var<T> v, Term<T> exp) => Language.Not[EvalPrimitive<T>.Singleton[v, exp]];

        //public static Goal operator ==(Term<T> exp, Var<T> v) => EvalPrimitive<T>.Singleton[v, exp];
        //public static Goal operator !=(Term<T> exp, Var<T> v) => Language.Not[EvalPrimitive<T>.Singleton[v, exp]];

        public static Goal operator ==(Constant<T> v, Term<T> exp) => EvalPrimitive<T>.Singleton[v, exp];
        public static Goal operator !=(Constant<T> v, Term<T> exp) => Language.Not[EvalPrimitive<T>.Singleton[v, exp]];

        public static Goal operator ==(Term<T> exp, Constant<T> v) => EvalPrimitive<T>.Singleton[v, exp];
        public static Goal operator !=(Term<T> exp, Constant<T> v) => Language.Not[EvalPrimitive<T>.Singleton[v, exp]];

        /// <summary>
        /// Compare the magnitudes of two values
        /// </summary>
        public static Goal operator <(Term<T> a, Term<T> b) =>
            CatchComparisonTypeInitializerProblem(() => ComparisonPrimitive<T>.LessThan[a, b], "<");
        /// <summary>
        /// Compare the magnitudes of two values
        /// </summary>
        public static Goal operator >(Term<T> a, Term<T> b)
            => CatchComparisonTypeInitializerProblem(() => ComparisonPrimitive<T>.GreaterThan[a, b], ">");
        /// <summary>
        /// Compare the magnitudes of two values
        /// </summary>
        public static Goal operator <=(Term<T> a, Term<T> b)
            => CatchComparisonTypeInitializerProblem(() => ComparisonPrimitive<T>.LessThanEq[a, b], "<=");
        /// <summary>
        /// Compare the magnitudes of two values
        /// </summary>
        public static Goal operator >=(Term<T> a, Term<T> b)
            => CatchComparisonTypeInitializerProblem(() => ComparisonPrimitive<T>.GreaterThanEq[a, b], ">=");

        public static FunctionalExpression<T> operator +(Term<T> a, Term<T> b)
            => CatchFunctionalExpressionTypeInitializerProblem(() => AdditionOperator<T>.Singleton[a, b], "+");

        public static FunctionalExpression<T> operator -(Term<T> a, Term<T> b)
            => CatchFunctionalExpressionTypeInitializerProblem(() => SubtractionOperator<T>.Singleton[a,b], "-");

        public static FunctionalExpression<T> operator -(Term<T> a)
            => CatchFunctionalExpressionTypeInitializerProblem(() => NegationOperator<T>.Singleton[a], "unary -");

        public static FunctionalExpression<T> operator *(Term<T> a, Term<T> b)
            => CatchFunctionalExpressionTypeInitializerProblem(() => MultiplicationOperator<T>.Singleton[a,b], "*");

        public static FunctionalExpression<T> operator /(Term<T> a, Term<T> b)
            => CatchFunctionalExpressionTypeInitializerProblem(() => DivisionOperator<T>.Singleton[a,b], "/");

        public static FunctionalExpression<T> operator %(Term<T> a, Term<T> b)
            => CatchFunctionalExpressionTypeInitializerProblem(() => ModulusOperator<T>.Singleton[a,b], "%");

        public static FunctionalExpression<T> operator |(Term<T> a, Term<T> b)
            => CatchFunctionalExpressionTypeInitializerProblem(() => BitwiseOrOperator<T>.Singleton[a,b], "|");

        public static FunctionalExpression<T> operator &(Term<T> a, Term<T> b)
            => CatchFunctionalExpressionTypeInitializerProblem(() => BitwiseAndOperator<T>.Singleton[a,b], "&");

        public static FunctionalExpression<T> operator ^(Term<T> a, Term<T> b)
            => CatchFunctionalExpressionTypeInitializerProblem(() => BitwiseXOrOperator<T>.Singleton[a,b], "^");

        public static FunctionalExpression<T> operator ~(Term<T> a)
            => CatchFunctionalExpressionTypeInitializerProblem(() => BitwiseNegationOperator<T>.Singleton[a], "~");
    }
}