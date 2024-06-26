﻿using System.Diagnostics;
using System.Linq;
using TED.Preprocessing;
using TED.Tables;

namespace TED.Interpreter
{
    /// <summary>
    /// A TableGoal represents a table predicate applied to arguments, e.g. p["a"], p[variable], etc.
    /// Untyped base class for all goals involving TablePredicates
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebugName) + "}")]
    public abstract class TableGoal : Goal
    {
        /// <summary>
        /// Predicate being called
        /// </summary>
        public readonly TablePredicate TablePredicate;

        /// <summary>
        /// Make a new goal object
        /// </summary>
        protected TableGoal(TablePredicate predicate, Term[] args) : base(args)
        {
            TablePredicate = predicate;
        }

        /// <summary>
        /// Add a rule for inferring this predicate from other predicates
        /// You cannot add rules to a table you add rows to using AddRow.
        /// </summary>

        public abstract void If(params Goal[] subgoals);

        /// <summary>
        /// Synonym for If().  This reads better on Table.Initially.Where(...) declarations
        /// than .If(...)
        /// </summary>
        /// <param name="subgoals"></param>
        public void Where(params Goal[] subgoals) => If(subgoals);

        /// <summary>
        /// Add a "fact" (rule with no subgoals) to the predicate
        /// IMPORTANT: this is different from adding the data directly using AddRow!
        /// A TablePredicate can either either rules (including facts) or you can add data manually
        /// using AddRow, but not both.
        /// </summary>
        public void Fact() => If();

        /// <inheritdoc />
        public override Predicate Predicate => TablePredicate;
    }
    /// <summary>
    /// Abstract syntax tree representing a call to a 1-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of predicate argument</typeparam>
    public class TableGoal<T1> : TableGoal
    {
        /// <inheritdoc />
        public TableGoal(TablePredicate predicate, Term<T1> arg1) : base(predicate, new Term[] { arg1 })
        {
            Arg1 = arg1;
        }

        /// <summary>
        /// First (and only) argument in the call.
        /// </summary>
        public readonly Term<T1> Arg1;

        internal override Goal RenameArguments(Substitution s) => new TableGoal<T1>((TablePredicate<T1>)Predicate, s.Substitute(Arg1));

        /// <summary>
        /// Generate a Pattern for matching the goal's argument list.
        /// </summary>
        /// <param name="tc">GoalScanner to use for read/write analysis</param>
        internal Pattern<T1> MakePattern(GoalAnalyzer tc) => new Pattern<T1>(tc.Emit(Arg1));

        /// <summary>
        /// Make a TableCall for this TableGoal
        /// </summary>
        /// <param name="ga">GoalAnalyzer to track the read/write states of argument</param>
        /// <returns></returns>
        internal override Call MakeCall(GoalAnalyzer ga)
        {
            ga.AddDependency(TablePredicate);
            var pattern = MakePattern(ga);
            var tablePredicate = (TablePredicate<T1>)TablePredicate;
            if (TablePredicate.Unique && pattern.IsInstantiated)
                return new TableCallUsingRowSet<T1>(tablePredicate, pattern);
            return new TableCallExhaustive<T1>(tablePredicate, pattern);
        }

        /// <summary>
        /// Add a rule for inferring this predicate from other predicates
        /// You cannot add rules to a table you add rows to using AddRow.
        /// </summary>
        public override void If(params Goal[] subgoals)
        {
            var tc = new GoalAnalyzer();
            // We have to compile this first because the first occurrences of variables have to be in the body
            var (head, body) = Preprocessor.GenerateCalls(tc, this, subgoals);
            TablePredicate.AddRule(new Rule<T1>((TablePredicate<T1>)TablePredicate, head.MakePattern(tc), body, tc.Dependencies, tc.VariableValueCells(), tc.Singletons));
        }
    }

    /// <summary>
    /// Abstract syntax tree representing a call to a 2-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of predicate's first argument</typeparam>
    /// <typeparam name="T2">Type of predicate's second argument</typeparam>
    public class TableGoal<T1, T2> : TableGoal
    {
        /// <inheritdoc />
        public TableGoal(TablePredicate predicate, Term<T1> arg1, Term<T2> arg2) : base(predicate, new Term[] { arg1, arg2 })
        {
            Arg1 = arg1;
            Arg2 = arg2;
        }

        /// <summary>
        /// First argument to the call
        /// </summary>
        public readonly Term<T1> Arg1;
        /// <summary>
        /// Second argument to the call
        /// </summary>
        public readonly Term<T2> Arg2;

        internal override Goal RenameArguments(Substitution s)
            => new TableGoal<T1, T2>((TablePredicate<T1, T2>)Predicate, s.Substitute(Arg1), s.Substitute(Arg2));

        /// <summary>
        /// Generate a Pattern for matching the goal's argument list.
        /// </summary>
        /// <param name="tc">GoalScanner to use for read/write analysis</param>
        internal Pattern<T1, T2> MakePattern(GoalAnalyzer tc) => new Pattern<T1, T2>(tc.Emit(Arg1), tc.Emit(Arg2));

        /// <summary>
        /// Make a TableCall for this TableGoal
        /// </summary>
        /// <param name="ga">GoalAnalyzer to track the read/write states of argument</param>
        /// <returns></returns>
        internal override Call MakeCall(GoalAnalyzer ga)
        {
            ga.AddDependency(TablePredicate);
            var pattern = MakePattern(ga);
            var tablePredicate = (TablePredicate<T1, T2>)TablePredicate;
            if (TablePredicate.Unique && pattern.IsInstantiated)
                return new TableCallUsingRowSet<T1, T2>(tablePredicate, pattern);

            var bestIndex = TablePredicate.TableUntyped.Indices.FirstOrDefault(i => i.CanMatchOn(pattern));
            if (bestIndex != null)
                return bestIndex.MakeCall(pattern);

            return new TableCallExhaustive<T1, T2>(tablePredicate, pattern);
        }

        /// <summary>
        /// Add a rule for inferring this predicate from other predicates
        /// You cannot add rules to a table you add rows to using AddRow.
        /// </summary>
        public override void If(params Goal[] subgoals)
        {
            var tc = new GoalAnalyzer();
            // We have to compile this first because the first occurrences of variables have to be in the body
            var (head, body) = Preprocessor.GenerateCalls(tc, this, subgoals);
            TablePredicate.AddRule(new Rule<T1, T2>((TablePredicate<T1, T2>)TablePredicate, head.MakePattern(tc), body, tc.Dependencies, tc.VariableValueCells(), tc.Singletons));
        }
    }

    /// <summary>
    /// Abstract syntax tree representing a call to a 3-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of predicate's first argument</typeparam>
    /// <typeparam name="T2">Type of predicate's second argument</typeparam>
    /// <typeparam name="T3">Type of predicate's third argument</typeparam>
    public class TableGoal<T1, T2, T3> : TableGoal
    {
        /// <inheritdoc />
        public TableGoal(TablePredicate predicate, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3) : base(predicate, new Term[] { arg1, arg2, arg3 })
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
        }

        /// <summary>
        /// First call argument
        /// </summary>
        public readonly Term<T1> Arg1;
        /// <summary>
        /// Second call argument
        /// </summary>
        public readonly Term<T2> Arg2;
        /// <summary>
        /// Third call argument
        /// </summary>
        public readonly Term<T3> Arg3;

        internal override Goal RenameArguments(Substitution s)
            => new TableGoal<T1, T2, T3>((TablePredicate<T1, T2, T3>)Predicate, s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3));


        /// <summary>
        /// Generate a Pattern for matching the goal's argument list.
        /// </summary>
        /// <param name="tc">GoalScanner to use for read/write analysis</param>
        internal Pattern<T1, T2, T3> MakePattern(GoalAnalyzer tc) => new Pattern<T1, T2, T3>(tc.Emit(Arg1), tc.Emit(Arg2), tc.Emit(Arg3));

        /// <summary>
        /// Make a TableCall for this TableGoal
        /// </summary>
        /// <param name="ga">GoalAnalyzer to track the read/write states of argument</param>
        /// <returns></returns>
        internal override Call MakeCall(GoalAnalyzer ga)
        {
            ga.AddDependency(TablePredicate);
            var pattern = MakePattern(ga);
            var tablePredicate = (TablePredicate<T1, T2, T3>)TablePredicate;
            if (TablePredicate.Unique && pattern.IsInstantiated)
                return new TableCallUsingRowSet<T1, T2, T3>(tablePredicate, pattern);

            var bestIndex = TablePredicate.TableUntyped.Indices.FirstOrDefault(i => i.CanMatchOn(pattern));
            if (bestIndex != null)
                return bestIndex.MakeCall(pattern);

            return new TableCallExhaustive<T1, T2, T3>(tablePredicate, pattern);
        }

        /// <summary>
        /// Add a rule for inferring this predicate from other predicates
        /// You cannot add rules to a table you add rows to using AddRow.
        /// </summary>
        public override void If(params Goal[] subgoals)
        {
            var tc = new GoalAnalyzer();
            // We have to compile this first because the first occurrences of variables have to be in the body
            var (head, body) = Preprocessor.GenerateCalls(tc, this, subgoals);
            TablePredicate.AddRule(new Rule<T1, T2, T3>((TablePredicate<T1, T2, T3>)TablePredicate, head.MakePattern(tc), body, tc.Dependencies, tc.VariableValueCells(), tc.Singletons));
        }
    }

    /// <summary>
    /// Abstract syntax tree representing a call to a 4-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of predicate's first argument</typeparam>
    /// <typeparam name="T2">Type of predicate's second argument</typeparam>
    /// <typeparam name="T3">Type of predicate's third argument</typeparam>
    /// <typeparam name="T4">Type of predicate's fourth argument</typeparam>
    public class TableGoal<T1, T2, T3, T4> : TableGoal
    {
        /// <inheritdoc />
        public TableGoal(TablePredicate predicate, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4) : base(predicate, new Term[] { arg1, arg2, arg3, arg4 })
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
        }

        /// <summary>
        /// First call argument
        /// </summary>
        public readonly Term<T1> Arg1;
        /// <summary>
        /// Second call argument
        /// </summary>
        public readonly Term<T2> Arg2;
        /// <summary>
        /// Third call argument
        /// </summary>
        public readonly Term<T3> Arg3;
        /// <summary>
        /// Fourth call argument
        /// </summary>
        public readonly Term<T4> Arg4;

        internal override Goal RenameArguments(Substitution s)
            => new TableGoal<T1, T2, T3, T4>((TablePredicate<T1, T2, T3, T4>)Predicate, s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3), s.Substitute(Arg4));

        /// <summary>
        /// Generate a Pattern for matching the goal's argument list.
        /// </summary>
        /// <param name="tc">GoalScanner to use for read/write analysis</param>
        internal Pattern<T1, T2, T3, T4> MakePattern(GoalAnalyzer tc)
            => new Pattern<T1, T2, T3, T4>(tc.Emit(Arg1), tc.Emit(Arg2), tc.Emit(Arg3), tc.Emit(Arg4));

        /// <summary>
        /// Make a TableCall for this TableGoal
        /// </summary>
        /// <param name="ga">GoalAnalyzer to track the read/write states of argument</param>
        /// <returns></returns>
        internal override Call MakeCall(GoalAnalyzer ga)
        {
            ga.AddDependency(TablePredicate);
            var pattern = MakePattern(ga);
            var tablePredicate = (TablePredicate<T1, T2, T3, T4>)TablePredicate;
            if (TablePredicate.Unique && pattern.IsInstantiated)
                return new TableCallUsingRowSet<T1, T2, T3, T4>(tablePredicate, pattern);

            var bestIndex = TablePredicate.TableUntyped.Indices.FirstOrDefault(i => i.CanMatchOn(pattern));
            if (bestIndex != null)
                return bestIndex.MakeCall(pattern);

            return new TableCallExhaustive<T1, T2, T3, T4>(tablePredicate, pattern);
        }

        /// <summary>
        /// Add a rule for inferring this predicate from other predicates
        /// You cannot add rules to a table you add rows to using AddRow.
        /// </summary>
        public override void If(params Goal[] subgoals)
        {
            var tc = new GoalAnalyzer();
            // We have to compile this first because the first occurrences of variables have to be in the body
            var (head, body) = Preprocessor.GenerateCalls(tc, this, subgoals);
            TablePredicate.AddRule(new Rule<T1, T2, T3, T4>((TablePredicate<T1, T2, T3, T4>)TablePredicate, head.MakePattern(tc), body, tc.Dependencies, tc.VariableValueCells(), tc.Singletons));
        }
    }

    /// <summary>
    /// Abstract syntax tree representing a call to a 5-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of predicate's first argument</typeparam>
    /// <typeparam name="T2">Type of predicate's second argument</typeparam>
    /// <typeparam name="T3">Type of predicate's third argument</typeparam>
    /// <typeparam name="T4">Type of predicate's fourth argument</typeparam>
    /// <typeparam name="T5">Type of predicate's fifth argument</typeparam>
    public class TableGoal<T1, T2, T3, T4, T5> : TableGoal
    {
        /// <inheritdoc />
        public TableGoal(TablePredicate predicate, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5) : base(predicate, new Term[] { arg1, arg2, arg3, arg4, arg5 })
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
        }

        /// <summary>
        /// First call argument
        /// </summary>
        public readonly Term<T1> Arg1;
        /// <summary>
        /// Second call argument
        /// </summary>
        public readonly Term<T2> Arg2;
        /// <summary>
        /// Third call argument
        /// </summary>
        public readonly Term<T3> Arg3;
        /// <summary>
        /// Fourth call argument
        /// </summary>
        public readonly Term<T4> Arg4;
        /// <summary>
        /// Fifth call argument
        /// </summary>
        public readonly Term<T5> Arg5;

        internal override Goal RenameArguments(Substitution s)
            => new TableGoal<T1, T2, T3, T4, T5>((TablePredicate<T1, T2, T3, T4, T5>)Predicate, s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3), s.Substitute(Arg4), s.Substitute(Arg5));

        /// <summary>
        /// Generate a Pattern for matching the goal's argument list.
        /// </summary>
        /// <param name="tc">GoalScanner to use for read/write analysis</param>
        internal Pattern<T1, T2, T3, T4, T5> MakePattern(GoalAnalyzer tc)
            => new Pattern<T1, T2, T3, T4, T5>(tc.Emit(Arg1), tc.Emit(Arg2), tc.Emit(Arg3), tc.Emit(Arg4),
                tc.Emit(Arg5));

        /// <summary>
        /// Make a TableCall for this TableGoal
        /// </summary>
        /// <param name="ga">GoalAnalyzer to track the read/write states of argument</param>
        /// <returns></returns>
        internal override Call MakeCall(GoalAnalyzer ga)
        {
            ga.AddDependency(TablePredicate);
            var pattern = MakePattern(ga);
            var tablePredicate = (TablePredicate<T1, T2, T3, T4, T5>)TablePredicate;
            if (TablePredicate.Unique && pattern.IsInstantiated)
                return new TableCallUsingRowSet<T1, T2, T3, T4, T5>(tablePredicate, pattern);

            var bestIndex = TablePredicate.TableUntyped.Indices.FirstOrDefault(i => i.CanMatchOn(pattern));
            if (bestIndex != null)
                return bestIndex.MakeCall(pattern);

            return new TableCallExhaustive<T1, T2, T3, T4, T5>(tablePredicate, pattern);
        }

        /// <summary>
        /// Add a rule for inferring this predicate from other predicates
        /// You cannot add rules to a table you add rows to using AddRow.
        /// </summary>
        public override void If(params Goal[] subgoals)
        {
            var tc = new GoalAnalyzer();
            // We have to compile this first because the first occurrences of variables have to be in the body
            var (head, body) = Preprocessor.GenerateCalls(tc, this, subgoals);
            TablePredicate.AddRule(new Rule<T1, T2, T3, T4, T5>((TablePredicate<T1, T2, T3, T4, T5>)TablePredicate, head.MakePattern(tc), body, tc.Dependencies, tc.VariableValueCells(), tc.Singletons));
        }
    }

    /// <summary>
    /// Abstract syntax tree representing a call to a 6-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of predicate's first argument</typeparam>
    /// <typeparam name="T2">Type of predicate's second argument</typeparam>
    /// <typeparam name="T3">Type of predicate's third argument</typeparam>
    /// <typeparam name="T4">Type of predicate's fourth argument</typeparam>
    /// <typeparam name="T5">Type of predicate's fifth argument</typeparam>
    /// <typeparam name="T6">Type of predicate's sixth argument</typeparam>
    public class TableGoal<T1, T2, T3, T4, T5, T6> : TableGoal
    {
        /// <inheritdoc />
        public TableGoal(TablePredicate predicate, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5, Term<T6> arg6) : base(predicate, new Term[] { arg1, arg2, arg3, arg4, arg5, arg6 })
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
            Arg6 = arg6;
        }

        /// <summary>
        /// First call argument
        /// </summary>
        public readonly Term<T1> Arg1;
        /// <summary>
        /// Second call argument
        /// </summary>
        public readonly Term<T2> Arg2;
        /// <summary>
        /// Third call argument
        /// </summary>
        public readonly Term<T3> Arg3;
        /// <summary>
        /// Fourth call argument
        /// </summary>
        public readonly Term<T4> Arg4;
        /// <summary>
        /// Fifth call argument
        /// </summary>
        public readonly Term<T5> Arg5;
        /// <summary>
        /// Sixth call argument
        /// </summary>
        public readonly Term<T6> Arg6;

        internal override Goal RenameArguments(Substitution s)
            => new TableGoal<T1, T2, T3, T4, T5, T6>((TablePredicate<T1, T2, T3, T4, T5, T6>)Predicate, s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3), s.Substitute(Arg4), s.Substitute(Arg5), s.Substitute(Arg6));


        /// <summary>
        /// Generate a Pattern for matching the goal's argument list.
        /// </summary>
        /// <param name="tc">GoalScanner to use for read/write analysis</param>
        internal Pattern<T1, T2, T3, T4, T5, T6> MakePattern(GoalAnalyzer tc)
            => new Pattern<T1, T2, T3, T4, T5, T6>(tc.Emit(Arg1), tc.Emit(Arg2), tc.Emit(Arg3), tc.Emit(Arg4),
                tc.Emit(Arg5), tc.Emit(Arg6));

        /// <summary>
        /// Make a TableCall for this TableGoal
        /// </summary>
        /// <param name="ga">GoalAnalyzer to track the read/write states of argument</param>
        /// <returns></returns>
        internal override Call MakeCall(GoalAnalyzer ga)
        {
            ga.AddDependency(TablePredicate);
            var pattern = MakePattern(ga);
            var tablePredicate = (TablePredicate<T1, T2, T3, T4, T5, T6>)TablePredicate;
            if (TablePredicate.Unique && pattern.IsInstantiated)
                return new TableCallUsingRowSet<T1, T2, T3, T4, T5, T6>(tablePredicate, pattern);

            var bestIndex = TablePredicate.TableUntyped.Indices.FirstOrDefault(i => i.CanMatchOn(pattern));
            if (bestIndex != null)
                return bestIndex.MakeCall(pattern);

            return new TableCallExhaustive<T1, T2, T3, T4, T5, T6>(tablePredicate, pattern);
        }

        /// <summary>
        /// Add a rule for inferring this predicate from other predicates
        /// You cannot add rules to a table you add rows to using AddRow.
        /// </summary>
        public override void If(params Goal[] subgoals)
        {
            var tc = new GoalAnalyzer();
            // We have to compile this first because the first occurrences of variables have to be in the body
            var (head, body) = Preprocessor.GenerateCalls(tc, this, subgoals);
            TablePredicate.AddRule(new Rule<T1, T2, T3, T4, T5, T6>((TablePredicate<T1, T2, T3, T4, T5, T6>)TablePredicate, head.MakePattern(tc), body, tc.Dependencies, tc.VariableValueCells(), tc.Singletons));
        }
    }

    /// <summary>
    /// Abstract syntax tree representing a call to a 7-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of predicate's first argument</typeparam>
    /// <typeparam name="T2">Type of predicate's second argument</typeparam>
    /// <typeparam name="T3">Type of predicate's third argument</typeparam>
    /// <typeparam name="T4">Type of predicate's fourth argument</typeparam>
    /// <typeparam name="T5">Type of predicate's fifth argument</typeparam>
    /// <typeparam name="T6">Type of predicate's sixth argument</typeparam>
    /// <typeparam name="T7">Type of predicate's seventh argument</typeparam>
    public class TableGoal<T1, T2, T3, T4, T5, T6, T7> : TableGoal
    {
        /// <inheritdoc />
        public TableGoal(TablePredicate predicate, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5, Term<T6> arg6, Term<T7> arg7) : base(predicate, new Term[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7 })
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
            Arg6 = arg6;
            Arg7 = arg7;
        }

        /// <summary>
        /// First call argument
        /// </summary>
        public readonly Term<T1> Arg1;
        /// <summary>
        /// Second call argument
        /// </summary>
        public readonly Term<T2> Arg2;
        /// <summary>
        /// Third call argument
        /// </summary>
        public readonly Term<T3> Arg3;
        /// <summary>
        /// Fourth call argument
        /// </summary>
        public readonly Term<T4> Arg4;
        /// <summary>
        /// Fifth call argument
        /// </summary>
        public readonly Term<T5> Arg5;
        /// <summary>
        /// Sixth call argument
        /// </summary>
        public readonly Term<T6> Arg6;
        /// <summary>
        /// Seventh call argument
        /// </summary>
        public readonly Term<T7> Arg7;

        internal override Goal RenameArguments(Substitution s)
            => new TableGoal<T1, T2, T3, T4, T5, T6, T7>((TablePredicate<T1, T2, T3, T4, T5, T6, T7>)Predicate, s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3), s.Substitute(Arg4), s.Substitute(Arg5), s.Substitute(Arg6), s.Substitute(Arg7));


        /// <summary>
        /// Generate a Pattern for matching the goal's argument list.
        /// </summary>
        /// <param name="tc">GoalScanner to use for read/write analysis</param>
        internal Pattern<T1, T2, T3, T4, T5, T6, T7> MakePattern(GoalAnalyzer tc)
            => new Pattern<T1, T2, T3, T4, T5, T6, T7>(tc.Emit(Arg1), tc.Emit(Arg2), tc.Emit(Arg3), tc.Emit(Arg4),
                tc.Emit(Arg5), tc.Emit(Arg6), tc.Emit(Arg7));

        /// <summary>
        /// Make a TableCall for this TableGoal
        /// </summary>
        /// <param name="ga">GoalAnalyzer to track the read/write states of argument</param>
        /// <returns></returns>
        internal override Call MakeCall(GoalAnalyzer ga)
        {
            ga.AddDependency(TablePredicate);
            var pattern = MakePattern(ga);
            var tablePredicate = (TablePredicate<T1, T2, T3, T4, T5, T6, T7>)TablePredicate;
            if (TablePredicate.Unique && pattern.IsInstantiated)
                return new TableCallUsingRowSet<T1, T2, T3, T4, T5, T6, T7>(tablePredicate, pattern);

            var bestIndex = TablePredicate.TableUntyped.Indices.FirstOrDefault(i => i.CanMatchOn(pattern));
            if (bestIndex != null)
                return bestIndex.MakeCall(pattern);

            return new TableCallExhaustive<T1, T2, T3, T4, T5, T6, T7>(tablePredicate, pattern);
        }

        /// <summary>
        /// Add a rule for inferring this predicate from other predicates
        /// You cannot add rules to a table you add rows to using AddRow.
        /// </summary>
        public override void If(params Goal[] subgoals)
        {
            var tc = new GoalAnalyzer();
            // We have to compile this first because the first occurrences of variables have to be in the body
            var (head, body) = Preprocessor.GenerateCalls(tc, this, subgoals);
            TablePredicate.AddRule(new Rule<T1, T2, T3, T4, T5, T6, T7>((TablePredicate<T1, T2, T3, T4, T5, T6, T7>)TablePredicate, head.MakePattern(tc), body, tc.Dependencies, tc.VariableValueCells(), tc.Singletons));
        }
    }

    /// <summary>
    /// Abstract syntax tree representing a call to a 8-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of predicate's first argument</typeparam>
    /// <typeparam name="T2">Type of predicate's second argument</typeparam>
    /// <typeparam name="T3">Type of predicate's third argument</typeparam>
    /// <typeparam name="T4">Type of predicate's fourth argument</typeparam>
    /// <typeparam name="T5">Type of predicate's fifth argument</typeparam>
    /// <typeparam name="T6">Type of predicate's sixth argument</typeparam>
    /// <typeparam name="T7">Type of predicate's seventh argument</typeparam>
    /// <typeparam name="T8">Type of predicate's eighth argument</typeparam>
    public class TableGoal<T1, T2, T3, T4, T5, T6, T7, T8> : TableGoal
    {
        /// <inheritdoc />
        public TableGoal(TablePredicate predicate, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5, Term<T6> arg6, Term<T7> arg7, Term<T8> arg8) : base(predicate, new Term[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 })
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
            Arg6 = arg6;
            Arg7 = arg7;
            Arg8 = arg8;
        }

        /// <summary>
        /// First call argument
        /// </summary>
        public readonly Term<T1> Arg1;
        /// <summary>
        /// Second call argument
        /// </summary>
        public readonly Term<T2> Arg2;
        /// <summary>
        /// Third call argument
        /// </summary>
        public readonly Term<T3> Arg3;
        /// <summary>
        /// Fourth call argument
        /// </summary>
        public readonly Term<T4> Arg4;
        /// <summary>
        /// Fifth call argument
        /// </summary>
        public readonly Term<T5> Arg5;
        /// <summary>
        /// Sixth call argument
        /// </summary>
        public readonly Term<T6> Arg6;
        /// <summary>
        /// Seventh call argument
        /// </summary>
        public readonly Term<T7> Arg7;
        /// <summary>
        /// Seventh call argument
        /// </summary>
        public readonly Term<T8> Arg8;

        internal override Goal RenameArguments(Substitution s)
            => new TableGoal<T1, T2, T3, T4, T5, T6, T7, T8>((TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8>)Predicate, s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3), s.Substitute(Arg4), s.Substitute(Arg5), s.Substitute(Arg6), s.Substitute(Arg7), s.Substitute(Arg8));


        /// <summary>
        /// Generate a Pattern for matching the goal's argument list.
        /// </summary>
        /// <param name="tc">GoalScanner to use for read/write analysis</param>
        internal Pattern<T1, T2, T3, T4, T5, T6, T7, T8> MakePattern(GoalAnalyzer tc)
            => new Pattern<T1, T2, T3, T4, T5, T6, T7, T8>(tc.Emit(Arg1), tc.Emit(Arg2), tc.Emit(Arg3), tc.Emit(Arg4),
                tc.Emit(Arg5), tc.Emit(Arg6), tc.Emit(Arg7), tc.Emit(Arg8));

        /// <summary>
        /// Make a TableCall for this TableGoal
        /// </summary>
        /// <param name="ga">GoalAnalyzer to track the read/write states of argument</param>
        /// <returns></returns>
        internal override Call MakeCall(GoalAnalyzer ga)
        {
            ga.AddDependency(TablePredicate);
            var pattern = MakePattern(ga);
            var tablePredicate = (TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8>)TablePredicate;
            if (TablePredicate.Unique && pattern.IsInstantiated)
                return new TableCallUsingRowSet<T1, T2, T3, T4, T5, T6, T7, T8>(tablePredicate, pattern);

            var bestIndex = TablePredicate.TableUntyped.Indices.FirstOrDefault(i => i.CanMatchOn(pattern));
            if (bestIndex != null)
                return bestIndex.MakeCall(pattern);

            return new TableCallExhaustive<T1, T2, T3, T4, T5, T6, T7, T8>(tablePredicate, pattern);
        }

        /// <summary>
        /// Add a rule for inferring this predicate from other predicates
        /// You cannot add rules to a table you add rows to using AddRow.
        /// </summary>
        public override void If(params Goal[] subgoals)
        {
            var tc = new GoalAnalyzer();
            // We have to compile this first because the first occurrences of variables have to be in the body
            var (head, body) = Preprocessor.GenerateCalls(tc, this, subgoals);
            TablePredicate.AddRule(new Rule<T1, T2, T3, T4, T5, T6, T7, T8>((TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8>)TablePredicate, head.MakePattern(tc), body, tc.Dependencies, tc.VariableValueCells(), tc.Singletons));
        }
    }
}
