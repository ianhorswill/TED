using System.Collections.Generic;

namespace TED
{
    /// <summary>
    /// A container for a run-time value manipulated by Calls in a Rule
    /// These are used to hold:
    /// - The run-time values of variables (Vars) within a Rule.
    /// - Constants
    /// These get read and written through MatchInstructions, which get run when a Pattern is Matched
    /// or written to a row in a table using its Write method.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ValueCell<T>
    {
        /// <summary>
        /// Data stored in the cell.
        /// </summary>
        public T Value;

        /// <summary>
        /// Object used to test equality of two objects of type T
        /// We need this because C# generics don't allow you to use ==
        /// </summary>
        private static readonly EqualityComparer<T> Equal = EqualityComparer<T>.Default;

        /// <summary>
        /// Table of ValueCells used for different constants of type T
        /// </summary>
        private static readonly Dictionary<T, ValueCell<T>> ConstantTable = new Dictionary<T, ValueCell<T>>();

        private ValueCell(T value)
        {
            Value = value;
        }

        private ValueCell() : this(default(T)!)
        {

        }

        /// <summary>
        /// Make a ValueCell to hold the run-time value of a variable (a Var[T]).
        /// </summary>
        public static ValueCell<T> MakeVariable() => new ValueCell<T>(default(T)!);

        /// <summary>
        /// Return a ValueCell holding the specified constant
        /// There will only be one such cell per constant.
        /// </summary>
        public static ValueCell<T> Constant(T value)
        {
            if (!ConstantTable.TryGetValue(value, out var c))
            {
                c = new ValueCell<T>(value);
                ConstantTable[value] = c;
            }

            return c;
        }

        /// <summary>
        /// Test of the cell holds the specified value
        /// </summary>
        public bool Match(in T value) => Equal.Equals(Value, value);
    }
}
