using System;
using System.Collections.Generic;
using System.Linq;
using static TED.Definition;

namespace TED
{
    /// <summary>
    /// Implements mapping of sequences of Goals (abstract syntax trees) to sequences of Calls (the internal
    /// representation/implementation of the calls).
    ///
    /// This process involves
    /// - Expanding calls to definitions
    /// - Hoisting functional expressions from calls: P(x+1) is transformed to Eval(temp, x+1), P(temp)
    /// - Analyzing which uses of variables are first uses (which bind) or subsequent uses (which match)
    /// </summary>
    internal static class Preprocessor
    {
        /// <summary>
        /// Reduce a series of goals to a different series in canonical form.
        /// Canonical form contains no calls to Definitions and has FunctionalExpressions only as arguments to Eval.
        /// </summary>
        public static IEnumerable<AnyGoal> CanonicalizeGoals(IEnumerable<AnyGoal> body)
            => ExpandDefinitions(body.SelectMany(HoistFunctionalExpressions).ToArray()).ToArray();

        /// <summary>
        /// Generate a series of Call objects for a body.
        /// </summary>
        public static AnyCall[] GenerateCalls(GoalAnalyzer ga, params AnyGoal[] body) 
            => CanonicalizeGoals(body).Select(s => s.MakeCall(ga)).ToArray();

        public static (T, AnyCall[]) GenerateCalls<T>(GoalAnalyzer ga, T head, AnyGoal[] body) where T : AnyTableGoal
        {
            var (_, hoistedHead, hoistedEvals) = HoistedVersion(head);
            var calls = CanonicalizeGoals(body.Concat(hoistedEvals)).Select(s => s.MakeCall(ga)).ToArray();
            return ((T)hoistedHead, calls);
        }

        /// <summary>
        /// Generate a single Call object that will run the goals in body.  The GoalAnalyzer will retain
        /// any bindings (first uses of variables) in the body.  And when executed, those variables will
        /// have whatever values they acquire in the body.
        /// 
        /// If the canonical form of body has only a single goal, then this will just be the call for the goal,
        /// otherwise it will be a call to And.
        /// </summary>
        public static AnyCall BodyToCall(GoalAnalyzer ga, params AnyGoal[] body)
        {
            var calls = GenerateCalls(ga, body);
            if (calls.Length == 1)
                return calls[0];
            return new AndPrimitive.Call(calls, body);
        }

        /// <summary>
        /// Generate a single Call object that will run the goals in body.  The GoalAnalyzer will not retain any
        /// bindings generated in the body.  That is, if there are any first uses of variables in the body, that
        /// will lead those variables to acquire values, those values will be thrown away afterward and the system
        /// will treat those variables as unbound after the body is executed.
        /// 
        /// If the canonical form of body has only a single goal, then this will just be the call for the goal,
        /// otherwise it will be a call to And.
        /// </summary>
        public static (AnyCall Call, GoalAnalyzer Bindings) BodyToCallWithLocalBindings(GoalAnalyzer ga, params AnyGoal[] body)
        {
            var goalAnalyzer = ga.MakeChild();
            return (BodyToCall(goalAnalyzer, body), goalAnalyzer);
        }

        /// <summary>
        /// If the goal is a call to a definition, return its expansion.  Otherwise, return the original goal.
        /// </summary>
        /// <param name="g">Goal to expand</param>
        public static IEnumerable<AnyGoal> ExpandDefinitions(AnyGoal g)
        {
            if (!(g is AnyDefinitionGoal dg))
                return new[] { g };
            return dg.Expand();
        }

        /// <summary>
        /// Expand any calls to definitions inside the body
        /// </summary>
        /// <param name="body">Sequence of Goals, some of which may be to Definitions</param>
        /// <returns>Expanded form with any calls to Definitions replaced with their expansions.</returns>
        public static IEnumerable<AnyGoal> ExpandDefinitions(IEnumerable<AnyGoal> body) => body.SelectMany(g => ExpandDefinitions(g));

        /// <summary>
        /// The arguments of the goal that happen to be functional expressions.
        /// </summary>
        public static IEnumerable<AnyTerm> FunctionalExpressions(AnyGoal g) =>
            g.Arguments.Where(a => a.IsFunctionalExpression);

        internal static (bool isHoisted, AnyGoal hoistedGoal, IEnumerable<AnyGoal> hoistedFunctionalExpressions)
            HoistedVersion(AnyGoal g)
        {
            if (g.Predicate is IEvalPrimitive)
                return (false, g, Array.Empty<AnyGoal>());

            var fes = FunctionalExpressions(g).ToArray();
            if (!fes.Any())
                return (false, g, Array.Empty<AnyGoal>());
            var liftedExpressions = fes.Select(fe => fe.HoistInfo()).ToArray();
            var newG = g.RenameArguments(new Substitution(liftedExpressions.Select(l => (l.Expression, l.Var)), false));
            return (true, newG, liftedExpressions.Select(l => l.EvalGoal));
        }

        /// <summary>
        /// Replace any calls to functional expressions in the Goal with temporary variables and prepend the
        /// transformed version with calls to Eval to bind the temporary variables with function expressions.
        /// Thus:
        /// - P(x,y) is unchanged; it maps to P(x,y)
        /// - P(x+1,y) is mapped to: Eval(t, x+1), P(t,y)
        /// </summary>
        internal static IEnumerable<AnyGoal> HoistFunctionalExpressions(AnyGoal g)
        {
            var (changed, goal, evalCalls) = HoistedVersion(g);
            if (!changed)
                return new[] { g };
            return evalCalls.Append(goal);
        }
    }
}
