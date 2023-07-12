using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TED.Interpreter;
using TED.Tables;
using TED.Utilities;

// ReSharper disable UnusedMember.Global

namespace TED {
    /// <summary>
    /// Untyped base class for TablePredicates
    /// These are predicates that store an explicit list (table) of their extensions (all their ground instances, aka rows)
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public abstract class TablePredicate : Predicate {
        /// <summary>
        /// Create a new table predicate without knowing its column types in advance.
        /// Note that this will not work on platforms that do not allow dynamic code generation,
        /// such as iOS, because of the way that generics are implemented in the CLR.
        /// </summary>
        /// <param name="name">Name to give to the new predicate</param>
        /// <param name="columns">Default variables for the columns</param>
        /// <returns>The table predicate</returns>
        /// <exception cref="ArgumentException">If columns is empty or longer than 8 elements.</exception>
        public static TablePredicate Create(string name, IVariable[] columns) {
            var types = columns.Select(v => v.Type).ToArray();

            var generic = columns.Length switch {
                1 => typeof(TablePredicate<>),
                2 => typeof(TablePredicate<,>),
                3 => typeof(TablePredicate<,,>),
                4 => typeof(TablePredicate<,,,>),
                5 => typeof(TablePredicate<,,,,>),
                6 => typeof(TablePredicate<,,,,,,>),
                7 => typeof(TablePredicate<,,,,,,>),
                8 => typeof(TablePredicate<,,,,,,,>),
                _ => throw new ArgumentException($"Cannot create a table predicate with {columns.Length} columns")
            };
            var t = generic.MakeGenericType(types);

            var args = new object[columns.Length+1];
            args[0] = name;
            for (var i = 0; i < columns.Length; i++)
                args[i + 1] = (IColumnSpec)columns[i];
            return (TablePredicate)t.InvokeMember(null, BindingFlags.CreateInstance, null, null, args);
        }

        /// <summary>
        /// Make a new predicate
        /// </summary>
        protected TablePredicate(string name, Action<Table>? updateProc, params IColumnSpec[] columns) : base(name) {
            Program.MaybeAddPredicate(this);
            TableUntyped.Name = name;
            DefaultVariables = columns.Select(spec => spec.UntypedVariable).ToArray();
            ColumnHeadings = DefaultVariables.Select(v => v.ToString()).ToArray();
            for (var i = 0; i < columns.Length; i++)
                switch (columns[i].IndexMode) {
                    case IndexMode.Key:
                        IndexByKey(i);
                        break;

                    case IndexMode.NonKey:
                        IndexBy(i);
                        break;
                }

            this.updateProc = updateProc;
            UpdateMode = updateProc == null?UpdateMode.BaseTable:UpdateMode.Operator;
            if (UpdateMode != UpdateMode.BaseTable)
                MustRecompute = true;
        }

        /// <summary>
        /// The Program or Simulation to which this predicate belongs
        /// </summary>
        public Program? Program;

        /// <summary>
        /// True if this is a table in a simulation that needs to be updated dynamically
        /// </summary>
        public bool IsDynamic { get; internal set; }

        /// <summary>
        /// True if this table doesn't change during a simulation.
        /// </summary>
        public bool IsStatic => !IsDynamic;

        /// <summary>
        /// True if this table is only used for initialization of some other table
        /// </summary>
        public bool InitializationOnly;
        
        protected abstract Table TableUntyped { get; }

        /// <summary>
        /// If true, the underlying table enforces uniqueness of row/tuples by indexing them with a hashtable.
        /// </summary>
        public bool Unique {
            get => TableUntyped.Unique;
            set => TableUntyped.Unique = value;
        }

        /// <summary>
        /// Human-readable descriptions of columns
        /// </summary>
        public readonly string[] ColumnHeadings;

        /// <summary>
        /// Default variables for use in TrueWhen()
        /// </summary>
        public readonly IVariable[] DefaultVariables;

        /// <summary>
        /// Returns a goal of the predicate applied to the specified arguments
        /// </summary>
        /// <param name="args">Arguments to the predicate</param>
        public TableGoal this[Term[] args] => args.Length != ColumnHeadings.Length
                                                  ? throw new ArgumentException($"Wrong number of arguments to predicate {Name}")
                                                  : GetGoal(args);

        /// <summary>
        /// Return a call to this predicate using the arguments from goal, followed by additionalArguments.
        /// Note that goal will generally be a call to some other predicate
        /// </summary>
        public TableGoal AppendArgs(TableGoal goal, params Term[] additionalArguments)
            => GetGoal(goal.Arguments.Concat(additionalArguments).ToArray());

        /// <summary>
        /// Returns a goal of the predicate applied to the specified arguments
        /// </summary>
        /// <param name="args">Arguments to the predicate</param>
        public abstract TableGoal GetGoal(Term[] args);

        /// <summary>
        /// A call to this predicate using it's "default" arguments
        /// </summary>
        public TableGoal DefaultGoal => GetGoal(DefaultVariables.Cast<Term>().ToArray());

        /// <summary>
        /// Add a key index
        /// </summary>
        public void IndexByKey(int columnIndex) => AddIndex(columnIndex, true);

        /// <summary>
        /// Add a key index
        /// </summary>
        public void IndexByKey(IVariable column) => AddIndex(column, true);

        /// <summary>
        /// Add an index; the column isn't a key, i.e. rows aren't assumed to have unique values for this column
        /// </summary>
        public void IndexBy(int columnIndex) => AddIndex(columnIndex, false);

        /// <summary>
        /// Add an index; the column isn't a key, i.e. rows aren't assumed to have unique values for this column
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public void IndexBy(IVariable column) => AddIndex(column, false);

        private void AddIndex(IVariable t, bool keyIndex) {
            var index = ColumnPositionOfDefaultVariable(t);
            AddIndex(index, keyIndex);
        }

        /// <summary>
        /// Find the column/argument position of an argument, given the variable used to declare it.
        /// </summary>
        protected int ColumnPositionOfDefaultVariable(IVariable t) {
            if (DefaultVariables == null)
                throw new InvalidOperationException(
                    $"No default arguments defined for table {Name}, so no known column {t}");
            var index = Array.IndexOf(DefaultVariables, t);
            return index < 0 ? throw new ArgumentException($"Table {Name} has no column named {t}") : index;
        }

        /// <summary>
        /// Add an index to the predicate's table
        /// </summary>
        protected abstract void AddIndex(int columnIndex, bool keyIndex);

        /// <summary>
        /// Return the index of the specified type for the specified column
        /// </summary>
        /// <param name="columnIndex">Column to find the index for</param>
        /// <param name="key">Whether to look for a key or non-key</param>
        /// <returns>The index or null if there is not index of that type for that column</returns>
        public TableIndex? IndexFor(int columnIndex, bool key) 
            => TableUntyped.Indices.FirstOrDefault(i => i.ColumnNumber == columnIndex && key == i.IsKey);

        /// <summary>
        /// Return the index of the specified type for the specified column
        /// </summary>
        /// <param name="column">Column to find the index for</param>
        /// <param name="key">Whether to look for a key or non-key</param>
        /// <returns>The index or null if there is not index of that type for that column</returns>
        public TableIndex? IndexFor(IVariable column, bool key) 
            => IndexFor(ColumnPositionOfDefaultVariable(column), key);

        /// <summary>
        /// All indices for the table
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public IEnumerable<TableIndex> Indices => TableUntyped.Indices;

        /// <summary>
        /// How this table should be updated, if at all
        /// </summary>
        public UpdateMode UpdateMode { get; private set; }

        /// <summary>
        /// Rules that can be used to prove goals involving this predicate
        /// </summary>
        public List<Rule>? Rules;

        private readonly Action<Table>? updateProc;

        /// <summary>
        /// For tables that are the results of operators.  The tables the operator takes as inputs.
        /// </summary>
        public IEnumerable<TablePredicate> OperatorDependencies = Array.Empty<TablePredicate>();

        private List<TablePredicate>? updatePrerequisites;
        /// <summary>
        /// Set of tables that need to already be updated before this can be updated.
        /// </summary>
        public List<TablePredicate> UpdatePrerequisites {
            get {
                if (updatePrerequisites == null) {
                    var dependencies = UpdateMode switch {
                        UpdateMode.Operator => OperatorDependencies,
                        UpdateMode.Rules => RuleDependencies,
                        UpdateMode.BaseTable => ColumnUpdateTables.Concat(Inputs),
                        _ => throw new NotImplementedException("Unknown update mode")
                    };
                    updatePrerequisites = new List<TablePredicate>(dependencies.Where(t => t.IsDynamic && t.UpdateMode != UpdateMode.BaseTable));
                };
                return updatePrerequisites;
            }
        }

#if PROFILER
        /// <summary>
        /// Combined average execution time of all the rules for this predicate.
        /// </summary>
        public float RuleExecutionTime => Rules == null?0:Rules.Select(r => r.AverageExecutionTime).Sum();
        #endif

        /// <summary>
        /// A TablePredicate is "intensional" if it's defined by rules.  Otherwise it's "extensional"
        /// </summary>
        public bool IsIntensional => UpdateMode != UpdateMode.BaseTable;

        /// <summary>
        /// A predicate is "extensional" if it is defined by directly specifying its extension (the set of it instances/rows)
        /// using AddRow.  If it's defined by rules, it's "intensional".
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public bool IsExtensional => UpdateMode == UpdateMode.BaseTable;


        /// <inheritdoc />
        public override bool IsPure => Rules == null || Rules.All(r => r.IsPure);

        internal abstract void AddInitialData();

        /// <summary>
        /// Remove all data from the predicate's table
        /// </summary>
        public void Clear() {
            TableUntyped.Clear();
            foreach (var i in TableUntyped.Indices)
                i.Clear();
        }

        /// <summary>
        /// True if we need to recompute the predicate's table
        /// </summary>
        protected internal bool MustRecompute;

        /// <summary>
        /// Compute the predicate's table if it hasn't already or if it's out of date
        /// </summary>
        internal void EnsureUpToDate() {
            if (!MustRecompute) return;
            Clear();
            switch (UpdateMode) {
                case UpdateMode.Rules:
                    foreach (var r in Rules!)
                        r.AddAllSolutions();
                    break;

                case UpdateMode.Operator:
                    foreach (var t in OperatorDependencies)
                        t.EnsureUpToDate();
                    updateProc!(TableUntyped);
                    break;
            }
            MustRecompute = false;
        }

        /// <summary>
        /// Force the re-computation of an intensional table
        /// </summary>
        public void ForceRecompute() {
            if (IsIntensional) MustRecompute = true;
        }

        /// <summary>
        /// Add a rule to an intensional predicate
        /// </summary>
        internal void AddRule(Rule r) {
            if (UpdateMode == UpdateMode.Operator)
                throw new InvalidOperationException(
                    $"You cannot add a rule to {Name} because it is the result of an operator");
            UpdateMode = UpdateMode.Rules;
            if (!r.Head.IsInstantiated)
                throw new InstantiationException("Rules in table predicates must have fully instantiated heads");
            Rules ??= new List<Rule>();
            Rules.Add(r);
            MustRecompute = true;
        }

        /// <summary>
        /// Add a rule that concludes the default arguments of this predicate
        /// </summary>
        internal void AddRule(params Goal[] body) => DefaultGoal.If(body);

        /// <summary>
        /// All TablePredicates that are used in rules for this TablePredicate, if any.
        /// </summary>
        public IEnumerable<TablePredicate> RuleDependencies
            => (Rules==null)?Array.Empty<TablePredicate>()
                : Rules.SelectMany(r => r.Dependencies).Distinct();

        /// <summary>
        /// The tables that are used to update base tables, through .Accumulates(), .Add, or .Set()
        /// </summary>
        public IEnumerable<TablePredicate> ImperativeDependencies
            => Inputs.Concat(ColumnUpdateTables);

        /// <summary>
        /// Tables that use this table as input
        /// This is computed by Program.FindDependents().
        /// </summary>
        public readonly List<TablePredicate> Dependents = new List<TablePredicate>();

        /// <summary>
        /// Null-tolerant version of ToString.
        /// </summary>
        protected static string Stringify<T>(in T value) => value == null ? "null" : value.ToString();

        /// <summary>
        /// Write the columns of the specified tuple in to the specified array of strings
        /// </summary>
        /// <param name="rowNumber">Row number within the table</param>
        /// <param name="buffer">Buffer in which to write the string forms</param>
        public virtual void RowToStrings(uint rowNumber, string[] buffer) {}

        /// <summary>
        /// Number of tuples (rows) in the predicates extension
        /// </summary>
        public abstract uint Length { get; }

        /// <summary>
        /// Get the RowToStrings output for the specified rows in the table starting at startRow and ending
        /// when the outer array in the buffer is full or when the end of the table is reached. Returns the
        /// number of rows that were added to the buffer.
        /// </summary>
        /// <param name="startRow">Row number within the table to start range from</param>
        /// <param name="buffer">Buffer in which to write the string forms</param>
        public uint RowRangeToStrings(uint startRow, string[][] buffer) {
            uint i;
            for (i = 0u; i < buffer.Length && startRow + i < Length; i++) 
                RowToStrings(startRow + i, buffer[i]);
            return i;
        }

        /// <summary>
        /// Call the specified function on each row of the table, allowing it to overwrite them
        /// </summary>
        /// <param name="updateFn"></param>
        public delegate void Update<T>(ref T updateFn);
        
        /// <summary>
        /// Append all the rows of the tables in Inputs to this table
        /// </summary>
        internal abstract void AppendInputs();

        /// <summary>
        /// Table predicates that this table predicate accumulates, if any.
        /// </summary>
        public abstract IEnumerable<TablePredicate> Inputs { get; }

        /// <summary>
        /// Untyped interface to the .Add table.
        /// </summary>
        public abstract TablePredicate AddUntyped { get; }

        /// <summary>
        /// The table that provides initial values to this table, if any.
        /// This is untyped, so you probably want to be using Initially instead.
        /// </summary>
        public abstract TablePredicate? InitialValueTable { get; }

        /// <summary>
        /// If you put a predicate in a rule body without arguments, it defaults to the rule's "default" arguments.
        /// </summary>
        public static implicit operator Goal(TablePredicate p) => p.DefaultGoal;

        /// <summary>
        /// Verify that the header row of a CSV file matches the declared variable names
        /// </summary>
        protected static void VerifyCsvColumnNames(string name, string[] headerRow, params IColumnSpec[] args) {
            if (headerRow.Length != args.Length)
                throw new InvalidDataException(
                    $"Predicate {name} declared with {args.Length} arguments, but CSV file contains {headerRow.Length} columns");
            for (var i = 0; i < headerRow.Length; i++)
                if (string.Compare(headerRow[i], args[i].ColumnName, true, CultureInfo.InvariantCulture) != 0)
                    throw new InvalidDataException(
                        $"For predicate {name}, the column name {headerRow[i]} in the CSV file does not match the declared name {args[i].ColumnName}");
        }

        /// <summary>
        /// Attempt to cast an untyped Term to a typed Term.  Throw an exception if it's not of the right type
        /// </summary>
        /// <param name="arg">Argument term</param>
        /// <param name="position">Which argument it is to the calling primitive</param>
        /// <typeparam name="T">Expected type</typeparam>
        /// <returns>Cast term</returns>
        /// <exception cref="ArgumentException">If it's the wrong type</exception>
        protected Term<T> CastArg<T>(Term arg, int position) => arg is Term<T> result ? result
                                                                    : throw new ArgumentException(
                                                                          $"Argument {position} to {Name} should be of type {typeof(T).Name}");

        /// <summary>
        /// Return a function that returns the value of the specified column given a row.
        /// </summary>
        public virtual Delegate Projection(int columnNumber) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Return a function that modifies the value of the specified column given a row.
        /// </summary>
        public virtual Delegate Mutator(int columnNumber) {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Obtain an object that makes the column of the table behave like a dictionary
        /// Given the key for a row, you can get or set the value of the column
        /// </summary>
        /// <param name="key">Column to use as the key</param>
        /// <param name="column">Column to access</param>
        /// <typeparam name="TColumn">Data type of the column</typeparam>
        /// <typeparam name="TKey">Data type of the key</typeparam>
        /// <returns>The accessor that lets you read and write column data</returns>
        public virtual ColumnAccessor<TColumn, TKey> Accessor<TColumn, TKey>(Var<TKey> key, Var<TColumn> column) {
            throw new NotImplementedException();
        }

        public abstract Func<uint, TColumn> ColumnValueFromRowNumber<TColumn>(Var<TColumn> var);
        
        internal ColumnAccessor<TRow, TColumn, TKey> Accessor<TRow, TColumn, TKey>(Table<TRow> table, Var<TKey> key, Var<TColumn> column) {
            var keyIndex = IndexFor(ColumnPositionOfDefaultVariable(key), true);
            var columnNumber = ColumnPositionOfDefaultVariable(column);
            var columnIndex = IndexFor(columnNumber, false);
            return new ColumnAccessor<TRow, TColumn, TKey>(table,
                (KeyIndex<TRow,TKey>)keyIndex!,
                (Table.Projection<TRow, TColumn>)Projection(columnNumber),
                (GeneralIndex<TRow,TColumn>)columnIndex!,
                (Table.Mutator<TRow, TColumn>)Mutator(columnNumber));
        }

        /// <summary>
        /// Returns a comparison that given the indices of two rows in the table, compares the specified column for the two rows.
        /// </summary>
        internal Comparison<uint> RowComparison<TRow, TColumn>(Table<TRow> table, int columnNumber) {
            if (!typeof(IComparable).IsAssignableFrom(typeof(TColumn))
                && !typeof(IComparable<TColumn>).IsAssignableFrom(typeof(TColumn)))
                return null!;
            var projection = (Table.Projection<TRow, TColumn>)Projection(columnNumber);
            var comparer = System.Collections.Generic.Comparer<TColumn>.Default;
            return (a, b) => comparer.Compare(projection(table[a]), projection(table[b]));
        }

        public abstract Comparison<uint> RowComparison(int columnNumber);

        /// <summary>
        /// List of procedures to call when the table is updated.
        /// </summary>
        internal event Action? OnUpdateColumns;

        private Dictionary<(IVariable, IVariable), TablePredicate>? updateTables;

        /// <summary>
        /// Tables that drive updates of columns of this table
        /// </summary>
        public IEnumerable<TablePredicate> ColumnUpdateTables => (updateTables == null)?Array.Empty<TablePredicate>():updateTables.Select(p => p.Value);

        /// <summary>
        /// Return a table predicate to which rules can be added to update the specified column of this table predicate
        /// given a key for the row to update.
        /// </summary>
        public TablePredicate<TKey, TColumn> Set<TKey, TColumn>(Var<TKey> key, Var<TColumn> column) {
            updateTables ??= new Dictionary<(IVariable, IVariable), TablePredicate>();
            if (updateTables.TryGetValue((key, column), out var t))
                return (TablePredicate<TKey, TColumn>)t;
            var accessor = Accessor(key, column);
            var updateTable = new TablePredicate<TKey, TColumn>($"{Name}_{column.Name}_update", key, column) {
                Property = { [UpdaterFor] = this, 
                    [VisualizerName] = $"set {column.Name}"
                }
            };
            var updater = new ColumnUpdater<TColumn, TKey>(accessor, updateTable);
            OnUpdateColumns += updater.DoUpdates;
            updateTables[(key, column)] = updateTable;
            return updateTable;
        }

        /// <summary>
        /// Sets the conditions for resetting the specified column to the specified value
        /// </summary>
        public TableGoal Set<TKey, TColumn>(Var<TKey> key, Var<TColumn> column, Term<TColumn> value)
            => Set(key, column)[key, value];

        /// <summary>
        /// Run any column updates for this table
        /// </summary>
        public void UpdateColumns() {
            OnUpdateColumns?.Invoke();
        }

        /// <inheritdoc />
        public override string ToString() => Name;

        #region Meta-data
        /// <summary>
        /// Property list for meta-data.
        /// For Boolean data, Feature[] is preferred.
        /// </summary>
        public Dictionary<string, object> Property = new Dictionary<string, object>();

        /// <summary>
        /// Boolean meta-data about the table.
        /// For non-Boolean data, use Property[]
        /// </summary>
        public HashSet<string> Feature = new HashSet<string>();

        /// <summary>
        /// Name of the metadata property indicating that this table predicate is an internal table
        /// make for updating another table using Input or Set().
        /// </summary>
        public const string UpdaterFor = "UpdaterFor";

        /// <summary>
        /// Name of the metadata property specifying a preferred name to include in a dataflow visualization
        /// </summary>
        public const string VisualizerName = "VisualizerName";

        #endregion

        #region Async update

        private Task? _updateTask;

        private Task[]? prerequisiteTasks;

        internal void ResetUpdateTask() => _updateTask = null;

        public Task UpdateTask {
            get {
                if (_updateTask != null)
                    return _updateTask;
                if (UpdatePrerequisites.Count > 0) {
                    prerequisiteTasks ??= new Task[UpdatePrerequisites.Count];
                    int i = 0;
                    foreach (var p in UpdatePrerequisites)
                        prerequisiteTasks[i++] = p.UpdateTask;
                    _updateTask = Task.Factory.ContinueWhenAll(prerequisiteTasks, (_) => UpdateAsyncDriver());
                }
                else _updateTask = Task.Factory.StartNew(UpdateAsyncDriver);
                MustRecompute = false;
                return _updateTask;
            }
        }
        
        public void UpdateAsyncDriver() {
            #if PROFILER
            UpdateTime.Start();
            #endif

            switch (UpdateMode) {
                case UpdateMode.BaseTable:
                    UpdateColumns();
                    AppendInputs();
                    break;
                case UpdateMode.Rules:
                    Clear();
                    foreach (var r in Rules!)
                        r.AddAllSolutions();
                    break;
                case UpdateMode.Operator:
                    Clear();
                    updateProc!(TableUntyped);
                    break;
                default:
                    throw new NotImplementedException($"Unknown update mode {UpdateMode}");
            }
            #if PROFILER
            UpdateTime.Start();
            #endif
        }

        #endregion
        #if PROFILER
        internal readonly Stopwatch UpdateTime = new Stopwatch();
        public long TotalExecutionTime => UpdateTime.ElapsedMilliseconds;
        #endif

        /// <summary>
        /// Used to add assertions to the Problems table for the current program.
        /// Usage: predicate.Problem(message).If(conditions...)
        /// </summary>
        public TableGoal Problem(string message) {
            if (Program == null)
                throw new InvalidOperationException(
                    $"Cannot attach a problem rule to {Name} because it does not belong to a Program or Simulation");
            // This is a gross kluge: the preprocessor special-cases the formal argument CaptureDebugStatePrimitive.DebugState
            // and adds a CaptureDebugState call at the end of whatever body you specify for the If() on this goal.
            return Program.Problems[this, message, Primitives.CaptureDebugStatePrimitive.DebugState];
        }
    }

    /// <summary>
    /// A 1-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of the predicate's argument</typeparam>
    public class TablePredicate<T1> : TablePredicate, IEnumerable<T1> {
        /// <summary>
        /// Make a Goal from this predicate with the specified argument value.
        /// </summary>
        public virtual TableGoal<T1> this[Term<T1> arg1] => new TableGoal<T1>(this, arg1);

        /// <inheritdoc />
        public override TableGoal GetGoal(Term[] args) => this[CastArg<T1>(args[0], 1)];

        /// <inheritdoc />
        protected override void AddIndex(int columnIndex, bool keyIndex) {
            throw new NotImplementedException("Single-column tables shouldn't be indexed.  Instead, set the Unique property to true.");
        }

        /// <summary>
        /// Add a rule to the predicate, using its default arguments as the head
        /// </summary>
        /// <param name="body">Antecedents for the rule</param>
        /// <returns>The original predicate (so these can be chained)</returns>
        public TablePredicate<T1> If(params Goal[] body) {
            AddRule(body);
            return this;
        }

        /// <summary>
        /// Add a rule to the predicate, using its default arguments as the head
        /// </summary>
        /// <param name="body">Antecedents for the rule</param>
        /// <returns>The original predicate (so these can be chained)</returns>
        public TablePredicate<T1> Where(params Goal[] body) => If(body);

        /// <summary>
        /// Make a new table predicate with the specified name
        /// </summary>
        /// <param name="name">Name of the predicate</param>
        /// <param name="arg1">Default variable for the first argument</param>
        public TablePredicate(string name, IColumnSpec<T1> arg1) : base(name, null, arg1) { }

        /// <summary>
        /// Make a new table predicate with the specified name
        /// </summary>
        /// <param name="name">Name of the predicate</param>
        /// <param name="updateProc">Procedure to call to update the contents of the table.  This should only be used for tables that are the results of operators</param>
        /// <param name="arg1">Default variable for the first argument</param>
        public TablePredicate(string name, Action<Table> updateProc, IColumnSpec<T1> arg1) : base(name, updateProc, arg1) { }

        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="name">Predicate name</param>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">Default argument to the predicate</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1> FromCsv(string name, string path, IColumnSpec<T1> arg1) {
            (var header, var data) = CsvReader.ReadCsv(path);
            VerifyCsvColumnNames(name, header, arg1);
            var p = new TablePredicate<T1>(name, arg1);
            foreach (var row in data)
                p.AddRow(CsvReader.ConvertCell<T1>(row[0]));
            return p;
        }

        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">Argument to the predicate</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1> FromCsv(string path, IColumnSpec<T1> arg1) => FromCsv(Path.GetFileNameWithoutExtension(path), path, arg1);

        // ReSharper disable once InconsistentNaming
        internal readonly Table<T1> _table = new Table<T1>();

        internal Table<T1> Table  {
            get {
                EnsureUpToDate();
                return _table;
            }
        }

        protected override Table TableUntyped => _table;

        /// <summary>
        /// Number of rows/items in the table/extension of the predicate
        /// </summary>
        public override uint Length => Table.Length;

        /// <summary>
        /// Manually add a row (ground instance) to the extension of the predicate
        /// This cannot be mixed with rules using If.  If you want to have rules for the predicate
        /// use Fact() instead of AddRow.
        /// </summary>
        public void AddRow(in T1 value) => Table.Add(value);

        /// <summary>
        /// Add a set of rows from a generator
        /// </summary>
        public void AddRows(IEnumerable<T1> generator) {
            var t = Table;
            foreach (var r in generator) t.Add(r);
        }

        // Test rig; do not use.
        internal IEnumerable<T1> Match(Pattern<T1> pat) {
            var t = Table;
            for (var i = 0u; i < t.Length; i++) {
                var e = t.PositionReference(i);
                if (pat.Match(e))
                    yield return e;
            }
        }

        /// <summary>
        /// Convert the columns of the specified row to strings and write them to buffer
        /// </summary>
        public override void RowToStrings(uint rowNumber, string[] buffer) {
            var r = Table.PositionReference(rowNumber);
            buffer[0] = Stringify(r);
        }

        /// <summary>
        /// Call method on every row of the table, passing it a reference so it can rewrite it as it likes
        /// </summary>
        /// <param name="u"></param>
        public void UpdateRows(Update<T1> u) {
            for (var i = 0u; i < _table.Length; i++)
                u(ref _table.PositionReference(i));
        }

        /// <summary>
        /// Add rows of t to rows of this predicate
        /// </summary>
        public void Append(TablePredicate<T1> t) {
            for (var i = 0u; i < t._table.Length; i++)
                AddRow(t._table.PositionReference(i));
        }

        private List<TablePredicate<T1>>? inputs;

        /// <inheritdoc />
        public override IEnumerable<TablePredicate> Inputs => (inputs != null)?(IEnumerable<TablePredicate>)inputs:Array.Empty<TablePredicate>();

        /// <summary>
        /// Declare that on each simulation tick, the contents of the specified tables should be appended to this table.
        /// </summary>
        /// <param name="input">Predicate to append to this predicate on each tick</param>
        /// <returns>This predicate</returns>
        public TablePredicate<T1> Accumulates(TablePredicate<T1> input) {
            inputs ??= new List<TablePredicate<T1>>();
            inputs.Add(input);
            return this;
        }

        private TablePredicate<T1>? addPredicate;

        /// <summary>
        /// A predicate that is automatically appended to this predicate on every update.
        /// </summary>
        public TablePredicate<T1> Add {
            get {
                if (addPredicate != null) return addPredicate;
                addPredicate = new TablePredicate<T1>(Name + "__add", (Var<T1>)DefaultVariables[0]);
                Accumulates(addPredicate);
                addPredicate.Property[UpdaterFor] = this;
                addPredicate.Property[VisualizerName] = "Add";
                return addPredicate;
            }
        }

        /// <inheritdoc />
        public override TablePredicate AddUntyped => Add;

        private TablePredicate<T1>? initialValueTable;

        /// <summary>
        /// A predicate that is automatically appended to this predicate on every update.
        /// </summary>
        public TablePredicate<T1> Initially {
            get {
                if (initialValueTable != null) return initialValueTable;
                initialValueTable = new TablePredicate<T1>(Name + "__initially", (Var<T1>)DefaultVariables[0]) {
                    Property = {
                        [UpdaterFor] = this, 
                        [VisualizerName] = "Initially"
                    }
                };
                return initialValueTable;
            }
        }

        /// <inheritdoc />
        public override TablePredicate InitialValueTable => initialValueTable;

        internal override void AddInitialData() {
            if (initialValueTable != null) Append(initialValueTable);
        }

        internal override void AppendInputs() {
            if (inputs == null) return;
            foreach (var input in inputs) Append(input);
        }

        /// <inheritdoc />
        public IEnumerator<T1> GetEnumerator() => Table.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override Delegate Projection(int columnNumber) => columnNumber switch {
            0 => (Table.Projection<T1, T1>)((in T1 row) => row),
            _ => throw new ArgumentException($"There is no column number {columnNumber} in this table")
        };
        public override Comparison<uint> RowComparison(int columnNumber) => columnNumber switch {
            0 => RowComparison<T1,T1>(_table, 0),
            _ => throw new ArgumentException($"There is no column number {columnNumber} in table {Name}")
        };

        public override Func<uint, TColumn> ColumnValueFromRowNumber<TColumn>(Var<TColumn> column) {
            var columnNumber = ColumnPositionOfDefaultVariable(column);
            return columnNumber switch {
                0 => (Func<uint, TColumn>)(Delegate)(Func<uint,T1>)(rowNum => _table.Data[rowNum]),
                _ => throw new ArgumentException($"There is no column number {columnNumber} in table {Name}")
            };
        }
    }

