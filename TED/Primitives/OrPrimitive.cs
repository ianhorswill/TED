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
    public sealed class OrPrimitive : Predicate
    {
        /// <summary>
        /// The And primitive itself
        /// </summary>
        public static readonly OrPrimitive Singleton = new OrPrimitive();
        private OrPrimitive() : base("Or")
        { }

        /// <summary>
        /// True when all the subgoals are true
        /// </summary>
        public Interpreter.Goal this[params Interpreter.Goal[] subgoals] => new Goal(Flatten(subgoals).ToArray());

        private static IEnumerable<Interpreter.Goal> Flatten(Interpreter.Goal[] subgoals) => subgoals.SelectMany(FlattenOne);

        private static Interpreter.Goal UnwrapGoalConstant(Constant<Interpreter.Goal> c) => c.Value;
        private static IEnumerable<Interpreter.Goal> FlattenOne(Interpreter.Goal g)
            => g.Predicate is OrPrimitive
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
            {
                (HashSet<Term> BoundVars, Interpreter.Call Call) ProcessBranch(Interpreter.Goal g)
                {
                    var localBindings = ga.MakeChild();
                    var call = g.MakeCall(localBindings);
                    return (localBindings.BoundVariables, call);
                }

                var branches = body.Select(ProcessBranch).ToArray();
                var finalSet = branches[0].BoundVars;
                foreach (var branch in branches)
                    finalSet.IntersectWith(branch.BoundVars);
                ga.BoundVariables.UnionWith(finalSet);
                return new Call(branches.Select(b => b.Call).ToArray(), body);
            }
        }

        internal class Call : Interpreter.Call
        {
            public readonly Interpreter.Call[] Body;
            public readonly Interpreter.Goal[] Source;
            private int currentBranchIndex;

            public Call(Interpreter.Call[] body, Interpreter.Goal[] source) : base(Singleton)
            {
                Body = body;
                Source = source;
            }

            public Interpreter.Call CurrentBranch => Body[currentBranchIndex];

            public override IPattern ArgumentPattern
                => new Pattern<Interpreter.Goal[]>(MatchOperation<Interpreter.Goal[]>.Constant(Source));

            public override void Reset()
            {
                currentBranchIndex = 0;
                CurrentBranch.Reset();
            }

            public override bool NextSolution()
            {
                while (true)
                {
                    if (CurrentBranch.NextSolution())
                        return true;
                    // Advance to the next subgoal
                    currentBranchIndex++;
                    if (currentBranchIndex == Body.Length)
                        break;
                    CurrentBranch.Reset();
                }

                return false;
            }
        }
    }
}
