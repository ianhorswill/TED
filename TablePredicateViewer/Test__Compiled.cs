// ReSharper disable InconsistentNaming
// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable RedundantUsingDirective
using TED;
using TED.Interpreter;
using TED.Compiler;
using TED.Tables;

// ReSharper disable once CheckNamespace
namespace TablePredicateViewer;

#pragma warning disable 0164,8618,8600,8620

[CompiledHelpersFor("Test")]
public class Test__Compiled : TED.Compiler.CompiledTEDProgram

{

    public static void Person__initially__CompiledUpdate()
    {
        // Person__initially[in person,in sex,in age,God,God,Alive].If(PrimordialPerson[out person,out sex,out age])
        {
            string person;
            string sex;
            int age;

            // PrimordialPerson[out person,out sex,out age]
            var row__0 = unchecked((uint)-1);
            restart__0:
            if (++row__0 == PrimordialPerson.Length) goto end;
            ref var data__0 = ref PrimordialPerson.Data[row__0];
            person = data__0.Item1;
            sex = data__0.Item2;
            age = data__0.Item3;

            // Write [in person,in sex,in age,God,God,Alive]
            Person__initially.Add((person,sex,age,"God","God",Status.Alive));
            goto restart__0;
        }

        end:;
    }
    public static void Person_age_update__CompiledUpdate()
    {
        // Person_age_update[in person,in temp0].If(Person[out person,out _String6,out age,out _String7,out _String8,Alive], temp0 == age+1)
        {
            string person;
            string _String6;
            int age;
            string _String7;
            string _String8;
            int temp0;

            // Person[out person,out _String6,out age,out _String7,out _String8,Alive]
            var row__0 = Person__5.FirstRowWithValue(Status.Alive);
            if (row__0 != Table.NoRow) goto match__0;
            goto end;
            restart__0:
            row__0 = Person__5.NextRowWithValue(row__0);
            if (row__0 == Table.NoRow) goto end;
            match__0:
            ref var data__0 = ref Person.Data[row__0];
            person = data__0.Item1;
            _String6 = data__0.Item2;
            age = data__0.Item3;
            _String7 = data__0.Item4;
            _String8 = data__0.Item5;
            if (data__0.Item6 != Status.Alive) goto restart__0;

            // temp0 == age+1
            temp0 = age+1;

            // Write [in person,in temp0]
            Person_age_update.Add((person,temp0));
            goto restart__0;
        }

        end:;
    }
    public static void Person_status_update__CompiledUpdate()
    {
        // Person_status_update[in person,Dead].If(Person[out person,out _String3,out _Int321,out _String4,out _String5,Alive], Prob[0.01])
        {
            string person;
            string _String3;
            int _Int321;
            string _String4;
            string _String5;

            // Person[out person,out _String3,out _Int321,out _String4,out _String5,Alive]
            var row__0 = Person__5.FirstRowWithValue(Status.Alive);
            if (row__0 != Table.NoRow) goto match__0;
            goto end;
            restart__0:
            row__0 = Person__5.NextRowWithValue(row__0);
            if (row__0 == Table.NoRow) goto end;
            match__0:
            ref var data__0 = ref Person.Data[row__0];
            person = data__0.Item1;
            _String3 = data__0.Item2;
            _Int321 = data__0.Item3;
            _String4 = data__0.Item4;
            _String5 = data__0.Item5;
            if (data__0.Item6 != Status.Alive) goto restart__0;

            // Prob[0.01]
            if (_Rng0.NextDouble() > 0.01f) goto restart__0;

            // Write [in person,Dead]
            Person_status_update.Add((person,Status.Dead));
            goto restart__0;
        }

        end:;
    }
    public static void Man__CompiledUpdate()
    {
        // Man[in person].If(Person[out person,m,out age,out _String9,out _String10,Alive], >[in age,in  const ])
        {
            string person;
            int age;
            string _String9;
            string _String10;

            // Person[out person,m,out age,out _String9,out _String10,Alive]
            var row__0 = Person__1.FirstRowWithValue("m");
            if (row__0 != Table.NoRow) goto match__0;
            goto end;
            restart__0:
            row__0 = Person__1.NextRowWithValue(row__0);
            if (row__0 == Table.NoRow) goto end;
            match__0:
            ref var data__0 = ref Person.Data[row__0];
            person = data__0.Item1;
            if (data__0.Item2 != "m") goto restart__0;
            age = data__0.Item3;
            _String9 = data__0.Item4;
            _String10 = data__0.Item5;
            if (data__0.Item6 != Status.Alive) goto restart__0;

            // >[in age,in  const ]
            if (!(age>18)) goto restart__0;

            // Write [in person]
            Man.Add(person);
            goto restart__0;
        }

        end:;
    }
    public static void Woman__CompiledUpdate()
    {
        // Woman[in person].If(Person[out person,f,out age,out _String11,out _String12,Alive], >[in age,in  const ])
        {
            string person;
            int age;
            string _String11;
            string _String12;

            // Person[out person,f,out age,out _String11,out _String12,Alive]
            var row__0 = Person__1.FirstRowWithValue("f");
            if (row__0 != Table.NoRow) goto match__0;
            goto end;
            restart__0:
            row__0 = Person__1.NextRowWithValue(row__0);
            if (row__0 == Table.NoRow) goto end;
            match__0:
            ref var data__0 = ref Person.Data[row__0];
            person = data__0.Item1;
            if (data__0.Item2 != "f") goto restart__0;
            age = data__0.Item3;
            _String11 = data__0.Item4;
            _String12 = data__0.Item5;
            if (data__0.Item6 != Status.Alive) goto restart__0;

            // >[in age,in  const ]
            if (!(age>18)) goto restart__0;

            // Write [in person]
            Woman.Add(person);
            goto restart__0;
        }

        end:;
    }
    public static void BirthTo__CompiledUpdate()
    {
        // BirthTo[in woman,in man,in sex].If(Woman[out woman], Prob[0.1], RandomElement[out man], PickRandomly[out sex])
        {
            string woman;
            string man;
            string sex;

            // Woman[out woman]
            var row__0 = unchecked((uint)-1);
            restart__0:
            if (++row__0 == Woman.Length) goto end;
            ref var data__0 = ref Woman.Data[row__0];
            woman = data__0;

            // Prob[0.1]
            if (_Rng1.NextDouble() > 0.1f) goto restart__0;

            // RandomElement[out man]
            if (Man.Length == 0) goto restart__0;
            ref var row___2 = ref Man.Data[_Rng2.Next()%Man.Length];
            man = row___2;

            // PickRandomly[out sex]
            sex = PickRandomlyArray__0[_Rng3.Next()%2];

            // Write [in woman,in man,in sex]
            BirthTo.Add((woman,man,sex));
            goto restart__0;
        }

        end:;
    }
    public static void Person__add__CompiledUpdate()
    {
        // Person__add[in person,f,0,in woman,in man,Alive].If(BirthTo[out man,out woman,f], RandomElement[out person])
        {
            string man;
            string woman;
            string person;

            // BirthTo[out man,out woman,f]
            var row__0 = unchecked((uint)-1);
            restart__0:
            if (++row__0 == BirthTo.Length) goto rule2;
            ref var data__0 = ref BirthTo.Data[row__0];
            man = data__0.Item1;
            woman = data__0.Item2;
            if (data__0.Item3 != "f") goto restart__0;

            // RandomElement[out person]
            if (FemaleName.Length == 0) goto restart__0;
            ref var row___1 = ref FemaleName.Data[_Rng4.Next()%FemaleName.Length];
            person = row___1;

            // Write [in person,f,0,in woman,in man,Alive]
            Person__add.Add((person,"f",0,woman,man,Status.Alive));
            goto restart__0;
        }

        rule2:;

        // Person__add[in person,m,0,in woman,in man,Alive].If(BirthTo[out man,out woman,m], RandomElement[out person])
        {
            string man;
            string woman;
            string person;

            // BirthTo[out man,out woman,m]
            var row__0 = unchecked((uint)-1);
            restart__0:
            if (++row__0 == BirthTo.Length) goto end;
            ref var data__0 = ref BirthTo.Data[row__0];
            man = data__0.Item1;
            woman = data__0.Item2;
            if (data__0.Item3 != "m") goto restart__0;

            // RandomElement[out person]
            if (MaleName.Length == 0) goto restart__0;
            ref var row___1 = ref MaleName.Data[_Rng5.Next()%MaleName.Length];
            person = row___1;

            // Write [in person,m,0,in woman,in man,Alive]
            Person__add.Add((person,"m",0,woman,man,Status.Alive));
            goto restart__0;
        }

        end:;
    }

