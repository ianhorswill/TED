using System.Collections.Generic;

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
        private static readonly List<TablePredicate> AllTablePredicates = new List<TablePredicate>();
        
        /// <summary>
        /// Make a new predicate
        /// </summary>
        protected TablePredicate(string name) : base(name)
        {
            AllTablePredicates.Add(this);
        }
        
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

        public TablePredicate(string name) : base(name)
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

        public TablePredicate(string name) : base(name)
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

        public TablePredicate(string name) : base(name)
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

        public TablePredicate(string name) : base(name)
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

        public TablePredicate(string name) : base(name)
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

        public TablePredicate(string name) : base(name)
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
    }
}
