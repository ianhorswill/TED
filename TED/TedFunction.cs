using System;
using System.Diagnostics;

namespace TED
{
    /// <summary>
    /// Untyped base class for objects that wrap C# functions
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public abstract class TedFunction
    {
        /// <summary>
        /// Name, for debugging purposes
        /// </summary>
        public readonly string Name;

        public TedFunction(string name)
        {
            Name = name;
        }

        public override string ToString() => Name;
    }

    /// <summary>
    /// A wrapper for a C# function to allow it to be called from TED expressions
    /// </summary>
    public class TedFunction<TOut> : TedFunction
    {
        internal readonly Func<TOut> Implementation;

        public TedFunction(string name, Func<TOut> implementation) : base(name)
        {
            Implementation = implementation;
        }

        public FunctionCall<TOut> Call() => new FunctionCall<TOut>(this);
    }

    /// <summary>
    /// A wrapper for a C# function to allow it to be called from TED expressions
    /// </summary>
    public class TedFunction<TIn, TOut> : TedFunction
    {
        
        internal readonly Func<TIn, TOut> Implementation;

        public TedFunction(string name, Func<TIn, TOut> implementation) : base(name)
        {
            Implementation = implementation;
        }

        public FunctionCall<TIn, TOut> this[Term<TIn> arg] => new FunctionCall<TIn, TOut>(this, arg);
    }

    /// <summary>
    /// A wrapper for a C# function to allow it to be called from TED expressions
    /// </summary>
    public class TedFunction<TIn1, TIn2, TOut> : TedFunction
    {
        
        internal readonly Func<TIn1, TIn2, TOut> Implementation;

        public TedFunction(string name, Func<TIn1, TIn2, TOut> implementation) : base(name)
        {
            Implementation = implementation;
        }

        public FunctionCall<TIn1, TIn2, TOut> this[Term<TIn1> arg1, Term<TIn2> arg2] => new FunctionCall<TIn1, TIn2, TOut>(this, arg1, arg2);
    }
}
