using TED.Interpreter;
using TED.Preprocessing;

namespace TED.Primitives
{
    /// <summary>
    /// Pseudo-primitive that checks the binding state of its argument and throws if it isn't the expected state.
    /// Does nothing if it is of the expected state.
    /// </summary>
    internal sealed class AssertBindingStatePrimitive<T> : PrimitivePredicate<T, string>
    {
        public static AssertBindingStatePrimitive<T> AssertIn 
            = new AssertBindingStatePrimitive<T>("AssertIn",(arg, inst) => inst);
        public static AssertBindingStatePrimitive<T> AssertOut 
            = new AssertBindingStatePrimitive<T>("AssertOut",(arg, inst) => !inst);
        public static AssertBindingStatePrimitive<T> AssertConstant 
            = new AssertBindingStatePrimitive<T>("AssertConstant",(arg, inst) => arg is Constant<T>);

        private delegate bool InstantiationTest(Term<T> arg, bool isInstantiated);
        private readonly InstantiationTest test;

        private AssertBindingStatePrimitive(string name, InstantiationTest test) : base(name)
        {
            this.test = test;
        }

        public override Call MakeCall(Goal g, GoalAnalyzer tc)
        {
            var arg = g.Arg1;
            var message = ((Constant<string>)g.Arg2).Value;
            if (!test(arg, !(arg is IVariable) || tc.IsInstantiated(arg)))
                throw new InstantiationException($"{message}: {arg}");
            return null!;
        }
    }
}
