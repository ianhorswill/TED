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

[CompiledHelpersFor("KeyIndexed")]
public static class KeyIndexed__Compiled

{
    [LinkToTable("Day")]
    public static Table<string> Day;
    [LinkToTable("NextDay")]
    public static Table<ValueTuple<string,string>> NextDay;
    public static KeyIndex<ValueTuple<string,string>,string> NextDay__0;
    public static GeneralIndex<ValueTuple<string,string>,string> NextDay__1;
    [LinkToTable("Mapped")]
    public static Table<ValueTuple<string,string>> Mapped;

    [CompiledRulesFor("Mapped")]
    public static void Mapped__CompiledUpdate()
    {
        // Mapped[in d,in n].If(Day[out d], NextDay[in d,out n])
        {
            string d;
            string n;

            // Day[out d]
            var row__0 = unchecked((uint)-1);
            restart__0:
            if (++row__0 == Day.Length) goto end;
            ref var data__0 = ref Day.Data[row__0];
            d = data__0;

            // NextDay[in d,out n]
            var row__1 = NextDay__0.RowWithKey(in d);
            if (row__1 == Table.NoRow) goto restart__0;
            ref var data__1 = ref NextDay.Data[row__1];
            if (data__1.Item1 != d) goto restart__0;
            n = data__1.Item2;

            // Write [in d,in n]
            Mapped.Add((d,n));
            goto restart__0;
        }

        end:;
    }
}
#pragma warning restore 0164,8618
