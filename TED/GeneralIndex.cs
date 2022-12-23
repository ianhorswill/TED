using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TED
{
    internal class GeneralIndex<TRow, TKey> : TableIndex
    {
        private readonly Func<TRow, TKey> projection;

        private (TKey key, uint firstRow)[] buckets;
        private uint[] nextRow;
        private uint mask;
        private readonly Table<TRow> table;
        private readonly TablePredicate predicate;
        private static readonly EqualityComparer<TKey> Comparer = EqualityComparer<TKey>.Default;

        public GeneralIndex(TablePredicate p, Table<TRow> t, int columnNumber, Func<TRow, TKey> projection) : base(columnNumber)
        {
            predicate = p;
            table = t;
            this.projection = projection;
            var capacity = t.Data.Length * 2;
            buckets = new (TKey key, uint firstRow)[capacity];
            Array.Fill(buckets!, (default(TKey), AnyTable.NoRow));
            nextRow = new uint[t.Data.Length];
            Array.Fill(nextRow, AnyTable.NoRow);
            mask = (uint)(capacity - 1);
            Debug.Assert((mask & capacity) == 0, "Capacity must be a power of 2");
            Reindex();
        }

        private static uint HashInternal(TKey value, uint mask) => (uint)Comparer.GetHashCode(value) & mask;

        public uint FirstRowWithValue(in TKey value)
        {
            for (var b = HashInternal(value, mask); buckets[b].firstRow != AnyTable.NoRow; b = (b + 1) & mask)
                if (Comparer.Equals(buckets[b].key, value))
                    return buckets[b].firstRow;
            return AnyTable.NoRow;
        }

        public uint NextRowWithValue(uint currentRow) => nextRow[currentRow];

        public override bool IsKey => false;

        public sealed override void Add(uint row)
        {
            uint b;
            var value = projection(table.Data[row]);
            // Find the first bucket which either has this value, or which is empty
            for (b = HashInternal(value, mask); buckets[b].firstRow != AnyTable.NoRow && !Comparer.Equals(value, buckets[b].key); b = (b + 1) & mask)
            { }

            // Insert row at the beginning of the list for this value;
            nextRow[row] = buckets[b].firstRow;
            buckets[b] = (value, row);
        }

        internal override void Expand()
        {
            buckets = new (TKey key, uint firstRow)[buckets.Length * 2];
            Array.Fill(buckets!, (default(TKey), AnyTable.NoRow));
            mask = (uint)(buckets.Length - 1);
            nextRow = new uint[nextRow.Length * 2];
            Array.Fill(nextRow, AnyTable.NoRow);
            Reindex();
        }

        internal override void Clear()
        {
            Array.Fill(buckets!, (default(TKey), AnyTable.NoRow));
        }

        internal sealed override void Reindex()
        {
            // Build the initial index
            for (var i = 0u; i < table.Length; i++)
                Add(i);
        }
    }
}
