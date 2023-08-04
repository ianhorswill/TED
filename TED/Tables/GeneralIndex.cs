using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using TED.Interpreter;

namespace TED.Tables
{
    /// <summary>
    /// A non-key index for a Table.
    /// If indexing by a column that should have unique values for each row (i.e. a key), then
    /// use a KeyIndex instead.
    /// </summary>
    /// <typeparam name="TRow">Type of the table rows.  This will be a tuple type unless it's a single-column table</typeparam>
    /// <typeparam name="TColumn">Type of the column we're indexing by</typeparam>
    public sealed class GeneralIndex<TRow, TColumn> : TableIndex<TRow, TColumn>, IGeneralIndex<TColumn>
    {
        //
        // Indices are implemented as direct-addressed hash tables in hopes of maximizing cache locality.
        // The tables use linear probing with a stride of 1, which is best-case for locality and worst-case
        // for cluster.  It also means we don't have to have hash tables with a prime number of buckets.
        // To reduce clustering, we size the table to keep the load factor below 0.5
        // TODO: Measure clustering and collision in a real application.
        //
        // For general indices, there may be many rows with a given column value so we use a linked list
        // of row indices.
        //
        // INVARIANTS:
        // - table.data.Length is a power of 2
        // - buckets.Length == table.data.length * 2
        // - Mask is buckets.Length-1, i.e. a bitmask for mapping integers into buckets
        // - nextRow.Length == table.data.length
        //

        public IEnumerable<TColumn> Keys => new KeyEnumeration(this);

        public struct KeyEnumeration : IEnumerable<TColumn>
        {
            private readonly GeneralIndex<TRow, TColumn> index;

            public KeyEnumeration(GeneralIndex<TRow, TColumn> index)
            {
                this.index = index;
            }

