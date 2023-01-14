using System;

namespace TED
{
    /// <summary>
    /// Wrapper for one of the unary "op_" methods that operator expressions in C# compile to.
    /// </summary>
    internal class UnaryArithmeticOperator<T> : TedFunction<T,T>
    {
        internal static readonly UnaryArithmeticOperator<T> Negate = new UnaryArithmeticOperator<T>("-", "op_UnaryNegation");
        internal static readonly UnaryArithmeticOperator<T> BitwiseNot = new UnaryArithmeticOperator<T>("~", "op_OnesComplement");

        private static Func<T, T> LookupOperator(string name, string methodName)
        {
            var type = typeof(T);
            var methodInfo = type.GetOperatorMethodInfo(methodName, type, type);
            if (methodInfo == null)
                throw new ArgumentException($"There is no {name} overload defined for type {type.Name} that maps a {type.Name} to itself");
            return (Func<T, T>)methodInfo.CreateDelegate(typeof(Func<T, T>));
        }
        private UnaryArithmeticOperator(string name, string methodName) : base(name, LookupOperator(name, methodName))
        {

        }
    }

}
