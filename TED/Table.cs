using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace TED
{
    /// <summary>
    /// Untyped base class for all tables
    /// Tables are essentially a custom version of the List class that is optimized for this application.
    /// Tables are *not* TablePredicates, but each TablePredicate has a Table in it to hold its data.
    /// </summary>
    internal abstract class Table
    {
        /// <summary>
        /// Row index to return when not no matching row is found
        /// </summary>
        public const uint NoRow = UInt32.MaxValue;

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
        internal void AddIndex(TableIndex i) => Indices.Add(i);
    }
    /// <summary>
    /// A list of rows that hold the extension of a predicate
    /// </summary>
    /// <typeparam name="T">Type of the rows of the table (a tuple of the predicate arguments)</typeparam>
    internal class Table<T> : Table, IEnumerable<T>
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
        internal T[] Data;

        /// <summary>
        /// True if there's space to add another row before having to grow the table
        /// </summary>
        //bool SpaceRemaining => Length < data.Length;

        private RowSet? rowSet;

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
                var newArray = new T[Data.Length * 2];
                Array.Copy(Data, newArray, Data.Length);
                Data = newArray;
                rowSet?.Expand();
                foreach (var i in Indices) i.Expand();
            }
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
                var capacity = t.Data.Length*2;
                buckets = new uint[capacity];
                Array.Fill(buckets, Empty);
                mask = (uint)(capacity - 1);
                Debug.Assert((mask&capacity) == 0, "Capacity must be a power of 2");
            }

            // We proactively left shift by 1 place to reduce clustering
            private static uint HashInternal(T value, uint mask) => (uint)Comparer.GetHashCode(value) & mask;

            public bool ContainsRow(in T value)
            {
                for (var b = HashInternal(value, mask); buckets[b] != Empty; b = (b+1)&mask)
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
                for (b = HashInternal(table.Data[row], mask); buckets[b] != Empty; b = ((b+1)&mask))
                    if (Comparer.Equals(table.Data[buckets[b]], table.Data[row]))
                        // It's already there
                        return false;

                // It's not there, but b is a free bucket, so store it there
                buckets[b] = row;
                return true;
            }

            public void Expand()
            {
                var newBuckets = new uint[buckets.Length*2];
                var newMask = (uint)(newBuckets.Length - 1);
                Array.Fill(newBuckets, Empty);
                for (var b = 0u; b < buckets.Length; b++)
                {
                    var row = buckets[b];
                    if (row != Empty)
                    {
                        uint nb;
                        // Find a free bucket
                        for (nb = HashInternal(table.Data[row], newMask); newBuckets[nb] != Empty; nb = (nb + 1) & newMask)
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
