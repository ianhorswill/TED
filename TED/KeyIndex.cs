using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TED
{
    internal class KeyIndex<TRow, TKey> : TableIndex
    {
        private readonly Func<TRow, TKey> projection;

        private (TKey key, uint row)[] buckets;
        private uint mask;
        private readonly Table<TRow> table;
        private readonly TablePredicate predicate;
        private static readonly EqualityComparer<TKey> Comparer = EqualityComparer<TKey>.Default;

        public KeyIndex(TablePredicate p, Table<TRow> t, int columnIndex, Func<TRow, TKey> projection) : base(columnIndex)
        {
            predicate = p;
            table = t;
            this.projection = projection;
            var capacity = t.Data.Length * 2;
            buckets = new (TKey key, uint row)[capacity];
            Array.Fill(buckets!, (default(TKey), Empty: NotFound));
            mask = (uint)(capacity - 1);
            Debug.Assert((mask & capacity) == 0, "Capacity must be a power of 2");

            // Build the initial index
            for (var i = 0u; i < t.Length; i++)
                Add(i);
        }

        private static uint HashInternal(TKey value, uint mask) => (uint)Comparer.GetHashCode(value) & mask;

        public uint RowWithKey(in TKey value)
        {
            for (var b = HashInternal(value, mask); buckets[b].row != NotFound; b = (b + 1) & mask)
                if (Comparer.Equals(buckets[b].key, value))
                    return buckets[b].row;
            return NotFound;
        }

        public override bool IsKey => true;

        public override void Add(uint row)
        {
            uint b;
            var key = projection(table.Data[row]);
            for (b = HashInternal(key, mask); buckets[b].row != NotFound; b = ((b + 1) & mask))
                if (Comparer.Equals(key, buckets[b].key))
                    // It's already there
                    throw new DuplicateKeyException(predicate, key!);

            // It's not there, but b is a free bucket, so store it there
            buckets[b] = (key, row);
        }

        public override void Expand()
        {
            var newBuckets = new (TKey key, uint row)[buckets.Length * 2];
            var newMask = (uint)(newBuckets.Length - 1);
            Array.Fill(newBuckets!, (default(TKey), Empty: NotFound));
            for (var b = 0u; b < buckets.Length; b++)
            {
                var buck = buckets[b];
                if (buck.row != NotFound)
                {
                    uint nb;
                    // Find a free bucket
                    for (nb = HashInternal(buck.key, newMask); newBuckets[nb].row != NotFound; nb = (nb + 1) & newMask)
                    {
                    } // Do nothing

                    newBuckets[nb] = buck;
                }
            }

            buckets = newBuckets;
            mask = newMask;
        }

        public override void Clear()
        {
            Array.Fill(buckets!, (default(TKey), Empty: NotFound));
        }
    }
}
