using System.Collections.Generic;
using System.Linq;
using static TED.Definition;

namespace TED
{
    internal static class Preprocessor
    {
        public static AnyCall[] GenerateCalls(GoalAnalyzer ga, params AnyGoal[] body)
        {
            var hoisted = body.SelectMany(HoistFunctionalExpressions).ToArray();
            var expanded = Expand(hoisted).ToArray();
            return expanded.Select(s => s.MakeCall(ga)).ToArray();
        }

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

        public static IEnumerable<AnyGoal> Expand(IEnumerable<AnyGoal> body) => body.SelectMany(g => Expand(g));

        public static IEnumerable<AnyTerm> FunctionalExpressions(AnyGoal g) =>
            g.Arguments.Where(a => a.IsFunctionalExpression);

        public static IEnumerable<AnyGoal> HoistFunctionalExpressions(AnyGoal g)
        {
            if (g.Predicate is IMatchPrimitive)
                return new[] { g };
            var fes = FunctionalExpressions(g).ToArray();
            if (!fes.Any())
                return new[] { g };
            var liftedExpressions = fes.Select(fe => fe.HoistInfo()).ToArray();
            var newG = g.RenameArguments(new Substitution(liftedExpressions.Select(l => (l.Expression, l.Var)), false));
            return liftedExpressions.Select(l => l.MatchGoal).Append(newG);
        }
    }
}
