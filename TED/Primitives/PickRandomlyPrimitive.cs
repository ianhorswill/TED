using TED.Compiler;
using TED.Interpreter;
using TED.Preprocessing;
using TED.Utilities;

namespace TED.Primitives
{
    internal sealed class PickRandomlyPrimitive<T> : PrimitivePredicate<T, T[]>
    {
        public static PickRandomlyPrimitive<T> Singleton = new PickRandomlyPrimitive<T>();

        /// <summary>
        /// Randomization does not behave like a pure predicate
        /// </summary>
        public override bool IsPure => false;
        
        public PickRandomlyPrimitive() : base("PickRandomly")
        {
        }

        public override Interpreter.Call MakeCall(Goal g, GoalAnalyzer tc)
        {
            var choices = ((Constant<T[]>)g.Arg2).Value;
            return new Call(this, choices, tc.Emit(g.Arg1));
        }

        private class Call : Interpreter.Call
        {
            private readonly T[] choices;
            private readonly MatchOperation<T> outputArg;
            public override IPattern ArgumentPattern => new Pattern<T>(outputArg);

            public Call(Predicate p, T[] choices, MatchOperation<T> outputArg) : base(p)
            {
                this.choices = choices;
                this.outputArg = outputArg;
            }

            private bool finished;

            private readonly System.Random rng = Random.MakeRng();

            public override void Reset() => finished = false;

            public override bool NextSolution()
            {
                var len = choices.Length;
                if (finished || len == 0) return false;
                finished = true;
                outputArg.Match(choices[(uint)rng.InRangeExclusive(0, len)]);
                return true;
            }

            public override Continuation Compile(Compiler.Compiler compiler, Continuation fail, string identifierSuffix)
            {
                var rng = compiler.MakeRng();
                var array = compiler.FieldUniqueName($"PickRandomlyArray", typeof(T[]),
                    compiler.ToSourceExpression(choices));
                if (choices.Length == 0)
                    compiler.Indented($"{fail.Invoke};");
                else if (outputArg.IsInstantiated)
                    compiler.Indented($"if ({compiler.ArgumentExpression(outputArg)} != {array}[{rng}.InRangeExclusive(0,{choices.Length})]) {fail.Invoke};");
                else
                    compiler.Indented($"{outputArg.Cell.Name} = {array}[{rng}.Next()%{choices.Length}];");
                return fail;
            }
        }
    }
}