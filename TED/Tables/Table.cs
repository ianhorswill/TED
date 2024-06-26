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
        /// <summary>
        /// Name of table for debugging purposes
        /// </summary>
#pragma warning disable CS8618
        public string Name { get; internal set; }
#pragma warning restore CS8618

        /// <inheritdoc />
        public override string ToString() => $"Table<{Name}>";

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

        /// <summary>
        /// Number of rows in the table, regardless of the size of the underlying array.
        /// </summary>
        public uint Length { get; protected set; }

        /// <summary>
        /// Remove all rows from the table
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// List of all the Indices into the table, be they KeyIndex or GeneralIndex.
        /// </summary>
        internal readonly List<TableIndex> Indices = new List<TableIndex>();
        
        /// <summary>
        /// If true, the rows of the table are required to be different from one another.
        /// In other words, this is a set rather than a bag.  Unique tables keep a hash table
        /// of all the rows (using a RowSet object) and use this to suppress adding duplicate rows.
        /// In addition, their TablePredicates can be queries in constant time rather than linear time
        /// when the call has all its arguments instantiated.
        /// </summary>
        public abstract bool Unique { get; set; }

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

        protected abstract void SetKeyIndex(TableIndex tableIndex);

        internal void UpdateIndexOrdering()
        {
            Indices.Sort();
        }

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
        public abstract void Reclaim();

        /// <summary>
        /// For use with tables that support compaction. Target fraction of space that should be used after compaction.
        /// If more than this fraction of space is in use, the table will expand its space.  Default is 0.5 (50%).
        /// </summary>
        public float PostCompactionTargetLoad = 0.5f;
    }

    /// <summary>
    /// A list of rows that hold the extension of a predicate
    /// </summary>
    /// <typeparam name="T">Type of the rows of the table (a tuple of the predicate arguments)</typeparam>
    public class Table<T> : Table, IEnumerable<T>
    {
        public Table()
        {
            Data = new T[InitialSize];
        }

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

        // Must be a power of 2
        private const int InitialSize = 16;

        /// <summary>
        /// Array holding the rows
        /// Elements 0 .. data.Length-1 hold the elements
        /// </summary>
        public T[] Data;

        /// <summary>
        /// True if there's space to add another row before having to grow the table
        /// </summary>
        //bool SpaceRemaining => Length < data.Length;

        private RowSet? rowSet;

        
        /// <summary>
        /// The key index for this table, if any.
        /// </summary>
        internal TableIndex<T>? KeyIndex;

        protected override void SetKeyIndex(TableIndex tableIndex)
        {
            KeyIndex = (TableIndex<T>)tableIndex;
        }

        /// <summary>
        /// If defined, then when the table runs out of space, it will delete all rows satisfying this predicate
        /// </summary>
        public RowTest<T>? ReclaimRowTest;

        public override void SetReclamationRowTest(Delegate t)
        {
            ReclaimRowTest = (RowTest<T>)t;
        }

        public override void Clear()

        {
            Length = 0;
            rowSet?.Clear();
            foreach (var i in Indices)
                i.Clear();
        }

        /// <summary>
        /// Make sure there's space for more rows
        /// If not, make a new array that's twice as big and copy over the data.
        /// </summary>
        /// <param name="extra"></param>
        private void EnsureSpace(int extra)
        {
            if (Length + extra > Data.Length)
            {
                if (ReclaimRowTest == null)
                {
                    // Easy case: copy everything over as a block
                    var newArray = new T[Data.Length * 2];
                    Array.Copy(Data, newArray, Data.Length);
                    Data = newArray;
                }
                else 
                    // Hard case: copy only the unreclaimed rows
                    ReclaimRows();

                rowSet?.Expand();
                foreach (var i in Indices) i.Expand();
            }
        }

        private void ReclaimRows()
        {
            var liveRows = MakeCompactionMap();
            var loadFactor = ((float)liveRows)/Data.Length;
            var destination = Data;
            if (loadFactor > PostCompactionTargetLoad)
                destination = new T[Data.Length * 2];
            CopyUsingCompactionMap(destination);
        }

        private List<(int start, int length)>? compactionMap;

        private int MakeCompactionMap()
        {
            if (compactionMap == null)
                compactionMap = new List<(int start, int length)>();
            else
                compactionMap.Clear();

            var length = 0;
            var blockStart = 0;
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

        private void CopyUsingCompactionMap(T[] array)
        {
            var dest = 0;
            foreach (var block in compactionMap!)
            {
                Array.Copy(Data, block.start, array, dest, block.length);
                dest += block.length;
            }

            Data = array;
            Length = (uint)dest;
        }

        public override void Reclaim()
        {
            MakeCompactionMap();
            CopyUsingCompactionMap(Data);
        }

        /// <summary>
        /// Add a row
        /// </summary>
        /// <param name="item">The row to add</param>
        public void Add(in T item)
        {
            EnsureSpace(1);
            // Write the data into the next free slot
            Data[Length] = item;
            // That same row might already be in the table
            // So check if it's already there.  If so, don't both incrementing Length.
            // Otherwise, add it to the rowSet and increment length.
            if (rowSet == null || rowSet.MaybeAddRow(Length))
            {
                foreach (var i in Indices)
                    i.Add(Length);
                Length++;
            }
        }

        internal void ReplaceRow(uint row, in T item)
        {
            foreach (var i in Indices)
                if (!i.IsKey)
                    i.Remove(row);
            Data[row] = item;
            foreach (var i in Indices)
                if (!i.IsKey)
                    i.Add(row);
        }

        internal void AddOrReplace(in T item)
        {
            var row = KeyIndex!.RowWithKey(in item);
            if (row == Table.NoRow)
                Add(item);
            else if (Indices.Count == 1)
                // Fast path: KeyIndex is the only index so we don't need to add/remove from rows.
                Data[row] = item;
            else
                ReplaceRow(row, item);
        }

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

        public bool ContainsRowUsingRowSet(in T row) => rowSet!.ContainsRow(row);

        /// <summary>
        /// Report back all the rows, in order
        /// This allocates storage, so it shouldn't be used in inner loops.
        /// </summary>
        public class RowSet
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

            public void Expand()
            {
                var newBuckets = new uint[buckets.Length * 2];
                var newMask = (uint)(newBuckets.Length - 1);
                Array.Fill(newBuckets, Empty);
                for (var b = 0u; b < buckets.Length; b++)
                {
                    var row = buckets[b];
                    if (row != Empty)
                    {
                        uint nb;
                        // Find a free bucket
                        for (nb = HashInternal(table.Data[row], newMask); newBuckets[nb] != Empty; nb = nb + 1 & newMask)
                        { }  // Do nothing
                        newBuckets[nb] = row;
                    }
                }

                buckets = newBuckets;
                mask = newMask;
            }

            public void Clear()
            {
                Array.Fill(buckets, Empty);
            }
        }

        public class TableEnumerator : IEnumerator<T>
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

        public IEnumerator<T> GetEnumerator() => new TableEnumerator(Data, (int)Length);

        IEnumerator IEnumerable.GetEnumerator() => new TableEnumerator(Data, (int)Length);
    }
}
