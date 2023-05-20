using System;
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

        /// <inheritdoc />
        public override void EndPredicates()
        {
            base.EndPredicates();
            FindDynamicPredicates();
            CheckTableDefinitions();
            InitializeTables();
        }

        private void InitializeTables()
        {
            foreach (var t in Tables)
                if (t.IsStatic)
                    t.EnsureUpToDate();

            foreach (var t in Tables)
                t.AddInitialData();
        }

        private void CheckTableDefinitions()
        {
            foreach (var t in Tables)
                if (t.IsIntensional)
                {
                    if (t.Inputs.Any())
                        throw new InvalidProgramException($"Predicate {t.Name} has both rules and inputs.");
                    if (t.ColumnUpdateTables.Any())
                        throw new InvalidProgramException($"Table {t.Name} has both rules and Set() rules.");
                    if (t.InitialValueTable != null)
                        foreach (var d in t.InitialValueTable.RuleDependencies)
                            if (d.IsDynamic)
                                throw new InvalidProgramException(
                                    $"The Initially rules for {t.Name} depend on {d.Name}, which is a dynamic table.");

                }
        }

        /// <summary>
        /// The tables in the simulation that vary from tick to tick
        /// </summary>
        public TablePredicate[] DynamicTables = Array.Empty<TablePredicate>();

        private void FindDynamicPredicates()
        {
            foreach (var t in Tables)
                t.IsDynamic = t.MustBeDynamic;

            void MarkDynamic(TablePredicate p)
            {
                if (p.IsDynamic)
                    return;
                p.IsDynamic = true;
                foreach (var d in p.Dependents)
                    MarkDynamic(d);
            }

            foreach (var t in Tables)
                foreach (var d in t.ImperativeDependencies)
                    MarkDynamic(d);

            DynamicTables = Tables.Where(t => t.IsDynamic).ToArray();
        }

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
            UpdateDynamicBaseTables();
            AppendAllInputs();
        }

        /// <summary>
        /// Forcibly recompute all the predicates
        /// </summary>
        public void RecomputeAll()
        {
            foreach (var p in DynamicTables)
                if (p.IsIntensional)
                    p.MustRecompute = true;
            foreach (var p in Tables)
                p.EnsureUpToDate();
        }

        /// <summary>
        /// Run any column updates defined using the Set() method.
        /// </summary>
        public void UpdateDynamicBaseTables()
        {
            foreach (var p in DynamicTables)
                if (p.IsExtensional)
                    p.UpdateColumns();
        }

        /// <summary>
        /// Append all the inputs to tables that accumulate other tables
        /// </summary>
        public void AppendAllInputs()
        {
            foreach (var p in DynamicTables)
                if (p.IsExtensional)
                    p.AppendInputs();
        }
    }
}
