using System.Collections.Generic;

namespace TED.Tables
{
    public abstract class ColumnAccessor<TColumn, TKey>
    {
        /// <summary>
        /// Get or set the value of the column for whatever row matches the specified key
        /// </summary>
        /// <param name="key">Key of the row to access</param>
        /// <exception cref="KeyNotFoundException">If no row has that key</exception>
        public abstract TColumn this[TKey key] { get; set; }
    }

    /// <summary>
    /// Wraps a column of a table to make it look like a mutable dictionary mapping keys to column values.
    /// Updating the value will modify the underlying table row.
    /// </summary>
    /// <typeparam name="TRow">Type of the table rows</typeparam>
    /// <typeparam name="TColumn">Type of the column to access</typeparam>
    /// <typeparam name="TKey">Type of the key used to select a unique row</typeparam>
    public sealed class ColumnAccessor<TRow, TColumn, TKey> : ColumnAccessor<TColumn, TKey>
    {
        private readonly Table<TRow> table;
        private readonly KeyIndex<TRow, TKey> keyIndex;
        private readonly Table.Projection<TRow,TColumn> projection;

        private readonly GeneralIndex<TRow, TColumn>? columnIndex;

        private readonly Table.Mutator<TRow,TColumn> mutator;

        internal ColumnAccessor(Table<TRow> table, KeyIndex<TRow, TKey> keyIndex, Table.Projection<TRow,TColumn> projection, GeneralIndex<TRow, TColumn>? columnIndex, Table.Mutator<TRow,TColumn> mutator)
        {
            this.table = table;
            this.keyIndex = keyIndex;
            this.projection = projection;
            this.columnIndex = columnIndex;
            if (columnIndex != null)
                columnIndex.EnableMutation();
            this.mutator = mutator;
        }

        /// <inheritdoc />
        public override TColumn this[TKey key]
        {
            get
            {
                var row = keyIndex.RowWithKey(key);
                if (row == Table.NoRow)
                    throw new KeyNotFoundException($"Key value {key} not found in table");
                return projection(table.Data[row]);
            }

            set
            {
                var row = keyIndex.RowWithKey(key);
                if (row == Table.NoRow)
                    throw new KeyNotFoundException($"Key value {key} not found in table");
                if (columnIndex != null)
                    columnIndex.Remove(row);
                mutator(ref table.Data[row], in value);
                if (columnIndex != null)
                    columnIndex.Add(row);
            }
        }
    }
}
