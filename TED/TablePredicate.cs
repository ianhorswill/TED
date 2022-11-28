using System.Collections.Generic;
using System.IO;

namespace TED
{
    /// <summary>
    /// Untyped base class for TablePredicates
    /// These are predicates that store an explicit list (table) of their extensions (all their ground instances, aka rows)
    /// </summary>
    public abstract class TablePredicate : AnyPredicate
    {
        /// <summary>
        /// List of all the TablePredicates so we can find them all and update them
        /// </summary>
        public static readonly List<TablePredicate> AllTablePredicates = new List<TablePredicate>();
        
        /// <summary>
        /// Make a new predicate
        /// </summary>
        protected TablePredicate(string name, string[] columnHeadings) : base(name)
        {
            AllTablePredicates.Add(this);
            ColumnHeadings = columnHeadings;
        }

        /// <summary>
        /// Human-readable descriptions of columns
        /// </summary>
        public readonly string[] ColumnHeadings;
        
        /// <summary>
        /// Rules that can be used to prove goals involving this predicate
        /// </summary>
        internal List<AnyRule>? Rules;

        /// <summary>
        /// A TablePredicate is "intensional" if it's defined by rules.  Otherwise it's "extensional"
        /// </summary>
        public bool IsIntensional => Rules != null;

        /// <summary>
        /// A predicate is "extensional" if it is defined by directly specifying its extension (the set of it instances/rows)
        /// using AddRow.  If it's defined by rules, it's "intensional".
        /// </summary>
        public bool IsExtensional => Rules == null;

        /// <summary>
        /// Erase all the rows in the predicate's table
        /// </summary>
        protected abstract void ClearTable();

        /// <summary>
        /// TRue if we need to recompute the predicate's table
        /// </summary>
        protected bool MustRecompute;

        /// <summary>
        /// Compute the predicate's table if it hasn't already or if it's out of date
        /// </summary>
        internal void EnsureUpToDate()
        {
            if (MustRecompute)
            {
                ClearTable();
                foreach (var r in Rules!)
                    r.AddAllSolutions();
                MustRecompute = false;
            }
        }

        /// <summary>
        /// Force the recomputation of an intensional table
        /// </summary>
        public void ForceRecompute()
        {
            if (IsIntensional)
                MustRecompute = true;
        }

        /// <summary>
        /// Forcibly recompute all the predicates
        /// </summary>
        public static void RecomputeAll()
        {
            foreach (var p in AllTablePredicates)
                if (p.IsIntensional)
                    p.MustRecompute = true;
            foreach (var p in AllTablePredicates)
                p.EnsureUpToDate();
        }

        /// <summary>
        /// Add a rule to an intensional predicate
        /// </summary>
        internal void AddRule(AnyRule r)
        {
            Rules ??= new List<AnyRule>();
            Rules.Add(r);
            MustRecompute = true;
        }

        /// <summary>
        /// Null-tolerant version of ToString.
        /// </summary>
        protected string Stringify<T>(in T value) => value == null ? "null" : value.ToString();

        public virtual void RowToStrings(uint rowNumber, string[] buffer) {}

        public abstract int Length { get; }

        public delegate void Update<T>(ref T arg);
    }

    /// <summary>
    /// A 1-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of the predicate's argument</typeparam>
    public class TablePredicate<T1> : TablePredicate
    {
        /// <summary>
        /// Make a Goal from this predicate with the specified argument value.
        /// </summary>
        public TableGoal<T1> this[Term<T1> arg1] => new TableGoal<T1>(this, arg1);

        public TablePredicate(string name, string col1 = "col1") : base(name, new []{ col1 })
        {
        }

        // ReSharper disable once InconsistentNaming
        internal readonly Table<T1> _table = new Table<T1>();

        internal Table<T1> Table  {
            get
            {
                EnsureUpToDate();
                return _table;
            }
        }

        public override int Length => Table.Length;

        protected override void ClearTable() => _table.Clear();

