using System;
using TED.Compiler;
using TED.Interpreter;
using TED.Preprocessing;

namespace TED.Primitives
{
    /// <summary>
    /// Just a type tag so we can test whether something is a EvalPrimitive
    /// </summary>
    internal interface IEvalPrimitive { }

    /// <summary>
    /// Implements evaluation of a functional expression and matching/storing it to a variable
    /// </summary>
    internal sealed class EvalPrimitive<T> : PrimitivePredicate<T, T>, IEvalPrimitive
    {
        public static EvalPrimitive<T> Singleton = new EvalPrimitive<T>();

        public EvalPrimitive() : base("Eval")
        {
            ConstantFolder = (Func<T,T,bool>)Comparer<T>.Default.Equals;
        }

        public override Call MakeCall(Goal g, GoalAnalyzer tc) =>
            new EvalCall(this, tc.Emit(g.Arg1), g.Arg2.MakeEvaluator(tc), g, g.Arg2.IsPure);

        private class EvalCall : Call
        {
            private readonly MatchOperation<T> matcher;
            private readonly Func<T> expressionEvaluator;
            private readonly Goal originalGoal;
            private readonly bool isPure;
            private bool restarted;

            /// <inheritdoc />
            // ReSharper disable once ConvertToAutoProperty
            public override bool IsPure => isPure;

            public EvalCall(Predicate p, MatchOperation<T> matcher, Func<T> expressionEvaluator, Goal originalGoal, bool pure) : base(p)
            {
                this.matcher = matcher;
                this.expressionEvaluator = expressionEvaluator;
                this.originalGoal = originalGoal;
                isPure = pure;
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

            public override Continuation Compile(Compiler.Compiler compiler, Continuation fail, string identifierSuffix)
            {
                var arg1 = originalGoal.Arg1.ToSourceExpression();
                var arg2 = originalGoal.Arg2.ToSourceExpression();
                switch (matcher.Opcode)
                {
                    case Opcode.Write:
                        compiler.Indented($"{arg1} = {arg2};");
                        break;

                    case Opcode.Read:
                    case Opcode.Constant:
                        compiler.Indented($"if ({arg1} != {arg2}) {fail.Invoke};");
                        break;

                    case Opcode.Ignore:
                        break;

                    default:
                        throw new NotImplementedException(
                            $"Can't compile Emit with first argument of opcode {matcher.Opcode}");
                }

                return fail;
            }

            public override string ToString() => $"{originalGoal.Arg1} == {originalGoal.Arg2}";
        }
    }
}
