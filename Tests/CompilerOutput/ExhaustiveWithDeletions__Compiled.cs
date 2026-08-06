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
    [CompiledHelpersFor("ExhaustiveWithDeletions")]
    public class ExhaustiveWithDeletions__Compiled : TED.Compiler.CompiledTEDProgram
    {

        public static void Q__initially__CompiledUpdate()
        {
            Q__initially.BeginRebuild();
            try
            {
                // Q__initially[in a,in b].If(P[out a], P[out b], a == b)
                try
                {
                    int a;
                    int b;

                    // P[out a]
                    var row__0 = unchecked((uint)-1);
                    restart__0:
                    if (++row__0 == P.Length) goto end;
                    ref var data__0 = ref P.Data[row__0];
                    a = data__0;

                    // P[out b]
                    var row__1 = unchecked((uint)-1);
                    restart__1:
                    if (++row__1 == P.Length) goto restart__0;
                    ref var data__1 = ref P.Data[row__1];
                    b = data__1;

                    // a == b
                    if (a != b) goto restart__1;

                    // Write [in a,in b]
                    Q__initially.RebuildRowNonUnique((a,b));
                    goto restart__1;
                }
                catch (Exception _ruleException) { Q__initially.ThrowDeferred(_ruleException); }

                end:;
            }
            finally
            {
                Q__initially.EndRebuild();
            }
        }
        public static void Q_delete_a__CompiledUpdate()
        {
            Q_delete_a.BeginRebuild();
            try
            {
                // Q_delete_a[in a].If(a == 4)
                try
                {
                    int a;

                    // a == 4
                    a = 4;

                    // Write [in a]
                    Q_delete_a.RebuildRowNonUnique(a);
                    goto end;
                }
                catch (Exception _ruleException) { Q_delete_a.ThrowDeferred(_ruleException); }

                end:;
            }
            finally
            {
                Q_delete_a.EndRebuild();
            }
        }
        public static void R__CompiledUpdate()
        {
            R.BeginRebuild();
            try
            {
                // R[in a].If(Q[out a,out b])
                try
                {
                    int a;

                    // Q[out a,out b]
                    var row__0 = unchecked((uint)-1);
                    restart__0:
                    if (++row__0 == Q.Length) goto end;
                    if (Q.RowDeleted![row__0]) goto restart__0;
                    ref var data__0 = ref Q.Data[row__0];
                    a = data__0.Item1;

                    // Write [in a]
                    R.RebuildRowNonUnique(a);
                    goto restart__0;
                }
                catch (Exception _ruleException) { R.ThrowDeferred(_ruleException); }

                end:;
            }
            finally
            {
                R.EndRebuild();
            }
        }

        public override void Link(TED.Program program)
        {
            program["Q__initially"].CompiledRules = (Action)Q__initially__CompiledUpdate;
            program["Q_delete_a"].CompiledRules = (Action)Q_delete_a__CompiledUpdate;
            program["R"].CompiledRules = (Action)R__CompiledUpdate;
            P = (Table<int>)program["P"].TableUntyped;
            Q = (Table<ValueTuple<int,int>>)program["Q"].TableUntyped;
            Q__0_key = (KeyIndex<ValueTuple<int,int>,int>)Q.IndexFor(0);
            Q__initially = (Table<ValueTuple<int,int>>)program["Q__initially"].TableUntyped;
            Q_delete_a = (Table<int>)program["Q_delete_a"].TableUntyped;
            R = (Table<int>)program["R"].TableUntyped;
        }

        public static Table<int> P;
        public static Table<ValueTuple<int,int>> Q;
        public static KeyIndex<ValueTuple<int,int>,int> Q__0_key;
        public static Table<ValueTuple<int,int>> Q__initially;
        public static Table<int> Q_delete_a;
        public static Table<int> R;
    }

}
#pragma warning restore 0164,8618,8600,8620
