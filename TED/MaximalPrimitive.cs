namespace TED
{
    /// <summary>
    /// Implements negation of a goal
    /// </summary>
    public sealed class MaximalPrimitive<T1> : AnyPredicate
    {
        public static MaximalPrimitive<T1> Singleton = new MaximalPrimitive<T1>();

        public MaximalPrimitive() : base("Maximal")
        {
        }

        public Goal this[Var<T1> arg, Var<float> utility, AnyGoal g] => new Goal(arg, utility, new Constant<AnyGoal>(g));

        public class Goal : AnyGoal
        {
            private readonly Var<T1> Arg;
            private readonly Var<float> Utility;
            private readonly AnyGoal Generator;

            public Goal(Var<T1> arg, Var<float> utility, Constant<AnyGoal> g) : base(new AnyTerm[] { arg, utility, g })
            {
                Arg = arg;
                Utility = utility;
                Generator = g.Value;
            }

            public override AnyPredicate Predicate => Singleton;

            internal override AnyGoal RenameArguments(Substitution s)
                => new Goal((Var<T1>)s.SubstituteVariable(Arg), (Var<float>)s.SubstituteVariable(Utility), new Constant<AnyGoal>(Generator.RenameArguments(s)));

            internal override AnyCall MakeCall(GoalAnalyzer ga)
            {
                if (ga.IsInstantiated(Arg))
                    throw new InstantiationException(Singleton, Arg);
                if (ga.IsInstantiated(Utility))
                    throw new InstantiationException(Singleton, Utility);

                var call = Generator.MakeCall(ga);

                if (!ga.IsInstantiated(Arg))
                    throw new InstantiationException($"Goal {Generator} does not bind the variable {Arg} in call {this}");
                if (!ga.IsInstantiated(Utility))
                    throw new InstantiationException($"Goal {Generator} does not bind the variable {Utility} in call {this}");

                return new Call(ga.Emit(Arg), ga.Emit(Utility), call);
            }
        }

        private class Call : AnyCall
        {
            private readonly MatchOperation<T1> Arg;
            private readonly MatchOperation<float> Utility;
            private readonly AnyCall Goal;
            private bool restart;

            public Call(MatchOperation<T1> arg, MatchOperation<float> utility, AnyCall call) : base(Singleton)
            {
                Arg = arg;
                Utility = utility;
                Goal = call;
            }

            public override IPattern ArgumentPattern => new Pattern<T1, float, AnyCall>(Arg, Utility,
                MatchOperation<AnyCall>.Constant(Goal));

            public override void Reset()
            {
                restart = true;
            }

            public override bool NextSolution()
            {
                if (!restart) return false;
                restart = false;
                Goal.Reset();
                var gotOne = false;
                var argCell = Arg.ValueCell;
                var utilityCell = Utility.ValueCell;
                var bestArg = argCell.Value;
                var bestUtil = utilityCell.Value;
                while (Goal.NextSolution())
                {
                    if (!gotOne || utilityCell.Value > bestUtil)
                    {
                        bestArg = argCell.Value;
                        bestUtil = utilityCell.Value;
                    }

                    gotOne = true;
                }

                if (gotOne)
                {
                    argCell.Value = bestArg;
                    utilityCell.Value = bestUtil;
                }

                return gotOne;
            }
        }
    }

    /// <summary>
    /// Implements negation of a goal
    /// </summary>
    public sealed class MaximalPrimitive<T1, T2> : AnyPredicate
    {
        public static MaximalPrimitive<T1, T2> Singleton = new MaximalPrimitive<T1, T2>();

        public MaximalPrimitive() : base("Maximal")
        {
        }

        public Goal this[Var<T1> arg1, Var<T2> arg2, Var<float> utility, AnyGoal g] => new Goal(arg1, arg2, utility, new Constant<AnyGoal>(g));

        public class Goal : AnyGoal
        {
            private readonly Var<T1> Arg1;
            private readonly Var<T2> Arg2;
            private readonly Var<float> Utility;
            private readonly AnyGoal Generator;

            public Goal(Var<T1> arg1, Var<T2> arg2, Var<float> utility, Constant<AnyGoal> g) : base(new AnyTerm[] { arg1, utility, g })
            {
                Arg1 = arg1;
                Arg2 = arg2;
                Utility = utility;
                Generator = g.Value;
            }

            public override AnyPredicate Predicate => Singleton;

            internal override AnyGoal RenameArguments(Substitution s)
                => new Goal((Var<T1>)s.SubstituteVariable(Arg1), (Var<T2>)s.SubstituteVariable(Arg2), (Var<float>)s.SubstituteVariable(Utility), new Constant<AnyGoal>(Generator.RenameArguments(s)));

            internal override AnyCall MakeCall(GoalAnalyzer ga)
            {
                if (ga.IsInstantiated(Arg1))
                    throw new InstantiationException(Singleton, Arg1);
                if (ga.IsInstantiated(Arg2))
                    throw new InstantiationException(Singleton, Arg2);
                if (ga.IsInstantiated(Utility))
                    throw new InstantiationException(Singleton, Utility);

                var call = Generator.MakeCall(ga);

                if (!ga.IsInstantiated(Arg1))
                    throw new InstantiationException($"Goal {Generator} does not bind the variable {Arg1} in call {this}");
                if (!ga.IsInstantiated(Arg2))
                    throw new InstantiationException($"Goal {Generator} does not bind the variable {Arg2} in call {this}");
                if (!ga.IsInstantiated(Utility))
                    throw new InstantiationException($"Goal {Generator} does not bind the variable {Utility} in call {this}");

                return new Call(ga.Emit(Arg1), ga.Emit(Arg2), ga.Emit(Utility), call);
            }
        }

        private class Call : AnyCall
        {
            private readonly MatchOperation<T1> Arg1;
            private readonly MatchOperation<T2> Arg2;
            private readonly MatchOperation<float> Utility;
            private readonly AnyCall Goal;
            private bool restart;

            public Call(MatchOperation<T1> arg1, MatchOperation<T2> arg2, MatchOperation<float> utility, AnyCall call) : base(Singleton)
            {
                Arg1 = arg1;
                Arg2 = arg2;
                Utility = utility;
                Goal = call;
            }

            public override IPattern ArgumentPattern => new Pattern<T1, T2, float, AnyCall>(Arg1, Arg2, Utility,
                MatchOperation<AnyCall>.Constant(Goal));

            public override void Reset()
            {
                restart = true;
            }

            public override bool NextSolution()
            {
                if (!restart) return false;
                restart = false;
                Goal.Reset();
                var gotOne = false;
                var arg1Cell = Arg1.ValueCell;
                var arg2Cell = Arg2.ValueCell;
                var utilityCell = Utility.ValueCell;
                var bestArg1 = arg1Cell.Value;
                var bestArg2 = arg2Cell.Value;
                var bestUtil = utilityCell.Value;
                while (Goal.NextSolution())
                {
                    if (!gotOne || utilityCell.Value > bestUtil)
                    {
                        bestArg1 = arg1Cell.Value;
                        bestArg2 = arg2Cell.Value;
                        bestUtil = utilityCell.Value;
                    }

                    gotOne = true;
                }

                if (gotOne)
                {
                    arg1Cell.Value = bestArg1;
                    arg2Cell.Value = bestArg2;
                    utilityCell.Value = bestUtil;
                }

                return gotOne;
            }
        }
    }
}
