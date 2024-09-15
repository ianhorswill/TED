using System;
using System.Diagnostics;
using TED.Interpreter;
using TED.Primitives;

namespace TED {
    /// <summary>
    /// Untyped base class for objects that wrap C# functions
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public abstract class Function {
        /// <summary>
        /// Name, for debugging purposes
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// If defined, when compiling calls to this, the generated C# code will be a call to the C# function with this name.
        /// </summary>
        public string NameForCompilation;

        private readonly bool isPure;

        /// <summary>
        /// Make a TedFunction with the specified name
        /// </summary>
        protected Function(string name, bool isPure)
        {
            Name = name;
            this.isPure = isPure;
            NameForCompilation = name;
        }
        
        /// <inheritdoc />
        public override string ToString() => Name;

        /// <summary>
        /// True if calls to this function should be printed as an operator rather than in F(args) format.
        /// </summary>
        public virtual bool RenderAsOperator => false;

        /// <summary>
        /// True if the function always returns the same value for the same inputs, and has no side-effects.
        /// </summary>
        public virtual bool IsPure => isPure;

        public delegate string CustomCompiler(Compiler.Compiler compiler, IFunctionalExpression exp);

        /// <summary>
        /// Custom delegate for generating expression strings for this function given strings for its arguments
        /// </summary>
        public CustomCompiler? CustomExpressionCompiler;
    }

    /// <summary>
    /// A wrapper for a C# function to allow it to be called from TED expressions
    /// </summary>
    public class Function<TOut> : Function {
        internal readonly Func<TOut> Implementation;

        /// <summary>
        /// Make a new TedFunction to wrap the specified C# function
        /// </summary>
        public Function(string name, Func<TOut> implementation, bool isPure = true) : base(name, isPure) => Implementation = implementation;

        /// <summary>
        /// True when result is the result of this function.
        /// </summary>
        /// <param name="result">Output of this function</param>
        public Goal this[Term<TOut> result] => EvalPrimitive<TOut>.Singleton[result, Call()];

        /// <summary>
        /// Make a call to this parameterless function
        /// </summary>
        /// <returns></returns>
        public FunctionCall<TOut> Call() => new FunctionCall<TOut>(this);
    }

    /// <summary>
    /// A wrapper for a C# function to allow it to be called from TED expressions
    /// </summary>
    public class Function<TIn, TOut> : Function {
        internal readonly Func<TIn, TOut> Implementation;

        /// <summary>
        /// Make a new TedFunction to wrap the specified C# function
        /// </summary>
        public Function(string name, Func<TIn, TOut> implementation, bool isPure = true) : base(name, isPure) => Implementation = implementation;

        /// <summary>
        /// Make a call to the function
        /// </summary>
        public FunctionCall<TIn, TOut> this[Term<TIn> arg] => new FunctionCall<TIn, TOut>(this, arg);

        /// <summary>
        /// True when result is the result of this function.
        /// </summary>
        /// <param name="arg1">First input to function</param>
        /// <param name="result">Output of this function</param>
        public Goal this[Term<TIn> arg1, Term<TOut> result] => EvalPrimitive<TOut>.Singleton[result, this[arg1]];
    }

    /// <summary>
    /// A wrapper for a C# function to allow it to be called from TED expressions
    /// </summary>
    public class Function<TIn1, TIn2, TOut> : Function {
        internal readonly Func<TIn1, TIn2, TOut> Implementation;

        /// <summary>
        /// Make a new TedFunction to wrap the specified C# function
        /// </summary>
        public Function(string name, Func<TIn1, TIn2, TOut> implementation, bool isPure = true) : base(name, isPure) => Implementation = implementation;

        /// <summary>
        /// Make a call to the function
        /// </summary>
        public FunctionCall<TIn1, TIn2, TOut> this[Term<TIn1> arg1, Term<TIn2> arg2] => new FunctionCall<TIn1, TIn2, TOut>(this, arg1, arg2);

