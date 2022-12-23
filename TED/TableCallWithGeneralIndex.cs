namespace TED
{
    internal abstract class TableCallWithGeneralIndex : AnyCall
    {
        public TableCallWithGeneralIndex(TablePredicate p) : base(p)
        { }

        protected bool Primed;
        protected uint Row;

        public override void Reset() => Primed = true;
    }

    internal class TableCallWithGeneralIndex<TKey,T1, T2> : TableCallWithGeneralIndex
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
            Row = Primed ? index.FirstRowWithValue(keyCell.Value) : index.NextRowWithValue(Row);
            Primed = false;

            while (Row != AnyTable.NoRow && !pattern.Match(predicate._table.PositionReference(Row)))
                Row = index.NextRowWithValue(Row);

            return Row != AnyTable.NoRow;
        }
    }
    
    internal class TableCallWithGeneralIndex<TKey, T1,T2,T3> : TableCallWithGeneralIndex
    {
        private readonly TablePredicate<T1,T2,T3> predicate;
        private readonly Pattern<T1,T2,T3> pattern;
        private readonly GeneralIndex<(T1, T2, T3), TKey> index;
        private readonly ValueCell<TKey> keyCell;

        public TableCallWithGeneralIndex(TablePredicate<T1,T2,T3> tablePredicate, Pattern<T1,T2,T3> pattern, GeneralIndex<(T1, T2, T3), TKey> index, ValueCell<TKey> keyCell) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell = keyCell;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            Row = Primed ? index.FirstRowWithValue(keyCell.Value) : index.NextRowWithValue(Row);
            Primed = false;

            while (Row != AnyTable.NoRow && !pattern.Match(predicate._table.PositionReference(Row)))
                Row = index.NextRowWithValue(Row);

            return Row != AnyTable.NoRow;
        }
    }

    internal class TableCallWithGeneralIndex<TKey, T1,T2,T3,T4> : TableCallWithGeneralIndex
    {
        private readonly TablePredicate<T1,T2,T3,T4> predicate;
        private readonly Pattern<T1,T2,T3,T4> pattern;
        private readonly GeneralIndex<(T1, T2, T3, T4), TKey> index;
        private readonly ValueCell<TKey> keyCell;

        public TableCallWithGeneralIndex(TablePredicate<T1,T2,T3,T4> tablePredicate, Pattern<T1,T2,T3,T4> pattern, GeneralIndex<(T1, T2, T3, T4), TKey> index, ValueCell<TKey> keyCell) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell = keyCell;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            Row = Primed ? index.FirstRowWithValue(keyCell.Value) : index.NextRowWithValue(Row);
            Primed = false;

            while (Row != AnyTable.NoRow && !pattern.Match(predicate._table.PositionReference(Row)))
                Row = index.NextRowWithValue(Row);

            return Row != AnyTable.NoRow;
        }
    }

    internal class TableCallWithGeneralIndex<TKey, T1,T2,T3,T4,T5> : TableCallWithGeneralIndex
    {
        private readonly TablePredicate<T1,T2,T3,T4,T5> predicate;
        private readonly Pattern<T1,T2,T3,T4,T5> pattern;
        private readonly GeneralIndex<(T1, T2, T3, T4, T5), TKey> index;
        private readonly ValueCell<TKey> keyCell;

        public TableCallWithGeneralIndex(TablePredicate<T1,T2,T3,T4,T5> tablePredicate, Pattern<T1,T2,T3,T4,T5> pattern, GeneralIndex<(T1, T2, T3, T4, T5), TKey> index, ValueCell<TKey> keyCell) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell = keyCell;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            Row = Primed ? index.FirstRowWithValue(keyCell.Value) : index.NextRowWithValue(Row);
            Primed = false;

            while (Row != AnyTable.NoRow && !pattern.Match(predicate._table.PositionReference(Row)))
                Row = index.NextRowWithValue(Row);

            return Row != AnyTable.NoRow;
        }
    }

    internal class TableCallWithGeneralIndex<TKey, T1,T2,T3,T4,T5,T6> : TableCallWithGeneralIndex
    {
        private readonly TablePredicate<T1,T2,T3,T4,T5,T6> predicate;
        private readonly Pattern<T1,T2,T3,T4,T5,T6> pattern;
        private readonly GeneralIndex<(T1, T2, T3, T4, T5, T6), TKey> index;
        private readonly ValueCell<TKey> keyCell;

        public TableCallWithGeneralIndex(TablePredicate<T1,T2,T3,T4,T5,T6> tablePredicate, Pattern<T1,T2,T3,T4,T5,T6> pattern, GeneralIndex<(T1, T2, T3, T4, T5, T6), TKey> index, ValueCell<TKey> keyCell) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell = keyCell;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            Row = Primed ? index.FirstRowWithValue(keyCell.Value) : index.NextRowWithValue(Row);
            Primed = false;

            while (Row != AnyTable.NoRow && !pattern.Match(predicate._table.PositionReference(Row)))
                Row = index.NextRowWithValue(Row);

            return Row != AnyTable.NoRow;
        }
    }
}
