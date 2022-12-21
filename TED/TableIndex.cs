using System;

namespace TED
{
    internal abstract class TableIndex
    {
        public abstract bool IsKey { get; }

        public abstract void Add(uint row);
        public abstract void Expand();

        public abstract void Clear();

        public readonly int ColumnIndex;

        /// <summary>
        /// Row index to return when not no matching row is found
        /// </summary>
        public const uint NotFound = UInt32.MaxValue;

        protected TableIndex(int columnIndex)
        {
            ColumnIndex = columnIndex;
        }

        internal static TableIndex MakeIndex<TRow, TKey>(TablePredicate p, Table<TRow> t, int columnIndex,
            Func<TRow, TKey> projection, bool isKey)
            => isKey
                ? new KeyIndex<TRow, TKey>(p, t, columnIndex, projection)
                : throw new NotImplementedException("Non-key indices not yet implemented");
    }
}
