using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TED.Interpreter;

namespace TED
{
    /// <summary>
    /// A container for a set of predicates
    /// This is basically a namespace for predicates.
    /// It's presently only used by the REPL.  So if you aren't using that, you don't need a Program.
    /// </summary>
    [DebuggerDisplay("TELL.Program {Name}")]
    public class Program
    {
        /// <summary>
        /// The top of the stack is the program to which new predicates should be added.
        /// It's a stack out of paranoia that something bad will happen if multiple programs are loaded and they get mixed.
        /// </summary>
        private static readonly Stack<Program?> LoadingPrograms = new Stack<Program?>();

        public readonly TablePredicate<Type, string, TablePredicate, Rule> Exceptions;
        public readonly TablePredicate<TablePredicate, string, Dictionary<string, object?>> Problems;

        /// <summary>
        /// Predicates and their names
        /// </summary>
        private readonly Dictionary<string, TablePredicate> tables = new Dictionary<string, TablePredicate>();

        /// <summary>
        /// All Predicates defined in this Program
        /// </summary>
        public IEnumerable<TablePredicate> Tables => tables.Select(pair => pair.Value);

        /// <summary>
        /// Base tables in the program
        /// </summary>
        public IEnumerable<TablePredicate> BaseTables => Tables.Where(t => t.IsExtensional);

        /// <summary>
        /// Name of the program, for debugging
        /// </summary>
        public readonly string Name;

        private Repl.Repl? repl;

        /// <summary>
        /// Return a REPL associated with this program, creating it if necessary.
        /// </summary>
        public Repl.Repl Repl
        {
            get
            {
                if (repl == null)
                    repl = new Repl.Repl(this);
                return repl;
            }
        }


        /// <summary>
        /// Make a new Program.  A Program is a container for predicates that are to be used with a Repl.
        /// To use: make a Program. call Program.Begin(), make a bunch of predicates, then call Program.End().
        /// Then you can access those predicates from the Repl
        /// </summary>
        /// <param name="name">Name of the program, for debugging purposes</param>
        public Program(string name)
        {
            Name = name;
            LoadingPrograms.Push(null);
            Exceptions = new TablePredicate<Type, string, TablePredicate, Rule>(nameof(Exceptions),
                (Var<Type>)"type", (Var<string>)"message", (Var<TablePredicate>)"table", (Var<Rule>)"rule");
            Problems = new TablePredicate<TablePredicate, string, Dictionary<string, object?>>(nameof(Problems),
                (Var<TablePredicate>)"table", (Var<string>)"message", TED.Primitives.CaptureDebugStatePrimitive.DebugState);
            LoadingPrograms.Pop();
        }

        /// <summary>
        /// Start defining predicates for the program.  All predicates created between calling and calling End() will be added
        /// to the program.
        /// </summary>
        public virtual void BeginPredicates() => LoadingPrograms.Push(this);

        /// <summary>
        /// Stop adding new predicates to this Program.
        /// </summary>
        public virtual void EndPredicates()
        {
            LoadingPrograms.Pop();
            FindDependents();
            CheckForCycles();
        }

        private void CheckForCycles()
        {
            var s = new Stack<TablePredicate>();

            void Visit(TablePredicate p)
            {
                if (s.Contains(p))
                    throw new InvalidProgramException($"The rules defining {p.Name} are recursive.");
                s.Push(p);
                foreach (var d in p.RuleDependencies)
                    Visit(d);
                s.Pop();
            }

            foreach (var t in Tables)
                Visit(t);
        }

        /// <summary>
        /// The predicate with the specified name
        /// </summary>
        public TablePredicate this[string name] => tables[name];

        /// <summary>
        /// The predicate with the specified name, or null if none is defined.
        /// </summary>
        public TablePredicate? PredicateNamed(string name) => tables.TryGetValue(name, out var p) ? p : null;

        internal static void MaybeAddPredicate(TablePredicate p)
        {
            if (LoadingPrograms.Count > 0 && LoadingPrograms.Peek() != null)
            {
                var program = LoadingPrograms.Peek();
                program.Add(p);
                p.Program = program;
            }
        }

        /// <summary>
        /// Add a new predicate to the program/simulation
        /// </summary>
        /// <param name="predicate">Predicate to add</param>
        /// <exception cref="InvalidOperationException">IF there is already a predicate by that name in the program/simulation</exception>
        public void Add(TablePredicate predicate)
        {
            if (tables.ContainsKey(predicate.Name))
                throw new InvalidOperationException(
                    $"Program {Name} already contains a predicate by the name {predicate.Name}");
            tables[predicate.Name] = predicate;
        }

        /// <summary>
        /// Remove a table from the program
        /// </summary>
        /// <param name="table">Table to remove</param>
        public void Remove(TablePredicate table)
        {
            Debug.Assert(tables[table.Name] == table, "Attempting to remove table from program but program has a different table listed under that name.");
            tables.Remove(table.Name);
        }

        /// <summary>
        /// Build dependent lists for all tables in the program.
        /// </summary>
        public void FindDependents()
        {
            foreach (var t in Tables)
                t.Dependents.Clear();
            foreach (var t in Tables)
            foreach (var d in t.RuleDependencies.Concat(t.ImperativeDependencies).Concat(t.OperatorDependencies))
                d.Dependents.Add(t);
        }
    }
}
