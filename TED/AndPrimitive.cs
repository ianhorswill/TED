using System.Linq;

namespace TED
{
    /// <summary>
    /// Implements conjunctions as a primitive predicate.  That is, And[g1, g2, ...] is true when all the gi are true.
    /// Procedurally, this means find solutions to g1, then g2, etc., backtracking in the normal manner.
    /// </summary>
    public sealed class AndPrimitive : AnyPredicate
    {
        public static readonly AndPrimitive Singleton = new AndPrimitive();
        private AndPrimitive() : base("And")
        { }

        /// <summary>
        /// True when all the subgoals are true
        /// </summary>
        public AnyGoal this[params AnyGoal[] subgoals] => new Goal(subgoals);

        public class Goal : AnyGoal
        {
            public readonly AnyGoal[] Body;
            // ReSharper disable once CoVariantArrayConversion
            public Goal(params AnyGoal[] body) : base(body.Select(g => new Constant<AnyGoal>(g)).ToArray())
            {
                Body = body;
            }

            public override AnyPredicate Predicate => Singleton;

            internal override AnyGoal RenameArguments(Substitution s) 
                => new Goal(Body.Select(g => g.RenameArguments(s)).ToArray());

            internal override AnyCall MakeCall(GoalAnalyzer ga)
                => new Call(Preprocessor.GenerateCalls(ga, Body), Body);
        }

        public class Call : AnyCall
        {
            public readonly AnyCall[] Body;
            public readonly AnyGoal[] Source;
            private int currentCallIndex;

            public Call(AnyCall[] body, AnyGoal[] source) : base(Singleton)
            {
                Body = body;
                Source = source;
            }

            public AnyCall CurrentCall => Body[currentCallIndex];

            public override IPattern ArgumentPattern
                => new Pattern<AnyGoal[]>(MatchOperation<AnyGoal[]>.Constant(Source));

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
