namespace TED
{
    /// <summary>
    /// The iterator that handles calls to a TablePredicate that cannot be accelerated using a hash table
    /// (index or RowSet).  This will test every row of the table against the call pattern.  This can be avoided
    /// under the right circumstances by adding an index and/or setting the Unique property of the TablePredicate.
    /// </summary>
    internal abstract class AnyTableCallExhaustive : AnyCall
    {
        /// <summary>
        /// What row we will text next in the table
        /// </summary>
        protected uint RowIndex;

        /// <summary>
        /// Move back to the beginning of the table.
        /// </summary>
        public override void Reset() => RowIndex = 0;

        protected AnyTableCallExhaustive(AnyPredicate predicate) : base(predicate)
        {
        }
    }
}