        /// <summary>
        /// True when result is the result of this function.
        /// </summary>
        /// <param name="arg1">First input to function</param>
        /// <param name="arg2">Second input</param>
        /// <param name="result">Output of this function</param>
        public Goal this[Term<TIn1> arg1, Term<TIn2> arg2, Term<TOut> result] => EvalPrimitive<TOut>.Singleton[result, this[arg1, arg2]];
    }

    /// <summary>
    /// A wrapper for a C# function to allow it to be called from TED expressions
    /// </summary>
    public class Function<TIn1, TIn2, TIn3, TOut> : Function {
        internal readonly Func<TIn1, TIn2, TIn3, TOut> Implementation;

        /// <summary>
        /// Make a new TedFunction to wrap the specified C# function
        /// </summary>
        public Function(string name, Func<TIn1, TIn2, TIn3, TOut> implementation, bool isPure = true) : base(name, isPure) => Implementation = implementation;

        /// <summary>
        /// Make a call to the function
        /// </summary>
        public FunctionCall<TIn1, TIn2, TIn3, TOut> this[Term<TIn1> arg1, Term<TIn2> arg2, Term<TIn3> arg3] => 
            new FunctionCall<TIn1, TIn2, TIn3, TOut>(this, arg1, arg2, arg3);

        /// <summary>
        /// True when result is the result of this function.
        /// </summary>
        /// <param name="arg1">First input to function</param>
        /// <param name="arg2">Second input</param>
        /// <param name="arg3">Third input</param>
        /// <param name="result">Output of this function</param>
        public Goal this[Term<TIn1> arg1, Term<TIn2> arg2, Term<TIn3> arg3, Term<TOut> result] => EvalPrimitive<TOut>.Singleton[result, this[arg1, arg2, arg3]];
    }

    /// <summary>
    /// A wrapper for a C# function to allow it to be called from TED expressions
    /// </summary>
    public class Function<TIn1, TIn2, TIn3, TIn4, TOut> : Function {
        internal readonly Func<TIn1, TIn2, TIn3, TIn4, TOut> Implementation;

        /// <summary>
        /// Make a new TedFunction to wrap the specified C# function
        /// </summary>
        public Function(string name, Func<TIn1, TIn2, TIn3, TIn4, TOut> implementation, bool isPure = true) : base(name, isPure) => Implementation = implementation;

        /// <summary>
        /// Make a call to the function
        /// </summary>
        public FunctionCall<TIn1, TIn2, TIn3, TIn4, TOut> this[Term<TIn1> arg1, Term<TIn2> arg2, Term<TIn3> arg3, Term<TIn4> arg4] =>
            new FunctionCall<TIn1, TIn2, TIn3, TIn4, TOut>(this, arg1, arg2, arg3, arg4);

        /// <summary>
        /// True when result is the result of this function.
        /// </summary>
        /// <param name="arg1">First input to function</param>
        /// <param name="arg2">Second input</param>
        /// <param name="arg3">Third input</param>
        /// <param name="arg4">Fourth input</param>
        /// <param name="result">Output of this function</param>
        public Goal this[Term<TIn1> arg1, Term<TIn2> arg2, Term<TIn3> arg3, Term<TIn4> arg4, Term<TOut> result] => EvalPrimitive<TOut>.Singleton[result, this[arg1, arg2, arg3, arg4]];
    }

    /// <summary>
    /// A wrapper for a C# function to allow it to be called from TED expressions
    /// </summary>
    public class Function<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> : Function {
        internal readonly Func<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> Implementation;

        /// <summary>
        /// Make a new TedFunction to wrap the specified C# function
        /// </summary>
        public Function(string name, Func<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> implementation, bool isPure = true) : base(name, isPure) => Implementation = implementation;

        /// <summary>
        /// Make a call to the function
        /// </summary>
        public FunctionCall<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> this[Term<TIn1> arg1, Term<TIn2> arg2, Term<TIn3> arg3, Term<TIn4> arg4, Term<TIn5> arg5] =>
            new FunctionCall<TIn1, TIn2, TIn3, TIn4, TIn5, TOut>(this, arg1, arg2, arg3, arg4, arg5);

