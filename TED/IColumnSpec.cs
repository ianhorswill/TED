using System;
using System.Collections.Generic;
using System.Text;

namespace TED
{
    /// <summary>
    /// A specification of a column/argument in the declaration of a table predicate
    /// </summary>
    public interface IColumnSpec
    {
        /// <summary>
        /// Default variable for this column
        /// </summary>
        AnyTerm UntypedVariable { get; }

        /// <summary>
        /// Whether to index this column
        /// </summary>
        IndexMode IndexMode { get; }

        /// <summary>
        /// Name of this column
        /// </summary>
        string ColumnName { get; }
    }

    /// <summary>
    /// Used in the constructor of a TablePredicate to specify information about a column (argument) of the table
    /// </summary>
    public interface IColumnSpec<T> : IColumnSpec
    {
        /// <summary>
        /// Default variable for this column
        /// </summary>
        public Var<T> TypedVariable { get;  }
    }

    public class IndexedColumnSpec<T> : IColumnSpec<T>
    {
        private readonly Var<T> variable;
        private readonly IndexMode mode;

        /// <summary>
        /// Default variable for this column
        /// </summary>
        public Var<T> TypedVariable => variable;

        public AnyTerm UntypedVariable => variable;

        /// <summary>
        /// Whether to maintain an index for the column
        /// </summary>
        public IndexMode IndexMode => mode;

        /// <summary>
        /// Specify information about a column/argument of a table
        /// </summary>
        /// <param name="defaultVariable">Default variable to use</param>
        /// <param name="indexMode">Whether to maintain an index</param>
        public IndexedColumnSpec(Var<T> defaultVariable, IndexMode indexMode)
        {
            variable = defaultVariable;
            mode = indexMode;
        }

        public string ColumnName => variable.Name;
    }
}
