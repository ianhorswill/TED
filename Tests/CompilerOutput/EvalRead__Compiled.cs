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

[CompiledHelpersFor("EvalRead")]
public static class EvalRead__Compiled

{
    [LinkToTable("P")]
    public static Table<ValueTuple<int,int>> P;
    [LinkToTable("Q")]
    public static Table<int> Q;

    [CompiledRulesFor("Q")]
    public static void Q__CompiledUpdate()
    {
        // Q[in i].If(P[out i,out j], i == j+1)
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

            // i == j+1
            if (i != j+1) goto restart__0;

            // Write [in i]
            Q.Add(i);
            goto restart__0;
        }

        end:;
    }
}
#pragma warning restore 0164,8618
