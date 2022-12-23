using System;
using System.Collections.Generic;

namespace TED
{
    /// <summary>
    /// Untyped base class for all tables
    /// Tables are essentially a custom version of the List class that is optimized for this application
    /// </summary>
    internal abstract class AnyTable
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

        internal readonly List<TableIndex> Indices = new List<TableIndex>();

        public abstract bool Unique { get; set; }

        internal void AddIndex(TableIndex i) => Indices.Add(i);
    }
}