    /// <summary>
    /// A 2-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of the predicate's 1st argument</typeparam>
    /// <typeparam name="T2">Type of the predicate's 2nd argument</typeparam>
    public class TablePredicate<T1, T2> : TablePredicate, IEnumerable<(T1,T2)>
    {
        /// <summary>
        /// Make a Goal from this predicate with the specified argument value.
        /// </summary>
        public virtual TableGoal<T1, T2> this[Term<T1> arg1, Term<T2> arg2] => new TableGoal<T1, T2>(this, arg1, arg2);

        /// <inheritdoc />
        public override TableGoal GetGoal(Term[] args) => this[CastArg<T1>(args[0], 1), CastArg<T2>(args[1], 2)];

        /// <inheritdoc />
        protected override void AddIndex(int columnIndex, bool keyIndex) {
            switch (columnIndex) {
                case 0:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2) r) => r.Item1, keyIndex));
                    break;
                case 1:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2) r) => r.Item2, keyIndex));
                    break;
                default:
                    throw new ArgumentException($"Attempt to add an index for nonexistent column number {columnIndex} to table {Name}");
            }
        }

        /// <summary>
        /// Make a new table predicate with the specified name
        /// </summary>
        /// <param name="name">Name of the predicate</param>
        /// <param name="arg1">Default variable for the first argument</param>
        /// <param name="arg2">Default variable for the second argument</param>
        public TablePredicate(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2) : 
            base(name, null, arg1, arg2) { }

        /// <summary>
        /// Make a new table predicate with the specified name
        /// </summary>
        /// <param name="name">Name of the predicate</param>
        /// <param name="updateProc">Procedure to call to update the contents of the table.  This should only be used for tables that are the results of operators</param>
        /// <param name="arg1">Default variable for the first argument</param>
        /// <param name="arg2">Default variable for the second argument</param>

        public TablePredicate(string name, Action<Table> updateProc, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2) : 
            base(name, updateProc, arg1, arg2) { }

        // ReSharper disable once InconsistentNaming
        internal readonly Table<(T1,T2)> _table = new Table<(T1, T2)>();

        internal Table<(T1, T2)> Table  {
            get {
                EnsureUpToDate();
                return _table;
            }
        }

        protected override Table TableUntyped => _table;

        /// <summary>
        /// The number of rows in the table (i.e. the number of tuples in the extension of the predicate)
        /// </summary>
        public override uint Length => Table.Length;

        /// <summary>
        /// Manually add a row (ground instance) to the extension of the predicate
        /// This cannot be mixed with rules using If.  If you want to have rules for the predicate
        /// use Fact() instead of AddRow.
        /// </summary>
        public void AddRow(in T1 value1, in T2 value2) => Table.Add((value1, value2));

        /// <summary>
        /// Add a set of rows from a generator
        /// </summary>
        public void AddRows(IEnumerable<(T1, T2)> generator) {
            var t = Table;
            foreach (var r in generator) t.Add(r);
        }

        internal IEnumerable<(T1, T2)> Match(Pattern<T1, T2> pat) {
            var t = Table;
            for (var i = 0u; i < t.Length; i++) {
                var e = t.PositionReference(i);
                if (pat.Match(e))
                    yield return e;
            }
        }

        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="name">Predicate name</param>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1, T2> FromCsv(string name, string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2) {
            (var header, var data) = CsvReader.ReadCsv(path);
            VerifyCsvColumnNames(name, header, arg1, arg2);
            var p = new TablePredicate<T1, T2>(name, arg1, arg2);
            foreach (var row in data)
                p.AddRow(CsvReader.ConvertCell<T1>(row[0]), CsvReader.ConvertCell<T2>(row[1]));
            return p;
        }


        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1, T2> FromCsv(string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2) => FromCsv(Path.GetFileNameWithoutExtension(path), path, arg1, arg2);

        /// <summary>
        /// Convert the columns of the specified row to strings and write them to buffer
        /// </summary>
        public override void RowToStrings(uint rowNumber, string[] buffer) {
            var r = Table.PositionReference(rowNumber);
            buffer[0] = Stringify(r.Item1);
            buffer[1] = Stringify(r.Item2);
        }
        
        /// <summary>
        /// Call method on every row of the table, passing it a reference so it can rewrite it as it likes
        /// </summary>
        /// <param name="u"></param>
        public void UpdateRows(Update<(T1, T2)> u) {
            for (var i = 0u; i < _table.Length; i++)
                u(ref _table.PositionReference(i));
        }

        /// <summary>
        /// Add rows of t to rows of this predicate
        /// </summary>
        public void Append(TablePredicate<T1, T2> t) {
            for (var i = 0u; i < t._table.Length; i++) {
                var row = t._table.PositionReference(i);
                AddRow(row.Item1, row.Item2);
            }
        }

        /// <summary>
        /// Add a rule using the default arguments as the head.
        /// </summary>
        /// <param name="body">subgoals</param>
        /// <returns>the original predicate</returns>
        public TablePredicate<T1, T2> If(params Goal[] body) {
            AddRule(body);
            return this;
        }

        /// <summary>
        /// Add a rule to the predicate, using its default arguments as the head
        /// </summary>
        /// <param name="body">Antecedents for the rule</param>
        /// <returns>The original predicate (so these can be chained)</returns>
        public TablePredicate<T1,T2> Where(params Goal[] body) => If(body);
        
        private List<TablePredicate<T1, T2>>? inputs;

        /// <inheritdoc />
        public override IEnumerable<TablePredicate> Inputs => (inputs != null)?(IEnumerable<TablePredicate>)inputs:Array.Empty<TablePredicate>();

        /// <summary>
        /// Declare that on each simulation tick, the contents of the specified tables should be appended to this table.
        /// </summary>
        /// <param name="input">Predicate to append to this predicate on each tick</param>
        /// <returns>This predicate</returns>
        public TablePredicate<T1, T2> Accumulates(TablePredicate<T1, T2> input) {
            inputs ??= new List<TablePredicate<T1, T2>>();
            inputs.Add(input);
            return this;
        }

        private TablePredicate<T1, T2>? addPredicate;

        /// <summary>
        /// A predicate that is automatically appended to this predicate on every update.
        /// </summary>
        public TablePredicate<T1, T2> Add {
            get {
                if (addPredicate != null) return addPredicate;
                addPredicate = new TablePredicate<T1, T2>(Name + "__add", (Var<T1>)DefaultVariables[0], (Var<T2>)DefaultVariables[1]);
                Accumulates(addPredicate);
                addPredicate.Property[UpdaterFor] = this;
                addPredicate.Property[VisualizerName] = "Add";
                return addPredicate;
            }
        }

        /// <inheritdoc />
        public override TablePredicate AddUntyped => Add;

        private TablePredicate<T1,T2>? initialValueTable;

        /// <summary>
        /// A predicate that is automatically appended to this predicate on every update.
        /// </summary>
        public TablePredicate<T1,T2> Initially {
            get {
                if (initialValueTable != null) return initialValueTable;
                initialValueTable = new TablePredicate<T1,T2>(Name + "__initially", (Var<T1>)DefaultVariables[0], (Var<T2>)DefaultVariables[1]) {
                    Property = {
                        [UpdaterFor] = this, 
                        [VisualizerName] = "Initially"
                    }
                };
                return initialValueTable;
            }
        }
        
        /// <inheritdoc />
        public override TablePredicate InitialValueTable => initialValueTable;

        internal override void AddInitialData() {
            if (initialValueTable != null) Append(initialValueTable);
        }

        internal override void AppendInputs() {
            if (inputs == null) return;
            foreach (var input in inputs) Append(input);
        }

        /// <summary>
        /// Get the index for the specified key
        /// </summary>
        /// <typeparam name="T">Type of the indexed column</typeparam>
        /// <param name="column">Default variable representing the column</param>
        /// <returns>The index</returns>
        /// <exception cref="InvalidOperationException">If there is no index or it's not a key index</exception>
        public KeyIndex<(T1, T2), T> KeyIndex<T>(Var<T> column) {
            var i = IndexFor(ColumnPositionOfDefaultVariable(column), true);
            return i == null ? throw new InvalidOperationException($"No key index defined for {column}")
                       : (KeyIndex<(T1, T2), T>)i;
        }

        /// <summary>
        /// Get the index for the specified key
        /// </summary>
        /// <typeparam name="T">Type of the indexed column</typeparam>
        /// <param name="column">Position of the column</param>
        /// <returns>The index</returns>
        /// <exception cref="InvalidOperationException">If there is no index or it's not a key index</exception>
        public KeyIndex<(T1, T2), T> KeyIndex<T>(int column) {
            var i = IndexFor(column, true);
            return i == null ? throw new InvalidOperationException($"No key index defined for column {column}")
                       : (KeyIndex<(T1, T2), T>)i;
        }

        /// <inheritdoc />
        public IEnumerator<(T1, T2)> GetEnumerator() => Table.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #region Projection and mutation
        /// <inheritdoc />
        public override Delegate Projection(int columnNumber)
        {
            return columnNumber switch
            {
                0 => (Table.Projection<(T1,T2), T1>)((in (T1,T2) row) => row.Item1),
                1 => (Table.Projection<(T1,T2), T2>)((in (T1,T2) row) => row.Item2),
                _ => throw new ArgumentException($"There is no column number {columnNumber} in this table")
            };
        }

        /// <inheritdoc />
        public override Delegate Mutator(int columnNumber)
        {
            return columnNumber switch
            {
                0 => (Table.Mutator<(T1,T2), T1>)((ref (T1,T2) row, in T1 value) => row.Item1 = value),
                1 => (Table.Mutator<(T1,T2), T2>)((ref (T1,T2) row, in T2 value) => row.Item2 = value),
                _ => throw new ArgumentException($"There is no column number {columnNumber} in this table")
            };
        }
        #endregion

        /// <inheritdoc />
        public override ColumnAccessor<TColumn, TKey> Accessor<TColumn, TKey>(Var<TKey> key, Var<TColumn> column)
            => Accessor(_table, key, column);

        public override Comparison<uint> RowComparison(int columnNumber) => columnNumber switch {
            0 => RowComparison<(T1,T2),T1>(_table, columnNumber),
            1 => RowComparison<(T1,T2),T2>(_table, columnNumber),
            _ => throw new ArgumentException($"There is no column number {columnNumber} in table {Name}")
        };

        public override Func<uint, TColumn> ColumnValueFromRowNumber<TColumn>(Var<TColumn> column) {
            var columnNumber = ColumnPositionOfDefaultVariable(column);
            return columnNumber switch {
                0 => (Func<uint, TColumn>)(Delegate)(Func<uint,T1>)(rowNum => _table.Data[rowNum].Item1),
                1 => (Func<uint, TColumn>)(Delegate)(Func<uint,T2>)(rowNum => _table.Data[rowNum].Item2),
                _ => throw new ArgumentException($"There is no column number {columnNumber} in table {Name}")
            };
        }
    }


    /// <summary>
    /// A 3-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of the predicate's 1st argument</typeparam>
    /// <typeparam name="T2">Type of the predicate's 2nd argument</typeparam>
    /// <typeparam name="T3">Type of the predicate's 3rd argument</typeparam>
    public class TablePredicate<T1, T2, T3> : TablePredicate, IEnumerable<(T1,T2,T3)> {
        /// <summary>
        /// Make a Goal from this predicate with the specified argument value.
        /// </summary>
        public virtual TableGoal<T1, T2, T3> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3] => new TableGoal<T1, T2, T3>(this, arg1, arg2,arg3);

        /// <inheritdoc />
        public override TableGoal GetGoal(Term[] args) 
            => this[CastArg<T1>(args[0], 1), CastArg<T2>(args[1], 2), CastArg<T3>(args[2], 3)];

        /// <inheritdoc />
        protected override void AddIndex(int columnIndex, bool keyIndex) {
            switch (columnIndex) {
                case 0:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3) r) => r.Item1, keyIndex));
                    break;
                case 1:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3) r) => r.Item2, keyIndex));
                    break;
                case 2:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3) r) => r.Item3, keyIndex));
                    break;
                default:
                    throw new ArgumentException($"Attempt to add an index for nonexistent column number {columnIndex} to table {Name}");
            }
        }

        /// <summary>
        /// Get the index for the specified key
        /// </summary>
        /// <typeparam name="T">Type of the indexed column</typeparam>
        /// <param name="column">Default variable representing the column</param>
        /// <returns>The index</returns>
        /// <exception cref="InvalidOperationException">If there is no index or it's not a key index</exception>
        public KeyIndex<(T1, T2, T3), T> KeyIndex<T>(Var<T> column) {
            var i = IndexFor(ColumnPositionOfDefaultVariable(column), true);
            return i == null ? throw new InvalidOperationException($"No key index defined for {column}")
                       : (KeyIndex<(T1, T2, T3), T>)i;
        }

        /// <summary>
        /// Get the index for the specified key
        /// </summary>
        /// <typeparam name="T">Type of the indexed column</typeparam>
        /// <param name="column">Position of column</param>
        /// <returns>The index</returns>
        /// <exception cref="InvalidOperationException">If there is no index or it's not a key index</exception>
        public KeyIndex<(T1, T2, T3), T> KeyIndex<T>(int column) {
            var i = IndexFor(column, true);
            return i == null ? throw new InvalidOperationException($"No key index defined for column {column}")
                       : (KeyIndex<(T1, T2, T3), T>)i;
        }

        /// <summary>
        /// Make a new table predicate with the specified name
        /// </summary>
        /// <param name="name">Name of the predicate</param>
        /// <param name="arg1">Default variable for the first argument</param>
        /// <param name="arg2">Default variable for the second argument</param>
        /// <param name="arg3">Default variable for the third argument</param>

        public TablePredicate(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3) 
            : base(name, null, arg1, arg2, arg3) { }

        /// <summary>
        /// Make a new table predicate with the specified name
        /// </summary>
        /// <param name="name">Name of the predicate</param>
        /// <param name="updateProc">Procedure to call to update the contents of the table.  This should only be used for tables that are the results of operators</param>
        /// <param name="arg1">Default variable for the first argument</param>
        /// <param name="arg2">Default variable for the second argument</param>
        /// <param name="arg3">Default variable for the third argument</param>

        public TablePredicate(string name, Action<Table> updateProc, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3)
            : base(name, updateProc, arg1, arg2, arg3) { }
        
        // ReSharper disable once InconsistentNaming
        internal readonly Table<(T1,T2, T3)> _table = new Table<(T1, T2, T3)>();

        internal Table<(T1, T2, T3)> Table  {
            get {
                EnsureUpToDate();
                return _table;
            }
        }

        protected override Table TableUntyped => _table;
        
        /// <summary>
        /// The number of rows in the table (i.e. the number of tuples in the extension of the predicate)
        /// </summary>
        public override uint Length => Table.Length;

        /// <summary>
        /// Manually add a row (ground instance) to the extension of the predicate
        /// This cannot be mixed with rules using If.  If you want to have rules for the predicate
        /// use Fact() instead of AddRow.
        /// </summary>
        public void AddRow(in T1 v1, in T2 v2, in T3 v3) => _table.Add((v1, v2, v3));

        /// <summary>
        /// Add a set of rows from a generator
        /// </summary>
        public void AddRows(IEnumerable<(T1, T2, T3)> generator) {
            var t = Table;
            foreach (var r in generator) t.Add(r);
        }

        internal IEnumerable<(T1, T2, T3)> Match(Pattern<T1, T2, T3> pat) {
            var t = Table;
            for (var i = 0u; i < t.Length; i++) {
                var e = t.PositionReference(i);
                if (pat.Match(e))
                    yield return e;
            }
        }

        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="name">Predicate name</param>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1, T2, T3> FromCsv(string name, string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3) {
            (var header, var data) = CsvReader.ReadCsv(path);
            VerifyCsvColumnNames(name, header, arg1, arg2, arg3);
            var p = new TablePredicate<T1, T2, T3>(name, arg1, arg2, arg3);
            foreach (var row in data)
                p.AddRow(CsvReader.ConvertCell<T1>(row[0]), CsvReader.ConvertCell<T2>(row[1]), CsvReader.ConvertCell<T3>(row[2]));
            return p;
        }

        
        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1, T2, T3> FromCsv(string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3) => FromCsv(Path.GetFileNameWithoutExtension(path), path, arg1, arg2, arg3);

        /// <summary>
        /// Convert the columns of the specified row to strings and write them to buffer
        /// </summary>
        public override void RowToStrings(uint rowNumber, string[] buffer) {
            var r = Table.PositionReference(rowNumber);
            buffer[0] = Stringify(r.Item1);
            buffer[1] = Stringify(r.Item2);
            buffer[2] = Stringify(r.Item3);
        }

        /// <summary>
        /// Call method on every row of the table, passing it a reference so it can rewrite it as it likes
        /// </summary>
        /// <param name="u"></param>
        public void UpdateRows(Update<(T1, T2, T3)> u) {
            for (var i = 0u; i < _table.Length; i++)
                u(ref _table.PositionReference(i));
        }

        /// <summary>
        /// Add rows of t to rows of this predicate
        /// </summary>
        public void Append(TablePredicate<T1, T2, T3> t) {
            for (var i = 0u; i < t._table.Length; i++) {
                var row = t._table.PositionReference(i);
                AddRow(row.Item1, row.Item2, row.Item3);
            }
        }

        /// <summary>
        /// Add a rule using the default arguments as the head.
        /// </summary>
        /// <param name="body">subgoals</param>
        /// <returns>the original predicate</returns>
        public TablePredicate<T1, T2, T3> If(params Goal[] body) {
            AddRule(body);
            return this;
        }

        /// <summary>
        /// Add a rule to the predicate, using its default arguments as the head
        /// </summary>
        /// <param name="body">Antecedents for the rule</param>
        /// <returns>The original predicate (so these can be chained)</returns>
        public TablePredicate<T1,T2,T3> Where(params Goal[] body) => If(body);

        private List<TablePredicate<T1, T2, T3>>? inputs;

        /// <inheritdoc />
        public override IEnumerable<TablePredicate> Inputs => (inputs != null)?(IEnumerable<TablePredicate>)inputs:Array.Empty<TablePredicate>();

        /// <summary>
        /// Declare that on each simulation tick, the contents of the specified tables should be appended to this table.
        /// </summary>
        /// <param name="input">Predicate to append to this predicate on each tick</param>
        /// <returns>This predicate</returns>
        public TablePredicate<T1, T2, T3> Accumulates(TablePredicate<T1, T2, T3> input) {
            inputs ??= new List<TablePredicate<T1, T2, T3>>();
            inputs.Add(input);
            return this;
        }

        private TablePredicate<T1, T2, T3>? addPredicate;

        /// <summary>
        /// A predicate that is automatically appended to this predicate on every update.
        /// </summary>
        public TablePredicate<T1, T2, T3> Add {
            get {
                if (addPredicate != null) return addPredicate;
                addPredicate = new TablePredicate<T1, T2, T3>(Name + "__add",
                                                              (Var<T1>)DefaultVariables[0],
                                                              (Var<T2>)DefaultVariables[1],
                                                              (Var<T3>)DefaultVariables[2]);
                Accumulates(addPredicate);
                addPredicate.Property[UpdaterFor] = this;
                addPredicate.Property[VisualizerName] = "Add";
                return addPredicate;
            }
        }

        /// <inheritdoc />
        public override TablePredicate AddUntyped => Add;

        private TablePredicate<T1,T2,T3>? initialValueTable;

        /// <summary>
        /// A predicate that is automatically appended to this predicate on every update.
        /// </summary>
        public TablePredicate<T1,T2,T3> Initially {
            get {
                if (initialValueTable != null) return initialValueTable;
                initialValueTable = new TablePredicate<T1,T2,T3>(Name + "__initially", 
                                                                 (Var<T1>)DefaultVariables[0], 
                                                                 (Var<T2>)DefaultVariables[1], 
                                                                 (Var<T3>)DefaultVariables[2]) {
                    Property = {
                        [UpdaterFor] = this, 
                        [VisualizerName] = "Initially"
                    }
                };
                return initialValueTable;
            }
        }
        
        /// <inheritdoc />
        public override TablePredicate InitialValueTable => initialValueTable;
        
        internal override void AddInitialData() {
            if (initialValueTable != null) Append(initialValueTable);
        }

        internal override void AppendInputs() {
            if (inputs == null) return;
            foreach (var input in inputs) Append(input);
        }

        /// <inheritdoc />
        public IEnumerator<(T1, T2, T3)> GetEnumerator() => Table.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #region Projection and mutation
        /// <inheritdoc />
        public override Delegate Projection(int columnNumber)
        {
            return columnNumber switch
            {
                0 => (Table.Projection<(T1,T2,T3), T1>)((in (T1,T2,T3) row) => row.Item1),
                1 => (Table.Projection<(T1,T2,T3), T2>)((in (T1,T2,T3) row) => row.Item2),
                2 => (Table.Projection<(T1,T2,T3), T3>)((in (T1,T2,T3) row) => row.Item3),
                _ => throw new ArgumentException($"There is no column number {columnNumber} in this table")
            };
        }

        /// <inheritdoc />
        public override Delegate Mutator(int columnNumber)
        {
            return columnNumber switch
            {
                0 => (Table.Mutator<(T1,T2,T3), T1>)((ref (T1,T2,T3) row, in T1 value) => row.Item1 = value),
                1 => (Table.Mutator<(T1,T2,T3), T2>)((ref (T1,T2,T3) row, in T2 value) => row.Item2 = value),
                2 => (Table.Mutator<(T1,T2,T3), T3>)((ref (T1,T2,T3) row, in T3 value) => row.Item3 = value),
                _ => throw new ArgumentException($"There is no column number {columnNumber} in this table")
            };
        }
        #endregion

        /// <inheritdoc />
        public override ColumnAccessor<TColumn, TKey> Accessor<TColumn, TKey>(Var<TKey> key, Var<TColumn> column)
            => Accessor(_table, key, column);

        public override Comparison<uint> RowComparison(int columnNumber) => columnNumber switch {
            0 => RowComparison<(T1,T2,T3),T1>(_table, columnNumber),
            1 => RowComparison<(T1,T2,T3),T2>(_table, columnNumber),
            2 => RowComparison<(T1,T2,T3),T3>(_table, columnNumber),
            _ => throw new ArgumentException($"There is no column number {columnNumber} in table {Name}")
        };

        public override Func<uint, TColumn> ColumnValueFromRowNumber<TColumn>(Var<TColumn> column) {
            var columnNumber = ColumnPositionOfDefaultVariable(column);
            return columnNumber switch {
                0 => (Func<uint, TColumn>)(Delegate)(Func<uint,T1>)(rowNum => _table.Data[rowNum].Item1),
                1 => (Func<uint, TColumn>)(Delegate)(Func<uint,T2>)(rowNum => _table.Data[rowNum].Item2),
                2 => (Func<uint, TColumn>)(Delegate)(Func<uint,T3>)(rowNum => _table.Data[rowNum].Item3),
                _ => throw new ArgumentException($"There is no column number {columnNumber} in table {Name}")
            };
        }
    }


    /// <summary>
    /// A 4-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of the predicate's 1st argument</typeparam>
    /// <typeparam name="T2">Type of the predicate's 2nd argument</typeparam>
    /// <typeparam name="T3">Type of the predicate's 3rd argument</typeparam>
    /// <typeparam name="T4">Type of the predicate's 4th argument</typeparam>
    public class TablePredicate<T1, T2, T3, T4> : TablePredicate, IEnumerable<(T1,T2,T3,T4)> {
        /// <summary>
        /// Make a Goal from this predicate with the specified argument value.
        /// </summary>
        public virtual TableGoal<T1, T2, T3, T4> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4] 
            => new TableGoal<T1, T2, T3, T4>(this, arg1, arg2,arg3, arg4);

        /// <inheritdoc />
        public override TableGoal GetGoal(Term[] args)
            => this[CastArg<T1>(args[0], 1), CastArg<T2>(args[1], 2), CastArg<T3>(args[2], 3), CastArg<T4>(args[3], 4)];

        /// <inheritdoc />
        protected override void AddIndex(int columnIndex, bool keyIndex) {
            switch (columnIndex) {
                case 0:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4) r) => r.Item1, keyIndex));
                    break;
                case 1:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4) r) => r.Item2, keyIndex));
                    break;
                case 2:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4) r) => r.Item3, keyIndex));
                    break;
                case 3:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4) r) => r.Item4, keyIndex));
                    break;
                default:
                    throw new ArgumentException($"Attempt to add an index for nonexistent column number {columnIndex} to table {Name}");
            }
        }

        /// <summary>
        /// Get the index for the specified key
        /// </summary>
        /// <typeparam name="TKey">Type of the indexed column</typeparam>
        /// <param name="column">Default variable representing the column</param>
        /// <returns>The index</returns>
        /// <exception cref="InvalidOperationException">If there is no index or it's not a key index</exception>
        public KeyIndex<(T1, T2, T3, T4), TKey> KeyIndex<TKey>(Var<TKey> column) {
            var i = IndexFor(ColumnPositionOfDefaultVariable(column), true);
            return i == null ? throw new InvalidOperationException($"No key index defined for {column}")
                       : (KeyIndex<(T1, T2, T3, T4), TKey>)i;
        }
        
        /// <summary>
        /// Get the index for the specified key
        /// </summary>
        /// <typeparam name="TKey">Type of the indexed column</typeparam>
        /// <param name="column">Position the column</param>
        /// <returns>The index</returns>
        /// <exception cref="InvalidOperationException">If there is no index or it's not a key index</exception>
        public KeyIndex<(T1, T2, T3, T4), TKey> KeyIndex<TKey>(int column) {
            var i = IndexFor(column, true);
            return i == null ? throw new InvalidOperationException($"No key index defined for column {column}")
                       : (KeyIndex<(T1, T2, T3, T4), TKey>)i;
        }
        
        /// <summary>
        /// Make a new table predicate with the specified name
        /// </summary>
        /// <param name="name">Name of the predicate</param>
        /// <param name="arg1">Default variable for the first argument</param>
        /// <param name="arg2">Default variable for the second argument</param>
        /// <param name="arg3">Default variable for the third argument</param>
        /// <param name="arg4">Default variable for the fourth argument</param>
        public TablePredicate(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, 
                              IColumnSpec<T3> arg3, IColumnSpec<T4> arg4)
            : base(name, null, arg1, arg2, arg3, arg4) { }

        /// <summary>
        /// Make a new table predicate with the specified name
        /// </summary>
        /// <param name="name">Name of the predicate</param>
        /// <param name="updateProc">Procedure to call to update the contents of the table.  This should only be used for tables that are the results of operators</param>
        /// <param name="arg1">Default variable for the first argument</param>
        /// <param name="arg2">Default variable for the second argument</param>
        /// <param name="arg3">Default variable for the third argument</param>
        /// <param name="arg4">Default variable for the fourth argument</param>

        public TablePredicate(string name, Action<Table> updateProc, IColumnSpec<T1> arg1, 
                              IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4)
            : base(name, updateProc, arg1, arg2, arg3, arg4) { }
        
        // ReSharper disable once InconsistentNaming
        internal readonly Table<(T1,T2, T3, T4)> _table = new Table<(T1, T2, T3, T4)>();

        internal Table<(T1, T2, T3, T4)> Table  {
            get {
                EnsureUpToDate();
                return _table;
            }
        }

        protected override Table TableUntyped => _table;

        /// <summary>
        /// The number of rows in the table (i.e. the number of tuples in the extension of the predicate)
        /// </summary>
        public override uint Length => Table.Length;

        /// <summary>
        /// Manually add a row (ground instance) to the extension of the predicate
        /// This cannot be mixed with rules using If.  If you want to have rules for the predicate
        /// use Fact() instead of AddRow.
        /// </summary>
        public void AddRow(in T1 v1, in T2 v2, in T3 v3, in T4 v4)
            => _table.Add((v1, v2, v3, v4));

        /// <summary>
        /// Add a set of rows from a generator
        /// </summary>
        public void AddRows(IEnumerable<(T1, T2, T3, T4)> generator) {
            var t = Table;
            foreach (var r in generator) t.Add(r);
        }

        internal IEnumerable<(T1, T2, T3, T4)> Match(Pattern<T1, T2, T3, T4> pat) {
            var t = Table;
            for (var i = 0u; i < t.Length; i++) {
                var e = t.PositionReference(i);
                if (pat.Match(e))
                    yield return e;
            }
        }

        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="name">Predicate name</param>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        /// <param name="arg4">Fourth argument</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1, T2, T3, T4> FromCsv(string name, string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4) {
            (var header, var data) = CsvReader.ReadCsv(path);
            VerifyCsvColumnNames(name, header, arg1, arg2, arg3, arg4);
            var p = new TablePredicate<T1, T2, T3, T4>(name, arg1, arg2, arg3, arg4);
            foreach (var row in data)
                p.AddRow(CsvReader.ConvertCell<T1>(row[0]), CsvReader.ConvertCell<T2>(row[1]), CsvReader.ConvertCell<T3>(row[2]), CsvReader.ConvertCell<T4>(row[3]));
            return p;
        }

        
        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        /// <param name="arg4">Fourth argument</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1, T2, T3, T4> FromCsv(string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4) => FromCsv(Path.GetFileNameWithoutExtension(path), path, arg1, arg2, arg3, arg4);

        /// <summary>
        /// Convert the columns of the specified row to strings and write them to buffer
        /// </summary>
        public override void RowToStrings(uint rowNumber, string[] buffer) {
            var r = Table.PositionReference(rowNumber);
            buffer[0] = Stringify(r.Item1);
            buffer[1] = Stringify(r.Item2);
            buffer[2] = Stringify(r.Item3);
            buffer[3] = Stringify(r.Item4);
        }

        /// <summary>
        /// Call method on every row of the table, passing it a reference so it can rewrite it as it likes
        /// </summary>
        /// <param name="u"></param>
        public void UpdateRows(Update<(T1, T2, T3, T4)> u) {
            for (var i = 0u; i < _table.Length; i++)
                u(ref _table.PositionReference(i));
        }

        /// <summary>
        /// Add rows of t to rows of this predicate
        /// </summary>
        public void Append(TablePredicate<T1, T2, T3, T4> t) {
            for (var i = 0u; i < t._table.Length; i++) {
                var row = t._table.PositionReference(i);
                AddRow(row.Item1, row.Item2, row.Item3, row.Item4);
            }
        }

        /// <summary>
        /// Add a rule using the default arguments as the head.
        /// </summary>
        /// <param name="body">subgoals</param>
        /// <returns>the original predicate</returns>
        public TablePredicate<T1, T2, T3, T4> If(params Goal[] body) {
            AddRule(body);
            return this;
        }

        /// <summary>
        /// Add a rule to the predicate, using its default arguments as the head
        /// </summary>
        /// <param name="body">Antecedents for the rule</param>
        /// <returns>The original predicate (so these can be chained)</returns>
        public TablePredicate<T1,T2,T3,T4> Where(params Goal[] body) => If(body);


        private List<TablePredicate<T1, T2, T3, T4>>? inputs;

        /// <inheritdoc />
        public override IEnumerable<TablePredicate> Inputs => (inputs != null)?(IEnumerable<TablePredicate>)inputs:Array.Empty<TablePredicate>();

        /// <summary>
        /// Declare that on each simulation tick, the contents of the specified tables should be appended to this table.
        /// </summary>
        /// <param name="input">Predicate to append to this predicate on each tick</param>
        /// <returns>This predicate</returns>
        public TablePredicate<T1, T2, T3, T4> Accumulates(TablePredicate<T1, T2, T3, T4> input) {
            inputs ??= new List<TablePredicate<T1, T2, T3, T4>>();
            inputs.Add(input);
            return this;
        }

        private TablePredicate<T1, T2, T3, T4>? addPredicate;

        /// <summary>
        /// A predicate that is automatically appended to this predicate on every update.
        /// </summary>
        public TablePredicate<T1, T2, T3, T4> Add {
            get {
                if (addPredicate != null) return addPredicate;
                addPredicate = new TablePredicate<T1, T2, T3, T4>(Name + "__add",
                                                                  (Var<T1>)DefaultVariables[0],
                                                                  (Var<T2>)DefaultVariables[1],
                                                                  (Var<T3>)DefaultVariables[2],
                                                                  (Var<T4>)DefaultVariables[3]);
                Accumulates(addPredicate);
                addPredicate.Property[UpdaterFor] = this;
                addPredicate.Property[VisualizerName] = "Add";
                return addPredicate;
            }
        }

        /// <inheritdoc />
        public override TablePredicate AddUntyped => Add;

        private TablePredicate<T1,T2,T3,T4>? initialValueTable;

        /// <summary>
        /// A predicate that is automatically appended to this predicate on every update.
        /// </summary>
        public TablePredicate<T1,T2,T3,T4> Initially {
            get {
                if (initialValueTable != null) return initialValueTable;
                initialValueTable = new TablePredicate<T1,T2,T3,T4>(Name + "__initially", 
                                                                    (Var<T1>)DefaultVariables[0], 
                                                                    (Var<T2>)DefaultVariables[1], 
                                                                    (Var<T3>)DefaultVariables[2], 
                                                                    (Var<T4>)DefaultVariables[3]) {
                    Property = {
                        [UpdaterFor] = this,
                        [VisualizerName] = "Initially"
                    }
                };
                return initialValueTable;
            }
        }
        
        /// <inheritdoc />
        public override TablePredicate InitialValueTable => initialValueTable;

        internal override void AddInitialData() {
            if (initialValueTable != null) Append(initialValueTable);
        }

        internal override void AppendInputs() {
            if (inputs == null) return;
            foreach (var input in inputs) Append(input);
        }

        /// <inheritdoc />
        public IEnumerator<(T1, T2, T3, T4)> GetEnumerator() => Table.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #region Projections and mutators
        /// <inheritdoc />
        public override Delegate Projection(int columnNumber)
        {
            return columnNumber switch
            {
                0 => (Table.Projection<(T1,T2,T3,T4), T1>)((in (T1,T2,T3,T4) row) => row.Item1),
                1 => (Table.Projection<(T1,T2,T3,T4), T2>)((in (T1,T2,T3,T4) row) => row.Item2),
                2 => (Table.Projection<(T1,T2,T3,T4), T3>)((in (T1,T2,T3,T4) row) => row.Item3),
                3 => (Table.Projection<(T1,T2,T3,T4), T4>)((in (T1,T2,T3,T4) row) => row.Item4),
                _ => throw new ArgumentException($"There is no column number {columnNumber} in this table")
            };
        }

        /// <inheritdoc />
        public override Delegate Mutator(int columnNumber)
        {
            return columnNumber switch
            {
                0 => (Table.Mutator<(T1,T2,T3,T4), T1>)((ref (T1,T2,T3,T4) row, in T1 value) => row.Item1 = value),
                1 => (Table.Mutator<(T1,T2,T3,T4), T2>)((ref (T1,T2,T3,T4) row, in T2 value) => row.Item2 = value),
                2 => (Table.Mutator<(T1,T2,T3,T4), T3>)((ref (T1,T2,T3,T4) row, in T3 value) => row.Item3 = value),
                3 => (Table.Mutator<(T1,T2,T3,T4), T4>)((ref (T1,T2,T3,T4) row, in T4 value) => row.Item4 = value),
                _ => throw new ArgumentException($"There is no column number {columnNumber} in this table")
            };
        }
        #endregion

        /// <inheritdoc />
        public override ColumnAccessor<TColumn, TKey> Accessor<TColumn, TKey>(Var<TKey> key, Var<TColumn> column)
            => Accessor(_table, key, column);

        public override Comparison<uint> RowComparison(int columnNumber) => columnNumber switch {
            0 => RowComparison<(T1,T2,T3,T4),T1>(_table, columnNumber),
            1 => RowComparison<(T1,T2,T3,T4),T2>(_table, columnNumber),
            2 => RowComparison<(T1,T2,T3,T4),T3>(_table, columnNumber),
            3 => RowComparison<(T1,T2,T3,T4),T4>(_table, columnNumber),
            _ => throw new ArgumentException($"There is no column number {columnNumber} in table {Name}")
        };

        public override Func<uint, TColumn> ColumnValueFromRowNumber<TColumn>(Var<TColumn> column) {
            var columnNumber = ColumnPositionOfDefaultVariable(column);
            return columnNumber switch {
                0 => (Func<uint, TColumn>)(Delegate)(Func<uint,T1>)(rowNum => _table.Data[rowNum].Item1),
                1 => (Func<uint, TColumn>)(Delegate)(Func<uint,T2>)(rowNum => _table.Data[rowNum].Item2),
                2 => (Func<uint, TColumn>)(Delegate)(Func<uint,T3>)(rowNum => _table.Data[rowNum].Item3),
                3 => (Func<uint, TColumn>)(Delegate)(Func<uint,T4>)(rowNum => _table.Data[rowNum].Item4),
                _ => throw new ArgumentException($"There is no column number {columnNumber} in table {Name}")
            };
        }
    }


    /// <summary>
    /// A 5-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of the predicate's 1st argument</typeparam>
    /// <typeparam name="T2">Type of the predicate's 2nd argument</typeparam>
    /// <typeparam name="T3">Type of the predicate's 3rd argument</typeparam>
    /// <typeparam name="T4">Type of the predicate's 4th argument</typeparam>
    /// <typeparam name="T5">Type of the predicate's 5th argument</typeparam>
    public class TablePredicate<T1, T2, T3, T4, T5> : TablePredicate, IEnumerable<(T1,T2,T3,T4,T5)> {
        /// <summary>
        /// Make a Goal from this predicate with the specified argument value.
        /// </summary>
        public virtual TableGoal<T1, T2, T3, T4, T5> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5] 
            => new TableGoal<T1, T2, T3, T4, T5>(this, arg1, arg2,arg3, arg4, arg5);

        /// <inheritdoc />
        public override TableGoal GetGoal(Term[] args)
            => this[CastArg<T1>(args[0], 1), CastArg<T2>(args[1], 2), CastArg<T3>(args[2], 3), CastArg<T4>(args[3], 4), CastArg<T5>(args[4], 5)];

        /// <inheritdoc />
        protected override void AddIndex(int columnIndex, bool keyIndex) {
            switch (columnIndex) {
                case 0:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5) r) => r.Item1, keyIndex));
                    break;
                case 1:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5) r) => r.Item2, keyIndex));
                    break;
                case 2:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5) r) => r.Item3, keyIndex));
                    break;
                case 3:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5) r) => r.Item4, keyIndex));
                    break;
                case 4:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5) r) => r.Item5, keyIndex));
                    break;
                default:
                    throw new ArgumentException($"Attempt to add an index for nonexistent column number {columnIndex} to table {Name}");
            }
        }

        /// <summary>
        /// Get the index for the specified key
        /// </summary>
        /// <typeparam name="TKey">Type of the indexed column</typeparam>
        /// <param name="column">Default variable representing the column</param>
        /// <returns>The index</returns>
        /// <exception cref="InvalidOperationException">If there is no index or it's not a key index</exception>
        public KeyIndex<(T1, T2, T3, T4, T5), TKey> KeyIndex<TKey>(Var<TKey> column) {
            var i = IndexFor(ColumnPositionOfDefaultVariable(column), true);
            return i == null ? throw new InvalidOperationException($"No key index defined for {column}")
                       : (KeyIndex<(T1, T2, T3, T4, T5), TKey>)i;
        }
        
        /// <summary>
        /// Get the index for the specified key
        /// </summary>
        /// <typeparam name="TKey">Type of the indexed column</typeparam>
        /// <param name="column">Position of the column</param>
        /// <returns>The index</returns>
        /// <exception cref="InvalidOperationException">If there is no index or it's not a key index</exception>
        public KeyIndex<(T1, T2, T3, T4, T5), TKey> KeyIndex<TKey>(int column) {
            var i = IndexFor(column, true);
            return i == null ? throw new InvalidOperationException($"No key index defined for column {column}")
                       : (KeyIndex<(T1, T2, T3, T4, T5), TKey>)i;
        }
        
        /// <summary>
        /// Make a new table predicate with the specified name
        /// </summary>
        /// <param name="name">Name of the predicate</param>
        /// <param name="arg1">Default variable for the first argument</param>
        /// <param name="arg2">Default variable for the second argument</param>
        /// <param name="arg3">Default variable for the third argument</param>
        /// <param name="arg4">Default variable for the fourth argument</param>
        /// <param name="arg5">Default variable for the fifth argument</param>
        public TablePredicate(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, 
                              IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5) 
            : base(name, null, arg1, arg2, arg3, arg4, arg5) { }

        /// <summary>
        /// Make a new table predicate with the specified name
        /// </summary>
        /// <param name="name">Name of the predicate</param>
        /// <param name="updateProc">Procedure to call to update the contents of the table.  This should only be used for tables that are the results of operators</param>
        /// <param name="arg1">Default variable for the first argument</param>
        /// <param name="arg2">Default variable for the second argument</param>
        /// <param name="arg3">Default variable for the third argument</param>
        /// <param name="arg4">Default variable for the fourth argument</param>
        /// <param name="arg5">Default variable for the fifth argument</param>
        public TablePredicate(string name, Action<Table> updateProc, IColumnSpec<T1> arg1, 
                              IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, 
                              IColumnSpec<T5> arg5)
            : base(name, updateProc, arg1, arg2, arg3, arg4, arg5) { }

        // ReSharper disable once InconsistentNaming
        internal readonly Table<(T1,T2, T3, T4, T5)> _table = new Table<(T1, T2, T3, T4, T5)>();

        internal Table<(T1, T2, T3, T4, T5)> Table  {
            get {
                EnsureUpToDate();
                return _table;
            }
        }

        protected override Table TableUntyped => _table;
        
        /// <summary>
        /// The number of rows in the table (i.e. the number of tuples in the extension of the predicate)
        /// </summary>
        public override uint Length => Table.Length;

        /// <summary>
        /// Manually add a row (ground instance) to the extension of the predicate
        /// This cannot be mixed with rules using If.  If you want to have rules for the predicate
        /// use Fact() instead of AddRow.
        /// </summary>
        public void AddRow(in T1 v1, in T2 v2, in T3 v3, in T4 v4, in T5 v5)
            => _table.Add((v1, v2, v3, v4, v5));

        /// <summary>
        /// Add a set of rows from a generator
        /// </summary>
        public void AddRows(IEnumerable<(T1, T2, T3, T4, T5)> generator) {
            var t = Table;
            foreach (var r in generator) t.Add(r);
        }

        internal IEnumerable<(T1, T2, T3, T4, T5)> Match(Pattern<T1, T2, T3, T4, T5> pat) {
            var t = Table;
            for (var i = 0u; i < t.Length; i++) {
                var e = t.PositionReference(i);
                if (pat.Match(e))
                    yield return e;
            }
        }

        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="name">Predicate name</param>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        /// <param name="arg4">Fourth argument</param>
        /// <param name="arg5">Fifth argument</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1, T2, T3, T4, T5> FromCsv(string name, string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5) {
            (var header, var data) = CsvReader.ReadCsv(path);
            VerifyCsvColumnNames(name, header, arg1, arg2, arg3, arg4, arg5);
            var p = new TablePredicate<T1, T2, T3, T4, T5>(name, arg1, arg2, arg3, arg4, arg5);
            foreach (var row in data)
                p.AddRow(CsvReader.ConvertCell<T1>(row[0]), CsvReader.ConvertCell<T2>(row[1]),
                    CsvReader.ConvertCell<T3>(row[2]), CsvReader.ConvertCell<T4>(row[3]), CsvReader.ConvertCell<T5>(row[4]));
            return p;
        }

        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        /// <param name="arg4">Fourth argument</param>
        /// <param name="arg5">Fifth argument</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1, T2, T3, T4, T5> FromCsv(string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5) => FromCsv(Path.GetFileNameWithoutExtension(path), path, arg1, arg2, arg3, arg4, arg5);


        /// <summary>
        /// Convert the columns of the specified row to strings and write them to buffer
        /// </summary>
        public override void RowToStrings(uint rowNumber, string[] buffer) {
            var r = Table.PositionReference(rowNumber);
            buffer[0] = Stringify(r.Item1);
            buffer[1] = Stringify(r.Item2);
            buffer[2] = Stringify(r.Item3);
            buffer[3] = Stringify(r.Item4);
            buffer[4] = Stringify(r.Item5);
        }

        /// <summary>
        /// Call method on every row of the table, passing it a reference so it can rewrite it as it likes
        /// </summary>
        /// <param name="u"></param>
        public void UpdateRows(Update<(T1, T2, T3, T4, T5)> u) {
            for (var i = 0u; i < _table.Length; i++)
                u(ref _table.PositionReference(i));
        }

        /// <summary>
        /// Add rows of t to rows of this predicate
        /// </summary>
        public void Append(TablePredicate<T1, T2, T3, T4, T5> t) {
            for (var i = 0u; i < t._table.Length; i++) {
                var row = t._table.PositionReference(i);
                AddRow(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5);
            }
        }

        /// <summary>
        /// Add a rule using the default arguments as the head.
        /// </summary>
        /// <param name="body">subgoals</param>
        /// <returns>the original predicate</returns>
        public TablePredicate<T1, T2, T3, T4, T5> If(params Goal[] body) {
            AddRule(body);
            return this;
        }

        /// <summary>
        /// Add a rule to the predicate, using its default arguments as the head
        /// </summary>
        /// <param name="body">Antecedents for the rule</param>
        /// <returns>The original predicate (so these can be chained)</returns>
        public TablePredicate<T1,T2,T3,T4,T5> Where(params Goal[] body) => If(body);


        private List<TablePredicate<T1, T2, T3, T4, T5>>? inputs;

        /// <inheritdoc />
        public override IEnumerable<TablePredicate> Inputs => (inputs != null)?(IEnumerable<TablePredicate>)inputs:Array.Empty<TablePredicate>();

        /// <summary>
        /// Declare that on each simulation tick, the contents of the specified tables should be appended to this table.
        /// </summary>
        /// <param name="input">Predicate to append to this predicate on each tick</param>
        /// <returns>This predicate</returns>
        public TablePredicate<T1, T2, T3, T4, T5> Accumulates(TablePredicate<T1, T2, T3, T4, T5> input) {
            inputs ??= new List<TablePredicate<T1, T2, T3, T4, T5>>();
            inputs.Add(input);
            return this;
        }

        private TablePredicate<T1, T2, T3, T4, T5>? addPredicate;

        /// <summary>
        /// A predicate that is automatically appended to this predicate on every update.
        /// </summary>
        public TablePredicate<T1, T2, T3, T4, T5> Add {
            get {
                if (addPredicate != null) return addPredicate;
                addPredicate = new TablePredicate<T1, T2, T3, T4, T5>(Name + "__add",
                                                                      (Var<T1>)DefaultVariables[0],
                                                                      (Var<T2>)DefaultVariables[1],
                                                                      (Var<T3>)DefaultVariables[2],
                                                                      (Var<T4>)DefaultVariables[3],
                                                                      (Var<T5>)DefaultVariables[4]);
                Accumulates(addPredicate);
                addPredicate.Property[UpdaterFor] = this;
                addPredicate.Property[VisualizerName] = "Add";
                return addPredicate;
            }
        }

        /// <inheritdoc />
        public override TablePredicate AddUntyped => Add;

        private TablePredicate<T1,T2,T3,T4,T5>? initialValueTable;

        /// <summary>
        /// A predicate that is automatically appended to this predicate on every update.
        /// </summary>
        public TablePredicate<T1,T2,T3,T4,T5> Initially {
            get {
                if (initialValueTable != null) return initialValueTable;
                initialValueTable = new TablePredicate<T1,T2,T3,T4,T5>(Name + "__initially", 
                                                                       (Var<T1>)DefaultVariables[0], 
                                                                       (Var<T2>)DefaultVariables[1], 
                                                                       (Var<T3>)DefaultVariables[2], 
                                                                       (Var<T4>)DefaultVariables[3], 
                                                                       (Var<T5>)DefaultVariables[4]) {
                    Property = {
                        [UpdaterFor] = this, 
                        [VisualizerName] = "Initially"
                    }
                };
                return initialValueTable;
            }
        }
        
        /// <inheritdoc />
        public override TablePredicate InitialValueTable => initialValueTable;

        internal override void AddInitialData() {
            if (initialValueTable != null) Append(initialValueTable);
        }

        internal override void AppendInputs() {
            if (inputs == null) return;
            foreach (var input in inputs) Append(input);
        }

        /// <inheritdoc />
        public IEnumerator<(T1, T2, T3, T4, T5)> GetEnumerator() => Table.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #region Projections and mutators
        /// <inheritdoc />
        public override Delegate Projection(int columnNumber)
        {
            return columnNumber switch
            {
                0 => (Table.Projection<(T1,T2,T3,T4,T5), T1>)((in (T1,T2,T3,T4,T5) row) => row.Item1),
                1 => (Table.Projection<(T1,T2,T3,T4,T5), T2>)((in (T1,T2,T3,T4,T5) row) => row.Item2),
                2 => (Table.Projection<(T1,T2,T3,T4,T5), T3>)((in (T1,T2,T3,T4,T5) row) => row.Item3),
                3 => (Table.Projection<(T1,T2,T3,T4,T5), T4>)((in (T1,T2,T3,T4,T5) row) => row.Item4),
                4 => (Table.Projection<(T1,T2,T3,T4,T5), T5>)((in (T1,T2,T3,T4,T5) row) => row.Item5),
                _ => throw new ArgumentException($"There is no column number {columnNumber} in this table")
            };
        }

        /// <inheritdoc />
        public override Delegate Mutator(int columnNumber)
        {
            return columnNumber switch
            {
                0 => (Table.Mutator<(T1,T2,T3,T4,T5), T1>)((ref (T1,T2,T3,T4,T5) row, in T1 value) => row.Item1 = value),
                1 => (Table.Mutator<(T1,T2,T3,T4,T5), T2>)((ref (T1,T2,T3,T4,T5) row, in T2 value) => row.Item2 = value),
                2 => (Table.Mutator<(T1,T2,T3,T4,T5), T3>)((ref (T1,T2,T3,T4,T5) row, in T3 value) => row.Item3 = value),
                3 => (Table.Mutator<(T1,T2,T3,T4,T5), T4>)((ref (T1,T2,T3,T4,T5) row, in T4 value) => row.Item4 = value),
                4 => (Table.Mutator<(T1,T2,T3,T4,T5), T5>)((ref (T1,T2,T3,T4,T5) row, in T5 value) => row.Item5 = value),
                _ => throw new ArgumentException($"There is no column number {columnNumber} in this table")
            };
        }
        #endregion

        /// <inheritdoc />
        public override ColumnAccessor<TColumn, TKey> Accessor<TColumn, TKey>(Var<TKey> key, Var<TColumn> column)
            => Accessor(_table, key, column);

        public override Comparison<uint> RowComparison(int columnNumber) => columnNumber switch {
            0 => RowComparison<(T1,T2,T3,T4,T5),T1>(_table, columnNumber),
            1 => RowComparison<(T1,T2,T3,T4,T5),T2>(_table, columnNumber),
            2 => RowComparison<(T1,T2,T3,T4,T5),T3>(_table, columnNumber),
            3 => RowComparison<(T1,T2,T3,T4,T5),T4>(_table, columnNumber),
            4 => RowComparison<(T1,T2,T3,T4,T5),T5>(_table, columnNumber),
            _ => throw new ArgumentException($"There is no column number {columnNumber} in table {Name}")
        };

        public override Func<uint, TColumn> ColumnValueFromRowNumber<TColumn>(Var<TColumn> column) {
            var columnNumber = ColumnPositionOfDefaultVariable(column);
            return columnNumber switch {
                0 => (Func<uint, TColumn>)(Delegate)(Func<uint,T1>)(rowNum => _table.Data[rowNum].Item1),
                1 => (Func<uint, TColumn>)(Delegate)(Func<uint,T2>)(rowNum => _table.Data[rowNum].Item2),
                2 => (Func<uint, TColumn>)(Delegate)(Func<uint,T3>)(rowNum => _table.Data[rowNum].Item3),
                3 => (Func<uint, TColumn>)(Delegate)(Func<uint,T4>)(rowNum => _table.Data[rowNum].Item4),
                4 => (Func<uint, TColumn>)(Delegate)(Func<uint,T5>)(rowNum => _table.Data[rowNum].Item5),
                _ => throw new ArgumentException($"There is no column number {columnNumber} in table {Name}")
            };
        }
    }


    /// <summary>
    /// A 6-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of the predicate's 1st argument</typeparam>
    /// <typeparam name="T2">Type of the predicate's 2nd argument</typeparam>
    /// <typeparam name="T3">Type of the predicate's 3rd argument</typeparam>
    /// <typeparam name="T4">Type of the predicate's 4th argument</typeparam>
    /// <typeparam name="T5">Type of the predicate's 5th argument</typeparam>
    /// <typeparam name="T6">Type of the predicate's 6th argument</typeparam>
    public class TablePredicate<T1, T2, T3, T4, T5, T6> : TablePredicate, IEnumerable<(T1,T2,T3,T4,T5,T6)> {
        /// <summary>
        /// Make a Goal from this predicate with the specified argument value.
        /// </summary>
        public virtual TableGoal<T1, T2, T3, T4, T5, T6> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5, Term<T6> arg6] 
            => new TableGoal<T1, T2, T3, T4, T5, T6>(this, arg1, arg2,arg3, arg4, arg5, arg6);

        /// <inheritdoc />
        public override TableGoal GetGoal(Term[] args)
            => this[CastArg<T1>(args[0], 1), CastArg<T2>(args[1], 2), CastArg<T3>(args[2], 3),
                CastArg<T4>(args[3], 4), CastArg<T5>(args[4], 5), CastArg<T6>(args[5], 6)];

        /// <inheritdoc />
        protected override void AddIndex(int columnIndex, bool keyIndex) {
            switch (columnIndex) {
                case 0:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5, T6) r) => r.Item1, keyIndex));
                    break;
                case 1:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5, T6) r) => r.Item2, keyIndex));
                    break;
                case 2:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5, T6) r) => r.Item3, keyIndex));
                    break;
                case 3:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5, T6) r) => r.Item4, keyIndex));
                    break;
                case 4:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5, T6) r) => r.Item5, keyIndex));
                    break;
                case 5:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5, T6) r) => r.Item6, keyIndex));
                    break;
                default:
                    throw new ArgumentException($"Attempt to add an index for nonexistent column number {columnIndex} to table {Name}");
            }
        }

        /// <summary>
        /// Get the index for the specified key
        /// </summary>
        /// <typeparam name="TKey">Type of the indexed column</typeparam>
        /// <param name="column">Default variable representing the column</param>
        /// <returns>The index</returns>
        /// <exception cref="InvalidOperationException">If there is no index or it's not a key index</exception>
        public KeyIndex<(T1, T2, T3, T4, T5, T6), TKey> KeyIndex<TKey>(Var<TKey> column) {
            var i = IndexFor(ColumnPositionOfDefaultVariable(column), true);
            return i == null ? throw new InvalidOperationException($"No key index defined for {column}")
                       : (KeyIndex<(T1, T2, T3, T4, T5, T6), TKey>)i;
        }
        
        /// <summary>
        /// Get the index for the specified key
        /// </summary>
        /// <typeparam name="TKey">Type of the indexed column</typeparam>
        /// <param name="column">Position of the column</param>
        /// <returns>The index</returns>
        /// <exception cref="InvalidOperationException">If there is no index or it's not a key index</exception>
        public KeyIndex<(T1, T2, T3, T4, T5, T6), TKey> KeyIndex<TKey>(int column) {
            var i = IndexFor(column, true);
            return i == null ? throw new InvalidOperationException($"No key index defined for column {column}")
                       : (KeyIndex<(T1, T2, T3, T4, T5, T6), TKey>)i;
        }

        /// <summary>
        /// Make a new table predicate with the specified name
        /// </summary>
        /// <param name="name">Name of the predicate</param>
        /// <param name="arg1">Default variable for the first argument</param>
        /// <param name="arg2">Default variable for the second argument</param>
        /// <param name="arg3">Default variable for the third argument</param>
        /// <param name="arg4">Default variable for the fourth argument</param>
        /// <param name="arg5">Default variable for the fifth argument</param>
        /// <param name="arg6">Default variable for the sixth argument</param>
        public TablePredicate(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, 
                              IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5,
                              IColumnSpec<T6> arg6) 
            : base(name, null, arg1, arg2, arg3, arg4, arg5, arg6) { }

        /// <summary>
        /// Make a new table predicate with the specified name
        /// </summary>
        /// <param name="name">Name of the predicate</param>
        /// <param name="updateProc">Procedure to call to update the contents of the table.  This should only be used for tables that are the results of operators</param>
        /// <param name="arg1">Default variable for the first argument</param>
        /// <param name="arg2">Default variable for the second argument</param>
        /// <param name="arg3">Default variable for the third argument</param>
        /// <param name="arg4">Default variable for the fourth argument</param>
        /// <param name="arg5">Default variable for the fifth argument</param>
        /// <param name="arg6">Default variable for the sixth argument</param>
        public TablePredicate(string name, Action<Table> updateProc, IColumnSpec<T1> arg1, 
                              IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, 
                              IColumnSpec<T5> arg5, IColumnSpec<T6> arg6)
            : base(name, updateProc, arg1, arg2, arg3, arg4, arg5, arg6) { }

        // ReSharper disable once InconsistentNaming
        internal readonly Table<(T1,T2, T3, T4, T5, T6)> _table = new Table<(T1, T2, T3, T4, T5, T6)>();

        internal Table<(T1, T2, T3, T4, T5, T6)> Table {
            get {
                EnsureUpToDate();
                return _table;
            }
        }

        protected override Table TableUntyped => _table;

        /// <summary>
        /// The number of rows in the table (i.e. the number of tuples in the extension of the predicate)
        /// </summary>
        public override uint Length => Table.Length;

        /// <summary>
        /// Manually add a row (ground instance) to the extension of the predicate
        /// This cannot be mixed with rules using If.  If you want to have rules for the predicate
        /// use Fact() instead of AddRow.
        /// </summary>
        public void AddRow(in T1 v1, in T2 v2, in T3 v3, in T4 v4, in T5 v5, in T6 v6)
            => _table.Add((v1, v2, v3, v4, v5, v6));

        /// <summary>
        /// Add a set of rows from a generator
        /// </summary>
        public void AddRows(IEnumerable<(T1, T2, T3, T4, T5, T6)> generator) {
            var t = Table;
            foreach (var r in generator) t.Add(r);
        }
        internal IEnumerable<(T1, T2, T3, T4, T5, T6)> Match(Pattern<T1, T2, T3, T4, T5, T6> pat) {
            var t = Table;
            for (var i = 0u; i < t.Length; i++) {
                var e = t.PositionReference(i);
                if (pat.Match(e))
                    yield return e;
            }
        }

        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="name">Predicate name</param>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        /// <param name="arg4">Fourth argument</param>
        /// <param name="arg5">Fifth argument</param>
        /// <param name="arg6">Sixth argument</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1, T2, T3, T4, T5, T6> FromCsv(string name, string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6) {
            (var header, var data) = CsvReader.ReadCsv(path);
            VerifyCsvColumnNames(name, header, arg1, arg2, arg3, arg4, arg5, arg6);
            var p = new TablePredicate<T1, T2, T3, T4, T5, T6>(name, arg1, arg2, arg3, arg4, arg5, arg6);
            foreach (var row in data)
                p.AddRow(CsvReader.ConvertCell<T1>(row[0]), CsvReader.ConvertCell<T2>(row[1]),
                    CsvReader.ConvertCell<T3>(row[2]), CsvReader.ConvertCell<T4>(row[3]), CsvReader.ConvertCell<T5>(row[4]),
                    CsvReader.ConvertCell<T6>(row[5]));
            return p;
        }

        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        /// <param name="arg4">Fourth argument</param>
        /// <param name="arg5">Fifth argument</param>
        /// <param name="arg6">Sixth argument</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1, T2, T3, T4, T5, T6> FromCsv(string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6) => FromCsv(Path.GetFileNameWithoutExtension(path), path, arg1, arg2, arg3, arg4, arg5, arg6);


        /// <summary>
        /// Convert the columns of the specified row to strings and write them to buffer
        /// </summary>
        public override void RowToStrings(uint rowNumber, string[] buffer) {
            var r = Table.PositionReference(rowNumber);
            buffer[0] = Stringify(r.Item1);
            buffer[1] = Stringify(r.Item2);
            buffer[2] = Stringify(r.Item3);
            buffer[3] = Stringify(r.Item4);
            buffer[4] = Stringify(r.Item5);
            buffer[5] = Stringify(r.Item6);
        }

        /// <summary>
        /// Call method on every row of the table, passing it a reference so it can rewrite it as it likes
        /// </summary>
        /// <param name="u"></param>
        public void UpdateRows(Update<(T1, T2, T3, T4, T5, T6)> u) {
            for (var i = 0u; i < _table.Length; i++)
                u(ref _table.PositionReference(i));
        }

        /// <summary>
        /// Add rows of t to rows of this predicate
        /// </summary>
        public void Append(TablePredicate<T1, T2, T3, T4, T5, T6> t) {
            for (var i = 0u; i < t._table.Length; i++) {
                var row = t._table.PositionReference(i);
                AddRow(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6);
            }
        }

        /// <summary>
        /// Add a rule using the default arguments as the head.
        /// </summary>
        /// <param name="body">subgoals</param>
        /// <returns>the original predicate</returns>
        public TablePredicate<T1, T2, T3, T4, T5, T6> If(params Goal[] body) {
            AddRule(body);
            return this;
        }

        /// <summary>
        /// Add a rule to the predicate, using its default arguments as the head
        /// </summary>
        /// <param name="body">Antecedents for the rule</param>
        /// <returns>The original predicate (so these can be chained)</returns>
        public TablePredicate<T1,T2,T3,T4,T5,T6> Where(params Goal[] body) => If(body);
        
        private List<TablePredicate<T1, T2, T3, T4, T5, T6>>? inputs;

        /// <inheritdoc />
        public override IEnumerable<TablePredicate> Inputs => (inputs != null)?(IEnumerable<TablePredicate>)inputs:Array.Empty<TablePredicate>();

        /// <summary>
        /// Declare that on each simulation tick, the contents of the specified tables should be appended to this table.
        /// </summary>
        /// <param name="input">Predicate to append to this predicate on each tick</param>
        /// <returns>This predicate</returns>
        public TablePredicate<T1, T2, T3, T4, T5, T6> Accumulates(TablePredicate<T1, T2, T3, T4, T5, T6> input) {
            inputs ??= new List<TablePredicate<T1, T2, T3, T4, T5, T6>>();
            inputs.Add(input);
            return this;
        }

        private TablePredicate<T1, T2, T3, T4, T5, T6>? addPredicate;

        /// <summary>
        /// A predicate that is automatically appended to this predicate on every update.
        /// </summary>
        public TablePredicate<T1, T2, T3, T4, T5, T6> Add {
            get {
                if (addPredicate != null) return addPredicate;
                addPredicate = new TablePredicate<T1, T2, T3, T4, T5, T6>(Name + "__add",
                                                                          (Var<T1>)DefaultVariables[0],
                                                                          (Var<T2>)DefaultVariables[1],
                                                                          (Var<T3>)DefaultVariables[2],
                                                                          (Var<T4>)DefaultVariables[3],
                                                                          (Var<T5>)DefaultVariables[4],
                                                                          (Var<T6>)DefaultVariables[5]);
                Accumulates(addPredicate);
                addPredicate.Property[UpdaterFor] = this;
                addPredicate.Property[VisualizerName] = "Add";
                return addPredicate;
            }
        }

        /// <inheritdoc />
        public override TablePredicate AddUntyped => Add;

        private TablePredicate<T1,T2,T3,T4,T5,T6>? initialValueTable;

        /// <summary>
        /// A predicate that is automatically appended to this predicate on every update.
        /// </summary>
        public TablePredicate<T1,T2,T3,T4,T5,T6> Initially {
            get {
                if (initialValueTable != null) return initialValueTable;
                initialValueTable = new TablePredicate<T1,T2,T3,T4,T5,T6>(Name + "__initially", 
                                                                          (Var<T1>)DefaultVariables[0],
                                                                          (Var<T2>)DefaultVariables[1],
                                                                          (Var<T3>)DefaultVariables[2],
                                                                          (Var<T4>)DefaultVariables[3],
                                                                          (Var<T5>)DefaultVariables[4],
                                                                          (Var<T6>)DefaultVariables[5]) {
                    Property = {
                        [UpdaterFor] = this, 
                        [VisualizerName] = "Initially"
                    }
                };
                return initialValueTable;
            }
        }
        
        /// <inheritdoc />
        public override TablePredicate InitialValueTable => initialValueTable;

        internal override void AddInitialData() {
            if (initialValueTable != null) Append(initialValueTable);
        }

        internal override void AppendInputs() {
            if (inputs == null) return;
            foreach (var input in inputs) Append(input);
        }

        /// <inheritdoc />
        public IEnumerator<(T1, T2, T3, T4, T5, T6)> GetEnumerator() => Table.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #region Projections and mutators
        /// <inheritdoc />
        public override Delegate Projection(int columnNumber)
        {
            return columnNumber switch
            {
                0 => (Table.Projection<(T1,T2,T3,T4,T5,T6), T1>)((in (T1,T2,T3,T4,T5,T6) row) => row.Item1),
                1 => (Table.Projection<(T1,T2,T3,T4,T5,T6), T2>)((in (T1,T2,T3,T4,T5,T6) row) => row.Item2),
                2 => (Table.Projection<(T1,T2,T3,T4,T5,T6), T3>)((in (T1,T2,T3,T4,T5,T6) row) => row.Item3),
                3 => (Table.Projection<(T1,T2,T3,T4,T5,T6), T4>)((in (T1,T2,T3,T4,T5,T6) row) => row.Item4),
                4 => (Table.Projection<(T1,T2,T3,T4,T5,T6), T5>)((in (T1,T2,T3,T4,T5,T6) row) => row.Item5),
                5 => (Table.Projection<(T1,T2,T3,T4,T5,T6), T6>)((in (T1,T2,T3,T4,T5,T6) row) => row.Item6),
                _ => throw new ArgumentException($"There is no column number {columnNumber} in this table")
            };
        }

        /// <inheritdoc />
        public override Delegate Mutator(int columnNumber)
        {
            return columnNumber switch
            {
                0 => (Table.Mutator<(T1,T2,T3,T4,T5,T6), T1>)((ref (T1,T2,T3,T4,T5,T6) row, in T1 value) => row.Item1 = value),
                1 => (Table.Mutator<(T1,T2,T3,T4,T5,T6), T2>)((ref (T1,T2,T3,T4,T5,T6) row, in T2 value) => row.Item2 = value),
                2 => (Table.Mutator<(T1,T2,T3,T4,T5,T6), T3>)((ref (T1,T2,T3,T4,T5,T6) row, in T3 value) => row.Item3 = value),
                3 => (Table.Mutator<(T1,T2,T3,T4,T5,T6), T4>)((ref (T1,T2,T3,T4,T5,T6) row, in T4 value) => row.Item4 = value),
                4 => (Table.Mutator<(T1,T2,T3,T4,T5,T6), T5>)((ref (T1,T2,T3,T4,T5,T6) row, in T5 value) => row.Item5 = value),
                5 => (Table.Mutator<(T1,T2,T3,T4,T5,T6), T6>)((ref (T1,T2,T3,T4,T5,T6) row, in T6 value) => row.Item6 = value),
                _ => throw new ArgumentException($"There is no column number {columnNumber} in this table")
            };
        }
        #endregion

        /// <inheritdoc />
        public override ColumnAccessor<TColumn, TKey> Accessor<TColumn, TKey>(Var<TKey> key, Var<TColumn> column)
            => Accessor(_table, key, column);

        public override Comparison<uint> RowComparison(int columnNumber) => columnNumber switch {
            0 => RowComparison<(T1,T2,T3,T4,T5,T6),T1>(_table, columnNumber),
            1 => RowComparison<(T1,T2,T3,T4,T5,T6),T2>(_table, columnNumber),
            2 => RowComparison<(T1,T2,T3,T4,T5,T6),T3>(_table, columnNumber),
            3 => RowComparison<(T1,T2,T3,T4,T5,T6),T4>(_table, columnNumber),
            4 => RowComparison<(T1,T2,T3,T4,T5,T6),T5>(_table, columnNumber),
            5 => RowComparison<(T1,T2,T3,T4,T5,T6),T6>(_table, columnNumber),
            _ => throw new ArgumentException($"There is no column number {columnNumber} in table {Name}")
        };

        public override Func<uint, TColumn> ColumnValueFromRowNumber<TColumn>(Var<TColumn> column) {
            var columnNumber = ColumnPositionOfDefaultVariable(column);
            return columnNumber switch {
                0 => (Func<uint, TColumn>)(Delegate)(Func<uint,T1>)(rowNum => _table.Data[rowNum].Item1),
                1 => (Func<uint, TColumn>)(Delegate)(Func<uint,T2>)(rowNum => _table.Data[rowNum].Item2),
                2 => (Func<uint, TColumn>)(Delegate)(Func<uint,T3>)(rowNum => _table.Data[rowNum].Item3),
                3 => (Func<uint, TColumn>)(Delegate)(Func<uint,T4>)(rowNum => _table.Data[rowNum].Item4),
                4 => (Func<uint, TColumn>)(Delegate)(Func<uint,T5>)(rowNum => _table.Data[rowNum].Item5),
                5 => (Func<uint, TColumn>)(Delegate)(Func<uint,T6>)(rowNum => _table.Data[rowNum].Item6),
                _ => throw new ArgumentException($"There is no column number {columnNumber} in table {Name}")
            };
        }
    }


    /// <summary>
    /// A 7-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of the predicate's 1st argument</typeparam>
    /// <typeparam name="T2">Type of the predicate's 2nd argument</typeparam>
    /// <typeparam name="T3">Type of the predicate's 3rd argument</typeparam>
    /// <typeparam name="T4">Type of the predicate's 4th argument</typeparam>
    /// <typeparam name="T5">Type of the predicate's 5th argument</typeparam>
    /// <typeparam name="T6">Type of the predicate's 6th argument</typeparam>
    /// <typeparam name="T7">Type of the predicate's 7th argument</typeparam>
    public class TablePredicate<T1, T2, T3, T4, T5, T6, T7> : TablePredicate, IEnumerable<(T1, T2, T3, T4, T5, T6, T7)> {
        /// <summary>
        /// Make a Goal from this predicate with the specified argument value.
        /// </summary>
        public virtual TableGoal<T1, T2, T3, T4, T5, T6, T7> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5, Term<T6> arg6, Term<T7> arg7]
            => new TableGoal<T1, T2, T3, T4, T5, T6, T7>(this, arg1, arg2, arg3, arg4, arg5, arg6, arg7);

        /// <inheritdoc />
        public override TableGoal GetGoal(Term[] args)
            => this[CastArg<T1>(args[0], 1), CastArg<T2>(args[1], 2), CastArg<T3>(args[2], 3),
                CastArg<T4>(args[3], 4), CastArg<T5>(args[4], 5), CastArg<T6>(args[5], 6), CastArg<T7>(args[6], 7)];

        /// <inheritdoc />
        protected override void AddIndex(int columnIndex, bool keyIndex) {
            switch (columnIndex) {
                case 0:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5, T6, T7) r) => r.Item1, keyIndex));
                    break;
                case 1:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5, T6, T7) r) => r.Item2, keyIndex));
                    break;
                case 2:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5, T6, T7) r) => r.Item3, keyIndex));
                    break;
                case 3:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5, T6, T7) r) => r.Item4, keyIndex));
                    break;
                case 4:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5, T6, T7) r) => r.Item5, keyIndex));
                    break;
                case 5:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5, T6, T7) r) => r.Item6, keyIndex));
                    break;
                case 6:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5, T6, T7) r) => r.Item7, keyIndex));
                    break;
                default:
                    throw new ArgumentException($"Attempt to add an index for nonexistent column number {columnIndex} to table {Name}");
            }
        }

        /// <summary>
        /// Get the index for the specified key
        /// </summary>
        /// <typeparam name="TKey">Type of the indexed column</typeparam>
        /// <param name="column">Default variable representing the column</param>
        /// <returns>The index</returns>
        /// <exception cref="InvalidOperationException">If there is no index or it's not a key index</exception>
        public KeyIndex<(T1, T2, T3, T4, T5, T6, T7), TKey> KeyIndex<TKey>(Var<TKey> column) {
            var i = IndexFor(ColumnPositionOfDefaultVariable(column), true);
            return i == null ? throw new InvalidOperationException($"No key index defined for {column}")
                       : (KeyIndex<(T1, T2, T3, T4, T5, T6, T7), TKey>)i;
        }

        /// <summary>
        /// Get the index for the specified key
        /// </summary>
        /// <typeparam name="TKey">Type of the indexed column</typeparam>
        /// <param name="column">Position of the column</param>
        /// <returns>The index</returns>
        /// <exception cref="InvalidOperationException">If there is no index or it's not a key index</exception>
        public KeyIndex<(T1, T2, T3, T4, T5, T6, T7), TKey> KeyIndex<TKey>(int column) {
            var i = IndexFor(column, true);
            return i == null ? throw new InvalidOperationException($"No key index defined for column {column}")
                       : (KeyIndex<(T1, T2, T3, T4, T5, T6, T7), TKey>)i;
        }

        /// <summary>
        /// Make a new table predicate with the specified name
        /// </summary>
        /// <param name="name">Name of the predicate</param>
        /// <param name="arg1">Default variable for the first argument</param>
        /// <param name="arg2">Default variable for the second argument</param>
        /// <param name="arg3">Default variable for the third argument</param>
        /// <param name="arg4">Default variable for the fourth argument</param>
        /// <param name="arg5">Default variable for the fifth argument</param>
        /// <param name="arg6">Default variable for the sixth argument</param>
        /// <param name="arg7">Default variable for the seventh argument</param>
        public TablePredicate(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, 
                              IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, 
                              IColumnSpec<T6> arg6, IColumnSpec<T7> arg7)
            : base(name, null, arg1, arg2, arg3, arg4, arg5, arg6, arg7) { }

        /// <summary>
        /// Make a new table predicate with the specified name
        /// </summary>
        /// <param name="name">Name of the predicate</param>
        /// <param name="updateProc">Procedure to call to update the contents of the table.  This should only be used for tables that are the results of operators</param>
        /// <param name="arg1">Default variable for the first argument</param>
        /// <param name="arg2">Default variable for the second argument</param>
        /// <param name="arg3">Default variable for the third argument</param>
        /// <param name="arg4">Default variable for the fourth argument</param>
        /// <param name="arg5">Default variable for the fifth argument</param>
        /// <param name="arg6">Default variable for the sixth argument</param>
        /// <param name="arg7">Default variable for the seventh argument</param>
        public TablePredicate(string name, Action<Table> updateProc, IColumnSpec<T1> arg1, 
                              IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, 
                              IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7)
            : base(name, updateProc, arg1, arg2, arg3, arg4, arg5, arg6, arg7) { }

        // ReSharper disable once InconsistentNaming
        internal readonly Table<(T1, T2, T3, T4, T5, T6, T7)> _table = new Table<(T1, T2, T3, T4, T5, T6, T7)>();

        internal Table<(T1, T2, T3, T4, T5, T6, T7)> Table {
            get {
                EnsureUpToDate();
                return _table;
            }
        }

        protected override Table TableUntyped => _table;

        /// <summary>
        /// The number of rows in the table (i.e. the number of tuples in the extension of the predicate)
        /// </summary>
        public override uint Length => Table.Length;

        /// <summary>
        /// Manually add a row (ground instance) to the extension of the predicate
        /// This cannot be mixed with rules using If.  If you want to have rules for the predicate
        /// use Fact() instead of AddRow.
        /// </summary>
        public void AddRow(in T1 v1, in T2 v2, in T3 v3, in T4 v4, in T5 v5, in T6 v6, in T7 v7)
            => _table.Add((v1, v2, v3, v4, v5, v6, v7));

        /// <summary>
        /// Add a set of rows from a generator
        /// </summary>
        public void AddRows(IEnumerable<(T1, T2, T3, T4, T5, T6, T7)> generator) {
            var t = Table;
            foreach (var r in generator) t.Add(r);
        }
        internal IEnumerable<(T1, T2, T3, T4, T5, T6, T7)> Match(Pattern<T1, T2, T3, T4, T5, T6, T7> pat) {
            var t = Table;
            for (var i = 0u; i < t.Length; i++) {
                var e = t.PositionReference(i);
                if (pat.Match(e))
                    yield return e;
            }
        }

        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="name">Predicate name</param>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        /// <param name="arg4">Fourth argument</param>
        /// <param name="arg5">Fifth argument</param>
        /// <param name="arg6">Sixth argument</param>
        /// <param name="arg7">Seventh argument</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1, T2, T3, T4, T5, T6, T7> FromCsv(string name, string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7) {
            (var header, var data) = CsvReader.ReadCsv(path);
            VerifyCsvColumnNames(name, header, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            var p = new TablePredicate<T1, T2, T3, T4, T5, T6, T7>(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            foreach (var row in data)
                p.AddRow(CsvReader.ConvertCell<T1>(row[0]), CsvReader.ConvertCell<T2>(row[1]),
                    CsvReader.ConvertCell<T3>(row[2]), CsvReader.ConvertCell<T4>(row[3]), CsvReader.ConvertCell<T5>(row[4]),
                    CsvReader.ConvertCell<T6>(row[5]), CsvReader.ConvertCell<T7>(row[6]));
            return p;
        }

        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        /// <param name="arg4">Fourth argument</param>
        /// <param name="arg5">Fifth argument</param>
        /// <param name="arg6">Sixth argument</param>
        /// <param name="arg7">Seventh argument</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1, T2, T3, T4, T5, T6, T7> FromCsv(string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7) => FromCsv(Path.GetFileNameWithoutExtension(path), path, arg1, arg2, arg3, arg4, arg5, arg6, arg7);


        /// <summary>
        /// Convert the columns of the specified row to strings and write them to buffer
        /// </summary>
        public override void RowToStrings(uint rowNumber, string[] buffer) {
            var r = Table.PositionReference(rowNumber);
            buffer[0] = Stringify(r.Item1);
            buffer[1] = Stringify(r.Item2);
            buffer[2] = Stringify(r.Item3);
            buffer[3] = Stringify(r.Item4);
            buffer[4] = Stringify(r.Item5);
            buffer[5] = Stringify(r.Item6);
            buffer[6] = Stringify(r.Item7);
        }

        /// <summary>
        /// Call method on every row of the table, passing it a reference so it can rewrite it as it likes
        /// </summary>
        /// <param name="u"></param>
        public void UpdateRows(Update<(T1, T2, T3, T4, T5, T6, T7)> u) {
            for (var i = 0u; i < _table.Length; i++)
                u(ref _table.PositionReference(i));
        }

        /// <summary>
        /// Add rows of t to rows of this predicate
        /// </summary>
        public void Append(TablePredicate<T1, T2, T3, T4, T5, T6, T7> t) {
            for (var i = 0u; i < t._table.Length; i++) {
                var row = t._table.PositionReference(i);
                AddRow(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6, row.Item7);
            }
        }

        /// <summary>
        /// Add a rule using the default arguments as the head.
        /// </summary>
        /// <param name="body">subgoals</param>
        /// <returns>the original predicate</returns>
        public TablePredicate<T1, T2, T3, T4, T5, T6, T7> If(params Goal[] body) {
            AddRule(body);
            return this;
        }

        /// <summary>
        /// Add a rule to the predicate, using its default arguments as the head
        /// </summary>
        /// <param name="body">Antecedents for the rule</param>
        /// <returns>The original predicate (so these can be chained)</returns>
        public TablePredicate<T1,T2,T3,T4,T5,T6,T7> Where(params Goal[] body) => If(body);

        private List<TablePredicate<T1, T2, T3, T4, T5, T6, T7>>? inputs;

        /// <inheritdoc />
        public override IEnumerable<TablePredicate> Inputs => (inputs != null)?(IEnumerable<TablePredicate>)inputs:Array.Empty<TablePredicate>();


        /// <summary>
        /// Declare that on each simulation tick, the contents of the specified tables should be appended to this table.
        /// </summary>
        /// <param name="input">Predicate to append to this predicate on each tick</param>
        /// <returns>This predicate</returns>
        public TablePredicate<T1, T2, T3, T4, T5, T6, T7> Accumulates(TablePredicate<T1, T2, T3, T4, T5, T6, T7> input) {
            inputs ??= new List<TablePredicate<T1, T2, T3, T4, T5, T6, T7>>();
            inputs.Add(input);
            return this;
        }

        private TablePredicate<T1, T2, T3, T4, T5, T6, T7>? addPredicate;

        /// <summary>
        /// A predicate that is automatically appended to this predicate on every update.
        /// </summary>
        public TablePredicate<T1, T2, T3, T4, T5, T6, T7> Add {
            get {
                if (addPredicate != null) return addPredicate;
                addPredicate = new TablePredicate<T1, T2, T3, T4, T5, T6, T7>(Name + "__add",
                                                                              (Var<T1>)DefaultVariables[0],
                                                                              (Var<T2>)DefaultVariables[1],
                                                                              (Var<T3>)DefaultVariables[2],
                                                                              (Var<T4>)DefaultVariables[3],
                                                                              (Var<T5>)DefaultVariables[4],
                                                                              (Var<T6>)DefaultVariables[5],
                                                                              (Var<T7>)DefaultVariables[6]);
                Accumulates(addPredicate);
                addPredicate.Property[UpdaterFor] = this;
                addPredicate.Property[VisualizerName] = "Add";
                return addPredicate;
            }
        }

        /// <inheritdoc />
        public override TablePredicate AddUntyped => Add;

        private TablePredicate<T1,T2,T3,T4,T5,T6,T7>? initialValueTable;

        /// <summary>
        /// A predicate that is automatically appended to this predicate on every update.
        /// </summary>
        public TablePredicate<T1,T2,T3,T4,T5,T6,T7> Initially {
            get {
                if (initialValueTable != null) return initialValueTable;
                initialValueTable = new TablePredicate<T1,T2,T3,T4,T5,T6,T7>(Name + "__initially", 
                                                                             (Var<T1>)DefaultVariables[0],
                                                                             (Var<T2>)DefaultVariables[1],
                                                                             (Var<T3>)DefaultVariables[2],
                                                                             (Var<T4>)DefaultVariables[3],
                                                                             (Var<T5>)DefaultVariables[4],
                                                                             (Var<T6>)DefaultVariables[5],
                                                                             (Var<T7>)DefaultVariables[6]) {
                    Property = { 
                        [UpdaterFor] = this, 
                        [VisualizerName] = "Initially"
                    }
                };
                return initialValueTable;
            }
        }
        
        /// <inheritdoc />
        public override TablePredicate InitialValueTable => initialValueTable;

        internal override void AddInitialData() {
            if (initialValueTable != null) Append(initialValueTable);
        }

        internal override void AppendInputs() {
            if (inputs == null) return;
            foreach (var input in inputs) Append(input);
        }

        /// <inheritdoc />
        public IEnumerator<(T1, T2, T3, T4, T5, T6, T7)> GetEnumerator() => Table.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #region Projections and mutators
        /// <inheritdoc />
        public override Delegate Projection(int columnNumber)
        {
            return columnNumber switch
            {
                0 => (Table.Projection<(T1,T2,T3,T4,T5,T6,T7), T1>)((in (T1,T2,T3,T4,T5,T6,T7) row) => row.Item1),
                1 => (Table.Projection<(T1,T2,T3,T4,T5,T6,T7), T2>)((in (T1,T2,T3,T4,T5,T6,T7) row) => row.Item2),
                2 => (Table.Projection<(T1,T2,T3,T4,T5,T6,T7), T3>)((in (T1,T2,T3,T4,T5,T6,T7) row) => row.Item3),
                3 => (Table.Projection<(T1,T2,T3,T4,T5,T6,T7), T4>)((in (T1,T2,T3,T4,T5,T6,T7) row) => row.Item4),
                4 => (Table.Projection<(T1,T2,T3,T4,T5,T6,T7), T5>)((in (T1,T2,T3,T4,T5,T6,T7) row) => row.Item5),
                5 => (Table.Projection<(T1,T2,T3,T4,T5,T6,T7), T6>)((in (T1,T2,T3,T4,T5,T6,T7) row) => row.Item6),
                6 => (Table.Projection<(T1,T2,T3,T4,T5,T6,T7), T7>)((in (T1,T2,T3,T4,T5,T6,T7) row) => row.Item7),
                _ => throw new ArgumentException($"There is no column number {columnNumber} in this table")
            };
        }

        /// <inheritdoc />
        public override Delegate Mutator(int columnNumber)
        {
            return columnNumber switch
            {
                0 => (Table.Mutator<(T1,T2,T3,T4,T5,T6,T7), T1>)((ref (T1,T2,T3,T4,T5,T6,T7) row, in T1 value) => row.Item1 = value),
                1 => (Table.Mutator<(T1,T2,T3,T4,T5,T6,T7), T2>)((ref (T1,T2,T3,T4,T5,T6,T7) row, in T2 value) => row.Item2 = value),
                2 => (Table.Mutator<(T1,T2,T3,T4,T5,T6,T7), T3>)((ref (T1,T2,T3,T4,T5,T6,T7) row, in T3 value) => row.Item3 = value),
                3 => (Table.Mutator<(T1,T2,T3,T4,T5,T6,T7), T4>)((ref (T1,T2,T3,T4,T5,T6,T7) row, in T4 value) => row.Item4 = value),
                4 => (Table.Mutator<(T1,T2,T3,T4,T5,T6,T7), T5>)((ref (T1,T2,T3,T4,T5,T6,T7) row, in T5 value) => row.Item5 = value),
                5 => (Table.Mutator<(T1,T2,T3,T4,T5,T6,T7), T6>)((ref (T1,T2,T3,T4,T5,T6,T7) row, in T6 value) => row.Item6 = value),
                6 => (Table.Mutator<(T1,T2,T3,T4,T5,T6,T7), T7>)((ref (T1,T2,T3,T4,T5,T6,T7) row, in T7 value) => row.Item7 = value),
                _ => throw new ArgumentException($"There is no column number {columnNumber} in this table")
            };
        }
        #endregion

        /// <inheritdoc />
        public override ColumnAccessor<TColumn, TKey> Accessor<TColumn, TKey>(Var<TKey> key, Var<TColumn> column)
            => Accessor(_table, key, column);

        public override Comparison<uint> RowComparison(int columnNumber) => columnNumber switch {
            0 => RowComparison<(T1,T2,T3,T4,T5,T6,T7),T1>(_table, columnNumber),
            1 => RowComparison<(T1,T2,T3,T4,T5,T6,T7),T2>(_table, columnNumber),
            2 => RowComparison<(T1,T2,T3,T4,T5,T6,T7),T3>(_table, columnNumber),
            3 => RowComparison<(T1,T2,T3,T4,T5,T6,T7),T4>(_table, columnNumber),
            4 => RowComparison<(T1,T2,T3,T4,T5,T6,T7),T5>(_table, columnNumber),
            5 => RowComparison<(T1,T2,T3,T4,T5,T6,T7),T6>(_table, columnNumber),
            6 => RowComparison<(T1,T2,T3,T4,T5,T6,T7),T7>(_table, columnNumber),
            _ => throw new ArgumentException($"There is no column number {columnNumber} in table {Name}")
        };

        public override Func<uint, TColumn> ColumnValueFromRowNumber<TColumn>(Var<TColumn> column) {
            var columnNumber = ColumnPositionOfDefaultVariable(column);
            return columnNumber switch {
                0 => (Func<uint, TColumn>)(Delegate)(Func<uint,T1>)(rowNum => _table.Data[rowNum].Item1),
                1 => (Func<uint, TColumn>)(Delegate)(Func<uint,T2>)(rowNum => _table.Data[rowNum].Item2),
                2 => (Func<uint, TColumn>)(Delegate)(Func<uint,T3>)(rowNum => _table.Data[rowNum].Item3),
                3 => (Func<uint, TColumn>)(Delegate)(Func<uint,T4>)(rowNum => _table.Data[rowNum].Item4),
                4 => (Func<uint, TColumn>)(Delegate)(Func<uint,T5>)(rowNum => _table.Data[rowNum].Item5),
                5 => (Func<uint, TColumn>)(Delegate)(Func<uint,T6>)(rowNum => _table.Data[rowNum].Item6),
                6 => (Func<uint, TColumn>)(Delegate)(Func<uint,T7>)(rowNum => _table.Data[rowNum].Item7),
                _ => throw new ArgumentException($"There is no column number {columnNumber} in table {Name}")
            };
        }
    }


    /// <summary>
    /// A 8-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of the predicate's 1st argument</typeparam>
    /// <typeparam name="T2">Type of the predicate's 2nd argument</typeparam>
    /// <typeparam name="T3">Type of the predicate's 3rd argument</typeparam>
    /// <typeparam name="T4">Type of the predicate's 4th argument</typeparam>
    /// <typeparam name="T5">Type of the predicate's 5th argument</typeparam>
    /// <typeparam name="T6">Type of the predicate's 6th argument</typeparam>
    /// <typeparam name="T7">Type of the predicate's 7th argument</typeparam>
    /// <typeparam name="T8">Type of the predicate's 8th argument</typeparam>
    public class TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> : TablePredicate, IEnumerable<(T1, T2, T3, T4, T5, T6, T7, T8)> {
        /// <summary>
        /// Make a Goal from this predicate with the specified argument value.
        /// </summary>
        public virtual TableGoal<T1, T2, T3, T4, T5, T6, T7, T8> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5, Term<T6> arg6, Term<T7> arg7, Term<T8> arg8]
            => new TableGoal<T1, T2, T3, T4, T5, T6, T7, T8>(this, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);

        /// <inheritdoc />
        public override TableGoal GetGoal(Term[] args)
            => this[CastArg<T1>(args[0], 1), CastArg<T2>(args[1], 2), CastArg<T3>(args[2], 3), CastArg<T4>(args[3], 4),
                    CastArg<T5>(args[4], 5), CastArg<T6>(args[5], 6), CastArg<T7>(args[6], 7), CastArg<T8>(args[7], 8)];

        /// <inheritdoc />
        protected override void AddIndex(int columnIndex, bool keyIndex) {
            switch (columnIndex) {
                case 0:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5, T6, T7, T8) r) => r.Item1, keyIndex));
                    break;
                case 1:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5, T6, T7, T8) r) => r.Item2, keyIndex));
                    break;
                case 2:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5, T6, T7, T8) r) => r.Item3, keyIndex));
                    break;
                case 3:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5, T6, T7, T8) r) => r.Item4, keyIndex));
                    break;
                case 4:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5, T6, T7, T8) r) => r.Item5, keyIndex));
                    break;
                case 5:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5, T6, T7, T8) r) => r.Item6, keyIndex));
                    break;
                case 6:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5, T6, T7, T8) r) => r.Item7, keyIndex));
                    break;
                case 7:
                    _table.AddIndex(TableIndex.MakeIndex(this, _table, columnIndex, (in (T1, T2, T3, T4, T5, T6, T7, T8) r) => r.Item8, keyIndex));
                    break;
                default:
                    throw new ArgumentException($"Attempt to add an index for nonexistent column number {columnIndex} to table {Name}");
            }
        }

        /// <summary>
        /// Get the index for the specified key
        /// </summary>
        /// <typeparam name="TKey">Type of the indexed column</typeparam>
        /// <param name="column">Default variable representing the column</param>
        /// <returns>The index</returns>
        /// <exception cref="InvalidOperationException">If there is no index or it's not a key index</exception>
        public KeyIndex<(T1, T2, T3, T4, T5, T6, T7, T8), TKey> KeyIndex<TKey>(Var<TKey> column) {
            var i = IndexFor(ColumnPositionOfDefaultVariable(column), true);
            return i == null ? throw new InvalidOperationException($"No key index defined for {column}")
                       : (KeyIndex<(T1, T2, T3, T4, T5, T6, T7, T8), TKey>)i;
        }

        /// <summary>
        /// Get the index for the specified key
        /// </summary>
        /// <typeparam name="TKey">Type of the indexed column</typeparam>
        /// <param name="column">Position of the column</param>
        /// <returns>The index</returns>
        /// <exception cref="InvalidOperationException">If there is no index or it's not a key index</exception>
        public KeyIndex<(T1, T2, T3, T4, T5, T6, T7, T8), TKey> KeyIndex<TKey>(int column) {
            var i = IndexFor(column, true);
            return i == null ? throw new InvalidOperationException($"No key index defined for column {column}")
                       : (KeyIndex<(T1, T2, T3, T4, T5, T6, T7, T8), TKey>)i;
        }

        /// <summary>
        /// Make a new table predicate with the specified name
        /// </summary>
        /// <param name="name">Name of the predicate</param>
        /// <param name="arg1">Default variable for the first argument</param>
        /// <param name="arg2">Default variable for the second argument</param>
        /// <param name="arg3">Default variable for the third argument</param>
        /// <param name="arg4">Default variable for the fourth argument</param>
        /// <param name="arg5">Default variable for the fifth argument</param>
        /// <param name="arg6">Default variable for the sixth argument</param>
        /// <param name="arg7">Default variable for the seventh argument</param>
        /// <param name="arg8">Default variable for the eighth argument</param>
        public TablePredicate(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, 
                              IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, 
                              IColumnSpec<T6> arg6, IColumnSpec<T7> arg7, IColumnSpec<T8> arg8)
            : base(name, null, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) { }

        /// <summary>
        /// Make a new table predicate with the specified name
        /// </summary>
        /// <param name="name">Name of the predicate</param>
        /// <param name="updateProc">Procedure to call to update the contents of the table.  This should only be used for tables that are the results of operators</param>
        /// <param name="arg1">Default variable for the first argument</param>
        /// <param name="arg2">Default variable for the second argument</param>
        /// <param name="arg3">Default variable for the third argument</param>
        /// <param name="arg4">Default variable for the fourth argument</param>
        /// <param name="arg5">Default variable for the fifth argument</param>
        /// <param name="arg6">Default variable for the sixth argument</param>
        /// <param name="arg7">Default variable for the seventh argument</param>
        /// <param name="arg8">Default variable for the eight argument</param>
        public TablePredicate(string name, Action<Table> updateProc, 
                              IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, 
                              IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, 
                              IColumnSpec<T7> arg7, IColumnSpec<T8> arg8)
            : base(name, updateProc, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) { }


        // ReSharper disable once InconsistentNaming
        internal readonly Table<(T1, T2, T3, T4, T5, T6, T7, T8)> _table = new Table<(T1, T2, T3, T4, T5, T6, T7, T8)>();

        internal Table<(T1, T2, T3, T4, T5, T6, T7, T8)> Table {
            get {
                EnsureUpToDate();
                return _table;
            }
        }

        protected override Table TableUntyped => _table;

        /// <summary>
        /// The number of rows in the table (i.e. the number of tuples in the extension of the predicate)
        /// </summary>
        public override uint Length => Table.Length;

        /// <summary>
        /// Manually add a row (ground instance) to the extension of the predicate
        /// This cannot be mixed with rules using If.  If you want to have rules for the predicate
        /// use Fact() instead of AddRow.
        /// </summary>
        public void AddRow(in T1 v1, in T2 v2, in T3 v3, in T4 v4, in T5 v5, in T6 v6, in T7 v7, in T8 v8)
            => _table.Add((v1, v2, v3, v4, v5, v6, v7, v8));

        /// <summary>
        /// Add a set of rows from a generator
        /// </summary>
        public void AddRows(IEnumerable<(T1, T2, T3, T4, T5, T6, T7, T8)> generator) {
            var t = Table;
            foreach (var r in generator) t.Add(r);
        }
        internal IEnumerable<(T1, T2, T3, T4, T5, T6, T7, T8)> Match(Pattern<T1, T2, T3, T4, T5, T6, T7, T8> pat) {
            var t = Table;
            for (var i = 0u; i < t.Length; i++) {
                var e = t.PositionReference(i);
                if (pat.Match(e))
                    yield return e;
            }
        }

        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="name">Predicate name</param>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        /// <param name="arg4">Fourth argument</param>
        /// <param name="arg5">Fifth argument</param>
        /// <param name="arg6">Sixth argument</param>
        /// <param name="arg7">Seventh argument</param>
        /// <param name="arg8">Eighth argument</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> FromCsv(string name, string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7, IColumnSpec<T8> arg8) {
            (var header, var data) = CsvReader.ReadCsv(path);
            VerifyCsvColumnNames(name, header, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            var p = new TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8>(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            foreach (var row in data)
                p.AddRow(CsvReader.ConvertCell<T1>(row[0]), CsvReader.ConvertCell<T2>(row[1]),
                    CsvReader.ConvertCell<T3>(row[2]), CsvReader.ConvertCell<T4>(row[3]), CsvReader.ConvertCell<T5>(row[4]),
                    CsvReader.ConvertCell<T6>(row[5]), CsvReader.ConvertCell<T7>(row[6]), CsvReader.ConvertCell<T8>(row[7]));
            return p;
        }

        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        /// <param name="arg4">Fourth argument</param>
        /// <param name="arg5">Fifth argument</param>
        /// <param name="arg6">Sixth argument</param>
        /// <param name="arg7">Seventh argument</param>
        /// <param name="arg8">Eighth argument</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> FromCsv(string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7, IColumnSpec<T8> arg8) => FromCsv(Path.GetFileNameWithoutExtension(path), path, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);


        /// <summary>
        /// Convert the columns of the specified row to strings and write them to buffer
        /// </summary>
        public override void RowToStrings(uint rowNumber, string[] buffer) {
            var r = Table.PositionReference(rowNumber);
            buffer[0] = Stringify(r.Item1);
            buffer[1] = Stringify(r.Item2);
            buffer[2] = Stringify(r.Item3);
            buffer[3] = Stringify(r.Item4);
            buffer[4] = Stringify(r.Item5);
            buffer[5] = Stringify(r.Item6);
            buffer[6] = Stringify(r.Item7);
            buffer[7] = Stringify(r.Item8);
        }

        /// <summary>
        /// Call method on every row of the table, passing it a reference so it can rewrite it as it likes
        /// </summary>
        /// <param name="u"></param>
        public void UpdateRows(Update<(T1, T2, T3, T4, T5, T6, T7, T8)> u) {
            for (var i = 0u; i < _table.Length; i++)
                u(ref _table.PositionReference(i));
        }

        /// <summary>
        /// Add rows of t to rows of this predicate
        /// </summary>
        public void Append(TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> t) {
            for (var i = 0u; i < t._table.Length; i++) {
                var row = t._table.PositionReference(i);
                AddRow(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6, row.Item7, row.Item8);
            }
        }

        /// <summary>
        /// Add a rule using the default arguments as the head.
        /// </summary>
        /// <param name="body">subgoals</param>
        /// <returns>the original predicate</returns>
        public TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> If(params Goal[] body) {
            AddRule(body);
            return this;
        }

        /// <summary>
        /// Add a rule to the predicate, using its default arguments as the head
        /// </summary>
        /// <param name="body">Antecedents for the rule</param>
        /// <returns>The original predicate (so these can be chained)</returns>
        public TablePredicate<T1,T2,T3,T4,T5,T6,T7,T8> Where(params Goal[] body) => If(body);

        private List<TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8>>? inputs;

        /// <inheritdoc />
        public override IEnumerable<TablePredicate> Inputs => (inputs != null)?(IEnumerable<TablePredicate>)inputs:Array.Empty<TablePredicate>();

        /// <summary>
        /// Declare that on each simulation tick, the contents of the specified tables should be appended to this table.
        /// </summary>
        /// <param name="input">Predicate to append to this predicate on each tick</param>
        /// <returns>This predicate</returns>
        public TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> Accumulates(TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> input) {
            inputs ??= new List<TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8>>();
            inputs.Add(input);
            return this;
        }

        private TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8>? addPredicate;

        /// <summary>
        /// A predicate that is automatically appended to this predicate on every update.
        /// </summary>
        public TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> Add {
            get {
                if (addPredicate != null) return addPredicate;
                addPredicate = new TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8>(Name + "__add", 
                    (Var<T1>)DefaultVariables[0],
                    (Var<T2>)DefaultVariables[1],
                    (Var<T3>)DefaultVariables[2],
                    (Var<T4>)DefaultVariables[3],
                    (Var<T5>)DefaultVariables[4],
                    (Var<T6>)DefaultVariables[5],
                    (Var<T7>)DefaultVariables[6],
                    (Var<T8>)DefaultVariables[7]);
                Accumulates(addPredicate);
                addPredicate.Property[UpdaterFor] = this;
                addPredicate.Property[VisualizerName] = "Add";
                return addPredicate;
            }
        }

        /// <inheritdoc />
        public override TablePredicate AddUntyped => Add;

        private TablePredicate<T1,T2,T3,T4,T5,T6,T7,T8>? initialValueTable;

        /// <summary>
        /// A predicate that is automatically appended to this predicate on every update.
        /// </summary>
        public TablePredicate<T1,T2,T3,T4,T5,T6,T7,T8> Initially {
            get {
                if (initialValueTable != null) return initialValueTable;
                initialValueTable = new TablePredicate<T1,T2,T3,T4,T5,T6,T7,T8>(Name + "__initially", 
                    (Var<T1>)DefaultVariables[0],
                    (Var<T2>)DefaultVariables[1],
                    (Var<T3>)DefaultVariables[2],
                    (Var<T4>)DefaultVariables[3],
                    (Var<T5>)DefaultVariables[4],
                    (Var<T6>)DefaultVariables[5], 
                    (Var<T7>)DefaultVariables[6],
                    (Var<T8>)DefaultVariables[7]) { 
                    Property = { 
                        [UpdaterFor] = this, 
                        [VisualizerName] = "Initially"
                    }
                };
                return initialValueTable;
            }
        }
        
        /// <inheritdoc />
        public override TablePredicate InitialValueTable => initialValueTable;

        internal override void AddInitialData() {
            if (initialValueTable != null) Append(initialValueTable);
        }

        internal override void AppendInputs() {
            if (inputs == null) return;
            foreach (var input in inputs) Append(input);
        }

        /// <inheritdoc />
        public IEnumerator<(T1, T2, T3, T4, T5, T6, T7, T8)> GetEnumerator() => Table.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #region Projections and mutators
        /// <inheritdoc />
        public override Delegate Projection(int columnNumber)
        {
            return columnNumber switch
            {
                0 => (Table.Projection<(T1,T2,T3,T4,T5,T6,T7,T8), T1>)((in (T1,T2,T3,T4,T5,T6,T7,T8) row) => row.Item1),
                1 => (Table.Projection<(T1,T2,T3,T4,T5,T6,T7,T8), T2>)((in (T1,T2,T3,T4,T5,T6,T7,T8) row) => row.Item2),
                2 => (Table.Projection<(T1,T2,T3,T4,T5,T6,T7,T8), T3>)((in (T1,T2,T3,T4,T5,T6,T7,T8) row) => row.Item3),
                3 => (Table.Projection<(T1,T2,T3,T4,T5,T6,T7,T8), T4>)((in (T1,T2,T3,T4,T5,T6,T7,T8) row) => row.Item4),
                4 => (Table.Projection<(T1,T2,T3,T4,T5,T6,T7,T8), T5>)((in (T1,T2,T3,T4,T5,T6,T7,T8) row) => row.Item5),
                5 => (Table.Projection<(T1,T2,T3,T4,T5,T6,T7,T8), T6>)((in (T1,T2,T3,T4,T5,T6,T7,T8) row) => row.Item6),
                6 => (Table.Projection<(T1,T2,T3,T4,T5,T6,T7,T8), T7>)((in (T1,T2,T3,T4,T5,T6,T7,T8) row) => row.Item7),
                7 => (Table.Projection<(T1,T2,T3,T4,T5,T6,T7,T8), T8>)((in (T1,T2,T3,T4,T5,T6,T7,T8) row) => row.Item8),
                _ => throw new ArgumentException($"There is no column number {columnNumber} in this table")
            };
        }

        /// <inheritdoc />
        public override Delegate Mutator(int columnNumber)
        {
            return columnNumber switch
            {
                0 => (Table.Mutator<(T1,T2,T3,T4,T5,T6,T7,T8), T1>)((ref (T1,T2,T3,T4,T5,T6,T7,T8) row, in T1 value) => row.Item1 = value),
                1 => (Table.Mutator<(T1,T2,T3,T4,T5,T6,T7,T8), T2>)((ref (T1,T2,T3,T4,T5,T6,T7,T8) row, in T2 value) => row.Item2 = value),
                2 => (Table.Mutator<(T1,T2,T3,T4,T5,T6,T7,T8), T3>)((ref (T1,T2,T3,T4,T5,T6,T7,T8) row, in T3 value) => row.Item3 = value),
                3 => (Table.Mutator<(T1,T2,T3,T4,T5,T6,T7,T8), T4>)((ref (T1,T2,T3,T4,T5,T6,T7,T8) row, in T4 value) => row.Item4 = value),
                4 => (Table.Mutator<(T1,T2,T3,T4,T5,T6,T7,T8), T5>)((ref (T1,T2,T3,T4,T5,T6,T7,T8) row, in T5 value) => row.Item5 = value),
                5 => (Table.Mutator<(T1,T2,T3,T4,T5,T6,T7,T8), T6>)((ref (T1,T2,T3,T4,T5,T6,T7,T8) row, in T6 value) => row.Item6 = value),
                6 => (Table.Mutator<(T1,T2,T3,T4,T5,T6,T7,T8), T7>)((ref (T1,T2,T3,T4,T5,T6,T7,T8) row, in T7 value) => row.Item7 = value),
                7 => (Table.Mutator<(T1,T2,T3,T4,T5,T6,T7,T8), T8>)((ref (T1,T2,T3,T4,T5,T6,T7,T8) row, in T8 value) => row.Item8 = value),
                _ => throw new ArgumentException($"There is no column number {columnNumber} in this table")
            };
        }
        #endregion

        /// <inheritdoc />
        public override ColumnAccessor<TColumn, TKey> Accessor<TColumn, TKey>(Var<TKey> key, Var<TColumn> column)
            => Accessor(_table, key, column);

        public override Comparison<uint> RowComparison(int columnNumber) => columnNumber switch {
            0 => RowComparison<(T1,T2,T3,T4,T5,T6,T7,T8),T1>(_table, columnNumber),
            1 => RowComparison<(T1,T2,T3,T4,T5,T6,T7,T8),T2>(_table, columnNumber),
            2 => RowComparison<(T1,T2,T3,T4,T5,T6,T7,T8),T3>(_table, columnNumber),
            3 => RowComparison<(T1,T2,T3,T4,T5,T6,T7,T8),T4>(_table, columnNumber),
            4 => RowComparison<(T1,T2,T3,T4,T5,T6,T7,T8),T5>(_table, columnNumber),
            5 => RowComparison<(T1,T2,T3,T4,T5,T6,T7,T8),T6>(_table, columnNumber),
            6 => RowComparison<(T1,T2,T3,T4,T5,T6,T7,T8),T7>(_table, columnNumber),
            7 => RowComparison<(T1,T2,T3,T4,T5,T6,T7,T8),T8>(_table, columnNumber),
            _ => throw new ArgumentException($"There is no column number {columnNumber} in table {Name}")
        };

        public override Func<uint, TColumn> ColumnValueFromRowNumber<TColumn>(Var<TColumn> column) {
            var columnNumber = ColumnPositionOfDefaultVariable(column);
            return columnNumber switch {
                0 => (Func<uint, TColumn>)(Delegate)(Func<uint,T1>)(rowNum => _table.Data[rowNum].Item1),
                1 => (Func<uint, TColumn>)(Delegate)(Func<uint,T2>)(rowNum => _table.Data[rowNum].Item2),
                2 => (Func<uint, TColumn>)(Delegate)(Func<uint,T3>)(rowNum => _table.Data[rowNum].Item3),
                3 => (Func<uint, TColumn>)(Delegate)(Func<uint,T4>)(rowNum => _table.Data[rowNum].Item4),
                4 => (Func<uint, TColumn>)(Delegate)(Func<uint,T5>)(rowNum => _table.Data[rowNum].Item5),
                5 => (Func<uint, TColumn>)(Delegate)(Func<uint,T6>)(rowNum => _table.Data[rowNum].Item6),
                6 => (Func<uint, TColumn>)(Delegate)(Func<uint,T7>)(rowNum => _table.Data[rowNum].Item7),
                7 => (Func<uint, TColumn>)(Delegate)(Func<uint,T8>)(rowNum => _table.Data[rowNum].Item8),
                _ => throw new ArgumentException($"There is no column number {columnNumber} in table {Name}")
            };
        }
    }
}
