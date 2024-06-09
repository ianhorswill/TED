using System;
using System.Diagnostics;
using TED.Compiler;

namespace TED.Interpreter
{
    /// <summary>
    /// Untyped base class for all calls to predicates.
    /// These are essentially iterators for the call: they contain the state information used for
    /// backtracking.  Calls to different kinds of predicates or with different mode patterns may
    /// have different call objects because they need different state information
    /// </summary>
    [DebuggerDisplay("{DebugString}")]
    public abstract class Call
    {
        /// <summary>
        /// The predicate being called
        /// </summary>
        public readonly Predicate Table;

        /// <summary>
        /// True if the call always produces the same results for the same input tables, and has no side effects of its own.
        /// </summary>
        public virtual bool IsPure => Table.IsPure;

        /// <summary>
        /// Make an object to interpret a particular goal in a particular rule.
        /// </summary>
        /// <param name="table"></param>
        protected Call(Predicate table)
        {
            Table = table;
        }

        /// <summary>
        /// The formal arguments in the call - the variables and constants for each argument
        /// </summary>
        public abstract IPattern ArgumentPattern { get; }

        /// <summary>
        /// Called before the start of a call.  Initializes any backtracking state for the call
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Find the first/next solution to the call, writing any variables that need to be bound by it
        /// </summary>
        /// <returns>True if it found another solution</returns>
        public abstract bool NextSolution();

        /// <inheritdoc />
        public override string ToString() => $"{Table.Name}{ArgumentPattern}";

        private string DebugString => ToString();

        /// <summary>
        /// Generate C# source code for executing this call.
        /// </summary>
        /// <param name="compiler">Compiler object generating the source code</param>
        /// <param name="fail">Continuation to invoke if the call fails.</param>
        /// <param name="identifierSuffix">Suffix unique to this call that can be added to identifiers in output source to prevent conflicts</param>
        /// <returns>Continuation the next call in the rule should invoke upon failure</returns>
        public virtual Continuation Compile(Compiler.Compiler compiler, Continuation fail, string identifierSuffix)
        {
            throw new NotImplementedException();
        }
    }
}
