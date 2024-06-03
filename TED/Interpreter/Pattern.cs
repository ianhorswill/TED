using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TED.Interpreter;

namespace TED
{
    /// <summary>
    /// Describes how to match or write the value of the arguments in a Goal
    /// Compiled from the arguments to a Goal object
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public readonly struct Pattern : IPattern {
        /// <inheritdoc />
        public bool IsInstantiated => true;

        /// <inheritdoc />
        public bool IsReadModeAt(int index)
            => throw new ArgumentException($"Pattern doesn't have an argument number {index}");

        public ValueCell ArgumentCell(int index) => throw new ArgumentException($"Pattern doesn't have an argument number {index}");

        /// <inheritdoc />
        public override string ToString() => "[No pattern]";

        private string DebuggerDisplay => ToString();

        /// <inheritdoc />
        public IMatchOperation[] Arguments => Array.Empty<IMatchOperation>();
    }

    /// <summary>
    /// Describes how to match or write the value of the arguments in a Goal
    /// Compiled from the arguments to a Goal object
    /// </summary>
    /// <typeparam name="T1">Type of the goal's argument</typeparam>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public readonly struct Pattern<T1> : IPattern {
        /// <summary>
        /// Argument
        /// </summary>
        public readonly MatchOperation<T1> Arg1;

        /// <summary>
        /// Make a pattern with the specified arguments
        /// </summary>
        public Pattern(MatchOperation<T1> arg1) {
            Arg1 = arg1;
        }

        /// <summary>
        /// Write the values from the pattern out to a row in a table after a successful match
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(out T1 target) {
            Arg1.Write(out target);
        }

        /// <summary>
        /// The value the pattern was successfully matched to
        /// </summary>
        public T1 Value {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Arg1.Value;
        }

        /// <summary>
        /// Attempt to match this pattern to a row, storing the relevant values into
        /// any Cells contained in the pattern, when those arguments are in write mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Match(in T1 target) => Arg1.Match(in target);

        /// <summary>
        /// True if all arguments in the pattern are instantiated.
        /// </summary>
        public bool IsInstantiated => Arg1.IsInstantiated;

        /// <inheritdoc />
        public bool IsReadModeAt(int index)
            => index switch
            {
                0 => Arg1.IsInstantiated,
                _ => throw new ArgumentException($"Pattern doesn't have an argument number {index}")
            };

        /// <inheritdoc />
        public ValueCell ArgumentCell(int index)
            => index switch
            {
                0 => Arg1.ValueCell,
                _ => throw new ArgumentException($"Pattern doesn't have an argument number {index}")
            };

        /// <inheritdoc />
        public override string ToString() => $"[{Arg1}]";

        private string DebuggerDisplay => ToString();

