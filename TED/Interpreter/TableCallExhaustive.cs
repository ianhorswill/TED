namespace TED.Interpreter
{
    /// <summary>
    /// The iterator that handles calls to a TablePredicate that cannot be accelerated using a hash table
    /// (index or RowSet).  This will test every row of the table against the call pattern.  This can be avoided
    /// under the right circumstances by adding an index and/or setting the Unique property of the TablePredicate.
    /// </summary>
    internal abstract class TableCallExhaustive : Call
    {
        /// <summary>
        /// What row we will text next in the table
        /// </summary>
        protected uint RowIndex;

        /// <summary>
        /// Move back to the beginning of the table.
        /// </summary>
        public override void Reset() => RowIndex = 0;

        protected TableCallExhaustive(Predicate predicate) : base(predicate)
        {
        }
    }

    //
    // This implements a call to a table predicate by exhaustively comparing the arguments to each row.
    // This is the call implementation of last resort and will not be used if there is an index or other hash
    // table that can be used to reduce the search.
    //

    internal class TableCallExhaustive<T1> : TableCallExhaustive
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
                if (Pattern.Match(in predicateTable.PositionReference(RowIndex++)))
                    return true;

            return false;
        }
    }

    internal class TableCallExhaustive<T1, T2> : TableCallExhaustive
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
                if (Pattern.Match(in predicateTable.PositionReference(RowIndex++)))
                    return true;

            return false;
        }
    }

    internal class TableCallExhaustive<T1, T2, T3> : TableCallExhaustive
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
                if (Pattern.Match(in predicateTable.PositionReference(RowIndex++)))
                    return true;

            return false;
        }
    }

    internal class TableCallExhaustive<T1, T2, T3, T4> : TableCallExhaustive
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
                if (Pattern.Match(in predicateTable.PositionReference(RowIndex++)))
                    return true;

            return false;
        }
    }

    internal class TableCallExhaustive<T1, T2, T3, T4, T5> : TableCallExhaustive
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
                if (Pattern.Match(in predicateTable.PositionReference(RowIndex++)))
                    return true;

            return false;
        }
    }

    internal class TableCallExhaustive<T1, T2, T3, T4, T5, T6> : TableCallExhaustive
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
                if (Pattern.Match(in predicateTable.PositionReference(RowIndex++)))
                    return true;

            return false;
        }
    }

    internal class TableCallExhaustive<T1, T2, T3, T4, T5, T6, T7> : TableCallExhaustive
    {
        public readonly TablePredicate<T1, T2, T3, T4, T5, T6, T7> TablePredicate;
        public readonly Pattern<T1, T2, T3, T4, T5, T6, T7> Pattern;
        public override IPattern ArgumentPattern => Pattern;

        public TableCallExhaustive(TablePredicate<T1, T2, T3, T4, T5, T6, T7> predicate, Pattern<T1, T2, T3, T4, T5, T6, T7> pattern) : base(predicate)
        {
            TablePredicate = predicate;
            Pattern = pattern;
        }

        public override bool NextSolution()
        {
            var predicateTable = TablePredicate.Table;
            while (RowIndex < predicateTable.Length)
                if (Pattern.Match(in predicateTable.PositionReference(RowIndex++)))
                    return true;

            return false;
        }
    }

    internal class TableCallExhaustive<T1, T2, T3, T4, T5, T6, T7, T8> : TableCallExhaustive
    {
        public readonly TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> TablePredicate;
        public readonly Pattern<T1, T2, T3, T4, T5, T6, T7, T8> Pattern;
        public override IPattern ArgumentPattern => Pattern;

        public TableCallExhaustive(TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> predicate, Pattern<T1, T2, T3, T4, T5, T6, T7, T8> pattern) : base(predicate)
        {
            TablePredicate = predicate;
            Pattern = pattern;
        }

        public override bool NextSolution()
        {
            var predicateTable = TablePredicate.Table;
            while (RowIndex < predicateTable.Length)
                if (Pattern.Match(in predicateTable.PositionReference(RowIndex++)))
                    return true;

            return false;
        }
    }
}
