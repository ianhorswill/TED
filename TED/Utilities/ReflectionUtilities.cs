using System;
using System.Collections.Generic;
using System.Reflection;

namespace TED.Utilities
{
    /// <summary>
    /// Convenience functions for System.Reflection
    /// </summary>
    internal static class ReflectionUtilities
    {
        /// <summary>
        /// Look up the MethodInfo object for the specified operator of the specified type
        /// </summary>
        /// <param name="t">Type to get the operator for</param>
        /// <param name="name">Internal string name of the method used by C# for the operator, for example, op_Addition, op_LessThan, etc.</param>
        /// <param name="returnType">Type to be returned by the method</param>
        /// <param name="parameterTypes">Types to be passed to the parameters</param>
        /// <returns>The MethodInfo object for the method implementing the operator, or null if the type doesn't implement the operator.</returns>
        internal static MethodInfo? GetOperatorMethodInfo(this Type t, string name, Type returnType, params Type[] parameterTypes)
        {
            foreach (var m in t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!m.Name.EndsWith(name) || !returnType.IsAssignableFrom(m.ReturnType))
                    continue;
                var p = m.GetParameters();
                if (p.Length != parameterTypes.Length)
                    continue;
                for (var i = 0; i < p.Length; i++)
                    if (!p[i].ParameterType.IsAssignableFrom(parameterTypes[i]))
                        goto nextOne;
                return m;
            nextOne:;
            }

            if (OperatorOverrideTable.TryGetValue((t, name), out var impl))
                return impl;

            return null;
        }

        private static readonly Dictionary<(Type t, string methodName), MethodInfo> OperatorOverrideTable =
            new Dictionary<(Type t, string methodName), MethodInfo>();

        public static void AddOperatorImplementation(Type t, string operatorName, MethodInfo m) =>
            OperatorOverrideTable[(t, operatorName)] = m;

        public static void AddOperatorImplementation(Type t, string operatorName, Delegate d) =>
            AddOperatorImplementation(t, operatorName, d.Method);

