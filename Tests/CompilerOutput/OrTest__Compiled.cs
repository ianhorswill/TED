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

{[CompiledHelpersFor("OrTest")]
public class OrTest__Compiled : TED.Compiler.CompiledTEDProgram

    {

        public static void s__CompiledUpdate()
        {
            // s[in x].If(And[TED.Interpreter.Goal[]])
            {
                int x;

                // And[TED.Interpreter.Goal[]]

                // t[out x,in x]
                var row__0_0 = unchecked((uint)-1);
                restart__0_0:
                if (++row__0_0 == t.Length) goto end;
                ref var data__0_0 = ref t.Data[row__0_0];
                x = data__0_0.Item1;
                if (data__0_0.Item2 != x) goto restart__0_0;

                // u[in x]
                var row__0_1 = unchecked((uint)-1);
                restart__0_1:
                if (++row__0_1 == u.Length) goto restart__0_0;
                ref var data__0_1 = ref u.Data[row__0_1];
                if (data__0_1 != x) goto restart__0_1;

                // Write [in x]
                s.Add(x);
                goto restart__0_1;
            }

            end:;
        }
        public static void w__CompiledUpdate()
        {
            // w[in x].If(Or[TED.Interpreter.Goal[]])
            {
                int x;

                // Or[TED.Interpreter.Goal[]]
                uint row__0_0 = default(uint);
                uint row__0_1 = default(uint);

                int branch__0;

                start0__0:
                branch__0 = 0;
                // s[out x]
                row__0_0 = unchecked((uint)-1);
                restart__0_0:
                if (++row__0_0 == s.Length) goto start1__0;
                ref var data__0_0 = ref s.Data[row__0_0];
                x = data__0_0;
                goto orSuccess__0;
                start1__0:
                branch__0 = 1;
                // v[out x]
                row__0_1 = unchecked((uint)-1);
                restart__0_1:
                if (++row__0_1 == v.Length) goto end;
                ref var data__0_1 = ref v.Data[row__0_1];
                x = data__0_1;
                goto orSuccess__0;


                restartDispatch__0:
                switch (branch__0)
                {
                    case 0: goto restart__0_0;
                    case 1: goto restart__0_1;
                }

                orSuccess__0: ;

                // Write [in x]
                w.Add(x);
                goto restartDispatch__0;
            }

            end:;
        }

        public override void Link(TED.Program program)
        {
            program["s"].CompiledRules = (Action)s__CompiledUpdate;
            program["w"].CompiledRules = (Action)w__CompiledUpdate;
            t = (Table<ValueTuple<int,int>>)program["t"].TableUntyped;
            u = (Table<int>)program["u"].TableUntyped;
            s = (Table<int>)program["s"].TableUntyped;
            v = (Table<int>)program["v"].TableUntyped;
            w = (Table<int>)program["w"].TableUntyped;
        }

        public static Table<ValueTuple<int,int>> t;
        public static Table<int> u;
        public static Table<int> s;
        public static Table<int> v;
        public static Table<int> w;
    }

}
#pragma warning restore 0164,8618,8600,8620
