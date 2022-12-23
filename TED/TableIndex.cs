using System;

namespace TED
{
    /// <summary>
    /// Base type of indices into tables
    /// </summary>
    internal abstract class TableIndex
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly int ColumnNumber;

        /// <summary>
        /// If true, this index is for a column that is a key, i.e. rows have unique values for this column
        /// </summary>
        public abstract bool IsKey { get; }

        /// <summary>
        /// Add a row to the index
        /// </summary>
        /// <param name="row">Position within the table array of the row to add</param>
        public abstract void Add(uint row);

        /// <summary>
        /// Double the size of the table.
        /// </summary>
        internal abstract void Expand();

        /// <summary>
        /// Remove all data from the index
        /// </summary>
        internal abstract void Clear();

        /// <summary>
        /// Forcibly rebuild the index
        /// </summary>
        internal abstract void Reindex();

        protected TableIndex(int columnNumber)
        {
            ColumnNumber = columnNumber;
        }

        /// <summary>
        /// Make an index, either keyed or not keyed, depending on the isKey argument
        /// </summary>
        internal static TableIndex MakeIndex<TRow, TKey>(TablePredicate p, Table<TRow> t, int columnIndex,
            Func<TRow, TKey> projection, bool isKey)
            => isKey
                ? (TableIndex)new KeyIndex<TRow, TKey>(p, t, columnIndex, projection)
                : new GeneralIndex<TRow, TKey>(p, t, columnIndex, projection);
    }
}
