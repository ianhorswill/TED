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

{[CompiledHelpersFor("EvalRead")]
public class EvalRead__Compiled : TED.Compiler.CompiledTEDProgram

    {

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

        public override void Link(TED.Program program)
        {
            program["Q"].CompiledRules = (Action)Q__CompiledUpdate;
            P = (Table<ValueTuple<int,int>>)program["P"].TableUntyped;
            Q = (Table<int>)program["Q"].TableUntyped;
        }

        public static Table<ValueTuple<int,int>> P;
        public static Table<int> Q;
    }

}
#pragma warning restore 0164,8618,8600,8620
