using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TED
{
    /// <summary>
    /// A non-key index for a Table.
    /// If indexing by a column that should have unique values for each row (i.e. a key), then
    /// use a KeyIndex instead.
    /// </summary>
    /// <typeparam name="TRow">Type of the table rows.  This will be a tuple type unless it's a single-column table</typeparam>
    /// <typeparam name="TColumn">Type of the column we're indexing by</typeparam>
    internal sealed class GeneralIndex<TRow, TColumn> : TableIndex<TRow, TColumn>
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

        /// <summary>
        /// Buckets for the hash table.  These contain a column value and the index of the first row in the
        /// linked list of rows having that column value.  The row after it is stored in nextRow[firstRow].
        /// Empty buckets have firstRow == AnyTable.NoRow.
        /// </summary>
        private (TColumn columnValue, uint firstRow)[] buckets;

        /// <summary>
        /// Next cells in the linked lists.
        /// All the rows with a given column value are stored in a linked list, starting with the row indicated
        /// in the firstRow field of the hashtable bucket for the column value.  The successor to row i is stored in
        /// nextRow[i], its successor in nextRow[nextRow[i]], etc.  The end of the list is indicated by a nextRow
        /// value of AnyTable.NoRow.
        /// </summary>
        private uint[] nextRow;

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
        /// The TablePredicate to which the Table belongs.
        /// </summary>
        private readonly TablePredicate predicate;

        /// <summary>
        /// Equality predicate for TColumn.
        /// </summary>
        private static readonly EqualityComparer<TColumn> Comparer = EqualityComparer<TColumn>.Default;

        internal GeneralIndex(TablePredicate p, Table<TRow> t, int columnNumber, Projection projection) : base(columnNumber, projection)
        {
            predicate = p;
            table = t;
            var capacity = t.Data.Length * 2;
            buckets = new (TColumn columnValue, uint firstRow)[capacity];
            Array.Fill(buckets!, (default(TColumn), AnyTable.NoRow));
            nextRow = new uint[t.Data.Length];
            Array.Fill(nextRow, AnyTable.NoRow);
            mask = (uint)(capacity - 1);
            Debug.Assert((mask & capacity) == 0, "Capacity must be a power of 2");
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
            for (var b = HashInternal(value, mask); buckets[b].firstRow != AnyTable.NoRow; b = (b + 1) & mask)
                if (Comparer.Equals(buckets[b].columnValue, value))
                    return buckets[b].firstRow;
            return AnyTable.NoRow;
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
        internal sealed override void Add(uint row)
        {
            uint b;
            var value = projection(table.Data[row]);
            // Find the first bucket which either has this value, or which is empty
            for (b = HashInternal(value, mask); buckets[b].firstRow != AnyTable.NoRow && !Comparer.Equals(value, buckets[b].columnValue); b = (b + 1) & mask)
            { }

            // Insert row at the beginning of the list for this value;
            nextRow[row] = buckets[b].firstRow;
            buckets[b] = (value, row);
        }

        /// <summary>
        /// Called after the original Table is doubled in size.
        /// Double the size of this table, and reindex it.
        /// </summary>
        internal override void Expand()
        {
            buckets = new (TColumn columnValue, uint firstRow)[buckets.Length * 2];
            Array.Fill(buckets!, (default(TColumn), AnyTable.NoRow));
            mask = (uint)(buckets.Length - 1);
            nextRow = new uint[nextRow.Length * 2];
            Array.Fill(nextRow, AnyTable.NoRow);
            Reindex();
        }

        /// <summary>
        /// Erase all the data in the index
        /// </summary>
        internal override void Clear()
        {
            Array.Fill(buckets!, (default(TColumn), AnyTable.NoRow));
        }

        /// <summary>
        /// Add all the rows in the table to the index.
        /// Call Clear() first.
        /// </summary>
        internal sealed override void Reindex()
        {
            // Build the initial index
            for (var i = 0u; i < table.Length; i++)
                Add(i);
        }
    }
}
