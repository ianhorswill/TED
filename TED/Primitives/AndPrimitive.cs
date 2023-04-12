using System.Collections.Generic;
using System.Linq;
using TED.Interpreter;
using TED.Preprocessing;

namespace TED.Primitives
{
    /// <summary>
    /// Implements conjunctions as a primitive predicate.  That is, And[g1, g2, ...] is true when all the gi are true.
    /// Procedurally, this means find solutions to g1, then g2, etc., backtracking in the normal manner.
    /// </summary>
    public sealed class AndPrimitive : Predicate
    {
        /// <summary>
        /// The And primitive itself
        /// </summary>
        public static readonly AndPrimitive Singleton = new AndPrimitive();
        private AndPrimitive() : base("And")
        { }

        /// <summary>
        /// True when all the subgoals are true
        /// </summary>
        public Interpreter.Goal this[params Interpreter.Goal[] subgoals]
        {
            get
            {
                var goals = Flatten(subgoals).ToArray();
                if (goals.Any(g => g.Predicate == Language.False))
                    return Language.False;
                return goals.Length switch
                {
                    0 => Language.True,
                    1 => goals[0],
                    _ => new Goal(goals)
                };
            }
        }

        private static IEnumerable<Interpreter.Goal> Flatten(Interpreter.Goal[] subgoals)
            => subgoals.Where(g => g.Predicate != Language.True).SelectMany(FlattenOne);

        private static Interpreter.Goal UnwrapGoalConstant(Constant<Interpreter.Goal> c) => c.Value;
        private static IEnumerable<Interpreter.Goal> FlattenOne(Interpreter.Goal g)
            => g.Predicate is AndPrimitive
                ? g.Arguments.SelectMany(subgoal => FlattenOne(UnwrapGoalConstant((Constant<Interpreter.Goal>)subgoal)))
                : new[] { g };

        private class Goal : Interpreter.Goal
        {
            private readonly Interpreter.Goal[] body;
            // ReSharper disable once CoVariantArrayConversion
            public Goal(params Interpreter.Goal[] body) : base(body.Select(g => new Constant<Interpreter.Goal>(g)).ToArray())
            {
                this.body = body;
            }

            public override Predicate Predicate => Singleton;

            internal override Interpreter.Goal RenameArguments(Substitution s)
                => new Goal(body.Select(g => g.RenameArguments(s)).ToArray());

            internal override Interpreter.Call MakeCall(GoalAnalyzer ga)
                => new Call(Preprocessor.GenerateCalls(ga, body), body);
        }

        internal class Call : Interpreter.Call
        {
            public readonly Interpreter.Call[] Body;
            public readonly Interpreter.Goal[] Source;
            private int currentCallIndex;

            public Call(Interpreter.Call[] body, Interpreter.Goal[] source) : base(Singleton)
            {
                Body = body;
                Source = source;
            }

            public Interpreter.Call CurrentCall => Body[currentCallIndex];

            public override IPattern ArgumentPattern
                => new Pattern<Interpreter.Goal[]>(MatchOperation<Interpreter.Goal[]>.Constant(Source));

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
