using TED.Tables;

namespace TED.Interpreter
{
    //
    // This implements calls to TablePredicates where some argument in the call is instantiated (known at run time
    // and not needing to be filled in from table data) and that argument/column has a GeneralIndex.  This will
    // allow it to iterate over only just the rows that match that column.
    //
    // This will not be used if it's possible to use TableCallWithKey or TableCallUsingRowSet, which are preferable.
    //

    internal abstract class TableCallWithGeneralIndex : Call
    {
        protected TableCallWithGeneralIndex(TablePredicate p) : base(p)
        { }

        protected bool Primed;
        protected uint Row;

        public override void Reset() => Primed = true;
    }

    internal class TableCallWithGeneralIndex<TKey, T1, T2> : TableCallWithGeneralIndex
    {
        private readonly TablePredicate<T1, T2> predicate;
        private readonly Pattern<T1, T2> pattern;
        private readonly GeneralIndex<(T1, T2), TKey> index;
        private readonly ValueCell<TKey> keyCell;

        public TableCallWithGeneralIndex(TablePredicate<T1, T2> tablePredicate, Pattern<T1, T2> pattern, GeneralIndex<(T1, T2), TKey> index, ValueCell<TKey> keyCell) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell = keyCell;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            Row = Primed ? index.FirstRowWithValue(in keyCell.Value) : index.NextRowWithValue(Row);
            Primed = false;

            while (Row != Table.NoRow && !pattern.Match(in predicate._table.PositionReference(Row)))
                Row = index.NextRowWithValue(Row);

