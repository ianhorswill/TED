namespace TED
{
    /// <summary>
    /// Untyped base class of all Rules
    /// Rule objects are essentially the compiled form of rules specified using .If or .Fact
    /// </summary>
    internal abstract class AnyRule
    {
        /// <summary>
        /// Body of the rule - a sequence of Call objects for each goal in the If.
        /// </summary>
        public readonly AnyCall[] Body;
        /// <summary>
        /// All the TablePredicates called directly by this rule.
        /// These tables must be computed before this rule runs.
        /// </summary>
        public readonly TablePredicate[] Dependencies;

        protected AnyRule(AnyCall[] body, TablePredicate[] dependencies)
        {
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
    }
}
