using System;
using System.Collections.Generic;
using System.Linq;
using TED.Interpreter;

namespace TED.Preprocessing
{
    /// <summary>
    /// Keeps the state involved in scanning the body of a rule:
    /// - What TablePredicates does this rule depend on (reference)
    /// - What variables appear in the rule?
    /// - What ValueCells do they correspond to?
    /// - Have they been bound by some previous goal yet?
    /// </summary>
    public class GoalAnalyzer
    {
        private GoalAnalyzer(Dictionary<Term, ValueCell> variableCells, HashSet<Term>? boundVars, HashSet<TablePredicate> dependencies)
        {
            variableValueCells = variableCells;
            if (boundVars != null)
                BoundVariables.UnionWith(boundVars);
            tableDependencies = dependencies;
        }

        internal GoalAnalyzer() : this(new Dictionary<Term, ValueCell>(), null, new HashSet<TablePredicate>())
        { }

        /// <summary>
        /// Makes a goal analyzer identical to this one, except that any subsequent value cells won't be added to this analyzer
        /// Dependencies will be added, however.
        /// </summary>
        public GoalAnalyzer MakeChild()
            => new GoalAnalyzer(variableValueCells, BoundVariables, tableDependencies);

        /// <summary>
        /// Maps Var objects, which are the abstract syntax tree representation for a TED variable,
        /// to ValueCell objects, which are the containers used to hold the variable's value at run-time.
        /// The key type of the dictionary is AnyTerm just because Var is a generic type, it's parent, Term
        /// is also a generic type, and so AnyTerm is the most immediate ancestor that's a parent to all Vars.
        /// </summary>
        private readonly Dictionary<Term, ValueCell> variableValueCells;

        /// <summary>
        /// Variables that are currently bound to values
        /// </summary>
        public readonly HashSet<Term> BoundVariables = new HashSet<Term>();

        private readonly HashSet<TablePredicate> tableDependencies;

        /// <summary>
        /// Note that the rule being processed depends on (reads from) the specified table predicate
        /// and so must run after that table predicate has been updated.
        /// </summary>
        public void AddDependency(TablePredicate p) => tableDependencies.Add(p);


        /// <summary>
        /// All the TablePredicates that the rule calls.
        /// The update system needs to know this so it can insure those tables are updated before this rule is called.
        /// </summary>
        public TablePredicate[] Dependencies => tableDependencies.ToArray();

        /// <summary>
        /// Generate a MatchOperation for the specified term, updating variableValueCells as needed.
        /// </summary>
        /// <typeparam name="T">Type of the term</typeparam>
        /// <param name="term">The term</param>
        /// <returns>A MatchOperation with the right opcode (read, write, or constant) and ValueCell to match this argument to this call.</returns>
        /// <exception cref="InvalidOperationException">If this is a term that can't be used as an argument</exception>
        public MatchOperation<T> Emit<T>(Term<T> term)
        {
            if (term is Constant<T> c)
                return MatchOperation<T>.Constant(c.Value);
            // it's a variable
            if (!(term is Var<T> v))
                throw new InvalidOperationException($"{term} cannot be used as an argument to a predicate because it is not a constant or a variable");
            if (BoundVariables.Contains(v))
                return MatchOperation<T>.Read((ValueCell<T>)variableValueCells[v]);
            if (!variableValueCells.ContainsKey(v))
            {
                var vc = ValueCell<T>.MakeVariable(v);
                variableValueCells[v] = vc;
            }

            BoundVariables.Add(v);
            return MatchOperation<T>.Write((ValueCell<T>)variableValueCells[v]);
        }

        /// <summary>
        /// Forcibly make a MatchOperation that will write v, even when v is already instantiated.
        /// Used for iteration constructs like Maximize.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public MatchOperation<T> MakeWrite<T>(Var<T> v) => MatchOperation<T>.Write((ValueCell<T>)variableValueCells[v]);

        /// <summary>
        /// ValueCells for all the variables in the rule
        /// </summary>
        public ValueCell[] VariableValueCells() => variableValueCells.Select(p => p.Value).ToArray();

        public ValueCell ValueCell(Term v) => variableValueCells[v];

        /// <summary>
        /// True if subsequent uses of this variable will be match to the value in the cell rather than store into it.
        /// </summary>
        public bool IsInstantiated(Term v) => BoundVariables.Contains(v);
    }
}
