using System;
using System.Diagnostics;

namespace TED
{
    /// <summary>
    /// Terms that represent constants of type T
    /// </summary>
    public class Constant<T> : Term<T>
    {
        /// <summary>
        /// The actual constant
        /// </summary>
        public readonly T Value;

        /// <summary>
        /// This is not a variable
        /// </summary>
        public override bool IsVariable => false;

        /// <summary>
        /// Make a Constant object to wrap the specified constant
        /// </summary>
        /// <param name="value">value to wrap</param>
        public Constant(T value)
        {
            // If value is a Term then something has gone wrong with your typing
            Debug.Assert(!(Value is AnyTerm));
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

        internal override Func<T> MakeEvaluator(GoalAnalyzer _) => () => Value;
    }}
