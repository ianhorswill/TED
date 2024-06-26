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

{[CompiledHelpersFor("DualRule")]
public class DualRule__Compiled : TED.Compiler.CompiledTEDProgram

    {

        public static void R__CompiledUpdate()
        {
            // R[in a].If(P[out a], Not[Q[in a]])
            {
                int a;

                // P[out a]
                var row__0 = unchecked((uint)-1);
                restart__0:
                if (++row__0 == P.Length) goto rule2;
                ref var data__0 = ref P.Data[row__0];
                a = data__0;

                // Not[Q[in a]]
                var row__1_ = unchecked((uint)-1);
                restart__1_:
                if (++row__1_ == Q.Length) goto not__1;
                ref var data__1_ = ref Q.Data[row__1_];
                if (data__1_ != a) goto restart__1_;
                goto restart__0;
                not__1: ;

                // Write [in a]
                R.Add(a);
                goto restart__0;
            }

            rule2:;

            // R[in a].If(Q[out a], Not[P[in a]])
            {
                int a;

                // Q[out a]
                var row__0 = unchecked((uint)-1);
                restart__0:
                if (++row__0 == Q.Length) goto end;
                ref var data__0 = ref Q.Data[row__0];
                a = data__0;

                // Not[P[in a]]
                var row__1_ = unchecked((uint)-1);
                restart__1_:
                if (++row__1_ == P.Length) goto not__1;
                ref var data__1_ = ref P.Data[row__1_];
                if (data__1_ != a) goto restart__1_;
                goto restart__0;
                not__1: ;

                // Write [in a]
                R.Add(a);
                goto restart__0;
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
