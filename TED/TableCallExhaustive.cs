namespace TED
{
    internal class TableCallExhaustive<T1> : AnyTableCallExhaustive
    {
        public readonly TablePredicate<T1> TablePredicate;
        public readonly Pattern<T1> Pattern;
        public override IPattern ArgumentPattern => Pattern;

        public TableCallExhaustive(TablePredicate<T1> predicate, Pattern<T1> pattern) : base(predicate)
        {
            TablePredicate = predicate;
            Pattern = pattern;
        }

        public override bool NextSolution()
        {
            var predicateTable = TablePredicate._table;
            while (RowIndex < predicateTable.Length)
                if (Pattern.Match(predicateTable.PositionReference(RowIndex++)))
                    return true;

            return false;
        }
    }

    internal class TableCallExhaustive<T1, T2> : AnyTableCallExhaustive
    {
        public readonly TablePredicate<T1, T2> TablePredicate;
        public readonly Pattern<T1, T2> Pattern;
        public override IPattern ArgumentPattern => Pattern;

        public TableCallExhaustive(TablePredicate<T1, T2> predicate, Pattern<T1, T2> pattern) : base(predicate)
        {
            TablePredicate = predicate;
            Pattern = pattern;
        }

        public override bool NextSolution()
        {
            var predicateTable = TablePredicate._table;
            while (RowIndex < predicateTable.Length)
                if (Pattern.Match(predicateTable.PositionReference(RowIndex++)))
                    return true;

            return false;
        }
    }

    internal class TableCallExhaustive<T1, T2, T3> : AnyTableCallExhaustive
    {
        public readonly TablePredicate<T1, T2, T3> TablePredicate;
        public readonly Pattern<T1, T2, T3> Pattern;
        public override IPattern ArgumentPattern => Pattern;

        public TableCallExhaustive(TablePredicate<T1, T2, T3> predicate, Pattern<T1, T2, T3> pattern) : base(predicate)
        {
            TablePredicate = predicate;
            Pattern = pattern;
        }

        public override bool NextSolution()
        {
            var predicateTable = TablePredicate.Table;
            while (RowIndex < predicateTable.Length)
                if (Pattern.Match(predicateTable.PositionReference(RowIndex++)))
                    return true;

            return false;
        }
    }

    internal class TableCallExhaustive<T1, T2, T3, T4> : AnyTableCallExhaustive
    {
        public readonly TablePredicate<T1, T2, T3, T4> TablePredicate;
        public readonly Pattern<T1, T2, T3, T4> Pattern;
        public override IPattern ArgumentPattern => Pattern;

        public TableCallExhaustive(TablePredicate<T1, T2, T3, T4> predicate, Pattern<T1, T2, T3, T4> pattern) : base(predicate)
        {
            TablePredicate = predicate;
            Pattern = pattern;
        }

        public override bool NextSolution()
        {
            var predicateTable = TablePredicate.Table;
            while (RowIndex < predicateTable.Length)
                if (Pattern.Match(predicateTable.PositionReference(RowIndex++)))
                    return true;

            return false;
        }
    }

    internal class TableCallExhaustive<T1, T2, T3, T4, T5> : AnyTableCallExhaustive
    {
        public readonly TablePredicate<T1, T2, T3, T4, T5> TablePredicate;
        public readonly Pattern<T1, T2, T3, T4, T5> Pattern;
        public override IPattern ArgumentPattern => Pattern;

        public TableCallExhaustive(TablePredicate<T1, T2, T3, T4, T5> predicate, Pattern<T1, T2, T3, T4, T5> pattern) : base(predicate)
        {
            TablePredicate = predicate;
            Pattern = pattern;
        }

        public override bool NextSolution()
        {
            var predicateTable = TablePredicate.Table;
            while (RowIndex < predicateTable.Length)
                if (Pattern.Match(predicateTable.PositionReference(RowIndex++)))
                    return true;

            return false;
        }
    }

    internal class TableCallExhaustive<T1, T2, T3, T4, T5, T6> : AnyTableCallExhaustive
    {
        public readonly TablePredicate<T1, T2, T3, T4, T5, T6> TablePredicate;
        public readonly Pattern<T1, T2, T3, T4, T5, T6> Pattern;
        public override IPattern ArgumentPattern => Pattern;

        public TableCallExhaustive(TablePredicate<T1, T2, T3, T4, T5, T6> predicate, Pattern<T1, T2, T3, T4, T5, T6> pattern) : base(predicate)
        {
            TablePredicate = predicate;
            Pattern = pattern;
        }

        public override bool NextSolution()
        {
            var predicateTable = TablePredicate.Table;
            while (RowIndex < predicateTable.Length)
                if (Pattern.Match(predicateTable.PositionReference(RowIndex++)))
                    return true;

            return false;
        }
    }
}
