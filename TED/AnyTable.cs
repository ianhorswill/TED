using System;
using System.Collections.Generic;

namespace TED
{
    /// <summary>
    /// Untyped base class for all tables
    /// Tables are essentially a custom version of the List class that is optimized for this application.
    /// Tables are *not* TablePredicates, but each TablePredicate has a Table in it to hold its data.
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
}
