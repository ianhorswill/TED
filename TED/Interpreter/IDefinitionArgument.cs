namespace TED.Interpreter
{
    /// <summary>
    /// A formal argument for a Definition
    /// This is an IVariable plus an optional input-output mode
    /// </summary>
    public interface IDefinitionArgument
    {
        /// <summary>
        /// The variable that is the formal argument
        /// </summary>
        public IVariable UntypedVariable { get; }

        /// <summary>
        /// Optional mode constraining whether the argument must be input, output, or a literal constant.
        /// </summary>
        public InstantiationConstraint? Mode { get; }
    }

    /// <summary>
    /// A formal argument for a Definition
    /// This is an Var[T] plus an optional input-output mode
    /// </summary>
    public interface IDefinitionArgument<T> : IDefinitionArgument
    {
        /// <summary>
        /// The variable that is the formal argument
        /// </summary>
        public Var<T> TypedVariable { get; }
    }

    /// <summary>
    /// An argument for a Definition that has a type constraint added to it.
    /// </summary>
    public class ModeConstrainedArgument<T> : IDefinitionArgument<T>
    {
        /// <inheritdoc />
        public Var<T> TypedVariable { get; private set; }

        /// <inheritdoc />
        public IVariable UntypedVariable=> TypedVariable;

        /// <inheritdoc />
        public InstantiationConstraint? Mode { get; private set; }

        /// <summary>
        /// Make a constrained version of the specified formal argument
        /// </summary>
        /// <param name="variable">Variable used as a formal argument</param>
        /// <param name="mode">Input/output mode to constraint it to</param>
        public ModeConstrainedArgument(Var<T> variable, InstantiationConstraint mode)
        {
            TypedVariable = variable;
            Mode = mode;
        }
    }
}
