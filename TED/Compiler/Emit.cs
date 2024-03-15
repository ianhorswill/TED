using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace TED.Compiler
{
    internal static class Emit
    {
        public static void Load<T>(ILGenerator g, FieldInfo cell)
        {
            g.Emit(OpCodes.Ldarg_0);
            g.Emit(OpCodes.Ldfld, cell);
            g.Emit(OpCodes.Ldfld, typeof(T).GetField("Value"));
        }

        public static void Equals<T>(ILGenerator g, FieldInfo a, FieldInfo b)
        {
            var comparer = EqualityComparer<T>.Default;
            var t = comparer.GetType();
            var equals = t.GetMethod("Equals");
            Load<T>(g, a);
            Load<T>(g, b);
            g.EmitCall(OpCodes.Call, equals, null);
        }
    }
}
