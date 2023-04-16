namespace TED.Tables
{
    /// <summary>
    /// Base type of indices into tables
    /// </summary>
    public abstract class TableIndex
    {
        /// <summary>
        /// Position of the column: 0=first column, 1=second, etc.
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
        internal abstract void Add(uint row);

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
        // ReSharper disable once UnusedMemberInSuper.Global
        internal abstract void Reindex();

        /// <summary>
        /// The index for the specified column, if any
        /// </summary>
        protected TableIndex(int columnNumber)
        {
            ColumnNumber = columnNumber;
        }

        /// <summary>
        /// Make an index, either keyed or not keyed, depending on the isKey argument
        /// </summary>
        internal static TableIndex MakeIndex<TRow, TColumn>(TablePredicate p, Table<TRow> t, int columnIndex,
            Table.Projection<TRow, TColumn> projection, bool isKey)
            => isKey
                ? (TableIndex)new KeyIndex<TRow, TColumn>(p, t, columnIndex, projection)
                : new GeneralIndex<TRow, TColumn>(p, t, columnIndex, projection);
    }

    /// <summary>
    /// Typed base class of all table indices
    /// </summary>
    /// <typeparam name="TRow">Type of the rows of the table</typeparam>
    /// <typeparam name="TColumn">Type of the column being indexed</typeparam>
    public abstract class TableIndex<TRow, TColumn> : TableIndex
    {
        /// <inheritdoc />
        protected TableIndex(int columnNumber, Table.Projection<TRow,TColumn> projection) : base(columnNumber)
        {
            this.projection = projection;
        }

        /// <summary>
        /// Function to extract the column value from a given row
        /// </summary>
        // ReSharper disable once InconsistentNaming
        protected readonly Table.Projection<TRow,TColumn> projection;
    }

}