        /// <summary>
        /// Manually add a row (ground instance) to the extension of the predicate
        /// This cannot be mixed with rules using If.  If you want to have rules for the predicate
        /// use Fact() instead of AddRow.
        /// </summary>
        public void AddRow(in T1 value) => Table.Add(value);

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
        /// Return all the rows of the table
        /// This allocates memory; do not use in inner lops
        /// </summary>
        public IEnumerable<T1> Rows => Table.Rows;

        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="name">Predicate name</param>
        /// <param name="path">Path to the CSV file</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1> FromCsv(string name, string path)
        {
            var (header, data) = CsvReader.ReadCsv(path);
            var p = new TablePredicate<T1>(name, header[0]);
            foreach (var row in data)
                p.AddRow(CsvReader.ConvertCell<T1>(row[0]));
            return p;
        }

        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="path">Path to the CSV file</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1> FromCsv(string path) => FromCsv(Path.GetFileNameWithoutExtension(path), path);

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
    }

    /// <summary>
    /// A 2-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of the predicate's 1st argument</typeparam>
    /// <typeparam name="T2">Type of the predicate's 2nd argument</typeparam>
    public class TablePredicate<T1, T2> : TablePredicate
    {
        /// <summary>
        /// Make a Goal from this predicate with the specified argument value.
        /// </summary>
        public TableGoal<T1, T2> this[Term<T1> arg1, Term<T2> arg2] => new TableGoal<T1, T2>(this, arg1, arg2);

        public TablePredicate(string name, string col1 = "col1", string col2 = "col2") 
            : base(name, new []{ col1, col2 })
        {
        }

        // ReSharper disable once InconsistentNaming
        internal readonly Table<(T1,T2)> _table = new Table<(T1, T2)>();

        internal Table<(T1, T2)> Table  {
            get
            {
                EnsureUpToDate();
                return _table;
            }
        }

        public override int Length => Table.Length;

        /// <summary>
        /// Manually add a row (ground instance) to the extension of the predicate
        /// This cannot be mixed with rules using If.  If you want to have rules for the predicate
        /// use Fact() instead of AddRow.
        /// </summary>
        public void AddRow(in T1 value1, in T2 value2) => Table.Add((value1, value2));

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

        protected override void ClearTable() => _table.Clear();

        /// <summary>
        /// Return all the rows of the table
        /// This allocates memory; do not use in inner lops
        /// </summary>
        public IEnumerable<(T1,T2)> Rows => Table.Rows;

        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="name">Predicate name</param>
        /// <param name="path">Path to the CSV file</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1, T2> FromCsv(string name, string path)
        {
            var (header, data) = CsvReader.ReadCsv(path);
            var p = new TablePredicate<T1, T2>(name, header[0], header[1]);
            foreach (var row in data)
                p.AddRow(CsvReader.ConvertCell<T1>(row[0]), CsvReader.ConvertCell<T2>(row[1]));
            return p;
        }

        
        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="path">Path to the CSV file</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1, T2> FromCsv(string path) => FromCsv(Path.GetFileNameWithoutExtension(path), path);

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

    }

    /// <summary>
    /// A 3-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of the predicate's 1st argument</typeparam>
    /// <typeparam name="T2">Type of the predicate's 2nd argument</typeparam>
    /// <typeparam name="T3">Type of the predicate's 3rd argument</typeparam>
    public class TablePredicate<T1, T2, T3> : TablePredicate
    {
        /// <summary>
        /// Make a Goal from this predicate with the specified argument value.
        /// </summary>
        public TableGoal<T1, T2, T3> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3] => new TableGoal<T1, T2, T3>(this, arg1, arg2,arg3);

        public TablePredicate(string name, string col1 = "col1", string col2 = "col2", string col3 = "col3")
            : base(name, new []{ col1, col2, col3 })
        {
        }

        // ReSharper disable once InconsistentNaming
        internal readonly Table<(T1,T2, T3)> _table = new Table<(T1, T2, T3)>();

        internal Table<(T1, T2, T3)> Table  {
            get
            {
                EnsureUpToDate();
                return _table;
            }
        }

        public override int Length => Table.Length;

        /// <summary>
        /// Manually add a row (ground instance) to the extension of the predicate
        /// This cannot be mixed with rules using If.  If you want to have rules for the predicate
        /// use Fact() instead of AddRow.
        /// </summary>
        public void AddRow(in T1 v1, in T2 v2, in T3 v3) => _table.Add((v1, v2, v3));

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
        
        protected override void ClearTable() => _table.Clear();