            public IEnumerator<TColumn> GetEnumerator()
            {
                return new KeyEnumerator(index);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        public struct KeyEnumerator : IEnumerator<TColumn>
        {
            private int bucket;
            private readonly (TColumn key, uint firstRow, int)[] buckets;

            public KeyEnumerator(GeneralIndex<TRow,TColumn> index)
            {
                this.buckets = index.buckets;
                bucket = -1;
            }

            public bool MoveNext()
            {
                while (++bucket < buckets.Length && !Table.ValidRow(buckets[bucket].firstRow)) { }
                return bucket < buckets.Length;
            }

            public void Reset()
            {
                bucket = -1;
            }

            public TColumn Current => buckets[bucket].key;

            object IEnumerator.Current => Current!;

            public void Dispose()
            { }
        }

        public IEnumerable<(TColumn, int)> CountsByKey => new KeyCountEnumeration(this);

        public struct KeyCountEnumeration : IEnumerable<(TColumn,int)>
        {
            private readonly GeneralIndex<TRow, TColumn> index;

            public KeyCountEnumeration(GeneralIndex<TRow, TColumn> index)
            {
                this.index = index;
            }

            public IEnumerator<(TColumn,int)> GetEnumerator()
            {
                return new KeyCountEnumerator(index);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        public struct KeyCountEnumerator : IEnumerator<(TColumn,int)>
        {
            private int bucket;
            private readonly (TColumn key, uint firstRow, int count)[] buckets;

            public KeyCountEnumerator(GeneralIndex<TRow,TColumn> index)
            {
                this.buckets = index.buckets;
                bucket = -1;
            }

            public bool MoveNext()
            {
                while (++bucket < buckets.Length && !Table.ValidRow(buckets[bucket].firstRow)) { }
                return bucket < buckets.Length;
            }

            public void Reset()
            {
                bucket = -1;
            }

            public (TColumn ,int) Current => (buckets[bucket].key, buckets[bucket].count);

            object IEnumerator.Current => Current;

            public void Dispose()
            { }
        }

        public IEnumerable<(TColumn key, uint firstRow, int count)> KeyInfo => new KeyInfoEnumeration(this);

        public struct KeyInfoEnumeration : IEnumerable<(TColumn,uint,int)>
        {
            private readonly GeneralIndex<TRow, TColumn> index;

            public KeyInfoEnumeration(GeneralIndex<TRow, TColumn> index)
            {
                this.index = index;
            }

            public IEnumerator<(TColumn, uint, int)> GetEnumerator()
            {
                return new KeyInfoEnumerator(index);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        public struct KeyInfoEnumerator : IEnumerator<(TColumn,uint, int)>
        {
            private int bucket;
            private readonly (TColumn key, uint firstRow, int count)[] buckets;

            public KeyInfoEnumerator(GeneralIndex<TRow,TColumn> index)
            {
                this.buckets = index.buckets;
                bucket = -1;
            }

            public bool MoveNext()
            {
                while (++bucket < buckets.Length && !Table.ValidRow(buckets[bucket].firstRow)) { }
                return bucket < buckets.Length;
            }

            public void Reset()
            {
                bucket = -1;
            }

            public (TColumn, uint, int) Current => buckets[bucket];

            object IEnumerator.Current => Current;

            public void Dispose()
            { }
        }

        /// <summary>
        /// Buckets for the hash table.  These contain a column value and the index of the first row in the
        /// linked list of rows having that column value.  The row after it is stored in nextRow[firstRow].
        /// Empty buckets have firstRow == AnyTable.NoRow.
        /// </summary>
        private (TColumn columnValue, uint firstRow, int count)[] buckets;

        private readonly bool enumeratedTypeColumn;

        /// <summary>
        /// Next cells in the linked lists.
        /// All the rows with a given column value are stored in a linked list, starting with the row indicated
        /// in the firstRow field of the hashtable bucket for the column value.  The successor to row i is stored in
        /// nextRow[i], its successor in nextRow[nextRow[i]], etc.  The end of the list is indicated by a nextRow
        /// value of AnyTable.NoRow.
        /// </summary>
        private uint[] nextRow;

        /// <summary>
        /// Previous row in a doubly-linked list of rows in a given bucket
        /// This is used if Remove() or changing values of the column is to be supported
        /// </summary>
        private uint[]? previousRow;

        /// <summary>
        /// The length of the buckets array is always a power of 2.  Mask is the length-1, so we can easily
        /// project a hash value into a bucket number by and'ing the hash value with the mask.
        /// </summary>
        private uint mask;

        /// <summary>
        /// The Table object this is indexing.
        /// Row numbers in the index are numbers of the rows in this table.
        /// </summary>
        private readonly Table<TRow> table;

        /// <summary>
        /// Equality Predicate for TColumn.
        /// </summary>
        private static readonly EqualityComparer<TColumn> Comparer = Comparer<TColumn>.Default;

        internal GeneralIndex(TablePredicate p, Table<TRow> t, int[] columnNumbers, Table.Projection<TRow,TColumn> projection) 
            : base(p, columnNumbers, projection)
        {
            table = t;
            var columnType = typeof(TColumn);
            enumeratedTypeColumn = columnType.IsEnum;
            var capacity = t.Data.Length * 2;
            buckets = new (TColumn columnValue, uint firstRow, int count)[enumeratedTypeColumn?Enum.GetValues(columnType).Cast<int>().Max()+1:capacity];
            Array.Fill(buckets!, (default(TColumn), Table.NoRow, 0));
            nextRow = new uint[t.Data.Length];
            Array.Fill(nextRow, Table.NoRow);
            if (enumeratedTypeColumn)
                mask = uint.MaxValue;
            else
            {
                mask = (uint)(capacity - 1);
                Debug.Assert((mask & capacity) == 0, "Capacity must be a power of 2");
            }

            Reindex();
        }

        public void EnableMutation()
        {
            if (previousRow != null)
                return;
            previousRow = new uint[nextRow.Length];
            Clear();
            Reindex();
        }

        /// <summary>
        /// Map a column value to an initial hash bucket.
        /// </summary>
        /// <param name="value">Column value</param>
        /// <param name="mask">Mask to AND with to generate a bucket number</param>
        /// <returns></returns>
        private static uint HashInternal(TColumn value, uint mask) => (uint)Comparer.GetHashCode(value) & mask;

        /// <summary>
        /// Search the table for the start of the linked list of rows with the specified column value
        /// If no linked list is found, return AnyTable.NoRow.
        /// </summary>
        public uint FirstRowWithValue(in TColumn value)
        {
            for (var b = HashInternal(value, mask); buckets[b].firstRow != Table.NoRow; b = b + 1 & mask)
                if (Comparer.Equals(buckets[b].columnValue, value))
                {
                    var first = buckets[b].firstRow;
                    if (first == Table.DeletedRow)
                        return Table.NoRow;
                    return first;
                }
            return Table.NoRow;
        }

        private uint? BucketWithValue(in TColumn value)
        {
            for (var b = HashInternal(value, mask); buckets[b].firstRow != Table.NoRow; b = b + 1 & mask)
                if (Comparer.Equals(buckets[b].columnValue, value))
                    return b;
            return null;
        }

        /// <summary>
        /// Return the next row after the specified row in the link list of rows with a given column value.
        /// </summary>
        public uint NextRowWithValue(uint currentRow) => nextRow[currentRow];

        /// <summary>
        /// This is not a key index
        /// </summary>
        public override bool IsKey => false;

        /// <summary>
        /// Add the row at the specified position in the table to the index.
        /// This will read the row from the table to get its column value for the purpose of indexing.
        /// </summary>
        /// <param name="row">Row number of the row we're adding</param>
        internal override void Add(uint row)
        {
            uint b;
            var value = projection(table.Data[row]);
            // Find the first bucket which either has this value, or which is empty
            for (b = HashInternal(value, mask); buckets[b].firstRow != Table.NoRow && !Comparer.Equals(value, buckets[b].columnValue); b = b + 1 & mask)
            { }

            var oldFirstRow = buckets[b].firstRow;
            if (oldFirstRow == Table.DeletedRow) 
                oldFirstRow = Table.NoRow;
            if (oldFirstRow == Table.NoRow)
            {
                buckets[b].columnValue = value;
                buckets[b].count = 0;
            }

            // Insert row at the beginning of the list for this value;
            buckets[b].firstRow = row;
            buckets[b].count++;
            nextRow[row] = oldFirstRow;


            // Update back-pointers, if this table supports removal
            if (previousRow != null)
            {
                previousRow[row] = Table.NoRow;
                if (oldFirstRow != Table.NoRow)
                    previousRow[oldFirstRow] = row;
            }
        }

        internal void Remove(uint row)
        {
            uint b;
            var value = projection(table.Data[row]);
            // Find the bucket that has this value
            for (b = HashInternal(value, mask); !Comparer.Equals(value, buckets[b].columnValue); b = b + 1 & mask)
            { }

            buckets[b].count--;

            var previous = previousRow![row];
            var next = nextRow[row];
            if (previous != Table.NoRow)
            {
                // It's not the head of the list, so just splice it out of the list.
                nextRow[previous] = next;
                if (next != Table.NoRow)
                    previousRow[next] = previous;
                return;
            }

            // It must be the first element of the list
            if (next == Table.NoRow)
            {
                // It was the only row in the list
                buckets[b].firstRow = Table.DeletedRow;
            }
            else
            {
                buckets[b].firstRow = next;
                previousRow[next] = Table.NoRow;
            }
        }

        /// <summary>
        /// Called after the original Table is doubled in size.
        /// Double the size of this table, and reindex it.
        /// </summary>
        internal override void Expand()
        {
            if (!enumeratedTypeColumn)
            {
                buckets = new (TColumn columnValue, uint firstRow, int count)[buckets.Length * 2];
                mask = (uint)(buckets.Length - 1);
            }
            Array.Fill(buckets!, (default(TColumn), Table.NoRow,0));
            nextRow = new uint[nextRow.Length * 2];
            Array.Fill(nextRow, Table.NoRow);
            if (previousRow != null)
                previousRow = new uint[nextRow.Length];
            Reindex();
        }

        /// <summary>
        /// Erase all the data in the index
        /// </summary>
        internal override void Clear()
        {
            Array.Fill(buckets!, (default(TColumn), Table.NoRow, 0));
        }

        /// <summary>
        /// Add all the rows in the table to the index.
        /// Call Clear() first.
        /// </summary>
        internal override void Reindex()
        {
            // Build the initial index
            for (var i = 0u; i < table.Length; i++)
                Add(i);
        }

        /// <summary>
        /// Tuples in the table matching the specified column value
        /// </summary>
        public IEnumerable<TRow> RowsMatching(TColumn value)
        {
            for (var rowNumber = FirstRowWithValue(value);
                 Table.ValidRow(rowNumber);
                 rowNumber = NextRowWithValue(rowNumber))
                yield return table[rowNumber];
        }

        /// <summary>
        /// The number of columns in the table with the specified value.
        /// </summary>
        public int RowsMatchingCount(TColumn value)
        {
            var b = BucketWithValue(value);
            return b == null ? 0 : buckets[b.Value].count;
        }
    }

    public interface IGeneralIndex<TColumn>
    {
        public IEnumerable<TColumn> Keys { get; }
        public IEnumerable<(TColumn, int)> CountsByKey { get; }
    }
}
