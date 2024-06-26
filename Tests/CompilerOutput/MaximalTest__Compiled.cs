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

{[CompiledHelpersFor("MaximalTest")]
public class MaximalTest__Compiled : TED.Compiler.CompiledTEDProgram

    {

        public static void M__CompiledUpdate()
        {
            // M[in name,in floatAge].If(Maximal[in name,in floatAge,And[TED.Interpreter.Goal[]]])
            {
                string name;
                int age;
                float floatAge;

                // Maximal[in name,in floatAge,And[TED.Interpreter.Goal[]]]
                var gotOne__0 = false;
                var bestname__0 = default(string);
                var bestfloatAge__0 = default(float);
                // And[TED.Interpreter.Goal[]]

                // test[out name,out age]
                var row__0_maxLoop_0 = unchecked((uint)-1);
                restart__0_maxLoop_0:
                if (++row__0_maxLoop_0 == test.Length) goto maxDone__0;
                ref var data__0_maxLoop_0 = ref test.Data[row__0_maxLoop_0];
                name = data__0_maxLoop_0.Item1;
                age = data__0_maxLoop_0.Item2;

                // floatAge == Float(age)
                floatAge = (float)(age);

                if (!gotOne__0 || floatAge > bestfloatAge__0)
                {
                    gotOne__0 = true;
                    bestname__0 = name;
                    bestfloatAge__0 = floatAge;
                }
                goto restart__0_maxLoop_0;

                maxDone__0:
                if (!gotOne__0) goto end;
                name = bestname__0;
                floatAge = bestfloatAge__0;

                // Write [in name,in floatAge]
                M.Add((name,floatAge));
                goto end;
            }

            end:;
        }

        public override void Link(TED.Program program)
        {
            program["M"].CompiledRules = (Action)M__CompiledUpdate;
            test = (Table<ValueTuple<string,int>>)program["test"].TableUntyped;
            M = (Table<ValueTuple<string,float>>)program["M"].TableUntyped;
        }

        public static Table<ValueTuple<string,int>> test;
        public static Table<ValueTuple<string,float>> M;
    }

}
#pragma warning restore 0164,8618,8600,8620
