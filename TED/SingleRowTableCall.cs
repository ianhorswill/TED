namespace TED
{
    /// <summary>
    /// Untyped base class for calls to TablePredicates whose arguments are satisfiable by at most one row,
    /// so they don't require enumerating rows
    /// </summary>
    internal abstract class SingleRowTableCall : AnyCall
    {
        /// <summary>
        /// What row we will text next in the table
        /// </summary>
        protected bool primed;

        /// <summary>
        /// Move back to the beginning of the table.
        /// </summary>
        public override void Reset() => primed = true;

        protected SingleRowTableCall(AnyPredicate predicate) : base(predicate)
        {
        }
    }
}
