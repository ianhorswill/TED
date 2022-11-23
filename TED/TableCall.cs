namespace TED
{
    internal class TableCall<T1> : AnyTableCall
    {
        public readonly TablePredicate<T1> TablePredicate;
        public readonly Pattern<T1> Pattern;

        public TableCall(TablePredicate<T1> predicate, Pattern<T1> pattern)
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

    internal class TableCall<T1, T2> : AnyTableCall
    {
        public readonly TablePredicate<T1, T2> TablePredicate;
        public readonly Pattern<T1, T2> Pattern;

        public TableCall(TablePredicate<T1, T2> predicate, Pattern<T1, T2> pattern)
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

    internal class TableCall<T1, T2, T3> : AnyTableCall
    {
        public readonly TablePredicate<T1, T2, T3> TablePredicate;
        public readonly Pattern<T1, T2, T3> Pattern;

        public TableCall(TablePredicate<T1, T2, T3> predicate, Pattern<T1, T2, T3> pattern)
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

    internal class TableCall<T1, T2, T3, T4> : AnyTableCall
    {
        public readonly TablePredicate<T1, T2, T3, T4> TablePredicate;
        public readonly Pattern<T1, T2, T3, T4> Pattern;

        public TableCall(TablePredicate<T1, T2, T3, T4> predicate, Pattern<T1, T2, T3, T4> pattern)
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

    internal class TableCall<T1, T2, T3, T4, T5> : AnyTableCall
    {
        public readonly TablePredicate<T1, T2, T3, T4, T5> TablePredicate;
        public readonly Pattern<T1, T2, T3, T4, T5> Pattern;

        public TableCall(TablePredicate<T1, T2, T3, T4, T5> predicate, Pattern<T1, T2, T3, T4, T5> pattern)
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

    internal class TableCall<T1, T2, T3, T4, T5, T6> : AnyTableCall
    {
        public readonly TablePredicate<T1, T2, T3, T4, T5, T6> TablePredicate;
        public readonly Pattern<T1, T2, T3, T4, T5, T6> Pattern;

        public TableCall(TablePredicate<T1, T2, T3, T4, T5, T6> predicate, Pattern<T1, T2, T3, T4, T5, T6> pattern)
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
