using System;
using TED.Interpreter;
using TED.Preprocessing;

namespace TED.Primitives
{
    /// <summary>
    /// Just a type tag so we can test whether something is a EvalPrimitive
    /// </summary>
    internal interface IEvalPrimitive { }

    /// <summary>
    /// Implements evaluation of a functional expression and matching/storing it to a variable
    /// </summary>
    internal sealed class EvalPrimitive<T> : PrimitivePredicate<T, T>, IEvalPrimitive
    {
        public static EvalPrimitive<T> Singleton = new EvalPrimitive<T>();

        public EvalPrimitive() : base("Eval")
        {
        }

        public override Call MakeCall(Goal g, GoalAnalyzer tc) =>
            new EvalCall(this, tc.Emit(g.Arg1), g.Arg2.MakeEvaluator(tc), g);

        private class EvalCall : Call
        {
            private readonly MatchOperation<T> matcher;
            private readonly Func<T> expressionEvaluator;
            private readonly Goal originalGoal;
            private bool restarted;

            public EvalCall(Predicate p, MatchOperation<T> matcher, Func<T> expressionEvaluator, Goal originalGoal) : base(p)
            {
                this.matcher = matcher;
                this.expressionEvaluator = expressionEvaluator;
                this.originalGoal = originalGoal;
            }


            public override IPattern ArgumentPattern => new Pattern<T, object>(matcher, MatchOperation<object>.Constant(expressionEvaluator));

            public override void Reset()
            {
                restarted = true;
            }

            public override bool NextSolution()
            {
                if (!restarted) return false;
                restarted = false;
                return matcher.Match(expressionEvaluator());
            }

            public override string ToString() => $"{originalGoal.Arg1} == {originalGoal.Arg2}";
        }
    }
}
