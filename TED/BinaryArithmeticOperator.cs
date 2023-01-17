using System;

namespace TED
{
    /// <summary>
    /// Wrapper for C# binary operators (e.g. +, *, %, etc.) as TEDFunctions.
    /// </summary>
    internal abstract class BinaryArithmeticOperator<T> : TedFunction<T,T,T>
    {
        private static Func<T, T, T> LookupOperator(string name, string methodName)
        {
            var type = typeof(T);
            var methodInfo = type.GetOperatorMethodInfo(methodName, type, type, type);
            if (methodInfo == null)
                throw new ArgumentException($"There is no {name} overload defined for type {type.Name} that maps two {type.Name}s to another {type.Name}");
            return (Func<T, T, T>)methodInfo.CreateDelegate(typeof(Func<T, T, T>));
        }
        protected BinaryArithmeticOperator(string name, string methodName) : base(name, LookupOperator(name, methodName))
        {

        }

        public override bool RenderAsOperator => true;
    }

    internal sealed class AdditionOperator<T> : BinaryArithmeticOperator<T>
    {
        internal static readonly AdditionOperator<T> Singleton = new AdditionOperator<T>();

        private AdditionOperator() : base("+", "op_Addition")
        {
        }
    }

    internal sealed class SubtractionOperator<T> : BinaryArithmeticOperator<T>
    {
        internal static readonly SubtractionOperator<T> Singleton = new SubtractionOperator<T>();

        private SubtractionOperator() : base("-", "op_Subtraction")
        {
        }
    }

    internal sealed class MultiplicationOperator<T> : BinaryArithmeticOperator<T>
    {
        internal static readonly MultiplicationOperator<T> Singleton = new MultiplicationOperator<T>();

        private MultiplicationOperator() : base("*", "op_Multiply")
        {
        }
    }

    internal sealed class DivisionOperator<T> : BinaryArithmeticOperator<T>
    {
        internal static readonly DivisionOperator<T> Singleton = new DivisionOperator<T>();

        private DivisionOperator() : base("/", "op_Division")
        {
        }
    }

    internal sealed class ModulusOperator<T> : BinaryArithmeticOperator<T>
    {
        internal static readonly ModulusOperator<T> Singleton = new ModulusOperator<T>();

        private ModulusOperator() : base("%", "op_Modulus")
        {
        }
    }

    internal sealed class BitwiseOrOperator<T> : BinaryArithmeticOperator<T>
    {
        internal static readonly BitwiseOrOperator<T> Singleton = new BitwiseOrOperator<T>();

        private BitwiseOrOperator() : base("|", "op_BitwiseOr")
        {
        }
    }

    internal sealed class BitwiseXOrOperator<T> : BinaryArithmeticOperator<T>
    {
        internal static readonly BitwiseXOrOperator<T> Singleton = new BitwiseXOrOperator<T>();

        private BitwiseXOrOperator() : base("^", "op_ExclusiveOr")
        {
        }
    }

    internal sealed class BitwiseAndOperator<T> : BinaryArithmeticOperator<T>
    {
        internal static readonly BitwiseAndOperator<T> Singleton = new BitwiseAndOperator<T>();

        private BitwiseAndOperator() : base("&", "op_BitwiseAnd")
        {
        }
    }
}
