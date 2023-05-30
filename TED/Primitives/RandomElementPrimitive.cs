using TED.Interpreter;
using TED.Preprocessing;
using TED.Utilities;

namespace TED.Primitives {
    /// <summary>
    /// Implementation of the RandomElement primitive
    /// </summary>
    /// <typeparam name="T">Type of list element we're selecting from</typeparam>
    internal sealed class RandomElementPrimitive<T> : PrimitivePredicate<TablePredicate<T>, T> {
        public static RandomElementPrimitive<T> Singleton = new RandomElementPrimitive<T>();
        public RandomElementPrimitive() : base("RandomElement") { }

        
        /// <summary>
        /// Randomization does not behave like a pure predicate
        /// </summary>
        public override bool IsPure => false;

        public override Interpreter.Call MakeCall(Goal g, GoalAnalyzer tc)
        {
            var predicate = ((Constant<TablePredicate<T>>)g.Arg1).Value;
            return new Call(predicate, tc.Emit(g.Arg2));
        }

        private class Call : Interpreter.Call
        {
            private readonly TablePredicate<T> table;
            private readonly MatchOperation<T> outputArg;

            public override IPattern ArgumentPattern => new Pattern<T>(outputArg);

            public Call(TablePredicate<T> table, MatchOperation<T> outputArg) : base(Singleton)
            {
                this.table = table;
                this.outputArg = outputArg;
            }

            private bool finished;

            public override void Reset() => finished = false;

            public override bool NextSolution()
            {
                var len = table.Length;
                if (finished || len == 0) return false;
                finished = true;
                outputArg.Match(table.Table[(uint)Random.InRangeExclusive(0, (int)len)]);
                return true;
            }
        }
    }

    /// <summary>
    /// Implementation of the RandomElement primitive
    /// </summary>
    /// <typeparam name="T1">Type of the first column from the table we're selecting from</typeparam>
    /// <typeparam name="T2">Type of the second column from the table we're selecting from</typeparam>
    internal sealed class RandomElementPrimitive<T1, T2> : PrimitivePredicate<TablePredicate<T1, T2>, (T1, T2)> {
        public static RandomElementPrimitive<T1, T2> Singleton = new RandomElementPrimitive<T1, T2>();
        public RandomElementPrimitive() : base("RandomElement") { }

        public override Interpreter.Call MakeCall(Goal g, GoalAnalyzer tc) {
            var predicate = ((Constant<TablePredicate<T1, T2>>)g.Arg1).Value;
            return new Call(predicate, tc.Emit(g.Arg2));
        }

        private class Call : Interpreter.Call {
            private readonly TablePredicate<T1, T2> predicate;
            private readonly MatchOperation<(T1, T2)> outputArg;

            public override IPattern ArgumentPattern => new Pattern<(T1, T2)>(outputArg);

            public Call(TablePredicate<T1, T2> predicate, MatchOperation<(T1, T2)> outputArg) : base(predicate) {
                this.predicate = predicate;
                this.outputArg = outputArg;
            }

            private bool finished;

            public override void Reset() => finished = false;

            public override bool NextSolution() {
                var len = predicate.Length;
                if (finished || len == 0) return false;
                finished = true;
                outputArg.Match(predicate.Table[(uint)Random.InRangeExclusive(0, (int)len)]);
                return true;
            }
        }
    }

    /// <summary>
    /// Implementation of the RandomElement primitive
    /// </summary>
    /// <typeparam name="T1">Type of the first column from the table we're selecting from</typeparam>
    /// <typeparam name="T2">Type of the second column from the table we're selecting from</typeparam>
    /// <typeparam name="T3">Type of the third column from the table we're selecting from</typeparam>
    internal sealed class RandomElementPrimitive<T1, T2, T3> : PrimitivePredicate<TablePredicate<T1, T2, T3>, (T1, T2, T3)> {
        public static RandomElementPrimitive<T1, T2, T3> Singleton = new RandomElementPrimitive<T1, T2, T3>();
        public RandomElementPrimitive() : base("RandomElement") { }

