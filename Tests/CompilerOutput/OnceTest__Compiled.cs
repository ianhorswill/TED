// ReSharper disable InconsistentNaming
// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable RedundantUsingDirective
using TED;
using TED.Interpreter;
using TED.Compiler;
using TED.Tables;

// ReSharper disable once CheckNamespace
namespace CompilerTests;

#pragma warning disable 0164,8618,8600,8620

[CompiledHelpersFor("OnceTest")]
public class OnceTest__Compiled : TED.Compiler.CompiledTEDProgram

{

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

    public override void Link(TED.Program program)
    {
        program["q"].CompiledRules = (Action)q__CompiledUpdate;
        p = (Table<int>)program["p"].TableUntyped;
        q = (Table<int>)program["q"].TableUntyped;
    }

    public static Table<int> p;
    public static Table<int> q;
}
#pragma warning restore 0164,8618,8600,8620