        /// <summary>
        /// Return all the rows of the table
        /// This allocates memory; do not use in inner lops
        /// </summary>
        public IEnumerable<(T1,T2, T3)> Rows => Table.Rows;

        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="name">Predicate name</param>
        /// <param name="path">Path to the CSV file</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1, T2, T3> FromCsv(string name, string path)
        {
            var (header, data) = CsvReader.ReadCsv(path);
            var p = new TablePredicate<T1, T2, T3>(name, header[0], header[1], header[2]);
            foreach (var row in data)
                p.AddRow(CsvReader.ConvertCell<T1>(row[0]), CsvReader.ConvertCell<T2>(row[1]), CsvReader.ConvertCell<T3>(row[2]));
            return p;
        }

        
        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="path">Path to the CSV file</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1, T2, T3> FromCsv(string path) => FromCsv(Path.GetFileNameWithoutExtension(path), path);

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
    }

    /// <summary>
    /// A 4-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of the predicate's 1st argument</typeparam>
    /// <typeparam name="T2">Type of the predicate's 2nd argument</typeparam>
    /// <typeparam name="T3">Type of the predicate's 3rd argument</typeparam>
    /// <typeparam name="T4">Type of the predicate's 4th argument</typeparam>
    public class TablePredicate<T1, T2, T3, T4> : TablePredicate
    {
        /// <summary>
        /// Make a Goal from this predicate with the specified argument value.
        /// </summary>
        public TableGoal<T1, T2, T3, T4> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4] 
            => new TableGoal<T1, T2, T3, T4>(this, arg1, arg2,arg3, arg4);

        public TablePredicate(string name, string col1 = "col1", string col2 = "col2", string col3 = "col3",
            string col4 = "col4") : base(name, new []{ col1, col2, col3, col4 })
        {
        }

        // ReSharper disable once InconsistentNaming
        internal readonly Table<(T1,T2, T3, T4)> _table = new Table<(T1, T2, T3, T4)>();

        internal Table<(T1, T2, T3, T4)> Table  {
            get
            {
                EnsureUpToDate();
                return _table;
            }
        }

        public override int Length => Table.Length;

        /// <summary>
        /// Manually add a row (ground instance) to the extension of the predicate
        /// This cannot be mixed with rules using If.  If you want to have rules for the predicate
        /// use Fact() instead of AddRow.
        /// </summary>
        public void AddRow(in T1 v1, in T2 v2, in T3 v3, in T4 v4)
            => _table.Add((v1, v2, v3, v4));

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
        
        protected override void ClearTable() => _table.Clear();

        /// <summary>
        /// Return all the rows of the table
        /// This allocates memory; do not use in inner lops
        /// </summary>
        public IEnumerable<(T1,T2, T3, T4)> Rows => Table.Rows;

        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="name">Predicate name</param>
        /// <param name="path">Path to the CSV file</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1, T2, T3, T4> FromCsv(string name, string path)
        {
            var (header, data) = CsvReader.ReadCsv(path);
            var p = new TablePredicate<T1, T2, T3, T4>(name, header[0], header[1], header[2], header[3]);
            foreach (var row in data)
                p.AddRow(CsvReader.ConvertCell<T1>(row[0]), CsvReader.ConvertCell<T2>(row[1]), CsvReader.ConvertCell<T3>(row[2]), CsvReader.ConvertCell<T4>(row[3]));
            return p;
        }

        
        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="path">Path to the CSV file</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1, T2, T3, T4> FromCsv(string path) => FromCsv(Path.GetFileNameWithoutExtension(path), path);

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
    }

    /// <summary>
    /// A 5-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of the predicate's 1st argument</typeparam>
    /// <typeparam name="T2">Type of the predicate's 2nd argument</typeparam>
    /// <typeparam name="T3">Type of the predicate's 3rd argument</typeparam>
    /// <typeparam name="T4">Type of the predicate's 4th argument</typeparam>
    /// <typeparam name="T5">Type of the predicate's 5th argument</typeparam>
    public class TablePredicate<T1, T2, T3, T4, T5> : TablePredicate
    {
        /// <summary>
        /// Make a Goal from this predicate with the specified argument value.
        /// </summary>
        public TableGoal<T1, T2, T3, T4, T5> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5] 
            => new TableGoal<T1, T2, T3, T4, T5>(this, arg1, arg2,arg3, arg4, arg5);

        public TablePredicate(string name, string col1 = "col1", string col2 = "col2", string col3 = "col3",
            string col4 = "col4", string col5 = "col5") : base(name, new []{ col1, col2, col3, col4, col5 })
        {
        }

        
        // ReSharper disable once InconsistentNaming
        internal readonly Table<(T1,T2, T3, T4, T5)> _table = new Table<(T1, T2, T3, T4, T5)>();

        internal Table<(T1, T2, T3, T4, T5)> Table  {
            get
            {
                EnsureUpToDate();
                return _table;
            }
        }

        public override int Length => Table.Length;

        /// <summary>
        /// Manually add a row (ground instance) to the extension of the predicate
        /// This cannot be mixed with rules using If.  If you want to have rules for the predicate
        /// use Fact() instead of AddRow.
        /// </summary>
        public void AddRow(in T1 v1, in T2 v2, in T3 v3, in T4 v4, in T5 v5)
            => _table.Add((v1, v2, v3, v4, v5));

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
        
        protected override void ClearTable() => _table.Clear();

        /// <summary>
        /// Return all the rows of the table
        /// This allocates memory; do not use in inner lops
        /// </summary>
        public IEnumerable<(T1,T2, T3, T4, T5)> Rows => Table.Rows;

        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="name">Predicate name</param>
        /// <param name="path">Path to the CSV file</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1, T2, T3, T4, T5> FromCsv(string name, string path)
        {
            var (header, data) = CsvReader.ReadCsv(path);
            var p = new TablePredicate<T1, T2, T3, T4, T5>(name, header[0], header[1], header[2], header[3], header[4]);
            foreach (var row in data)
                p.AddRow(CsvReader.ConvertCell<T1>(row[0]), CsvReader.ConvertCell<T2>(row[1]),
                    CsvReader.ConvertCell<T3>(row[2]), CsvReader.ConvertCell<T4>(row[3]), CsvReader.ConvertCell<T5>(row[4]));
            return p;
        }

        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="path">Path to the CSV file</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1, T2, T3, T4, T5> FromCsv(string path) => FromCsv(Path.GetFileNameWithoutExtension(path), path);


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
    public class TablePredicate<T1, T2, T3, T4, T5, T6> : TablePredicate
    {
        /// <summary>
        /// Make a Goal from this predicate with the specified argument value.
        /// </summary>
        public TableGoal<T1, T2, T3, T4, T5, T6> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5, Term<T6> arg6] 
            => new TableGoal<T1, T2, T3, T4, T5, T6>(this, arg1, arg2,arg3, arg4, arg5, arg6);

        public TablePredicate(string name, string col1 = "col1", string col2 = "col2", string col3 = "col3",
            string col4 = "col4", string col5 = "col5", string col6 = "col6")
            : base(name, new []{ col1, col2, col3, col4, col5, col6 })
        {
        }

        // ReSharper disable once InconsistentNaming
        internal readonly Table<(T1,T2, T3, T4, T5, T6)> _table = new Table<(T1, T2, T3, T4, T5, T6)>();

        internal Table<(T1, T2, T3, T4, T5, T6)> Table  {
            get
            {
                EnsureUpToDate();
                return _table;
            }
        }

        public override int Length => Table.Length;

        /// <summary>
        /// Manually add a row (ground instance) to the extension of the predicate
        /// This cannot be mixed with rules using If.  If you want to have rules for the predicate
        /// use Fact() instead of AddRow.
        /// </summary>
        public void AddRow(in T1 v1, in T2 v2, in T3 v3, in T4 v4, in T5 v5, in T6 v6)
            => _table.Add((v1, v2, v3, v4, v5, v6));

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
        
        protected override void ClearTable() => _table.Clear();
        /// <summary>
        /// Return all the rows of the table
        /// This allocates memory; do not use in inner lops
        /// </summary>
        public IEnumerable<(T1,T2, T3, T4, T5, T6)> Rows => Table.Rows;

        /// <summary>
        /// Read an extensional predicate from a CSV file
        /// </summary>
        /// <param name="name">Predicate name</param>
        /// <param name="path">Path to the CSV file</param>
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1, T2, T3, T4, T5, T6> FromCsv(string name, string path)
        {
            var (header, data) = CsvReader.ReadCsv(path);
            var p = new TablePredicate<T1, T2, T3, T4, T5, T6>(name, header[0], header[1], header[2], header[3], header[4], header[5]);
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
        /// <returns>The TablePredicate</returns>
        public static TablePredicate<T1, T2, T3, T4, T5, T6> FromCsv(string path) => FromCsv(Path.GetFileNameWithoutExtension(path), path);


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
    }
}