        public override Interpreter.Call MakeCall(Goal g, GoalAnalyzer tc) {
            var predicate = ((Constant<TablePredicate<T1, T2, T3>>)g.Arg1).Value;
            return new Call(predicate, tc.Emit(g.Arg2)); }

        private class Call : Interpreter.Call {
            private readonly TablePredicate<T1, T2, T3> predicate;
            private readonly MatchOperation<(T1, T2, T3)> outputArg;

            public override IPattern ArgumentPattern => new Pattern<(T1, T2, T3)>(outputArg);

            public Call(TablePredicate<T1, T2, T3> predicate, MatchOperation<(T1, T2, T3)> outputArg) : base(predicate) {
                this.predicate = predicate;
                this.outputArg = outputArg; }

            private bool finished;

            public override void Reset() => finished = false;

            public override bool NextSolution() {
                var len = predicate.Length;
                if (finished || len == 0) return false;
                finished = true;
                outputArg.Match(predicate.Table[(uint)Random.InRangeExclusive(0, (int)len)]);
                return true;
            }
        }
    }

    /// <summary>
    /// Implementation of the RandomElement primitive
    /// </summary>
    /// <typeparam name="T1">Type of the first column from the table we're selecting from</typeparam>
    /// <typeparam name="T2">Type of the second column from the table we're selecting from</typeparam>
    /// <typeparam name="T3">Type of the third column from the table we're selecting from</typeparam>
    /// <typeparam name="T4">Type of the fourth column from the table we're selecting from</typeparam>
    internal sealed class RandomElementPrimitive<T1, T2, T3, T4> : PrimitivePredicate<TablePredicate<T1, T2, T3, T4>, (T1, T2, T3, T4)> {
        public static RandomElementPrimitive<T1, T2, T3, T4> Singleton = new RandomElementPrimitive<T1, T2, T3, T4>();
        public RandomElementPrimitive() : base("RandomElement") { }

        public override Interpreter.Call MakeCall(Goal g, GoalAnalyzer tc) {
            var predicate = ((Constant<TablePredicate<T1, T2, T3, T4>>)g.Arg1).Value;
            return new Call(predicate, tc.Emit(g.Arg2));
        }

        private class Call : Interpreter.Call {
            private readonly TablePredicate<T1, T2, T3, T4> predicate;
            private readonly MatchOperation<(T1, T2, T3, T4)> outputArg;

            public override IPattern ArgumentPattern => new Pattern<(T1, T2, T3, T4)>(outputArg);

            public Call(TablePredicate<T1, T2, T3, T4> predicate, MatchOperation<(T1, T2, T3, T4)> outputArg) : base(predicate) {
                this.predicate = predicate;
                this.outputArg = outputArg;
            }

            private bool finished;

            public override void Reset() => finished = false;

            public override bool NextSolution() {
                var len = predicate.Length;
                if (finished || len == 0) return false;
                finished = true;
                outputArg.Match(predicate.Table[(uint)Random.InRangeExclusive(0, (int)len)]);
                return true;
            }
        }
    }

    /// <summary>
    /// Implementation of the RandomElement primitive
    /// </summary>
    /// <typeparam name="T1">Type of the first column from the table we're selecting from</typeparam>
    /// <typeparam name="T2">Type of the second column from the table we're selecting from</typeparam>
    /// <typeparam name="T3">Type of the third column from the table we're selecting from</typeparam>
    /// <typeparam name="T4">Type of the fourth column from the table we're selecting from</typeparam>
    /// <typeparam name="T5">Type of the fifth column from the table we're selecting from</typeparam>
    internal sealed class RandomElementPrimitive<T1, T2, T3, T4, T5> : PrimitivePredicate<TablePredicate<T1, T2, T3, T4, T5>, (T1, T2, T3, T4, T5)> {
        public static RandomElementPrimitive<T1, T2, T3, T4, T5> Singleton = new RandomElementPrimitive<T1, T2, T3, T4, T5>();
        public RandomElementPrimitive() : base("RandomElement") { }

        public override Interpreter.Call MakeCall(Goal g, GoalAnalyzer tc) {
            var predicate = ((Constant<TablePredicate<T1, T2, T3, T4, T5>>)g.Arg1).Value;
            return new Call(predicate, tc.Emit(g.Arg2));
        }