        /// <inheritdoc />
        public IMatchOperation[] Arguments => new[] { (IMatchOperation)Arg1 };
    }

    /// <summary>
    /// Describes how to match or write the value of the arguments in a Goal
    /// Compiled from the arguments to a Goal object
    /// </summary>
    /// <typeparam name="T1">Type of the goal's first argument</typeparam>
    /// <typeparam name="T2">Type of the goal's second argument</typeparam>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public readonly struct Pattern<T1, T2> : IPattern {
        /// <summary>
        /// First argument
        /// </summary>
        public readonly MatchOperation<T1> Arg1;
        /// <summary>
        /// Second argument
        /// </summary>
        public readonly MatchOperation<T2> Arg2;
        
        /// <summary>
        /// Make a pattern with the specified arguments
        /// </summary>
        public Pattern(MatchOperation<T1> arg1, MatchOperation<T2> arg2) {
            Arg1 = arg1;
            Arg2 = arg2;
        }

        /// <summary>
        /// Write the values from the pattern out to a row in a table after a successful match
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(out (T1, T2) target) {
            Arg1.Write(out target.Item1);
            Arg2.Write(out target.Item2);
        }

        /// <summary>
        /// The value the pattern was successfully matched to
        /// </summary>
        public (T1, T2) Value {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (Arg1.Value, Arg2.Value);
        }

        /// <summary>
        /// Attempt to match this pattern to a row, storing the relevant values into
        /// any Cells contained in the pattern, when those arguments are in write mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Match(in (T1, T2) target) => Arg1.Match(in target.Item1) && Arg2.Match(in target.Item2);

        /// <summary>
        /// True if all arguments in the pattern are instantiated.
        /// </summary>
        public bool IsInstantiated => Arg1.IsInstantiated && Arg2.IsInstantiated;

        /// <inheritdoc />
        public bool IsReadModeAt(int index)
            => index switch
            {
                0 => Arg1.IsInstantiated,
                1 => Arg2.IsInstantiated,
                _ => throw new ArgumentException($"Pattern doesn't have an argument number {index}")
            };

        /// <inheritdoc />
        public ValueCell ArgumentCell(int index)
            => index switch
            {
                0 => Arg1.ValueCell,
                1 => Arg2.ValueCell,
                _ => throw new ArgumentException($"Pattern doesn't have an argument number {index}")
            };

        /// <inheritdoc />
        public override string ToString() => $"[{Arg1},{Arg2}]";

        private string DebuggerDisplay => ToString();

        /// <inheritdoc />
        public IMatchOperation[] Arguments => new[] { (IMatchOperation)Arg1, (IMatchOperation)Arg2 };
    }

    /// <summary>
    /// Describes how to match or write the value of the arguments in a Goal
    /// Compiled from the arguments to a Goal object
    /// </summary>
    /// <typeparam name="T1">Type of the goal's first argument</typeparam>
    /// <typeparam name="T2">Type of the goal's second argument</typeparam>
    /// <typeparam name="T3">Type of the goal's third argument</typeparam>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public readonly struct Pattern<T1, T2, T3> : IPattern {
        /// <summary>
        /// First argument
        /// </summary>
        public readonly MatchOperation<T1> Arg1;
        /// <summary>
        /// Second argument
        /// </summary>
        public readonly MatchOperation<T2> Arg2;
        /// <summary>
        /// Third argument
        /// </summary>
        public readonly MatchOperation<T3> Arg3;

        /// <summary>
        /// Make a pattern with the specified arguments
        /// </summary>
        public Pattern(MatchOperation<T1> arg1, MatchOperation<T2> arg2, MatchOperation<T3> arg3) {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
        }

        /// <summary>
        /// Write the values from the pattern out to a row in a table after a successful match
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(out (T1, T2, T3) target) {
            Arg1.Write(out target.Item1);
            Arg2.Write(out target.Item2);
            Arg3.Write(out target.Item3);
        }

        /// <summary>
        /// The value the pattern was successfully matched to
        /// </summary>
        public (T1, T2, T3) Value {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (Arg1.Value, Arg2.Value, Arg3.Value);
        }

        /// <summary>
        /// Attempt to match this pattern to a row, storing the relevant values into
        /// any Cells contained in the pattern, when those arguments are in write mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Match(in (T1, T2, T3) target)
            => Arg1.Match(in target.Item1) && Arg2.Match(in target.Item2) && Arg3.Match(in target.Item3);

        /// <summary>
        /// True if all arguments in the pattern are instantiated.
        /// </summary>
        public bool IsInstantiated => Arg1.IsInstantiated && Arg2.IsInstantiated && Arg3.IsInstantiated;

        /// <inheritdoc />
        public bool IsReadModeAt(int index)
            => index switch
            {
                0 => Arg1.IsInstantiated,
                1 => Arg2.IsInstantiated,
                2 => Arg3.IsInstantiated,
                _ => throw new ArgumentException($"Pattern doesn't have an argument number {index}")
            };

        /// <inheritdoc />
        public ValueCell ArgumentCell(int index)
            => index switch
            {
                0 => Arg1.ValueCell,
                1 => Arg2.ValueCell,
                2 => Arg3.ValueCell,
                _ => throw new ArgumentException($"Pattern doesn't have an argument number {index}")
            };

        /// <inheritdoc />
        public override string ToString() => $"[{Arg1},{Arg2},{Arg3}]";

        private string DebuggerDisplay => ToString();

        /// <inheritdoc />
        public IMatchOperation[] Arguments => new[] { (IMatchOperation)Arg1, (IMatchOperation)Arg2, (IMatchOperation)Arg3 };
    }

    /// <summary>
    /// Describes how to match or write the value of the arguments in a Goal
    /// Compiled from the arguments to a Goal object
    /// </summary>
    /// <typeparam name="T1">Type of the goal's first argument</typeparam>
    /// <typeparam name="T2">Type of the goal's second argument</typeparam>
    /// <typeparam name="T3">Type of the goal's third argument</typeparam>
    /// <typeparam name="T4">Type of the goal's fourth argument</typeparam>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public readonly struct Pattern<T1, T2, T3, T4> : IPattern {
        /// <summary>
        /// First argument
        /// </summary>
        public readonly MatchOperation<T1> Arg1;
        /// <summary>
        /// Second argument
        /// </summary>
        public readonly MatchOperation<T2> Arg2;
        /// <summary>
        /// Third argument
        /// </summary>
        public readonly MatchOperation<T3> Arg3;
        /// <summary>
        /// Fourth argument
        /// </summary>
        public readonly MatchOperation<T4> Arg4;

        /// <summary>
        /// Make a pattern with the specified arguments
        /// </summary>
        public Pattern(MatchOperation<T1> arg1, MatchOperation<T2> arg2, MatchOperation<T3> arg3, MatchOperation<T4> arg4) {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
        }

        /// <summary>
        /// Write the values from the pattern out to a row in a table after a successful match
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(out (T1, T2, T3, T4) target) {
            Arg1.Write(out target.Item1);
            Arg2.Write(out target.Item2);
            Arg3.Write(out target.Item3);
            Arg4.Write(out target.Item4);
        }

        /// <summary>
        /// The value the pattern was successfully matched to
        /// </summary>
        public (T1, T2, T3, T4) Value {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (Arg1.Value, Arg2.Value, Arg3.Value, Arg4.Value);
        }

        /// <summary>
        /// Attempt to match this pattern to a row, storing the relevant values into
        /// any Cells contained in the pattern, when those arguments are in write mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Match(in (T1, T2, T3, T4) target)
            => Arg1.Match(in target.Item1) && Arg2.Match(in target.Item2) && Arg3.Match(in target.Item3)
               && Arg4.Match(in target.Item4);

        /// <summary>
        /// True if all arguments in the pattern are instantiated.
        /// </summary>
        public bool IsInstantiated
            => Arg1.IsInstantiated && Arg2.IsInstantiated && Arg3.IsInstantiated && Arg4.IsInstantiated;

        /// <inheritdoc />
        public bool IsReadModeAt(int index)
            => index switch
            {
                0 => Arg1.IsInstantiated,
                1 => Arg2.IsInstantiated,
                2 => Arg3.IsInstantiated,
                3 => Arg4.IsInstantiated,
                _ => throw new ArgumentException($"Pattern doesn't have an argument number {index}")
            };

        /// <inheritdoc />
        public ValueCell ArgumentCell(int index)
            => index switch
            {
                0 => Arg1.ValueCell,
                1 => Arg2.ValueCell,
                2 => Arg3.ValueCell,
                3 => Arg4.ValueCell,
                _ => throw new ArgumentException($"Pattern doesn't have an argument number {index}")
            };

        /// <inheritdoc />
        public override string ToString() => $"[{Arg1},{Arg2},{Arg3},{Arg4}]";

        private string DebuggerDisplay => ToString();

        /// <inheritdoc />
        public IMatchOperation[] Arguments => new[] { (IMatchOperation)Arg1, (IMatchOperation)Arg2, (IMatchOperation)Arg3, (IMatchOperation)Arg4 };
    }

    /// <summary>
    /// Describes how to match or write the value of the arguments in a Goal
    /// Compiled from the arguments to a Goal object
    /// </summary>
    /// <typeparam name="T1">Type of the goal's first argument</typeparam>
    /// <typeparam name="T2">Type of the goal's second argument</typeparam>
    /// <typeparam name="T3">Type of the goal's third argument</typeparam>
    /// <typeparam name="T4">Type of the goal's fourth argument</typeparam>
    /// <typeparam name="T5">Type of the goal's fifth argument</typeparam>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public readonly struct Pattern<T1, T2, T3, T4, T5> : IPattern {
        /// <summary>
        /// First argument
        /// </summary>
        public readonly MatchOperation<T1> Arg1;
        /// <summary>
        /// Second argument
        /// </summary>
        public readonly MatchOperation<T2> Arg2;
        /// <summary>
        /// Third argument
        /// </summary>
        public readonly MatchOperation<T3> Arg3;
        /// <summary>
        /// Fourth argument
        /// </summary>
        public readonly MatchOperation<T4> Arg4;
        /// <summary>
        /// Fifth argument
        /// </summary>
        public readonly MatchOperation<T5> Arg5;

        /// <summary>
        /// Make a pattern with the specified arguments
        /// </summary>
        public Pattern(MatchOperation<T1> arg1, MatchOperation<T2> arg2, MatchOperation<T3> arg3, MatchOperation<T4> arg4, MatchOperation<T5> arg5) {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
        }

        /// <summary>
        /// Write the values from the pattern out to a row in a table after a successful match
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(out (T1, T2, T3, T4, T5) target) {
            Arg1.Write(out target.Item1);
            Arg2.Write(out target.Item2);
            Arg3.Write(out target.Item3);
            Arg4.Write(out target.Item4);
            Arg5.Write(out target.Item5);
        }

        /// <summary>
        /// The value the pattern was successfully matched to
        /// </summary>
        public (T1, T2, T3, T4, T5) Value {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (Arg1.Value, Arg2.Value, Arg3.Value, Arg4.Value, Arg5.Value);
        }

        /// <summary>
        /// Attempt to match this pattern to a row, storing the relevant values into
        /// any Cells contained in the pattern, when those arguments are in write mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Match(in (T1, T2, T3, T4, T5) target)
            => Arg1.Match(in target.Item1) && Arg2.Match(in target.Item2) && Arg3.Match(in target.Item3)
               && Arg4.Match(in target.Item4) && Arg5.Match(in target.Item5);

        /// <inheritdoc />
        public bool IsInstantiated
            => Arg1.IsInstantiated && Arg2.IsInstantiated && Arg3.IsInstantiated && Arg4.IsInstantiated
               && Arg5.IsInstantiated;

        /// <inheritdoc />
        public bool IsReadModeAt(int index)
            => index switch
            {
                0 => Arg1.IsInstantiated,
                1 => Arg2.IsInstantiated,
                2 => Arg3.IsInstantiated,
                3 => Arg4.IsInstantiated,
                4 => Arg5.IsInstantiated,
                _ => throw new ArgumentException($"Pattern doesn't have an argument number {index}")
            };

        /// <inheritdoc />
        public ValueCell ArgumentCell(int index)
            => index switch
            {
                0 => Arg1.ValueCell,
                1 => Arg2.ValueCell,
                2 => Arg3.ValueCell,
                3 => Arg4.ValueCell,
                4 => Arg5.ValueCell,
                _ => throw new ArgumentException($"Pattern doesn't have an argument number {index}")
            };

        /// <inheritdoc />
        public override string ToString() => $"[{Arg1},{Arg2},{Arg3},{Arg4},{Arg5}]";

        private string DebuggerDisplay => ToString();

        /// <inheritdoc />
        public IMatchOperation[] Arguments => new[] { (IMatchOperation)Arg1, (IMatchOperation)Arg2, (IMatchOperation)Arg3, (IMatchOperation)Arg4, (IMatchOperation)Arg5 };
    }

    /// <summary>
    /// Describes how to match or write the value of the arguments in a Goal
    /// Compiled from the arguments to a Goal object
    /// </summary>
    /// <typeparam name="T1">Type of the goal's first argument</typeparam>
    /// <typeparam name="T2">Type of the goal's second argument</typeparam>
    /// <typeparam name="T3">Type of the goal's third argument</typeparam>
    /// <typeparam name="T4">Type of the goal's fourth argument</typeparam>
    /// <typeparam name="T5">Type of the goal's fifth argument</typeparam>
    /// <typeparam name="T6">Type of the goal's sixth argument</typeparam>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public readonly struct Pattern<T1, T2, T3, T4, T5, T6> : IPattern {
        /// <summary>
        /// First argument
        /// </summary>
        public readonly MatchOperation<T1> Arg1;
        /// <summary>
        /// Second argument
        /// </summary>
        public readonly MatchOperation<T2> Arg2;
        /// <summary>
        /// Third argument
        /// </summary>
        public readonly MatchOperation<T3> Arg3;
        /// <summary>
        /// Fourth argument
        /// </summary>
        public readonly MatchOperation<T4> Arg4;
        /// <summary>
        /// Fifth argument
        /// </summary>
        public readonly MatchOperation<T5> Arg5;
        /// <summary>
        /// Sixth argument
        /// </summary>
        public readonly MatchOperation<T6> Arg6;
        
        /// <summary>
        /// Make a pattern with the specified arguments
        /// </summary>
        public Pattern(MatchOperation<T1> arg1, MatchOperation<T2> arg2, MatchOperation<T3> arg3, MatchOperation<T4> arg4, MatchOperation<T5> arg5, MatchOperation<T6> arg6) {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
            Arg6 = arg6;
        }

        //public IEnumerable<int> InstantiatedPositions {
        //    get {
        //        if (Arg1.IsInstantiated) yield return 0;
        //        if (Arg2.IsInstantiated) yield return 1;
        //        if (Arg3.IsInstantiated) yield return 2;
        //        if (Arg4.IsInstantiated) yield return 3;
        //        if (Arg5.IsInstantiated) yield return 4;
        //        if (Arg6.IsInstantiated) yield return 5;
        //    }
        //}

        /// <summary>
        /// Write the values from the pattern out to a row in a table after a successful match
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(out (T1, T2, T3, T4, T5, T6) target) {
            Arg1.Write(out target.Item1);
            Arg2.Write(out target.Item2);
            Arg3.Write(out target.Item3);
            Arg4.Write(out target.Item4);
            Arg5.Write(out target.Item5);
            Arg6.Write(out target.Item6);
        }

        /// <summary>
        /// The value the pattern was successfully matched to
        /// </summary>
        public (T1, T2, T3, T4, T5, T6) Value {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (Arg1.Value, Arg2.Value, Arg3.Value, Arg4.Value, Arg5.Value, Arg6.Value);
        }

        /// <summary>
        /// Attempt to match this pattern to a row, storing the relevant values into
        /// any Cells contained in the pattern, when those arguments are in write mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Match(in (T1, T2, T3, T4, T5, T6) target)
            => Arg1.Match(in target.Item1) && Arg2.Match(in target.Item2) && Arg3.Match(in target.Item3)
               && Arg4.Match(in target.Item4) && Arg5.Match(in target.Item5) && Arg6.Match(in target.Item6);

        /// <inheritdoc />
        public bool IsInstantiated
            => Arg1.IsInstantiated && Arg2.IsInstantiated && Arg3.IsInstantiated && Arg4.IsInstantiated
               && Arg5.IsInstantiated && Arg6.IsInstantiated;

        /// <inheritdoc />
        public bool IsReadModeAt(int index)
            => index switch
            {
                0 => Arg1.IsInstantiated,
                1 => Arg2.IsInstantiated,
                2 => Arg3.IsInstantiated,
                3 => Arg4.IsInstantiated,
                4 => Arg5.IsInstantiated,
                5 => Arg6.IsInstantiated,
                _ => throw new ArgumentException($"Pattern doesn't have an argument number {index}")
            };

        /// <inheritdoc />
        public ValueCell ArgumentCell(int index)
            => index switch
            {
                0 => Arg1.ValueCell,
                1 => Arg2.ValueCell,
                2 => Arg3.ValueCell,
                3 => Arg4.ValueCell,
                4 => Arg5.ValueCell,
                5 => Arg6.ValueCell,
                _ => throw new ArgumentException($"Pattern doesn't have an argument number {index}")
            };

        /// <inheritdoc />
        public override string ToString() => $"[{Arg1},{Arg2},{Arg3},{Arg4},{Arg5},{Arg6}]";

        private string DebuggerDisplay => ToString();

        /// <inheritdoc />
        public IMatchOperation[] Arguments => new[] { (IMatchOperation)Arg1, (IMatchOperation)Arg2, (IMatchOperation)Arg3, (IMatchOperation)Arg4, (IMatchOperation)Arg5, (IMatchOperation)Arg6 };
    }

    /// <summary>
    /// Describes how to match or write the value of the arguments in a Goal
    /// Compiled from the arguments to a Goal object
    /// </summary>
    /// <typeparam name="T1">Type of the goal's first argument</typeparam>
    /// <typeparam name="T2">Type of the goal's second argument</typeparam>
    /// <typeparam name="T3">Type of the goal's third argument</typeparam>
    /// <typeparam name="T4">Type of the goal's fourth argument</typeparam>
    /// <typeparam name="T5">Type of the goal's fifth argument</typeparam>
    /// <typeparam name="T6">Type of the goal's sixth argument</typeparam>
    /// <typeparam name="T7">Type of the goal's seventh argument</typeparam>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public readonly struct Pattern<T1, T2, T3, T4, T5, T6, T7> : IPattern {
        /// <summary>
        /// First argument
        /// </summary>
        public readonly MatchOperation<T1> Arg1;
        /// <summary>
        /// Second argument
        /// </summary>
        public readonly MatchOperation<T2> Arg2;
        /// <summary>
        /// Third argument
        /// </summary>
        public readonly MatchOperation<T3> Arg3;
        /// <summary>
        /// Fourth argument
        /// </summary>
        public readonly MatchOperation<T4> Arg4;
        /// <summary>
        /// Fifth argument
        /// </summary>
        public readonly MatchOperation<T5> Arg5;
        /// <summary>
        /// Sixth argument
        /// </summary>
        public readonly MatchOperation<T6> Arg6;
        /// <summary>
        /// Seventh argument
        /// </summary>
        public readonly MatchOperation<T7> Arg7;

        /// <summary>
        /// Make a pattern with the specified arguments
        /// </summary>
        public Pattern(MatchOperation<T1> arg1, MatchOperation<T2> arg2, MatchOperation<T3> arg3, MatchOperation<T4> arg4, MatchOperation<T5> arg5, MatchOperation<T6> arg6, MatchOperation<T7> arg7) {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
            Arg6 = arg6;
            Arg7 = arg7;
        }

        //public IEnumerable<int> InstantiatedPositions {
        //    get {
        //        if (Arg1.IsInstantiated) yield return 0;
        //        if (Arg2.IsInstantiated) yield return 1;
        //        if (Arg3.IsInstantiated) yield return 2;
        //        if (Arg4.IsInstantiated) yield return 3;
        //        if (Arg5.IsInstantiated) yield return 4;
        //        if (Arg6.IsInstantiated) yield return 5;
        //        if (Arg7.IsInstantiated) yield return 6;
        //    }
        //}

        /// <summary>
        /// Write the values from the pattern out to a row in a table after a successful match
        /// </summary>
        public void Write(out (T1, T2, T3, T4, T5, T6, T7) target) {
            Arg1.Write(out target.Item1);
            Arg2.Write(out target.Item2);
            Arg3.Write(out target.Item3);
            Arg4.Write(out target.Item4);
            Arg5.Write(out target.Item5);
            Arg6.Write(out target.Item6);
            Arg7.Write(out target.Item7);
        }

        /// <summary>
        /// The value the pattern was successfully matched to
        /// </summary>
        public (T1, T2, T3, T4, T5, T6, T7) Value
            => (Arg1.Value, Arg2.Value, Arg3.Value, Arg4.Value, Arg5.Value, Arg6.Value, Arg7.Value);

        /// <summary>
        /// Attempt to match this pattern to a row, storing the relevant values into
        /// any Cells contained in the pattern, when those arguments are in write mode.
        /// </summary>
        public bool Match(in (T1, T2, T3, T4, T5, T6, T7) target)
            => Arg1.Match(in target.Item1) && Arg2.Match(in target.Item2) && Arg3.Match(in target.Item3)
               && Arg4.Match(in target.Item4) && Arg5.Match(in target.Item5) && Arg6.Match(in target.Item6)
               && Arg7.Match(in target.Item7);

        /// <inheritdoc />
        public bool IsInstantiated
            => Arg1.IsInstantiated && Arg2.IsInstantiated && Arg3.IsInstantiated && Arg4.IsInstantiated
               && Arg5.IsInstantiated && Arg6.IsInstantiated && Arg7.IsInstantiated;

        /// <inheritdoc />
        public bool IsReadModeAt(int index)
            => index switch
            {
                0 => Arg1.IsInstantiated,
                1 => Arg2.IsInstantiated,
                2 => Arg3.IsInstantiated,
                3 => Arg4.IsInstantiated,
                4 => Arg5.IsInstantiated,
                5 => Arg6.IsInstantiated,
                6 => Arg7.IsInstantiated,
                _ => throw new ArgumentException($"Pattern doesn't have an argument number {index}")
            };

        /// <inheritdoc />
        public ValueCell ArgumentCell(int index)
            => index switch
            {
                0 => Arg1.ValueCell,
                1 => Arg2.ValueCell,
                2 => Arg3.ValueCell,
                3 => Arg4.ValueCell,
                4 => Arg5.ValueCell,
                5 => Arg6.ValueCell,
                6 => Arg7.ValueCell,
                _ => throw new ArgumentException($"Pattern doesn't have an argument number {index}")
            };

        /// <inheritdoc />
        public override string ToString() => $"[{Arg1},{Arg2},{Arg3},{Arg4},{Arg5},{Arg6},{Arg7}]";

        private string DebuggerDisplay => ToString();

        /// <inheritdoc />
        public IMatchOperation[] Arguments => new[] { (IMatchOperation)Arg1, (IMatchOperation)Arg2, (IMatchOperation)Arg3, (IMatchOperation)Arg4,
            (IMatchOperation)Arg5, (IMatchOperation)Arg6, (IMatchOperation)Arg7 };
    }

    /// <summary>
    /// Describes how to match or write the value of the arguments in a Goal
    /// Compiled from the arguments to a Goal object
    /// </summary>
    /// <typeparam name="T1">Type of the goal's first argument</typeparam>
    /// <typeparam name="T2">Type of the goal's second argument</typeparam>
    /// <typeparam name="T3">Type of the goal's third argument</typeparam>
    /// <typeparam name="T4">Type of the goal's fourth argument</typeparam>
    /// <typeparam name="T5">Type of the goal's fifth argument</typeparam>
    /// <typeparam name="T6">Type of the goal's sixth argument</typeparam>
    /// <typeparam name="T7">Type of the goal's seventh argument</typeparam>
    /// <typeparam name="T8">Type of the goal's either argument</typeparam>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public readonly struct Pattern<T1, T2, T3, T4, T5, T6, T7, T8> : IPattern {
        /// <summary>
        /// First argument
        /// </summary>
        public readonly MatchOperation<T1> Arg1;
        /// <summary>
        /// Second argument
        /// </summary>
        public readonly MatchOperation<T2> Arg2;
        /// <summary>
        /// Third argument
        /// </summary>
        public readonly MatchOperation<T3> Arg3;
        /// <summary>
        /// Fourth argument
        /// </summary>
        public readonly MatchOperation<T4> Arg4;
        /// <summary>
        /// Fifth argument
        /// </summary>
        public readonly MatchOperation<T5> Arg5;
        /// <summary>
        /// Sixth argument
        /// </summary>
        public readonly MatchOperation<T6> Arg6;
        /// <summary>
        /// Seventh argument
        /// </summary>
        public readonly MatchOperation<T7> Arg7;
        /// <summary>
        /// Eighth argument
        /// </summary>
        public readonly MatchOperation<T8> Arg8;

        /// <summary>
        /// Make a pattern with the specified arguments
        /// </summary>
        public Pattern(MatchOperation<T1> arg1, MatchOperation<T2> arg2, MatchOperation<T3> arg3, MatchOperation<T4> arg4, MatchOperation<T5> arg5, MatchOperation<T6> arg6, MatchOperation<T7> arg7, MatchOperation<T8> arg8) {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
            Arg6 = arg6;
            Arg7 = arg7;
            Arg8 = arg8;
        }

        //public IEnumerable<int> InstantiatedPositions {
        //    get {
        //        if (Arg1.IsInstantiated) yield return 0;
        //        if (Arg2.IsInstantiated) yield return 1;
        //        if (Arg3.IsInstantiated) yield return 2;
        //        if (Arg4.IsInstantiated) yield return 3;
        //        if (Arg5.IsInstantiated) yield return 4;
        //        if (Arg6.IsInstantiated) yield return 5;
        //        if (Arg7.IsInstantiated) yield return 6;
        //        if (Arg8.IsInstantiated) yield return 7;
        //    }
        //}

        /// <summary>
        /// Write the values from the pattern out to a row in a table after a successful match
        /// </summary>
        public void Write(out (T1, T2, T3, T4, T5, T6, T7, T8) target) {
            Arg1.Write(out target.Item1);
            Arg2.Write(out target.Item2);
            Arg3.Write(out target.Item3);
            Arg4.Write(out target.Item4);
            Arg5.Write(out target.Item5);
            Arg6.Write(out target.Item6);
            Arg7.Write(out target.Item7);
            Arg8.Write(out target.Item8);
        }

        /// <summary>
        /// The value the pattern was successfully matched to
        /// </summary>
        public (T1, T2, T3, T4, T5, T6, T7, T8) Value
            => (Arg1.Value, Arg2.Value, Arg3.Value, Arg4.Value, Arg5.Value, Arg6.Value, Arg7.Value,
                    Arg8.Value);

        /// <summary>
        /// Attempt to match this pattern to a row, storing the relevant values into
        /// any Cells contained in the pattern, when those arguments are in write mode.
        /// </summary>
        public bool Match(in (T1, T2, T3, T4, T5, T6, T7, T8) target)
            => Arg1.Match(in target.Item1) && Arg2.Match(in target.Item2) && Arg3.Match(in target.Item3)
               && Arg4.Match(in target.Item4) && Arg5.Match(in target.Item5) && Arg6.Match(in target.Item6)
               && Arg7.Match(in target.Item7) && Arg8.Match(in target.Item8);

        /// <inheritdoc />
        public bool IsInstantiated
            => Arg1.IsInstantiated && Arg2.IsInstantiated && Arg3.IsInstantiated && Arg4.IsInstantiated
               && Arg5.IsInstantiated && Arg6.IsInstantiated && Arg7.IsInstantiated && Arg8.IsInstantiated;

        /// <inheritdoc />
        public bool IsReadModeAt(int index)
            => index switch
            {
                0 => Arg1.IsInstantiated,
                1 => Arg2.IsInstantiated,
                2 => Arg3.IsInstantiated,
                3 => Arg4.IsInstantiated,
                4 => Arg5.IsInstantiated,
                5 => Arg6.IsInstantiated,
                6 => Arg7.IsInstantiated,
                7 => Arg8.IsInstantiated,
                _ => throw new ArgumentException($"Pattern doesn't have an argument number {index}")
            };

        /// <inheritdoc />
        public ValueCell ArgumentCell(int index)
            => index switch
            {
                0 => Arg1.ValueCell,
                1 => Arg2.ValueCell,
                2 => Arg3.ValueCell,
                3 => Arg4.ValueCell,
                4 => Arg5.ValueCell,
                5 => Arg6.ValueCell,
                6 => Arg7.ValueCell,
                7 => Arg8.ValueCell,
                _ => throw new ArgumentException($"Pattern doesn't have an argument number {index}")
            };

        /// <inheritdoc />
        public override string ToString() => $"[{Arg1},{Arg2},{Arg3},{Arg4},{Arg5},{Arg6},{Arg7},{Arg8}]";

        private string DebuggerDisplay => ToString();

        /// <inheritdoc />
        public IMatchOperation[] Arguments => new[] { (IMatchOperation)Arg1, (IMatchOperation)Arg2, (IMatchOperation)Arg3, (IMatchOperation)Arg4,
            (IMatchOperation)Arg5, (IMatchOperation)Arg6, (IMatchOperation)Arg7, (IMatchOperation)Arg8 };
    }

    //[DebuggerDisplay("{DebuggerDisplay}")]
    //public readonly struct Pattern<T1, T2, T3, T4, T5, T6, T7, T8, T9> : IPattern {
    //    public readonly MatchOperation<T1> Arg1;
    //    public readonly MatchOperation<T2> Arg2;
    //    public readonly MatchOperation<T3> Arg3;
    //    public readonly MatchOperation<T4> Arg4;
    //    public readonly MatchOperation<T5> Arg5;
    //    public readonly MatchOperation<T6> Arg6;
    //    public readonly MatchOperation<T7> Arg7;
    //    public readonly MatchOperation<T8> Arg8;
    //    public readonly MatchOperation<T9> Arg9;

    //    /// <summary>
    //    /// Make a pattern with the specified arguments
    //    /// </summary>
    //    public Pattern(MatchOperation<T1> arg1, MatchOperation<T2> arg2, MatchOperation<T3> arg3, MatchOperation<T4> arg4, MatchOperation<T5> arg5, MatchOperation<T6> arg6, MatchOperation<T7> arg7, MatchOperation<T8> arg8, MatchOperation<T9> arg9) {
    //        Arg1 = arg1;
    //        Arg2 = arg2;
    //        Arg3 = arg3;
    //        Arg4 = arg4;
    //        Arg5 = arg5;
    //        Arg6 = arg6;
    //        Arg7 = arg7;
    //        Arg8 = arg8;
    //        Arg9 = arg9;
    //    }

    //    public IEnumerable<int> InstantiatedPositions {
    //        get {
    //            if (Arg1.IsInstantiated) yield return 0;
    //            if (Arg2.IsInstantiated) yield return 1;
    //            if (Arg3.IsInstantiated) yield return 2;
    //            if (Arg4.IsInstantiated) yield return 3;
    //            if (Arg5.IsInstantiated) yield return 4;
    //            if (Arg6.IsInstantiated) yield return 5;
    //            if (Arg7.IsInstantiated) yield return 6;
    //            if (Arg8.IsInstantiated) yield return 7;
    //            if (Arg9.IsInstantiated) yield return 8;
    //        }
    //    }

    //    public void Write(out (T1, T2, T3, T4, T5, T6, T7, T8, T9) target) {
    //        Arg1.Write(out target.Item1);
    //        Arg2.Write(out target.Item2);
    //        Arg3.Write(out target.Item3);
    //        Arg4.Write(out target.Item4);
    //        Arg5.Write(out target.Item5);
    //        Arg6.Write(out target.Item6);
    //        Arg7.Write(out target.Item7);
    //        Arg8.Write(out target.Item8);
    //        Arg9.Write(out target.Item9);
    //    }

    //    public (T1, T2, T3, T4, T5, T6, T7, T8, T9) Value
    //        => (Arg1.Value, Arg2.Value, Arg3.Value, Arg4.Value, Arg5.Value, Arg6.Value, Arg7.Value,
    //            Arg8.Value, Arg9.Value);

    //    public bool Match(in (T1, T2, T3, T4, T5, T6, T7, T8, T9) target)
    //        => Arg1.Match(in target.Item1) && Arg2.Match(in target.Item2) && Arg3.Match(in target.Item3)
    //           && Arg4.Match(in target.Item4) && Arg5.Match(in target.Item5) && Arg6.Match(in target.Item6)
    //           && Arg7.Match(in target.Item7) && Arg8.Match(in target.Item8) && Arg9.Match(in target.Item9);

    //    public bool IsInstantiated
    //        => Arg1.IsInstantiated && Arg2.IsInstantiated && Arg3.IsInstantiated && Arg4.IsInstantiated
    //           && Arg5.IsInstantiated && Arg6.IsInstantiated && Arg7.IsInstantiated
    //           && Arg8.IsInstantiated && Arg9.IsInstantiated;

    //    public override string ToString() => $"[{Arg1},{Arg2},{Arg3},{Arg4},{Arg5},{Arg6},{Arg7},{Arg8},{Arg9}]";

    //    private string DebuggerDisplay => ToString();
    //}

    //[DebuggerDisplay("{DebuggerDisplay}")]
    //public readonly struct Pattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : IPattern {
    //    public readonly MatchOperation<T1> Arg1;
    //    public readonly MatchOperation<T2> Arg2;
    //    public readonly MatchOperation<T3> Arg3;
    //    public readonly MatchOperation<T4> Arg4;
    //    public readonly MatchOperation<T5> Arg5;
    //    public readonly MatchOperation<T6> Arg6;
    //    public readonly MatchOperation<T7> Arg7;
    //    public readonly MatchOperation<T8> Arg8;
    //    public readonly MatchOperation<T9> Arg9;
    //    public readonly MatchOperation<T10> Arg10;

    //    /// <summary>
    //    /// Make a pattern with the specified arguments
    //    /// </summary>
    //    public Pattern(MatchOperation<T1> arg1, MatchOperation<T2> arg2, MatchOperation<T3> arg3, MatchOperation<T4> arg4, MatchOperation<T5> arg5, MatchOperation<T6> arg6, MatchOperation<T7> arg7, MatchOperation<T8> arg8, MatchOperation<T9> arg9, MatchOperation<T10> arg10) {
    //        Arg1 = arg1;
    //        Arg2 = arg2;
    //        Arg3 = arg3;
    //        Arg4 = arg4;
    //        Arg5 = arg5;
    //        Arg6 = arg6;
    //        Arg7 = arg7;
    //        Arg8 = arg8;
    //        Arg9 = arg9;
    //        Arg10 = arg10;
    //    }

    //    public IEnumerable<int> InstantiatedPositions {
    //        get {
    //            if (Arg1.IsInstantiated) yield return 0;
    //            if (Arg2.IsInstantiated) yield return 1;
    //            if (Arg3.IsInstantiated) yield return 2;
    //            if (Arg4.IsInstantiated) yield return 3;
    //            if (Arg5.IsInstantiated) yield return 4;
    //            if (Arg6.IsInstantiated) yield return 5;
    //            if (Arg7.IsInstantiated) yield return 6;
    //            if (Arg8.IsInstantiated) yield return 7;
    //            if (Arg9.IsInstantiated) yield return 8;
    //            if (Arg10.IsInstantiated) yield return 9;
    //        }
    //    }

    //    public void Write(out (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10) target) {
    //        Arg1.Write(out target.Item1);
    //        Arg2.Write(out target.Item2);
    //        Arg3.Write(out target.Item3);
    //        Arg4.Write(out target.Item4);
    //        Arg5.Write(out target.Item5);
    //        Arg6.Write(out target.Item6);
    //        Arg7.Write(out target.Item7);
    //        Arg8.Write(out target.Item8);
    //        Arg9.Write(out target.Item9);
    //        Arg10.Write(out target.Item10);
    //    }

    //    public (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10) Value
    //        => (Arg1.Value, Arg2.Value, Arg3.Value, Arg4.Value, Arg5.Value, Arg6.Value, Arg7.Value,
    //            Arg8.Value, Arg9.Value, Arg10.Value);

    //    public bool Match(in (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10) target)
    //        => Arg1.Match(in target.Item1) && Arg2.Match(in target.Item2) && Arg3.Match(in target.Item3)
    //           && Arg4.Match(in target.Item4) && Arg5.Match(in target.Item5) && Arg6.Match(in target.Item6)
    //           && Arg7.Match(in target.Item7) && Arg8.Match(in target.Item8) && Arg9.Match(in target.Item9)
    //           && Arg10.Match(in target.Item10);

    //    public bool IsInstantiated
    //        => Arg1.IsInstantiated && Arg2.IsInstantiated && Arg3.IsInstantiated && Arg4.IsInstantiated
    //           && Arg5.IsInstantiated && Arg6.IsInstantiated && Arg7.IsInstantiated
    //           && Arg8.IsInstantiated && Arg9.IsInstantiated && Arg10.IsInstantiated;

    //    public override string ToString() => $"[{Arg1},{Arg2},{Arg3},{Arg4},{Arg5},{Arg6},{Arg7},{Arg8},{Arg9}.{Arg10}]";

    //    private string DebuggerDisplay => ToString();
    //}
}
