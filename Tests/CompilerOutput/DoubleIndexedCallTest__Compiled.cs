// ReSharper disable InconsistentNaming
// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable RedundantUsingDirective
using TED;
using TED.Interpreter;
using TED.Compiler;
using TED.Tables;

// ReSharper disable once CheckNamespace
namespace CompilerTests;

#pragma warning disable 0164,8618,8600,8620

[CompiledHelpersFor("DoubleIndexedCallTest")]
public class DoubleIndexedCallTest__Compiled : TED.Compiler.CompiledTEDProgram

{

    public static void Mapped__CompiledUpdate()
    {
        // Mapped[in relationship].If(Relation[Sara,Rachel,out relationship])
        {
            string relationship;

            // Relation[Sara,Rachel,out relationship]
            var row__0 = Relation__0_1_key.RowWithKey(("Sara", "Rachel"));
            if (row__0 == Table.NoRow) goto end;
            ref var data__0 = ref Relation.Data[row__0];
            if (data__0.Item1 != "Sara") goto end;
            if (data__0.Item2 != "Rachel") goto end;
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
#pragma warning restore 0164,8618,8600,8620
