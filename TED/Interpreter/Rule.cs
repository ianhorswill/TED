using System.Text;
using System;
using System.Diagnostics;
using System.Linq;

namespace TED.Interpreter
{
    /// <summary>
    /// Untyped base class of all Rules
    /// Rule objects are the preprocessed form of rules specified using .If or .Fact.
    /// They consist of the Pattern for the head of the rule and an array of call objects
    /// for each of the subgoals in the body
    /// </summary>
    public abstract class Rule
    {
        /// <summary>
        /// Predicate this rule applies to.
        /// Rules can only be applied to TablePredicates, not primitives or definitions
        /// </summary>
        public readonly TablePredicate Predicate;

        /// <summary>
        /// Pattern object for the head of the rule.
        /// The head of a rule is its "conclusion" - the thing that has to be true when the body is true.
        /// </summary>
        public abstract IPattern Head { get; }

        /// <summary>
        /// Body of the rule - a sequence of Call objects for each goal in the If.
        /// </summary>
        public readonly Call[] Body;
        /// <summary>
        /// All the TablePredicates called directly by this rule.
        /// These tables must be computed before this rule runs.
        /// </summary>
        public readonly TablePredicate[] Dependencies;

        /// <summary>
        /// Cells holding the values of the variables in the rule.
        /// </summary>
        public readonly ValueCell[] ValueCells;

        /// <summary>
        /// True if the rule only calls pure predicates.
        /// </summary>
        public bool IsPure => Body.All(c => c.IsPure);

#if PROFILER
        private readonly System.Diagnostics.Stopwatch executionTimer = new System.Diagnostics.Stopwatch();
        private int executionCount;

        /// <summary>
        /// Total execution time spent in this rule.
        /// </summary>
        public float TotalExecutionTime => executionTimer.ElapsedMilliseconds;

        /// <summary>
        /// Execution time for the most recent execution of this rule.
        /// </summary>
        public float InstantaneousExecutionTime;

        /// <summary>
        /// Average number of milliseconds required to run this rule
        /// </summary>
        public float AverageExecutionTime;

        public const float ExecutionTimeAveragingTimeConstant = 0.1f;
#endif

#pragma warning disable CS1591
        protected Rule(TablePredicate predicate, Call[] body, TablePredicate[] dependencies, ValueCell[] valueCells)
#pragma warning restore CS1591
        {
            Predicate = predicate;
            Body = body;
            Dependencies = dependencies;
            ValueCells = valueCells;
        }

        /// <summary>
        /// Called after the body has computed a row and written the values of variables into it.
        /// Writes the values of each element of the row into the table.
        /// </summary>
        internal abstract void WriteHead();

        /// <summary>
        /// Repeatedly runs all the calls in the body and backtracks them to completion
        /// Calls WriteHead each time a solution is found to write it into the table.
        /// </summary>
        internal void AddAllSolutions()
        {
#if PROFILER
            var ticksBefore = executionTimer.ElapsedTicks;
            executionTimer.Start();
            executionCount++;
#endif

            var subgoal = 0;
            try
            {
                if (Body.Length == 0)
                {
                    WriteHead();
                    return;
                }

                foreach (var d in Dependencies)
                    d.EnsureUpToDate();

                Body[subgoal].Reset();
                while (subgoal >= 0)
                {
                    if (Body[subgoal].NextSolution())
                    {
                        // Succeeded
                        if (subgoal == Body.Length - 1)
                            WriteHead();
                        else
                        {
                            // Advance to the next subgoal
                            subgoal++;
                            Body[subgoal].Reset();
                        }
                    }
                    else
                        // Failed
                        subgoal--;
                }
            }
            catch (Exception e)
            {
                if (Predicate.Program != null)
                    Predicate.Program.Exceptions.AddRow(e.GetType(), e.Message, Predicate, this);
                throw new RuleExecutionException(this, Body[subgoal], e);
            }

#if PROFILER
            executionTimer.Stop();
            var ticksThisTime = executionTimer.ElapsedTicks - ticksBefore;
            InstantaneousExecutionTime = ticksThisTime * (1f / TimeSpan.TicksPerMillisecond);
            AverageExecutionTime = ExecutionTimeAveragingTimeConstant * InstantaneousExecutionTime +
                                   (1f - ExecutionTimeAveragingTimeConstant) * AverageExecutionTime;
#endif
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var b = new StringBuilder();
            b.Append(Predicate.Name);
            b.Append(Head);
            b.Append(".If(");
            bool firstOne = true;
            foreach (var c in Body)
            {
                if (firstOne)
                    firstOne = false;
                else b.Append(", ");
                b.Append(c);
            }

            b.Append(")");
            return b.ToString();
        }
    }

