using System;

namespace TED
{
    /// <summary>
    /// A wrapper for a zero-argument function
    /// </summary>
    /// <typeparam name="TOut">Type of the result of the function</typeparam>
    public class FunctionCall<TOut> : FunctionalExpression<TOut>
    {
        public readonly TedFunction<TOut> Function;

        public FunctionCall(TedFunction<TOut> function)
        {
            Function = function;
        }

        internal override Func<TOut> MakeEvaluator(GoalAnalyzer _) => Function.Implementation;

        public override string ToString() => Function.Name;

        internal override Term<TOut> RecursivelySubstitute(Substitution s) => this;
    }

    /// <summary>
    /// A wrapper for a 1-argument function
    /// </summary>
    /// <typeparam name="TIn1">Type of the input of the function</typeparam>
    /// <typeparam name="TOut">Type of the result of the function</typeparam>
    public class FunctionCall<TIn1, TOut> : FunctionalExpression<TOut>
    {
        public readonly TedFunction<TIn1, TOut> Function;
        public readonly Term<TIn1> Arg1;

        public FunctionCall(TedFunction<TIn1, TOut> function, Term<TIn1> arg1)
        {
            Function = function;
            Arg1 = arg1;
        }

        internal override Term<TOut> RecursivelySubstitute(Substitution s) =>
            new FunctionCall<TIn1,TOut>(Function, s.Substitute(Arg1));

        internal override Func<TOut> MakeEvaluator(GoalAnalyzer ga)
        {
            var arg1E = Arg1.MakeEvaluator(ga);
            return () => Function.Implementation(arg1E());
        }

        public override string ToString() => Function.RenderAsOperator?$"{Function.Name}{Arg1}":$"{Function.Name}({Arg1})";
    }

    /// <summary>
    /// A wrapper for a 2-argument function
    /// </summary>
    /// <typeparam name="TIn1">Type of the first input of the function</typeparam>
    /// <typeparam name="TIn2">Type of the second input of the function</typeparam>
    /// <typeparam name="TOut">Type of the result of the function</typeparam>
    public class FunctionCall<TIn1, TIn2, TOut> : FunctionalExpression<TOut>
    {
        public readonly TedFunction<TIn1, TIn2, TOut> Function;
        public readonly Term<TIn1> Arg1;
        public readonly Term<TIn2> Arg2;

        public FunctionCall(TedFunction<TIn1, TIn2, TOut> function, Term<TIn1> arg1, Term<TIn2> arg2)
        {
            Function = function;
            Arg1 = arg1;
            Arg2 = arg2;
        }

        internal override Term<TOut> RecursivelySubstitute(Substitution s) =>
            new FunctionCall<TIn1,TIn2, TOut>(Function, s.Substitute(Arg1), s.Substitute(Arg2));

        internal override Func<TOut> MakeEvaluator(GoalAnalyzer ga)
        {
            var arg1E = Arg1.MakeEvaluator(ga);
            var arg2E = Arg2.MakeEvaluator(ga);
            return () => Function.Implementation(arg1E(), arg2E());
        }

        public override string ToString() => Function.RenderAsOperator?$"{Arg1}{Function.Name}{Arg2}":$"{Function.Name}({Arg1}, {Arg2})";
    }

    /// <summary>
    /// A wrapper for a 3-argument function
    /// </summary>
    /// <typeparam name="TIn1">Type of the first input of the function</typeparam>
    /// <typeparam name="TIn2">Type of the second input of the function</typeparam>
    /// <typeparam name="TIn3">Type of the second input of the function</typeparam>
    /// <typeparam name="TOut">Type of the result of the function</typeparam>
    public class FunctionCall<TIn1, TIn2, TIn3, TOut> : FunctionalExpression<TOut> {
        public readonly TedFunction<TIn1, TIn2, TIn3, TOut> Function;
        public readonly Term<TIn1> Arg1;
        public readonly Term<TIn2> Arg2;
        public readonly Term<TIn3> Arg3;

        public FunctionCall(TedFunction<TIn1, TIn2, TIn3, TOut> function, Term<TIn1> arg1, Term<TIn2> arg2, Term<TIn3> arg3) {
            Function = function;
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
        }

        internal override Term<TOut> RecursivelySubstitute(Substitution s) =>
            new FunctionCall<TIn1, TIn2, TIn3, TOut>(Function, s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3));

