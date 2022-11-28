using System;

namespace TED
{
    /// <summary>
    /// Static class containing all the standard primitive predicates, such as Not.
    /// </summary>
    public static class Primitives
    {
        /// <summary>
        /// True if its argument is false
        /// </summary>
        public static readonly NotPrimitive Not = new NotPrimitive();

        /// <summary>
        /// Prob(p) succeeds with a probability of p (p in the range [0,1])
        /// </summary>
        public static readonly PrimitiveTest<float> Prob = new PrimitiveTest<float>("Prob", Random.Roll);

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
