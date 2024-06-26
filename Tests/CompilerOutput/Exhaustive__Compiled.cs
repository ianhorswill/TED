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

{[CompiledHelpersFor("Exhaustive")]
public class Exhaustive__Compiled : TED.Compiler.CompiledTEDProgram

    {

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

        public override void Link(TED.Program program)
        {
            program["R"].CompiledRules = (Action)R__CompiledUpdate;
            P = (Table<int>)program["P"].TableUntyped;
            Q = (Table<int>)program["Q"].TableUntyped;
            R = (Table<int>)program["R"].TableUntyped;
        }

        public static Table<int> P;
        public static Table<int> Q;
        public static Table<int> R;
    }

}
#pragma warning restore 0164,8618,8600,8620
