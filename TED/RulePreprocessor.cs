using System.Linq;

namespace TED
{
    internal static class RulePreprocessor
    {
        public static AnyCall[] GenerateCalls(GoalAnalyzer ga, params AnyGoal[] body) =>
            Definition.Expand(body).Select(s => s.MakeCall(ga)).ToArray();

        public static AnyCall BodyToCall(GoalAnalyzer ga, params AnyGoal[] body)
        {
            var calls = GenerateCalls(ga, body);
            if (calls.Length == 1)
                return calls[0];
            return new AndPrimitive.Call(calls, body);
        }
    }
}
