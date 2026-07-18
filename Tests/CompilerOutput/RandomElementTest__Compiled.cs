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
    [CompiledHelpersFor("RandomElementTest")]
    public class RandomElementTest__Compiled : TED.Compiler.CompiledTEDProgram
    {

        public static void P__CompiledUpdate()
        {
            P.BeginRebuild();
            try
            {
                // P[in n].If(RandomElement[out n])
                try
                {
                    int n;

                    // RandomElement[out n]
                    if (Source.Length == 0) goto end;
                    ref var row___0 = ref Source.Data[_Rng0.Next()%Source.Length];
                    n = row___0;

                    // Write [in n]
                    P.RebuildRowNonUnique(n);
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
            Source = (Table<int>)program["Source"].TableUntyped;
            P = (Table<int>)program["P"].TableUntyped;
            _Rng0 = MakeRng();
        }

        public static Table<int> Source;
        public static Table<int> P;
        public static Random _Rng0;
    }

}
#pragma warning restore 0164,8618,8600,8620
