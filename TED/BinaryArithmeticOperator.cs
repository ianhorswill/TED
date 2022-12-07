using System;

namespace TED
{
    public class BinaryArithmeticOperator<T> : TedFunction<T,T,T>
    {
        public static readonly BinaryArithmeticOperator<T> Add = new BinaryArithmeticOperator<T>("+", "op_Addition");
        public static readonly BinaryArithmeticOperator<T> Subtract = new BinaryArithmeticOperator<T>("-", "op_Subtraction");
        public static readonly BinaryArithmeticOperator<T> Multiply = new BinaryArithmeticOperator<T>("*", "op_Multiply");
        public static readonly BinaryArithmeticOperator<T> Divide = new BinaryArithmeticOperator<T>("/", "op_Division");
        public static readonly BinaryArithmeticOperator<T> Modulus = new BinaryArithmeticOperator<T>("/", "op_Modulus");

        public static readonly BinaryArithmeticOperator<T> BitwiseOr = new BinaryArithmeticOperator<T>("|", "op_BitwiseOr");
        public static readonly BinaryArithmeticOperator<T> BitwiseAnd = new BinaryArithmeticOperator<T>("&", "op_BitwiseAnd");
        public static readonly BinaryArithmeticOperator<T> BitwiseXor = new BinaryArithmeticOperator<T>("^", "op_ExclusiveOr");

        private static Func<T, T, T> LookupOperator(string name, string methodName)
        {
            var methodInfo = typeof(T).GetOperatorMethodInfo(methodName);
            if (methodInfo == null)
                throw new ArgumentException($"There is no {name} overload defined for type {typeof(T).Name}");
            return (Func<T, T, T>)methodInfo.CreateDelegate(typeof(Func<T, T, T>));
        }
        private BinaryArithmeticOperator(string name, string methodName) : base(name, LookupOperator(name, methodName))
        {

        }
    }

}
