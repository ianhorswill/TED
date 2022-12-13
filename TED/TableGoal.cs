namespace TED
{
    /// <summary>
    /// Abstract syntax tree representing a call to a 1-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of predicate argument</typeparam>
    public class TableGoal<T1> : AnyTableGoal
    {
        public TableGoal(TablePredicate predicate, Term<T1> arg1) : base(predicate, new AnyTerm[] { arg1 })
        {
            Arg1 = arg1;
        }

        public readonly Term<T1> Arg1;

        public override AnyGoal RenameArguments(Substitution s) => new TableGoal<T1>((TablePredicate<T1>)Predicate, s.Substitute(Arg1));

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
        internal override AnyCall MakeCall(GoalAnalyzer ga)
        {
            ga.AddDependency(TablePredicate);
            var pattern = MakePattern(ga);
            var tablePredicate = (TablePredicate<T1>)TablePredicate;
            if (pattern.IsInstantiated)
                return new TableCallTest<T1>(tablePredicate, pattern);
            return new TableCallExhaustive<T1>(tablePredicate, pattern);
        }

        /// <summary>
        /// Add a rule for inferring this predicate from other predicates
        /// You cannot add rules to a table you add rows to using AddRow.
        /// </summary>
        public override void If(params AnyGoal[] subgoals)
        {
            var tc = new GoalAnalyzer();
            // We have to compile this first because the first occurrences of variables have to be in the body
            var body = RulePreprocessor.GenerateCalls(tc, subgoals);
            TablePredicate.AddRule(new Rule<T1>((TablePredicate<T1>)TablePredicate, MakePattern(tc), body, tc.Dependencies));
        }
    }

    /// <summary>
    /// Abstract syntax tree representing a call to a 2-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of predicate's first argument</typeparam>
    /// <typeparam name="T2">Type of predicate's second argument</typeparam>
    public class TableGoal<T1, T2> : AnyTableGoal
    {
        public TableGoal(TablePredicate predicate, Term<T1> arg1, Term<T2> arg2) : base(predicate, new AnyTerm[] { arg1, arg2 })
        {
            Arg1 = arg1;
            Arg2 = arg2;
        }

        public readonly Term<T1> Arg1;
        public readonly Term<T2> Arg2;

        public override AnyGoal RenameArguments(Substitution s)
            => new TableGoal<T1,T2>((TablePredicate<T1,T2>)Predicate, s.Substitute(Arg1), s.Substitute(Arg2));

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
        internal override AnyCall MakeCall(GoalAnalyzer ga)
        {
            ga.AddDependency(TablePredicate);
            var pattern = MakePattern(ga);
            var tablePredicate = (TablePredicate<T1,T2>)TablePredicate;
            if (pattern.IsInstantiated)
                return new TableCallTest<T1,T2>(tablePredicate, pattern);
            return new TableCallExhaustive<T1,T2>(tablePredicate, pattern);
        }

        /// <summary>
        /// Add a rule for inferring this predicate from other predicates
        /// You cannot add rules to a table you add rows to using AddRow.
        /// </summary>
        public override void If(params AnyGoal[] subgoals)
        {
            var tc = new GoalAnalyzer();
            // We have to compile this first because the first occurrences of variables have to be in the body
            var body = RulePreprocessor.GenerateCalls(tc, subgoals);
            TablePredicate.AddRule(new Rule<T1, T2>((TablePredicate<T1, T2>)TablePredicate, MakePattern(tc), body, tc.Dependencies));
        }
    }

    /// <summary>
    /// Abstract syntax tree representing a call to a 3-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of predicate's first argument</typeparam>
    /// <typeparam name="T2">Type of predicate's second argument</typeparam>
    /// <typeparam name="T3">Type of predicate's third argument</typeparam>
    public class TableGoal<T1, T2, T3> : AnyTableGoal
    {
        public TableGoal(TablePredicate predicate, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3) : base(predicate, new AnyTerm[] { arg1, arg2, arg3 })
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
        }

        public readonly Term<T1> Arg1;
        public readonly Term<T2> Arg2;
        public readonly Term<T3> Arg3;

        public override AnyGoal RenameArguments(Substitution s)
            => new TableGoal<T1,T2,T3>((TablePredicate<T1,T2,T3>)Predicate, s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3));


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
        internal override AnyCall MakeCall(GoalAnalyzer ga)
        {
            ga.AddDependency(TablePredicate);
            var pattern = MakePattern(ga);
            var tablePredicate = (TablePredicate<T1,T2,T3>)TablePredicate;
            if (pattern.IsInstantiated)
                return new TableCallTest<T1,T2,T3>(tablePredicate, pattern);
            return new TableCallExhaustive<T1,T2,T3>(tablePredicate, pattern);
        }

        /// <summary>
        /// Add a rule for inferring this predicate from other predicates
        /// You cannot add rules to a table you add rows to using AddRow.
        /// </summary>
        public override void If(params AnyGoal[] subgoals)
        {
            var tc = new GoalAnalyzer();
            // We have to compile this first because the first occurrences of variables have to be in the body
            var body = RulePreprocessor.GenerateCalls(tc, subgoals);
            TablePredicate.AddRule(new Rule<T1, T2, T3>((TablePredicate<T1, T2, T3>)TablePredicate, MakePattern(tc), body, tc.Dependencies));
        }
    }

    /// <summary>
    /// Abstract syntax tree representing a call to a 4-argument TablePredicate
    /// </summary>
    /// <typeparam name="T1">Type of predicate's first argument</typeparam>
    /// <typeparam name="T2">Type of predicate's second argument</typeparam>
    /// <typeparam name="T3">Type of predicate's third argument</typeparam>
    /// <typeparam name="T4">Type of predicate's fourth argument</typeparam>
    public class TableGoal<T1, T2, T3, T4> : AnyTableGoal
    {
        public TableGoal(TablePredicate predicate, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4) : base(predicate, new AnyTerm[] { arg1, arg2, arg3, arg4 })
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
        }

        public readonly Term<T1> Arg1;
        public readonly Term<T2> Arg2;
        public readonly Term<T3> Arg3;
        public readonly Term<T4> Arg4;

        public override AnyGoal RenameArguments(Substitution s)
            => new TableGoal<T1,T2,T3,T4>((TablePredicate<T1,T2,T3,T4>)Predicate, s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3), s.Substitute(Arg4));

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
        internal override AnyCall MakeCall(GoalAnalyzer ga)
        {
            ga.AddDependency(TablePredicate);
            var pattern = MakePattern(ga);
            var tablePredicate = (TablePredicate<T1,T2,T3,T4>)TablePredicate;
            if (pattern.IsInstantiated)
                return new TableCallTest<T1,T2,T3,T4>(tablePredicate, pattern);
            return new TableCallExhaustive<T1,T2,T3,T4>(tablePredicate, pattern);
        }

        /// <summary>
        /// Add a rule for inferring this predicate from other predicates
        /// You cannot add rules to a table you add rows to using AddRow.
        /// </summary>
        public override void If(params AnyGoal[] subgoals)
        {
            var tc = new GoalAnalyzer();
            // We have to compile this first because the first occurrences of variables have to be in the body
            var body = RulePreprocessor.GenerateCalls(tc, subgoals);
            TablePredicate.AddRule(new Rule<T1, T2, T3, T4>((TablePredicate<T1, T2, T3, T4>)TablePredicate, MakePattern(tc), body, tc.Dependencies));
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
    public class TableGoal<T1, T2, T3, T4, T5> : AnyTableGoal
    {
        public TableGoal(TablePredicate predicate, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5) : base(predicate, new AnyTerm[] { arg1, arg2, arg3, arg4, arg5 })
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
        }

        public readonly Term<T1> Arg1;
        public readonly Term<T2> Arg2;
        public readonly Term<T3> Arg3;
        public readonly Term<T4> Arg4;
        public readonly Term<T5> Arg5;

        public override AnyGoal RenameArguments(Substitution s)
            => new TableGoal<T1,T2,T3,T4,T5>((TablePredicate<T1,T2,T3,T4,T5>)Predicate, s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3), s.Substitute(Arg4), s.Substitute(Arg5));

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
        internal override AnyCall MakeCall(GoalAnalyzer ga)
        {
            ga.AddDependency(TablePredicate);
            var pattern = MakePattern(ga);
            var tablePredicate = (TablePredicate<T1,T2,T3,T4,T5>)TablePredicate;
            if (pattern.IsInstantiated)
                return new TableCallTest<T1,T2,T3,T4,T5>(tablePredicate, pattern);
            return new TableCallExhaustive<T1,T2,T3,T4,T5>(tablePredicate, pattern);
        }

        /// <summary>
        /// Add a rule for inferring this predicate from other predicates
        /// You cannot add rules to a table you add rows to using AddRow.
        /// </summary>
        public override void If(params AnyGoal[] subgoals)
        {
            var tc = new GoalAnalyzer();
            // We have to compile this first because the first occurrences of variables have to be in the body
            var body = RulePreprocessor.GenerateCalls(tc, subgoals);
            TablePredicate.AddRule(new Rule<T1, T2, T3, T4, T5>((TablePredicate<T1, T2, T3, T4, T5>)TablePredicate, MakePattern(tc), body, tc.Dependencies));
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
    public class TableGoal<T1, T2, T3, T4, T5, T6> : AnyTableGoal
    {
        public TableGoal(TablePredicate predicate, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5, Term<T6> arg6) : base(predicate, new AnyTerm[] { arg1, arg2, arg3, arg4, arg5, arg6 })
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
            Arg6 = arg6;
        }

        public readonly Term<T1> Arg1;
        public readonly Term<T2> Arg2;
        public readonly Term<T3> Arg3;
        public readonly Term<T4> Arg4;
        public readonly Term<T5> Arg5;
        public readonly Term<T6> Arg6;

        public override AnyGoal RenameArguments(Substitution s)
            => new TableGoal<T1,T2,T3,T4,T5,T6>((TablePredicate<T1,T2,T3,T4,T5,T6>)Predicate, s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3), s.Substitute(Arg4), s.Substitute(Arg5), s.Substitute(Arg6));


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
        internal override AnyCall MakeCall(GoalAnalyzer ga)
        {
            ga.AddDependency(TablePredicate);
            var pattern = MakePattern(ga);
            var tablePredicate = (TablePredicate<T1,T2,T3,T4,T5,T6>)TablePredicate;
            if (pattern.IsInstantiated)
                return new TableCallTest<T1,T2,T3,T4,T5,T6>(tablePredicate, pattern);
            return new TableCallExhaustive<T1,T2,T3,T4,T5,T6>(tablePredicate, pattern);
        }

        /// <summary>
        /// Add a rule for inferring this predicate from other predicates
        /// You cannot add rules to a table you add rows to using AddRow.
        /// </summary>
        public override void If(params AnyGoal[] subgoals)
        {
            var tc = new GoalAnalyzer();
            // We have to compile this first because the first occurrences of variables have to be in the body
            var body = RulePreprocessor.GenerateCalls(tc, subgoals);
            TablePredicate.AddRule(new Rule<T1, T2, T3, T4, T5, T6>((TablePredicate<T1, T2, T3, T4, T5, T6>)TablePredicate, MakePattern(tc), body, tc.Dependencies));
        }
    }
}