        internal override Func<TOut> MakeEvaluator(GoalAnalyzer ga) {
            var arg1E = Arg1.MakeEvaluator(ga);
            var arg2E = Arg2.MakeEvaluator(ga);
            var arg3E = Arg3.MakeEvaluator(ga);
            return () => Function.Implementation(arg1E(), arg2E(), arg3E());
        }

        // NOTE: No render as operator options for 3+ input args
        public override string ToString() => $"{Function.Name}({Arg1}, {Arg2}, {Arg3})";
    }

    /// <summary>
    /// A wrapper for a 4-argument function
    /// </summary>
    /// <typeparam name="TIn1">Type of the first input of the function</typeparam>
    /// <typeparam name="TIn2">Type of the second input of the function</typeparam>
    /// <typeparam name="TIn3">Type of the second input of the function</typeparam>
    /// <typeparam name="TIn4">Type of the second input of the function</typeparam>
    /// <typeparam name="TOut">Type of the result of the function</typeparam>
    public class FunctionCall<TIn1, TIn2, TIn3, TIn4, TOut> : FunctionalExpression<TOut> {
        public readonly TedFunction<TIn1, TIn2, TIn3, TIn4, TOut> Function;
        public readonly Term<TIn1> Arg1;
        public readonly Term<TIn2> Arg2;
        public readonly Term<TIn3> Arg3;
        public readonly Term<TIn4> Arg4;

        public FunctionCall(TedFunction<TIn1, TIn2, TIn3, TIn4, TOut> function, Term<TIn1> arg1, Term<TIn2> arg2, Term<TIn3> arg3, Term<TIn4> arg4) {
            Function = function;
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
        }

        internal override Term<TOut> RecursivelySubstitute(Substitution s) =>
            new FunctionCall<TIn1, TIn2, TIn3, TIn4, TOut>(Function, s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3), s.Substitute(Arg4));

        internal override Func<TOut> MakeEvaluator(GoalAnalyzer ga) {
            var arg1E = Arg1.MakeEvaluator(ga);
            var arg2E = Arg2.MakeEvaluator(ga);
            var arg3E = Arg3.MakeEvaluator(ga);
            var arg4E = Arg4.MakeEvaluator(ga);
            return () => Function.Implementation(arg1E(), arg2E(), arg3E(), arg4E());
        }

        // NOTE: No render as operator options for 3+ input args
        public override string ToString() => $"{Function.Name}({Arg1}, {Arg2}, {Arg3}, {Arg4})";
    }

    /// <summary>
    /// A wrapper for a 4-argument function
    /// </summary>
    /// <typeparam name="TIn1">Type of the first input of the function</typeparam>
    /// <typeparam name="TIn2">Type of the second input of the function</typeparam>
    /// <typeparam name="TIn3">Type of the second input of the function</typeparam>
    /// <typeparam name="TIn4">Type of the second input of the function</typeparam>
    /// <typeparam name="TIn5">Type of the second input of the function</typeparam>
    /// <typeparam name="TOut">Type of the result of the function</typeparam>
    public class FunctionCall<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> : FunctionalExpression<TOut> {
        public readonly TedFunction<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> Function;
        public readonly Term<TIn1> Arg1;
        public readonly Term<TIn2> Arg2;
        public readonly Term<TIn3> Arg3;
        public readonly Term<TIn4> Arg4;
        public readonly Term<TIn5> Arg5;

        public FunctionCall(TedFunction<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> function, Term<TIn1> arg1, Term<TIn2> arg2, Term<TIn3> arg3, Term<TIn4> arg4, Term<TIn5> arg5) {
            Function = function;
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
        }

        internal override Term<TOut> RecursivelySubstitute(Substitution s) =>
            new FunctionCall<TIn1, TIn2, TIn3, TIn4, TIn5, TOut>(Function, s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3), s.Substitute(Arg4), s.Substitute(Arg5));

        internal override Func<TOut> MakeEvaluator(GoalAnalyzer ga) {
            var arg1E = Arg1.MakeEvaluator(ga);
            var arg2E = Arg2.MakeEvaluator(ga);
            var arg3E = Arg3.MakeEvaluator(ga);
            var arg4E = Arg4.MakeEvaluator(ga);
            var arg5E = Arg5.MakeEvaluator(ga);
            return () => Function.Implementation(arg1E(), arg2E(), arg3E(), arg4E(), arg5E());
        }

        // NOTE: No render as operator options for 3+ input args
        public override string ToString() => $"{Function.Name}({Arg1}, {Arg2}, {Arg3}, {Arg4}, {Arg5})";
    }
}
