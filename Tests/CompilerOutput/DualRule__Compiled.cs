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

[CompiledHelpersFor("DualRule")]
public static class DualRule__Compiled

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
        // R[in a].If(P[out a], Not[Q[in a]])
        {
            int a;

            // P[out a]
            var row__0 = unchecked((uint)-1);
            restart__0:
            if (++row__0 == P.Length) goto rule2;
            ref var data__0 = ref P.Data[row__0];
            a = data__0;

            // Not[Q[in a]]
            var row__1_ = unchecked((uint)-1);
            restart__1_:
            if (++row__1_ == Q.Length) goto not__1;
            ref var data__1_ = ref Q.Data[row__1_];
            if (data__1_ != a) goto restart__1_;
            goto restart__0;
            not__1: ;

            // Write [in a]
            R.Add(a);
            goto restart__0;
        }

        rule2:;

        // R[in a].If(Q[out a], Not[P[in a]])
        {
            int a;

            // Q[out a]
            var row__0 = unchecked((uint)-1);
            restart__0:
            if (++row__0 == Q.Length) goto end;
            ref var data__0 = ref Q.Data[row__0];
            a = data__0;

            // Not[P[in a]]
            var row__1_ = unchecked((uint)-1);
            restart__1_:
            if (++row__1_ == P.Length) goto not__1;
            ref var data__1_ = ref P.Data[row__1_];
            if (data__1_ != a) goto restart__1_;
            goto restart__0;
            not__1: ;

            // Write [in a]
            R.Add(a);
            goto restart__0;
        }

        end:;
    }
}
#pragma warning restore 0164,8618
