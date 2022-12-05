namespace TED
{
    /// <summary>
    /// Untyped base class for calls to TablePredicates
    /// </summary>
    internal abstract class AnyTableCallTest : AnyCall
    {
        /// <summary>
        /// What row we will text next in the table
        /// </summary>
        protected bool primed;

        /// <summary>
        /// Move back to the beginning of the table.
        /// </summary>
        public override void Reset() => primed = true;

        protected AnyTableCallTest(AnyPredicate predicate) : base(predicate)
        {
        }
    }
}
