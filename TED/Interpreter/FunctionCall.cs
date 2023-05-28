using System;
using TED.Preprocessing;

namespace TED.Interpreter
{
    /// <summary>
    /// A wrapper for a zero-argument function
    /// </summary>
    /// <typeparam name="TOut">Type of the result of the function</typeparam>
    public class FunctionCall<TOut> : FunctionalExpression<TOut>
    {
        /// <summary>
        /// The function to be called
        /// </summary>
        public readonly Function<TOut> Function;

        /// <summary>
        /// Make a term representing a call to a TEDFunction
        /// </summary>
        public FunctionCall(Function<TOut> function)
        {
            Function = function;
        }

        /// <inheritdoc />
        public override bool IsPure => Function.IsPure;

        internal override Func<TOut> MakeEvaluator(GoalAnalyzer _) => Function.Implementation;

        /// <inheritdoc />
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
        /// <summary>
        /// The function to be called
        /// </summary>
        public readonly Function<TIn1, TOut> Function;

        /// <inheritdoc />
        public override bool IsPure => Function.IsPure;

        /// <summary>
        /// The argument to the function
        /// </summary>
        public readonly Term<TIn1> Arg1;

        /// <summary>
        /// Make a term representing a call to a TEDFunction
        /// </summary>
        public FunctionCall(Function<TIn1, TOut> function, Term<TIn1> arg1)
        {
            Function = function;
            Arg1 = arg1;
        }

        internal override Term<TOut> RecursivelySubstitute(Substitution s) =>
            new FunctionCall<TIn1, TOut>(Function, s.Substitute(Arg1));

        internal override Func<TOut> MakeEvaluator(GoalAnalyzer ga)
        {
            var arg1E = Arg1.MakeEvaluator(ga);
            return () => Function.Implementation(arg1E());
        }

        /// <inheritdoc />
        public override string ToString() => Function.RenderAsOperator ? $"{Function.Name}{Arg1}" : $"{Function.Name}({Arg1})";
    }

    /// <summary>
    /// A wrapper for a 2-argument function
    /// </summary>
    /// <typeparam name="TIn1">Type of the first input of the function</typeparam>
    /// <typeparam name="TIn2">Type of the second input of the function</typeparam>
    /// <typeparam name="TOut">Type of the result of the function</typeparam>
    public class FunctionCall<TIn1, TIn2, TOut> : FunctionalExpression<TOut>
    {
        /// <summary>
        /// The function to be called
        /// </summary>
        public readonly Function<TIn1, TIn2, TOut> Function;

        /// <inheritdoc />
        public override bool IsPure => Function.IsPure;

        /// <summary>
        /// The first argument
        /// </summary>
        public readonly Term<TIn1> Arg1;
        /// <summary>
        ///  The second argument
        /// </summary>
        public readonly Term<TIn2> Arg2;

        /// <summary>
        /// Make a term representing a call to a TEDFunction
        /// </summary>
        public FunctionCall(Function<TIn1, TIn2, TOut> function, Term<TIn1> arg1, Term<TIn2> arg2)
        {
            Function = function;
            Arg1 = arg1;
            Arg2 = arg2;
        }

        internal override Term<TOut> RecursivelySubstitute(Substitution s) =>
            new FunctionCall<TIn1, TIn2, TOut>(Function, s.Substitute(Arg1), s.Substitute(Arg2));

        internal override Func<TOut> MakeEvaluator(GoalAnalyzer ga)
        {
            var arg1E = Arg1.MakeEvaluator(ga);
            var arg2E = Arg2.MakeEvaluator(ga);
            return () => Function.Implementation(arg1E(), arg2E());
        }

        /// <inheritdoc />
        public override string ToString() => Function.RenderAsOperator ? $"{Arg1}{Function.Name}{Arg2}" : $"{Function.Name}({Arg1}, {Arg2})";
    }

    /// <summary>
    /// A wrapper for a 3-argument function
    /// </summary>
    /// <typeparam name="TIn1">Type of the first input of the function</typeparam>
    /// <typeparam name="TIn2">Type of the second input of the function</typeparam>
    /// <typeparam name="TIn3">Type of the third input of the function</typeparam>
    /// <typeparam name="TOut">Type of the result of the function</typeparam>
    public class FunctionCall<TIn1, TIn2, TIn3, TOut> : FunctionalExpression<TOut>
    {
        /// <summary>
        /// The function to be called
        /// </summary>
        public readonly Function<TIn1, TIn2, TIn3, TOut> Function;

