using System.Text;

namespace TED
{
    /// <summary>
    /// Untyped base class of all Rules
    /// Rule objects are the preprocessed form of rules specified using .If or .Fact.
    /// They consist of the Pattern for the head of the rule and an array of call objects
    /// for each of the subgoals in the body
    /// </summary>
    internal abstract class AnyRule
    {
        /// <summary>
        /// Predicate this rule applies to.
        /// Rules can only be applied to TablePredicates, not primitives or definitions
        /// </summary>
        public readonly TablePredicate Predicate;

        /// <summary>
        /// Pattern object for the head of the rule.
        /// The head of a rule is its "conclusion" - the thing that has to be true when the body is true.
        /// </summary>
        public abstract IPattern Head { get; }

        /// <summary>
        /// Body of the rule - a sequence of Call objects for each goal in the If.
        /// </summary>
        public readonly AnyCall[] Body;
        /// <summary>
        /// All the TablePredicates called directly by this rule.
        /// These tables must be computed before this rule runs.
        /// </summary>
        public readonly TablePredicate[] Dependencies;

        protected AnyRule(TablePredicate predicate, AnyCall[] body, TablePredicate[] dependencies)
        {
            Predicate = predicate;
            Body = body;
            Dependencies = dependencies;
        }

        /// <summary>
        /// Called after the body has computed a row and written the values of variables into it.
        /// Writes the values of each element of the row into the table.
        /// </summary>
        internal abstract void WriteHead();

        /// <summary>
        /// Repeatedly runs all the calls in the body and backtracks them to completion
        /// Calls WriteHead each time a solution is found to write it into the table.
        /// </summary>
        public void AddAllSolutions()
        {
            if (Body.Length == 0)
            {
                WriteHead();
                return;
            }

            var subgoal = 0;

            foreach (var d in Dependencies)
                d.EnsureUpToDate();

            Body[subgoal].Reset();
            while (subgoal >= 0)
            {
                if (Body[subgoal].NextSolution())
                {
                    // Succeeded
                    if (subgoal == Body.Length - 1)
                        WriteHead();
                    else
                    {
                        // Advance to the next subgoal
                        subgoal++;
                        Body[subgoal].Reset();
                    }
                }
                else
                    // Failed
                    subgoal--;
            }
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            b.Append(Predicate.Name);
            b.Append(Head);
            b.Append(".If(");
            bool firstOne = true;
            foreach (var c in Body)
            {
                if (firstOne)
                    firstOne = false;
                else b.Append(", ");
                b.Append(c);
            }

            b.Append(")");
            return b.ToString();
        }
    }
}
