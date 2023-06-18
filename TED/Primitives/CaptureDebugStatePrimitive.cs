using System.Collections.Generic;
using System.Linq;
using System.Text;
using TED.Interpreter;
using TED.Preprocessing;

namespace TED.Primitives
{
    public class CaptureDebugStatePrimitive : PrimitivePredicate<CaptureDebugStatePrimitive.CapturedState>
    {
        public static CaptureDebugStatePrimitive Singleton = new CaptureDebugStatePrimitive("CaptureDebugState");

        internal static readonly Var<CapturedState> DebugState =
            new Var<CapturedState>("debugState");

        internal static readonly Interpreter.Goal DefaultGoal = Singleton[DebugState];

        private CaptureDebugStatePrimitive(string name) : base(name)
        {
        }

        public class CapturedState : Dictionary<string, object?>
        {
            public CapturedState(IEnumerable<KeyValuePair<string, object?>> bindings) : base(bindings)
            {
            }

            public override string ToString()
            {
                var b = new StringBuilder();
                foreach (var binding in this)
                {
                    b.Append(binding.Key);
                    b.Append(" = ");
                    if (binding.Value != null)
                        b.AppendLine(binding.Value.ToString());
                    else b.AppendLine("null");
                }
                return b.ToString();
            }
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
            public readonly MatchOperation<CapturedState> output;
            public readonly ValueCell[] StateVariables;
            private bool enabled;

            public Call(Predicate table, MatchOperation<CapturedState> output, ValueCell[] stateVariables) : base(table)
            {
                this.output = output;
                StateVariables = stateVariables;
                ArgumentPattern = new Pattern<CapturedState>(output);
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
                return output.Match(new CapturedState(
                    StateVariables.Select(c => new KeyValuePair<string, object?>(c.Name, c.BoxedValue)
                    )));
            }
        }
    }
}
