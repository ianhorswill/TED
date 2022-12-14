using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TED
{
    public class AggregateFunctionCall<T> : FunctionalExpression<T>
    {
        public readonly T InitialValue;
        public readonly Func<T, T, T> Aggregator;
        public readonly AnyGoal Goal;
        public readonly Term<T> AggregationTerm;

        public AggregateFunctionCall(AnyGoal goal, T initialValue, Func<T, T, T> aggregator, Term<T> aggregationTerm)
        {
            InitialValue = initialValue;
            Aggregator = aggregator;
            AggregationTerm = aggregationTerm;
            Goal = goal;
        }

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
    }
}