        /// <summary>
        /// True when result is the result of this function.
        /// </summary>
        /// <param name="arg1">First input to function</param>
        /// <param name="arg2">Second input</param>
        /// <param name="arg3">Third input</param>
        /// <param name="arg4">Fourth input</param>
        /// <param name="arg5">Fifth input</param>
        /// <param name="result">Output of this function</param>
        public Goal this[Term<TIn1> arg1, Term<TIn2> arg2, Term<TIn3> arg3, Term<TIn4> arg4, Term<TIn5> arg5, Term<TOut> result]
            => EvalPrimitive<TOut>.Singleton[result, this[arg1, arg2, arg3, arg4, arg5]];
    }

    /// <summary>
    /// A wrapper for a C# function to allow it to be called from TED expressions
    /// </summary>
    public class Function<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut> : Function {
        internal readonly Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut> Implementation;

        /// <summary>
        /// Make a new TedFunction to wrap the specified C# function
        /// </summary>
        public Function(string name, Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut> implementation, bool isPure = true) : base(name, isPure) => Implementation = implementation;
        
        /// <summary>
        /// Make a call to the function
        /// </summary>
        public FunctionCall<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut> this[Term<TIn1> arg1, Term<TIn2> arg2, Term<TIn3> arg3, Term<TIn4> arg4, Term<TIn5> arg5, Term<TIn6> arg6] =>
            new FunctionCall<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut>(this, arg1, arg2, arg3, arg4, arg5, arg6);

        /// <summary>
        /// True when result is the result of this function.
        /// </summary>
        /// <param name="arg1">First input to function</param>
        /// <param name="arg2">Second input</param>
        /// <param name="arg3">Third input</param>
        /// <param name="arg4">Fourth input</param>
        /// <param name="arg5">Fifth input</param>
        /// <param name="arg6">Sixth input</param>
        /// <param name="result">Output of this function</param>
        public Goal this[Term<TIn1> arg1, Term<TIn2> arg2, Term<TIn3> arg3, Term<TIn4> arg4, Term<TIn5> arg5, Term<TIn6> arg6, Term<TOut> result]
            => EvalPrimitive<TOut>.Singleton[result, this[arg1, arg2, arg3, arg4, arg5, arg6]];
    }

    /// <summary>
    /// A wrapper for a C# function to allow it to be called from TED expressions
    /// </summary>
    public class Function<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut> : Function {
        internal readonly Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut> Implementation;

        /// <summary>
        /// Make a new TedFunction to wrap the specified C# function
        /// </summary>
        public Function(string name, Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut> implementation, bool isPure = true) : base(name, isPure) => Implementation = implementation;
        
        /// <summary>
        /// Make a call to the function
        /// </summary>
        public FunctionCall<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut> this[Term<TIn1> arg1, Term<TIn2> arg2, Term<TIn3> arg3, Term<TIn4> arg4, Term<TIn5> arg5, Term<TIn6> arg6, Term<TIn7> arg7] =>
            new FunctionCall<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut>(this, arg1, arg2, arg3, arg4, arg5, arg6, arg7);

        /// <summary>
        /// True when result is the result of this function.
        /// </summary>
        /// <param name="arg1">First input to function</param>
        /// <param name="arg2">Second input</param>
        /// <param name="arg3">Third input</param>
        /// <param name="arg4">Fourth input</param>
        /// <param name="arg5">Fifth input</param>
        /// <param name="arg6">Sixth input</param>
        /// <param name="arg7">Seventh input</param>
        /// <param name="result">Output of this function</param>
        public Goal this[Term<TIn1> arg1, Term<TIn2> arg2, Term<TIn3> arg3, Term<TIn4> arg4, Term<TIn5> arg5, Term<TIn6> arg6, Term<TIn7> arg7, Term<TOut> result]
            => EvalPrimitive<TOut>.Singleton[result, this[arg1, arg2, arg3, arg4, arg5, arg6, arg7]];
    }
}