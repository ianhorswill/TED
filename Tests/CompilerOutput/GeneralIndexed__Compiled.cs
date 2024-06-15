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

[CompiledHelpersFor("GeneralIndexed")]
public static class GeneralIndexed__Compiled

{
    [LinkToTable("P")]
    public static Table<ValueTuple<int,int>> P;
    [LinkToTable("Q")]
    public static Table<ValueTuple<int,int>> Q;

    [CompiledRulesFor("Q")]
    public static void Q__CompiledUpdate()
    {
        // Q[in i,in j].If(P[out i,out j], P[in j,in i])
        {
            int i;
            int j;

            // P[out i,out j]
            var row__0 = unchecked((uint)-1);
            restart__0:
            if (++row__0 == P.Length) goto end;
            ref var data__0 = ref P.Data[row__0];
            i = data__0.Item1;
            j = data__0.Item2;

            // P[in j,in i]
            var row__1 = unchecked((uint)-1);
            restart__1:
            if (++row__1 == P.Length) goto restart__0;
            ref var data__1 = ref P.Data[row__1];
            if (data__1.Item1 != j) goto restart__1;
            if (data__1.Item2 != i) goto restart__1;

            // Write [in i,in j]
            Q.Add((i,j));
            goto restart__1;
        }

        end:;
    }
}
#pragma warning restore 0164,8618
