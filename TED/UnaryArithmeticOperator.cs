using System;

namespace TED
{
    /// <summary>
    /// Wrapper for one of the unary "op_" methods that operator expressions in C# compile to.
    /// </summary>
    internal abstract class UnaryArithmeticOperator<T> : TedFunction<T,T>
    {
        private static Func<T, T> LookupOperator(string name, string methodName)
        {
            var type = typeof(T);
            var methodInfo = type.GetOperatorMethodInfo(methodName, type, type);
            if (methodInfo == null)
                throw new ArgumentException($"There is no {name} overload defined for type {type.Name} that maps a {type.Name} to itself");
            return (Func<T, T>)methodInfo.CreateDelegate(typeof(Func<T, T>));
        }
        protected UnaryArithmeticOperator(string name, string methodName) : base(name, LookupOperator(name, methodName))
        {

        }

        public override bool RenderAsOperator => true;
    }

    internal sealed class NegationOperator<T> : UnaryArithmeticOperator<T>
    {
        internal static readonly NegationOperator<T> Singleton = new NegationOperator<T>();

        private NegationOperator() : base("-", "op_UnaryNegation")
        { }
    }

    internal sealed class BitwiseNegationOperator<T> : UnaryArithmeticOperator<T>
    {
        internal static readonly BitwiseNegationOperator<T> Singleton = new BitwiseNegationOperator<T>();

        private BitwiseNegationOperator() : base("~", "op_OnesComplement")
        { }
    }
}