        /// <inheritdoc />
        public override bool IsPure => Function.IsPure;

        /// <summary>
        /// The first argument
        /// </summary>
        public readonly Term<TIn1> Arg1;
        /// <summary>
        ///  The second argument
        /// </summary>
        public readonly Term<TIn2> Arg2;
        /// <summary>
        /// The third argument
        /// </summary>
        public readonly Term<TIn3> Arg3;

        /// <summary>
        /// Make a term representing a call to a TEDFunction
        /// </summary>
        public FunctionCall(Function<TIn1, TIn2, TIn3, TOut> function, Term<TIn1> arg1, Term<TIn2> arg2, Term<TIn3> arg3)
        {
            Function = function;
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
        }

        internal override Term<TOut> RecursivelySubstitute(Substitution s) =>
            new FunctionCall<TIn1, TIn2, TIn3, TOut>(Function, s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3));

        internal override Func<TOut> MakeEvaluator(GoalAnalyzer ga)
        {
            var arg1E = Arg1.MakeEvaluator(ga);
            var arg2E = Arg2.MakeEvaluator(ga);
            var arg3E = Arg3.MakeEvaluator(ga);
            return () => Function.Implementation(arg1E(), arg2E(), arg3E());
        }

        // NOTE: No render as operator options for 3+ input args
        /// <inheritdoc />
        public override string ToString() => $"{Function.Name}({Arg1}, {Arg2}, {Arg3})";
    }

    /// <summary>
    /// A wrapper for a 4-argument function
    /// </summary>
    /// <typeparam name="TIn1">Type of the first input of the function</typeparam>
    /// <typeparam name="TIn2">Type of the second input of the function</typeparam>
    /// <typeparam name="TIn3">Type of the third input of the function</typeparam>
    /// <typeparam name="TIn4">Type of the fourth input of the function</typeparam>
    /// <typeparam name="TOut">Type of the result of the function</typeparam>
    public class FunctionCall<TIn1, TIn2, TIn3, TIn4, TOut> : FunctionalExpression<TOut>
    {
        /// <summary>
        /// The function to be called
        /// </summary>
        public readonly Function<TIn1, TIn2, TIn3, TIn4, TOut> Function;

        /// <inheritdoc />
        public override bool IsPure => Function.IsPure;

        /// <summary>
        /// The first argument
        /// </summary>
        public readonly Term<TIn1> Arg1;
        /// <summary>
        ///  The second argument
        /// </summary>
        public readonly Term<TIn2> Arg2;
        /// <summary>
        /// The third argument
        /// </summary>
        public readonly Term<TIn3> Arg3;
        /// <summary>
        /// The fourth argument
        /// </summary>
        public readonly Term<TIn4> Arg4;

        /// <summary>
        /// Make a term representing a call to a TEDFunction
        /// </summary>
        public FunctionCall(Function<TIn1, TIn2, TIn3, TIn4, TOut> function, Term<TIn1> arg1, Term<TIn2> arg2, Term<TIn3> arg3, Term<TIn4> arg4)
        {
            Function = function;
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
        }

        internal override Term<TOut> RecursivelySubstitute(Substitution s) =>
            new FunctionCall<TIn1, TIn2, TIn3, TIn4, TOut>(Function, s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3), s.Substitute(Arg4));

        internal override Func<TOut> MakeEvaluator(GoalAnalyzer ga)
        {
            var arg1E = Arg1.MakeEvaluator(ga);
            var arg2E = Arg2.MakeEvaluator(ga);
            var arg3E = Arg3.MakeEvaluator(ga);
            var arg4E = Arg4.MakeEvaluator(ga);
            return () => Function.Implementation(arg1E(), arg2E(), arg3E(), arg4E());
        }

        // NOTE: No render as operator options for 3+ input args
        /// <inheritdoc />
        public override string ToString() => $"{Function.Name}({Arg1}, {Arg2}, {Arg3}, {Arg4})";
    }

    /// <summary>
    /// A wrapper for a 5-argument function
    /// </summary>
    /// <typeparam name="TIn1">Type of the first input of the function</typeparam>
    /// <typeparam name="TIn2">Type of the second input of the function</typeparam>
    /// <typeparam name="TIn3">Type of the third input of the function</typeparam>
    /// <typeparam name="TIn4">Type of the fourth input of the function</typeparam>
    /// <typeparam name="TIn5">Type of the fifth input of the function</typeparam>
    /// <typeparam name="TOut">Type of the result of the function</typeparam>
    public class FunctionCall<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> : FunctionalExpression<TOut>
    {
        /// <summary>
        /// The function to be called
        /// </summary>
        public readonly Function<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> Function;

        /// <inheritdoc />
        public override bool IsPure => Function.IsPure;

        /// <summary>
        /// The first argument
        /// </summary>
        public readonly Term<TIn1> Arg1;
        /// <summary>
        ///  The second argument
        /// </summary>
        public readonly Term<TIn2> Arg2;
        /// <summary>
        /// The third argument
        /// </summary>
        public readonly Term<TIn3> Arg3;
        /// <summary>
        /// The fourth argument
        /// </summary>
        public readonly Term<TIn4> Arg4;
        /// <summary>
        /// The fifth argument
        /// </summary>
        public readonly Term<TIn5> Arg5;

        /// <summary>
        /// Make a term representing a call to a TEDFunction
        /// </summary>
        public FunctionCall(Function<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> function, Term<TIn1> arg1, Term<TIn2> arg2, Term<TIn3> arg3, Term<TIn4> arg4, Term<TIn5> arg5)
        {
            Function = function;
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
        }

        internal override Term<TOut> RecursivelySubstitute(Substitution s) =>
            new FunctionCall<TIn1, TIn2, TIn3, TIn4, TIn5, TOut>(Function, s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3), s.Substitute(Arg4), s.Substitute(Arg5));

        internal override Func<TOut> MakeEvaluator(GoalAnalyzer ga)
        {
            var arg1E = Arg1.MakeEvaluator(ga);
            var arg2E = Arg2.MakeEvaluator(ga);
            var arg3E = Arg3.MakeEvaluator(ga);
            var arg4E = Arg4.MakeEvaluator(ga);
            var arg5E = Arg5.MakeEvaluator(ga);
            return () => Function.Implementation(arg1E(), arg2E(), arg3E(), arg4E(), arg5E());
        }

        // NOTE: No render as operator options for 3+ input args
        /// <inheritdoc />
        public override string ToString() => $"{Function.Name}({Arg1}, {Arg2}, {Arg3}, {Arg4}, {Arg5})";
    }

    /// <summary>
    /// A wrapper for a 6-argument function
    /// </summary>
    /// <typeparam name="TIn1">Type of the first input of the function</typeparam>
    /// <typeparam name="TIn2">Type of the second input of the function</typeparam>
    /// <typeparam name="TIn3">Type of the third input of the function</typeparam>
    /// <typeparam name="TIn4">Type of the fourth input of the function</typeparam>
    /// <typeparam name="TIn5">Type of the fifth input of the function</typeparam>
    /// <typeparam name="TIn6">Type of the sixth input of the function</typeparam>
    /// <typeparam name="TOut">Type of the result of the function</typeparam>
    public class FunctionCall<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut> : FunctionalExpression<TOut>
    {
        /// <summary>
        /// The function to be called
        /// </summary>
        public readonly Function<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut> Function;

        /// <inheritdoc />
        public override bool IsPure => Function.IsPure;

        /// <summary>
        /// The first argument
        /// </summary>
        public readonly Term<TIn1> Arg1;
        /// <summary>
        ///  The second argument
        /// </summary>
        public readonly Term<TIn2> Arg2;
        /// <summary>
        /// The third argument
        /// </summary>
        public readonly Term<TIn3> Arg3;
        /// <summary>
        /// The fourth argument
        /// </summary>
        public readonly Term<TIn4> Arg4;
        /// <summary>
        /// The fifth argument
        /// </summary>
        public readonly Term<TIn5> Arg5;
        /// <summary>
        /// The sixth argument
        /// </summary>
        public readonly Term<TIn6> Arg6;

        /// <summary>
        /// Make a term representing a call to a TEDFunction
        /// </summary>
        public FunctionCall(Function<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut> function, Term<TIn1> arg1, Term<TIn2> arg2, Term<TIn3> arg3, Term<TIn4> arg4, Term<TIn5> arg5, Term<TIn6> arg6)
        {
            Function = function;
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
            Arg6 = arg6;
        }

        internal override Term<TOut> RecursivelySubstitute(Substitution s) =>
            new FunctionCall<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut>(Function, s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3), s.Substitute(Arg4), s.Substitute(Arg5), s.Substitute(Arg6));

        internal override Func<TOut> MakeEvaluator(GoalAnalyzer ga)
        {
            var arg1E = Arg1.MakeEvaluator(ga);
            var arg2E = Arg2.MakeEvaluator(ga);
            var arg3E = Arg3.MakeEvaluator(ga);
            var arg4E = Arg4.MakeEvaluator(ga);
            var arg5E = Arg5.MakeEvaluator(ga);
            var arg6E = Arg6.MakeEvaluator(ga);
            return () => Function.Implementation(arg1E(), arg2E(), arg3E(), arg4E(), arg5E(), arg6E());
        }

        // NOTE: No render as operator options for 3+ input args
        /// <inheritdoc />
        public override string ToString() => $"{Function.Name}({Arg1}, {Arg2}, {Arg3}, {Arg4}, {Arg5}, {Arg6})";
    }

    /// <summary>
    /// A wrapper for a 7-argument function
    /// </summary>
    /// <typeparam name="TIn1">Type of the first input of the function</typeparam>
    /// <typeparam name="TIn2">Type of the second input of the function</typeparam>
    /// <typeparam name="TIn3">Type of the third input of the function</typeparam>
    /// <typeparam name="TIn4">Type of the fourth input of the function</typeparam>
    /// <typeparam name="TIn5">Type of the fifth input of the function</typeparam>
    /// <typeparam name="TIn6">Type of the sixth input of the function</typeparam>
    /// <typeparam name="TIn7">Type of the seventh input of the function</typeparam>
    /// <typeparam name="TOut">Type of the result of the function</typeparam>
    public class FunctionCall<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut> : FunctionalExpression<TOut>
    {
        /// <summary>
        /// The function to be called
        /// </summary>
        public readonly Function<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut> Function;

        /// <inheritdoc />
        public override bool IsPure => Function.IsPure;

        /// <summary>
        /// The first argument
        /// </summary>
        public readonly Term<TIn1> Arg1;
        /// <summary>
        ///  The second argument
        /// </summary>
        public readonly Term<TIn2> Arg2;
        /// <summary>
        /// The third argument
        /// </summary>
        public readonly Term<TIn3> Arg3;
        /// <summary>
        /// The fourth argument
        /// </summary>
        public readonly Term<TIn4> Arg4;
        /// <summary>
        /// The fifth argument
        /// </summary>
        public readonly Term<TIn5> Arg5;
        /// <summary>
        /// The sixth argument
        /// </summary>
        public readonly Term<TIn6> Arg6;
        /// <summary>
        /// The seventh argument
        /// </summary>
        public readonly Term<TIn7> Arg7;

        /// <summary>
        /// Make a term representing a call to a TEDFunction
        /// </summary>
        public FunctionCall(Function<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut> function, Term<TIn1> arg1, Term<TIn2> arg2, Term<TIn3> arg3, Term<TIn4> arg4, Term<TIn5> arg5, Term<TIn6> arg6, Term<TIn7> arg7)
        {
            Function = function;
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
            Arg6 = arg6;
            Arg7 = arg7;
        }

        internal override Term<TOut> RecursivelySubstitute(Substitution s) =>
            new FunctionCall<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut>(Function, s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3), s.Substitute(Arg4), s.Substitute(Arg5), s.Substitute(Arg6), s.Substitute(Arg7));

        internal override Func<TOut> MakeEvaluator(GoalAnalyzer ga)
        {
            var arg1E = Arg1.MakeEvaluator(ga);
            var arg2E = Arg2.MakeEvaluator(ga);
            var arg3E = Arg3.MakeEvaluator(ga);
            var arg4E = Arg4.MakeEvaluator(ga);
            var arg5E = Arg5.MakeEvaluator(ga);
            var arg6E = Arg6.MakeEvaluator(ga);
            var arg7E = Arg7.MakeEvaluator(ga);
            return () => Function.Implementation(arg1E(), arg2E(), arg3E(), arg4E(), arg5E(), arg6E(), arg7E());
        }

        // NOTE: No render as operator options for 3+ input args
        /// <inheritdoc />
        public override string ToString() => $"{Function.Name}({Arg1}, {Arg2}, {Arg3}, {Arg4}, {Arg5}, {Arg6}, {Arg7})";
    }
}
