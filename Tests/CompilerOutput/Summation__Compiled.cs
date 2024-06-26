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

{
    [CompiledHelpersFor("Summation")]
    public class Summation__Compiled : TED.Compiler.CompiledTEDProgram
    {

        public static void Q__CompiledUpdate()
        {
            // Q[in n].If(n == TED.Primitives.AggregateFunctionCall`1[System.Int32])
            {
                int n;
                int m;

                // n == TED.Primitives.AggregateFunctionCall`1[System.Int32]
                var accumulator__aggregator0 = 0;
                // And[TED.Interpreter.Goal[]]

                // P[out m]
                var row__aggregator0_0 = unchecked((uint)-1);
                restart__aggregator0_0:
                if (++row__aggregator0_0 == P.Length) goto end__aggregator0;
                ref var data__aggregator0_0 = ref P.Data[row__aggregator0_0];
                m = data__aggregator0_0;

                // 0 == m%2
                if (0 != m%2) goto restart__aggregator0_0;
                accumulator__aggregator0 = (accumulator__aggregator0)+(m);
                goto restart__aggregator0_0;
                end__aggregator0: ;
                n = accumulator__aggregator0;

                // Write [in n]
                Q.Add(n);
                goto end;
            }

            end:;
        }

        public override void Link(TED.Program program)
        {
            program["Q"].CompiledRules = (Action)Q__CompiledUpdate;
            P = (Table<int>)program["P"].TableUntyped;
            Q = (Table<int>)program["Q"].TableUntyped;
        }

        public static Table<int> P;
        public static Table<int> Q;
    }

}
#pragma warning restore 0164,8618,8600,8620