        private class Call : Interpreter.Call {
            private readonly TablePredicate<T1, T2, T3, T4, T5> predicate;
            private readonly MatchOperation<(T1, T2, T3, T4, T5)> outputArg;

            public override IPattern ArgumentPattern => new Pattern<(T1, T2, T3, T4, T5)>(outputArg);

            public Call(TablePredicate<T1, T2, T3, T4, T5> predicate, MatchOperation<(T1, T2, T3, T4, T5)> outputArg) : base(predicate) {
                this.predicate = predicate;
                this.outputArg = outputArg;
            }

            private bool finished;

            public override void Reset() => finished = false;

            public override bool NextSolution() {
                var len = predicate.Length;
                if (finished || len == 0) return false;
                finished = true;
                outputArg.Match(predicate.Table[(uint)Random.InRangeExclusive(0, (int)len)]);
                return true;
            }
        }
    }

    /// <summary>
    /// Implementation of the RandomElement primitive
    /// </summary>
    /// <typeparam name="T1">Type of the first column from the table we're selecting from</typeparam>
    /// <typeparam name="T2">Type of the second column from the table we're selecting from</typeparam>
    /// <typeparam name="T3">Type of the third column from the table we're selecting from</typeparam>
    /// <typeparam name="T4">Type of the fourth column from the table we're selecting from</typeparam>
    /// <typeparam name="T5">Type of the fifth column from the table we're selecting from</typeparam>
    /// <typeparam name="T6">Type of the sixth column from the table we're selecting from</typeparam>
    internal sealed class RandomElementPrimitive<T1, T2, T3, T4, T5, T6> : 
        PrimitivePredicate<TablePredicate<T1, T2, T3, T4, T5, T6>, (T1, T2, T3, T4, T5, T6)> {
        public static RandomElementPrimitive<T1, T2, T3, T4, T5, T6> Singleton = new RandomElementPrimitive<T1, T2, T3, T4, T5, T6>();
        public RandomElementPrimitive() : base("RandomElement") { }

        public override Interpreter.Call MakeCall(Goal g, GoalAnalyzer tc) {
            var predicate = ((Constant<TablePredicate<T1, T2, T3, T4, T5, T6>>)g.Arg1).Value;
            return new Call(predicate, tc.Emit(g.Arg2));
        }

        private class Call : Interpreter.Call {
            private readonly TablePredicate<T1, T2, T3, T4, T5, T6> predicate;
            private readonly MatchOperation<(T1, T2, T3, T4, T5, T6)> outputArg;

            public override IPattern ArgumentPattern => new Pattern<(T1, T2, T3, T4, T5, T6)>(outputArg);

            public Call(TablePredicate<T1, T2, T3, T4, T5, T6> predicate, MatchOperation<(T1, T2, T3, T4, T5, T6)> outputArg) : base(predicate) {
                this.predicate = predicate;
                this.outputArg = outputArg;
            }

            private bool finished;

            public override void Reset() => finished = false;

            public override bool NextSolution() {
                var len = predicate.Length;
                if (finished || len == 0) return false;
                finished = true;
                outputArg.Match(predicate.Table[(uint)Random.InRangeExclusive(0, (int)len)]);
                return true;
            }
        }
    }

