using System;
using System.Diagnostics;
using TED.Interpreter;
using TED.Preprocessing;
using TED.Tables;

namespace TED
{
    /// <summary>
    /// Represents a variable in a Goal (abstract syntax tree of a call to a predicate)
    /// This is not the run-time representation of a variable.  That is held in a ValueCell.
    /// </summary>
    /// <typeparam name="T">Type of the variable's value</typeparam>
    [DebuggerDisplay("{DebugName}")]
    public class Var<T> : Term<T>, IColumnSpec<T>, IVariable, IDefinitionArgument<T>
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

        /// <inheritdoc />
        public Goal EquateTo(Term t) => this == (Term<T>)t;

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

        /// <inheritdoc />
        public override string ToString() => Name;

        internal string DebugName => ToString();

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
        // ReSharper disable once UnusedMember.Global
        public IColumnSpec<T> Indexed => new IndexedColumnSpec<T>(this, IndexMode.NonKey);

        /// <summary>
        /// Make a column spec for this variable that specifies this column should be indexed, but is not a key, and should have the specified priority.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public IColumnSpec<T> IndexPriority(int priority) => new IndexedColumnSpec<T>(this, IndexMode.NonKey, priority);

        /// <summary>
        /// For variables being used as formal parameters in predicate declarations: whether this column of the table is indexed or not
        /// </summary>
        public IndexMode IndexMode => IndexMode.None;

        /// <inheritdoc cref="IColumnSpec" />
        public Var<T> TypedVariable => this;

        /// <inheritdoc cref="IColumnSpec" />
        public IVariable UntypedVariable => this;

        /// <inheritdoc />
        public InstantiationConstraint? Mode => null;

        /// <summary>
        /// For use as a formal argument in a definition; this argument must be an input, i.e. a value that will be known at call time.
        /// </summary>
        public ModeConstrainedArgument<T> In => new ModeConstrainedArgument<T>(this, InstantiationConstraint.In);
        /// <summary>
        /// For use as a formal argument in a definition; this argument must be an output, i.e. a variable that will not yet have a value at call time.
        /// </summary>
        public ModeConstrainedArgument<T> Out => new ModeConstrainedArgument<T>(this, InstantiationConstraint.Out);
        /// <summary>
        /// For use as a formal argument in a definition; this argument must be a literal constant.
        /// </summary>
        public ModeConstrainedArgument<T> Constant => new ModeConstrainedArgument<T>(this, InstantiationConstraint.Constant);
    }
}
