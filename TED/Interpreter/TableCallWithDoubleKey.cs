using TED.Compiler;
using TED.Tables;

namespace TED.Interpreter
{
    internal abstract class TableCallWithDoubleKey : SingleRowTableCall
    {
        protected TableCallWithDoubleKey(TablePredicate p) : base(p) { }

        public abstract TableIndex Index { get; }
        public abstract ValueCell Cell1 { get; }
        public abstract ValueCell Cell2 { get; }

        /// <inheritdoc />
        public override Continuation Compile(Compiler.Compiler compiler, Continuation fail, string identifierSuffix)
        {
            var rowNumber = $"row{identifierSuffix}";
            var rowData = $"data{identifierSuffix}";
            var key = $"key{identifierSuffix}";
            var index = Compiler.Compiler.VariableNameForIndex(Index);

            //compiler.Indented($"var {rowNumber} = {Compiler.Compiler.VariableNameForIndex(Index)}.RowWithKey(({Cell1.ReadExpression}, {Cell2.ReadExpression}));");

            compiler.Indented($"var {key} = ({Cell1.ReadExpression}, {Cell2.ReadExpression});");
            TableIndex.CompileIndexLookup(compiler, fail, identifierSuffix, rowNumber, key, index, "row");
            compiler.Indented($"ref var {rowData} = ref {Table.Name}.Data[{rowNumber}];");
            compiler.CompilePatternMatch(rowData, ArgumentPattern, fail, Index.ColumnNumbers);

            //var row__2_maxLoop_1_0 = Table.NoRow;
            //var key = (person, other);
            //for (var b = key.GetHashCode()&Affinity__0_1_key.mask; Affinity__0_1_key.buckets[b].row != Table.NoRow; b = b + 1 & Affinity__0_1_key.mask)
            //    if (Affinity__0_1_key.buckets[b].key == key)
            //    {
            //        row__2_maxLoop_1_0 = Affinity__0_1_key.buckets[b].row;
            //    }


            return fail;
        }
    }
    internal class TableCallWithDoubleKey<TKey1, TKey2, T1, T2> : TableCallWithDoubleKey
    {
        private readonly TablePredicate<T1, T2> predicate;
        private readonly Pattern<T1, T2> pattern;
        private readonly KeyIndex<(T1, T2), (TKey1,TKey2)> index;
        private readonly ValueCell<TKey1> keyCell1; private readonly ValueCell<TKey2> keyCell2;

        public override TableIndex Index => index;
        public override ValueCell Cell1 => keyCell1;
        public override ValueCell Cell2 => keyCell2;

