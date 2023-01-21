using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace TED
{
    /// <summary>
    /// Represents an instance of an argument being passed to a predicate
    /// Represents:
    /// - What is being passed (variable or constant, both represented by a ValueCell)
    /// - Whether it is
    ///     - The first use of a variable, and so needs to be stored into the variable (Write mode)
    ///     - It's a constant (Constant mode) or variable that's already been set (Read mode)
    ///     - It's ignored
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("{debuggerDisplay}")]
    public readonly struct MatchOperation<T>
    {
        /// <summary>
        /// How to match this formal argument in a pattern to the value in a table row.
        /// </summary>
        enum Opcode
        {
            /// <summary>
            /// The argument is a variable and it's already been bound to a value by a previous call.
            /// So match the value of the variable's ValueCell to the value.
            /// </summary>
            Read,
            /// <summary>
            /// The argument is a variable that has not yet been bound to a variable.
            /// So write the value into the variable's ValueCell
            /// </summary>
            Write,
            /// <summary>
            /// The value is a constant stored in a ValueCell, so match the value passed to the one in the ValueCell
            /// </summary>
            Constant,
            /// <summary>
            /// The argument is a variable that isn't used anywhere else.  So don't bother doing anything.
            /// </summary>
            Ignore
        };

        /// <summary>
        /// Whether we are reading, writing or ignoring the argument
        /// </summary>
        private readonly Opcode opcode;

        /// <summary>
        /// The value cell to contain the argument value
        /// </summary>
        public readonly ValueCell<T> ValueCell;

        /// <summary>
        /// Are we comparing to a known value?
        /// </summary>
        public bool IsInstantiated => opcode == Opcode.Read || opcode == Opcode.Constant;

        private MatchOperation(Opcode opcode, ValueCell<T> valueCell)
        {
            this.opcode = opcode;
            this.ValueCell = valueCell;
        }

        /// <summary>
        /// A MatchOperation that tests whether the value we're matching to is a specified constant
        /// </summary>
        public static MatchOperation<T> Constant(T value) =>
            new MatchOperation<T>(Opcode.Constant, ValueCell<T>.Constant(value));

        /// <summary>
        /// A MatchOperation that tests whether the value we're matching is the value of a bound variable
        /// </summary>
        public static MatchOperation<T> Read(ValueCell<T> value) =>
            new MatchOperation<T>(Opcode.Read, value);

        /// <summary>
        /// A MatchOperation that stores the value being matched to into a variable (variable should not be previously bound)
        /// </summary>
        public static MatchOperation<T> Write(ValueCell<T> value) =>
            new MatchOperation<T>(Opcode.Write, value);

        /// <summary>
        /// Write the value of the variable or constant in the MatchOperation to the specified location
        /// </summary>
        /// <param name="target"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(out T target) => target = ValueCell.Value;

        /// <summary>
        /// Return the value of the variable or constant in this MatchOperation.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Match(in T target)
        {
            switch (opcode)
            {
                case Opcode.Write:
                    ValueCell.Value = target;
                    return true;

                case Opcode.Constant:
                    case Opcode.Read:
                    return ValueCell.Match(target);

                case Opcode.Ignore:
                    return true;

                default:
                    throw new InvalidOperationException("Invalid opcode in match cal");
            }
        }

        /// <summary>
        /// Return the value of the variable or constant in this MatchOperation.
        /// </summary>
        public T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                switch (opcode)
                {
                    case Opcode.Constant:
                    case Opcode.Read:
                        return ValueCell.Value;

                    default:
                        throw new InvalidOperationException("Invalid opcode in Copy() call");
                }
            }
        }

        public override string ToString()
        {
            switch (opcode)
            {
                case Opcode.Constant:
                    return Value==null?"null":Value.ToString();

                case Opcode.Ignore:
                    return "_";

                case Opcode.Read:
                    return $"in {ValueCell.Name}";

                case Opcode.Write:
                    return $"out {ValueCell.Name}";

                default:
                    throw new InvalidDataException("Invalid opcode in MatchInstruction");
            }
        }

        private string debuggerDisplay => ToString();
    }
}
