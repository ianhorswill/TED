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
    [CompiledHelpersFor("PickRandomlyTest")]
    public class PickRandomlyTest__Compiled : TED.Compiler.CompiledTEDProgram
    {

        public static void P__CompiledUpdate()
        {
            // P[in n].If(PickRandomly[out n])
            {
                int n;

                // PickRandomly[out n]
                n = PickRandomlyArray__0[_Rng0.Next()%5];

                // Write [in n]
                P.Add(n);
                goto end;
            }

            end:;
        }

        public override void Link(TED.Program program)
        {
            program["P"].CompiledRules = (Action)P__CompiledUpdate;
            P = (Table<int>)program["P"].TableUntyped;
            _Rng0 = MakeRng();
            PickRandomlyArray__0 = new int[] {0, 1, 2, 3, 4, };
        }

        public static Table<int> P;
        public static Random _Rng0;
        public static int[] PickRandomlyArray__0;
    }

}
#pragma warning restore 0164,8618,8600,8620
