using System;
using TED.Compiler;
using TED.Interpreter;
using TED.Preprocessing;
using TED.Utilities;

namespace TED.Primitives
{
    /// <summary>
    /// Wrapper for C# comparison operators for some type.
    /// </summary>
    internal sealed class ComparisonPrimitive<T> : PrimitivePredicate<T, T>
    {
        internal static readonly ComparisonPrimitive<T> LessThan = new ComparisonPrimitive<T>("<", "op_LessThan");
        internal static readonly ComparisonPrimitive<T> LessThanEq = new ComparisonPrimitive<T>("<=", "op_LessThanOrEqual");
        internal static readonly ComparisonPrimitive<T> GreaterThan = new ComparisonPrimitive<T>(">", "op_GreaterThan");
        internal static readonly ComparisonPrimitive<T> GreaterThanEq = new ComparisonPrimitive<T>(">=", "op_GreaterThanOrEqual");

        private readonly Func<T, T, bool> comparison;

        private ComparisonPrimitive(string name, string operatorName) : base(name)
        {
            var type = typeof(T);
            var methodInfo = type.GetOperatorMethodInfo(operatorName, typeof(bool), type, type);
            if (methodInfo == null)
                throw new ArgumentException($"There is no {name} overload defined for comparing two {type.Name}s");
            comparison = (Func<T, T, bool>)methodInfo.CreateDelegate(typeof(Func<T, T, bool>));
        }

        public override Interpreter.Call MakeCall(Goal g, GoalAnalyzer tc)
        {
            var i1 = tc.Emit(g.Arg1);
            var i2 = tc.Emit(g.Arg2);
            if (!i1.IsInstantiated)
                throw new InstantiationException(this, g.Arg1);
            if (!i2.IsInstantiated)
                throw new InstantiationException(this, g.Arg2);
            return new Call(g, this, i1.ValueCell, i2.ValueCell, comparison);
        }

        private class Call : Interpreter.Call
        {
            private readonly ValueCell<T> arg1;
            private readonly ValueCell<T> arg2;
            private readonly Func<T, T, bool> test;
            private bool ready;
            private readonly Goal Goal;

            public override IPattern ArgumentPattern =>
                new Pattern<T, T>(MatchOperation<T>.Read(arg1), MatchOperation<T>.Read(arg2));

            public Call(Goal goal, ComparisonPrimitive<T> predicate, ValueCell<T> arg1, ValueCell<T> arg2, Func<T, T, bool> test) : base(predicate)
            {
                this.arg1 = arg1;
                this.arg2 = arg2;
                this.test = test;
                Goal = goal;
            }

            public override void Reset()
            {
                ready = true;
            }

            public override bool NextSolution()
            {
                if (!ready) return false;
                ready = false;
                return test(arg1.Value, arg2.Value);
            }

            public override Continuation Compile(Compiler.Compiler compiler, Continuation fail, string identifierSuffix)
            {
                compiler.Indented($"if (!({Goal.Arg1.ToSourceExpressionParenthesized(compiler)}{Goal.Predicate.Name}{Goal.Arg2.ToSourceExpressionParenthesized(compiler)})) {fail.Invoke};");
                return fail;
            }
        }
    }

}
