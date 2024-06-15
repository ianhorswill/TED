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

[CompiledHelpersFor("InGenerateModeTest")]
public static class InGenerateModeTest__Compiled

{
    [LinkToTable("Test")]
    public static Table<int> Test;

    [CompiledRulesFor("Test")]
    public static void Test__CompiledUpdate()
    {
        // Test[in n].If(In[out n,System.Int32[]])
        {
            int n;

            // In[out n,System.Int32[]]
            var enumerator__0 = ((IEnumerable<int>)(new [] { 1, 2, 3, 4, 5, })).GetEnumerator();
            in_restart___0:
            if (!enumerator__0.MoveNext()) goto end;
            n = enumerator__0.Current;

            // Write [in n]
            Test.Add(n);
            goto in_restart___0;
        }

        end:;
    }
}
#pragma warning restore 0164,8618
