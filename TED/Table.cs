using System;
using System.Collections.Generic;

namespace TED
{
    /// <summary>
    /// A list of rows that hold the extension of a predicate
    /// </summary>
    /// <typeparam name="T">Type of the rows of the table (a tuple of the predicate arguments)</typeparam>
    internal class Table<T> : AnyTable
    {
        /// <summary>
        /// Array holding the rows
        /// Elements 0 .. data.Length-1 hold the elements
        /// </summary>
        private T[] data = new T[16];

        /// <summary>
        /// True if there's space to add another row before having to grow the table
        /// </summary>
        bool SpaceRemaining => Length < data.Length;

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
            }
        }

        /// <summary>
        /// Add a row
        /// </summary>
        /// <param name="item">The row to add</param>
        public void Add(in T item)
        {
            EnsureSpace(1);
            data[Length++] = item;
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
    }
}
