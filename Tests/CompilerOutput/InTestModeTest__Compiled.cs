// ReSharper disable InconsistentNaming
// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable RedundantUsingDirective
using TED;
using TED.Interpreter;
using TED.Compiler;
using TED.Tables;

// ReSharper disable once CheckNamespace
namespace CompilerTests;

#pragma warning disable 0164,8618

[CompiledHelpersFor("InTestModeTest")]
public static class InTestModeTest__Compiled

{
    [LinkToTable("Test")]
    public static Table<int> Test;

    [CompiledRulesFor("Test")]
    public static void Test__CompiledUpdate()
    {
        // Test[0].If(In[0,System.Int32[]])
        {

            // In[0,System.Int32[]]
            if (!new [] { 1, 2, 3, 4, 5, }.Contains(0)) goto rule2;

            // Write [0]
            Test.Add(0);
            goto rule2;
        }

        rule2:;

        // Test[1].If(In[4,System.Int32[]])
        {

            // In[4,System.Int32[]]
            if (!new [] { 1, 2, 3, 4, 5, }.Contains(4)) goto end;

            // Write [1]
            Test.Add(1);
            goto end;
        }

        end:;
    }
}
#pragma warning restore 0164,8618
