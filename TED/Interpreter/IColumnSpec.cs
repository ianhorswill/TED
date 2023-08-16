using TED.Tables;

namespace TED.Interpreter
{
    /// <summary>
    /// A specification of a column/argument in the declaration of a table predicate
    /// </summary>
    public interface IColumnSpec
    {
        /// <summary>
        /// Default variable for this column
        /// </summary>
        IVariable UntypedVariable { get; }

        /// <summary>
        /// Whether to index this column
        /// </summary>
        IndexMode IndexMode { get; }

        /// <summary>
        /// Name of this column
        /// </summary>
        string ColumnName { get; }

        /// <summary>
        /// For general indices, the priority of this index
        /// </summary>
        int? Priority => null;
    }

    /// <summary>
    /// Used in the constructor of a TablePredicate to specify information about a column (argument) of the table
    /// </summary>
    public interface IColumnSpec<T> : IColumnSpec
    {
        /// <summary>
        /// Default variable for this column
        /// </summary>
        public Var<T> TypedVariable { get; }
    }

    internal class IndexedColumnSpec<T> : IColumnSpec<T>
    {
        private readonly Var<T> variable;

        /// <summary>
        /// Default variable for this column
        /// </summary>
        public Var<T> TypedVariable => variable;

        public IVariable UntypedVariable => variable;

        /// <summary>
        /// Whether to maintain an index for the column
        /// </summary>
        public IndexMode IndexMode { get; }

        private readonly int? priority;

        public int? Priority => priority;

        /// <summary>
        /// Specify information about a column/argument of a table
        /// </summary>
        /// <param name="defaultVariable">Default variable to use</param>
        /// <param name="indexMode">Whether to maintain an index</param>
        /// <param name="priority">Relative priority for using this index in a call versus other indices</param>
        public IndexedColumnSpec(Var<T> defaultVariable, IndexMode indexMode, int? priority = null)
        {
            variable = defaultVariable;
            IndexMode = indexMode;
            this.priority = priority;
        }

        public string ColumnName => variable.Name;
    }
}
