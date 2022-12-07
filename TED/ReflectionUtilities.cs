using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace TED
{
    internal static class ReflectionUtilities
    {
        internal static MethodInfo? GetOperatorMethodInfo(this Type t, string name)
        {
            foreach (var m in t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (m.Name.EndsWith(name))
                    return m;
            }

            return null;
        }
    }
}
