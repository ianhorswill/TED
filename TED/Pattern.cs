using System.Collections.Generic;
using System.Diagnostics;

namespace TED
{
    /// <summary>
    /// Describes how to match or write the value of the arguments in a Goal
    /// Compiled from the arguments to a Goal object
    /// </summary>
    /// <typeparam name="T1">Type of the goal's argument</typeparam>
    [DebuggerDisplay("{DebuggerDisplay}")]
    internal readonly struct Pattern<T1> : IPattern
    {
        public readonly MatchOperation<T1> Arg1;

        public Pattern(MatchOperation<T1> arg1)
        {
            Arg1 = arg1;
        }

        public void Write(out T1 target)
        {
            Arg1.Write(out target);
        }

        public T1 Value => Arg1.Value;

        public bool Match(in T1 target) => Arg1.Match(target);

        public bool IsInstantiated => Arg1.IsInstantiated;

        public override string ToString() => $"[{Arg1}]";

        private string DebuggerDisplay => ToString();
    }

    /// <summary>
    /// Describes how to match or write the value of the arguments in a Goal
    /// Compiled from the arguments to a Goal object
    /// </summary>
    /// <typeparam name="T1">Type of the goal's first argument</typeparam>
    /// <typeparam name="T2">Type of the goal's second argument</typeparam>
    [DebuggerDisplay("{DebuggerDisplay}")]
    internal readonly struct Pattern<T1, T2> : IPattern
    {
        public readonly MatchOperation<T1> Arg1;
        public readonly MatchOperation<T2> Arg2;

        public Pattern(MatchOperation<T1> arg1, MatchOperation<T2> arg2)
        {
            Arg1 = arg1;
            Arg2 = arg2;
        }

        public void Write(out (T1, T2) target)
        {
            Arg1.Write(out target.Item1);
            Arg2.Write(out target.Item2);
        }

        public (T1, T2) Value => (Arg1.Value, Arg2.Value);

        public bool Match(in (T1,T2) target) => Arg1.Match(target.Item1) && Arg2.Match(target.Item2);

        public bool IsInstantiated => Arg1.IsInstantiated && Arg2.IsInstantiated;

        public override string ToString() => $"[{Arg1},{Arg2}]";

        private string DebuggerDisplay => ToString();
    }

    /// <summary>
    /// Describes how to match or write the value of the arguments in a Goal
    /// Compiled from the arguments to a Goal object
    /// </summary>
    /// <typeparam name="T1">Type of the goal's first argument</typeparam>
    /// <typeparam name="T2">Type of the goal's second argument</typeparam>
    /// <typeparam name="T3">Type of the goal's third argument</typeparam>
    [DebuggerDisplay("{DebuggerDisplay}")]
    internal readonly struct Pattern<T1, T2, T3> : IPattern
    {
        public readonly MatchOperation<T1> Arg1;
        public readonly MatchOperation<T2> Arg2;
        public readonly MatchOperation<T3> Arg3;

        public Pattern(MatchOperation<T1> arg1, MatchOperation<T2> arg2, MatchOperation<T3> arg3)
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
        }

        public void Write(out (T1, T2, T3) target)
        {
            Arg1.Write(out target.Item1);
            Arg2.Write(out target.Item2);
            Arg3.Write(out target.Item3);
        }

        public (T1, T2, T3) Value => (Arg1.Value, Arg2.Value, Arg3.Value);

        public bool Match(in (T1, T2, T3) target)
            => Arg1.Match(target.Item1) && Arg2.Match(target.Item2) && Arg3.Match(target.Item3);

        public bool IsInstantiated => Arg1.IsInstantiated && Arg2.IsInstantiated && Arg3.IsInstantiated;

        public override string ToString() => $"[{Arg1},{Arg2},{Arg3}]";

        private string DebuggerDisplay => ToString();
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
    internal readonly struct Pattern<T1, T2, T3, T4> : IPattern
    {
        public readonly MatchOperation<T1> Arg1;
        public readonly MatchOperation<T2> Arg2;
        public readonly MatchOperation<T3> Arg3;
        public readonly MatchOperation<T4> Arg4;

        public Pattern(MatchOperation<T1> arg1, MatchOperation<T2> arg2, MatchOperation<T3> arg3, MatchOperation<T4> arg4)
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
        }
        
        public void Write(out (T1, T2, T3, T4) target)
        {
            Arg1.Write(out target.Item1);
            Arg2.Write(out target.Item2);
            Arg3.Write(out target.Item3);
            Arg4.Write(out target.Item4);
        }