    /// <summary>
    /// Represents the final, "compiled" representation of a rule.
    /// </summary>
    /// <typeparam name="T1">Type of the argument to the rule's head (conclusion)</typeparam>
    internal sealed class Rule<T1> : Rule
    {
        public readonly TablePredicate<T1> TablePredicate;
        public readonly Pattern<T1> HeadPattern;

        public override IPattern Head => HeadPattern;

        public Rule(TablePredicate<T1> predicate, Pattern<T1> headPattern, Call[] body, TablePredicate[] dependencies, ValueCell[] valueCells)
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
    internal sealed class Rule<T1, T2> : Rule
    {
        public readonly TablePredicate<T1, T2> TablePredicate;
        public readonly Pattern<T1, T2> HeadPattern;
        public override IPattern Head => HeadPattern;

        public Rule(TablePredicate<T1, T2> predicate, Pattern<T1, T2> headPattern, Call[] body, TablePredicate[] dependencies, ValueCell[] valueCells)
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
    internal sealed class Rule<T1, T2, T3> : Rule
    {
        public readonly TablePredicate<T1, T2, T3> TablePredicate;
        public readonly Pattern<T1, T2, T3> HeadPattern;
        public override IPattern Head => HeadPattern;

        public Rule(TablePredicate<T1, T2, T3> predicate, Pattern<T1, T2, T3> headPattern, Call[] body, TablePredicate[] dependencies, ValueCell[] valueCells)
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
    internal sealed class Rule<T1, T2, T3, T4> : Rule
    {
        public readonly TablePredicate<T1, T2, T3, T4> TablePredicate;
        public readonly Pattern<T1, T2, T3, T4> HeadPattern;
        public override IPattern Head => HeadPattern;

        public Rule(TablePredicate<T1, T2, T3, T4> predicate, Pattern<T1, T2, T3, T4> headPattern, Call[] body, TablePredicate[] dependencies, ValueCell[] valueCells)
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
    internal sealed class Rule<T1, T2, T3, T4, T5> : Rule
    {
        public readonly TablePredicate<T1, T2, T3, T4, T5> TablePredicate;
        public readonly Pattern<T1, T2, T3, T4, T5> HeadPattern;
        public override IPattern Head => HeadPattern;

        public Rule(TablePredicate<T1, T2, T3, T4, T5> predicate, Pattern<T1, T2, T3, T4, T5> headPattern, Call[] body, TablePredicate[] dependencies, ValueCell[] valueCells)
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
    internal sealed class Rule<T1, T2, T3, T4, T5, T6> : Rule
    {
        public readonly TablePredicate<T1, T2, T3, T4, T5, T6> TablePredicate;
        public readonly Pattern<T1, T2, T3, T4, T5, T6> HeadPattern;
        public override IPattern Head => HeadPattern;

        public Rule(TablePredicate<T1, T2, T3, T4, T5, T6> predicate, Pattern<T1, T2, T3, T4, T5, T6> headPattern, Call[] body, TablePredicate[] dependencies, ValueCell[] valueCells)
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
    /// <typeparam name="T7">Type of the seventh argument to the rule's head</typeparam>
    internal sealed class Rule<T1, T2, T3, T4, T5, T6, T7> : Rule
    {
        public readonly TablePredicate<T1, T2, T3, T4, T5, T6, T7> TablePredicate;
        public readonly Pattern<T1, T2, T3, T4, T5, T6, T7> HeadPattern;
        public override IPattern Head => HeadPattern;

        public Rule(TablePredicate<T1, T2, T3, T4, T5, T6, T7> predicate, Pattern<T1, T2, T3, T4, T5, T6, T7> headPattern, Call[] body, TablePredicate[] dependencies, ValueCell[] valueCells)
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
    /// <typeparam name="T7">Type of the seventh argument to the rule's head</typeparam>
    /// <typeparam name="T8">Type of the eighth argument to the rule's head</typeparam>
    internal sealed class Rule<T1, T2, T3, T4, T5, T6, T7, T8> : Rule
    {
        public readonly TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> TablePredicate;
        public readonly Pattern<T1, T2, T3, T4, T5, T6, T7, T8> HeadPattern;
        public override IPattern Head => HeadPattern;

        public Rule(TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> predicate, Pattern<T1, T2, T3, T4, T5, T6, T7, T8> headPattern, Call[] body, TablePredicate[] dependencies, ValueCell[] valueCells)
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
