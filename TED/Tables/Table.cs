using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TED.Tables
{
    /// <summary>
    /// Untyped base class for all tables
    /// Tables are essentially a custom version of the List class that is optimized for this application.
    /// Tables are *not* TablePredicates, but each TablePredicate has a Table in it to hold its data.
    /// </summary>
    public abstract class Table
    {
        #region Row numbers
        /// <summary>
        /// Row index to return when not no matching row is found
        /// Also used to mark the end of a linked list of rows
        /// </summary>
        public const uint NoRow = uint.MaxValue;

        /// <summary>
        /// Used to mark linked lists for hash buckets that are allocated but have empty lists
        /// This happens when something is added to an index but then all rows with that value are removed.
        /// </summary>
        public const uint DeletedRow = NoRow - 1;

        /// <summary>
        /// True when the row number is neither NoRow (end of a linked list of rows)
        /// nor DeletedRow (marks a hash bucket that's allocated but has an empty list in it).
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ValidRow(uint row) => row < DeletedRow;
        #endregion

        /// <summary>
        /// Set this before creating tables.  If true, track provenance for all tables.
        /// </summary>
        public static bool TrackAllProvenance = false;

        #region Instance fields and properties
        /// <summary>
        /// Name of table for debugging purposes
        /// </summary>
#pragma warning disable CS8618
        public string Name { get; internal set; }
#pragma warning restore CS8618

        /// <inheritdoc />
        public override string ToString() => $"Table<{Name}>";

        /// <summary>
        /// Number of rows in the table, regardless of the size of the underlying array.
        /// </summary>
        public uint Length { get; protected set; }

        /// <summary>
        /// For use with tables that support compaction. Target fraction of space that should be used after compaction.
        /// If more than this fraction of space is in use, the table will expand its space.  Default is 0.5 (50%).
        /// </summary>
        public float PostCompactionTargetLoad = 0.5f;

        /// <summary>
        /// If true, the rows of the table are required to be different from one another.
        /// In other words, this is a set rather than a bag.  Unique tables keep a hash table
        /// of all the rows (using a RowSet object) and use this to suppress adding duplicate rows.
        /// In addition, their TablePredicates can be queries in constant time rather than linear time
        /// when the call has all its arguments instantiated.
        /// </summary>
        public abstract bool Unique { get; set; }

        /// <summary>
        /// List of all the Indices into the table, be they KeyIndex or GeneralIndex.
        /// These are kept sorted in order of decreasing desirability of use.  So first KeyIndices, then GeneralIndices.
        /// </summary>
        internal readonly List<TableIndex> Indices = new List<TableIndex>();

        /// <summary>
        /// Parallel array to Data whose i'th element has the source code to the rule that generated row i.
        /// </summary>
        public string?[]? Provenance;

        /// <summary>
        /// If true, this table tracks which rule generated each row
        /// </summary>
        public virtual bool TrackProvenance
        {
            get => Provenance != null;
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// Exceptions generated during table update but not thrown directly
        /// Compiled code and driver methods for updating base tables write here rather than directly throwing the exception
        /// </summary>
        public readonly List<Exception> DeferredExceptions = new();

        internal void ThrowPendingDeferredExceptions()
        {
            if (DeferredExceptions.Count > 0)
            {
                var exceptions = DeferredExceptions.ToArray();
                DeferredExceptions.Clear();
                throw exceptions.Length > 1 ? new AggregateException(exceptions) : exceptions[0];
            }
        }

        internal void ThrowDeferred(Exception e)
        {
            DeferredExceptions.Add(e);
        }
        #endregion

        #region Column projection and mutation
        /// <summary>
        /// Returns the key value given the row
        /// </summary>
        public delegate TColumn Projection<TRow, out TColumn>(in TRow row);

        /// <summary>
        /// Given two column projections, make a projection that returns the two as a tuple.
        /// </summary>
        /// <param name="p1">Projection for the first column</param>
        /// <param name="p2">Projection for the second column</param>
        /// <typeparam name="TRow">Row type</typeparam>
        /// <typeparam name="TColumn1">Type of the first column</typeparam>
        /// <typeparam name="TColumn2">Type of the second column</typeparam>
        /// <returns>Combined projection</returns>
        public static Projection<TRow, (TColumn1, TColumn2)> JointProjection<TRow, TColumn1, TColumn2>(
            Projection<TRow, TColumn1> p1, Projection<TRow, TColumn2> p2)
            => (in TRow row) => (p1(in row), p2(in row));

        /// <summary>
        /// Modifies the value of a column in a row.
        /// </summary>
        /// <param name="row">The row to modify (in place)</param>
        /// <param name="newValue">New value for the column</param>
        public delegate void Mutator<TRow, TColumn>(ref TRow row, in TColumn newValue);
        #endregion

        #region Row update
        /// <summary>
        /// Remove all rows from the table
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// The fastest add method that is safe to use with this table.
        /// </summary>
        internal string AddMethodForCompiledCode =>
            Indices.Count == 0 && !Unique ?
                nameof(Table<string>.RebuildRowNonUnique)
                : nameof(Table<string>.RebuildRowUnique);
        #endregion

        #region Index management
        /// <summary>
        /// Add an index to the table.
        /// This will not test for a duplicate index on the same column.
        /// </summary>
        internal TableIndex AddIndex(TableIndex i)
        {
            if (i.IsKey) SetKeyIndex(i);
            Indices.Add(i);
            Indices.Sort();
            return i;
        }
        
        /// <summary>
        /// Find the index for the specified columns
        /// </summary>
        public TableIndex IndexFor(params int[] columns) => Indices.FirstOrDefault(i => i.ColumnNumbers.SequenceEqual(columns))??throw new InvalidOperationException($"No index found for specified column(s) of table {Name}");

        /// <summary>
        /// Designate this index as the index to use when finding rows for AddOrReplace.
        /// It doesn't matter which key index is used for this purpose, but it must be a KeyIndex.
        /// </summary>
        /// <param name="tableIndex"></param>
        protected abstract void SetKeyIndex(TableIndex tableIndex);

        /// <summary>
        /// Re-sort the list of indices in order of decreasing desirability of use.
        /// </summary>
        internal void UpdateIndexOrdering()
        {
            Indices.Sort();
        }
        #endregion

        #region Space management
        /// <summary>
        /// Predicate over a table row.  Used for user-defined row reclamation policies.
        /// </summary>
        public delegate bool RowTest<T>(in T row);

        /// <summary>
        /// Set the test used to decide if a row should be reclaimed
        /// </summary>
        /// <param name="t">A RowTest that returns true if a row should be reclaimed</param>
        public abstract void SetReclamationRowTest(Delegate t);

        /// <summary>
        /// Force deletion of reclaimable rows.
        /// This will not grow the underlying array.
        /// </summary>
        internal abstract void UnsafeReclaim();
        #endregion
    }

    /// <summary>
    /// A list of rows that hold the extension of a predicate
    /// </summary>
    /// <typeparam name="T">Type of the rows of the table (a tuple of the predicate arguments)</typeparam>
    public sealed class Table<T> : Table, IEnumerable<T>
    {
        /// <summary>
        /// Make a new, empty table with no indices and no rows.
        /// </summary>
        public Table()
        {
            Data = new T[InitialSize];
            if (TrackAllProvenance)
                Provenance = new string?[Data.Length];
        }

        #region Instance fields
        /// <summary>
        /// If true, enforce that rows of the table are unique, by making a hashtable of them
        /// </summary>
        public override bool Unique
        {
            get => rowSet != null;
            set
            {
                if (value)
                    rowSet ??= new RowSet(this);
                else
                    rowSet = null;
            }
        }

        /// <summary>
        /// Array holding the rows
        /// Elements 0 .. data.Length-1 hold the elements
        /// IMPORTANT: Data.Length must be a power of 2.
        /// </summary>
        public T[] Data;

        /// <summary>
        /// HashSet of Rows in the table, if Unique is true.  Otherwise, null.
        /// </summary>
        private RowSet? rowSet;

        /// <summary>
        /// The canonical key index for this table, if any.
        /// This is used by AddOrReplace to find the row it's replacing.
        /// If there are multiple key indices, this is one of them, but it doesn't matter which.
        /// If there are no key indices, this is null.
        /// </summary>
        internal TableIndex<T>? KeyIndex;

        /// <inheritdoc/>
        protected override void SetKeyIndex(TableIndex tableIndex)
        {
            KeyIndex = (TableIndex<T>)tableIndex;
        }

        /// <inheritdoc />
        public override bool TrackProvenance
        {
            set
            {
                if (value && Provenance == null)
                    Provenance = new string[Data.Length];
            }
        }

        /// <summary>
        /// If defined, then when the table runs out of space, it will delete all rows satisfying this predicate
        /// </summary>
        public RowTest<T>? ReclaimRowTest;

        /// <summary>
        /// Declares that the table may (but need not) reclaim rows for which the specified
        /// predicate returns true
        /// </summary>
        public override void SetReclamationRowTest(Delegate t)
        {
            ReclaimRowTest = (RowTest<T>)t;
        }
        #endregion

        #region Rebuild interface
        /// <summary>
        /// Prepare for rebuilding all the data in the table.
        /// This clears the data in the table and any RowSet is might have, but does not clear indices;
        /// those are batch rebuilt in EndRebuild().  This is safe because rules for a table cannot reference
        /// the table itself.
        /// </summary>
        public void BeginRebuild()
        {
            Length = 0;
            rowSet?.Clear();
        }

        /// <summary>
        /// Add a row to a table for which Unique == false.
        /// Does not update indices.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RebuildRowNonUnique(in T item)
        {
            Data[Length++] = item;
            EnsureSpace();
        }

        /// <summary>
        /// Adda row to a table for which Unique==true.
        /// Does not update indices.
        /// </summary>
        /// <param name="row"></param>
        public void RebuildRowUnique(T row)
        {
            // Write the data into the next free slot
            Data[Length] = row;
            // That same row might already be in the table
            // So check if it's already there.  If so, don't both incrementing Length.
            // Otherwise, add it to the rowSet and increment length.
            if (rowSet!.MaybeAddRow(Length))
                Length++;
            EnsureSpace();
        }

        /// <summary>
        /// Finish rebuilding the table.  Rebuild all indices.
        /// </summary>
        public void EndRebuild()
        {
            // Batch rebuild all indices
            foreach (var i in Indices) i.Reindex();
        }
        #endregion

        #region Space management
        /// <summary>
        /// Initial size of the Data array.  Must be a power of 2, since Data's length must always be a power of 2.
        /// </summary>
        private const int InitialSize = 16;

        /// <summary>
        /// Make sure there's space for one more row.
        /// If not, find more space
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureSpace(uint extraSpace = 1)
        {
            var requiredSize = Length + extraSpace;
            if (requiredSize >= Data.Length)
            {
                // Insufficient space
                FindSpace(extraSpace);
            }
        }

        /// <summary>
        /// Find more space, either by reclaiming rows or by growing the underlying array.
        /// </summary>
        private void FindSpace(uint extraSpace)
        {

            if (ReclaimRowTest == null)
            {
                var requiredSize = Length + extraSpace;
                // Easy case: copy everything over as a block
                ExpandDataArrays(RoundUpPowerOf2(requiredSize));
            }
            else 
                // Hard case: copy only the unreclaimed rows
                ReclaimRows(extraSpace);

            rowSet?.ExpandAndRehash();
            foreach (var i in Indices) i.ExpandAndReindex();
        }

        /// <summary>
        /// Round number up to the nearest power of 2
        /// </summary>
        uint RoundUpPowerOf2(uint n)
        {
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            return n + 1;
        }

        /// IMPORTANT!
        /// If you add more parallel arrays besides Data and Provenance, you must add code here and in ReclaimRows to
        /// expand them when needed.
        private void ExpandDataArrays(uint newSize)
        {
            Expand(ref Data, newSize);
            if (Provenance != null)
                Expand(ref Provenance, newSize);
        }

        private void Expand<TElement>(ref TElement[] array, uint newSize)
        {
            var newArray = new TElement[newSize];
            Array.Copy(array, newArray, array.Length);
            array = newArray;
        }

        /// <summary>
        /// Attempt to reclaim space by deleting rows that satisfy the ReclaimRowTest predicate.
        /// If this fails to free up enough space, expand the underlying array but copy only the unreclaimed rows.
        /// </summary>
        private void ReclaimRows(uint extraSpace)
        {
            var liveRows = MakeCompactionMap();
            var loadFactor = ((float)liveRows)/Data.Length;
            T[] dataDestination;
            string?[]? provenanceDestination = null;
            if (loadFactor < PostCompactionTargetLoad && (Data.Length-liveRows) > extraSpace)
            {
                // Compaction was sufficient; don't grow the array, just copy the live rows to the front of the existing array
                dataDestination = Data;
                provenanceDestination = Provenance;
            }
            else
            {
                // Compaction was insufficient; grow the array and copy the live rows to the new array
                var newSize = RoundUpPowerOf2(liveRows + extraSpace);
                dataDestination = new T[newSize];
                if (Provenance != null)
                    provenanceDestination = new string?[newSize];
            }
            CopyUsingCompactionMap(dataDestination, provenanceDestination);
        }

        /// <summary>
        /// Scratch buffer for compaction.  Holds runs of live rows, as pairs of (start index, length).
        /// </summary>
        private List<(uint start, uint length)>? compactionMap;

        /// <summary>
        /// Update the compactionMap.
        /// </summary>
        /// <returns>Number of live rows</returns>
        private uint MakeCompactionMap()
        {
            if (compactionMap == null)
                compactionMap = new List<(uint start, uint length)>();
            else
                compactionMap.Clear();

            var length = 0u;
            var blockStart = 0u;
            while (blockStart < Length)
            {
                // Find the start of the next block of preserved rows
                for (; blockStart < Length && ReclaimRowTest!(Data[blockStart]); blockStart++)
                {
                }

                if (blockStart == Length)
                    break;
                // Find the end of the block
                var i = blockStart + 1;
                for (; i < Length && !ReclaimRowTest!(Data[i]); i++)
                {
                }

                var blockLength = i - blockStart;
                compactionMap.Add((blockStart, blockLength));
                blockStart += blockLength;
                length += blockLength;
            }

            return length;
        }

        /// <summary>
        /// Copy subset of Data specified by compactionMap to array, compacting it in the process.
        /// This is safe to with array == Data, in which case the compaction is done in-place.
        /// </summary>
        private void CopyUsingCompactionMap(T[] array, string?[]? provenanceArray)
        {
            var dest = 0u;
            foreach (var block in compactionMap!)
            {
                Array.Copy(Data, block.start, array, dest, block.length);
                if (provenanceArray != null)
                    Array.Copy(Provenance!, block.start, provenanceArray, dest, block.length);
                dest += block.length;
            }

            Data = array;
            Length = dest;
        }

        /// <summary>
        /// For testing purposes only.
        /// Force immediate removal of rows satisfying the ReclaimRowTest predicate, and compact the table to remove gaps.
        /// This is unsafe because it doesn't reindex the table.
        /// </summary>
        internal override void UnsafeReclaim()
        {
            MakeCompactionMap();
            CopyUsingCompactionMap(Data, Provenance);
        }
        #endregion
        
        #region Row access
        /// <summary>
        /// The data of the index'th row
        /// </summary>
        /// <param name="index">Position in the table to retrieve the row</param>
        /// <returns>Row data</returns>
        public T this[uint index] => Data[index];

        /// <summary>
        /// Return a reference (pointer) to the row at the specified position
        /// This lets the row to be passed to another method without copying
        /// </summary>
        public ref T PositionReference(uint index) => ref Data[index];
        #endregion

        #region Update
        /// <summary>
        /// Remove all rows from the table
        /// </summary>
        public override void Clear()

        {
            Length = 0;
            rowSet?.Clear();
            foreach (var i in Indices)
                i.Clear();
        }
        
        /// <summary>
        /// Add a row
        /// </summary>
        /// <param name="item">The row to add</param>
        /// <returns>The index of the added row, or NoRow if the row was not because Unique = true and the row is already in the table</returns>
        public uint Add(in T item)
        {
            EnsureSpace();
            var row = Length;
            // Write the data into the next free slot
            Data[row] = item;
            // That same row might already be in the table
            // So check if it's already there.  If so, don't both incrementing Length.
            // Otherwise, add it to the rowSet and increment length.
            if (rowSet == null || rowSet.MaybeAddRow(row))
            {
                foreach (var i in Indices)
                    i.Add(row);
                Length++;
                return row;
            }

            return NoRow;
        }

        /// <summary>
        /// Overwrite the row at the specified position with new data.  Update general indices accordingly.
        /// Note that this must not change the keys!
        /// </summary>
        internal void ReplaceRow(uint row, in T item)
        {
            foreach (var i in Indices)
                if (!i.IsKey)  // Don't update indices that are keys, because the key value is not changing
                    i.Remove(row);
            Data[row] = item;
            foreach (var i in Indices)
                if (!i.IsKey)  // Don't update the indices that are keys
                    i.Add(row);
        }

        /// <summary>
        /// If no row has the key in the specified rowData, add the row to the table.
        /// Otherwise, replace the existing row with this data.
        /// If the table has multiple keys, this must not change the key values, or it will break the table indices.
        /// </summary>
        internal uint AddOrReplace(in T rowData)
        {
            var row = KeyIndex!.RowWithKey(in rowData);
            if (row == Table.NoRow)
            {
                row = Length;
                Add(rowData);
            }
            else if (Indices.Count == 1)
                // Fast path: KeyIndex is the only index so we don't need to add/remove from rows.
                Data[row] = rowData;
            else
                ReplaceRow(row, rowData);

            return row;
        }

        /// <summary>
        /// Append all data in extra to the end of this table
        /// Update indices, etc. accordingly.
        /// </summary>
        internal void Append(Table<T> extra, bool overwrite)
        {
            var copyProvenance = Provenance != null && extra.Provenance != null;

            if (overwrite)
            {
                // Row by row copy using AddOrReplace
                for (uint i = 0; i < extra.Length; i++)
                {
                    var row = AddOrReplace(extra.Data[i]);
                    if (copyProvenance)
                        Provenance![row] = extra.Provenance![i];
                }
            }
            else if (rowSet != null)
            {
                // Need to enforce uniqueness, so copy row by row using Add (which enforces uniqueness)
                for (uint i = 0; i < extra.Length; i++)
                {
                    var row = Add(extra.Data[i]);
                    if (copyProvenance && row != NoRow)
                        Provenance![row] = extra.Provenance![i];
                }
            }
            else
            {
                // Fast path: bulk copy, followed by indexing
                EnsureSpace(extra.Length);
                Array.Copy(extra.Data, 0, Data, Length, extra.Length);
                if (Provenance != null && extra.Provenance != null)
                    Array.Copy(extra.Provenance, 0, Provenance, Length, extra.Length);
                for (uint i = 0; i < extra.Length; i++)
                    foreach (var index in Indices)
                        index.Add(Length + i);
                Length += extra.Length;
            }
        }
        #endregion

        #region Rowsets
        /// <summary>
        /// Check if the row is already contained in the table.
        /// Assumes rowSet is bound and contains the set of rows already in the table.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        public bool ContainsRowUsingRowSet(in T row) => rowSet!.ContainsRow(row);

        /// <summary>
        /// Report back all the rows, in order
        /// This allocates storage, so it shouldn't be used in inner loops.
        /// </summary>
        internal class RowSet
        {
            private uint[] buckets;
            private uint mask;
            private readonly Table<T> table;
            const uint Empty = NoRow;
            private static readonly EqualityComparer<T> Comparer = EqualityComparer<T>.Default;

            public RowSet(Table<T> t)
            {
                table = t;
                var capacity = t.Data.Length * 2;
                buckets = new uint[capacity];
                Array.Fill(buckets, Empty);
                mask = (uint)(capacity - 1);
                Debug.Assert((mask & capacity) == 0, "Capacity must be a power of 2");
            }

            // We proactively left shift by 1 place to reduce clustering
            private static uint HashInternal(T value, uint mask) => (uint)Comparer.GetHashCode(value) & mask;

            public bool ContainsRow(in T value)
            {
                for (var b = HashInternal(value, mask); buckets[b] != Empty; b = b + 1 & mask)
                    if (Comparer.Equals(table.Data[buckets[b]], value))
                        return true;
                return false;
            }

            /// <summary>
            /// Add row to set if it's not already in the set.  Return true if it was added.
            /// </summary>
            /// <param name="row">row number of the row</param>
            /// <returns>True if it was added, false if there was already a row equal to the one at this index</returns>
            public bool MaybeAddRow(uint row)
            {
                uint b;
                for (b = HashInternal(table.Data[row], mask); buckets[b] != Empty; b = b + 1 & mask)
                    if (Comparer.Equals(table.Data[buckets[b]], table.Data[row]))
                        // It's already there
                        return false;

                // It's not there, but b is a free bucket, so store it there
                buckets[b] = row;
                return true;
            }

            internal void ExpandAndRehash()
            {
                var targetLength = table.Data.Length * 2;
                if (buckets.Length != targetLength)
                    buckets = new uint[targetLength];
                mask = (uint)(buckets.Length - 1);
                Array.Fill(buckets, Empty);
                for (uint i = 0; i < table.Length; i++)
                    MaybeAddRow(i);
            }

            public void Clear()
            {
                Array.Fill(buckets, Empty);
            }
        }
        #endregion

        #region Table enumeration
        internal class TableEnumerator : IEnumerator<T>
        {
            private readonly T[] array;
            private readonly int limit;
            private int position;

            public TableEnumerator(T[] array, int limit)
            {
                this.array = array;
                this.limit = limit;
                position = -1;
            }

            public bool MoveNext() => ++position < limit;

            public void Reset()
            {
                position = -1;
            }

            public T Current => array[position];

            object IEnumerator.Current => Current!;

            public void Dispose()
            { }
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator() => new TableEnumerator(Data, (int)Length);

        IEnumerator IEnumerable.GetEnumerator() => new TableEnumerator(Data, (int)Length);
        #endregion
    }
}
