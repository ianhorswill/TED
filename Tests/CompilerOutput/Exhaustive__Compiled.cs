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

[CompiledHelpersFor("Exhaustive")]
public static class Exhaustive__Compiled

{
    [LinkToTable("P")]
    public static Table<int> P;
    [LinkToTable("Q")]
    public static Table<int> Q;
    [LinkToTable("R")]
    public static Table<int> R;

    [CompiledRulesFor("R")]
    public static void R__CompiledUpdate()
    {
        // R[in a].If(P[out a], Q[in a])
        {
            int a;

            // P[out a]
            var row__0 = unchecked((uint)-1);
            restart__0:
            if (++row__0 == P.Length) goto end;
            ref var data__0 = ref P.Data[row__0];
            a = data__0;

            // Q[in a]
            var row__1 = unchecked((uint)-1);
            restart__1:
            if (++row__1 == Q.Length) goto restart__0;
            ref var data__1 = ref Q.Data[row__1];
            if (data__1 != a) goto restart__1;

            // Write [in a]
            R.Add(a);
            goto restart__1;
        }

        end:;
    }
}
#pragma warning restore 0164,8618
