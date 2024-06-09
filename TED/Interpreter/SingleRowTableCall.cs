using TED.Compiler;

namespace TED.Interpreter
{
    /// <summary>
    /// Untyped base class for calls to TablePredicates whose arguments are satisfiable by at most one row,
    /// so they don't require enumerating rows
    /// </summary>
    internal abstract class SingleRowTableCall : Call
    {
        /// <summary>
        /// What row we will text next in the table
        /// </summary>
        protected bool Primed;

        /// <summary>
        /// Move back to the beginning of the table.
        /// </summary>
        public override void Reset() => Primed = true;

        protected SingleRowTableCall(Predicate predicate) : base(predicate)
        {
        }
    }
}
