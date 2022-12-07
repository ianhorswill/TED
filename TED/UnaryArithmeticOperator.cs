using System;

namespace TED
{
    public class UnaryArithmeticOperator<T> : TedFunction<T,T>
    {
        public static readonly UnaryArithmeticOperator<T> Negate = new UnaryArithmeticOperator<T>("-", "op_UnaryNegation");
        public static readonly UnaryArithmeticOperator<T> BitwiseNot = new UnaryArithmeticOperator<T>("~", "op_OnesComplement");

        private static Func<T, T> LookupOperator(string name, string methodName)
        {
            var methodInfo = typeof(T).GetOperatorMethodInfo(methodName);
            if (methodInfo == null)
                throw new ArgumentException($"There is no {name} overload defined for type {typeof(T).Name}");
            return (Func<T, T>)methodInfo.CreateDelegate(typeof(Func<T, T>));
        }
        private UnaryArithmeticOperator(string name, string methodName) : base(name, LookupOperator(name, methodName))
        {

        }
    }

}
