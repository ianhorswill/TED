using System.Collections.Generic;
using System.Linq;
using static TED.Definition;

namespace TED
{
    internal static class Preprocessor
    {
        public static AnyCall[] GenerateCalls(GoalAnalyzer ga, params AnyGoal[] body) =>
            Expand(body).Select(s => s.MakeCall(ga)).ToArray();

        public static AnyCall BodyToCall(GoalAnalyzer ga, params AnyGoal[] body)
        {
            var calls = GenerateCalls(ga, body);
            if (calls.Length == 1)
                return calls[0];
            return new AndPrimitive.Call(calls, body);
        }

        public static (AnyCall Call, GoalAnalyzer Bindings) BodyToCallWithLocalBindings(GoalAnalyzer ga, params AnyGoal[] body)
        {
            var goalAnalyzer = ga.MakeChild();
            return (BodyToCall(goalAnalyzer, body), goalAnalyzer);
        }

        public static IEnumerable<AnyGoal> Expand(AnyGoal g)
        {
            if (!(g is AnyDefinitionGoal dg))
                return new[] { g };
            return dg.Expand();
        }

        public static IEnumerable<AnyGoal> Expand(IEnumerable<AnyGoal> body) => body.SelectMany(Expand);
    }
}
