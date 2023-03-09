namespace TED
{
    //
    // This implements calls to TablePredicates where:
    // - The call is fully instantiated, meaning all the arguments will be known at run-time, rather than needing
    //   to fill some or all in from the table data
    // - The predicate has the Unique property, meaning that it keeps a hash set of the tuples in the table
    //
    // This is the most desirable call implementation, since it's just a hash set lookup.
    //

    internal class TableCallUsingRowSet<T1> : SingleRowTableCall
    {
        private readonly TablePredicate<T1> predicate;
        private readonly Pattern<T1> pattern;

        public TableCallUsingRowSet(TablePredicate<T1> tablePredicate, Pattern<T1> pattern) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
        }

        public override IPattern ArgumentPattern => pattern;
        public override bool NextSolution()
        {
            if (!primed) return false;
            primed = false;
            return predicate.Table.ContainsRowUsingRowSet(pattern.Value);
        }
    }

    internal class TableCallUsingRowSet<T1, T2> : SingleRowTableCall
    {
        private readonly TablePredicate<T1, T2> predicate;
        private readonly Pattern<T1, T2> pattern;

        public TableCallUsingRowSet(TablePredicate<T1, T2> tablePredicate, Pattern<T1, T2> pattern) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            if (!primed) return false;
            primed = false;
            return predicate.Table.ContainsRowUsingRowSet(pattern.Value);
        }
    }

    

    internal class TableCallUsingRowSet<T1,T2,T3> : SingleRowTableCall
    {
        private readonly TablePredicate<T1,T2,T3> predicate;
        private readonly Pattern<T1,T2,T3> pattern;

        public TableCallUsingRowSet(TablePredicate<T1,T2,T3> tablePredicate, Pattern<T1,T2,T3> pattern) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            if (!primed) return false;
            primed = false;
            return predicate.Table.ContainsRowUsingRowSet(pattern.Value);
        }
    }

    internal class TableCallUsingRowSet<T1,T2,T3,T4> : SingleRowTableCall
    {
        private readonly TablePredicate<T1,T2,T3,T4> predicate;
        private readonly Pattern<T1,T2,T3,T4> pattern;

        public TableCallUsingRowSet(TablePredicate<T1,T2,T3,T4> tablePredicate, Pattern<T1,T2,T3,T4> pattern) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            if (!primed) return false;
            primed = false;
            return predicate.Table.ContainsRowUsingRowSet(pattern.Value);
        }
    }

    internal class TableCallUsingRowSet<T1,T2,T3,T4,T5> : SingleRowTableCall
    {
        private readonly TablePredicate<T1,T2,T3,T4,T5> predicate;
        private readonly Pattern<T1,T2,T3,T4,T5> pattern;

        public TableCallUsingRowSet(TablePredicate<T1,T2,T3,T4,T5> tablePredicate, Pattern<T1,T2,T3,T4,T5> pattern) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            if (!primed) return false;
            primed = false;
            return predicate.Table.ContainsRowUsingRowSet(pattern.Value);
        }
    }

    internal class TableCallUsingRowSet<T1,T2,T3,T4,T5,T6> : SingleRowTableCall
    {
        private readonly TablePredicate<T1,T2,T3,T4,T5,T6> predicate;
        private readonly Pattern<T1,T2,T3,T4,T5,T6> pattern;

        public TableCallUsingRowSet(TablePredicate<T1,T2,T3,T4,T5,T6> tablePredicate, Pattern<T1,T2,T3,T4,T5,T6> pattern) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            if (!primed) return false;
            primed = false;
            return predicate.Table.ContainsRowUsingRowSet(pattern.Value);
        }
    }

    internal class TableCallUsingRowSet<T1, T2, T3, T4, T5, T6, T7> : SingleRowTableCall {
        private readonly TablePredicate<T1, T2, T3, T4, T5, T6, T7> predicate;
        private readonly Pattern<T1, T2, T3, T4, T5, T6, T7> pattern;

        public TableCallUsingRowSet(TablePredicate<T1, T2, T3, T4, T5, T6, T7> tablePredicate, Pattern<T1, T2, T3, T4, T5, T6, T7> pattern) : base(tablePredicate) {
            predicate = tablePredicate;
            this.pattern = pattern;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution() {
            if (!primed) return false;
            primed = false;
            return predicate.Table.ContainsRowUsingRowSet(pattern.Value);
        }
    }

    internal class TableCallUsingRowSet<T1, T2, T3, T4, T5, T6, T7, T8> : SingleRowTableCall {
        private readonly TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> predicate;
        private readonly Pattern<T1, T2, T3, T4, T5, T6, T7, T8> pattern;

        public TableCallUsingRowSet(TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> tablePredicate, Pattern<T1, T2, T3, T4, T5, T6, T7, T8> pattern) : base(tablePredicate) {
            predicate = tablePredicate;
            this.pattern = pattern;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution() {
            if (!primed) return false;
            primed = false;
            return predicate.Table.ContainsRowUsingRowSet(pattern.Value);
        }
    }
}
