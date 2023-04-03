using System.Diagnostics;
using System.Linq;

namespace TED
{
    /// <summary>
    /// A simulation written in TED.
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public class Simulation : Program
    {
        /// <summary>
        /// Make a simulation with the specified name
        /// </summary>
        public Simulation(string name) : base(name)
        { }

        #if PROFILER
        /// <summary>
        /// Average combined execution time for all the rules in the simulation
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public float RuleExecutionTime => Tables.Select(t => t.RuleExecutionTime).Sum();
        #endif

        /// <inheritdoc />
        public override string ToString() => Name;

        /// <summary>
        /// Update all tables in the simulation
        /// </summary>
        public void Update()
        {
            RecomputeAll();
            AppendAllInputs();
        }

        /// <summary>
        /// Forcibly recompute all the predicates
        /// </summary>
        public void RecomputeAll()
        {
            foreach (var p in Tables)
                if (p.IsIntensional)
                    p.MustRecompute = true;
            foreach (var p in Tables)
                p.EnsureUpToDate();
        }

        /// <summary>
        /// Append all the inputs to tables that accumulate other tables
        /// </summary>
        public void AppendAllInputs()
        {
            foreach (var p in Tables)
                p.AppendInputs();
        }
    }
}
