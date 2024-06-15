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

[CompiledHelpersFor("FirstOfTest")]
public static class FirstOfTest__Compiled

{
    [LinkToTable("P")]
    public static Table<int> P;
    [LinkToTable("Q")]
    public static Table<string> Q;

    [CompiledRulesFor("Q")]
    public static void Q__CompiledUpdate()
    {
        // Q[in b].If(P[out a], FirstOf[TED.Interpreter.Goal[]])
        {
            int a;
            string b;

            // P[out a]
            var row__0 = unchecked((uint)-1);
            restart__0:
            if (++row__0 == P.Length) goto end;
            ref var data__0 = ref P.Data[row__0];
            a = data__0;

            // FirstOf[TED.Interpreter.Goal[]]
            // And[TED.Interpreter.Goal[]]

            // Odd[in a]
            if (!Tests.CompilerTests.Odd(a)) goto firstOFBranch1__1;

            // b == "odd"
            b = "odd";
            goto firstOfSuccess__1;

            firstOFBranch1__1:
            // b == "even"
            b = "even";
            goto firstOfSuccess__1;

            firstOfSuccess__1: ;

            // Write [in b]
            Q.Add(b);
            goto restart__0;
        }

        end:;
    }
}
#pragma warning restore 0164,8618
