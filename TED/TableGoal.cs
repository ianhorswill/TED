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

        /// <summary>
        /// First (and only) argument in the call.
        /// </summary>
        public readonly Term<T1> Arg1;

        internal override AnyGoal RenameArguments(Substitution s) => new TableGoal<T1>((TablePredicate<T1>)Predicate, s.Substitute(Arg1));

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
            if (TablePredicate.Unique && pattern.IsInstantiated)
                return new TableCallUsingRowSet<T1>(tablePredicate, pattern);
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
            var (head, body) = Preprocessor.GenerateCalls(tc, this, subgoals);
            TablePredicate.AddRule(new Rule<T1>((TablePredicate<T1>)TablePredicate, head.MakePattern(tc), body, tc.Dependencies, tc.VariableValueCells()));
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

        /// <summary>
        /// First argument to the call
        /// </summary>
        public readonly Term<T1> Arg1;
        /// <summary>
        /// Second argument to the call
        /// </summary>
        public readonly Term<T2> Arg2;

        internal override AnyGoal RenameArguments(Substitution s)
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
            if (TablePredicate.Unique && pattern.IsInstantiated)
                return new TableCallUsingRowSet<T1,T2>(tablePredicate, pattern);

            if (pattern.Arg1.IsInstantiated && TablePredicate.IndexFor(0, true) is KeyIndex<(T1,T2), T1> index)
                return new TableCallWithKey<T1, T1, T2>(tablePredicate, pattern, index, pattern.Arg1.ValueCell);
            if (pattern.Arg2.IsInstantiated && TablePredicate.IndexFor(1, true) is KeyIndex<(T1,T2), T2> index2)
                return new TableCallWithKey<T2, T1, T2>(tablePredicate, pattern, index2, pattern.Arg2.ValueCell);

            if (pattern.Arg1.IsInstantiated && TablePredicate.IndexFor(0, true) is GeneralIndex<(T1,T2), T1> g)
                return new TableCallWithGeneralIndex<T1, T1, T2>(tablePredicate, pattern, g, pattern.Arg1.ValueCell);
            if (pattern.Arg2.IsInstantiated && TablePredicate.IndexFor(1, true) is GeneralIndex<(T1,T2), T2> g2)
                return new TableCallWithGeneralIndex<T2, T1, T2>(tablePredicate, pattern, g2, pattern.Arg2.ValueCell);

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
            var (head, body) = Preprocessor.GenerateCalls(tc, this, subgoals);
            TablePredicate.AddRule(new Rule<T1, T2>((TablePredicate<T1, T2>)TablePredicate, head.MakePattern(tc), body, tc.Dependencies, tc.VariableValueCells()));
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

        internal override AnyGoal RenameArguments(Substitution s)
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
            if (TablePredicate.Unique && pattern.IsInstantiated)
                return new TableCallUsingRowSet<T1,T2,T3>(tablePredicate, pattern);

            if (pattern.Arg1.IsInstantiated && TablePredicate.IndexFor(0, true) is KeyIndex<(T1,T2, T3), T1> index)
                return new TableCallWithKey<T1, T1, T2, T3>(tablePredicate, pattern, index, pattern.Arg1.ValueCell);
            if (pattern.Arg2.IsInstantiated && TablePredicate.IndexFor(1, true) is KeyIndex<(T1,T2, T3), T2> index2)
                return new TableCallWithKey<T2, T1, T2, T3>(tablePredicate, pattern, index2, pattern.Arg2.ValueCell);
            if (pattern.Arg3.IsInstantiated && TablePredicate.IndexFor(2, true) is KeyIndex<(T1,T2, T3), T3> index3)
                return new TableCallWithKey<T3, T1, T2, T3>(tablePredicate, pattern, index3, pattern.Arg3.ValueCell);

            if (pattern.Arg1.IsInstantiated && TablePredicate.IndexFor(0, true) is GeneralIndex<(T1,T2, T3), T1> g)
                return new TableCallWithGeneralIndex<T1, T1, T2, T3>(tablePredicate, pattern, g, pattern.Arg1.ValueCell);
            if (pattern.Arg2.IsInstantiated && TablePredicate.IndexFor(1, true) is GeneralIndex<(T1,T2, T3), T2> g2)
                return new TableCallWithGeneralIndex<T2, T1, T2, T3>(tablePredicate, pattern, g2, pattern.Arg2.ValueCell);
            if (pattern.Arg3.IsInstantiated && TablePredicate.IndexFor(2, true) is GeneralIndex<(T1,T2, T3), T3> g3)
                return new TableCallWithGeneralIndex<T3, T1, T2, T3>(tablePredicate, pattern, g3, pattern.Arg3.ValueCell);


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
            var (head, body) = Preprocessor.GenerateCalls(tc, this, subgoals);
            TablePredicate.AddRule(new Rule<T1, T2, T3>((TablePredicate<T1, T2, T3>)TablePredicate, head.MakePattern(tc), body, tc.Dependencies, tc.VariableValueCells()));
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

        internal override AnyGoal RenameArguments(Substitution s)
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
            if (TablePredicate.Unique && pattern.IsInstantiated)
                return new TableCallUsingRowSet<T1,T2,T3,T4>(tablePredicate, pattern);

            if (pattern.Arg1.IsInstantiated && TablePredicate.IndexFor(0, true) is KeyIndex<(T1,T2, T3, T4), T1> index)
                return new TableCallWithKey<T1, T1, T2, T3, T4>(tablePredicate, pattern, index, pattern.Arg1.ValueCell);
            if (pattern.Arg2.IsInstantiated && TablePredicate.IndexFor(1, true) is KeyIndex<(T1,T2, T3, T4), T2> index2)
                return new TableCallWithKey<T2, T1, T2, T3, T4>(tablePredicate, pattern, index2, pattern.Arg2.ValueCell);
            if (pattern.Arg3.IsInstantiated && TablePredicate.IndexFor(2, true) is KeyIndex<(T1,T2, T3, T4), T3> index3)
                return new TableCallWithKey<T3, T1, T2, T3, T4>(tablePredicate, pattern, index3, pattern.Arg3.ValueCell);
            if (pattern.Arg4.IsInstantiated && TablePredicate.IndexFor(3, true) is KeyIndex<(T1,T2, T3, T4), T4> index4)
                return new TableCallWithKey<T4, T1, T2, T3, T4>(tablePredicate, pattern, index4, pattern.Arg4.ValueCell);

            if (pattern.Arg1.IsInstantiated && TablePredicate.IndexFor(0, true) is GeneralIndex<(T1,T2, T3, T4), T1> g)
                return new TableCallWithGeneralIndex<T1, T1, T2, T3, T4>(tablePredicate, pattern, g, pattern.Arg1.ValueCell);
            if (pattern.Arg2.IsInstantiated && TablePredicate.IndexFor(1, true) is GeneralIndex<(T1,T2, T3, T4), T2> g2)
                return new TableCallWithGeneralIndex<T2, T1, T2, T3, T4>(tablePredicate, pattern, g2, pattern.Arg2.ValueCell);
            if (pattern.Arg3.IsInstantiated && TablePredicate.IndexFor(2, true) is GeneralIndex<(T1,T2, T3, T4), T3> g3)
                return new TableCallWithGeneralIndex<T3, T1, T2, T3, T4>(tablePredicate, pattern, g3, pattern.Arg3.ValueCell);
            if (pattern.Arg4.IsInstantiated && TablePredicate.IndexFor(3, true) is GeneralIndex<(T1,T2, T3, T4), T4> g4)
                return new TableCallWithGeneralIndex<T4, T1, T2, T3, T4>(tablePredicate, pattern, g4, pattern.Arg4.ValueCell);

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
            var (head, body) = Preprocessor.GenerateCalls(tc, this, subgoals);
            TablePredicate.AddRule(new Rule<T1, T2, T3, T4>((TablePredicate<T1, T2, T3, T4>)TablePredicate, head.MakePattern(tc), body, tc.Dependencies, tc.VariableValueCells()));
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

        internal override AnyGoal RenameArguments(Substitution s)
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
            if (TablePredicate.Unique && pattern.IsInstantiated)
                return new TableCallUsingRowSet<T1,T2,T3,T4,T5>(tablePredicate, pattern);

            if (pattern.Arg1.IsInstantiated && TablePredicate.IndexFor(0, true) is KeyIndex<(T1,T2, T3, T4, T5), T1> index)
                return new TableCallWithKey<T1, T1, T2, T3, T4, T5>(tablePredicate, pattern, index, pattern.Arg1.ValueCell);
            if (pattern.Arg2.IsInstantiated && TablePredicate.IndexFor(1, true) is KeyIndex<(T1,T2, T3, T4, T5), T2> index2)
                return new TableCallWithKey<T2, T1, T2, T3, T4, T5>(tablePredicate, pattern, index2, pattern.Arg2.ValueCell);
            if (pattern.Arg3.IsInstantiated && TablePredicate.IndexFor(2, true) is KeyIndex<(T1,T2, T3, T4, T5), T3> index3)
                return new TableCallWithKey<T3, T1, T2, T3, T4, T5>(tablePredicate, pattern, index3, pattern.Arg3.ValueCell);
            if (pattern.Arg4.IsInstantiated && TablePredicate.IndexFor(3, true) is KeyIndex<(T1,T2, T3, T4, T5), T4> index4)
                return new TableCallWithKey<T4, T1, T2, T3, T4, T5>(tablePredicate, pattern, index4, pattern.Arg4.ValueCell);
            if (pattern.Arg5.IsInstantiated && TablePredicate.IndexFor(4, true) is KeyIndex<(T1,T2, T3, T4, T5), T5> index5)
                return new TableCallWithKey<T5, T1, T2, T3, T4, T5>(tablePredicate, pattern, index5, pattern.Arg5.ValueCell);

            if (pattern.Arg1.IsInstantiated && TablePredicate.IndexFor(0, true) is GeneralIndex<(T1,T2, T3, T4, T5), T1> g)
                return new TableCallWithGeneralIndex<T1, T1, T2, T3, T4, T5>(tablePredicate, pattern, g, pattern.Arg1.ValueCell);
            if (pattern.Arg2.IsInstantiated && TablePredicate.IndexFor(1, true) is GeneralIndex<(T1,T2, T3, T4, T5), T2> g2)
                return new TableCallWithGeneralIndex<T2, T1, T2, T3, T4, T5>(tablePredicate, pattern, g2, pattern.Arg2.ValueCell);
            if (pattern.Arg3.IsInstantiated && TablePredicate.IndexFor(2, true) is GeneralIndex<(T1,T2, T3, T4, T5), T3> g3)
                return new TableCallWithGeneralIndex<T3, T1, T2, T3, T4, T5>(tablePredicate, pattern, g3, pattern.Arg3.ValueCell);
            if (pattern.Arg4.IsInstantiated && TablePredicate.IndexFor(3, true) is GeneralIndex<(T1,T2, T3, T4, T5), T4> g4)
                return new TableCallWithGeneralIndex<T4, T1, T2, T3, T4, T5>(tablePredicate, pattern, g4, pattern.Arg4.ValueCell);
            if (pattern.Arg5.IsInstantiated && TablePredicate.IndexFor(4, true) is GeneralIndex<(T1,T2, T3, T4, T5), T5> g5)
                return new TableCallWithGeneralIndex<T5, T1, T2, T3, T4, T5>(tablePredicate, pattern, g5, pattern.Arg5.ValueCell);

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
            var (head, body) = Preprocessor.GenerateCalls(tc, this, subgoals);
            TablePredicate.AddRule(new Rule<T1, T2, T3, T4, T5>((TablePredicate<T1, T2, T3, T4, T5>)TablePredicate, head.MakePattern(tc), body, tc.Dependencies, tc.VariableValueCells()));
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

        internal override AnyGoal RenameArguments(Substitution s)
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
            if (TablePredicate.Unique && pattern.IsInstantiated)
                return new TableCallUsingRowSet<T1,T2,T3,T4,T5,T6>(tablePredicate, pattern);

            if (pattern.Arg1.IsInstantiated && TablePredicate.IndexFor(0, true) is KeyIndex<(T1,T2, T3, T4, T5, T6), T1> index)
                return new TableCallWithKey<T1, T1, T2, T3, T4, T5, T6>(tablePredicate, pattern, index, pattern.Arg1.ValueCell);
            if (pattern.Arg2.IsInstantiated && TablePredicate.IndexFor(1, true) is KeyIndex<(T1,T2, T3, T4, T5, T6), T2> index2)
                return new TableCallWithKey<T2, T1, T2, T3, T4, T5, T6>(tablePredicate, pattern, index2, pattern.Arg2.ValueCell);
            if (pattern.Arg3.IsInstantiated && TablePredicate.IndexFor(2, true) is KeyIndex<(T1,T2, T3, T4, T5, T6), T3> index3)
                return new TableCallWithKey<T3, T1, T2, T3, T4, T5, T6>(tablePredicate, pattern, index3, pattern.Arg3.ValueCell);
            if (pattern.Arg4.IsInstantiated && TablePredicate.IndexFor(3, true) is KeyIndex<(T1,T2, T3, T4, T5, T6), T4> index4)
                return new TableCallWithKey<T4, T1, T2, T3, T4, T5, T6>(tablePredicate, pattern, index4, pattern.Arg4.ValueCell);
            if (pattern.Arg5.IsInstantiated && TablePredicate.IndexFor(4, true) is KeyIndex<(T1,T2, T3, T4, T5, T6), T5> index5)
                return new TableCallWithKey<T5, T1, T2, T3, T4, T5, T6>(tablePredicate, pattern, index5, pattern.Arg5.ValueCell);
            if (pattern.Arg6.IsInstantiated && TablePredicate.IndexFor(5, true) is KeyIndex<(T1,T2, T3, T4, T5, T6), T6> index6)
                return new TableCallWithKey<T6, T1, T2, T3, T4, T5, T6>(tablePredicate, pattern, index6, pattern.Arg6.ValueCell);
            
            if (pattern.Arg1.IsInstantiated && TablePredicate.IndexFor(0, true) is GeneralIndex<(T1,T2, T3, T4, T5, T6), T1> g)
                return new TableCallWithGeneralIndex<T1, T1, T2, T3, T4, T5, T6>(tablePredicate, pattern, g, pattern.Arg1.ValueCell);
            if (pattern.Arg2.IsInstantiated && TablePredicate.IndexFor(1, true) is GeneralIndex<(T1,T2, T3, T4, T5, T6), T2> g2)
                return new TableCallWithGeneralIndex<T2, T1, T2, T3, T4, T5, T6>(tablePredicate, pattern, g2, pattern.Arg2.ValueCell);
            if (pattern.Arg3.IsInstantiated && TablePredicate.IndexFor(2, true) is GeneralIndex<(T1,T2, T3, T4, T5, T6), T3> g3)
                return new TableCallWithGeneralIndex<T3, T1, T2, T3, T4, T5, T6>(tablePredicate, pattern, g3, pattern.Arg3.ValueCell);
            if (pattern.Arg4.IsInstantiated && TablePredicate.IndexFor(3, true) is GeneralIndex<(T1,T2, T3, T4, T5, T6), T4> g4)
                return new TableCallWithGeneralIndex<T4, T1, T2, T3, T4, T5, T6>(tablePredicate, pattern, g4, pattern.Arg4.ValueCell);
            if (pattern.Arg5.IsInstantiated && TablePredicate.IndexFor(4, true) is GeneralIndex<(T1,T2, T3, T4, T5, T6), T5> g5)
                return new TableCallWithGeneralIndex<T5, T1, T2, T3, T4, T5, T6>(tablePredicate, pattern, g5, pattern.Arg5.ValueCell);
            if (pattern.Arg6.IsInstantiated && TablePredicate.IndexFor(5, true) is GeneralIndex<(T1,T2, T3, T4, T5, T6), T6> g6)
                return new TableCallWithGeneralIndex<T6, T1, T2, T3, T4, T5, T6>(tablePredicate, pattern, g6, pattern.Arg6.ValueCell);

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
            var (head, body) = Preprocessor.GenerateCalls(tc, this, subgoals);
            TablePredicate.AddRule(new Rule<T1, T2, T3, T4, T5, T6>((TablePredicate<T1, T2, T3, T4, T5, T6>)TablePredicate, head.MakePattern(tc), body, tc.Dependencies, tc.VariableValueCells()));
        }
    }
}
