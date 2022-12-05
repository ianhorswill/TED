using System;

namespace TED
{
    /// <summary>
    /// Implements negation of a goal
    /// </summary>
    public class NotPrimitive : PrimitivePredicate<AnyGoal>
    {
        public NotPrimitive() : base("Not")
        {
        }

        internal override AnyCall MakeCall(Goal g, GoalAnalyzer tc)
        {
            switch (g.Arg1)
            {
                case Constant<AnyGoal> target:
                    return new NotCall(target.Value.MakeCall(tc));

                default:
                    throw new ArgumentException("Argument to Not or ! must be a goal expression, not a variable");
            }
        }

        private class NotCall : AnyCall
        {
            private readonly AnyCall call;
            private bool restarted;

            public NotCall(AnyCall call) : base(Language.Not)
            {
                this.call = call;
            }

            public override IPattern ArgumentPattern => new Pattern<AnyCall>(MatchOperation<AnyCall>.Constant(call));

            public override void Reset()
            {
                restarted = true;
            }

            public override bool NextSolution()
            {
                if (!restarted) return false;
                restarted = false;
                call.Reset();
                return !call.NextSolution();
            }
        }
    }
}