        public (T1, T2, T3, T4) Value => (Arg1.Value, Arg2.Value, Arg3.Value, Arg4.Value);

        public bool Match(in (T1, T2, T3, T4) target)
            => Arg1.Match(target.Item1) && Arg2.Match(target.Item2) && Arg3.Match(target.Item3)
               && Arg4.Match(target.Item4);

        public bool IsInstantiated
            => Arg1.IsInstantiated && Arg2.IsInstantiated && Arg3.IsInstantiated && Arg4.IsInstantiated;
        
        public override string ToString() => $"[{Arg1},{Arg2},{Arg3},{Arg4}]";

        private string DebuggerDisplay => ToString();
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
    internal readonly struct Pattern<T1, T2, T3, T4, T5> : IPattern
    {
        public readonly MatchOperation<T1> Arg1;
        public readonly MatchOperation<T2> Arg2;
        public readonly MatchOperation<T3> Arg3;
        public readonly MatchOperation<T4> Arg4;
        public readonly MatchOperation<T5> Arg5;

        public Pattern(MatchOperation<T1> arg1, MatchOperation<T2> arg2, MatchOperation<T3> arg3, MatchOperation<T4> arg4, MatchOperation<T5> arg5)
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
        }

        public void Write(out (T1, T2, T3, T4, T5) target)
        {
            Arg1.Write(out target.Item1);
            Arg2.Write(out target.Item2);
            Arg3.Write(out target.Item3);
            Arg4.Write(out target.Item4);
            Arg5.Write(out target.Item5);
        }

        public (T1, T2, T3, T4, T5) Value => (Arg1.Value, Arg2.Value, Arg3.Value, Arg4.Value, Arg5.Value);

        public bool Match(in (T1, T2, T3, T4, T5) target)
            => Arg1.Match(target.Item1) && Arg2.Match(target.Item2) && Arg3.Match(target.Item3)
               && Arg4.Match(target.Item4) && Arg5.Match(target.Item5);

        public bool IsInstantiated
            => Arg1.IsInstantiated && Arg2.IsInstantiated && Arg3.IsInstantiated && Arg4.IsInstantiated
               && Arg5.IsInstantiated;
        
        public override string ToString() => $"[{Arg1},{Arg2},{Arg3},{Arg4},{Arg5}]";

        private string DebuggerDisplay => ToString();
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
    internal readonly struct Pattern<T1, T2, T3, T4, T5, T6> : IPattern
    {
        public readonly MatchOperation<T1> Arg1;
        public readonly MatchOperation<T2> Arg2;
        public readonly MatchOperation<T3> Arg3;
        public readonly MatchOperation<T4> Arg4;
        public readonly MatchOperation<T5> Arg5;
        public readonly MatchOperation<T6> Arg6;

        public Pattern(MatchOperation<T1> arg1, MatchOperation<T2> arg2, MatchOperation<T3> arg3, MatchOperation<T4> arg4, MatchOperation<T5> arg5, MatchOperation<T6> arg6)
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
            Arg6 = arg6;
        }

        public IEnumerable<int> InstantiatedPositions
        {
            get
            {
                if (Arg1.IsInstantiated) yield return 0;
                if (Arg2.IsInstantiated) yield return 1;
                if (Arg3.IsInstantiated) yield return 2;
                if (Arg4.IsInstantiated) yield return 3;
                if (Arg5.IsInstantiated) yield return 4;
                if (Arg6.IsInstantiated) yield return 5;
            }
        }

        public void Write(out (T1, T2, T3, T4, T5, T6) target)
        {
            Arg1.Write(out target.Item1);
            Arg2.Write(out target.Item2);
            Arg3.Write(out target.Item3);
            Arg4.Write(out target.Item4);
            Arg5.Write(out target.Item5);
            Arg6.Write(out target.Item6);
        }

        public (T1, T2, T3, T4, T5, T6) Value
            => (Arg1.Value, Arg2.Value, Arg3.Value, Arg4.Value, Arg5.Value, Arg6.Value);

        public bool Match(in (T1, T2, T3, T4, T5, T6) target)
            => Arg1.Match(target.Item1) && Arg2.Match(target.Item2) && Arg3.Match(target.Item3)
               && Arg4.Match(target.Item4) && Arg5.Match(target.Item5) && Arg6.Match(target.Item6);

