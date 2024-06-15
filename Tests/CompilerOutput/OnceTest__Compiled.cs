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

[CompiledHelpersFor("OnceTest")]
public static class OnceTest__Compiled

{
    [LinkToTable("p")]
    public static Table<int> p;
    [LinkToTable("q")]
    public static Table<int> q;

    [CompiledRulesFor("q")]
    public static void q__CompiledUpdate()
    {
        // q[in x].If(Not[p[out x]])
        {
            int x;

            // Not[p[out x]]
            // p[out x]
            var row__0 = unchecked((uint)-1);
            restart__0:
            if (++row__0 == p.Length) goto end;
            ref var data__0 = ref p.Data[row__0];
            x = data__0;

            // Write [in x]
            q.Add(x);
            goto end;
        }

        end:;
    }
}
#pragma warning restore 0164,8618