    public override void Link(TED.Program program)
    {
        program["Person__initially"].CompiledRules = (Action)Person__initially__CompiledUpdate;
        program["Person_age_update"].CompiledRules = (Action)Person_age_update__CompiledUpdate;
        program["Person_status_update"].CompiledRules = (Action)Person_status_update__CompiledUpdate;
        program["Man"].CompiledRules = (Action)Man__CompiledUpdate;
        program["Woman"].CompiledRules = (Action)Woman__CompiledUpdate;
        program["BirthTo"].CompiledRules = (Action)BirthTo__CompiledUpdate;
        program["Person__add"].CompiledRules = (Action)Person__add__CompiledUpdate;
        PrimordialPerson = (Table<ValueTuple<string,string,int>>)program["PrimordialPerson"].TableUntyped;
        Person = (Table<ValueTuple<string,string,int,string,string,Status>>)program["Person"].TableUntyped;
        Person__0 = (KeyIndex<ValueTuple<string,string,int,string,string,Status>,string>)Person.IndexFor(0);
        Person__1 = (GeneralIndex<ValueTuple<string,string,int,string,string,Status>,string>)Person.IndexFor(1);
        Person__5 = (GeneralIndex<ValueTuple<string,string,int,string,string,Status>,Status>)Person.IndexFor(5);
        Person__initially = (Table<ValueTuple<string,string,int,string,string,Status>>)program["Person__initially"].TableUntyped;
        MaleName = (Table<string>)program["MaleName"].TableUntyped;
        FemaleName = (Table<string>)program["FemaleName"].TableUntyped;
        Person_age_update = (Table<ValueTuple<string,int>>)program["Person_age_update"].TableUntyped;
        Person_status_update = (Table<ValueTuple<string,Status>>)program["Person_status_update"].TableUntyped;
        Man = (Table<string>)program["Man"].TableUntyped;
        Woman = (Table<string>)program["Woman"].TableUntyped;
        BirthTo = (Table<ValueTuple<string,string,string>>)program["BirthTo"].TableUntyped;
        Person__add = (Table<ValueTuple<string,string,int,string,string,Status>>)program["Person__add"].TableUntyped;
        _Rng0 = MakeRng();
        _Rng1 = MakeRng();
        _Rng2 = MakeRng();
        _Rng3 = MakeRng();
        PickRandomlyArray__0 = new string[] {"f", "m", };
        _Rng4 = MakeRng();
        _Rng5 = MakeRng();
    }

    public static Table<ValueTuple<string,string,int>> PrimordialPerson;
    public static Table<ValueTuple<string,string,int,string,string,Status>> Person;
    public static KeyIndex<ValueTuple<string,string,int,string,string,Status>,string> Person__0;
    public static GeneralIndex<ValueTuple<string,string,int,string,string,Status>,string> Person__1;
    public static GeneralIndex<ValueTuple<string,string,int,string,string,Status>,Status> Person__5;
    public static Table<ValueTuple<string,string,int,string,string,Status>> Person__initially;
    public static Table<string> MaleName;
    public static Table<string> FemaleName;
    public static Table<ValueTuple<string,int>> Person_age_update;
    public static Table<ValueTuple<string,Status>> Person_status_update;
    public static Table<string> Man;
    public static Table<string> Woman;
    public static Table<ValueTuple<string,string,string>> BirthTo;
    public static Table<ValueTuple<string,string,int,string,string,Status>> Person__add;
    public static Random _Rng0;
    public static Random _Rng1;
    public static Random _Rng2;
    public static Random _Rng3;
    public static string[] PickRandomlyArray__0;
    public static Random _Rng4;
    public static Random _Rng5;
}
#pragma warning restore 0164,8618,8600,8620
