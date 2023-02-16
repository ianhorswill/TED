using System;
using System.Diagnostics;

namespace TED
{
    /// <summary>
    /// Terms that represent constants of type T
    /// </summary>
    public sealed class Constant<T> : Term<T>
    {
        /// <summary>
        /// The actual constant
        /// </summary>
        public readonly T Value;
        
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
    }}
