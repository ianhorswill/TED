using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

// ReSharper disable UnusedMember.Global

namespace TED
{
    /// <summary>
    /// Untyped base class for TablePredicates
    /// These are predicates that store an explicit list (table) of their extensions (all their ground instances, aka rows)
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public abstract class TablePredicate : Predicate
    {
        /// <summary>
        /// Make a new predicate
        /// </summary>
        protected TablePredicate(string name, params IColumnSpec[] columns) : base(name)
        {
            Simulation.AddToCurrentSimulation(this);
            DefaultVariables = columns.Select(spec => spec.UntypedVariable).ToArray();
            ColumnHeadings = DefaultVariables.Select(v => v.ToString()).ToArray();
            for (var i = 0; i < columns.Length; i++)
                switch (columns[i].IndexMode)
                {
                    case IndexMode.Key:
                        IndexByKey(i);
                        break;

                    case IndexMode.NonKey:
                        IndexBy(i);
                        break;
                }
        }

        internal abstract Table TableUntyped { get; }

        /// <summary>
        /// If true, the underlying table enforces uniqueness of row/tuples by indexing them with a hashtable.
        /// </summary>
        public bool Unique
        {
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
        public TableGoal this[Term[] args]
        {
            get
            {
                if (args.Length != ColumnHeadings.Length)
                    throw new ArgumentException($"Wrong number of arguments to predicate {Name}");
                return GetGoal(args);
            }
        }

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
        public void IndexByKey(Term column) => AddIndex(column, true);

        /// <summary>
        /// Add an index; the column isn't a key, i.e. rows aren't assumed to have unique values for this column
        /// </summary>
        public void IndexBy(int columnIndex) => AddIndex(columnIndex, false);

        /// <summary>
        /// Add an index; the column isn't a key, i.e. rows aren't assumed to have unique values for this column
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public void IndexBy(Term column) => AddIndex(column, false);

        private void AddIndex(Term t, bool keyIndex)
        {
            var index = ColumnPositionOfDefaultVariable(t);
            AddIndex(index, keyIndex);
        }

        /// <summary>
        /// Find the column/argument position of an argument, given the variable used to declare it.
        /// </summary>
        protected int ColumnPositionOfDefaultVariable(Term t)
        {
            if (DefaultVariables == null)
                throw new InvalidOperationException(
                    $"No default arguments defined for table {Name}, so no known column {t}");
            var index = Array.IndexOf(DefaultVariables, t);
            if (index < 0)
                throw new ArgumentException($"Table {Name} has no column named {t}");
            return index;
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
        internal TableIndex? IndexFor(int columnIndex, bool key) 
            => TableUntyped.Indices.FirstOrDefault(i => i.ColumnNumber == columnIndex && key == i.IsKey);

        /// <summary>
        /// All indices for the table
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public IEnumerable<TableIndex> Indices => TableUntyped.Indices;

        /// <summary>
        /// Rules that can be used to prove goals involving this predicate
        /// </summary>
        public List<Rule>? Rules;

        #if PROFILER
        /// <summary>
        /// Combined average execution time of all the rules for this predicate.
        /// </summary>
        public float RuleExecutionTime => Rules == null?0:Rules.Select(r => r.AverageExecutionTime).Sum();
        #endif

        /// <summary>
        /// A TablePredicate is "intensional" if it's defined by rules.  Otherwise it's "extensional"
        /// </summary>
        public bool IsIntensional => Rules != null;

        /// <summary>
        /// A predicate is "extensional" if it is defined by directly specifying its extension (the set of it instances/rows)
        /// using AddRow.  If it's defined by rules, it's "intensional".
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public bool IsExtensional => Rules == null;

        /// <summary>
        /// Remove all data from the predicate's table
        /// </summary>
        public void Clear()
        {
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
        internal void EnsureUpToDate()
        {
            if (MustRecompute)
            {
                Clear();
                foreach (var r in Rules!)
                    r.AddAllSolutions();
                MustRecompute = false;
            }
        }

        /// <summary>
        /// Force the re-computation of an intensional table
        /// </summary>
        public void ForceRecompute()
        {
            if (IsIntensional)
                MustRecompute = true;
        }

        /// <summary>
        /// Add a rule to an intensional predicate
        /// </summary>
        internal void AddRule(Rule r)
        {
            if (!r.Head.IsInstantiated)
                throw new InstantiationException("Rules in table predicates must have fully instantiated heads");
            Rules ??= new List<Rule>();
            Rules.Add(r);
            MustRecompute = true;
        }

        /// <summary>
        /// Add a rule that concludes the default arguments of this predicate
        /// </summary>
        protected void AddRule(params Goal[] body) => DefaultGoal.If(body);

        /// <summary>
        /// Null-tolerant version of ToString.
        /// </summary>
        protected string Stringify<T>(in T value) => value == null ? "null" : value.ToString();

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
        /// Get the RowToStrings output for every row in the table as a List of string arrays
        /// </summary>
        public List<string[]> TableToStrings()
        {
            var list = new List<string[]>();
            for (var i = 0u; i < Length; i++) {
                var buffer = new string[ColumnHeadings.Length];
                RowToStrings(i, buffer);
                list.Add(buffer); }
            return list;
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
        /// If you put a predicate in a rule body without arguments, it defaults to the rule's "default" arguments.
        /// </summary>
        public static implicit operator Goal(TablePredicate p) => p.DefaultGoal;

        /// <summary>
        /// Verify that the header row of a CSV file matches the declared variable names
        /// </summary>
        protected static void VerifyCsvColumnNames(string name, string[] headerRow, params IColumnSpec[] args)
        {
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
        protected Term<T> CastArg<T>(Term arg, int position)
        {
            if (arg is Term<T> result)
                return result;
            throw new ArgumentException($"Argument {position} to {Name} should be of type {typeof(T).Name}");
        }
    }

    /// <summary>
    /// A 1-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of the predicate's argument</typeparam>
    public class TablePredicate<T1> : TablePredicate, IEnumerable<T1>
    {
        /// <summary>
        /// Make a Goal from this predicate with the specified argument value.
        /// </summary>
        public TableGoal<T1> this[Term<T1> arg1] => new TableGoal<T1>(this, arg1);

        /// <inheritdoc />
        public override TableGoal GetGoal(Term[] args) => this[CastArg<T1>(args[0], 1)];

        /// <inheritdoc />
        protected override void AddIndex(int columnIndex, bool keyIndex)
        {
            throw new NotImplementedException("Single-column tables shouldn't be indexed.  Instead, set the Unique property to true.");
        }

        /// <summary>
        /// Add a rule to the predicate, using its default arguments as the head
        /// </summary>
        /// <param name="body">Antecedents for the rule</param>
        /// <returns>The original predicate (so these can be chained)</returns>
        public TablePredicate<T1> If(params Goal[] body)
        {
            AddRule(body);
            return this;
        }

        /// <summary>
        /// Make a new table predicate with the specified name
        /// </summary>
        /// <param name="name">Name of the predicate</param>
        /// <param name="arg1">Default variable for the first argument</param>
        public TablePredicate(string name, IColumnSpec<T1> arg1) : base(name, arg1)
        {
        }

        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="name">Predicate name</param>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">Default argument to the predicate</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1> FromCsv(string name, string path, IColumnSpec<T1> arg1)
        {
            var (header, data) = CsvReader.ReadCsv(path);
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
            get
            {
                EnsureUpToDate();
                return _table;
            }
        }

        internal override Table TableUntyped => _table;

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
        public void AddRows(IEnumerable<T1> generator)
        {
            var t = Table;
            foreach (var r in generator) t.Add(r);
        }

        // Test rig; do not use.
        internal IEnumerable<T1> Match(Pattern<T1> pat)
        {
            var t = Table;
            for (var i = 0u; i < t.Length; i++)
            {
                var e = t.PositionReference(i);
                if (pat.Match(e))
                    yield return e;
            }
        }

        /// <summary>
        /// Convert the columns of the specified row to strings and write them to buffer
        /// </summary>
        public override void RowToStrings(uint rowNumber, string[] buffer)
        {
            var r = Table.PositionReference(rowNumber);
            buffer[0] = Stringify(r);
        }

        /// <summary>
        /// Call method on every row of the table, passing it a reference so it can rewrite it as it likes
        /// </summary>
        /// <param name="u"></param>
        public void UpdateRows(Update<T1> u)
        {
            for (var i = 0u; i < _table.Length; i++)
                u(ref _table.PositionReference(i));
        }

        /// <summary>
        /// Add rows of t to rows of this predicate
        /// </summary>
        public void Append(TablePredicate<T1> t)
        {
            for (var i = 0u; i < t._table.Length; i++)
                AddRow(t._table.PositionReference(i));
        }

        private List<TablePredicate<T1>>? inputs;

        /// <summary>
        /// Declare that on each simulation tick, the contents of the specified tables should be appended to this table.
        /// </summary>
        /// <param name="input">Predicate to append to this predicate on each tick</param>
        /// <returns>This predicate</returns>
        public TablePredicate<T1> Accumulates(TablePredicate<T1> input)
        {
            inputs ??= new List<TablePredicate<T1>>();
            inputs.Add(input);
            return this;
        }

        internal override void AppendInputs()
        {
            if (inputs != null)
                foreach (var input in inputs) Append(input);
        }

        /// <inheritdoc />
        public IEnumerator<T1> GetEnumerator() => Table.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
        public TableGoal<T1, T2> this[Term<T1> arg1, Term<T2> arg2] => new TableGoal<T1, T2>(this, arg1, arg2);

        /// <inheritdoc />
        public override TableGoal GetGoal(Term[] args) => this[CastArg<T1>(args[0], 1), CastArg<T2>(args[1], 2)];

        /// <inheritdoc />
        protected override void AddIndex(int columnIndex, bool keyIndex)
        {
            switch (columnIndex)
            {
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
        public TablePredicate(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2) : base(name, arg1, arg2)
        { }

        // ReSharper disable once InconsistentNaming
        internal readonly Table<(T1,T2)> _table = new Table<(T1, T2)>();

        internal Table<(T1, T2)> Table  {
            get
            {
                EnsureUpToDate();
                return _table;
            }
        }

        internal override Table TableUntyped => _table;

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
        public void AddRows(IEnumerable<(T1, T2)> generator)
        {
            var t = Table;
            foreach (var r in generator) t.Add(r);
        }

        internal IEnumerable<(T1, T2)> Match(Pattern<T1, T2> pat)
        {
            var t = Table;
            for (var i = 0u; i < t.Length; i++)
            {
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
        public static TablePredicate<T1, T2> FromCsv(string name, string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2)
        {
            var (header, data) = CsvReader.ReadCsv(path);
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
        public override void RowToStrings(uint rowNumber, string[] buffer)
        {
            var r = Table.PositionReference(rowNumber);
            buffer[0] = Stringify(r.Item1);
            buffer[1] = Stringify(r.Item2);
        }
        
        /// <summary>
        /// Call method on every row of the table, passing it a reference so it can rewrite it as it likes
        /// </summary>
        /// <param name="u"></param>
        public void UpdateRows(Update<(T1, T2)> u)
        {
            for (var i = 0u; i < _table.Length; i++)
                u(ref _table.PositionReference(i));
        }

        /// <summary>
        /// Add rows of t to rows of this predicate
        /// </summary>
        public void Append(TablePredicate<T1, T2> t)
        {
            for (var i = 0u; i < t._table.Length; i++)
            {
                var row = t._table.PositionReference(i);
                AddRow(row.Item1, row.Item2);
            }
        }

        /// <summary>
        /// Add a rule using the default arguments as the head.
        /// </summary>
        /// <param name="body">subgoals</param>
        /// <returns>the original predicate</returns>
        public TablePredicate<T1, T2> If(params Goal[] body)
        {
            AddRule(body);
            return this;
        }
        
        private List<TablePredicate<T1, T2>>? inputs;

        /// <summary>
        /// Declare that on each simulation tick, the contents of the specified tables should be appended to this table.
        /// </summary>
        /// <param name="input">Predicate to append to this predicate on each tick</param>
        /// <returns>This predicate</returns>
        public TablePredicate<T1, T2> Accumulates(TablePredicate<T1, T2> input)
        {
            inputs ??= new List<TablePredicate<T1, T2>>();
            inputs.Add(input);
            return this;
        }

        internal override void AppendInputs()
        {
            if (inputs != null)
                foreach (var input in inputs) Append(input);
        }

        /// <summary>
        /// Get the index for the specified key
        /// </summary>
        /// <typeparam name="T">Type of the indexed column</typeparam>
        /// <param name="column">Default variable representing the column</param>
        /// <returns>The index</returns>
        /// <exception cref="InvalidOperationException">If there is no index or it's not a key index</exception>
        public KeyIndex<(T1, T2), T> KeyIndex<T>(Var<T> column)
        {
            var i = IndexFor(ColumnPositionOfDefaultVariable(column), true);
            if (i == null)
                throw new InvalidOperationException($"No key index defined for {column}");
            return (KeyIndex<(T1, T2), T>)i;
        }

        /// <summary>
        /// Get the index for the specified key
        /// </summary>
        /// <typeparam name="T">Type of the indexed column</typeparam>
        /// <param name="column">Position of the column</param>
        /// <returns>The index</returns>
        /// <exception cref="InvalidOperationException">If there is no index or it's not a key index</exception>
        public KeyIndex<(T1, T2), T> KeyIndex<T>(int column)
        {
            var i = IndexFor(column, true);
            if (i == null)
                throw new InvalidOperationException($"No key index defined for column {column}");
            return (KeyIndex<(T1, T2), T>)i;
        }

        /// <inheritdoc />
        public IEnumerator<(T1, T2)> GetEnumerator() => Table.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// A 3-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of the predicate's 1st argument</typeparam>
    /// <typeparam name="T2">Type of the predicate's 2nd argument</typeparam>
    /// <typeparam name="T3">Type of the predicate's 3rd argument</typeparam>
    public class TablePredicate<T1, T2, T3> : TablePredicate, IEnumerable<(T1,T2,T3)>
    {
        /// <summary>
        /// Make a Goal from this predicate with the specified argument value.
        /// </summary>
        public TableGoal<T1, T2, T3> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3] => new TableGoal<T1, T2, T3>(this, arg1, arg2,arg3);

        /// <inheritdoc />
        public override TableGoal GetGoal(Term[] args) 
            => this[CastArg<T1>(args[0], 1), CastArg<T2>(args[1], 2), CastArg<T3>(args[2], 3)];

        /// <inheritdoc />
        protected override void AddIndex(int columnIndex, bool keyIndex)
        {
            switch (columnIndex)
            {
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
        public KeyIndex<(T1, T2, T3), T> KeyIndex<T>(Var<T> column)
        {
            var i = IndexFor(ColumnPositionOfDefaultVariable(column), true);
            if (i == null)
                throw new InvalidOperationException($"No key index defined for {column}");
            return (KeyIndex<(T1, T2, T3), T>)i;
        }

        /// <summary>
        /// Get the index for the specified key
        /// </summary>
        /// <typeparam name="T">Type of the indexed column</typeparam>
        /// <param name="column">Position of column</param>
        /// <returns>The index</returns>
        /// <exception cref="InvalidOperationException">If there is no index or it's not a key index</exception>
        public KeyIndex<(T1, T2, T3), T> KeyIndex<T>(int column)
        {
            var i = IndexFor(column, true);
            if (i == null)
                throw new InvalidOperationException($"No key index defined for column {column}");
            return (KeyIndex<(T1, T2, T3), T>)i;
        }

        /// <summary>
        /// Make a new table predicate with the specified name
        /// </summary>
        /// <param name="name">Name of the predicate</param>
        /// <param name="arg1">Default variable for the first argument</param>
        /// <param name="arg2">Default variable for the second argument</param>
        /// <param name="arg3">Default variable for the third argument</param>
        public TablePredicate(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3) : base(name, arg1, arg2, arg3)
        { }
        
        // ReSharper disable once InconsistentNaming
        internal readonly Table<(T1,T2, T3)> _table = new Table<(T1, T2, T3)>();

        internal Table<(T1, T2, T3)> Table  {
            get
            {
                EnsureUpToDate();
                return _table;
            }
        }

        internal override Table TableUntyped => _table;
        
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
        public void AddRows(IEnumerable<(T1, T2, T3)> generator)
        {
            var t = Table;
            foreach (var r in generator) t.Add(r);
        }

        internal IEnumerable<(T1, T2, T3)> Match(Pattern<T1, T2, T3> pat)
        {
            var t = Table;
            for (var i = 0u; i < t.Length; i++)
            {
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
        public static TablePredicate<T1, T2, T3> FromCsv(string name, string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3)
        {
            var (header, data) = CsvReader.ReadCsv(path);
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
        public override void RowToStrings(uint rowNumber, string[] buffer)
        {
            var r = Table.PositionReference(rowNumber);
            buffer[0] = Stringify(r.Item1);
            buffer[1] = Stringify(r.Item2);
            buffer[2] = Stringify(r.Item3);
        }

        /// <summary>
        /// Call method on every row of the table, passing it a reference so it can rewrite it as it likes
        /// </summary>
        /// <param name="u"></param>
        public void UpdateRows(Update<(T1, T2, T3)> u)
        {
            for (var i = 0u; i < _table.Length; i++)
                u(ref _table.PositionReference(i));
        }

        /// <summary>
        /// Add rows of t to rows of this predicate
        /// </summary>
        public void Append(TablePredicate<T1, T2, T3> t)
        {
            for (var i = 0u; i < t._table.Length; i++)
            {
                var row = t._table.PositionReference(i);
                AddRow(row.Item1, row.Item2, row.Item3);
            }
        }

        /// <summary>
        /// Add a rule using the default arguments as the head.
        /// </summary>
        /// <param name="body">subgoals</param>
        /// <returns>the original predicate</returns>
        public TablePredicate<T1, T2, T3> If(params Goal[] body)
        {
            AddRule(body);
            return this;
        }

        private List<TablePredicate<T1, T2, T3>>? inputs;

        /// <summary>
        /// Declare that on each simulation tick, the contents of the specified tables should be appended to this table.
        /// </summary>
        /// <param name="input">Predicate to append to this predicate on each tick</param>
        /// <returns>This predicate</returns>
        public TablePredicate<T1, T2, T3> Accumulates(TablePredicate<T1, T2, T3> input)
        {
            inputs ??= new List<TablePredicate<T1, T2, T3>>();
            inputs.Add(input);
            return this;
        }

        internal override void AppendInputs()
        {
            if (inputs != null)
                foreach (var input in inputs) Append(input);
        }

        /// <inheritdoc />
        public IEnumerator<(T1, T2, T3)> GetEnumerator() => Table.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// A 4-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of the predicate's 1st argument</typeparam>
    /// <typeparam name="T2">Type of the predicate's 2nd argument</typeparam>
    /// <typeparam name="T3">Type of the predicate's 3rd argument</typeparam>
    /// <typeparam name="T4">Type of the predicate's 4th argument</typeparam>
    public class TablePredicate<T1, T2, T3, T4> : TablePredicate, IEnumerable<(T1,T2,T3,T4)>
    {
        /// <summary>
        /// Make a Goal from this predicate with the specified argument value.
        /// </summary>
        public TableGoal<T1, T2, T3, T4> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4] 
            => new TableGoal<T1, T2, T3, T4>(this, arg1, arg2,arg3, arg4);

        /// <inheritdoc />
        public override TableGoal GetGoal(Term[] args)
            => this[CastArg<T1>(args[0], 1), CastArg<T2>(args[1], 2), CastArg<T3>(args[2], 3), CastArg<T4>(args[3], 4)];

        /// <inheritdoc />
        protected override void AddIndex(int columnIndex, bool keyIndex)
        {
            switch (columnIndex)
            {
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
        public KeyIndex<(T1, T2, T3, T4), TKey> KeyIndex<TKey>(Var<TKey> column)
        {
            var i = IndexFor(ColumnPositionOfDefaultVariable(column), true);
            if (i == null)
                throw new InvalidOperationException($"No key index defined for {column}");
            return (KeyIndex<(T1, T2, T3, T4), TKey>)i;
        }
        
        /// <summary>
        /// Get the index for the specified key
        /// </summary>
        /// <typeparam name="TKey">Type of the indexed column</typeparam>
        /// <param name="column">Position the column</param>
        /// <returns>The index</returns>
        /// <exception cref="InvalidOperationException">If there is no index or it's not a key index</exception>
        public KeyIndex<(T1, T2, T3, T4), TKey> KeyIndex<TKey>(int column)
        {
            var i = IndexFor(column, true);
            if (i == null)
                throw new InvalidOperationException($"No key index defined for column {column}");
            return (KeyIndex<(T1, T2, T3, T4), TKey>)i;
        }
        
        /// <summary>
        /// Make a new table predicate with the specified name
        /// </summary>
        /// <param name="name">Name of the predicate</param>
        /// <param name="arg1">Default variable for the first argument</param>
        /// <param name="arg2">Default variable for the second argument</param>
        /// <param name="arg3">Default variable for the third argument</param>
        /// <param name="arg4">Default variable for the fourth argument</param>
        public TablePredicate(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4) : base(name, arg1, arg2, arg3, arg4)
        { }
        
        // ReSharper disable once InconsistentNaming
        internal readonly Table<(T1,T2, T3, T4)> _table = new Table<(T1, T2, T3, T4)>();

        internal Table<(T1, T2, T3, T4)> Table  {
            get
            {
                EnsureUpToDate();
                return _table;
            }
        }

        internal override Table TableUntyped => _table;

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
        public void AddRows(IEnumerable<(T1, T2, T3, T4)> generator)
        {
            var t = Table;
            foreach (var r in generator) t.Add(r);
        }

        internal IEnumerable<(T1, T2, T3, T4)> Match(Pattern<T1, T2, T3, T4> pat)
        {
            var t = Table;
            for (var i = 0u; i < t.Length; i++)
            {
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
        public static TablePredicate<T1, T2, T3, T4> FromCsv(string name, string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4)
        {
            var (header, data) = CsvReader.ReadCsv(path);
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
        public override void RowToStrings(uint rowNumber, string[] buffer)
        {
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
        public void UpdateRows(Update<(T1, T2, T3, T4)> u)
        {
            for (var i = 0u; i < _table.Length; i++)
                u(ref _table.PositionReference(i));
        }

        /// <summary>
        /// Add rows of t to rows of this predicate
        /// </summary>
        public void Append(TablePredicate<T1, T2, T3, T4> t)
        {
            for (var i = 0u; i < t._table.Length; i++)
            {
                var row = t._table.PositionReference(i);
                AddRow(row.Item1, row.Item2, row.Item3, row.Item4);
            }
        }

        /// <summary>
        /// Add a rule using the default arguments as the head.
        /// </summary>
        /// <param name="body">subgoals</param>
        /// <returns>the original predicate</returns>
        public TablePredicate<T1, T2, T3, T4> If(params Goal[] body)
        {
            AddRule(body);
            return this;
        }

        private List<TablePredicate<T1, T2, T3, T4>>? inputs;

        /// <summary>
        /// Declare that on each simulation tick, the contents of the specified tables should be appended to this table.
        /// </summary>
        /// <param name="input">Predicate to append to this predicate on each tick</param>
        /// <returns>This predicate</returns>
        public TablePredicate<T1, T2, T3, T4> Accumulates(TablePredicate<T1, T2, T3, T4> input)
        {
            inputs ??= new List<TablePredicate<T1, T2, T3, T4>>();
            inputs.Add(input);
            return this;
        }

        internal override void AppendInputs()
        {
            if (inputs != null)
                foreach (var input in inputs) Append(input);
        }

        /// <inheritdoc />
        public IEnumerator<(T1, T2, T3, T4)> GetEnumerator() => Table.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
    public class TablePredicate<T1, T2, T3, T4, T5> : TablePredicate, IEnumerable<(T1,T2,T3,T4,T5)>
    {
        /// <summary>
        /// Make a Goal from this predicate with the specified argument value.
        /// </summary>
        public TableGoal<T1, T2, T3, T4, T5> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5] 
            => new TableGoal<T1, T2, T3, T4, T5>(this, arg1, arg2,arg3, arg4, arg5);

        /// <inheritdoc />
        public override TableGoal GetGoal(Term[] args)
            => this[CastArg<T1>(args[0], 1), CastArg<T2>(args[1], 2), CastArg<T3>(args[2], 3), CastArg<T4>(args[3], 4), CastArg<T5>(args[4], 5)];

        /// <inheritdoc />
        protected override void AddIndex(int columnIndex, bool keyIndex)
        {
            switch (columnIndex)
            {
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
        public KeyIndex<(T1, T2, T3, T4, T5), TKey> KeyIndex<TKey>(Var<TKey> column)
        {
            var i = IndexFor(ColumnPositionOfDefaultVariable(column), true);
            if (i == null)
                throw new InvalidOperationException($"No key index defined for {column}");
            return (KeyIndex<(T1, T2, T3, T4, T5), TKey>)i;
        }
        
        /// <summary>
        /// Get the index for the specified key
        /// </summary>
        /// <typeparam name="TKey">Type of the indexed column</typeparam>
        /// <param name="column">Position of the column</param>
        /// <returns>The index</returns>
        /// <exception cref="InvalidOperationException">If there is no index or it's not a key index</exception>
        public KeyIndex<(T1, T2, T3, T4, T5), TKey> KeyIndex<TKey>(int column)
        {
            var i = IndexFor(column, true);
            if (i == null)
                throw new InvalidOperationException($"No key index defined for column {column}");
            return (KeyIndex<(T1, T2, T3, T4, T5), TKey>)i;
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
        public TablePredicate(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5) 
            : base(name, arg1, arg2, arg3, arg4, arg5)
        { }

        // ReSharper disable once InconsistentNaming
        internal readonly Table<(T1,T2, T3, T4, T5)> _table = new Table<(T1, T2, T3, T4, T5)>();

        internal Table<(T1, T2, T3, T4, T5)> Table  {
            get
            {
                EnsureUpToDate();
                return _table;
            }
        }

        internal override Table TableUntyped => _table;
        
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
        public void AddRows(IEnumerable<(T1, T2, T3, T4, T5)> generator)
        {
            var t = Table;
            foreach (var r in generator) t.Add(r);
        }

        internal IEnumerable<(T1, T2, T3, T4, T5)> Match(Pattern<T1, T2, T3, T4, T5> pat)
        {
            var t = Table;
            for (var i = 0u; i < t.Length; i++)
            {
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
        public static TablePredicate<T1, T2, T3, T4, T5> FromCsv(string name, string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5)
        {
            var (header, data) = CsvReader.ReadCsv(path);
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
        public override void RowToStrings(uint rowNumber, string[] buffer)
        {
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
        public void UpdateRows(Update<(T1, T2, T3, T4, T5)> u)
        {
            for (var i = 0u; i < _table.Length; i++)
                u(ref _table.PositionReference(i));
        }

        /// <summary>
        /// Add rows of t to rows of this predicate
        /// </summary>
        public void Append(TablePredicate<T1, T2, T3, T4, T5> t)
        {
            for (var i = 0u; i < t._table.Length; i++)
            {
                var row = t._table.PositionReference(i);
                AddRow(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5);
            }
        }

        /// <summary>
        /// Add a rule using the default arguments as the head.
        /// </summary>
        /// <param name="body">subgoals</param>
        /// <returns>the original predicate</returns>
        public TablePredicate<T1, T2, T3, T4, T5> If(params Goal[] body)
        {
            AddRule(body);
            return this;
        }

        private List<TablePredicate<T1, T2, T3, T4, T5>>? inputs;

        /// <summary>
        /// Declare that on each simulation tick, the contents of the specified tables should be appended to this table.
        /// </summary>
        /// <param name="input">Predicate to append to this predicate on each tick</param>
        /// <returns>This predicate</returns>
        public TablePredicate<T1, T2, T3, T4, T5> Accumulates(TablePredicate<T1, T2, T3, T4, T5> input)
        {
            inputs ??= new List<TablePredicate<T1, T2, T3, T4, T5>>();
            inputs.Add(input);
            return this;
        }

        internal override void AppendInputs()
        {
            if (inputs != null)
                foreach (var input in inputs) Append(input);
        }

        /// <inheritdoc />
        public IEnumerator<(T1, T2, T3, T4, T5)> GetEnumerator() => Table.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
    public class TablePredicate<T1, T2, T3, T4, T5, T6> : TablePredicate, IEnumerable<(T1,T2,T3,T4,T5,T6)>
    {
        /// <summary>
        /// Make a Goal from this predicate with the specified argument value.
        /// </summary>
        public TableGoal<T1, T2, T3, T4, T5, T6> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5, Term<T6> arg6] 
            => new TableGoal<T1, T2, T3, T4, T5, T6>(this, arg1, arg2,arg3, arg4, arg5, arg6);

        /// <inheritdoc />
        public override TableGoal GetGoal(Term[] args)
            => this[CastArg<T1>(args[0], 1), CastArg<T2>(args[1], 2), CastArg<T3>(args[2], 3),
                CastArg<T4>(args[3], 4), CastArg<T5>(args[4], 5), CastArg<T6>(args[5], 6)];

        /// <inheritdoc />
        protected override void AddIndex(int columnIndex, bool keyIndex)
        {
            switch (columnIndex)
            {
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
        public KeyIndex<(T1, T2, T3, T4, T5, T6), TKey> KeyIndex<TKey>(Var<TKey> column)
        {
            var i = IndexFor(ColumnPositionOfDefaultVariable(column), true);
            if (i == null)
                throw new InvalidOperationException($"No key index defined for {column}");
            return (KeyIndex<(T1, T2, T3, T4, T5, T6), TKey>)i;
        }
        
        /// <summary>
        /// Get the index for the specified key
        /// </summary>
        /// <typeparam name="TKey">Type of the indexed column</typeparam>
        /// <param name="column">Position of the column</param>
        /// <returns>The index</returns>
        /// <exception cref="InvalidOperationException">If there is no index or it's not a key index</exception>
        public KeyIndex<(T1, T2, T3, T4, T5, T6), TKey> KeyIndex<TKey>(int column)
        {
            var i = IndexFor(column, true);
            if (i == null)
                throw new InvalidOperationException($"No key index defined for column {column}");
            return (KeyIndex<(T1, T2, T3, T4, T5, T6), TKey>)i;
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
        public TablePredicate(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6) 
            : base(name, arg1, arg2, arg3, arg4, arg5, arg6)
        { }

        // ReSharper disable once InconsistentNaming
        internal readonly Table<(T1,T2, T3, T4, T5, T6)> _table = new Table<(T1, T2, T3, T4, T5, T6)>();

        internal Table<(T1, T2, T3, T4, T5, T6)> Table  {
            get
            {
                EnsureUpToDate();
                return _table;
            }
        }

        internal override Table TableUntyped => _table;

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
        public void AddRows(IEnumerable<(T1, T2, T3, T4, T5, T6)> generator)
        {
            var t = Table;
            foreach (var r in generator) t.Add(r);
        }
        internal IEnumerable<(T1, T2, T3, T4, T5, T6)> Match(Pattern<T1, T2, T3, T4, T5, T6> pat)
        {
            var t = Table;
            for (var i = 0u; i < t.Length; i++)
            {
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
        public static TablePredicate<T1, T2, T3, T4, T5, T6> FromCsv(string name, string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6)
        {
            var (header, data) = CsvReader.ReadCsv(path);
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
        public override void RowToStrings(uint rowNumber, string[] buffer)
        {
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
        public void UpdateRows(Update<(T1, T2, T3, T4, T5, T6)> u)
        {
            for (var i = 0u; i < _table.Length; i++)
                u(ref _table.PositionReference(i));
        }

        /// <summary>
        /// Add rows of t to rows of this predicate
        /// </summary>
        public void Append(TablePredicate<T1, T2, T3, T4, T5, T6> t)
        {
            for (var i = 0u; i < t._table.Length; i++)
            {
                var row = t._table.PositionReference(i);
                AddRow(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6);
            }
        }

        /// <summary>
        /// Add a rule using the default arguments as the head.
        /// </summary>
        /// <param name="body">subgoals</param>
        /// <returns>the original predicate</returns>
        public TablePredicate<T1, T2, T3, T4, T5, T6> If(params Goal[] body)
        {
            AddRule(body);
            return this;
        }

        private List<TablePredicate<T1, T2, T3, T4, T5, T6>>? inputs;

        /// <summary>
        /// Declare that on each simulation tick, the contents of the specified tables should be appended to this table.
        /// </summary>
        /// <param name="input">Predicate to append to this predicate on each tick</param>
        /// <returns>This predicate</returns>
        public TablePredicate<T1, T2, T3, T4, T5, T6> Accumulates(TablePredicate<T1, T2, T3, T4, T5, T6> input)
        {
            inputs ??= new List<TablePredicate<T1, T2, T3, T4, T5, T6>>();
            inputs.Add(input);
            return this;
        }

        internal override void AppendInputs()
        {
            if (inputs != null)
                foreach (var input in inputs) Append(input);
        }

        /// <inheritdoc />
        public IEnumerator<(T1, T2, T3, T4, T5, T6)> GetEnumerator() => Table.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
        public TableGoal<T1, T2, T3, T4, T5, T6, T7> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5, Term<T6> arg6, Term<T7> arg7]
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
            if (i == null)
                throw new InvalidOperationException($"No key index defined for {column}");
            return (KeyIndex<(T1, T2, T3, T4, T5, T6, T7), TKey>)i;
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
            if (i == null)
                throw new InvalidOperationException($"No key index defined for column {column}");
            return (KeyIndex<(T1, T2, T3, T4, T5, T6, T7), TKey>)i;
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
        public TablePredicate(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7)
            : base(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7) { }

        // ReSharper disable once InconsistentNaming
        internal readonly Table<(T1, T2, T3, T4, T5, T6, T7)> _table = new Table<(T1, T2, T3, T4, T5, T6, T7)>();

        internal Table<(T1, T2, T3, T4, T5, T6, T7)> Table {
            get {
                EnsureUpToDate();
                return _table;
            }
        }

        internal override Table TableUntyped => _table;

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
            var (header, data) = CsvReader.ReadCsv(path);
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

        private List<TablePredicate<T1, T2, T3, T4, T5, T6, T7>>? inputs;

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

        internal override void AppendInputs() {
            if (inputs != null)
                foreach (var input in inputs) Append(input);
        }

        /// <inheritdoc />
        public IEnumerator<(T1, T2, T3, T4, T5, T6, T7)> GetEnumerator() => Table.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
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
        public TableGoal<T1, T2, T3, T4, T5, T6, T7, T8> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5, Term<T6> arg6, Term<T7> arg7, Term<T8> arg8]
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
            if (i == null)
                throw new InvalidOperationException($"No key index defined for {column}");
            return (KeyIndex<(T1, T2, T3, T4, T5, T6, T7, T8), TKey>)i;
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
            if (i == null)
                throw new InvalidOperationException($"No key index defined for column {column}");
            return (KeyIndex<(T1, T2, T3, T4, T5, T6, T7, T8), TKey>)i;
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
        public TablePredicate(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7, IColumnSpec<T8> arg8)
            : base(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) { }

        // ReSharper disable once InconsistentNaming
        internal readonly Table<(T1, T2, T3, T4, T5, T6, T7, T8)> _table = new Table<(T1, T2, T3, T4, T5, T6, T7, T8)>();

        internal Table<(T1, T2, T3, T4, T5, T6, T7, T8)> Table {
            get {
                EnsureUpToDate();
                return _table;
            }
        }

        internal override Table TableUntyped => _table;

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
            var (header, data) = CsvReader.ReadCsv(path);
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

        private List<TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8>>? inputs;

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

        internal override void AppendInputs() {
            if (inputs != null)
                foreach (var input in inputs) Append(input);
        }

        /// <inheritdoc />
        public IEnumerator<(T1, T2, T3, T4, T5, T6, T7, T8)> GetEnumerator() => Table.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
