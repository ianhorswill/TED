namespace TED
{
    internal class TableCallTest<T1> : AnyTableCallTest
    {
        private readonly TablePredicate<T1> predicate;
        private readonly Pattern<T1> pattern;

        public TableCallTest(TablePredicate<T1> tablePredicate, Pattern<T1> pattern) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
        }

        public override IPattern ArgumentPattern => pattern;
        public override bool NextSolution()
        {
            if (!primed) return false;
            primed = false;
            return predicate.Table.ContainsRow(pattern.Value);
        }
    }

    internal class TableCallTest<T1, T2> : AnyTableCallTest
    {
        private readonly TablePredicate<T1, T2> predicate;
        private readonly Pattern<T1, T2> pattern;

        public TableCallTest(TablePredicate<T1, T2> tablePredicate, Pattern<T1, T2> pattern) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            if (!primed) return false;
            primed = false;
            return predicate.Table.ContainsRow(pattern.Value);
        }
    }

    

    internal class TableCallTest<T1,T2,T3> : AnyTableCallTest
    {
        private readonly TablePredicate<T1,T2,T3> predicate;
        private readonly Pattern<T1,T2,T3> pattern;

        public TableCallTest(TablePredicate<T1,T2,T3> tablePredicate, Pattern<T1,T2,T3> pattern) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            if (!primed) return false;
            primed = false;
            return predicate.Table.ContainsRow(pattern.Value);
        }
    }

    internal class TableCallTest<T1,T2,T3,T4> : AnyTableCallTest
    {
        private readonly TablePredicate<T1,T2,T3,T4> predicate;
        private readonly Pattern<T1,T2,T3,T4> pattern;

        public TableCallTest(TablePredicate<T1,T2,T3,T4> tablePredicate, Pattern<T1,T2,T3,T4> pattern) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            if (!primed) return false;
            primed = false;
            return predicate.Table.ContainsRow(pattern.Value);
        }
    }

    internal class TableCallTest<T1,T2,T3,T4,T5> : AnyTableCallTest
    {
        private readonly TablePredicate<T1,T2,T3,T4,T5> predicate;
        private readonly Pattern<T1,T2,T3,T4,T5> pattern;

        public TableCallTest(TablePredicate<T1,T2,T3,T4,T5> tablePredicate, Pattern<T1,T2,T3,T4,T5> pattern) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            if (!primed) return false;
            primed = false;
            return predicate.Table.ContainsRow(pattern.Value);
        }
    }

    internal class TableCallTest<T1,T2,T3,T4,T5,T6> : AnyTableCallTest
    {
        private readonly TablePredicate<T1,T2,T3,T4,T5,T6> predicate;
        private readonly Pattern<T1,T2,T3,T4,T5,T6> pattern;

        public TableCallTest(TablePredicate<T1,T2,T3,T4,T5,T6> tablePredicate, Pattern<T1,T2,T3,T4,T5,T6> pattern) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            if (!primed) return false;
            primed = false;
            return predicate.Table.ContainsRow(pattern.Value);
        }
    }
}
