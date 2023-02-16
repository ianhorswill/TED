using System;

namespace TED
{
    /// <summary>
    /// Implements negation of a goal
    /// </summary>
    public sealed class NotPrimitive : PrimitivePredicate<Goal>
    {
        public static NotPrimitive Singleton = new NotPrimitive();

        public NotPrimitive() : base("Not")
        {
        }

        public override Call MakeCall(Goal g, GoalAnalyzer tc)
        {
            switch (g.Arg1)
            {
                case Constant<TED.Goal> target:
                    return new NotCall(Preprocessor.BodyToCallWithLocalBindings(tc, target.Value).Call);

                default:
                    throw new ArgumentException("Argument to Not or ! must be a goal expression, not a variable");
            }
        }

        private class NotCall : Call
        {
            private readonly Call call;
            private bool restarted;

            public NotCall(Call call) : base(Language.Not)
            {
                this.call = call;
            }

            public override IPattern ArgumentPattern => new Pattern<Call>(MatchOperation<Call>.Constant(call));

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
