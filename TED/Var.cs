using System;
using System.Diagnostics;

namespace TED
{
    /// <summary>
    /// Represents a variable in a Goal (abstract syntax tree of a call to a predicate)
    /// This is not the run-time representation of a variable.  That is held in a ValueCell.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("{DebugName}")]
    public class Var<T> : Term<T>, IColumnSpec<T>, IVariable
    {
        /// <summary>
        /// Make a new variable
        /// </summary>
        /// <param name="name">Human-readable name of the variable</param>
        public Var(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Name of the variable, for debugging purposes
        /// </summary>
        public readonly string Name;

        /// <inheritdoc />
        public string ColumnName => Name;

        /// <inheritdoc />
        public string VariableName => Name;
        
        /// <summary>
        /// Make a TED variable of the specified type and name
        /// </summary>
        /// <param name="s"></param>
        public static explicit operator Var<T>(string s) => new Var<T>(s);

        internal override Term<T> ApplySubstitution(Substitution s) => s.SubstituteVariable(this);

        internal override Func<T> MakeEvaluator(GoalAnalyzer ga)
        {
            var ma = ga.Emit(this);
            if (!ma.IsInstantiated)
                throw new InstantiationException(
                    $"Variable {this} used in a functional expression before it is bound to a value.");
            var cell = ma.ValueCell;
            return () => cell.Value;
        }

        public override string ToString() => Name;

        public string DebugName => ToString();

        /// <summary>
        /// Make a different variable with the same name and type
        /// </summary>
        public Term Clone() => new Var<T>(Name);

        /// <summary>
        /// Make a column spec for this variable that specifies the column should be indexed as a key
        /// </summary>
        public IColumnSpec<T> Key => new IndexedColumnSpec<T>(this, IndexMode.Key);

        /// <summary>
        /// Make a column spec for this variable that specifies this column should be indexed, but is not a key.
        /// </summary>
        public IColumnSpec<T> Indexed => new IndexedColumnSpec<T>(this, IndexMode.NonKey);

        public IndexMode IndexMode => IndexMode.None;

        public Var<T> TypedVariable => this;

        public IVariable UntypedVariable => this;
    }
}
