using System;
using System.Diagnostics;

namespace TED
{
    [DebuggerDisplay("{Name}")]
    public abstract class TedFunction
    {
        public readonly string Name;

        public TedFunction(string name)
        {
            Name = name;
        }

        public override string ToString() => Name;
    }
    public class TedFunction<TOut> : TedFunction
    {
        
        private readonly Func<TOut> function;

        public TedFunction(string name, Func<TOut> function) : base(name)
        {
            this.function = function;
        }

        public FunctionCall<TOut> Call() => new FunctionCall<TOut>(function);
    }

    public class TedFunction<TIn, TOut> : TedFunction
    {
        
        private readonly Func<TIn, TOut> function;

        public TedFunction(string name, Func<TIn, TOut> function) : base(name)
        {
            this.function = function;
        }

        public FunctionCall<TIn, TOut> this[Term<TIn> arg] => new FunctionCall<TIn, TOut>(function, arg);
    }

    public class TedFunction<TIn1, TIn2, TOut> : TedFunction
    {
        
        private readonly Func<TIn1, TIn2, TOut> function;

        public TedFunction(string name, Func<TIn1, TIn2, TOut> function) : base(name)
        {
            this.function = function;
        }

        public FunctionCall<TIn1, TIn2, TOut> this[Term<TIn1> arg1, Term<TIn2> arg2] => new FunctionCall<TIn1, TIn2, TOut>(function, arg1, arg2);
    }
}
