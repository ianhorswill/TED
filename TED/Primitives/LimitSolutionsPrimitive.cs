using System;
using TED.Compiler;
using TED.Interpreter;
using TED.Preprocessing;

namespace TED.Primitives
{
    /// <summary>
    /// Generates only the first solution of a goal
    /// </summary>
    public sealed class LimitSolutionsPrimitive : PrimitivePredicate<int,Goal>
    {
        /// <summary>
        /// The object implementing LimitSolutions
        /// </summary>
        public static LimitSolutionsPrimitive Singleton = new LimitSolutionsPrimitive();

        private LimitSolutionsPrimitive() : base("LimitSolutions")
        {
        }

        /// <inheritdoc />
        public override Call MakeCall(Goal g, GoalAnalyzer tc)
        {
            var count = g.Arg1 is Constant<int> c
                ? c.Value
                : throw new ArgumentException($"Argument {g.Arg1} to LimitSolutions must be a constant");
            switch (g.Arg2)
            {
                case Constant<Interpreter.Goal> target:
                    return new LimitCall(count, Preprocessor.BodyToCall(tc, target.Value));

                default:
                    throw new ArgumentException($"Argument {g.Arg2} to LimitSolutions must be a goal expression, not a variable");
            }
        }

        private class LimitCall : Call
        {
            private readonly int maxCount;
            private readonly Call call;
            private int currentCount;

            public LimitCall(int count, Call call) : base(Language.LimitSolutions)
            {
                maxCount = count;
                this.call = call;
            }

            public override IPattern ArgumentPattern => new Pattern<Call>(MatchOperation<Call>.Constant(call));

            public override void Reset()
            {
                currentCount = 0;
                call.Reset();
            }

            public override bool NextSolution()
            {
                if (currentCount >= maxCount) return false;
                currentCount++;
                return call.NextSolution();
            }

            public override Continuation Compile(Compiler.Compiler compiler, Continuation fail, string identifierSuffix)
            {
                var restart = new Continuation($"restart{identifierSuffix}");
                var success = new Continuation($"success{identifierSuffix}");
                var solutionCount = compiler.LocalVariable($"count{identifierSuffix}", typeof(int), "0");
                var restartGoal = compiler.CompileGoal(call, fail, identifierSuffix+"__ls");
                compiler.Indented($"{solutionCount}++;");
                compiler.Indented(success.Invoke+";");
                compiler.Label(restart);
                compiler.Indented($"if ({solutionCount} >= {maxCount}) {fail.Invoke};");
                compiler.Indented(restartGoal.Invoke+";");
                compiler.Label(success);
                return restart;
            }
        }
    }
}