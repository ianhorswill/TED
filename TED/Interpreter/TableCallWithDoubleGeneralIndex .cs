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

    internal class TableCallWithDoubleGeneralIndex<TKey1,  TKey2, T1, T2, T3> : TableCallWithGeneralIndex
    {
        private readonly TablePredicate<T1, T2, T3> predicate;
        private readonly Pattern<T1, T2, T3> pattern;
        private readonly GeneralIndex<(T1, T2, T3), (TKey1, TKey2)> index;
        private readonly ValueCell<TKey1> keyCell1; private readonly ValueCell<TKey2> keyCell2;

        public TableCallWithDoubleGeneralIndex(TablePredicate<T1, T2, T3> tablePredicate, Pattern<T1, T2, T3> pattern, GeneralIndex<(T1, T2, T3), (TKey1, TKey2)> index, ValueCell<TKey1> keyCell1, ValueCell<TKey2> keyCell2) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell1 = keyCell1; this.keyCell2 = keyCell2;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            Row = Primed ? index.FirstRowWithValue((keyCell1.Value, keyCell2.Value)) : index.NextRowWithValue(Row);
            Primed = false;

            while (Row != Tables.Table.NoRow && !pattern.Match(in predicate._table.PositionReference(Row)))
                Row = index.NextRowWithValue(Row);

            return Row != Tables.Table.NoRow;
        }
    }

    internal class TableCallWithDoubleGeneralIndex<TKey1,  TKey2, T1, T2, T3, T4> : TableCallWithGeneralIndex
    {
        private readonly TablePredicate<T1, T2, T3, T4> predicate;
        private readonly Pattern<T1, T2, T3, T4> pattern;
        private readonly GeneralIndex<(T1, T2, T3, T4), (TKey1, TKey2)> index;
        private readonly ValueCell<TKey1> keyCell1; private readonly ValueCell<TKey2> keyCell2;

        public TableCallWithDoubleGeneralIndex(TablePredicate<T1, T2, T3, T4> tablePredicate, Pattern<T1, T2, T3, T4> pattern, GeneralIndex<(T1, T2, T3, T4), (TKey1, TKey2)> index, ValueCell<TKey1> keyCell1, ValueCell<TKey2> keyCell2) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell1 = keyCell1; this.keyCell2 = keyCell2;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            Row = Primed ? index.FirstRowWithValue((keyCell1.Value, keyCell2.Value)) : index.NextRowWithValue(Row);
            Primed = false;

            while (Row != Tables.Table.NoRow && !pattern.Match(in predicate._table.PositionReference(Row)))
                Row = index.NextRowWithValue(Row);

            return Row != Tables.Table.NoRow;
        }
    }

    internal class TableCallWithDoubleGeneralIndex<TKey1,  TKey2, T1, T2, T3, T4, T5> : TableCallWithGeneralIndex
    {
        private readonly TablePredicate<T1, T2, T3, T4, T5> predicate;
        private readonly Pattern<T1, T2, T3, T4, T5> pattern;
        private readonly GeneralIndex<(T1, T2, T3, T4, T5), (TKey1, TKey2)> index;
        private readonly ValueCell<TKey1> keyCell1; private readonly ValueCell<TKey2> keyCell2;

        public TableCallWithDoubleGeneralIndex(TablePredicate<T1, T2, T3, T4, T5> tablePredicate, Pattern<T1, T2, T3, T4, T5> pattern, GeneralIndex<(T1, T2, T3, T4, T5), (TKey1, TKey2)> index, ValueCell<TKey1> keyCell1, ValueCell<TKey2> keyCell2) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell1 = keyCell1; this.keyCell2 = keyCell2;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            Row = Primed ? index.FirstRowWithValue((keyCell1.Value, keyCell2.Value)) : index.NextRowWithValue(Row);
            Primed = false;

            while (Row != Tables.Table.NoRow && !pattern.Match(in predicate._table.PositionReference(Row)))
                Row = index.NextRowWithValue(Row);

            return Row != Tables.Table.NoRow;
        }
    }

    internal class TableCallWithDoubleGeneralIndex<TKey1,  TKey2, T1, T2, T3, T4, T5, T6> : TableCallWithGeneralIndex
    {
        private readonly TablePredicate<T1, T2, T3, T4, T5, T6> predicate;
        private readonly Pattern<T1, T2, T3, T4, T5, T6> pattern;
        private readonly GeneralIndex<(T1, T2, T3, T4, T5, T6), (TKey1, TKey2)> index;
        private readonly ValueCell<TKey1> keyCell1; private readonly ValueCell<TKey2> keyCell2;

        public TableCallWithDoubleGeneralIndex(TablePredicate<T1, T2, T3, T4, T5, T6> tablePredicate, Pattern<T1, T2, T3, T4, T5, T6> pattern, GeneralIndex<(T1, T2, T3, T4, T5, T6), (TKey1, TKey2)> index, ValueCell<TKey1> keyCell1, ValueCell<TKey2> keyCell2) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell1 = keyCell1; this.keyCell2 = keyCell2;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            Row = Primed ? index.FirstRowWithValue((keyCell1.Value, keyCell2.Value)) : index.NextRowWithValue(Row);
            Primed = false;

            while (Row != Tables.Table.NoRow && !pattern.Match(in predicate._table.PositionReference(Row)))
                Row = index.NextRowWithValue(Row);

            return Row != Tables.Table.NoRow;
        }
    }

    internal class TableCallWithDoubleGeneralIndex<TKey1,  TKey2, T1, T2, T3, T4, T5, T6, T7> : TableCallWithGeneralIndex
    {
        private readonly TablePredicate<T1, T2, T3, T4, T5, T6, T7> predicate;
        private readonly Pattern<T1, T2, T3, T4, T5, T6, T7> pattern;
        private readonly GeneralIndex<(T1, T2, T3, T4, T5, T6, T7), (TKey1, TKey2)> index;
        private readonly ValueCell<TKey1> keyCell1; private readonly ValueCell<TKey2> keyCell2;

        public TableCallWithDoubleGeneralIndex(TablePredicate<T1, T2, T3, T4, T5, T6, T7> tablePredicate, Pattern<T1, T2, T3, T4, T5, T6, T7> pattern, GeneralIndex<(T1, T2, T3, T4, T5, T6, T7), (TKey1, TKey2)> index, ValueCell<TKey1> keyCell1, ValueCell<TKey2> keyCell2) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell1 = keyCell1; this.keyCell2 = keyCell2;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            Row = Primed ? index.FirstRowWithValue((keyCell1.Value, keyCell2.Value)) : index.NextRowWithValue(Row);
            Primed = false;

            while (Row != Tables.Table.NoRow && !pattern.Match(in predicate._table.PositionReference(Row)))
                Row = index.NextRowWithValue(Row);

            return Row != Tables.Table.NoRow;
        }
    }

    internal class TableCallWithDoubleGeneralIndex<TKey1,  TKey2, T1, T2, T3, T4, T5, T6, T7, T8> : TableCallWithGeneralIndex
    {
        private readonly TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> predicate;
        private readonly Pattern<T1, T2, T3, T4, T5, T6, T7, T8> pattern;
        private readonly GeneralIndex<(T1, T2, T3, T4, T5, T6, T7, T8), (TKey1, TKey2)> index;
        private readonly ValueCell<TKey1> keyCell1; private readonly ValueCell<TKey2> keyCell2;

        public TableCallWithDoubleGeneralIndex(TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> tablePredicate, Pattern<T1, T2, T3, T4, T5, T6, T7, T8> pattern, GeneralIndex<(T1, T2, T3, T4, T5, T6, T7, T8), (TKey1, TKey2)> index, ValueCell<TKey1> keyCell1, ValueCell<TKey2> keyCell2) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell1 = keyCell1; this.keyCell2 = keyCell2;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            Row = Primed ? index.FirstRowWithValue((keyCell1.Value, keyCell2.Value)) : index.NextRowWithValue(Row);
            Primed = false;

            while (Row != Tables.Table.NoRow && !pattern.Match(in predicate._table.PositionReference(Row)))
                Row = index.NextRowWithValue(Row);

            return Row != Tables.Table.NoRow;
        }
    }
}
