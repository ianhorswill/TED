namespace TED
{
    internal class PickRandomlyPrimitive<T> : PrimitivePredicate<T, T[]>
    {
        public static PickRandomlyPrimitive<T> Singleton = new PickRandomlyPrimitive<T>();
        public PickRandomlyPrimitive() : base("PickRandomly")
        {
        }

        internal override AnyCall MakeCall(Goal g, GoalAnalyzer tc)
        {
            var choices = ((Constant<T[]>)g.Arg2).Value;
            return new Call(choices, tc.Emit(g.Arg1));
        }

        private class Call : AnyCall
        {
            private readonly T[] choices;
            private readonly MatchOperation<T> outputArg;

            public Call(T[] choices, MatchOperation<T> outputArg)
            {
                this.choices = choices;
                this.outputArg = outputArg;
            }

            private bool finished;

            public override void Reset() => finished = false;

            public override bool NextSolution()
            {
                var len = choices.Length;
                if (finished || len == 0) return false;
                finished = true;
                outputArg.Match(choices[(uint)Random.InRangeExclusive(0, len)]);
                return true;
            }
        }
    }
}