        public bool IsInstantiated
            => Arg1.IsInstantiated && Arg2.IsInstantiated && Arg3.IsInstantiated && Arg4.IsInstantiated
               && Arg5.IsInstantiated && Arg6.IsInstantiated;
        
        public override string ToString() => $"[{Arg1},{Arg2},{Arg3},{Arg4},{Arg5},{Arg6}]";

        private string DebuggerDisplay => ToString();
    }

    [DebuggerDisplay("{DebuggerDisplay}")]
    internal readonly struct Pattern<T1, T2, T3, T4, T5, T6, T7> : IPattern
    {
        private readonly MatchOperation<T1> arg1;
        private readonly MatchOperation<T2> arg2;
        private readonly MatchOperation<T3> arg3;
        private readonly MatchOperation<T4> arg4;
        private readonly MatchOperation<T5> arg5;
        private readonly MatchOperation<T6> arg6;
        private readonly MatchOperation<T7> arg7;

        public Pattern(MatchOperation<T1> arg1, MatchOperation<T2> arg2, MatchOperation<T3> arg3, MatchOperation<T4> arg4, MatchOperation<T5> arg5, MatchOperation<T6> arg6, MatchOperation<T7> arg7)
        {
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.arg3 = arg3;
            this.arg4 = arg4;
            this.arg5 = arg5;
            this.arg6 = arg6;
            this.arg7 = arg7;
        }

        public IEnumerable<int> InstantiatedPositions
        {
            get
            {
                if (arg1.IsInstantiated) yield return 0;
                if (arg2.IsInstantiated) yield return 1;
                if (arg3.IsInstantiated) yield return 2;
                if (arg4.IsInstantiated) yield return 3;
                if (arg5.IsInstantiated) yield return 4;
                if (arg6.IsInstantiated) yield return 5;
                if (arg7.IsInstantiated) yield return 6;
            }
        }

        public void Write(out (T1, T2, T3, T4, T5, T6, T7) target)
        {
            arg1.Write(out target.Item1);
            arg2.Write(out target.Item2);
            arg3.Write(out target.Item3);
            arg4.Write(out target.Item4);
            arg5.Write(out target.Item5);
            arg6.Write(out target.Item6);
            arg7.Write(out target.Item7);
        }

        public (T1, T2, T3, T4, T5, T6, T7) Value
            => (arg1.Value, arg2.Value, arg3.Value, arg4.Value, arg5.Value, arg6.Value, arg7.Value);

        public bool Match(in (T1, T2, T3, T4, T5, T6, T7) target)
            => arg1.Match(target.Item1) && arg2.Match(target.Item2) && arg3.Match(target.Item3)
               && arg4.Match(target.Item4) && arg5.Match(target.Item5) && arg6.Match(target.Item6)
               && arg7.Match(target.Item7);

        public bool IsInstantiated
            => arg1.IsInstantiated && arg2.IsInstantiated && arg3.IsInstantiated && arg4.IsInstantiated
               && arg5.IsInstantiated && arg6.IsInstantiated && arg7.IsInstantiated;
        
        public override string ToString() => $"[{arg1},{arg2},{arg3},{arg4},{arg5},{arg6},{arg7}]";

        private string DebuggerDisplay => ToString();
    }

    [DebuggerDisplay("{DebuggerDisplay}")]
    internal readonly struct Pattern<T1, T2, T3, T4, T5, T6, T7, T8> : IPattern
    {
        private readonly MatchOperation<T1> arg1;
        private readonly MatchOperation<T2> arg2;
        private readonly MatchOperation<T3> arg3;
        private readonly MatchOperation<T4> arg4;
        private readonly MatchOperation<T5> arg5;
        private readonly MatchOperation<T6> arg6;
        private readonly MatchOperation<T7> arg7;
        private readonly MatchOperation<T8> arg8;

        public Pattern(MatchOperation<T1> arg1, MatchOperation<T2> arg2, MatchOperation<T3> arg3, MatchOperation<T4> arg4, MatchOperation<T5> arg5, MatchOperation<T6> arg6, MatchOperation<T7> arg7, MatchOperation<T8> arg8)
        {
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.arg3 = arg3;
            this.arg4 = arg4;
            this.arg5 = arg5;
            this.arg6 = arg6;
            this.arg7 = arg7;
            this.arg8 = arg8;
        }

        public IEnumerable<int> InstantiatedPositions
        {
            get
            {
                if (arg1.IsInstantiated) yield return 0;
                if (arg2.IsInstantiated) yield return 1;
                if (arg3.IsInstantiated) yield return 2;
                if (arg4.IsInstantiated) yield return 3;
                if (arg5.IsInstantiated) yield return 4;
                if (arg6.IsInstantiated) yield return 5;
                if (arg7.IsInstantiated) yield return 6;
                if (arg8.IsInstantiated) yield return 7;
            }
        }

        public void Write(out (T1, T2, T3, T4, T5, T6, T7, T8) target)
        {
            arg1.Write(out target.Item1);
            arg2.Write(out target.Item2);
            arg3.Write(out target.Item3);
            arg4.Write(out target.Item4);
            arg5.Write(out target.Item5);
            arg6.Write(out target.Item6);
            arg7.Write(out target.Item7);
            arg8.Write(out target.Item8);
        }

        public (T1, T2, T3, T4, T5, T6, T7, T8) Value
            => (arg1.Value, arg2.Value, arg3.Value, arg4.Value, arg5.Value, arg6.Value, arg7.Value,
                    arg8.Value);

        public bool Match(in (T1, T2, T3, T4, T5, T6, T7, T8) target)
            => arg1.Match(target.Item1) && arg2.Match(target.Item2) && arg3.Match(target.Item3)
               && arg4.Match(target.Item4) && arg5.Match(target.Item5) && arg6.Match(target.Item6)
               && arg7.Match(target.Item7) && arg8.Match(target.Item8);

        public bool IsInstantiated
            => arg1.IsInstantiated && arg2.IsInstantiated && arg3.IsInstantiated && arg4.IsInstantiated
               && arg5.IsInstantiated && arg6.IsInstantiated && arg7.IsInstantiated && arg8.IsInstantiated;
        
        public override string ToString() => $"[{arg1},{arg2},{arg3},{arg4},{arg5},{arg6},{arg7},{arg8}]";

        private string DebuggerDisplay => ToString();
    }

    [DebuggerDisplay("{DebuggerDisplay}")]
    internal readonly struct Pattern<T1, T2, T3, T4, T5, T6, T7, T8, T9> : IPattern
    {
        private readonly MatchOperation<T1> arg1;
        private readonly MatchOperation<T2> arg2;
        private readonly MatchOperation<T3> arg3;
        private readonly MatchOperation<T4> arg4;
        private readonly MatchOperation<T5> arg5;
        private readonly MatchOperation<T6> arg6;
        private readonly MatchOperation<T7> arg7;
        private readonly MatchOperation<T8> arg8;
        private readonly MatchOperation<T9> arg9;

        public Pattern(MatchOperation<T1> arg1, MatchOperation<T2> arg2, MatchOperation<T3> arg3, MatchOperation<T4> arg4, MatchOperation<T5> arg5, MatchOperation<T6> arg6, MatchOperation<T7> arg7, MatchOperation<T8> arg8, MatchOperation<T9> arg9)
        {
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.arg3 = arg3;
            this.arg4 = arg4;
            this.arg5 = arg5;
            this.arg6 = arg6;
            this.arg7 = arg7;
            this.arg8 = arg8;
            this.arg9 = arg9;
        }

        public IEnumerable<int> InstantiatedPositions
        {
            get
            {
                if (arg1.IsInstantiated) yield return 0;
                if (arg2.IsInstantiated) yield return 1;
                if (arg3.IsInstantiated) yield return 2;
                if (arg4.IsInstantiated) yield return 3;
                if (arg5.IsInstantiated) yield return 4;
                if (arg6.IsInstantiated) yield return 5;
                if (arg7.IsInstantiated) yield return 6;
                if (arg8.IsInstantiated) yield return 7;
                if (arg9.IsInstantiated) yield return 8;
            }
        }

        public void Write(out (T1, T2, T3, T4, T5, T6, T7, T8, T9) target)
        {
            arg1.Write(out target.Item1);
            arg2.Write(out target.Item2);
            arg3.Write(out target.Item3);
            arg4.Write(out target.Item4);
            arg5.Write(out target.Item5);
            arg6.Write(out target.Item6);
            arg7.Write(out target.Item7);
            arg8.Write(out target.Item8);
            arg9.Write(out target.Item9);
        }

        public (T1, T2, T3, T4, T5, T6, T7, T8, T9) Value
            => (arg1.Value, arg2.Value, arg3.Value, arg4.Value, arg5.Value, arg6.Value, arg7.Value,
                arg8.Value, arg9.Value);

        public bool Match(in (T1, T2, T3, T4, T5, T6, T7, T8, T9) target)
            => arg1.Match(target.Item1) && arg2.Match(target.Item2) && arg3.Match(target.Item3)
               && arg4.Match(target.Item4) && arg5.Match(target.Item5) && arg6.Match(target.Item6)
               && arg7.Match(target.Item7) && arg8.Match(target.Item8) && arg9.Match(target.Item9);

        public bool IsInstantiated
            => arg1.IsInstantiated && arg2.IsInstantiated && arg3.IsInstantiated && arg4.IsInstantiated
               && arg5.IsInstantiated && arg6.IsInstantiated && arg7.IsInstantiated
               && arg8.IsInstantiated && arg9.IsInstantiated;
        
        public override string ToString() => $"[{arg1},{arg2},{arg3},{arg4},{arg5},{arg6},{arg7},{arg8},{arg9}]";

        private string DebuggerDisplay => ToString();
    }

    [DebuggerDisplay("{DebuggerDisplay}")]
    internal readonly struct Pattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : IPattern
    {
        private readonly MatchOperation<T1> arg1;
        private readonly MatchOperation<T2> arg2;
        private readonly MatchOperation<T3> arg3;
        private readonly MatchOperation<T4> arg4;
        private readonly MatchOperation<T5> arg5;
        private readonly MatchOperation<T6> arg6;
        private readonly MatchOperation<T7> arg7;
        private readonly MatchOperation<T8> arg8;
        private readonly MatchOperation<T9> arg9;
        private readonly MatchOperation<T10> arg10;

        public Pattern(MatchOperation<T1> arg1, MatchOperation<T2> arg2, MatchOperation<T3> arg3, MatchOperation<T4> arg4, MatchOperation<T5> arg5, MatchOperation<T6> arg6, MatchOperation<T7> arg7, MatchOperation<T8> arg8, MatchOperation<T9> arg9, MatchOperation<T10> arg10)
        {
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.arg3 = arg3;
            this.arg4 = arg4;
            this.arg5 = arg5;
            this.arg6 = arg6;
            this.arg7 = arg7;
            this.arg8 = arg8;
            this.arg9 = arg9;
            this.arg10 = arg10;
        }

        public IEnumerable<int> InstantiatedPositions
        {
            get
            {
                if (arg1.IsInstantiated) yield return 0;
                if (arg2.IsInstantiated) yield return 1;
                if (arg3.IsInstantiated) yield return 2;
                if (arg4.IsInstantiated) yield return 3;
                if (arg5.IsInstantiated) yield return 4;
                if (arg6.IsInstantiated) yield return 5;
                if (arg7.IsInstantiated) yield return 6;
                if (arg8.IsInstantiated) yield return 7;
                if (arg9.IsInstantiated) yield return 8;
                if (arg10.IsInstantiated) yield return 9;
            }
        }

        public void Write(out (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10) target)
        {
            arg1.Write(out target.Item1);
            arg2.Write(out target.Item2);
            arg3.Write(out target.Item3);
            arg4.Write(out target.Item4);
            arg5.Write(out target.Item5);
            arg6.Write(out target.Item6);
            arg7.Write(out target.Item7);
            arg8.Write(out target.Item8);
            arg9.Write(out target.Item9);
            arg10.Write(out target.Item10);
        }

        public (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10) Value
            => (arg1.Value, arg2.Value, arg3.Value, arg4.Value, arg5.Value, arg6.Value, arg7.Value,
                arg8.Value, arg9.Value, arg10.Value);

        public bool Match(in (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10) target)
            => arg1.Match(target.Item1) && arg2.Match(target.Item2) && arg3.Match(target.Item3)
               && arg4.Match(target.Item4) && arg5.Match(target.Item5) && arg6.Match(target.Item6)
               && arg7.Match(target.Item7) && arg8.Match(target.Item8) && arg9.Match(target.Item9)
               && arg10.Match(target.Item10);

        public bool IsInstantiated
            => arg1.IsInstantiated && arg2.IsInstantiated && arg3.IsInstantiated && arg4.IsInstantiated
               && arg5.IsInstantiated && arg6.IsInstantiated && arg7.IsInstantiated
               && arg8.IsInstantiated && arg9.IsInstantiated && arg10.IsInstantiated;
        
        public override string ToString() => $"[{arg1},{arg2},{arg3},{arg4},{arg5},{arg6},{arg7},{arg8},{arg9}.{arg10}]";

        private string DebuggerDisplay => ToString();
    }
}
