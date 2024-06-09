using TED;
using TED.Interpreter;
using TED.Compiler;
using TED.Tables;

namespace CompilerTests;

#pragma warning disable 0164,8618

[CompiledHelpersFor("RowSet")]
public static class RowSetHelpers
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
        {
            start1:
            // R[in a].If(P[out a], Q[in a])
            int a;
            // P[out a]
            var row__0 = unchecked((uint)-1);
            restart__0:
            if (++row__0 == P.Length) goto end1;
            ref var data__0 = ref P.Data[row__0];
            a = data__0;
            // Q[in a]
            if (!Q.ContainsRowUsingRowSet(a)) goto restart__0;
            R.Add(a);
            goto restart__0;
            end1: ;
        }
    }
}
#pragma warning restore 0164,8618
