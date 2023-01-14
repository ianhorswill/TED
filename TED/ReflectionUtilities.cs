using System;
using System.Linq;
using System.Reflection;

namespace TED
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
                for (var i = 0; i<p.Length; i++)
                    if (!p[i].ParameterType.IsAssignableFrom(parameterTypes[i]))
                        continue;
                return m;
            }

            return null;
        }
    }
}
