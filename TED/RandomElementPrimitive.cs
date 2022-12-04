namespace TED
{
    internal class RandomElementPrimitive<T> : PrimitivePredicate<TablePredicate<T>,T>
    {
        public static RandomElementPrimitive<T> Singleton = new RandomElementPrimitive<T>();
        public RandomElementPrimitive() : base("RandomElement")
        {
        }

        internal override AnyCall MakeCall(Goal g, GoalAnalyzer tc)
        {
            var predicate = ((Constant<TablePredicate<T>>)g.Arg1).Value;
            return new Call(predicate, tc.Emit(g.Arg2));
        }

        private class Call : AnyCall
        {
            private readonly TablePredicate<T> predicate;
            private readonly MatchOperation<T> outputArg;

            public override IPattern ArgumentPattern => new Pattern<T>(outputArg);

            public Call(TablePredicate<T> predicate, MatchOperation<T> outputArg) : base(predicate)
            {
                this.predicate = predicate;
                this.outputArg = outputArg;
            }

            private bool finished;

            public override void Reset() => finished = false;

            public override bool NextSolution()
            {
                var len = predicate.Length;
                if (finished || len == 0) return false;
                finished = true;
                outputArg.Match(predicate.Table[(uint)Random.InRangeExclusive(0, (int)len)]);
                return true;
            }
        }
    }
}
