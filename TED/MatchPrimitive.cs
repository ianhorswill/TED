using System;

namespace TED
{
    /// <summary>
    /// Just a type tag so we can test whether something is a EvalPrimitive
    /// </summary>
    internal interface IEvalPrimitive { }

    /// <summary>
    /// Implements evaluation of a functional expression and matching/storing it to a variable
    /// </summary>
    public sealed class EvalPrimitive<T> : PrimitivePredicate<T,T>, IEvalPrimitive
    {
        public static EvalPrimitive<T> Singleton = new EvalPrimitive<T>();

        public EvalPrimitive() : base("Eval")
        {
        }

        internal override AnyCall MakeCall(Goal g, GoalAnalyzer tc) =>
            new SetCall(this, tc.Emit(g.Arg1), g.Arg2.MakeEvaluator(tc));

        private class SetCall : AnyCall
        {
            private readonly MatchOperation<T> matcher;
            private readonly Func<T> expressionEvaluator;
            private bool restarted;

            public SetCall(AnyPredicate p, MatchOperation<T> matcher, Func<T> expressionEvaluator) : base(p)
            {
                this.matcher = matcher;
                this.expressionEvaluator = expressionEvaluator;
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
        }
    }
}