        public TableCallWithDoubleKey(TablePredicate<T1, T2> tablePredicate, Pattern<T1, T2> pattern, KeyIndex<(T1, T2), (TKey1, TKey2)> index, ValueCell<TKey1> keyCell1, ValueCell<TKey2> keyCell2) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell1 = keyCell1; this.keyCell2 = keyCell2;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            if (!Primed) return false;
            Primed = false;
            var row = index.RowWithKey((keyCell1.Value, keyCell2.Value));
            return row != Tables.Table.NoRow && pattern.Match(in predicate._table.PositionReference(row));
        }
    }

    internal class TableCallWithDoubleKey<TKey1, TKey2, T1, T2, T3> : TableCallWithDoubleKey
    {
        private readonly TablePredicate<T1, T2, T3> predicate;
        private readonly Pattern<T1, T2, T3> pattern;
        private readonly KeyIndex<(T1, T2, T3), (TKey1, TKey2)> index;
        private readonly ValueCell<TKey1> keyCell1; private readonly ValueCell<TKey2> keyCell2;

        public override TableIndex Index => index;
        public override ValueCell Cell1 => keyCell1;
        public override ValueCell Cell2 => keyCell2;

        public TableCallWithDoubleKey(TablePredicate<T1, T2, T3> tablePredicate, Pattern<T1, T2, T3> pattern, KeyIndex<(T1, T2, T3), (TKey1, TKey2)> index, ValueCell<TKey1> keyCell1, ValueCell<TKey2> keyCell2) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell1 = keyCell1; this.keyCell2 = keyCell2;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            if (!Primed) return false;
            Primed = false;
            var row = index.RowWithKey((keyCell1.Value, keyCell2.Value));
            return row != Tables.Table.NoRow && pattern.Match(in predicate._table.PositionReference(row));
        }
    }

    internal class TableCallWithDoubleKey<TKey1, TKey2, T1, T2, T3, T4> : TableCallWithDoubleKey
    {
        private readonly TablePredicate<T1, T2, T3, T4> predicate;
        private readonly Pattern<T1, T2, T3, T4> pattern;
        private readonly KeyIndex<(T1, T2, T3, T4), (TKey1, TKey2)> index;
        private readonly ValueCell<TKey1> keyCell1; private readonly ValueCell<TKey2> keyCell2;

        public override TableIndex Index => index;
        public override ValueCell Cell1 => keyCell1;
        public override ValueCell Cell2 => keyCell2;

        public TableCallWithDoubleKey(TablePredicate<T1, T2, T3, T4> tablePredicate, Pattern<T1, T2, T3, T4> pattern, KeyIndex<(T1, T2, T3, T4), (TKey1, TKey2)> index, ValueCell<TKey1> keyCell1, ValueCell<TKey2> keyCell2) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell1 = keyCell1; this.keyCell2 = keyCell2;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            if (!Primed) return false;
            Primed = false;
            var row = index.RowWithKey((keyCell1.Value, keyCell2.Value));
            return row != Tables.Table.NoRow && pattern.Match(in predicate._table.PositionReference(row));
        }
    }

    internal class TableCallWithDoubleKey<TKey1, TKey2, T1, T2, T3, T4, T5> : TableCallWithDoubleKey
    {
        private readonly TablePredicate<T1, T2, T3, T4, T5> predicate;
        private readonly Pattern<T1, T2, T3, T4, T5> pattern;
        private readonly KeyIndex<(T1, T2, T3, T4, T5), (TKey1, TKey2)> index;
        private readonly ValueCell<TKey1> keyCell1; private readonly ValueCell<TKey2> keyCell2;

        public override TableIndex Index => index;
        public override ValueCell Cell1 => keyCell1;
        public override ValueCell Cell2 => keyCell2;

        public TableCallWithDoubleKey(TablePredicate<T1, T2, T3, T4, T5> tablePredicate, Pattern<T1, T2, T3, T4, T5> pattern, KeyIndex<(T1, T2, T3, T4, T5), (TKey1, TKey2)> index, ValueCell<TKey1> keyCell1, ValueCell<TKey2> keyCell2) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell1 = keyCell1; this.keyCell2 = keyCell2;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            if (!Primed) return false;
            Primed = false;
            var row = index.RowWithKey((keyCell1.Value, keyCell2.Value));
            return row != Tables.Table.NoRow && pattern.Match(in predicate._table.PositionReference(row));
        }
    }

    internal class TableCallWithDoubleKey<TKey1, TKey2, T1, T2, T3, T4, T5, T6> : TableCallWithDoubleKey
    {
        private readonly TablePredicate<T1, T2, T3, T4, T5, T6> predicate;
        private readonly Pattern<T1, T2, T3, T4, T5, T6> pattern;
        private readonly KeyIndex<(T1, T2, T3, T4, T5, T6), (TKey1, TKey2)> index;
        private readonly ValueCell<TKey1> keyCell1; private readonly ValueCell<TKey2> keyCell2;

        public override TableIndex Index => index;
        public override ValueCell Cell1 => keyCell1;
        public override ValueCell Cell2 => keyCell2;

        public TableCallWithDoubleKey(TablePredicate<T1, T2, T3, T4, T5, T6> tablePredicate, Pattern<T1, T2, T3, T4, T5, T6> pattern, KeyIndex<(T1, T2, T3, T4, T5, T6), (TKey1, TKey2)> index, ValueCell<TKey1> keyCell1, ValueCell<TKey2> keyCell2) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell1 = keyCell1; this.keyCell2 = keyCell2;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            if (!Primed) return false;
            Primed = false;
            var row = index.RowWithKey((keyCell1.Value, keyCell2.Value));
            return row != Tables.Table.NoRow && pattern.Match(in predicate._table.PositionReference(row));
        }
    }

    internal class TableCallWithDoubleKey<TKey1, TKey2, T1, T2, T3, T4, T5, T6, T7> : TableCallWithDoubleKey
    {
        private readonly TablePredicate<T1, T2, T3, T4, T5, T6, T7> predicate;
        private readonly Pattern<T1, T2, T3, T4, T5, T6, T7> pattern;
        private readonly KeyIndex<(T1, T2, T3, T4, T5, T6, T7), (TKey1, TKey2)> index;
        private readonly ValueCell<TKey1> keyCell1; private readonly ValueCell<TKey2> keyCell2;

        public override TableIndex Index => index;
        public override ValueCell Cell1 => keyCell1;
        public override ValueCell Cell2 => keyCell2;

        public TableCallWithDoubleKey(TablePredicate<T1, T2, T3, T4, T5, T6, T7> tablePredicate, Pattern<T1, T2, T3, T4, T5, T6, T7> pattern, KeyIndex<(T1, T2, T3, T4, T5, T6, T7), (TKey1, TKey2)> index, ValueCell<TKey1> keyCell1, ValueCell<TKey2> keyCell2) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell1 = keyCell1; this.keyCell2 = keyCell2;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            if (!Primed) return false;
            Primed = false;
            var row = index.RowWithKey((keyCell1.Value, keyCell2.Value));
            return row != Tables.Table.NoRow && pattern.Match(in predicate._table.PositionReference(row));
        }
    }

    internal class TableCallWithDoubleKey<TKey1, TKey2, T1, T2, T3, T4, T5, T6, T7, T8> : TableCallWithDoubleKey
    {
        private readonly TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> predicate;
        private readonly Pattern<T1, T2, T3, T4, T5, T6, T7, T8> pattern;
        private readonly KeyIndex<(T1, T2, T3, T4, T5, T6, T7, T8), (TKey1, TKey2)> index;
        private readonly ValueCell<TKey1> keyCell1; private readonly ValueCell<TKey2> keyCell2;

        public override TableIndex Index => index;
        public override ValueCell Cell1 => keyCell1;
        public override ValueCell Cell2 => keyCell2;

        public TableCallWithDoubleKey(TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> tablePredicate, Pattern<T1, T2, T3, T4, T5, T6, T7, T8> pattern, KeyIndex<(T1, T2, T3, T4, T5, T6, T7, T8), (TKey1, TKey2)> index, ValueCell<TKey1> keyCell1, ValueCell<TKey2> keyCell2) : base(tablePredicate)
        {
            predicate = tablePredicate;
            this.pattern = pattern;
            this.index = index;
            this.keyCell1 = keyCell1; this.keyCell2 = keyCell2;
        }

        public override IPattern ArgumentPattern => pattern;

        public override bool NextSolution()
        {
            if (!Primed) return false;
            Primed = false;
            var row = index.RowWithKey((keyCell1.Value, keyCell2.Value));
            return row != Tables.Table.NoRow && pattern.Match(in predicate._table.PositionReference(row));
        }
    }
}
