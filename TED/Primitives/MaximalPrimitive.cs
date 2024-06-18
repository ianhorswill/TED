using System;
using System.Runtime.InteropServices;
using TED.Compiler;
using TED.Interpreter;
using TED.Preprocessing;
using static TED.Compiler.Compiler;

namespace TED.Primitives
{
    /// <summary>
    /// Implements Maximal and Minimal Primitives with one argument to check against
    /// </summary>
    internal sealed class MaximalPrimitive<TArg, TUtility> : Predicate where TUtility : IComparable<TUtility> {

        public static MaximalPrimitive<TArg, TUtility> Maximal = new MaximalPrimitive<TArg, TUtility>(1);
        public static MaximalPrimitive<TArg, TUtility> Minimal = new MaximalPrimitive<TArg, TUtility>(-1);

        private readonly int multiplier;

        public MaximalPrimitive(int multiplier) : base("Maximal") => this.multiplier = multiplier;

        public Goal this[Var<TArg> arg, Var<TUtility> utility, Interpreter.Goal g] => 
            new Goal(this, arg, utility, new Constant<Interpreter.Goal>(g));

        public class Goal : Interpreter.Goal {
            private readonly MaximalPrimitive<TArg, TUtility> predicate;
            private readonly Var<TArg> arg;
            private readonly Var<TUtility> utility;
            private readonly Interpreter.Goal generator;

            public Goal(MaximalPrimitive<TArg, TUtility> predicate, Var<TArg> arg, Var<TUtility> utility, 
                        Constant<Interpreter.Goal> g) : base(new Term[] { arg, utility, g }) {
                this.arg = arg;
                this.utility = utility;
                this.predicate = predicate;
                generator = g.Value;
            }

            public override Predicate Predicate => predicate;

            internal override Interpreter.Goal RenameArguments(Substitution s)
                => new Goal(predicate, (Var<TArg>)s.SubstituteVariable(arg), 
                    (Var<TUtility>)s.SubstituteVariable(utility), 
                    new Constant<Interpreter.Goal>(generator.RenameArguments(s)));

            internal override Interpreter.Call MakeCall(GoalAnalyzer ga) {
                if (ga.IsInstantiated(arg))
                    throw new InstantiationException(Maximal, arg);
                if (ga.IsInstantiated(utility))
                    throw new InstantiationException(Maximal, utility);

                var call = generator.MakeCall(ga);

                if (!ga.IsInstantiated(arg))
                    throw new InstantiationException($"Goal {generator} does not bind the variable {arg} in call {this}");
                if (!ga.IsInstantiated(utility))
                    throw new InstantiationException($"Goal {generator} does not bind the variable {utility} in call {this}");

                return new Call(predicate, ga.Emit(arg), ga.Emit(utility), call);
            }
        }

        private class Call : Interpreter.Call {
            private readonly MaximalPrimitive<TArg, TUtility> predicate;
            private readonly MatchOperation<TArg> arg;
            private readonly MatchOperation<TUtility> utility;
            private readonly Interpreter.Call goal;
            private bool restart;

            public Call(MaximalPrimitive<TArg, TUtility> predicate, MatchOperation<TArg> arg, MatchOperation<TUtility> utility, 
                        Interpreter.Call call) : base(Maximal) {
                this.arg = arg;
                this.utility = utility;
                goal = call;
                this.predicate = predicate;
            }

            public override IPattern ArgumentPattern => new Pattern<TArg, TUtility, Interpreter.Call>(arg, utility,
                MatchOperation<Interpreter.Call>.Constant(goal));

            public override void Reset() => restart = true;

            public override bool NextSolution() {
                if (!restart) return false;
                restart = false;
                goal.Reset();
                var gotOne = false;
                var argCell = arg.ValueCell;
                var utilityCell = utility.ValueCell;
                var bestArg = argCell.Value;
                var bestUtil = utilityCell.Value;
                while (goal.NextSolution()) {
                    var u = utilityCell.Value;
                    if (!gotOne || u.CompareTo(bestUtil) * predicate.multiplier > 0) {
                        bestArg = argCell.Value;
                        bestUtil = u;
                    }
                    gotOne = true;
                }
                if (!gotOne) return gotOne;
                argCell.Value = bestArg;
                utilityCell.Value = bestUtil;
                return gotOne;
            }

