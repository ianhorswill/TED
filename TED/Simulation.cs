using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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
            FindInitializationOnlyPredicates();
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
            {
                if (t.ImperativeDependencies.Any()
                    || t is { IsIntensional: true, IsPure: false, InitializationOnly: false })
                    MarkDynamic(t);
            }

            void MarkDynamic(TablePredicate p)
            {
                if (p.IsDynamic)
                    return;
                p.IsDynamic = true;
                foreach (var d in p.Dependents)
                    MarkDynamic(d);
                foreach (var d in p.ImperativeDependencies)
                    MarkDynamic(d);
            }

            DynamicTables = Tables.Where(t => t.IsDynamic).ToArray();
        }

        private void FindInitializationOnlyPredicates()
        {
            // Find the .Initially tables and make sure they're only used for initialization.
            foreach (var t in Tables)
            {
                var i = t.InitialValueTable;
                if (i is { Dependents: { Count: 0 } }) 
                    i.InitializationOnly = true;
            }

            // Queue of tables newly discovered to be initialization only
            var q = new Queue<TablePredicate>();
            foreach (var t in Tables)
                if (t.InitializationOnly)
                    q.Enqueue(t);
            while (q.Count > 0)
            {
                var i = q.Dequeue();
                foreach (var t in i.RuleDependencies)
                    if (!t.InitializationOnly
                        && t.Dependents.Count > 0 
                        && t.Dependents.All(d => d.InitializationOnly))
                    {
                        // We just discovered t is also initialization-only
                        t.InitializationOnly = true;
                        q.Enqueue(t);
                    }
            }
        }

#if PROFILER
        /// <summary>
        /// Average combined execution time for all the rules in the simulation
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public float RuleExecutionTime => DynamicTables.Select(t => t.RuleExecutionTime).Sum();
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

        private Task[]? UpdateTasks;

        public async Task UpdateAsync()
        {
            if (UpdateTasks == null)
            {
                UpdateTasks = new Task[DynamicTables.Length];
            }

            foreach (var t in DynamicTables)
                t.ResetUpdateTask();

            var i = 0;
            foreach (var t in DynamicTables)
            {
                UpdateTasks[i++] = t.UpdateTask;
            }
            Task.WaitAll(UpdateTasks);
        }
    }
}