    /// <summary>
    /// Implementation of the RandomElement primitive
    /// </summary>
    /// <typeparam name="T1">Type of the first column from the table we're selecting from</typeparam>
    /// <typeparam name="T2">Type of the second column from the table we're selecting from</typeparam>
    /// <typeparam name="T3">Type of the third column from the table we're selecting from</typeparam>
    /// <typeparam name="T4">Type of the fourth column from the table we're selecting from</typeparam>
    /// <typeparam name="T5">Type of the fifth column from the table we're selecting from</typeparam>
    /// <typeparam name="T6">Type of the sixth column from the table we're selecting from</typeparam>
    /// <typeparam name="T7">Type of the seventh column from the table we're selecting from</typeparam>
    internal sealed class RandomElementPrimitive<T1, T2, T3, T4, T5, T6, T7> :
        PrimitivePredicate<TablePredicate<T1, T2, T3, T4, T5, T6, T7>, (T1, T2, T3, T4, T5, T6, T7)> {
        public static RandomElementPrimitive<T1, T2, T3, T4, T5, T6, T7> Singleton = new RandomElementPrimitive<T1, T2, T3, T4, T5, T6, T7>();
        public RandomElementPrimitive() : base("RandomElement") { }

        public override Interpreter.Call MakeCall(Goal g, GoalAnalyzer tc) {
            var predicate = ((Constant<TablePredicate<T1, T2, T3, T4, T5, T6, T7>>)g.Arg1).Value;
            return new Call(predicate, tc.Emit(g.Arg2));
        }

        private class Call : Interpreter.Call {
            private readonly TablePredicate<T1, T2, T3, T4, T5, T6, T7> predicate;
            private readonly MatchOperation<(T1, T2, T3, T4, T5, T6, T7)> outputArg;

            public override IPattern ArgumentPattern => new Pattern<(T1, T2, T3, T4, T5, T6, T7)>(outputArg);

            public Call(TablePredicate<T1, T2, T3, T4, T5, T6, T7> predicate, 
                MatchOperation<(T1, T2, T3, T4, T5, T6, T7)> outputArg) : base(predicate) {
                this.predicate = predicate;
                this.outputArg = outputArg;
            }

            private bool finished;

            public override void Reset() => finished = false;

            public override bool NextSolution() {
                var len = predicate.Length;
                if (finished || len == 0) return false;
                finished = true;
                outputArg.Match(predicate.Table[(uint)Random.InRangeExclusive(0, (int)len)]);
                return true;
            }
        }
    }

    /// <summary>
    /// Implementation of the RandomElement primitive
    /// </summary>
    /// <typeparam name="T1">Type of the first column from the table we're selecting from</typeparam>
    /// <typeparam name="T2">Type of the second column from the table we're selecting from</typeparam>
    /// <typeparam name="T3">Type of the third column from the table we're selecting from</typeparam>
    /// <typeparam name="T4">Type of the fourth column from the table we're selecting from</typeparam>
    /// <typeparam name="T5">Type of the fifth column from the table we're selecting from</typeparam>
    /// <typeparam name="T6">Type of the sixth column from the table we're selecting from</typeparam>
    /// <typeparam name="T7">Type of the seventh column from the table we're selecting from</typeparam>
    /// <typeparam name="T8">Type of the seventh column from the table we're selecting from</typeparam>
    internal sealed class RandomElementPrimitive<T1, T2, T3, T4, T5, T6, T7, T8> :
        PrimitivePredicate<TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8>, (T1, T2, T3, T4, T5, T6, T7, T8)> {
        public static RandomElementPrimitive<T1, T2, T3, T4, T5, T6, T7, T8> Singleton = new RandomElementPrimitive<T1, T2, T3, T4, T5, T6, T7, T8>();
        public RandomElementPrimitive() : base("RandomElement") { }

        public override Interpreter.Call MakeCall(Goal g, GoalAnalyzer tc) {
            var predicate = ((Constant<TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8>>)g.Arg1).Value;
            return new Call(predicate, tc.Emit(g.Arg2));
        }

        private class Call : Interpreter.Call {
            private readonly TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> predicate;
            private readonly MatchOperation<(T1, T2, T3, T4, T5, T6, T7, T8)> outputArg;

            public override IPattern ArgumentPattern => new Pattern<(T1, T2, T3, T4, T5, T6, T7, T8)>(outputArg);

            public Call(TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> predicate,
                MatchOperation<(T1, T2, T3, T4, T5, T6, T7, T8)> outputArg) : base(predicate) {
                this.predicate = predicate;
                this.outputArg = outputArg;
            }

            private bool finished;

            public override void Reset() => finished = false;

            public override bool NextSolution() {
                var len = predicate.Length;
                if (finished || len == 0) return false;
                finished = true;
                outputArg.Match(predicate.Table[(uint)Random.InRangeExclusive(0, (int)len)]);
                return true;
            }
        }
    }
}
