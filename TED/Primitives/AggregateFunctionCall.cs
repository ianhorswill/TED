using System;
using TED.Interpreter;
using TED.Preprocessing;

namespace TED.Primitives
{
    /// <summary>
    /// Implements functions that aggregate over all the solutions of a query, such as Sum or Count.
    /// </summary>
    public sealed class AggregateFunctionCall<T> : FunctionalExpression<T>
    {
        /// <summary>
        /// Starting value for the accumulator
        /// </summary>
        public readonly T InitialValue;
        /// <summary>
        /// Function used to add a new value to the accumulator
        /// </summary>
        public readonly Func<T, T, T> Aggregator;
        /// <summary>
        /// Goal that generates values for the AggregationTerm
        /// </summary>
        public readonly Goal Generator;
        /// <summary>
        /// Term to be aggregated (summed, multiplied, etc.) over the solutions to the Generator.
        /// </summary>
        public readonly Term<T> AggregationTerm;

        /// <summary>
        /// Make a new function that aggregates over all the solutions to a goal.
        /// After exit from the function, any variables bound within the goal are left unbound.
        /// If the goal is P[x] and x is the aggregation term, then it will find all the x's from all
        /// the solutions to P[x], and return aggregator(aggregator(aggregator(initialValue, x1), x2), x3) ...
        /// </summary>
        /// <param name="generator">Goal to find solutions to</param>
        /// <param name="aggregationTerm">Value to take from each solution; the values from all solutions will be aggregated together</param>
        /// <param name="initialValue">Initial value to start from when aggregating</param>
        /// <param name="aggregator">C# function mapping two values to an aggregate value</param>
        public AggregateFunctionCall(Goal generator, Term<T> aggregationTerm, T initialValue, Func<T, T, T> aggregator)
        {
            InitialValue = initialValue;
            Aggregator = aggregator;
            AggregationTerm = aggregationTerm;
            Generator = generator;
        }


        /// <inheritdoc />
        internal override Func<T> MakeEvaluator(GoalAnalyzer ga)
        {
            var (call, bindings) = Preprocessor.BodyToCallWithLocalBindings(ga, Generator);
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
            => new AggregateFunctionCall<T>(Generator.RenameArguments(s), s.Substitute(AggregationTerm), InitialValue,
                Aggregator);
    }
}
