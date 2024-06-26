﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TED.Tables
{
    /// <summary>
    /// An index to a table that indexes by a column that is a key (has unique values for each row in the table)
    /// </summary>
    /// <typeparam name="TRow">Type of the rows of the table; this will be a tuple type unless it is a single-column table</typeparam>
    /// <typeparam name="TKey">Type of the column we're indexing on</typeparam>
    public sealed class KeyIndex<TRow, TKey> : TableIndex<TRow, TKey>
    {
        //
        // Indices are implemented as direct-addressed hash tables in hopes of maximizing cache locality.
        // The tables use linear probing with a stride of 1, which is best-case for locality and worst-case
        // for cluster.  It also means we don't have to have hash tables with a prime number of buckets.
        // To reduce clustering, we size the table to keep the load factor below 0.5
        // TODO: Measure clustering and collision in a real application.
        //
        // For key indices, there is at most one row for any given key value.  If this is not true, use a general index.
        //
        // INVARIANTS:
        // - table.data.Length is a power of 2
        // - buckets.Length == table.data.length * 2
        // - Mask is buckets.Length-1, i.e. a bitmask for mapping integers into buckets
        //

        /// <summary>
        /// INTERNAL: Do not use; exposed only for the benefit of compiled code
        /// Hash table buckets mapping key values to row numbers
        /// </summary>
        public (TKey key, uint row)[] Buckets;

        /// <summary>
        /// INTERNAL: Do not use; exposed only for the benefit of compiled code
        /// Mask to and with a hash to get a bucket number
        /// </summary>
        public uint Mask;

        /// <summary>
        /// Underlying table we're indexing
        /// </summary>
        private readonly Table<TRow> table;

        /// <summary>
        /// Equality predicate for TKey
        /// </summary>
        private static readonly EqualityComparer<TKey> Comparer = Comparer<TKey>.Default;

        internal KeyIndex(TablePredicate p, Table<TRow> t, int[] columnNumbers, Table.Projection<TRow,TKey> projection) : base(p, columnNumbers, projection)
        {
            table = t;
            var capacity = t.Data.Length * 2;
            Buckets = new (TKey key, uint row)[capacity];
            Array.Fill(Buckets!, (default(TKey), Table.NoRow));
            Mask = (uint)(capacity - 1);
            Debug.Assert((Mask & capacity) == 0, "Capacity must be a power of 2");
            Reindex();
        }

        /// <summary>
        /// Hash functions mapping a key value and a mask to a bucket number
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint HashInternal(TKey value, uint mask) => unchecked((uint)Comparer.GetHashCode(value)) & mask;

        /// <summary>
        /// Return a reference to the row with the specified key.
        /// This will blow up if there is no such row.
        /// </summary>
        public ref TRow this[in TKey key]
        {
            get
            {
                var rowWithKey = RowWithKey(in key);
                if (rowWithKey == Table.NoRow)
                    throw new KeyNotFoundException($"No row in table {table.Name} has key {key}");
                return ref table.PositionReference(rowWithKey);
            }
        }

        /// <summary>
        /// Test if the table contains a row with the specified key
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public bool ContainsKey(in TKey key) => RowWithKey(in key) != Table.NoRow;

        /// <summary>
        /// Row containing this key, if any
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint RowWithKey(in TKey value)
        {
            for (var b = HashInternal(value, Mask); Buckets[b].row != Table.NoRow; b = b + 1 & Mask)
                if (Comparer.Equals(Buckets[b].key, value))
                    return Buckets[b].row;
            return Table.NoRow;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override uint RowWithKey(in TRow rowData) => RowWithKey(projection(in rowData));

        /// <summary>
        /// Yes, this is a key index
        /// </summary>
        public override bool IsKey => true;

#if PROFILER
        private uint insertions;
        private uint probes;

        /// <summary>
        /// Average number of probes of the table until an insertion finds a free slot
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public float ProbesPerInsertion => (float)probes / insertions;
#endif

        /// <summary>
        /// Add the row with the specified row number to the table
        /// </summary>
        /// <param name="row">Number of the row</param>
        /// <exception cref="DuplicateKeyException">If there is always a row in the table containing that key value</exception>
        internal override void Add(uint row)
        {
            uint b;
            var key = projection(table.Data[row]);
            for (b = HashInternal(key, Mask); Buckets[b].row != Table.NoRow; b = b + 1 & Mask)
            {
#if PROFILER
                probes++;
#endif
                if (Comparer.Equals(key, Buckets[b].key))
                    // It's already there
                    throw new DuplicateKeyException(Predicate, key!);
            }

            // It's not there, but b is a free bucket, so store it there
            Buckets[b] = (key, row);
#if PROFILER
            insertions++;
#endif
        }

        /// <summary>
        /// Double the size of the index's hash table and reindex.
        /// Called after the underlying table has doubled in side.
        /// </summary>
        internal override void Expand()
        {
            Buckets = new (TKey key, uint row)[Buckets.Length * 2];
            Array.Fill(Buckets!, (default(TKey), Table.NoRow));
            Mask = (uint)(Buckets.Length - 1);
            Reindex();
        }

        /// <summary>
        /// Erase the index
        /// </summary>
        internal override void Clear()
        {
            Array.Fill(Buckets!, (default(TKey), Table.NoRow));
        }

        /// <summary>
        /// Reindex the table.  Call Clear() first.
        /// </summary>
        internal override void Reindex()
        {
            // Build the initial index
            for (var i = 0u; i < table.Length; i++)
                Add(i);
        }
    }
}
