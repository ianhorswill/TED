using System.Linq;

namespace TED
{
    internal static class RulePreprocessor
    {
        public static AnyCall[] GenerateCalls(GoalAnalyzer ga, AnyGoal[] body) =>
            Definition.Expand(body).Select(s => s.MakeCall(ga)).ToArray();
    }
}