            public override Continuation Compile(Compiler.Compiler compiler, Continuation fail, string identifierSuffix)
            {
                var gotOne = $"gotOne{identifierSuffix}";
                var bestArg = $"best{arg.Cell.Name}{identifierSuffix}";
                var bestUtility = $"best{utility.Cell.Name}{identifierSuffix}";
                var comparison = predicate.multiplier > 0 ? ">" : "<";

                // Initialize state variables
                compiler.Indented($"var {gotOne} = false;");
                compiler.Indented($"var {bestArg} = default({FormatType(arg.Type)});");
                compiler.Indented($"var {bestUtility} = default({FormatType(utility.Type)});");
                var done = new Continuation($"maxDone{identifierSuffix}");

                // Find a solution
                var nextSolution = compiler.CompileGoal(goal, done, identifierSuffix + "_maxLoop");

                compiler.NewLine();

                // Update if it's a better solution
                compiler.Indented($"if (!{gotOne} || {utility.Cell.Name} {comparison} {bestUtility})");
                compiler.CurlyBraceBlock(() =>
                {
                    compiler.Indented($"{gotOne} = true;");
                    compiler.Indented($"{bestArg} = {arg.Cell.Name};");
                    compiler.Indented($"{bestUtility} = {utility.Cell.Name};");
                });
                // Get next solution
                compiler.Indented(nextSolution.Invoke+";");

                compiler.NewLine();

                // Done
                compiler.Label(done);
                compiler.Indented($"if (!{gotOne}) {fail.Invoke};");  // Complete failure
                compiler.Indented($"{arg.Cell.Name} = {bestArg};");
                compiler.Indented($"{utility.Cell.Name} = {bestUtility};");
                return fail;
            }
        }
    }

    /// <summary>
    /// Implements Maximal and Minimal Primitives with two arguments to check against
    /// </summary>
    internal sealed class MaximalPrimitive<TArg1, TArg2, TUtility> : Predicate where TUtility : IComparable<TUtility> {

        public static MaximalPrimitive<TArg1, TArg2, TUtility> Maximal = new MaximalPrimitive<TArg1, TArg2, TUtility>(1);
        public static MaximalPrimitive<TArg1, TArg2, TUtility> Minimal = new MaximalPrimitive<TArg1, TArg2, TUtility>(-1);

        private readonly int multiplier;

        public MaximalPrimitive(int multiplier) : base("Maximal") => this.multiplier = multiplier;

        public Goal this[Var<TArg1> arg1, Var<TArg2> arg2, Var<TUtility> utility, Interpreter.Goal g] =>
            new Goal(this, arg1, arg2, utility, new Constant<Interpreter.Goal>(g));

        public class Goal : Interpreter.Goal {
            private readonly MaximalPrimitive<TArg1, TArg2, TUtility> predicate;
            private readonly Var<TArg1> arg1;
            private readonly Var<TArg2> arg2;
            private readonly Var<TUtility> utility;
            private readonly Interpreter.Goal generator;

            public Goal(MaximalPrimitive<TArg1, TArg2, TUtility> predicate, Var<TArg1> arg1, Var<TArg2> arg2, Var<TUtility> utility,
                        Constant<Interpreter.Goal> g) : base(new Term[] { arg1, arg2, utility, g }) {
                this.arg1 = arg1;
                this.arg2 = arg2;
                this.utility = utility;
                this.predicate = predicate;
                generator = g.Value;
            }

            public override Predicate Predicate => predicate;

            internal override Interpreter.Goal RenameArguments(Substitution s)
                => new Goal(predicate, (Var<TArg1>)s.SubstituteVariable(arg1),
                    (Var<TArg2>)s.SubstituteVariable(arg2),
                    (Var<TUtility>)s.SubstituteVariable(utility),
                    new Constant<Interpreter.Goal>(generator.RenameArguments(s)));

            internal override Interpreter.Call MakeCall(GoalAnalyzer ga) {
                if (ga.IsInstantiated(arg1))
                    throw new InstantiationException(Maximal, arg1);
                if (ga.IsInstantiated(arg2))
                    throw new InstantiationException(Maximal, arg2);
                if (ga.IsInstantiated(utility))
                    throw new InstantiationException(Maximal, utility);

                var call = generator.MakeCall(ga);

                if (!ga.IsInstantiated(arg1))
                    throw new InstantiationException($"Goal {generator} does not bind the variable {arg1} in call {this}");
                if (!ga.IsInstantiated(arg2))
                    throw new InstantiationException($"Goal {generator} does not bind the variable {arg2} in call {this}");
                if (!ga.IsInstantiated(utility))
                    throw new InstantiationException($"Goal {generator} does not bind the variable {utility} in call {this}");

                return new Call(predicate, ga.Emit(arg1), ga.Emit(arg2), ga.Emit(utility), call);
            }
        }

