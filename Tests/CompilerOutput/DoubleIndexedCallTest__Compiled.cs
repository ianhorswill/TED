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
    [CompiledHelpersFor("DoubleIndexedCallTest")]
    public class DoubleIndexedCallTest__Compiled : TED.Compiler.CompiledTEDProgram
    {

        public static void Mapped__CompiledUpdate()
        {
            // Mapped[in relationship].If(Relation[Sara,Rachel,out relationship])
            {
                string relationship;

                // Relation[Sara,Rachel,out relationship]
                var key__0 = ("Sara", "Rachel");
                var row__0 = Table.NoRow;
                for (var bucket__0=key__0.GetHashCode()&Relation__0_1_key.Mask; Relation__0_1_key.Buckets[bucket__0].row != Table.NoRow; bucket__0 = (bucket__0+1)&Relation__0_1_key.Mask)
                    if (Relation__0_1_key.Buckets[bucket__0].key == key__0)
                    {
                        row__0 = Relation__0_1_key.Buckets[bucket__0].row;
                        break;
                    }
                if (row__0 == Table.NoRow) goto end;
                ref var data__0 = ref Relation.Data[row__0];
                relationship = data__0.Item3;

                // Write [in relationship]
                Mapped.Add(relationship);
                goto end;
            }

            end:;
        }

        public override void Link(TED.Program program)
        {
            program["Mapped"].CompiledRules = (Action)Mapped__CompiledUpdate;
            Person = (Table<string>)program["Person"].TableUntyped;
            Relation = (Table<ValueTuple<string,string,string>>)program["Relation"].TableUntyped;
            Relation__0_1_key = (KeyIndex<ValueTuple<string,string,string>,ValueTuple<string,string>>)Relation.IndexFor(0, 1);
            Relation__1 = (GeneralIndex<ValueTuple<string,string,string>,string>)Relation.IndexFor(1);
            Mapped = (Table<string>)program["Mapped"].TableUntyped;
        }

        public static Table<string> Person;
        public static Table<ValueTuple<string,string,string>> Relation;
        public static KeyIndex<ValueTuple<string,string,string>,ValueTuple<string,string>> Relation__0_1_key;
        public static GeneralIndex<ValueTuple<string,string,string>,string> Relation__1;
        public static Table<string> Mapped;
    }

}
#pragma warning restore 0164,8618,8600,8620
