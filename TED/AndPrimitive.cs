using System.Linq;

namespace TED
{
    /// <summary>
    /// Implements conjunctions as a primitive predicate.  That is, And[g1, g2, ...] is true when all the gi are true.
    /// Procedurally, this means find solutions to g1, then g2, etc., backtracking in the normal manner.
    /// </summary>
    public sealed class AndPrimitive : Predicate
    {
        public static readonly AndPrimitive Singleton = new AndPrimitive();
        private AndPrimitive() : base("And")
        { }

        /// <summary>
        /// True when all the subgoals are true
        /// </summary>
        public TED.Goal this[params TED.Goal[] subgoals] => new Goal(subgoals);

        public class Goal : TED.Goal
        {
            public readonly TED.Goal[] Body;
            // ReSharper disable once CoVariantArrayConversion
            public Goal(params TED.Goal[] body) : base(body.Select(g => new Constant<TED.Goal>(g)).ToArray())
            {
                Body = body;
            }

            public override Predicate Predicate => Singleton;

            internal override TED.Goal RenameArguments(Substitution s) 
                => new Goal(Body.Select(g => g.RenameArguments(s)).ToArray());

            internal override TED.Call MakeCall(GoalAnalyzer ga)
                => new Call(Preprocessor.GenerateCalls(ga, Body), Body);
        }

        public class Call : TED.Call
        {
            public readonly TED.Call[] Body;
            public readonly TED.Goal[] Source;
            private int currentCallIndex;

            public Call(TED.Call[] body, TED.Goal[] source) : base(Singleton)
            {
                Body = body;
                Source = source;
            }

            public TED.Call CurrentCall => Body[currentCallIndex];

            public override IPattern ArgumentPattern
                => new Pattern<TED.Goal[]>(MatchOperation<TED.Goal[]>.Constant(Source));

            public override void Reset()
            {
                currentCallIndex = 0;
                CurrentCall.Reset();
            }

            public override bool NextSolution()
            {
                while (currentCallIndex >= 0)
                {
                    if (CurrentCall.NextSolution())
                    {
                        // Succeeded
                        if (currentCallIndex == Body.Length - 1)
                            return true;
                        else
                        {
                            // Advance to the next subgoal
                            currentCallIndex++;
                            CurrentCall.Reset();
                        }
                    }
                    else
                        // Failed
                        currentCallIndex--;
                }
                return false;
            }
        }
    }
}
