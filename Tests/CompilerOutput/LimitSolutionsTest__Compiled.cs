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
    [CompiledHelpersFor("LimitSolutionsTest")]
    public class LimitSolutionsTest__Compiled : TED.Compiler.CompiledTEDProgram
    {

        public static void q__CompiledUpdate()
        {
            // q[in x].If(LimitSolutions[p[out x]])
            {
                int x;

                // LimitSolutions[p[out x]]
                var count__0 = 0;
                // p[out x]
                var row__0__ls = unchecked((uint)-1);
                restart__0__ls:
                if (++row__0__ls == p.Length) goto end;
                ref var data__0__ls = ref p.Data[row__0__ls];
                x = data__0__ls;
                count__0++;
                goto success__0;
                restart__0:
                if (count__0 >= 5) goto end;
                goto restart__0__ls;
                success__0:

                // Write [in x]
                q.Add(x);
                goto restart__0;
            }

            end:;
        }

        public override void Link(TED.Program program)
        {
            program["q"].CompiledRules = (Action)q__CompiledUpdate;
            p = (Table<int>)program["p"].TableUntyped;
            q = (Table<int>)program["q"].TableUntyped;
        }

        public static Table<int> p;
        public static Table<int> q;
    }

}
#pragma warning restore 0164,8618,8600,8620
