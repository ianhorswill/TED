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
    [CompiledHelpersFor("ThrowException")]
    public class ThrowException__Compiled : TED.Compiler.CompiledTEDProgram
    {

        public static void P__CompiledUpdate()
        {
            P.BeginRebuild();
            try
            {
                // P[in i,in j].If(i == 0, j == 0, ForceException[An error occurred])
                try
                {
                    int i;
                    int j;

                    // i == 0
                    i = 0;

                    // j == 0
                    j = 0;

                    // ForceException[An error occurred]
                    if (!TED.Language.ForceExceptionImplementation("An error occurred")) goto end;

                    // Write [in i,in j]
                    P.RebuildRowNonUnique((i,j));
                    goto end;
                }
                catch (Exception _ruleException) { P.ThrowDeferred(_ruleException); }

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
            P = (Table<ValueTuple<int,int>>)program["P"].TableUntyped;
        }

        public static Table<ValueTuple<int,int>> P;
    }

}
#pragma warning restore 0164,8618,8600,8620
