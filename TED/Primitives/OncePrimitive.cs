using System;
using TED.Compiler;
using TED.Interpreter;
using TED.Preprocessing;

namespace TED.Primitives
{
    /// <summary>
    /// Generates only the first solution of a goal
    /// </summary>
    public sealed class OncePrimitive : PrimitivePredicate<Goal>
    {
        /// <summary>
        /// The object implementing Once
        /// </summary>
        public static OncePrimitive Singleton = new OncePrimitive();

        private OncePrimitive() : base("Once")
        {
        }

        /// <inheritdoc />
        public override Call MakeCall(Goal g, GoalAnalyzer tc)
        {
            switch (g.Arg1)
            {
                case Constant<Interpreter.Goal> target:
                    return new OnceCall(Preprocessor.BodyToCall(tc, target.Value));

                default:
                    throw new ArgumentException($"Argument {g.Arg1} to Once must be a goal expression, not a variable");
            }
        }

        private class OnceCall : Call
        {
            private readonly Call call;
            private bool restarted;

            public OnceCall(Call call) : base(Language.Once)
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
                return call.NextSolution();
            }

            public override Continuation Compile(Compiler.Compiler compiler, Continuation fail, string identifierSuffix)
            {
                compiler.CompileGoal(call, fail, identifierSuffix);
                return fail;
            }
        }
    }
}