        static ReflectionUtilities()
        {
            // Unity doesn't define the most of the standard operator methods for int and float, so we have to provide our own

            // Int type
            AddOperatorImplementation(typeof(int), "op_LessThan", (Func<int, int, bool>)IntLessThan);
            AddOperatorImplementation(typeof(int), "op_LessThanOrEqual", (Func<int, int, bool>)IntLessThanEqual);
            AddOperatorImplementation(typeof(int), "op_GreaterThan", (Func<int, int, bool>)IntGreaterThan);
            AddOperatorImplementation(typeof(int), "op_GreaterThanOrEqual", (Func<int, int, bool>)IntGreaterThanEqual);
            AddOperatorImplementation(typeof(int), "op_Equality", (Func<int, int, bool>)IntEquality);
            AddOperatorImplementation(typeof(int), "op_Inequality", (Func<int, int, bool>)IntInequality);

            AddOperatorImplementation(typeof(int), "op_Addition", (Func<int, int, int>)IntAddition);
            AddOperatorImplementation(typeof(int), "op_Subtraction", (Func<int, int, int>)IntSubtraction);
            AddOperatorImplementation(typeof(int), "op_Multiply", (Func<int, int, int>)IntMultiply);
            AddOperatorImplementation(typeof(int), "op_Division", (Func<int, int, int>)IntDivision);
            AddOperatorImplementation(typeof(int), "op_Modulus", (Func<int, int, int>)IntModulus);
            AddOperatorImplementation(typeof(int), "op_UnaryNegation", (Func<int, int>)IntUnaryNegation);

            // Float type
            AddOperatorImplementation(typeof(float), "op_LessThan", (Func<float, float, bool>)FloatLessThan);
            AddOperatorImplementation(typeof(float), "op_LessThanOrEqual", (Func<float, float, bool>)FloatLessThanEqual);
            AddOperatorImplementation(typeof(float), "op_GreaterThan", (Func<float, float, bool>)FloatGreaterThan);
            AddOperatorImplementation(typeof(float), "op_GreaterThanOrEqual", (Func<float, float, bool>)FloatGreaterThanEqual);
            // float equality is a bit trickier, usually using some delta...

            AddOperatorImplementation(typeof(float), "op_Addition", (Func<float, float, float>)FloatAddition);
            AddOperatorImplementation(typeof(float), "op_Subtraction", (Func<float, float, float>)FloatSubtraction);
            AddOperatorImplementation(typeof(float), "op_Multiply", (Func<float, float, float>)FloatMultiply);
            AddOperatorImplementation(typeof(float), "op_Division", (Func<float, float, float>)FloatDivision);
            AddOperatorImplementation(typeof(float), "op_Modulus", (Func<float, float, float>)FloatModulus);
            AddOperatorImplementation(typeof(float), "op_UnaryNegation", (Func<float, float>)FloatUnaryNegation);

            // Sbyte type
            AddOperatorImplementation(typeof(sbyte), "op_LessThan", (Func<sbyte, sbyte, bool>)SbyteLessThan);
            AddOperatorImplementation(typeof(sbyte), "op_LessThanOrEqual", (Func<sbyte, sbyte, bool>)SbyteLessThanEqual);
            AddOperatorImplementation(typeof(sbyte), "op_GreaterThan", (Func<sbyte, sbyte, bool>)SbyteGreaterThan);
            AddOperatorImplementation(typeof(sbyte), "op_GreaterThanOrEqual", (Func<sbyte, sbyte, bool>)SbyteGreaterThanEqual);
            AddOperatorImplementation(typeof(sbyte), "op_Equality", (Func<sbyte, sbyte, bool>)SbyteEquality);
            AddOperatorImplementation(typeof(sbyte), "op_Inequality", (Func<sbyte, sbyte, bool>)SbyteInequality);
        }

        // Ints
        private static bool IntLessThan(int a, int b) => a < b;
        private static bool IntLessThanEqual(int a, int b) => a <= b;
        private static bool IntGreaterThan(int a, int b) => a > b;
        private static bool IntGreaterThanEqual(int a, int b) => a >= b;
        private static bool IntEquality(int a, int b) => a == b;
        private static bool IntInequality(int a, int b) => a != b;

        private static int IntAddition(int a, int b) => a + b;
        private static int IntSubtraction(int a, int b) => a - b;
        private static int IntMultiply(int a, int b) => a * b;
        private static int IntDivision(int a, int b) => a / b;
        private static int IntModulus(int a, int b) => a % b;
        private static int IntUnaryNegation(int a) => -a;

        // Floats
        private static bool FloatLessThan(float a, float b) => a < b;
        private static bool FloatLessThanEqual(float a, float b) => a <= b;
        private static bool FloatGreaterThan(float a, float b) => a > b;
        private static bool FloatGreaterThanEqual(float a, float b) => a >= b;

        private static float FloatAddition(float a, float b) => a + b;
        private static float FloatSubtraction(float a, float b) => a - b;
        private static float FloatMultiply(float a, float b) => a * b;
        private static float FloatDivision(float a, float b) => a / b;
        private static float FloatModulus(float a, float b) => a % b;
        private static float FloatUnaryNegation(float a) => -a;

        // Sbytes
        private static bool SbyteLessThan(sbyte a, sbyte b) => a < b;
        private static bool SbyteLessThanEqual(sbyte a, sbyte b) => a <= b;
        private static bool SbyteGreaterThan(sbyte a, sbyte b) => a > b;
        private static bool SbyteGreaterThanEqual(sbyte a, sbyte b) => a >= b;
        private static bool SbyteEquality(sbyte a, sbyte b) => a == b;
        private static bool SbyteInequality(sbyte a, sbyte b) => a != b;
    }
}
