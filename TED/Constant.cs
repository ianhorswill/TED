using System;
using System.Diagnostics;
using TED.Interpreter;
using TED.Preprocessing;

namespace TED
{
    /// <summary>
    /// Terms that represent constants of type T
    /// </summary>
    public sealed class Constant<T> : Term<T>, IConstant
    {
        /// <summary>
        /// The actual constant
        /// </summary>
        public readonly T Value;

        /// <inheritdoc />
        public object? ValueUntyped => Value;
        
        /// <summary>
        /// Make a Constant object to wrap the specified constant
        /// </summary>
        /// <param name="value">value to wrap</param>
        public Constant(T value)
        {
            // If value is a Term then something has gone wrong with your typing
            Debug.Assert(!(Value is Term));
            Value = value;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            switch (Value)
            {
                case null: return "null";
                case string s: return $"\"{s}\"";
                default: return Value.ToString();
            }
        }
        
        /// <inheritdoc />
        internal override Func<T> MakeEvaluator(GoalAnalyzer _) => () => Value;

        internal override Term<T> ApplySubstitution(Substitution s)
        {
            if (this is Constant<Goal> subgoal)
                return (Term<T>)(object)(new Constant<Goal>(subgoal.Value.RenameArguments(s)));
            return this;
        }

        /// <inheritdoc />
        public bool IsSameConstant(Term t)
        {
            return t is Constant<T> c && Equals(Value, c.Value);
        }

        /// <inheritdoc />
        public override string ToSourceExpression(Compiler.Compiler _) => Compiler.Compiler.ToSourceLiteral(Value);
    }}