            return Row != Table.NoRow;
        }
    }

    internal class TableCallWithGeneralIndex<TKey, T1, T2, T3> : TableCallWithGeneralIndex
    {
        private readonly TablePredicate<T1, T2, T3> predicate;
        private readonly Pattern<T1, T2, T3> pattern;
        private readonly GeneralIndex<(T1, T2, T3), TKey> index;
        private readonly ValueCell<TKey> keyCell;

        public TableCallWithGeneralIndex(TablePredicate<T1, T2, T3> tablePredicate, Pattern<T1, T2, T3> pattern, GeneralIndex<(T1, T2, T3), TKey> index, ValueCell<TKey> keyCell) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell = keyCell;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            Row = Primed ? index.FirstRowWithValue(in keyCell.Value) : index.NextRowWithValue(Row);
            Primed = false;

            while (Row != Table.NoRow && !pattern.Match(in predicate._table.PositionReference(Row)))
                Row = index.NextRowWithValue(Row);

            return Row != Table.NoRow;
        }
    }

    internal class TableCallWithGeneralIndex<TKey, T1, T2, T3, T4> : TableCallWithGeneralIndex
    {
        private readonly TablePredicate<T1, T2, T3, T4> predicate;
        private readonly Pattern<T1, T2, T3, T4> pattern;
        private readonly GeneralIndex<(T1, T2, T3, T4), TKey> index;
        private readonly ValueCell<TKey> keyCell;

        public TableCallWithGeneralIndex(TablePredicate<T1, T2, T3, T4> tablePredicate, Pattern<T1, T2, T3, T4> pattern, GeneralIndex<(T1, T2, T3, T4), TKey> index, ValueCell<TKey> keyCell) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell = keyCell;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            Row = Primed ? index.FirstRowWithValue(in keyCell.Value) : index.NextRowWithValue(Row);
            Primed = false;

            while (Row != Table.NoRow && !pattern.Match(in predicate._table.PositionReference(Row)))
                Row = index.NextRowWithValue(Row);

            return Row != Table.NoRow;
        }
    }

    internal class TableCallWithGeneralIndex<TKey, T1, T2, T3, T4, T5> : TableCallWithGeneralIndex
    {
        private readonly TablePredicate<T1, T2, T3, T4, T5> predicate;
        private readonly Pattern<T1, T2, T3, T4, T5> pattern;
        private readonly GeneralIndex<(T1, T2, T3, T4, T5), TKey> index;
        private readonly ValueCell<TKey> keyCell;

        public TableCallWithGeneralIndex(TablePredicate<T1, T2, T3, T4, T5> tablePredicate, Pattern<T1, T2, T3, T4, T5> pattern, GeneralIndex<(T1, T2, T3, T4, T5), TKey> index, ValueCell<TKey> keyCell) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell = keyCell;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            Row = Primed ? index.FirstRowWithValue(in keyCell.Value) : index.NextRowWithValue(Row);
            Primed = false;

            while (Row != Table.NoRow && !pattern.Match(in predicate._table.PositionReference(Row)))
                Row = index.NextRowWithValue(Row);

            return Row != Table.NoRow;
        }
    }

    internal class TableCallWithGeneralIndex<TKey, T1, T2, T3, T4, T5, T6> : TableCallWithGeneralIndex
    {
        private readonly TablePredicate<T1, T2, T3, T4, T5, T6> predicate;
        private readonly Pattern<T1, T2, T3, T4, T5, T6> pattern;
        private readonly GeneralIndex<(T1, T2, T3, T4, T5, T6), TKey> index;
        private readonly ValueCell<TKey> keyCell;

        public TableCallWithGeneralIndex(TablePredicate<T1, T2, T3, T4, T5, T6> tablePredicate, Pattern<T1, T2, T3, T4, T5, T6> pattern, GeneralIndex<(T1, T2, T3, T4, T5, T6), TKey> index, ValueCell<TKey> keyCell) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell = keyCell;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            Row = Primed ? index.FirstRowWithValue(in keyCell.Value) : index.NextRowWithValue(Row);
            Primed = false;

            while (Row != Table.NoRow && !pattern.Match(in predicate._table.PositionReference(Row)))
                Row = index.NextRowWithValue(Row);

            return Row != Table.NoRow;
        }
    }

    internal class TableCallWithGeneralIndex<TKey, T1, T2, T3, T4, T5, T6, T7> : TableCallWithGeneralIndex
    {
        private readonly TablePredicate<T1, T2, T3, T4, T5, T6, T7> predicate;
        private readonly Pattern<T1, T2, T3, T4, T5, T6, T7> pattern;
        private readonly GeneralIndex<(T1, T2, T3, T4, T5, T6, T7), TKey> index;
        private readonly ValueCell<TKey> keyCell;

        public TableCallWithGeneralIndex(TablePredicate<T1, T2, T3, T4, T5, T6, T7> tablePredicate, Pattern<T1, T2, T3, T4, T5, T6, T7> pattern, GeneralIndex<(T1, T2, T3, T4, T5, T6, T7), TKey> index, ValueCell<TKey> keyCell) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell = keyCell;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            Row = Primed ? index.FirstRowWithValue(in keyCell.Value) : index.NextRowWithValue(Row);
            Primed = false;

            while (Row != Table.NoRow && !pattern.Match(in predicate._table.PositionReference(Row)))
                Row = index.NextRowWithValue(Row);

            return Row != Table.NoRow;
        }
    }

    internal class TableCallWithGeneralIndex<TKey, T1, T2, T3, T4, T5, T6, T7, T8> : TableCallWithGeneralIndex
    {
        private readonly TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> predicate;
        private readonly Pattern<T1, T2, T3, T4, T5, T6, T7, T8> pattern;
        private readonly GeneralIndex<(T1, T2, T3, T4, T5, T6, T7, T8), TKey> index;
        private readonly ValueCell<TKey> keyCell;

        public TableCallWithGeneralIndex(TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> tablePredicate, Pattern<T1, T2, T3, T4, T5, T6, T7, T8> pattern, GeneralIndex<(T1, T2, T3, T4, T5, T6, T7, T8), TKey> index, ValueCell<TKey> keyCell) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell = keyCell;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            Row = Primed ? index.FirstRowWithValue(in keyCell.Value) : index.NextRowWithValue(Row);
            Primed = false;

            while (Row != Table.NoRow && !pattern.Match(in predicate._table.PositionReference(Row)))
                Row = index.NextRowWithValue(Row);

            return Row != Table.NoRow;
        }
    }
}
