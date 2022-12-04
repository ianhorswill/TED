using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TED
{
    /// <summary>
    /// A list of rows that hold the extension of a predicate
    /// </summary>
    /// <typeparam name="T">Type of the rows of the table (a tuple of the predicate arguments)</typeparam>
    internal class Table<T> : AnyTable
    {
        public Table()
        {
            data = new T[InitialSize];
            rowSet = new RowSet(this);
        }

        // Must be a power of 2
        private const int InitialSize = 16;

        /// <summary>
        /// Array holding the rows
        /// Elements 0 .. data.Length-1 hold the elements
        /// </summary>
        private T[] data;

        /// <summary>
        /// True if there's space to add another row before having to grow the table
        /// </summary>
        bool SpaceRemaining => Length < data.Length;

        private RowSet rowSet;

        public override void Clear()

        {
            Length = 0;
            rowSet.Clear();
        }

        /// <summary>
        /// Make sure there's space for more rows
        /// If not, make a new array that's twice as big and copy over the data.
        /// </summary>
        /// <param name="extra"></param>
        private void EnsureSpace(int extra)
        {
            if (Length + extra > data.Length)
            {
                var newArray = new T[data.Length * 2];
                Array.Copy(data, newArray, data.Length);
                data = newArray;
                rowSet.Expand();
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
            data[Length] = item;
            // That same row might already be in the table
            // So check if it's already there.  If so, don't both incrementing Length.
            // Otherwise, add it to the rowSet and increment length.
            if (rowSet.MaybeAddRow(Length))
                Length++;
        }

        /// <summary>
        /// The data of the index'th row
        /// </summary>
        /// <param name="index">Position in the table to retrieve the row</param>
        /// <returns>Row data</returns>
        public T this[uint index] => data[index];

        /// <summary>
        /// Return a reference (pointer) to the row at the specified position
        /// This lets the row to be passed to another method without copying
        /// </summary>
        public ref T PositionReference(uint index) => ref data[index];

        /// <summary>
        /// Report back all the rows, in order
        /// This allocates storage, so it shouldn't be used in inner loops.
        /// </summary>
        public IEnumerable<T> Rows
        {
            get
            {
                for (var i = 0; i < Length; i++)
                    yield return data[i];
            }
        }

        public class RowSet
        {
            private uint[] buckets;
            private uint mask;
            private Table<T> table;
            const uint Empty = UInt32.MaxValue;
            private static readonly EqualityComparer<T> Comparer = EqualityComparer<T>.Default;

            public RowSet(Table<T> t)
            {
                table = t;
                var capacity = t.data.Length;
                buckets = new uint[capacity];
                Array.Fill(buckets, Empty);
                mask = (uint)(capacity - 1);
                Debug.Assert((mask&capacity) == 0, "Capacity must be a power of 2");
            }

            // We proactively left shift by 1 place to reduce clustering
            private static uint HashInternal(T value, uint mask) => (uint)(Comparer.GetHashCode(value) << 1) & mask;

            public bool Probe(in T value)
            {
                for (var b = HashInternal(value, mask); buckets[b] != Empty; b = (b+1)&mask)
                    if (Comparer.Equals(table.data[buckets[b]], value))
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
                for (b = HashInternal(table.data[row], mask); buckets[b] != Empty; b = ((b+1)&mask))
                    if (Comparer.Equals(table.data[buckets[b]], table.data[row]))
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
                        for (nb = HashInternal(table.data[row], newMask); newBuckets[nb] != Empty; nb = (nb + 1) & newMask) ;
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
    }
}
