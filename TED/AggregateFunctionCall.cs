using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TED
{
    /// <summary>
    /// Implements functions that aggregate over all the solutions of a query, such as Sum or Count.
    /// </summary>
    public sealed class AggregateFunctionCall<T> : FunctionalExpression<T>
    {
        public readonly T InitialValue;
        public readonly Func<T, T, T> Aggregator;
        public readonly Goal Goal;
        public readonly Term<T> AggregationTerm;

        /// <summary>
        /// Make a new function that aggregates over all the solutions to a goal.
        /// After exit from the function, any variables bound within the goal are left unbound.
        /// If the goal is P[x] and x is the aggregation term, then it will find all the x's from all
        /// the solutions to P[x], and return aggregator(aggregator(aggregator(initialValue, x1), x2), x3) ...
        /// </summary>
        /// <param name="goal">Goal to find solutions to</param>
        /// <param name="aggregationTerm">Value to take from each solution; the values from all solutions will be aggregated together</param>
        /// <param name="initialValue">Initial value to start from when aggregating</param>
        /// <param name="aggregator">C# function mapping two values to an aggregate value</param>
        public AggregateFunctionCall(Goal goal, Term<T> aggregationTerm, T initialValue, Func<T, T, T> aggregator)
        {
            InitialValue = initialValue;
            Aggregator = aggregator;
            AggregationTerm = aggregationTerm;
            Goal = goal;
        }


        /// <inheritdoc />
        internal override Func<T> MakeEvaluator(GoalAnalyzer ga)
        {
            var (call, bindings) = Preprocessor.BodyToCallWithLocalBindings(ga, Goal);
            var term = bindings.Emit(AggregationTerm);
            if (!term.IsInstantiated)
                throw new InstantiationException(
                    $"Value to aggregate, {AggregationTerm} is not instantiated after the call");
            var cell = term.ValueCell;
            return () =>
            {
                T result = InitialValue;
                call.Reset();
                while (call.NextSolution())
                    result = Aggregator(result, cell.Value);
                return result;
            };
        }

        internal override Term<T> RecursivelySubstitute(Substitution s)
            => new AggregateFunctionCall<T>(Goal.RenameArguments(s), s.Substitute(AggregationTerm), InitialValue,
                Aggregator);
    }
}