        private class Call : Interpreter.Call {
            private readonly MaximalPrimitive<TArg1, TArg2, TUtility> predicate;
            private readonly MatchOperation<TArg1> arg1;
            private readonly MatchOperation<TArg2> arg2;
            private readonly MatchOperation<TUtility> utility;
            private readonly Interpreter.Call goal;
            private bool restart;

            public Call(MaximalPrimitive<TArg1, TArg2, TUtility> predicate, MatchOperation<TArg1> arg1, MatchOperation<TArg2> arg2, 
                        MatchOperation<TUtility> utility, Interpreter.Call call) : base(Maximal) {
                this.arg1 = arg1;
                this.arg2 = arg2;
                this.utility = utility;
                goal = call;
                this.predicate = predicate;
            }

            public override IPattern ArgumentPattern => new Pattern<TArg1, TArg2, TUtility, Interpreter.Call>(arg1, arg2, utility,
                MatchOperation<Interpreter.Call>.Constant(goal));

            public override void Reset() => restart = true;

            public override bool NextSolution() {
                if (!restart) return false;
                restart = false;
                goal.Reset();
                var gotOne = false;
                var arg1Cell = arg1.ValueCell;
                var arg2Cell = arg2.ValueCell;
                var utilityCell = utility.ValueCell;
                var bestArg1 = arg1Cell.Value;
                var bestArg2 = arg2Cell.Value;
                var bestUtil = utilityCell.Value;
                while (goal.NextSolution()) {
                    var u = utilityCell.Value;
                    if (!gotOne || u.CompareTo(bestUtil) * predicate.multiplier > 0) {
                        bestArg1 = arg1Cell.Value;
                        bestArg2 = arg2Cell.Value;
                        bestUtil = u;
                    }
                    gotOne = true;
                }
                if (!gotOne) return gotOne;
                arg1Cell.Value = bestArg1;
                arg2Cell.Value = bestArg2;
                utilityCell.Value = bestUtil;
                return gotOne;
            }

            public override Continuation Compile(Compiler.Compiler compiler, Continuation fail, string identifierSuffix)
            {
                var gotOne = $"gotOne{identifierSuffix}";
                var bestArg1 = $"best{arg1.Cell.Name}{identifierSuffix}";
                var bestArg2 = $"best{arg2.Cell.Name}{identifierSuffix}";
                var bestUtility = $"best{utility.Cell.Name}{identifierSuffix}";
                var comparison = predicate.multiplier > 0 ? ">" : "<";

                // Initialize state variables
                compiler.Indented($"var {gotOne} = false;");
                compiler.Indented($"var {bestArg1} = default({FormatType(arg1.Type)});");
                compiler.Indented($"var {bestArg2} = default({FormatType(arg2.Type)});");
                compiler.Indented($"var {bestUtility} = default({FormatType(utility.Type)});");
                var done = new Continuation($"maxDone{identifierSuffix}");

                // Find a solution
                var nextSolution = compiler.CompileGoal(goal, done, identifierSuffix + "_maxLoop");

                compiler.NewLine();

                // Update if it's a better solution
                compiler.Indented($"if (!{gotOne} || {utility.Cell.Name} {comparison} {bestUtility})");
                compiler.CurlyBraceBlock(() =>
                {
                    compiler.Indented($"{gotOne} = true;");
                    compiler.Indented($"{bestArg1} = {arg1.Cell.Name};");
                    compiler.Indented($"{bestArg2} = {arg2.Cell.Name};");
                    compiler.Indented($"{bestUtility} = {utility.Cell.Name};");
                });
                // Get next solution
                compiler.Indented(nextSolution.Invoke+";");

                compiler.NewLine();

                // Done
                compiler.Label(done);
                compiler.Indented($"if (!{gotOne}) {fail.Invoke};");  // Complete failure
                compiler.Indented($"{arg1.Cell.Name} = {bestArg1};");
                compiler.Indented($"{arg2.Cell.Name} = {bestArg2};");
                compiler.Indented($"{utility.Cell.Name} = {bestUtility};");
                return fail;
            }
        }
    }


