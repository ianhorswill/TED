using System;
using System.Diagnostics;
using System.IO;

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
    internal readonly struct MatchOperation<T>
    {
        enum Opcode
        {
            Read,
            Write,
            Constant,
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
        public void Write(out T target) => target = ValueCell.Value;

        /// <summary>
        /// Return the value of the variable or constant in this MatchOperation.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public bool Match(T target)
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
                    return Value.ToString();

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
