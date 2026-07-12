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
    [CompiledHelpersFor("Provenance")]
    public class Provenance__Compiled : TED.Compiler.CompiledTEDProgram
    {

        public static void P__CompiledUpdate()
        {
            P.BeginRebuild();
            try
            {
                // P[1].If()
                {

                    // Write [1]
                    P.Provenance[P.Length] = "P[1].If()";
                    P.RebuildRowNonUnique(1);
                    goto rule2;
                }

                rule2:;

                // P[2].If()
                {

                    // Write [2]
                    P.Provenance[P.Length] = "P[2].If()";
                    P.RebuildRowNonUnique(2);
                    goto end;
                }

                end:;
            }
            finally
            {
                P.EndRebuild();
            }
        }

        public override void Link(TED.Program program)
        {
            program["P"].CompiledRules = (Action)P__CompiledUpdate;
            P = (Table<int>)program["P"].TableUntyped;
        }

        public static Table<int> P;
    }

}
#pragma warning restore 0164,8618,8600,8620