    /// <summary>
    /// Implements Maximal and Minimal Primitives with three arguments to check against
    /// </summary>
    internal sealed class MaximalPrimitive<TArg1, TArg2, TArg3, TUtility> : Predicate where TUtility : IComparable<TUtility> {

        public static MaximalPrimitive<TArg1, TArg2, TArg3, TUtility> Maximal = new MaximalPrimitive<TArg1, TArg2, TArg3, TUtility>(1);
        public static MaximalPrimitive<TArg1, TArg2, TArg3, TUtility> Minimal = new MaximalPrimitive<TArg1, TArg2, TArg3, TUtility>(-1);

        private readonly int multiplier;

        public MaximalPrimitive(int multiplier) : base("Maximal") => this.multiplier = multiplier;

        public Goal this[Var<TArg1> arg1, Var<TArg2> arg2, Var<TArg3> arg3, Var<TUtility> utility, Interpreter.Goal g] =>
            new Goal(this, arg1, arg2, arg3, utility, new Constant<Interpreter.Goal>(g));

        public class Goal : Interpreter.Goal {
            private readonly MaximalPrimitive<TArg1, TArg2, TArg3, TUtility> predicate;
            private readonly Var<TArg1> arg1;
            private readonly Var<TArg2> arg2;
            private readonly Var<TArg3> arg3;
            private readonly Var<TUtility> utility;
            private readonly Interpreter.Goal generator;

            public Goal(MaximalPrimitive<TArg1, TArg2, TArg3, TUtility> predicate, Var<TArg1> arg1, Var<TArg2> arg2, Var<TArg3> arg3, Var<TUtility> utility,
                        Constant<Interpreter.Goal> g) : base(new Term[] { arg1, arg2, arg3, utility, g }) {
                this.arg1 = arg1;
                this.arg2 = arg2;
                this.arg3 = arg3;
                this.utility = utility;
                this.predicate = predicate;
                generator = g.Value;
            }

            public override Predicate Predicate => predicate;

            internal override Interpreter.Goal RenameArguments(Substitution s)
                => new Goal(predicate, (Var<TArg1>)s.SubstituteVariable(arg1),
                    (Var<TArg2>)s.SubstituteVariable(arg2),
                    (Var<TArg3>)s.SubstituteVariable(arg3),
                    (Var<TUtility>)s.SubstituteVariable(utility),
                    new Constant<Interpreter.Goal>(generator.RenameArguments(s)));

            internal override Interpreter.Call MakeCall(GoalAnalyzer ga) {
                if (ga.IsInstantiated(arg1))
                    throw new InstantiationException(Maximal, arg1);
                if (ga.IsInstantiated(arg2))
                    throw new InstantiationException(Maximal, arg2);
                if (ga.IsInstantiated(arg3))
                    throw new InstantiationException(Maximal, arg3);
                if (ga.IsInstantiated(utility))
                    throw new InstantiationException(Maximal, utility);

                var call = generator.MakeCall(ga);

                if (!ga.IsInstantiated(arg1))
                    throw new InstantiationException($"Goal {generator} does not bind the variable {arg1} in call {this}");
                if (!ga.IsInstantiated(arg2))
                    throw new InstantiationException($"Goal {generator} does not bind the variable {arg2} in call {this}");
                if (!ga.IsInstantiated(arg3))
                    throw new InstantiationException($"Goal {generator} does not bind the variable {arg3} in call {this}");
                if (!ga.IsInstantiated(utility))
                    throw new InstantiationException($"Goal {generator} does not bind the variable {utility} in call {this}");

                return new Call(predicate, ga.Emit(arg1), ga.Emit(arg2), ga.Emit(arg3), ga.Emit(utility), call);
            }
        }

        private class Call : Interpreter.Call {
            private readonly MaximalPrimitive<TArg1, TArg2, TArg3, TUtility> predicate;
            private readonly MatchOperation<TArg1> arg1;
            private readonly MatchOperation<TArg2> arg2;
            private readonly MatchOperation<TArg3> arg3;
            private readonly MatchOperation<TUtility> utility;
            private readonly Interpreter.Call goal;
            private bool restart;

