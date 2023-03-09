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

        public virtual bool RenderAsOperator => false;
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

    /// <summary>
    /// A wrapper for a C# function to allow it to be called from TED expressions
    /// </summary>
    public class TedFunction<TIn1, TIn2, TIn3, TOut> : TedFunction {

        internal readonly Func<TIn1, TIn2, TIn3, TOut> Implementation;

        public TedFunction(string name, Func<TIn1, TIn2, TIn3, TOut> implementation) : base(name) {
            Implementation = implementation;
        }

        public FunctionCall<TIn1, TIn2, TIn3, TOut> this[Term<TIn1> arg1, Term<TIn2> arg2, Term<TIn3> arg3] => 
            new FunctionCall<TIn1, TIn2, TIn3, TOut>(this, arg1, arg2, arg3);
    }

    /// <summary>
    /// A wrapper for a C# function to allow it to be called from TED expressions
    /// </summary>
    public class TedFunction<TIn1, TIn2, TIn3, TIn4, TOut> : TedFunction {

        internal readonly Func<TIn1, TIn2, TIn3, TIn4, TOut> Implementation;

        public TedFunction(string name, Func<TIn1, TIn2, TIn3, TIn4, TOut> implementation) : base(name) {
            Implementation = implementation;
        }

        public FunctionCall<TIn1, TIn2, TIn3, TIn4, TOut> this[Term<TIn1> arg1, Term<TIn2> arg2, Term<TIn3> arg3, Term<TIn4> arg4] =>
            new FunctionCall<TIn1, TIn2, TIn3, TIn4, TOut>(this, arg1, arg2, arg3, arg4);
    }

    /// <summary>
    /// A wrapper for a C# function to allow it to be called from TED expressions
    /// </summary>
    public class TedFunction<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> : TedFunction {

        internal readonly Func<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> Implementation;

        public TedFunction(string name, Func<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> implementation) : base(name) {
            Implementation = implementation;
        }

        public FunctionCall<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> this[Term<TIn1> arg1, Term<TIn2> arg2, Term<TIn3> arg3, Term<TIn4> arg4, Term<TIn5> arg5] =>
            new FunctionCall<TIn1, TIn2, TIn3, TIn4, TIn5, TOut>(this, arg1, arg2, arg3, arg4, arg5);
    }

    /// <summary>
    /// A wrapper for a C# function to allow it to be called from TED expressions
    /// </summary>
    public class TedFunction<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut> : TedFunction {

        internal readonly Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut> Implementation;

        public TedFunction(string name, Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut> implementation) : base(name) {
            Implementation = implementation;
        }

        public FunctionCall<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut> this[Term<TIn1> arg1, Term<TIn2> arg2, Term<TIn3> arg3, Term<TIn4> arg4, Term<TIn5> arg5, Term<TIn6> arg6] =>
            new FunctionCall<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut>(this, arg1, arg2, arg3, arg4, arg5, arg6);
    }

    /// <summary>
    /// A wrapper for a C# function to allow it to be called from TED expressions
    /// </summary>
    public class TedFunction<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut> : TedFunction {

        internal readonly Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut> Implementation;

        public TedFunction(string name, Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut> implementation) : base(name) {
            Implementation = implementation;
        }

        public FunctionCall<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut> this[Term<TIn1> arg1, Term<TIn2> arg2, Term<TIn3> arg3, Term<TIn4> arg4, Term<TIn5> arg5, Term<TIn6> arg6, Term<TIn7> arg7] =>
            new FunctionCall<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut>(this, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
    }
}