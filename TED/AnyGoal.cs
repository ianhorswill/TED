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
    public abstract class AnyGoal
    {
        /// <summary>
        /// All arguments, as an array.  Used by the printer
        /// </summary>
        public readonly AnyTerm[] Arguments;

        public abstract AnyPredicate Predicate { get; }

        protected AnyGoal(AnyTerm[] arguments)
        {
            Arguments = arguments;
        }

        /// <summary>
        /// Return the call object 
        /// </summary>
        /// <param name="ga">Goal scanner used at "compile time" to do binding analysis of variables and track dependencies.</param>
        /// <returns>The Call object</returns>
        internal abstract AnyCall MakeCall(GoalAnalyzer ga);

        /// <summary>
        /// True if the goal is false
        /// Sugar for a call to Primitives.Not.
        /// </summary>
        /// <param name="g">Goal to negate</param>
        /// <returns></returns>
        public static AnyGoal operator !(AnyGoal g) => Language.Not[g];

        
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
