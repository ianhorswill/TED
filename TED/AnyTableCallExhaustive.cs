namespace TED
{
    /// <summary>
    /// Untyped base class for calls to TablePredicates that exhaustively enumerate all rows of the table
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
