using System.Collections.Generic;
using System.Linq;
using TED.Compiler;
using TED.Interpreter;
using TED.Preprocessing;

namespace TED.Primitives
{
    /// <summary>
    /// Succeeds with the first goal of the 
    /// </summary>
    public sealed class FirstOfPrimitive : Predicate
    {
        /// <summary>
        /// The And primitive itself
        /// </summary>
        public static readonly FirstOfPrimitive Singleton = new FirstOfPrimitive();
        private FirstOfPrimitive() : base("FirstOf")
        { }

        /// <summary>
        /// True when all the subgoals are true
        /// </summary>
        public Interpreter.Goal this[params Interpreter.Goal[] subgoals]
        {
            get
            {
                var goals = Flatten(subgoals).ToArray();
                if (goals.Any(g => g.Predicate == Language.True))
                    return Language.True;
                return goals.Length switch
                {
                    0 => Language.False,
                    1 => goals[0],
                    _ => new Goal(goals)
                };
            }
        }

        private static IEnumerable<Interpreter.Goal> Flatten(Interpreter.Goal[] subgoals) => subgoals.Where(g => g.Predicate != Language.False).SelectMany(FlattenOne);

        private static Interpreter.Goal UnwrapGoalConstant(Constant<Interpreter.Goal> c) => c.Value;
        private static IEnumerable<Interpreter.Goal> FlattenOne(Interpreter.Goal g)
            => g.Predicate is FirstOfPrimitive
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
            private bool restarted;

            public Call(Interpreter.Call[] body, Interpreter.Goal[] source) : base(Singleton)
            {
                Body = body;
                Source = source;
            }

            public override IPattern ArgumentPattern
                => new Pattern<Interpreter.Goal[]>(MatchOperation<Interpreter.Goal[]>.Constant(Source));

            public override void Reset()
            {
                restarted = true;
                Body[0].Reset();
            }

            public override bool NextSolution()
            {
                if (!restarted) return false;
                var currentBranchIndex = 0;
                while (true)
                {
                    if (Body[currentBranchIndex].NextSolution())
                    {
                        restarted = false;
                        return true;
                    }
                    // Advance to the next subgoal
                    currentBranchIndex++;
                    if (currentBranchIndex == Body.Length)
                        break;
                    Body[currentBranchIndex].Reset();
                }

                return false;
            }

            public override Continuation Compile(Compiler.Compiler compiler, Continuation fail, string identifierSuffix)
            {
                var success = new Continuation("firstOfSuccess" + identifierSuffix);
                Continuation? nextLabel = null;
                for (var i = 0; i < Body.Length; i++)
                {
                    if (nextLabel != null)
                    {
                        compiler.NewLine();
                        compiler.Label(nextLabel);
                    }
                    nextLabel = i == Body.Length - 1
                        ? fail
                        : new Continuation($"firstOFBranch{i + 1}{identifierSuffix}");
                    compiler.CompileGoal(Body[i], nextLabel, $"{identifierSuffix}_{i}");
                    compiler.Indented(success.Invoke+";");
                }
                compiler.NewLine();
                compiler.Label(success, true);
                return fail;
            }
        }
    }
}
