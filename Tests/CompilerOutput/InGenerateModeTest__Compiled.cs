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
    [CompiledHelpersFor("InGenerateModeTest")]
    public class InGenerateModeTest__Compiled : TED.Compiler.CompiledTEDProgram
    {

        public static void Test__CompiledUpdate()
        {
            // Test[in n].If(In[out n,System.Int32[]])
            {
                int n;

                // In[out n,System.Int32[]]
                var enumerator__0 = ((IEnumerable<int>)(new int[] {1, 2, 3, 4, 5, })).GetEnumerator();;
                in_restart___0:
                if (!enumerator__0.MoveNext()) goto end;
                n = enumerator__0.Current;

                // Write [in n]
                Test.Add(n);
                goto in_restart___0;
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
