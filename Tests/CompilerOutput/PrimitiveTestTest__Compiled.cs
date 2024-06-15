// ReSharper disable InconsistentNaming
// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable RedundantUsingDirective
using TED;
using TED.Interpreter;
using TED.Compiler;
using TED.Tables;

// ReSharper disable once CheckNamespace
namespace CompilerTests;

#pragma warning disable 0164,8618

[CompiledHelpersFor("PrimitiveTestTest")]
public static class PrimitiveTestTest__Compiled

{
    [LinkToTable("P")]
    public static Table<int> P;
    [LinkToTable("Q")]
    public static Table<int> Q;

    [CompiledRulesFor("Q")]
    public static void Q__CompiledUpdate()
    {
        // Q[in a].If(P[out a], Odd[in a])
        {
            int a;

            // P[out a]
            var row__0 = unchecked((uint)-1);
            restart__0:
            if (++row__0 == P.Length) goto end;
            ref var data__0 = ref P.Data[row__0];
            a = data__0;

            // Odd[in a]
            if (!Tests.CompilerTests.Odd(a)) goto restart__0;

            // Write [in a]
            Q.Add(a);
            goto restart__0;
        }

        end:;
    }
}
#pragma warning restore 0164,8618
