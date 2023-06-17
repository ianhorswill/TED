using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TED.Interpreter;
using TED.Preprocessing;

namespace TED.Primitives
{
    internal class CaptureDebugStatePrimitive : PrimitivePredicate<Dictionary<string,object?>>
    {
        public static CaptureDebugStatePrimitive Singleton = new CaptureDebugStatePrimitive("CaptureDebugState");

        internal static readonly Var<Dictionary<string, object?>> DebugState =
            new Var<Dictionary<string, object?>>("debugState");

        internal static readonly Interpreter.Goal DefaultGoal = Singleton[DebugState];

        private CaptureDebugStatePrimitive(string name) : base(name)
        {
        }

        public override Interpreter.Call MakeCall(Goal g, GoalAnalyzer tc)
        {
            if (tc.IsInstantiated(g.Arg1))
                throw new InstantiationException(this, g.Arg1);
            // This needs to run before the tc.Emit() below, so we don't capture the argument itself.
            var stateVariables = tc.BoundVariables.Select(tc.ValueCell).ToArray();  
            return new Call(this, tc.Emit(g.Arg1), stateVariables);
        }

        private class Call : Interpreter.Call
        {
            public readonly MatchOperation<Dictionary<string, object?>> output;
            public readonly ValueCell[] StateVariables;
            private bool enabled;

            public Call(Predicate table, MatchOperation<Dictionary<string, object?>> output, ValueCell[] stateVariables) : base(table)
            {
                this.output = output;
                StateVariables = stateVariables;
                ArgumentPattern = new Pattern<Dictionary<string, object?>>(output);
            }

            public override IPattern ArgumentPattern { get; }
            public override void Reset()
            {
                enabled = true;
            }

            public override bool NextSolution()
            {
                if (!enabled) return false;
                enabled = false;
                return output.Match(new Dictionary<string, object?>(
                    StateVariables.Select(c => new KeyValuePair<string, object?>(c.Name, c.BoxedValue)
                    )));
            }
        }
    }
}
