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
    [CompiledHelpersFor("KeyIndexed")]
    public class KeyIndexed__Compiled : TED.Compiler.CompiledTEDProgram
    {

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
                var row__1 = Table.NoRow;
                for (var bucket__1=d.GetHashCode()&NextDay__0_key.Mask; NextDay__0_key.Buckets[bucket__1].row != Table.NoRow; bucket__1 = (bucket__1+1)&NextDay__0_key.Mask)
                    if (NextDay__0_key.Buckets[bucket__1].key == d)
                    {
                        row__1 = NextDay__0_key.Buckets[bucket__1].row;
                        break;
                    }
                if (row__1 == Table.NoRow) goto restart__0;
                ref var data__1 = ref NextDay.Data[row__1];
                n = data__1.Item2;

                // Write [in d,in n]
                Mapped.Add((d,n));
                goto restart__0;
            }

            end:;
        }

        public override void Link(TED.Program program)
        {
            program["Mapped"].CompiledRules = (Action)Mapped__CompiledUpdate;
            Day = (Table<string>)program["Day"].TableUntyped;
            NextDay = (Table<ValueTuple<string,string>>)program["NextDay"].TableUntyped;
            NextDay__0_key = (KeyIndex<ValueTuple<string,string>,string>)NextDay.IndexFor(0);
            NextDay__1 = (GeneralIndex<ValueTuple<string,string>,string>)NextDay.IndexFor(1);
            Mapped = (Table<ValueTuple<string,string>>)program["Mapped"].TableUntyped;
        }

        public static Table<string> Day;
        public static Table<ValueTuple<string,string>> NextDay;
        public static KeyIndex<ValueTuple<string,string>,string> NextDay__0_key;
        public static GeneralIndex<ValueTuple<string,string>,string> NextDay__1;
        public static Table<ValueTuple<string,string>> Mapped;
    }

}
#pragma warning restore 0164,8618,8600,8620
