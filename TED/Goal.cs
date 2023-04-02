using System.Text;

namespace TED
{
    /// <summary>
    /// Untyped base class for the abstract syntax tree of a goal (a predicate applied to arguments)
    /// These are used:
    /// - As the heads of rules
    /// - In the bodies of rules
    /// - As arguments to higher-order predicates appearing in the bodies of rules.
    ///
    /// Goals are different from calls.  A Goal is the AST representing a predicate applied to arguments
    /// these are then "compiled" into Call objects when they're used in the body of a rule.
    /// </summary>
    public abstract class Goal
    {
        /// <summary>
        /// All arguments, as an array.  Used by the printer
        /// </summary>
        public readonly Term[] Arguments;

        /// <summary>
        /// The predicate being called
        /// </summary>
        public abstract Predicate Predicate { get; }

        /// <summary>
        /// Make an object representing a specific goal, i.e. a specific call to a predicate within a rule.
        /// </summary>
        protected Goal(Term[] arguments) => Arguments = arguments;

        /// <summary>
        /// Apply a substitution to the arguments of this goal
        /// </summary>
        /// <param name="s">Substitution to apply</param>
        /// <returns>Transformed goal</returns>
        internal abstract Goal RenameArguments(Substitution s);

        /// <summary>
        /// Return the call object 
        /// </summary>
        /// <param name="ga">Goal scanner used at "compile time" to do binding analysis of variables and track dependencies.</param>
        /// <returns>The Call object</returns>
        internal abstract Call MakeCall(GoalAnalyzer ga);

        /// <summary>
        /// True if the goal is false
        /// Sugar for a call to Language.Not.
        /// </summary>
        /// <param name="g">Goal to negate</param>
        /// <returns></returns>
        public static Goal operator !(Goal g) => Language.Not[g];

        /// <summary>
        /// True if both the left-hand side and right-hand side goals are true.
        /// Sugar for a call to Language.And.
        /// </summary>
        /// <param name="lhs">Left-hand side goal to and</param>
        /// <param name="rhs">Right-hand side goal to and</param>
        /// <returns></returns>
        public static Goal operator &(Goal lhs, Goal rhs) => Language.And[lhs, rhs];

        #region Printing
        /// <summary>
        /// Convert the goal to a human-readable string, for purposes of printing.
        /// </summary>
        public override string ToString()
        {
            var b = new StringBuilder();
            ToString(b);
            return b.ToString();
        }
        
        /// <summary>
        /// Add the printed representation of the goal to this StringBuilder.
        /// </summary>
        public void ToString(StringBuilder b)
        {
            b.Append(Predicate.Name);
            b.Append('[');
            var first = true;
            foreach (var arg in Arguments)
            {
                if (first)
                    first = false;
                else
                    b.Append(", ");

                b.Append(arg);
            }

            b.Append(']');
        }

        /// <summary>
        /// This is just so that this appears in human-readable form in the debugger
        /// </summary>
        public string DebugName => ToString();
        #endregion
    }
}