            public Call(MaximalPrimitive<TArg1, TArg2, TArg3, TUtility> predicate, MatchOperation<TArg1> arg1, 
                        MatchOperation<TArg2> arg2, MatchOperation<TArg3> arg3,
                        MatchOperation<TUtility> utility, Interpreter.Call call) : base(Maximal) {
                this.arg1 = arg1;
                this.arg2 = arg2;
                this.arg3 = arg3;
                this.utility = utility;
                goal = call;
                this.predicate = predicate;
            }

            public override IPattern ArgumentPattern => new Pattern<TArg1, TArg2, TArg3, TUtility, Interpreter.Call>(arg1, arg2, arg3, utility,
                MatchOperation<Interpreter.Call>.Constant(goal));

            public override void Reset() => restart = true;

            public override bool NextSolution() {
                if (!restart) return false;
                restart = false;
                goal.Reset();
                var gotOne = false;
                var arg1Cell = arg1.ValueCell;
                var arg2Cell = arg2.ValueCell;
                var arg3Cell = arg3.ValueCell;
                var utilityCell = utility.ValueCell;
                var bestArg1 = arg1Cell.Value;
                var bestArg2 = arg2Cell.Value;
                var bestArg3 = arg3Cell.Value;
                var bestUtil = utilityCell.Value;
                while (goal.NextSolution()) {
                    var u = utilityCell.Value;
                    if (!gotOne || u.CompareTo(bestUtil) * predicate.multiplier > 0) {
                        bestArg1 = arg1Cell.Value;
                        bestArg2 = arg2Cell.Value;
                        bestArg3 = arg3Cell.Value;
                        bestUtil = u;
                    }
                    gotOne = true;
                }
                if (!gotOne) return gotOne;
                arg1Cell.Value = bestArg1;
                arg2Cell.Value = bestArg2;
                arg3Cell.Value = bestArg3;
                utilityCell.Value = bestUtil;
                return gotOne;
            }

            public override Continuation Compile(Compiler.Compiler compiler, Continuation fail, string identifierSuffix)
            {
                var gotOne = $"gotOne{identifierSuffix}";
                var bestArg1 = $"best{arg1.Cell.Name}{identifierSuffix}";
                var bestArg2 = $"best{arg2.Cell.Name}{identifierSuffix}";
                var bestArg3 = $"best{arg3.Cell.Name}{identifierSuffix}";
                var bestUtility = $"best{utility.Cell.Name}{identifierSuffix}";
                var comparison = predicate.multiplier > 0 ? ">" : "<";

                // Initialize state variables
                compiler.Indented($"var {gotOne} = false;");
                compiler.Indented($"var {bestArg1} = default({FormatType(arg1.Type)});");
                compiler.Indented($"var {bestArg2} = default({FormatType(arg2.Type)});");
                compiler.Indented($"var {bestArg3} = default({FormatType(arg3.Type)});");
                compiler.Indented($"var {bestUtility} = default({FormatType(utility.Type)});");
                var done = new Continuation($"maxDone{identifierSuffix}");

                // Find a solution
                var nextSolution = compiler.CompileGoal(goal, done, identifierSuffix + "_maxLoop");

                compiler.NewLine();

                // Update if it's a better solution
                compiler.Indented($"if (!{gotOne} || {utility.Cell.Name} {comparison} {bestUtility})");
                compiler.CurlyBraceBlock(() =>
                {
                    compiler.Indented($"{gotOne} = true;");
                    compiler.Indented($"{bestArg1} = {arg1.Cell.Name};");
                    compiler.Indented($"{bestArg2} = {arg2.Cell.Name};");
                    compiler.Indented($"{bestArg3} = {arg3.Cell.Name};");
                    compiler.Indented($"{bestUtility} = {utility.Cell.Name};");
                });
                // Get next solution
                compiler.Indented(nextSolution.Invoke+";");

                compiler.NewLine();

                // Done
                compiler.Label(done);
                compiler.Indented($"if (!{gotOne}) {fail.Invoke};");  // Complete failure
                compiler.Indented($"{arg1.Cell.Name} = {bestArg1};");
                compiler.Indented($"{arg2.Cell.Name} = {bestArg2};");
                compiler.Indented($"{arg3.Cell.Name} = {bestArg3};");
                compiler.Indented($"{utility.Cell.Name} = {bestUtility};");
                return fail;
            }
        }
    }

}
