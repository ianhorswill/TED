// ReSharper disable InconsistentNaming
// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable RedundantUsingDirective
using System;
using TED;
using TED.Interpreter;
using TED.Compiler;
using TED.Tables;

#pragma warning disable 0164,8618,8600,8620

// ReSharper disable once CheckNamespace
namespace CompilerTests

{[CompiledHelpersFor("FirstOfTest")]
public class FirstOfTest__Compiled : TED.Compiler.CompiledTEDProgram

    {

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

        public override void Link(TED.Program program)
        {
            program["Q"].CompiledRules = (Action)Q__CompiledUpdate;
            P = (Table<int>)program["P"].TableUntyped;
            Q = (Table<string>)program["Q"].TableUntyped;
        }

        public static Table<int> P;
        public static Table<string> Q;
    }

}
#pragma warning restore 0164,8618,8600,8620
