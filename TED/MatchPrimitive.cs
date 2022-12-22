using System;

namespace TED
{
    internal interface IMatchPrimitive { }

    /// <summary>
    /// Implements negation of a goal
    /// </summary>
    public class MatchPrimitive<T> : PrimitivePredicate<T,T>, IMatchPrimitive
    {
        public static MatchPrimitive<T> Singleton = new MatchPrimitive<T>();

        public MatchPrimitive() : base("Match")
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
