using TED.Tables;

namespace TED.Interpreter
{
    internal class TableCallWithKey<TKey, T1, T2> : SingleRowTableCall
    {
        private readonly TablePredicate<T1, T2> predicate;
        private readonly Pattern<T1, T2> pattern;
        private readonly KeyIndex<(T1, T2), TKey> index;
        private readonly ValueCell<TKey> keyCell;

        public TableCallWithKey(TablePredicate<T1, T2> tablePredicate, Pattern<T1, T2> pattern, KeyIndex<(T1, T2), TKey> index, ValueCell<TKey> keyCell) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell = keyCell;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            if (!Primed) return false;
            Primed = false;
            var row = index.RowWithKey(in keyCell.Value);
            return row != Tables.Table.NoRow && pattern.Match(in predicate._table.PositionReference(row));
        }
    }

    internal class TableCallWithKey<TKey, T1, T2, T3> : SingleRowTableCall
    {
        private readonly TablePredicate<T1, T2, T3> predicate;
        private readonly Pattern<T1, T2, T3> pattern;
        private readonly KeyIndex<(T1, T2, T3), TKey> index;
        private readonly ValueCell<TKey> keyCell;

        public TableCallWithKey(TablePredicate<T1, T2, T3> tablePredicate, Pattern<T1, T2, T3> pattern, KeyIndex<(T1, T2, T3), TKey> index, ValueCell<TKey> keyCell) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell = keyCell;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            if (!Primed) return false;
            Primed = false;
            var row = index.RowWithKey(in keyCell.Value);
            return row != Tables.Table.NoRow && pattern.Match(in predicate._table.PositionReference(row));
        }
    }

    internal class TableCallWithKey<TKey, T1, T2, T3, T4> : SingleRowTableCall
    {
        private readonly TablePredicate<T1, T2, T3, T4> predicate;
        private readonly Pattern<T1, T2, T3, T4> pattern;
        private readonly KeyIndex<(T1, T2, T3, T4), TKey> index;
        private readonly ValueCell<TKey> keyCell;

        public TableCallWithKey(TablePredicate<T1, T2, T3, T4> tablePredicate, Pattern<T1, T2, T3, T4> pattern, KeyIndex<(T1, T2, T3, T4), TKey> index, ValueCell<TKey> keyCell) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell = keyCell;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            if (!Primed) return false;
            Primed = false;
            var row = index.RowWithKey(in keyCell.Value);
            return row != Tables.Table.NoRow && pattern.Match(in predicate._table.PositionReference(row));
        }
    }

    internal class TableCallWithKey<TKey, T1, T2, T3, T4, T5> : SingleRowTableCall
    {
        private readonly TablePredicate<T1, T2, T3, T4, T5> predicate;
        private readonly Pattern<T1, T2, T3, T4, T5> pattern;
        private readonly KeyIndex<(T1, T2, T3, T4, T5), TKey> index;
        private readonly ValueCell<TKey> keyCell;

        public TableCallWithKey(TablePredicate<T1, T2, T3, T4, T5> tablePredicate, Pattern<T1, T2, T3, T4, T5> pattern, KeyIndex<(T1, T2, T3, T4, T5), TKey> index, ValueCell<TKey> keyCell) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell = keyCell;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            if (!Primed) return false;
            Primed = false;
            var row = index.RowWithKey(in keyCell.Value);
            return row != Tables.Table.NoRow && pattern.Match(in predicate._table.PositionReference(row));
        }
    }

    internal class TableCallWithKey<TKey, T1, T2, T3, T4, T5, T6> : SingleRowTableCall
    {
        private readonly TablePredicate<T1, T2, T3, T4, T5, T6> predicate;
        private readonly Pattern<T1, T2, T3, T4, T5, T6> pattern;
        private readonly KeyIndex<(T1, T2, T3, T4, T5, T6), TKey> index;
        private readonly ValueCell<TKey> keyCell;

        public TableCallWithKey(TablePredicate<T1, T2, T3, T4, T5, T6> tablePredicate, Pattern<T1, T2, T3, T4, T5, T6> pattern, KeyIndex<(T1, T2, T3, T4, T5, T6), TKey> index, ValueCell<TKey> keyCell) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell = keyCell;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            if (!Primed) return false;
            Primed = false;
            var row = index.RowWithKey(in keyCell.Value);
            return row != Tables.Table.NoRow && pattern.Match(in predicate._table.PositionReference(row));
        }
    }

    internal class TableCallWithKey<TKey, T1, T2, T3, T4, T5, T6, T7> : SingleRowTableCall
    {
        private readonly TablePredicate<T1, T2, T3, T4, T5, T6, T7> predicate;
        private readonly Pattern<T1, T2, T3, T4, T5, T6, T7> pattern;
        private readonly KeyIndex<(T1, T2, T3, T4, T5, T6, T7), TKey> index;
        private readonly ValueCell<TKey> keyCell;

        public TableCallWithKey(TablePredicate<T1, T2, T3, T4, T5, T6, T7> tablePredicate, Pattern<T1, T2, T3, T4, T5, T6, T7> pattern, KeyIndex<(T1, T2, T3, T4, T5, T6, T7), TKey> index, ValueCell<TKey> keyCell) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell = keyCell;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            if (!Primed) return false;
            Primed = false;
            var row = index.RowWithKey(in keyCell.Value);
            return row != Tables.Table.NoRow && pattern.Match(in predicate._table.PositionReference(row));
        }
    }

    internal class TableCallWithKey<TKey, T1, T2, T3, T4, T5, T6, T7, T8> : SingleRowTableCall
    {
        private readonly TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> predicate;
        private readonly Pattern<T1, T2, T3, T4, T5, T6, T7, T8> pattern;
        private readonly KeyIndex<(T1, T2, T3, T4, T5, T6, T7, T8), TKey> index;
        private readonly ValueCell<TKey> keyCell;

        public TableCallWithKey(TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> tablePredicate, Pattern<T1, T2, T3, T4, T5, T6, T7, T8> pattern, KeyIndex<(T1, T2, T3, T4, T5, T6, T7, T8), TKey> index, ValueCell<TKey> keyCell) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell = keyCell;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            if (!Primed) return false;
            Primed = false;
            var row = index.RowWithKey(in keyCell.Value);
            return row != Tables.Table.NoRow && pattern.Match(in predicate._table.PositionReference(row));
        }
    }
}
