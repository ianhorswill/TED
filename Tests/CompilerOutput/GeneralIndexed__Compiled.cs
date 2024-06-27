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
    [CompiledHelpersFor("GeneralIndexed")]
    public class GeneralIndexed__Compiled : TED.Compiler.CompiledTEDProgram
    {

        public static void Q__CompiledUpdate()
        {
            // Q[in i,in j].If(P[out i,out j], P[in j,in i])
            {
                int i;
                int j;

                // P[out i,out j]
                var row__0 = unchecked((uint)-1);
                restart__0:
                if (++row__0 == P.Length) goto end;
                ref var data__0 = ref P.Data[row__0];
                i = data__0.Item1;
                j = data__0.Item2;

                // P[in j,in i]
                var row__1 = P__0.FirstRowWithValue(in j);
                if (row__1 != Table.NoRow) goto match__1;
                goto restart__0;
                restart__1:
                row__1 = P__0.NextRow[row__1];
                if (row__1 == Table.NoRow) goto restart__0;
                match__1:
                ref var data__1 = ref P.Data[row__1];
                if (data__1.Item2 != i) goto restart__1;

                // Write [in i,in j]
                Q.Add((i,j));
                goto restart__1;
            }

            end:;
        }

        public override void Link(TED.Program program)
        {
            program["Q"].CompiledRules = (Action)Q__CompiledUpdate;
            P = (Table<ValueTuple<int,int>>)program["P"].TableUntyped;
            P__0 = (GeneralIndex<ValueTuple<int,int>,int>)P.IndexFor(0);
            Q = (Table<ValueTuple<int,int>>)program["Q"].TableUntyped;
        }

        public static Table<ValueTuple<int,int>> P;
        public static GeneralIndex<ValueTuple<int,int>,int> P__0;
        public static Table<ValueTuple<int,int>> Q;
    }

}
#pragma warning restore 0164,8618,8600,8620
