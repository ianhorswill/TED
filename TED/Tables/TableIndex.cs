using System;
using System.Linq;
using TED.Interpreter;

namespace TED.Tables
{
    /// <summary>
    /// Base type of indices into tables
    /// </summary>
    public abstract class TableIndex : IComparable<TableIndex>
    {
        /// <summary>
        /// TablePredicate corresponding to the table
        /// </summary>
        protected TablePredicate Predicate;

        /// <summary>
        /// Position of the column: 0=first column, 1=second, etc.
        /// </summary>
        public readonly int[] ColumnNumbers;

        /// <summary>
        /// Delegate to generate calls for this table given a goal's IPattern.
        /// </summary>
        public delegate Call CallGenerator(IPattern p);

        /// <summary>
        /// Delegate to generate calls for this table given a goal's IPattern.
        /// </summary>
        protected CallGenerator GenerateCall;

        internal void SetCallGenerator(CallGenerator g) => GenerateCall = g;

        private int priority;

        /// <summary>
        /// Indices with higher priority numbers are used in preference to indices with smaller numbers
        /// </summary>
        public int Priority
        {
            get => priority;
            set
            {
                priority = value;
                Predicate.TableUntyped.UpdateIndexOrdering();
            }
        }

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
#pragma warning disable CS8618
        protected TableIndex(TablePredicate p, int[] columnNumbers)
#pragma warning restore CS8618
        {
            Predicate = p;
            Array.Sort(columnNumbers);
            ColumnNumbers = columnNumbers;
            // ReSharper disable once VirtualMemberCallInConstructor
            priority = IsKey ? 1000 : 100 * columnNumbers.Length;
        }

        /// <summary>
        /// True if all the columns for this index are read mode in the specified pattern.
        /// </summary>
        public bool CanMatchOn(IPattern pattern) => ColumnNumbers.All(pattern.IsReadModeAt);

        /// <summary>
        /// Make an index, either keyed or not keyed, depending on the isKey argument
        /// </summary>
        internal static TableIndex MakeIndex<TRow, TColumn>(TablePredicate p, Table<TRow> t, int[] columnIndices,
            Table.Projection<TRow, TColumn> projection, bool isKey)
            => isKey
                ? (TableIndex)new KeyIndex<TRow, TColumn>(p, t, columnIndices, projection)
                : new GeneralIndex<TRow, TColumn>(p, t, columnIndices, projection);

        internal static TableIndex MakeIndex<TRow, TColumn>(TablePredicate p, Table<TRow> t, int columnIndex,
            Table.Projection<TRow, TColumn> projection, bool isKey) =>
            MakeIndex(p, t, new[] { columnIndex }, projection, isKey);

        /// <summary>
        /// Make a call using arguments p and this index.
        /// </summary>
        internal Call MakeCall(IPattern p) => GenerateCall(p);

        public int CompareTo(TableIndex? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            return -Priority.CompareTo(other!.Priority);
        }
    }

    /// <summary>
    /// Typed base class of all table indices
    /// </summary>
    /// <typeparam name="TRow">Type of the rows of the table</typeparam>
    /// <typeparam name="TColumn">Type of the column being indexed</typeparam>
    public abstract class TableIndex<TRow, TColumn> : TableIndex
    {
        /// <inheritdoc />
        protected TableIndex(TablePredicate p, int[] columnNumbers, Table.Projection<TRow,TColumn> projection) 
            : base(p, columnNumbers)
        {
            this.projection = projection;
            GenerateCall = pat =>
                Predicate.MakeIndexCall(this, pat, (ValueCell<TColumn>)pat.ArgumentCell(ColumnNumbers[0]));
        }

        /// <summary>
        /// Function to extract the column value from a given row
        /// </summary>
        // ReSharper disable once InconsistentNaming
        protected readonly Table.Projection<TRow,TColumn> projection;
    }

}

