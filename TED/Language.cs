using System;
using System.Diagnostics;

namespace TED
{
    /// <summary>
    /// Static class containing all the standard primitive predicates, such as Not.
    /// </summary>
    public static class Language
    {
        public static TablePredicate<T1> TPredicate<T1>(string name, Var<T1> arg) 
            => new TablePredicate<T1>(name, arg);
        public static TablePredicate<T1,T2> TPredicate<T1,T2>(string name, Var<T1> arg1, Var<T2> arg2) 
            => new TablePredicate<T1,T2>(name, arg1, arg2);
        public static TablePredicate<T1,T2,T3> TPredicate<T1,T2,T3>(string name, Var<T1> arg1, Var<T2> arg2, Var<T3> arg3) 
            => new TablePredicate<T1,T2,T3>(name, arg1, arg2, arg3);
        public static TablePredicate<T1,T2,T3,T4> TPredicate<T1,T2,T3,T4>(string name, Var<T1> arg1, Var<T2> arg2, Var<T3> arg3, Var<T4> arg4) 
            => new TablePredicate<T1,T2,T3,T4>(name, arg1, arg2, arg3, arg4);
        public static TablePredicate<T1,T2,T3,T4,T5> TPredicate<T1,T2,T3,T4,T5>(string name, Var<T1> arg1, Var<T2> arg2, Var<T3> arg3, Var<T4> arg4, Var<T5> arg5) 
            => new TablePredicate<T1,T2,T3,T4,T5>(name, arg1, arg2, arg3, arg4, arg5);
        public static TablePredicate<T1,T2,T3,T4,T5,T6> TPredicate<T1,T2,T3,T4,T5,T6>(string name, Var<T1> arg1, Var<T2> arg2, Var<T3> arg3, Var<T4> arg4, Var<T5> arg5, Var<T6> arg6) 
            => new TablePredicate<T1,T2,T3,T4,T5,T6>(name, arg1, arg2, arg3, arg4, arg5,arg6);

        /// <summary>
        /// True if its argument is false
        /// </summary>
        public static readonly NotPrimitive Not = new NotPrimitive();

        /// <summary>
        /// Prob(p) succeeds with a probability of p (p in the range [0,1])
        /// </summary>
        public static readonly PrimitiveTest<float> Prob = new PrimitiveTest<float>("Prob", Random.Roll);

        /// <summary>
        /// Breakpoint execution of a rule
        /// This drops the caller into the underlying C# debugger
        /// </summary>
        public static readonly PrimitiveTest<object> BreakPoint = new PrimitiveTest<object>(nameof(BreakPoint),
            arg =>
            {
                Debugger.Break();
                return true;
            });

        /// <summary>
        /// Compares two integers
        /// </summary>
        public static readonly PrimitiveTest<int, int> IntegerLessThan = new PrimitiveTest<int, int>("<", (a, b) => a < b);
        /// <summary>
        /// Compares two integers
        /// </summary>
        public static readonly PrimitiveTest<int, int> IntegerGreaterThan = new PrimitiveTest<int, int>(">", (a, b) => a > b);
        /// <summary>
        /// Compares two integers
        /// </summary>
        public static readonly PrimitiveTest<int, int> IntegerLessThanEqual = new PrimitiveTest<int, int>("<=", (a, b) => a <= b);
        /// <summary>
        /// Compares two integers
        /// </summary>
        public static readonly PrimitiveTest<int, int> IntegerGreaterThanEqual = new PrimitiveTest<int, int>(">=", (a, b) => a >= b);

        /// <summary>
        /// Compares two floats
        /// </summary>
        public static readonly PrimitiveTest<float, float> FloatLessThan = new PrimitiveTest<float, float>("<", (a, b) => a < b);
        /// <summary>
        /// Compares two floats
        /// </summary>
        public static readonly PrimitiveTest<float, float> FloatGreaterThan = new PrimitiveTest<float, float>(">", (a, b) => a > b);
        /// <summary>
        /// Compares two floats
        /// </summary>
        public static readonly PrimitiveTest<float, float> FloatLessThanEqual = new PrimitiveTest<float, float>("<=", (a, b) => a <= b);
        /// <summary>
        /// Compares two floats
        /// </summary>
        public static readonly PrimitiveTest<float, float> FloatGreaterThanEqual = new PrimitiveTest<float, float>(">=", (a, b) => a >= b);

        /// <summary>
        /// Compares two values.  Hopefully they're something you can compare.
        /// </summary>
        public static AnyGoal LessThan<T>(object a, object b)
        {
            if (typeof(T) == typeof(int))
                return IntegerLessThan[(Term<int>)a, (Term<int>)b];
            if (typeof(T) == typeof(float))
                return FloatLessThan[(Term<float>)a, (Term<float>)b];
            throw new ArgumentException($"< operator not defined for the type {typeof(T)}");
        }
        /// <summary>
        /// Compares two values.  Hopefully they're something you can compare.
        /// </summary>
        public static AnyGoal GreaterThan<T>(object a, object b)
        {
            if (typeof(T) == typeof(int))
                return IntegerGreaterThan[(Term<int>)a, (Term<int>)b];
            if (typeof(T) == typeof(float))
                return FloatGreaterThan[(Term<float>)a, (Term<float>)b];
            throw new ArgumentException($"< operator not defined for the type {typeof(T)}");
        }

        /// <summary>
        /// Compares two values.  Hopefully they're something you can compare.
        /// </summary>
        public static AnyGoal LessThanEqual<T>(object a, object b)
        {
            if (typeof(T) == typeof(int))
                return IntegerLessThanEqual[(Term<int>)a, (Term<int>)b];
            if (typeof(T) == typeof(float))
                return FloatLessThanEqual[(Term<float>)a, (Term<float>)b];
            throw new ArgumentException($"< operator not defined for the type {typeof(T)}");
        }

        /// <summary>
        /// Compares two values.  Hopefully they're something you can compare.
        /// </summary>
        public static AnyGoal GreaterThanEqual<T>(object a, object b)
        {
            if (typeof(T) == typeof(int))
                return IntegerGreaterThanEqual[(Term<int>)a, (Term<int>)b];
            if (typeof(T) == typeof(float))
                return FloatGreaterThanEqual[(Term<float>)a, (Term<float>)b];
            throw new ArgumentException($"< operator not defined for the type {typeof(T)}");
        }

        public static AnyGoal RandomElement<T>(TablePredicate<T> predicate, Term<T> output) =>
            RandomElementPrimitive<T>.Singleton[predicate, output];

        public static AnyGoal PickRandomly<T>(Term<T> output, params T[] choices) =>
            PickRandomlyPrimitive<T>.Singleton[output, choices];
    }
}
