namespace TED
{
    /// <summary>
    /// Represents the final, "compiled" representation of a rule.
    /// </summary>
    /// <typeparam name="T1">Type of the argument to the rule's head (conclusion)</typeparam>
    internal sealed class Rule<T1> : AnyRule
    {
        public readonly TablePredicate<T1> TablePredicate;
        public readonly Pattern<T1> HeadPattern;

        public override IPattern Head => HeadPattern;

        public Rule(TablePredicate<T1> predicate, Pattern<T1> headPattern, AnyCall[] body, TablePredicate[] dependencies, ValueCell[] valueCells)
            : base(predicate, body, dependencies, valueCells)
        {
            HeadPattern = headPattern;
            TablePredicate = predicate;
        }

        internal override void WriteHead()
        {
            // Whole body succeed
            HeadPattern.Write(out var result);
            TablePredicate._table.Add(in result);
        }
    }

    /// <summary>
    /// Represents the final, "compiled" representation of a rule.
    /// </summary>
    /// <typeparam name="T1">Type of the first argument to the rule's head (conclusion)</typeparam>
    /// <typeparam name="T2">Type of the second argument to the rule's head</typeparam>
    internal sealed class Rule<T1, T2> : AnyRule
    {
        public readonly TablePredicate<T1, T2> TablePredicate;
        public readonly Pattern<T1, T2> HeadPattern;
        public override IPattern Head => HeadPattern;

        public Rule(TablePredicate<T1, T2> predicate, Pattern<T1, T2> headPattern, AnyCall[] body, TablePredicate[] dependencies, ValueCell[] valueCells)
            : base(predicate, body, dependencies, valueCells)
        {
            HeadPattern = headPattern;
            TablePredicate = predicate;
        }

        internal override void WriteHead()
        {
            // Whole body succeed
            HeadPattern.Write(out var result);
            TablePredicate._table.Add(in result);
        }
    }

    /// <summary>
    /// Represents the final, "compiled" representation of a rule.
    /// </summary>
    /// <typeparam name="T1">Type of the first argument to the rule's head (conclusion)</typeparam>
    /// <typeparam name="T2">Type of the second argument to the rule's head</typeparam>
    /// <typeparam name="T3">Type of the third argument to the rule's head</typeparam>
    internal sealed class Rule<T1, T2, T3> : AnyRule
    {
        public readonly TablePredicate<T1, T2, T3> TablePredicate;
        public readonly Pattern<T1, T2, T3> HeadPattern;
        public override IPattern Head => HeadPattern;

        public Rule(TablePredicate<T1, T2, T3> predicate, Pattern<T1, T2, T3> headPattern, AnyCall[] body, TablePredicate[] dependencies, ValueCell[] valueCells)
            : base(predicate, body, dependencies, valueCells)
        {
            HeadPattern = headPattern;
            TablePredicate = predicate;
        }

        internal override void WriteHead()
        {
            // Whole body succeed
            HeadPattern.Write(out var result);
            TablePredicate._table.Add(in result);
        }
    }

    /// <summary>
    /// Represents the final, "compiled" representation of a rule.
    /// </summary>
    /// <typeparam name="T1">Type of the first argument to the rule's head (conclusion)</typeparam>
    /// <typeparam name="T2">Type of the second argument to the rule's head</typeparam>
    /// <typeparam name="T3">Type of the third argument to the rule's head</typeparam>
    /// <typeparam name="T4">Type of the fourth argument to the rule's head</typeparam>
    internal sealed class Rule<T1, T2, T3, T4> : AnyRule
    {
        public readonly TablePredicate<T1, T2, T3, T4> TablePredicate;
        public readonly Pattern<T1, T2, T3, T4> HeadPattern;
        public override IPattern Head => HeadPattern;

        public Rule(TablePredicate<T1, T2, T3, T4> predicate, Pattern<T1, T2, T3, T4> headPattern, AnyCall[] body, TablePredicate[] dependencies, ValueCell[] valueCells)
            : base(predicate, body, dependencies, valueCells)
        {
            HeadPattern = headPattern;
            TablePredicate = predicate;
        }

        internal override void WriteHead()
        {
            // Whole body succeed
            HeadPattern.Write(out var result);
            TablePredicate._table.Add(in result);
        }
    }

    /// <summary>
    /// Represents the final, "compiled" representation of a rule.
    /// </summary>
    /// <typeparam name="T1">Type of the first argument to the rule's head (conclusion)</typeparam>
    /// <typeparam name="T2">Type of the second argument to the rule's head</typeparam>
    /// <typeparam name="T3">Type of the third argument to the rule's head</typeparam>
    /// <typeparam name="T4">Type of the fourth argument to the rule's head</typeparam>
    /// <typeparam name="T5">Type of the fifth argument to the rule's head</typeparam>
    internal sealed class Rule<T1, T2, T3, T4, T5> : AnyRule
    {
        public readonly TablePredicate<T1, T2, T3, T4, T5> TablePredicate;
        public readonly Pattern<T1, T2, T3, T4, T5> HeadPattern;
        public override IPattern Head => HeadPattern;

        public Rule(TablePredicate<T1, T2, T3, T4, T5> predicate, Pattern<T1, T2, T3, T4, T5> headPattern, AnyCall[] body, TablePredicate[] dependencies, ValueCell[] valueCells)
            : base(predicate, body, dependencies, valueCells)
        {
            HeadPattern = headPattern;
            TablePredicate = predicate;
        }

        internal override void WriteHead()
        {
            // Whole body succeed
            HeadPattern.Write(out var result);
            TablePredicate._table.Add(in result);
        }
    }

    /// <summary>
    /// Represents the final, "compiled" representation of a rule.
    /// </summary>
    /// <typeparam name="T1">Type of the first argument to the rule's head (conclusion)</typeparam>
    /// <typeparam name="T2">Type of the second argument to the rule's head</typeparam>
    /// <typeparam name="T3">Type of the third argument to the rule's head</typeparam>
    /// <typeparam name="T4">Type of the fourth argument to the rule's head</typeparam>
    /// <typeparam name="T5">Type of the fifth argument to the rule's head</typeparam>
    /// <typeparam name="T6">Type of the sixth argument to the rule's head</typeparam>
    internal sealed class Rule<T1, T2, T3, T4, T5, T6> : AnyRule
    {
        public readonly TablePredicate<T1, T2, T3, T4, T5, T6> TablePredicate;
        public readonly Pattern<T1, T2, T3, T4, T5, T6> HeadPattern;
        public override IPattern Head => HeadPattern;

        public Rule(TablePredicate<T1, T2, T3, T4, T5, T6> predicate, Pattern<T1, T2, T3, T4, T5, T6> headPattern, AnyCall[] body, TablePredicate[] dependencies, ValueCell[] valueCells)
            : base(predicate, body, dependencies, valueCells)
        {
            HeadPattern = headPattern;
            TablePredicate = predicate;
        }

        internal override void WriteHead()
        {
            // Whole body succeed
            HeadPattern.Write(out var result);
            TablePredicate._table.Add(in result);
        }
    }
}
