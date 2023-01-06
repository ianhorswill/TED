using System;

namespace TED
{
    /// <summary>
    /// A wrapper for a zero-argument function
    /// </summary>
    /// <typeparam name="TOut">Type of the result of the function</typeparam>
    public class FunctionCall<TOut> : FunctionalExpression<TOut>
    {
        public readonly Func<TOut> Function;

        public FunctionCall(Func<TOut> function)
        {
            Function = function;
        }

        internal override Func<TOut> MakeEvaluator(GoalAnalyzer _) => Function;
    }

    /// <summary>
    /// A wrapper for a 1-argument function
    /// </summary>
    /// <typeparam name="TIn1">Type of the input of the function</typeparam>
    /// <typeparam name="TOut">Type of the result of the function</typeparam>
    public class FunctionCall<TIn1, TOut> : FunctionalExpression<TOut>
    {
        public readonly Func<TIn1, TOut> Function;
        public readonly Term<TIn1> Arg1;

        public FunctionCall(Func<TIn1, TOut> function, Term<TIn1> arg1)
        {
            Function = function;
            Arg1 = arg1;
        }

        internal override Func<TOut> MakeEvaluator(GoalAnalyzer ga)
        {
            var arg1E = Arg1.MakeEvaluator(ga);
            return () => Function(arg1E());
        }
    }

    /// <summary>
    /// A wrapper for a 2-argument function
    /// </summary>
    /// <typeparam name="TIn1">Type of the first input of the function</typeparam>
    /// <typeparam name="TIn2">Type of the second input of the function</typeparam>
    /// <typeparam name="TOut">Type of the result of the function</typeparam>
    public class FunctionCall<TIn1, TIn2, TOut> : FunctionalExpression<TOut>
    {
        public readonly Func<TIn1, TIn2, TOut> Function;
        public readonly Term<TIn1> Arg1;
        public readonly Term<TIn2> Arg2;

        public FunctionCall(Func<TIn1, TIn2, TOut> function, Term<TIn1> arg1, Term<TIn2> arg2)
        {
            Function = function;
            Arg1 = arg1;
            Arg2 = arg2;
        }

        internal override Func<TOut> MakeEvaluator(GoalAnalyzer ga)
        {
            var arg1E = Arg1.MakeEvaluator(ga);
            var arg2E = Arg2.MakeEvaluator(ga);
            return () => Function(arg1E(), arg2E());
        }
    }
}
