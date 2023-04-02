namespace TED
{
    /// <summary>
    /// Implements negation of a goal
    /// </summary>
    internal sealed class MaximalPrimitive<T1> : Predicate
    {
        public static MaximalPrimitive<T1> Maximal = new MaximalPrimitive<T1>(1);
        public static MaximalPrimitive<T1> Minimal = new MaximalPrimitive<T1>(-1);

        private readonly int multiplier;

        public MaximalPrimitive(int multiplier) : base("Maximal")
        {
            this.multiplier = multiplier;
        }

        public Goal this[Var<T1> arg, Var<float> utility, TED.Goal g] => new Goal(this, arg, utility, new Constant<TED.Goal>(g));

        public class Goal : TED.Goal
        {
            private readonly MaximalPrimitive<T1> predicate;
            private readonly Var<T1> arg;
            private readonly Var<float> utility;
            private readonly TED.Goal generator;

            public Goal(MaximalPrimitive<T1> predicate, Var<T1> arg, Var<float> utility, Constant<TED.Goal> g) : base(new Term[] { arg, utility, g })
            {
                this.arg = arg;
                this.utility = utility;
                this.predicate = predicate;
                generator = g.Value;
            }

            public override Predicate Predicate => predicate;

            internal override TED.Goal RenameArguments(Substitution s)
                => new Goal(predicate, (Var<T1>)s.SubstituteVariable(arg), (Var<float>)s.SubstituteVariable(utility), new Constant<TED.Goal>(generator.RenameArguments(s)));

            internal override TED.Call MakeCall(GoalAnalyzer ga)
            {
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

        private class Call : TED.Call
        {
            private readonly MaximalPrimitive<T1> predicate;
            private readonly MatchOperation<T1> arg;
            private readonly MatchOperation<float> utility;
            private readonly TED.Call goal;
            private bool restart;

            public Call(MaximalPrimitive<T1> predicate, MatchOperation<T1> arg, MatchOperation<float> utility, TED.Call call) : base(Maximal)
            {
                this.arg = arg;
                this.utility = utility;
                goal = call;
                this.predicate = predicate;
            }

            public override IPattern ArgumentPattern => new Pattern<T1, float, TED.Call>(arg, utility,
                MatchOperation<TED.Call>.Constant(goal));

            public override void Reset()
            {
                restart = true;
            }

            public override bool NextSolution()
            {
                if (!restart) return false;
                restart = false;
                goal.Reset();
                var gotOne = false;
                var argCell = arg.ValueCell;
                var utilityCell = utility.ValueCell;
                var bestArg = argCell.Value;
                var bestUtil = utilityCell.Value;
                while (goal.NextSolution())
                {
                    var u = utilityCell.Value * predicate.multiplier;
                    if (!gotOne || u > bestUtil)
                    {
                        bestArg = argCell.Value;
                        bestUtil = u;
                    }

                    gotOne = true;
                }

                if (gotOne)
                {
                    argCell.Value = bestArg;
                    utilityCell.Value = bestUtil * predicate.multiplier;
                }

                return gotOne;
            }
        }
    }

    /// <summary>
    /// Implements negation of a goal
    /// </summary>
    internal sealed class MaximalPrimitive<T1, T2> : Predicate
    {
        public static MaximalPrimitive<T1, T2> Maximal = new MaximalPrimitive<T1, T2>(1);
        public static MaximalPrimitive<T1, T2> Minimal = new MaximalPrimitive<T1, T2>(-1);

        private readonly int multiplier;

        public MaximalPrimitive(int multiplier) : base("Maximal")
        {
            this.multiplier = multiplier;
        }

        public Goal this[Var<T1> arg1, Var<T2> arg2, Var<float> utility, TED.Goal g] => new Goal(this, arg1, arg2, utility, new Constant<TED.Goal>(g));

        public class Goal : TED.Goal
        {
            private readonly MaximalPrimitive<T1, T2> predicate;
            private readonly Var<T1> arg1;
            private readonly Var<T2> arg2;
            private readonly Var<float> utility;
            private readonly TED.Goal generator;

            public Goal(MaximalPrimitive<T1, T2> predicate, Var<T1> arg1, Var<T2> arg2, Var<float> utility, Constant<TED.Goal> g) : base(new Term[] { arg1, utility, g })
            {
                this.arg1 = arg1;
                this.arg2 = arg2;
                this.utility = utility;
                this.predicate = predicate;
                generator = g.Value;
            }

            public override Predicate Predicate => predicate;

            internal override TED.Goal RenameArguments(Substitution s)
                => new Goal(predicate, (Var<T1>)s.SubstituteVariable(arg1), (Var<T2>)s.SubstituteVariable(arg2), (Var<float>)s.SubstituteVariable(utility), new Constant<TED.Goal>(generator.RenameArguments(s)));

            internal override TED.Call MakeCall(GoalAnalyzer ga)
            {
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

        private class Call : TED.Call
        {
            private readonly MaximalPrimitive<T1, T2> predicate;
            private readonly MatchOperation<T1> arg1;
            private readonly MatchOperation<T2> arg2;
            private readonly MatchOperation<float> utility;
            private readonly TED.Call goal;
            private bool restart;

            public Call(MaximalPrimitive<T1, T2> predicate, MatchOperation<T1> arg1, MatchOperation<T2> arg2, MatchOperation<float> utility, TED.Call call) : base(Maximal)
            {
                this.arg1 = arg1;
                this.arg2 = arg2;
                this.utility = utility;
                goal = call;
                this.predicate = predicate;
            }

            public override IPattern ArgumentPattern => new Pattern<T1, T2, float, TED.Call>(arg1, arg2, utility,
                MatchOperation<TED.Call>.Constant(goal));

            public override void Reset()
            {
                restart = true;
            }

            public override bool NextSolution()
            {
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
                while (goal.NextSolution())
                {
                    var u = utilityCell.Value * predicate.multiplier;
                    if (!gotOne || u > bestUtil)
                    {
                        bestArg1 = arg1Cell.Value;
                        bestArg2 = arg2Cell.Value;
                        bestUtil = u;
                    }

                    gotOne = true;
                }

                if (gotOne)
                {
                    arg1Cell.Value = bestArg1;
                    arg2Cell.Value = bestArg2;
                    utilityCell.Value = bestUtil * predicate.multiplier;
                }

                return gotOne;
            }
        }
    }

        /// <summary>
    /// Implements negation of a goal
    /// </summary>
    internal sealed class MaximalPrimitive<T1, T2, T3> : Predicate
    {
        public static MaximalPrimitive<T1, T2, T3> Maximal = new MaximalPrimitive<T1, T2, T3>(1);
        public static MaximalPrimitive<T1, T2, T3> Minimal = new MaximalPrimitive<T1, T2, T3>(-1);

        private readonly int multiplier;

        public MaximalPrimitive(int multiplier) : base("Maximal")
        {
            this.multiplier = multiplier;
        }

        public Goal this[Var<T1> arg1, Var<T2> arg2, Var<T3> arg3, Var<float> utility, TED.Goal g] => new Goal(this, arg1, arg2, arg3, utility, new Constant<TED.Goal>(g));

        public class Goal : TED.Goal
        {
            private readonly MaximalPrimitive<T1, T2, T3> predicate;
            private readonly Var<T1> arg1;
            private readonly Var<T2> arg2;
            private readonly Var<T3> arg3;
            private readonly Var<float> utility;
            private readonly TED.Goal generator;

            public Goal(MaximalPrimitive<T1, T2, T3> predicate, Var<T1> arg1, Var<T2> arg2, Var<T3> arg3, Var<float> utility, Constant<TED.Goal> g) : base(new Term[] { arg1, utility, g })
            {
                this.arg1 = arg1;
                this.arg2 = arg2;
                this.arg3 = arg3;
                this.utility = utility;
                this.predicate = predicate;
                generator = g.Value;
            }

            public override Predicate Predicate => predicate;

            internal override TED.Goal RenameArguments(Substitution s)
                => new Goal(predicate, (Var<T1>)s.SubstituteVariable(arg1), (Var<T2>)s.SubstituteVariable(arg2), (Var<T3>)s.SubstituteVariable(arg3), (Var<float>)s.SubstituteVariable(utility), new Constant<TED.Goal>(generator.RenameArguments(s)));

            internal override TED.Call MakeCall(GoalAnalyzer ga)
            {
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

                return new Call(predicate, ga.Emit(arg1), ga.Emit(arg2), ga.Emit(arg3), ga.Emit(utility), call);
            }
        }

        private class Call : TED.Call
        {
            private readonly MaximalPrimitive<T1, T2, T3> predicate;
            private readonly MatchOperation<T1> arg1;
            private readonly MatchOperation<T2> arg2;
            private readonly MatchOperation<T3> arg3;
            private readonly MatchOperation<float> utility;
            private readonly TED.Call goal;
            private bool restart;

            public Call(MaximalPrimitive<T1, T2, T3> predicate, MatchOperation<T1> arg1, MatchOperation<T2> arg2, MatchOperation<T3> arg3, MatchOperation<float> utility, TED.Call call) : base(Maximal)
            {
                this.arg1 = arg1;
                this.arg2 = arg2;
                this.arg3 = arg3;
                this.utility = utility;
                goal = call;
                this.predicate = predicate;
            }

            public override IPattern ArgumentPattern => new Pattern<T1, T2, float, TED.Call>(arg1, arg2, utility,
                MatchOperation<TED.Call>.Constant(goal));

            public override void Reset()
            {
                restart = true;
            }

            public override bool NextSolution()
            {
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
                while (goal.NextSolution())
                {
                    var u = utilityCell.Value * predicate.multiplier;
                    if (!gotOne || u > bestUtil)
                    {
                        bestArg1 = arg1Cell.Value;
                        bestArg2 = arg2Cell.Value;
                        bestArg3 = arg3Cell.Value;
                        bestUtil = u;
                    }

                    gotOne = true;
                }

                if (gotOne)
                {
                    arg1Cell.Value = bestArg1;
                    arg2Cell.Value = bestArg2;
                    arg3Cell.Value = bestArg3;
                    utilityCell.Value = bestUtil * predicate.multiplier;
                }

                return gotOne;
            }
        }
    }

}
