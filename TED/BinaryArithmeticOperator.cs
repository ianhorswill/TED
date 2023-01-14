using System;

namespace TED
{
    /// <summary>
    /// Wrapper for C# binary operators (e.g. +, *, %, etc.) as TEDFunctions.
    /// </summary>
    internal sealed class BinaryArithmeticOperator<T> : TedFunction<T,T,T>
    {
        internal static readonly BinaryArithmeticOperator<T> Add = new BinaryArithmeticOperator<T>("+", "op_Addition");
        internal static readonly BinaryArithmeticOperator<T> Subtract = new BinaryArithmeticOperator<T>("-", "op_Subtraction");
        internal static readonly BinaryArithmeticOperator<T> Multiply = new BinaryArithmeticOperator<T>("*", "op_Multiply");
        internal static readonly BinaryArithmeticOperator<T> Divide = new BinaryArithmeticOperator<T>("/", "op_Division");
        internal static readonly BinaryArithmeticOperator<T> Modulus = new BinaryArithmeticOperator<T>("/", "op_Modulus");

        internal static readonly BinaryArithmeticOperator<T> BitwiseOr = new BinaryArithmeticOperator<T>("|", "op_BitwiseOr");
        internal static readonly BinaryArithmeticOperator<T> BitwiseAnd = new BinaryArithmeticOperator<T>("&", "op_BitwiseAnd");
        internal static readonly BinaryArithmeticOperator<T> BitwiseXor = new BinaryArithmeticOperator<T>("^", "op_ExclusiveOr");

        private static Func<T, T, T> LookupOperator(string name, string methodName)
        {
            var type = typeof(T);
            var methodInfo = type.GetOperatorMethodInfo(methodName, type, type, type);
            if (methodInfo == null)
                throw new ArgumentException($"There is no {name} overload defined for type {type.Name} that maps two {type.Name}s to another {type.Name}");
            return (Func<T, T, T>)methodInfo.CreateDelegate(typeof(Func<T, T, T>));
        }
        private BinaryArithmeticOperator(string name, string methodName) : base(name, LookupOperator(name, methodName))
        {

        }
    }

}
