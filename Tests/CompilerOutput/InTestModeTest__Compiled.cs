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

{[CompiledHelpersFor("InTestModeTest")]
public class InTestModeTest__Compiled : TED.Compiler.CompiledTEDProgram

    {

        public static void Test__CompiledUpdate()
        {
            // Test[0].If(In[0,System.Int32[]])
            {

                // In[0,System.Int32[]]
                if (!new int[] {1, 2, 3, 4, 5, }.Contains(0)) goto rule2;

                // Write [0]
                Test.Add(0);
                goto rule2;
            }

            rule2:;

            // Test[1].If(In[4,System.Int32[]])
            {

                // In[4,System.Int32[]]
                if (!new int[] {1, 2, 3, 4, 5, }.Contains(4)) goto end;

                // Write [1]
                Test.Add(1);
                goto end;
            }

            end:;
        }

        public override void Link(TED.Program program)
        {
            program["Test"].CompiledRules = (Action)Test__CompiledUpdate;
            Test = (Table<int>)program["Test"].TableUntyped;
        }

        public static Table<int> Test;
    }

}
#pragma warning restore 0164,8618,8600,8620
