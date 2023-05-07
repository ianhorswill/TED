using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using TED.Interpreter;
using TED.Primitives;
using Random = TED.Utilities.Random;

namespace TED
{
    /// <summary>
    /// Static class containing all the standard primitive predicates, such as Not.
    /// </summary>
    public static class Language
    {
        #region Primitives
        /// <summary>
        /// True if its argument is false
        /// </summary>
        public static readonly NotPrimitive Not = NotPrimitive.Singleton;

        /// <summary>
        /// True if all argument goals are also true
        /// </summary>
        public static readonly AndPrimitive And = AndPrimitive.Singleton;

        /// <summary>
        /// True if any argument goals are true
        /// </summary>
        public static readonly OrPrimitive Or = OrPrimitive.Singleton;

        /// <summary>
        /// Predicate that always fails
        /// </summary>
        public static readonly PrimitiveTest False = new PrimitiveTest("False", () => false);

        /// <summary>
        /// Predicate that always succeeds
        /// </summary>
        public static readonly PrimitiveTest True = new PrimitiveTest("True", () => true);

        /// <summary>
        /// Matches or stores the value of the functional expression to the variable.
        /// </summary>
        public static Goal Eval<T>(Var<T> v, FunctionalExpression<T> e) => EvalPrimitive<T>.Singleton[v, e];

        /// <summary>
        /// Prob(p) succeeds with a probability of p (p in the range [0,1])
        /// </summary>
        public static readonly PrimitiveTest<float> Prob = new PrimitiveTest<float>("Prob", Random.Roll, false);

        /// <summary>
        /// Breakpoint execution of a rule
        /// This drops the caller into the underlying C# debugger
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static Goal BreakPoint<T>(Term<T> arg) => new PrimitiveTest<T>(nameof(BreakPoint),
            argValue => {
                Debugger.Break();
                return true;
            })[arg];

        /// <summary>
        /// Matches output against a random element of the table.
        /// </summary>
        public static Goal RandomElement<T>(TablePredicate<T> predicate, Term<T> output) =>
            RandomElementPrimitive<T>.Singleton[predicate, output];

        /// <summary>
        /// True when element is an element of the collection
        /// </summary>
        /// <typeparam name="T">element/collection type</typeparam>
        /// <param name="element">Candidate element of the collection</param>
        /// <param name="collection">Collection to check</param>
        public static Goal In<T>(Term<T> element, Term<ICollection<T>> collection) =>
            InPrimitive<T>.Singleton[element, collection];

        //public static Goal In<T>(Term<T> element, TablePredicate<T> table) =>
        //    In(element, table.ToList());

        /// <summary>
        /// Matches output against a randomly chosen element of choices using a uniform distribution.
        /// </summary>
        public static Goal PickRandomly<T>(Term<T> output, params T[] choices) =>
            PickRandomlyPrimitive<T>.Singleton[output, choices];
        #endregion

        #region Aggregation functions
        /// <summary>
        /// The number of solutions to goal.
        /// </summary>
        public static AggregateFunctionCall<int> Count(Goal g)
            => new AggregateFunctionCall<int>(g, 1, 0, (a, b) => a + b);

        /// <summary>
        /// Aggregates value of variable from all the solutions to goal.
        /// </summary>
        /// <param name="v">Variable that appears int he goal</param>
        /// <param name="g">Goal to find the solutions of</param>
        /// <param name="initialValue">Value to return if g has no solutions</param>
        /// <param name="aggregator">Function mapping the current total and a new value of the variable to a new total</param>
        /// <returns>The aggregate value</returns>
        // ReSharper disable once UnusedMember.Global
        public static AggregateFunctionCall<T> Aggregate<T>(Var<T> v, Goal g, T initialValue,
            Func<T, T, T> aggregator)
            => new AggregateFunctionCall<T>(g, v, initialValue, aggregator);

        /// <summary>
        /// Return the sum of the specified variable from every solution to the goal
        /// </summary>
        public static AggregateFunctionCall<int> SumInt(Var<int> v, Goal g)
            => new AggregateFunctionCall<int>(g, v, 0, (a, b) => a + b);
        /// <summary>
        /// Return the sum of the specified variable from every solution to the goal
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static AggregateFunctionCall<float> SumFloat(Var<float> v, Goal g)
            => new AggregateFunctionCall<float>(g, v, 0, (a, b) => a + b);
        #endregion

        #region Optimization predicates
        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        /// <typeparam name="TArg">Type of the arg variable</typeparam>
        /// <typeparam name="TUtility">Type of the utility variable</typeparam>
        /// <param name="arg">Variable from goal to report the value of for the maximal solution</param>
        /// <param name="objective">Variable to maximize across solutions to goal</param>
        /// <param name="goal">Goal to find the maximal solution of</param>
        public static Goal Maximal<TArg, TUtility>(Var<TArg> arg, Var<TUtility> objective, Goal goal) where TUtility : IComparable<TUtility> =>
            MaximalPrimitive<TArg, TUtility>.Maximal[arg, objective, goal];

        #region Single arg Maximal wrappers for numerical utilities
        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        public static Goal Maximal<TArg>(Var<TArg> arg, Var<float> objective, Goal goal) => Maximal<TArg, float>(arg, objective, goal);
        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        public static Goal Maximal<TArg>(Var<TArg> arg, Var<long> objective, Goal goal) => Maximal<TArg, long>(arg, objective, goal);
        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        public static Goal Maximal<TArg>(Var<TArg> arg, Var<uint> objective, Goal goal) => Maximal<TArg, uint>(arg, objective, goal);
        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        public static Goal Maximal<TArg>(Var<TArg> arg, Var<int> objective, Goal goal) => Maximal<TArg, int>(arg, objective, goal);
        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        public static Goal Maximal<TArg>(Var<TArg> arg, Var<ushort> objective, Goal goal) => Maximal<TArg, ushort>(arg, objective, goal);
        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        public static Goal Maximal<TArg>(Var<TArg> arg, Var<short> objective, Goal goal) => Maximal<TArg, short>(arg, objective, goal);
        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        public static Goal Maximal<TArg>(Var<TArg> arg, Var<byte> objective, Goal goal) => Maximal<TArg, byte>(arg, objective, goal);
        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        public static Goal Maximal<TArg>(Var<TArg> arg, Var<sbyte> objective, Goal goal) => Maximal<TArg, sbyte>(arg, objective, goal);
        #endregion

        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        /// <typeparam name="TArg">Type of the arg variable</typeparam>
        /// <typeparam name="TUtility">Type of the utility variable</typeparam>
        /// <param name="arg">Variable from goal to report the value of for the minimal solution</param>
        /// <param name="objective">Variable to minimize across solutions to goal</param>
        /// <param name="goal">Goal to find the minimal solution of</param>
        public static Goal Minimal<TArg, TUtility>(Var<TArg> arg, Var<TUtility> objective, Goal goal) where TUtility : IComparable<TUtility> =>
            MaximalPrimitive<TArg, TUtility>.Minimal[arg, objective, goal];

        #region Single arg Minimal wrappers for numerical utilities
        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        public static Goal Minimal<TArg>(Var<TArg> arg, Var<float> objective, Goal goal) => Minimal<TArg, float>(arg, objective, goal);
        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        public static Goal Minimal<TArg>(Var<TArg> arg, Var<long> objective, Goal goal) => Minimal<TArg, long>(arg, objective, goal);
        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        public static Goal Minimal<TArg>(Var<TArg> arg, Var<uint> objective, Goal goal) => Minimal<TArg, uint>(arg, objective, goal);
        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        public static Goal Minimal<TArg>(Var<TArg> arg, Var<int> objective, Goal goal) => Minimal<TArg, int>(arg, objective, goal);
        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        public static Goal Minimal<TArg>(Var<TArg> arg, Var<ushort> objective, Goal goal) => Minimal<TArg, ushort>(arg, objective, goal);
        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        public static Goal Minimal<TArg>(Var<TArg> arg, Var<short> objective, Goal goal) => Minimal<TArg, short>(arg, objective, goal);
        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        public static Goal Minimal<TArg>(Var<TArg> arg, Var<byte> objective, Goal goal) => Minimal<TArg, byte>(arg, objective, goal);
        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        public static Goal Minimal<TArg>(Var<TArg> arg, Var<sbyte> objective, Goal goal) => Minimal<TArg, sbyte>(arg, objective, goal);
        #endregion

        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        /// <typeparam name="TArg1">Type of the first variable to report back</typeparam>
        /// <typeparam name="TArg2">Type of the second variable to report back</typeparam>
        /// <typeparam name="TUtility">Type of the utility variable</typeparam>
        /// <param name="args">Variables from Goal to report the values of for the maximal solution</param>
        /// <param name="objective">Variable to maximize across solutions to goal</param>
        /// <param name="goal">Goal to find the maximal solution of</param>
        public static Goal Maximal<TArg1, TArg2, TUtility>((Var<TArg1>, Var<TArg2>)args, Var<TUtility> objective, Goal goal) where TUtility : IComparable<TUtility> =>
            MaximalPrimitive<TArg1, TArg2, TUtility>.Maximal[args.Item1, args.Item2, objective, goal];

        #region Two arg Maximal wrappers for numerical utilities
        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        public static Goal Maximal<TArg1, TArg2>((Var<TArg1>, Var<TArg2>) args, Var<float> objective, Goal goal) => Maximal<TArg1, TArg2, float>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        public static Goal Maximal<TArg1, TArg2>((Var<TArg1>, Var<TArg2>) args, Var<long> objective, Goal goal) =>  Maximal<TArg1, TArg2, long>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        public static Goal Maximal<TArg1, TArg2>((Var<TArg1>, Var<TArg2>) args, Var<uint> objective, Goal goal) => Maximal<TArg1, TArg2, uint>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        public static Goal Maximal<TArg1, TArg2>((Var<TArg1>, Var<TArg2>) args, Var<int> objective, Goal goal) => Maximal<TArg1, TArg2, int>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        public static Goal Maximal<TArg1, TArg2>((Var<TArg1>, Var<TArg2>) args, Var<ushort> objective, Goal goal) => Maximal<TArg1, TArg2, ushort>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        public static Goal Maximal<TArg1, TArg2>((Var<TArg1>, Var<TArg2>) args, Var<short> objective, Goal goal) => Maximal<TArg1, TArg2, short>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        public static Goal Maximal<TArg1, TArg2>((Var<TArg1>, Var<TArg2>) args, Var<byte> objective, Goal goal) => Maximal<TArg1, TArg2, byte>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        public static Goal Maximal<TArg1, TArg2>((Var<TArg1>, Var<TArg2>) args, Var<sbyte> objective, Goal goal) => Maximal<TArg1, TArg2, sbyte>(args, objective, goal);
        #endregion

        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        /// <typeparam name="TArg1">Type of the first variable to report back</typeparam>
        /// <typeparam name="TArg2">Type of the second variable to report back</typeparam>
        /// <typeparam name="TUtility">Type of the utility variable</typeparam>
        /// <param name="args">Variables from Goal to report the values of for the minimal solution</param>
        /// <param name="objective">Variable to minimize across solutions to goal</param>
        /// <param name="goal">Goal to find the minimal solution of</param>
        public static Goal Minimal<TArg1, TArg2, TUtility>((Var<TArg1>, Var<TArg2>)args, Var<TUtility> objective, Goal goal) where TUtility : IComparable<TUtility> =>
            MaximalPrimitive<TArg1, TArg2, TUtility>.Minimal[args.Item1, args.Item2, objective, goal];

        #region Two arg Minimal wrappers for numerical utilities
        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        public static Goal Minimal<TArg1, TArg2>((Var<TArg1>, Var<TArg2>) args, Var<float> objective, Goal goal) => Minimal<TArg1, TArg2, float>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        public static Goal Minimal<TArg1, TArg2>((Var<TArg1>, Var<TArg2>) args, Var<long> objective, Goal goal) => Minimal<TArg1, TArg2, long>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        public static Goal Minimal<TArg1, TArg2>((Var<TArg1>, Var<TArg2>) args, Var<uint> objective, Goal goal) => Minimal<TArg1, TArg2, uint>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        public static Goal Minimal<TArg1, TArg2>((Var<TArg1>, Var<TArg2>) args, Var<int> objective, Goal goal) => Minimal<TArg1, TArg2, int>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        public static Goal Minimal<TArg1, TArg2>((Var<TArg1>, Var<TArg2>) args, Var<ushort> objective, Goal goal) => Minimal<TArg1, TArg2, ushort>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        public static Goal Minimal<TArg1, TArg2>((Var<TArg1>, Var<TArg2>) args, Var<short> objective, Goal goal) => Minimal<TArg1, TArg2, short>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        public static Goal Minimal<TArg1, TArg2>((Var<TArg1>, Var<TArg2>) args, Var<byte> objective, Goal goal) => Minimal<TArg1, TArg2, byte>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        public static Goal Minimal<TArg1, TArg2>((Var<TArg1>, Var<TArg2>) args, Var<sbyte> objective, Goal goal) => Minimal<TArg1, TArg2, sbyte>(args, objective, goal);
        #endregion

        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        /// <typeparam name="TArg1">Type of the first variable to report back</typeparam>
        /// <typeparam name="TArg2">Type of the second variable to report back</typeparam>
        /// <typeparam name="TArg3">Type of the third variable to report back</typeparam>
        /// <typeparam name="TUtility">Type of the utility variable</typeparam>
        /// <param name="args">Variables from Goal to report the values of for the minimal solution</param>
        /// <param name="objective">Variable to maximize across solutions to goal</param>
        /// <param name="goal">Goal to find the maximal solution of</param>
        public static Goal Maximal<TArg1, TArg2, TArg3, TUtility>((Var<TArg1>, Var<TArg2>, Var<TArg3>)args, Var<TUtility> objective, Goal goal) where TUtility : IComparable<TUtility> =>
            MaximalPrimitive<TArg1, TArg2, TArg3, TUtility>.Maximal[args.Item1, args.Item2, args.Item3, objective, goal];

        #region Three arg Maximal wrappers for numerical utilities
        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        public static Goal Maximal<TArg1, TArg2, TArg3>((Var<TArg1>, Var<TArg2>, Var<TArg3>) args, Var<float> objective, Goal goal) => Maximal<TArg1, TArg2, TArg3, float>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        public static Goal Maximal<TArg1, TArg2, TArg3>((Var<TArg1>, Var<TArg2>, Var<TArg3>) args, Var<long> objective, Goal goal) => Maximal<TArg1, TArg2, TArg3, long>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        public static Goal Maximal<TArg1, TArg2, TArg3>((Var<TArg1>, Var<TArg2>, Var<TArg3>) args, Var<uint> objective, Goal goal) => Maximal<TArg1, TArg2, TArg3, uint>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        public static Goal Maximal<TArg1, TArg2, TArg3>((Var<TArg1>, Var<TArg2>, Var<TArg3>) args, Var<int> objective, Goal goal) => Maximal<TArg1, TArg2, TArg3, int>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        public static Goal Maximal<TArg1, TArg2, TArg3>((Var<TArg1>, Var<TArg2>, Var<TArg3>) args, Var<ushort> objective, Goal goal) => Maximal<TArg1, TArg2, TArg3, ushort>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        public static Goal Maximal<TArg1, TArg2, TArg3>((Var<TArg1>, Var<TArg2>, Var<TArg3>) args, Var<short> objective, Goal goal) => Maximal<TArg1, TArg2, TArg3, short>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        public static Goal Maximal<TArg1, TArg2, TArg3>((Var<TArg1>, Var<TArg2>, Var<TArg3>) args, Var<byte> objective, Goal goal) => Maximal<TArg1, TArg2, TArg3, byte>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with maximal value of objective
        /// </summary>
        public static Goal Maximal<TArg1, TArg2, TArg3>((Var<TArg1>, Var<TArg2>, Var<TArg3>) args, Var<sbyte> objective, Goal goal) => Maximal<TArg1, TArg2, TArg3, sbyte>(args, objective, goal);
        #endregion

        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        /// <typeparam name="TArg1">Type of the first variable to report back</typeparam>
        /// <typeparam name="TArg2">Type of the second variable to report back</typeparam>
        /// <typeparam name="TArg3">Type of the third variable to report back</typeparam>
        /// <typeparam name="TUtility">Type of the utility variable</typeparam>
        /// <param name="args">Variables from Goal to report the values of for the minimal solution</param>
        /// <param name="objective">Variable to minimize across solutions to goal</param>
        /// <param name="goal">Goal to find the minimal solution of</param>
        // ReSharper disable once UnusedMember.Global
        public static Goal Minimal<TArg1, TArg2, TArg3, TUtility>((Var<TArg1>, Var<TArg2>, Var<TArg3>)args, Var<TUtility> objective, Goal goal) where TUtility : IComparable<TUtility> =>
            MaximalPrimitive<TArg1, TArg2, TArg3, TUtility>.Minimal[args.Item1, args.Item2, args.Item3, objective, goal];

        #region Three arg Minimal wrappers for numerical utilities
        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        public static Goal Minimal<TArg1, TArg2, TArg3>((Var<TArg1>, Var<TArg2>, Var<TArg3>) args, Var<float> objective, Goal goal) => Minimal<TArg1, TArg2, TArg3, float>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        public static Goal Minimal<TArg1, TArg2, TArg3>((Var<TArg1>, Var<TArg2>, Var<TArg3>) args, Var<long> objective, Goal goal) => Minimal<TArg1, TArg2, TArg3, long>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        public static Goal Minimal<TArg1, TArg2, TArg3>((Var<TArg1>, Var<TArg2>, Var<TArg3>) args, Var<uint> objective, Goal goal) => Minimal<TArg1, TArg2, TArg3, uint>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        public static Goal Minimal<TArg1, TArg2, TArg3>((Var<TArg1>, Var<TArg2>, Var<TArg3>) args, Var<int> objective, Goal goal) => Minimal<TArg1, TArg2, TArg3, int>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        public static Goal Minimal<TArg1, TArg2, TArg3>((Var<TArg1>, Var<TArg2>, Var<TArg3>) args, Var<ushort> objective, Goal goal) => Minimal<TArg1, TArg2, TArg3, ushort>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        public static Goal Minimal<TArg1, TArg2, TArg3>((Var<TArg1>, Var<TArg2>, Var<TArg3>) args, Var<short> objective, Goal goal) => Minimal<TArg1, TArg2, TArg3, short>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        public static Goal Minimal<TArg1, TArg2, TArg3>((Var<TArg1>, Var<TArg2>, Var<TArg3>) args, Var<byte> objective, Goal goal) => Minimal<TArg1, TArg2, TArg3, byte>(args, objective, goal);
        /// <summary>
        /// Find the solution to goal with minimal value of objective
        /// </summary>
        public static Goal Minimal<TArg1, TArg2, TArg3>((Var<TArg1>, Var<TArg2>, Var<TArg3>) args, Var<sbyte> objective, Goal goal) => Minimal<TArg1, TArg2, TArg3, sbyte>(args, objective, goal);
        #endregion

        #endregion

        #region Math functions
        /// <summary>
        /// The largest of two arguments
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static readonly Function<int, int, int> MaxInt = new Function<int, int, int>("Max", Math.Max);
        /// <summary>
        /// The largest of two arguments
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static readonly Function<float, float, float> MaxFloat = new Function<float, float, float>("Max", Math.Max);
        
        /// <summary>
        /// The smaller of two arguments
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static readonly Function<int, int, int> MinInt = new Function<int, int, int>("Min", Math.Min);
        /// <summary>
        /// The smaller of two arguments
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static readonly Function<float, float, float> MinFloat = new Function<float, float, float>("Min", Math.Min);

        /// <summary>
        /// The absolute value of a number
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static readonly Function<int, int> AbsInt = new Function<int, int>("Abs", Math.Abs);
        /// <summary>
        /// The absolute value of a number
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static readonly Function<float, float> AbsFloat = new Function<float, float>("Abs", MathF.Abs);

        /// <summary>
        /// Sign number (1, 0, or -1) of x
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static readonly Function<int, int> SignInt = new Function<int, int>("Sign", Math.Sign);
        /// <summary>
        /// Sign number (1, 0, or -1) of x
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static readonly Function<float, int> SignFloat = new Function<float, int>("Sign", MathF.Sign);


        /// <summary>
        /// The square root of a float
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static readonly Function<float, float> Sqrt = new Function<float, float>("Sqrt", MathF.Sqrt);

        /// <summary>
        /// The natural log of a float
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static readonly Function<float, float> Log = new Function<float, float>("Log", MathF.Log);
        
        /// <summary>
        /// e^x
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static readonly Function<float, float> Exp = new Function<float, float>("Exp", MathF.Exp);

        /// <summary>
        /// x^y
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static readonly Function<float, float, float> Pow = new Function<float, float, float>("Pow", MathF.Pow);

        /// <summary>
        /// Sin x
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static readonly Function<float, float> Sin = new Function<float, float>("Sin", MathF.Sin);
        
        /// <summary>
        /// Cos x
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static readonly Function<float, float> Cos = new Function<float, float>("Cos", MathF.Cos);
        
        /// <summary>
        /// Tan x
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static readonly Function<float, float> Tan = new Function<float, float>("Tan", MathF.Tan);

        /// <summary>
        /// Arc Sin x
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static readonly Function<float, float> Asin = new Function<float, float>("Asin", MathF.Asin);
        
        /// <summary>
        /// Arc Cos x
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static readonly Function<float, float> ACos = new Function<float, float>("Acos", MathF.Acos);
        
        /// <summary>
        /// Arc Tan x
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static readonly Function<float, float> Atan = new Function<float, float>("Atan", MathF.Atan);
        
        /// <summary>
        /// Arc Tan x/y
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static readonly Function<float, float, float> Atan2 = new Function<float, float, float>("Atan2", MathF.Atan2);

        /// <summary>
        /// Returns (int)x
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static readonly Function<float, int> Int = new Function<float, int>("Int", x => (int)x);
        
        /// <summary>
        /// Returns (float)x
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static readonly Function<int, float> Float = new Function<int, float>("Float", x => x);

        /// <summary>
        /// Smallest integer larger than x
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static readonly Function<float, int> Ceiling = new Function<float, int>("Ceiling", x => (int)MathF.Ceiling(x));
        
        /// <summary>
        /// Largest integer less than x
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static readonly Function<float, int> Floor = new Function<float, int>("Floor", x => (int)MathF.Floor(x));
        
        /// <summary>
        /// The nearest integer to x
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static readonly Function<float, int> Round = new Function<float, int>("Round", x => (int)MathF.Round(x));


        #endregion

        /// <summary>
        /// Coerces a C# constant to a Constant Term for times when C#'s type inference isn't smart enough to figure it out.
        /// </summary>
        public static Constant<T> Constant<T>(T value) => new Constant<T>(value);

        #region Predicate declaration sugar
        /// <summary>
        /// Make a function that returns predicate.Length
        /// </summary>
        /// <param name="name">Name of the Function, for debugging purposes</param>
        /// <param name="predicate">Table to get the Length of</param>
        /// <returns></returns>
        public static Function<uint> Length(string name, TablePredicate predicate) => Function(name, () => predicate.Length);
        /// <summary>
        /// Make a function that returns predicate.Length as an int
        /// </summary>
        /// <param name="name">Name of the Function, for debugging purposes</param>
        /// <param name="predicate">Table to get the Length of</param>
        /// <returns></returns>
        public static Function<int> IntLength(string name, TablePredicate predicate) => Function(name, () => (int)predicate.Length);

        /// <summary>
        /// Make a new table predicate
        /// </summary>
        /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
        /// <param name="name">Name for the predicate, for debugging purposes</param>
        /// <param name="arg">Variable to be used as the default first argument.</param>
        public static TablePredicate<T1> Predicate<T1>(string name, IColumnSpec<T1> arg) 
            => new TablePredicate<T1>(name, arg);
        
        /// <summary>
        /// Make a new table predicate
        /// </summary>
        /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
        /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
        /// <param name="name">Name for the predicate, for debugging purposes</param>
        /// <param name="arg1">Variable to be used as the default first argument.</param>
        /// <param name="arg2">Variable to be used as the default second argument.</param>
        public static TablePredicate<T1,T2> Predicate<T1,T2>(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2) 
            => new TablePredicate<T1,T2>(name, arg1, arg2);
        
        /// <summary>
        /// Make a new table predicate
        /// </summary>
        /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
        /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
        /// <typeparam name="T3">Type of the third argument to the predicate</typeparam>
        /// <param name="name">Name for the predicate, for debugging purposes</param>
        /// <param name="arg1">Variable to be used as the default first argument.</param>
        /// <param name="arg2">Variable to be used as the default second argument.</param>
        /// <param name="arg3">Variable to be used as the default third argument.</param>
        public static TablePredicate<T1,T2,T3> Predicate<T1,T2,T3>(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3) 
            => new TablePredicate<T1,T2,T3>(name, arg1, arg2, arg3);
        
        /// <summary>
        /// Make a new table predicate
        /// </summary>
        /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
        /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
        /// <typeparam name="T3">Type of the third argument to the predicate</typeparam>
        /// <typeparam name="T4">Type of the fourth argument to the predicate</typeparam>
        /// <param name="name">Name for the predicate, for debugging purposes</param>
        /// <param name="arg1">Variable to be used as the default first argument.</param>
        /// <param name="arg2">Variable to be used as the default second argument.</param>
        /// <param name="arg3">Variable to be used as the default third argument.</param>
        /// <param name="arg4">Variable to be used as the default fourth argument.</param>
        public static TablePredicate<T1,T2,T3,T4> Predicate<T1,T2,T3,T4>(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4) 
            => new TablePredicate<T1,T2,T3,T4>(name, arg1, arg2, arg3, arg4);
        
        /// <summary>
        /// Make a new table predicate
        /// </summary>
        /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
        /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
        /// <typeparam name="T3">Type of the third argument to the predicate</typeparam>
        /// <typeparam name="T4">Type of the fourth argument to the predicate</typeparam>
        /// <typeparam name="T5">Type of the fifth argument to the predicate</typeparam>
        /// <param name="name">Name for the predicate, for debugging purposes</param>
        /// <param name="arg1">Variable to be used as the default first argument.</param>
        /// <param name="arg2">Variable to be used as the default second argument.</param>
        /// <param name="arg3">Variable to be used as the default third argument.</param>
        /// <param name="arg4">Variable to be used as the default fourth argument.</param>
        /// <param name="arg5">Variable to be used as the default fifth argument.</param>
        public static TablePredicate<T1,T2,T3,T4,T5> Predicate<T1,T2,T3,T4,T5>(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5) 
            => new TablePredicate<T1,T2,T3,T4,T5>(name, arg1, arg2, arg3, arg4, arg5);
        
        /// <summary>
        /// Make a new table predicate
        /// </summary>
        /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
        /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
        /// <typeparam name="T3">Type of the third argument to the predicate</typeparam>
        /// <typeparam name="T4">Type of the fourth argument to the predicate</typeparam>
        /// <typeparam name="T5">Type of the fifth argument to the predicate</typeparam>
        /// <typeparam name="T6">Type of the sixth argument to the predicate</typeparam>
        /// <param name="name">Name for the predicate, for debugging purposes</param>
        /// <param name="arg1">Variable to be used as the default first argument.</param>
        /// <param name="arg2">Variable to be used as the default second argument.</param>
        /// <param name="arg3">Variable to be used as the default third argument.</param>
        /// <param name="arg4">Variable to be used as the default fourth argument.</param>
        /// <param name="arg5">Variable to be used as the default fifth argument.</param>
        /// <param name="arg6">Variable to be used as the default sixth argument.</param>
        public static TablePredicate<T1,T2,T3,T4,T5,T6> Predicate<T1,T2,T3,T4,T5,T6>(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6) 
            => new TablePredicate<T1,T2,T3,T4,T5,T6>(name, arg1, arg2, arg3, arg4, arg5,arg6);

        /// <summary>
        /// Make a new table predicate
        /// </summary>
        /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
        /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
        /// <typeparam name="T3">Type of the third argument to the predicate</typeparam>
        /// <typeparam name="T4">Type of the fourth argument to the predicate</typeparam>
        /// <typeparam name="T5">Type of the fifth argument to the predicate</typeparam>
        /// <typeparam name="T6">Type of the sixth argument to the predicate</typeparam>
        /// <typeparam name="T7">Type of the seventh argument to the predicate</typeparam>
        /// <param name="name">Name for the predicate, for debugging purposes</param>
        /// <param name="arg1">Variable to be used as the default first argument.</param>
        /// <param name="arg2">Variable to be used as the default second argument.</param>
        /// <param name="arg3">Variable to be used as the default third argument.</param>
        /// <param name="arg4">Variable to be used as the default fourth argument.</param>
        /// <param name="arg5">Variable to be used as the default fifth argument.</param>
        /// <param name="arg6">Variable to be used as the default sixth argument.</param>
        /// <param name="arg7">Variable to be used as the default seventh argument.</param>
        public static TablePredicate<T1, T2, T3, T4, T5, T6, T7> Predicate<T1, T2, T3, T4, T5, T6, T7>(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7)
            => new TablePredicate<T1, T2, T3, T4, T5, T6, T7>(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7);

        /// <summary>
        /// Make a new table predicate
        /// </summary>
        /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
        /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
        /// <typeparam name="T3">Type of the third argument to the predicate</typeparam>
        /// <typeparam name="T4">Type of the fourth argument to the predicate</typeparam>
        /// <typeparam name="T5">Type of the fifth argument to the predicate</typeparam>
        /// <typeparam name="T6">Type of the sixth argument to the predicate</typeparam>
        /// <typeparam name="T7">Type of the seventh argument to the predicate</typeparam>
        /// <typeparam name="T8">Type of the seventh argument to the predicate</typeparam>
        /// <param name="name">Name for the predicate, for debugging purposes</param>
        /// <param name="arg1">Variable to be used as the default first argument.</param>
        /// <param name="arg2">Variable to be used as the default second argument.</param>
        /// <param name="arg3">Variable to be used as the default third argument.</param>
        /// <param name="arg4">Variable to be used as the default fourth argument.</param>
        /// <param name="arg5">Variable to be used as the default fifth argument.</param>
        /// <param name="arg6">Variable to be used as the default sixth argument.</param>
        /// <param name="arg7">Variable to be used as the default seventh argument.</param>
        /// <param name="arg8">Variable to be used as the default seventh argument.</param>
        public static TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> Predicate<T1, T2, T3, T4, T5, T6, T7, T8>(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7, IColumnSpec<T8> arg8)
            => new TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8>(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);

        /// <summary>
        /// Make a new TablePredicate from a row generator, and pre-populate it with rows from the generator.
        /// </summary>
        public static TablePredicate<T1> Predicate<T1>(string name, IEnumerable<T1> generator, string arg1 = "arg1")
        {
            var p = new TablePredicate<T1>(name, (Var<T1>)arg1);
            p.AddRows(generator);
            return p;
        }

        /// <summary>
        /// Make a new TablePredicate from a row generator, and pre-populate it with rows from the generator.
        /// </summary>
        public static TablePredicate<T1,T2> Predicate<T1,T2>(string name, IEnumerable<(T1,T2)> generator, string arg1 = "arg1", string arg2 = "arg2")
        {
            var p = new TablePredicate<T1,T2>(name, (Var<T1>)arg1, (Var<T2>)arg2);
            p.AddRows(generator);
            return p;
        }

        /// <summary>
        /// Make a new TablePredicate from a row generator, and pre-populate it with rows from the generator.
        /// </summary>
        public static TablePredicate<T1,T2,T3> Predicate<T1,T2, T3>(string name, IEnumerable<(T1,T2,T3)> generator, string arg1 = "arg1", string arg2 = "arg2", string arg3="arg3")
        {
            var p = new TablePredicate<T1,T2,T3>(name, (Var<T1>)arg1, (Var<T2>)arg2, (Var<T3>)arg3);
            p.AddRows(generator);
            return p;
        }

        /// <summary>
        /// Make a new TablePredicate from a row generator, and pre-populate it with rows from the generator.
        /// </summary>
        public static TablePredicate<T1,T2,T3,T4> Predicate<T1,T2, T3, T4>(string name, IEnumerable<(T1,T2,T3,T4)> generator, string arg1 = "arg1", string arg2 = "arg2", string arg3="arg3", string arg4="arg4")
        {
            var p = new TablePredicate<T1,T2,T3,T4>(name, (Var<T1>)arg1, (Var<T2>)arg2, (Var<T3>)arg3, (Var<T4>)arg4);
            p.AddRows(generator);
            return p;
        }

        /// <summary>
        /// Make a new TablePredicate from a row generator, and pre-populate it with rows from the generator.
        /// </summary>
        public static TablePredicate<T1,T2,T3,T4,T5> Predicate<T1,T2, T3, T4, T5>(string name, IEnumerable<(T1,T2,T3,T4,T5)> generator, string arg1 = "arg1", string arg2 = "arg2", string arg3="arg3", string arg4="arg4", string arg5="arg5")
        {
            var p = new TablePredicate<T1,T2,T3,T4,T5>(name, (Var<T1>)arg1, (Var<T2>)arg2, (Var<T3>)arg3, (Var<T4>)arg4, (Var<T5>)arg5);
            p.AddRows(generator);
            return p;
        }

        /// <summary>
        /// Make a new TablePredicate from a row generator, and pre-populate it with rows from the generator.
        /// </summary>
        public static TablePredicate<T1,T2,T3,T4,T5,T6> Predicate<T1,T2, T3, T4, T5, T6>(string name, IEnumerable<(T1,T2,T3,T4,T5,T6)> generator, string arg1 = "arg1", string arg2 = "arg2", string arg3="arg3", string arg4="arg4", string arg5="arg5", string arg6="arg6")
        {
            var p = new TablePredicate<T1,T2,T3,T4,T5,T6>(name, (Var<T1>)arg1, (Var<T2>)arg2, (Var<T3>)arg3, (Var<T4>)arg4, (Var<T5>)arg5, (Var<T6>)arg6);
            p.AddRows(generator);
            return p;
        }

        /// <summary>
        /// Make a new TablePredicate from a row generator, and pre-populate it with rows from the generator.
        /// </summary>
        public static TablePredicate<T1, T2, T3, T4, T5, T6, T7> Predicate<T1, T2, T3, T4, T5, T6, T7>(string name, IEnumerable<(T1, T2, T3, T4, T5, T6, T7)> generator, string arg1 = "arg1", string arg2 = "arg2", string arg3 = "arg3", string arg4 = "arg4", string arg5 = "arg5", string arg6 = "arg6", string arg7 = "arg7") {
            var p = new TablePredicate<T1, T2, T3, T4, T5, T6, T7>(name, (Var<T1>)arg1, (Var<T2>)arg2, (Var<T3>)arg3, (Var<T4>)arg4, (Var<T5>)arg5, (Var<T6>)arg6, (Var<T7>)arg7);
            p.AddRows(generator);
            return p;
        }

        /// <summary>
        /// Make a new TablePredicate from a row generator, and pre-populate it with rows from the generator.
        /// </summary>
        public static TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> Predicate<T1, T2, T3, T4, T5, T6, T7, T8>(string name, IEnumerable<(T1, T2, T3, T4, T5, T6, T7, T8)> generator, string arg1 = "arg1", string arg2 = "arg2", string arg3 = "arg3", string arg4 = "arg4", string arg5 = "arg5", string arg6 = "arg6", string arg7 = "arg7", string arg8 = "arg8") {
            var p = new TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8>(name, (Var<T1>)arg1, (Var<T2>)arg2, (Var<T3>)arg3, (Var<T4>)arg4, (Var<T5>)arg5, (Var<T6>)arg6, (Var<T7>)arg7, (Var<T8>)arg8);
            p.AddRows(generator);
            return p;
        }

        /// <summary>
        /// Make a new TablePredicate from a row generator, and pre-populate it with rows from the generator.
        /// </summary>
        public static TablePredicate<T1> Predicate<T1>(string name, IEnumerable<T1> generator, IColumnSpec<T1> arg1)
        {
            var p = new TablePredicate<T1>(name, arg1);
            p.AddRows(generator);
            return p;
        }

        /// <summary>
        /// Make a new TablePredicate from a row generator, and pre-populate it with rows from the generator.
        /// </summary>
        public static TablePredicate<T1,T2> Predicate<T1,T2>(string name, IEnumerable<(T1,T2)> generator, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2)
        {
            var p = new TablePredicate<T1,T2>(name, arg1, arg2);
            p.AddRows(generator);
            return p;
        }

        /// <summary>
        /// Make a new TablePredicate from a row generator, and pre-populate it with rows from the generator.
        /// </summary>
        public static TablePredicate<T1,T2,T3> Predicate<T1,T2, T3>(string name, IEnumerable<(T1,T2,T3)> generator, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3)
        {
            var p = new TablePredicate<T1,T2,T3>(name, arg1, arg2, arg3);
            p.AddRows(generator);
            return p;
        }

        /// <summary>
        /// Make a new TablePredicate from a row generator, and pre-populate it with rows from the generator.
        /// </summary>
        public static TablePredicate<T1,T2,T3,T4> Predicate<T1,T2, T3, T4>(string name, IEnumerable<(T1,T2,T3,T4)> generator, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4)
        {
            var p = new TablePredicate<T1,T2,T3,T4>(name, arg1, arg2, arg3, arg4);
            p.AddRows(generator);
            return p;
        }

        /// <summary>
        /// Make a new TablePredicate from a row generator, and pre-populate it with rows from the generator.
        /// </summary>
        public static TablePredicate<T1,T2,T3,T4,T5> Predicate<T1,T2, T3, T4, T5>(string name, IEnumerable<(T1,T2,T3,T4,T5)> generator, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5)
        {
            var p = new TablePredicate<T1,T2,T3,T4,T5>(name, arg1, arg2, arg3, arg4, arg5);
            p.AddRows(generator);
            return p;
        }

        /// <summary>
        /// Make a new TablePredicate from a row generator, and pre-populate it with rows from the generator.
        /// </summary>
        public static TablePredicate<T1,T2,T3,T4,T5,T6> Predicate<T1,T2, T3, T4, T5, T6>(string name, IEnumerable<(T1,T2,T3,T4,T5,T6)> generator, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6)
        {
            var p = new TablePredicate<T1,T2,T3,T4,T5,T6>(name, arg1, arg2, arg3, arg4, arg5, arg6);
            p.AddRows(generator);
            return p;
        }

        /// <summary>
        /// Make a new TablePredicate from a row generator, and pre-populate it with rows from the generator.
        /// </summary>
        public static TablePredicate<T1, T2, T3, T4, T5, T6, T7> Predicate<T1, T2, T3, T4, T5, T6, T7>(string name, IEnumerable<(T1, T2, T3, T4, T5, T6, T7)> generator, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7) {
            var p = new TablePredicate<T1, T2, T3, T4, T5, T6, T7>(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            p.AddRows(generator);
            return p;
        }

        /// <summary>
        /// Make a new TablePredicate from a row generator, and pre-populate it with rows from the generator.
        /// </summary>
        public static TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> Predicate<T1, T2, T3, T4, T5, T6, T7, T8>(string name, IEnumerable<(T1, T2, T3, T4, T5, T6, T7, T8)> generator, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7, IColumnSpec<T8> arg8) {
            var p = new TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8>(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            p.AddRows(generator);
            return p;
        }

        /// <summary>
        /// Make a new table predicate, loading in rows from the specified csv
        /// </summary>
        /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
        /// <param name="name">Name for the predicate, for debugging purposes</param>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">Variable to be used as the default first argument.</param>
        // ReSharper disable once UnusedMember.Global
        public static TablePredicate<T1> FromCsv<T1>(string name, string path, IColumnSpec<T1> arg1)
            => TablePredicate<T1>.FromCsv(name, path, arg1);

        /// <summary>
        /// Make a new table predicate, loading in rows from the specified csv
        /// </summary>
        /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
        /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
        /// <param name="name">Name for the predicate, for debugging purposes</param>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">Variable to be used as the default first argument.</param>
        /// <param name="arg2">Variable to be used as the default second argument.</param>
        // ReSharper disable once UnusedMember.Global
        public static TablePredicate<T1, T2> FromCsv<T1, T2>(string name, string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2)
            => TablePredicate<T1, T2>.FromCsv(name, path, arg1, arg2);

        /// <summary>
        /// Make a new table predicate, loading in rows from the specified csv
        /// </summary>
        /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
        /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
        /// <typeparam name="T3">Type of the third argument to the predicate</typeparam>
        /// <param name="name">Name for the predicate, for debugging purposes</param>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">Variable to be used as the default first argument.</param>
        /// <param name="arg2">Variable to be used as the default second argument.</param>
        /// <param name="arg3">Variable to be used as the default third argument.</param>
        // ReSharper disable once UnusedMember.Global
        public static TablePredicate<T1, T2, T3> FromCsv<T1, T2, T3>(string name, string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3)
            => TablePredicate<T1, T2, T3>.FromCsv(name, path, arg1, arg2, arg3);
        
        /// <summary>
        /// Make a new table predicate, loading in rows from the specified csv
        /// </summary>
        /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
        /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
        /// <typeparam name="T3">Type of the third argument to the predicate</typeparam>
        /// <typeparam name="T4">Type of the fourth argument to the predicate</typeparam>
        /// <param name="name">Name for the predicate, for debugging purposes</param>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">Variable to be used as the default first argument.</param>
        /// <param name="arg2">Variable to be used as the default second argument.</param>
        /// <param name="arg3">Variable to be used as the default third argument.</param>
        /// <param name="arg4">Variable to be used as the default fourth argument.</param>
        // ReSharper disable once UnusedMember.Global
        public static TablePredicate<T1, T2, T3, T4> FromCsv<T1, T2, T3, T4>(string name, string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4)
            => TablePredicate<T1, T2, T3, T4>.FromCsv(name, path, arg1, arg2, arg3, arg4);

        /// <summary>
        /// Make a new table predicate, loading in rows from the specified csv
        /// </summary>
        /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
        /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
        /// <typeparam name="T3">Type of the third argument to the predicate</typeparam>
        /// <typeparam name="T4">Type of the fourth argument to the predicate</typeparam>
        /// <typeparam name="T5">Type of the fifth argument to the predicate</typeparam>
        /// <param name="name">Name for the predicate, for debugging purposes</param>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">Variable to be used as the default first argument.</param>
        /// <param name="arg2">Variable to be used as the default second argument.</param>
        /// <param name="arg3">Variable to be used as the default third argument.</param>
        /// <param name="arg4">Variable to be used as the default fourth argument.</param>
        /// <param name="arg5">Variable to be used as the default fifth argument.</param>
        // ReSharper disable once UnusedMember.Global
        public static TablePredicate<T1, T2, T3, T4, T5> FromCsv<T1, T2, T3, T4, T5>(string name, string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5)
            => TablePredicate<T1, T2, T3, T4, T5>.FromCsv(name, path, arg1, arg2, arg3, arg4, arg5);

        /// <summary>
        /// Make a new table predicate, loading in rows from the specified csv
        /// </summary>
        /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
        /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
        /// <typeparam name="T3">Type of the third argument to the predicate</typeparam>
        /// <typeparam name="T4">Type of the fourth argument to the predicate</typeparam>
        /// <typeparam name="T5">Type of the fifth argument to the predicate</typeparam>
        /// <typeparam name="T6">Type of the sixth argument to the predicate</typeparam>
        /// <param name="name">Name for the predicate, for debugging purposes</param>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">Variable to be used as the default first argument.</param>
        /// <param name="arg2">Variable to be used as the default second argument.</param>
        /// <param name="arg3">Variable to be used as the default third argument.</param>
        /// <param name="arg4">Variable to be used as the default fourth argument.</param>
        /// <param name="arg5">Variable to be used as the default fifth argument.</param>
        /// <param name="arg6">Variable to be used as the default sixth argument.</param>
        // ReSharper disable once UnusedMember.Global
        public static TablePredicate<T1, T2, T3, T4, T5, T6> FromCsv<T1, T2, T3, T4, T5, T6>(string name, string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6)
            => TablePredicate<T1, T2, T3, T4, T5, T6>.FromCsv(name, path, arg1, arg2, arg3, arg4, arg5, arg6);

        /// <summary>
        /// Make a new table predicate, loading in rows from the specified csv
        /// </summary>
        /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
        /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
        /// <typeparam name="T3">Type of the third argument to the predicate</typeparam>
        /// <typeparam name="T4">Type of the fourth argument to the predicate</typeparam>
        /// <typeparam name="T5">Type of the fifth argument to the predicate</typeparam>
        /// <typeparam name="T6">Type of the sixth argument to the predicate</typeparam>
        /// <typeparam name="T7">Type of the seventh argument to the predicate</typeparam>
        /// <param name="name">Name for the predicate, for debugging purposes</param>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">Variable to be used as the default first argument.</param>
        /// <param name="arg2">Variable to be used as the default second argument.</param>
        /// <param name="arg3">Variable to be used as the default third argument.</param>
        /// <param name="arg4">Variable to be used as the default fourth argument.</param>
        /// <param name="arg5">Variable to be used as the default fifth argument.</param>
        /// <param name="arg6">Variable to be used as the default sixth argument.</param>
        /// <param name="arg7">Variable to be used as the default seventh argument.</param>
        // ReSharper disable once UnusedMember.Global
        public static TablePredicate<T1, T2, T3, T4, T5, T6, T7> FromCsv<T1, T2, T3, T4, T5, T6, T7>(string name, string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7)
            => TablePredicate<T1, T2, T3, T4, T5, T6, T7>.FromCsv(name, path, arg1, arg2, arg3, arg4, arg5, arg6, arg7);

        /// <summary>
        /// Make a new table predicate, loading in rows from the specified csv
        /// </summary>
        /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
        /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
        /// <typeparam name="T3">Type of the third argument to the predicate</typeparam>
        /// <typeparam name="T4">Type of the fourth argument to the predicate</typeparam>
        /// <typeparam name="T5">Type of the fifth argument to the predicate</typeparam>
        /// <typeparam name="T6">Type of the sixth argument to the predicate</typeparam>
        /// <typeparam name="T7">Type of the seventh argument to the predicate</typeparam>
        /// <typeparam name="T8">Type of the eighth argument to the predicate</typeparam>
        /// <param name="name">Name for the predicate, for debugging purposes</param>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">Variable to be used as the default first argument.</param>
        /// <param name="arg2">Variable to be used as the default second argument.</param>
        /// <param name="arg3">Variable to be used as the default third argument.</param>
        /// <param name="arg4">Variable to be used as the default fourth argument.</param>
        /// <param name="arg5">Variable to be used as the default fifth argument.</param>
        /// <param name="arg6">Variable to be used as the default sixth argument.</param>
        /// <param name="arg7">Variable to be used as the default seventh argument.</param>
        /// <param name="arg8">Variable to be used as the default eighth argument.</param>
        // ReSharper disable once UnusedMember.Global
        public static TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> FromCsv<T1, T2, T3, T4, T5, T6, T7, T8>(string name, string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7, IColumnSpec<T8> arg8)
            => TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8>.FromCsv(name, path, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);

        /// <summary>
        /// Make a new table predicate, loading in rows from the specified csv
        /// </summary>
        /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">Variable to be used as the default first argument.</param>
        // ReSharper disable once UnusedMember.Global
        public static TablePredicate<T1> FromCsv<T1>(string path, IColumnSpec<T1> arg1)
            => TablePredicate<T1>.FromCsv(path, arg1);

        /// <summary>
        /// Make a new table predicate, loading in rows from the specified csv
        /// </summary>
        /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
        /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">Variable to be used as the default first argument.</param>
        /// <param name="arg2">Variable to be used as the default second argument.</param>
        // ReSharper disable once UnusedMember.Global
        public static TablePredicate<T1, T2> FromCsv<T1, T2>(string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2)
            => TablePredicate<T1, T2>.FromCsv(path, arg1, arg2);

        /// <summary>
        /// Make a new table predicate, loading in rows from the specified csv
        /// </summary>
        /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
        /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
        /// <typeparam name="T3">Type of the third argument to the predicate</typeparam>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">Variable to be used as the default first argument.</param>
        /// <param name="arg2">Variable to be used as the default second argument.</param>
        /// <param name="arg3">Variable to be used as the default third argument.</param>
        // ReSharper disable once UnusedMember.Global
        public static TablePredicate<T1, T2, T3> FromCsv<T1, T2, T3>(string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3)
            => TablePredicate<T1, T2, T3>.FromCsv(path, arg1, arg2, arg3);

        /// <summary>
        /// Make a new table predicate, loading in rows from the specified csv
        /// </summary>
        /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
        /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
        /// <typeparam name="T3">Type of the third argument to the predicate</typeparam>
        /// <typeparam name="T4">Type of the fourth argument to the predicate</typeparam>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">Variable to be used as the default first argument.</param>
        /// <param name="arg2">Variable to be used as the default second argument.</param>
        /// <param name="arg3">Variable to be used as the default third argument.</param>
        /// <param name="arg4">Variable to be used as the default fourth argument.</param>
        // ReSharper disable once UnusedMember.Global
        public static TablePredicate<T1, T2, T3, T4> FromCsv<T1, T2, T3, T4>(string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4)
            => TablePredicate<T1, T2, T3, T4>.FromCsv(path, arg1, arg2, arg3, arg4);

        /// <summary>
        /// Make a new table predicate, loading in rows from the specified csv
        /// </summary>
        /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
        /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
        /// <typeparam name="T3">Type of the third argument to the predicate</typeparam>
        /// <typeparam name="T4">Type of the fourth argument to the predicate</typeparam>
        /// <typeparam name="T5">Type of the fifth argument to the predicate</typeparam>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">Variable to be used as the default first argument.</param>
        /// <param name="arg2">Variable to be used as the default second argument.</param>
        /// <param name="arg3">Variable to be used as the default third argument.</param>
        /// <param name="arg4">Variable to be used as the default fourth argument.</param>
        /// <param name="arg5">Variable to be used as the default fifth argument.</param>
        // ReSharper disable once UnusedMember.Global
        public static TablePredicate<T1, T2, T3, T4, T5> FromCsv<T1, T2, T3, T4, T5>(string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5)
            => TablePredicate<T1, T2, T3, T4, T5>.FromCsv(path, arg1, arg2, arg3, arg4, arg5);

        /// <summary>
        /// Make a new table predicate, loading in rows from the specified csv
        /// </summary>
        /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
        /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
        /// <typeparam name="T3">Type of the third argument to the predicate</typeparam>
        /// <typeparam name="T4">Type of the fourth argument to the predicate</typeparam>
        /// <typeparam name="T5">Type of the fifth argument to the predicate</typeparam>
        /// <typeparam name="T6">Type of the sixth argument to the predicate</typeparam>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">Variable to be used as the default first argument.</param>
        /// <param name="arg2">Variable to be used as the default second argument.</param>
        /// <param name="arg3">Variable to be used as the default third argument.</param>
        /// <param name="arg4">Variable to be used as the default fourth argument.</param>
        /// <param name="arg5">Variable to be used as the default fifth argument.</param>
        /// <param name="arg6">Variable to be used as the default sixth argument.</param>
        // ReSharper disable once UnusedMember.Global
        public static TablePredicate<T1, T2, T3, T4, T5, T6> FromCsv<T1, T2, T3, T4, T5, T6>(string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6)
            => TablePredicate<T1, T2, T3, T4, T5, T6>.FromCsv(path, arg1, arg2, arg3, arg4, arg5, arg6);

        /// <summary>
        /// Make a new table predicate, loading in rows from the specified csv
        /// </summary>
        /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
        /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
        /// <typeparam name="T3">Type of the third argument to the predicate</typeparam>
        /// <typeparam name="T4">Type of the fourth argument to the predicate</typeparam>
        /// <typeparam name="T5">Type of the fifth argument to the predicate</typeparam>
        /// <typeparam name="T6">Type of the sixth argument to the predicate</typeparam>
        /// <typeparam name="T7">Type of the seventh argument to the predicate</typeparam>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">Variable to be used as the default first argument.</param>
        /// <param name="arg2">Variable to be used as the default second argument.</param>
        /// <param name="arg3">Variable to be used as the default third argument.</param>
        /// <param name="arg4">Variable to be used as the default fourth argument.</param>
        /// <param name="arg5">Variable to be used as the default fifth argument.</param>
        /// <param name="arg6">Variable to be used as the default sixth argument.</param>
        /// <param name="arg7">Variable to be used as the default seventh argument.</param>
        // ReSharper disable once UnusedMember.Global
        public static TablePredicate<T1, T2, T3, T4, T5, T6, T7> FromCsv<T1, T2, T3, T4, T5, T6, T7>(string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7)
            => TablePredicate<T1, T2, T3, T4, T5, T6, T7>.FromCsv(path, arg1, arg2, arg3, arg4, arg5, arg6, arg7);

        /// <summary>
        /// Make a new table predicate, loading in rows from the specified csv
        /// </summary>
        /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
        /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
        /// <typeparam name="T3">Type of the third argument to the predicate</typeparam>
        /// <typeparam name="T4">Type of the fourth argument to the predicate</typeparam>
        /// <typeparam name="T5">Type of the fifth argument to the predicate</typeparam>
        /// <typeparam name="T6">Type of the sixth argument to the predicate</typeparam>
        /// <typeparam name="T7">Type of the seventh argument to the predicate</typeparam>
        /// <typeparam name="T8">Type of the eighth argument to the predicate</typeparam>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="arg1">Variable to be used as the default first argument.</param>
        /// <param name="arg2">Variable to be used as the default second argument.</param>
        /// <param name="arg3">Variable to be used as the default third argument.</param>
        /// <param name="arg4">Variable to be used as the default fourth argument.</param>
        /// <param name="arg5">Variable to be used as the default fifth argument.</param>
        /// <param name="arg6">Variable to be used as the default sixth argument.</param>
        /// <param name="arg7">Variable to be used as the default seventh argument.</param>
        /// <param name="arg8">Variable to be used as the default eighth argument.</param>
        // ReSharper disable once UnusedMember.Global
        public static TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8> FromCsv<T1, T2, T3, T4, T5, T6, T7, T8>(string path, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7, IColumnSpec<T8> arg8)
            => TablePredicate<T1, T2, T3, T4, T5, T6, T7, T8>.FromCsv(path, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        #endregion

        #region Definition declaration sugar
        /// <summary>
        /// Creates a predicate that is true whenever the goals of the IfAndOnlyIf clause are true.
        /// Definitions are not computed as tables; they're replaced with the bodies of their IfAndOnlyIf clauses whenever they're called.
        /// </summary>
        /// <param name="name">Name of the predicate, for debugging purposes</param>
        /// <param name="arg1">Argument to the predicate</param>
        public static Definition<T1> Definition<T1>(string name, Var<T1> arg1) => new Definition<T1>(name, arg1);

        /// <summary>
        /// Creates a predicate that is true whenever the goals of the IfAndOnlyIf clause are true.
        /// Definitions are not computed as tables; they're replaced with the bodies of their IfAndOnlyIf clauses whenever they're called.
        /// </summary>
        /// <param name="name">Name of the predicate, for debugging purposes</param>
        /// <param name="arg1">First argument to the predicate</param>
        /// <param name="arg2">Second argument to the predicate</param>
        public static Definition<T1,T2> Definition<T1,T2>(string name, Var<T1> arg1, Var<T2> arg2) => new Definition<T1,T2>(name, arg1, arg2);
        /// <summary>
        /// Creates a predicate that is true whenever the goals of the IfAndOnlyIf clause are true.
        /// Definitions are not computed as tables; they're replaced with the bodies of their IfAndOnlyIf clauses whenever they're called.
        /// </summary>
        /// <param name="name">Name of the predicate, for debugging purposes</param>
        /// <param name="arg1">First argument to the predicate</param>
        /// <param name="arg2">Second argument to the predicate</param>
        /// <param name="arg3">Third argument to the predicate</param>
        // ReSharper disable once UnusedMember.Global
        public static Definition<T1,T2,T3> Definition<T1,T2,T3>(string name, Var<T1> arg1, Var<T2> arg2, Var<T3> arg3) => new Definition<T1,T2,T3>(name, arg1, arg2, arg3);
        /// <summary>
        /// Creates a predicate that is true whenever the goals of the IfAndOnlyIf clause are true.
        /// Definitions are not computed as tables; they're replaced with the bodies of their IfAndOnlyIf clauses whenever they're called.
        /// </summary>
        /// <param name="name">Name of the predicate, for debugging purposes</param>
        /// <param name="arg1">First argument to the predicate</param>
        /// <param name="arg2">Second argument to the predicate</param>
        /// <param name="arg3">Third argument to the predicate</param>
        /// <param name="arg4">Fourth argument to the predicate</param>
        // ReSharper disable once UnusedMember.Global
        public static Definition<T1,T2,T3,T4> Definition<T1,T2,T3,T4>(string name, Var<T1> arg1, Var<T2> arg2, Var<T3> arg3, Var<T4> arg4) 
            => new Definition<T1,T2,T3,T4>(name, arg1, arg2, arg3, arg4);
        /// <summary>
        /// Creates a predicate that is true whenever the goals of the IfAndOnlyIf clause are true.
        /// Definitions are not computed as tables; they're replaced with the bodies of their IfAndOnlyIf clauses whenever they're called.
        /// </summary>
        /// <param name="name">Name of the predicate, for debugging purposes</param>
        /// <param name="arg1">First argument to the predicate</param>
        /// <param name="arg2">Second argument to the predicate</param>
        /// <param name="arg3">Third argument to the predicate</param>
        /// <param name="arg4">Fourth argument to the predicate</param>
        /// <param name="arg5">Fifth argument to the predicate</param>
        // ReSharper disable once UnusedMember.Global
        public static Definition<T1,T2,T3,T4,T5> Definition<T1,T2,T3,T4,T5>(string name, Var<T1> arg1, Var<T2> arg2, Var<T3> arg3, Var<T4> arg4, Var<T5> arg5) 
            => new Definition<T1,T2,T3,T4,T5>(name, arg1, arg2, arg3, arg4, arg5);
        /// <summary>
        /// Creates a predicate that is true whenever the goals of the IfAndOnlyIf clause are true.
        /// Definitions are not computed as tables; they're replaced with the bodies of their IfAndOnlyIf clauses whenever they're called.
        /// </summary>
        /// <param name="name">Name of the predicate, for debugging purposes</param>
        /// <param name="arg1">First argument to the predicate</param>
        /// <param name="arg2">Second argument to the predicate</param>
        /// <param name="arg3">Third argument to the predicate</param>
        /// <param name="arg4">Fourth argument to the predicate</param>
        /// <param name="arg5">Fifth argument to the predicate</param>
        /// <param name="arg6">Sixth argument to the predicate</param>
        // ReSharper disable once UnusedMember.Global
        public static Definition<T1,T2,T3,T4,T5,T6> Definition<T1,T2,T3,T4,T5,T6>(string name, Var<T1> arg1, Var<T2> arg2, Var<T3> arg3, Var<T4> arg4, Var<T5> arg5, Var<T6> arg6) 
            => new Definition<T1,T2,T3,T4,T5,T6>(name, arg1, arg2, arg3, arg4, arg5, arg6);
        #endregion

        internal static Func<T> BuildSafeMemberAccess<T>(object target, string property) => 
            (Func<T>)Delegate.CreateDelegate(typeof(Func<T>), target, 
                (target.GetType().GetProperty(property) ?? throw new MemberAccessException(
                    $"{property} property not found for type {target.GetType()}")
                ).GetMethod);

        #region PrimitiveTest declaration sugar
        /// <summary>
        /// Makes a PrimitiveTest where the bool Func is built from a property on the given type
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static PrimitiveTest TestMember(object type, string property) => 
            new PrimitiveTest(property, BuildSafeMemberAccess<bool>(type, property));

        /// <summary>
        /// Makes a PrimitiveTest with the same name as the Func being passed in
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static PrimitiveTest Test(Func<bool> fn) => new PrimitiveTest(fn.Method.Name, fn);
        /// <summary>
        /// Makes a PrimitiveTest with the given name
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static PrimitiveTest Test(string name, Func<bool> fn) => new PrimitiveTest(name, fn);

        /// <summary>
        /// Makes a PrimitiveTest with the same name as the System.Predicate being passed in
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static PrimitiveTest<TIn> TestMethod<TIn>(Func<TIn, bool> fn) => new PrimitiveTest<TIn>(fn.Method.Name, fn);
        /// <summary>
        /// Makes a PrimitiveTest with the given name
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static PrimitiveTest<TIn> Test<TIn>(string name, Func<TIn, bool> fn) => new PrimitiveTest<TIn>(name, fn);
         

        //public static PrimitiveTest<TIn> TestMethod<TIn>(Func<TIn, bool> fn) => new PrimitiveTest<TIn>(fn.Method.Name, fn);
        //public static PrimitiveTest<TIn> Test<TIn>(string name, Func<TIn, bool> fn) => new PrimitiveTest<TIn>(name, fn);


        /// <summary>
        /// Makes a PrimitiveTest with the same name as the Func being passed in
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static PrimitiveTest<TIn1, TIn2> TestMethod<TIn1, TIn2>(Func<TIn1, TIn2, bool> fn) => new PrimitiveTest<TIn1, TIn2>(fn.Method.Name, fn);
        /// <summary>
        /// Makes a PrimitiveTest with the given name
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static PrimitiveTest<TIn1, TIn2> Test<TIn1, TIn2>(string name, Func<TIn1, TIn2, bool> fn) => new PrimitiveTest<TIn1, TIn2>(name, fn);

        /// <summary>
        /// Makes a PrimitiveTest with the same name as the Func being passed in
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static PrimitiveTest<TIn1, TIn2, TIn3> TestMethod<TIn1, TIn2, TIn3>(Func<TIn1, TIn2, TIn3, bool> fn) => new PrimitiveTest<TIn1, TIn2, TIn3>(fn.Method.Name, fn);
        /// <summary>
        /// Makes a PrimitiveTest with the given name
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static PrimitiveTest<TIn1, TIn2, TIn3> Test<TIn1, TIn2, TIn3>(string name, Func<TIn1, TIn2, TIn3, bool> fn) => new PrimitiveTest<TIn1, TIn2, TIn3>(name, fn);

        /// <summary>
        /// Makes a PrimitiveTest with the same name as the Func being passed in
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static PrimitiveTest<TIn1, TIn2, TIn3, TIn4> TestMethod<TIn1, TIn2, TIn3, TIn4>(Func<TIn1, TIn2, TIn3, TIn4, bool> fn) => new PrimitiveTest<TIn1, TIn2, TIn3, TIn4>(fn.Method.Name, fn);
        /// <summary>
        /// Makes a PrimitiveTest with the given name
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static PrimitiveTest<TIn1, TIn2, TIn3, TIn4> Test<TIn1, TIn2, TIn3, TIn4>(string name, Func<TIn1, TIn2, TIn3, TIn4, bool> fn) => new PrimitiveTest<TIn1, TIn2, TIn3, TIn4>(name, fn);

        /// <summary>
        /// Makes a PrimitiveTest with the same name as the Func being passed in
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static PrimitiveTest<TIn1, TIn2, TIn3, TIn4, TIn5> TestMethod<TIn1, TIn2, TIn3, TIn4, TIn5>(Func<TIn1, TIn2, TIn3, TIn4, TIn5, bool> fn) => new PrimitiveTest<TIn1, TIn2, TIn3, TIn4, TIn5>(fn.Method.Name, fn);
        /// <summary>
        /// Makes a PrimitiveTest with the given name
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static PrimitiveTest<TIn1, TIn2, TIn3, TIn4, TIn5> Test<TIn1, TIn2, TIn3, TIn4, TIn5>(string name, Func<TIn1, TIn2, TIn3, TIn4, TIn5, bool> fn) => new PrimitiveTest<TIn1, TIn2, TIn3, TIn4, TIn5>(name, fn);

        /// <summary>
        /// Makes a PrimitiveTest with the same name as the Func being passed in
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static PrimitiveTest<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6> TestMethod<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6>(Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, bool> fn) => new PrimitiveTest<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6>(fn.Method.Name, fn);
        /// <summary>
        /// Makes a PrimitiveTest with the given name
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static PrimitiveTest<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6> Test<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6>(string name, Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, bool> fn) => new PrimitiveTest<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6>(name, fn);

        /// <summary>
        /// Makes a PrimitiveTest with the same name as the Func being passed in
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static PrimitiveTest<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7> TestMethod<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7>(Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, bool> fn) => new PrimitiveTest<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7>(fn.Method.Name, fn);
        /// <summary>
        /// Makes a PrimitiveTest with the given name
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static PrimitiveTest<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7> Test<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7>(string name, Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, bool> fn) => new PrimitiveTest<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7>(name, fn);

        /// <summary>
        /// Makes a PrimitiveTest with the same name as the Func being passed in
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static PrimitiveTest<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8> TestMethod<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8>(Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, bool> fn) => new PrimitiveTest<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8>(fn.Method.Name, fn);
        /// <summary>
        /// Makes a PrimitiveTest with the given name
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static PrimitiveTest<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8> Test<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8>(string name, Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, bool> fn) => new PrimitiveTest<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8>(name, fn);
        #endregion

        #region Function declaration sugar
        /// <summary>
        /// Makes a function that can be placed in functional expressions
        /// </summary>
        /// <param name="name">Name, for debugging purposes</param>
        /// <param name="fn">C# code implementing the function</param>
        // ReSharper disable once UnusedMember.Global
        public static Function<T> Function<T>(string name, Func<T> fn) => new Function<T>(name, fn);
        /// <summary>
        /// Makes a Function with the same name as the Func being passed in
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static Function<TIn> Method<TIn>(Func<TIn> fn) => new Function<TIn>(fn.Method.Name, fn);

        /// <summary>
        /// Makes a Function where the Func T is built from a property on the given type
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static Function<T> Member<T>(object type, string property) =>
            new Function<T>(property, BuildSafeMemberAccess<T>(type, property));
        /// <summary>
        /// Makes a Function where the Func T is built from a property on the given type,
        /// pre-pending Get to the property for the Function name
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static Function<T> GetMember<T>(object type, string property) =>
            new Function<T>($"Get{property}", BuildSafeMemberAccess<T>(type, property));

        /// <summary>
        /// Makes a function that can be placed in functional expressions
        /// </summary>
        /// <param name="name">Name, for debugging purposes</param>
        /// <param name="fn">C# code implementing the function</param>
        public static Function<TIn, TOut> Function<TIn, TOut>(string name, Func<TIn, TOut> fn) => new Function<TIn, TOut>(name, fn);
        /// <summary>
        /// Makes a Function with the same name as the Func being passed in
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static Function<TIn, TOut> Method<TIn, TOut>(Func<TIn, TOut> fn) => new Function<TIn, TOut>(fn.Method.Name, fn);

        /// <summary>
        /// Makes a function that can be placed in functional expressions
        /// </summary>
        /// <param name="name">Name, for debugging purposes</param>
        /// <param name="fn">C# code implementing the function</param>
        // ReSharper disable once UnusedMember.Global
        public static Function<TIn1, TIn2, TOut> Function<TIn1, TIn2, TOut>(string name, Func<TIn1, TIn2, TOut> fn) => new Function<TIn1, TIn2, TOut>(name, fn);
        /// <summary>
        /// Makes a Function with the same name as the Func being passed in
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static Function<TIn1, TIn2, TOut> Method<TIn1, TIn2, TOut>(Func<TIn1, TIn2, TOut> fn) => new Function<TIn1, TIn2, TOut>(fn.Method.Name, fn);
        
        /// <summary>
        /// Makes a function that can be placed in functional expressions
        /// </summary>
        /// <param name="name">Name, for debugging purposes</param>
        /// <param name="fn">C# code implementing the function</param>
        // ReSharper disable once UnusedMember.Global
        public static Function<TIn1, TIn2, TIn3, TOut> Function<TIn1, TIn2, TIn3, TOut>(string name, Func<TIn1, TIn2, TIn3, TOut> fn) => new Function<TIn1, TIn2, TIn3, TOut>(name, fn);
        /// <summary>
        /// Makes a Function with the same name as the Func being passed in
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static Function<TIn1, TIn2, TIn3, TOut> Method<TIn1, TIn2, TIn3, TOut>(Func<TIn1, TIn2, TIn3, TOut> fn) => new Function<TIn1, TIn2, TIn3, TOut>(fn.Method.Name, fn);
        
        /// <summary>
        /// Makes a function that can be placed in functional expressions
        /// </summary>
        /// <param name="name">Name, for debugging purposes</param>
        /// <param name="fn">C# code implementing the function</param>
        // ReSharper disable once UnusedMember.Global
        public static Function<TIn1, TIn2, TIn3, TIn4, TOut> Function<TIn1, TIn2, TIn3, TIn4, TOut>(string name, Func<TIn1, TIn2, TIn3, TIn4, TOut> fn) => new Function<TIn1, TIn2, TIn3, TIn4, TOut>(name, fn);
        /// <summary>
        /// Makes a Function with the same name as the Func being passed in
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static Function<TIn1, TIn2, TIn3, TIn4, TOut> Method<TIn1, TIn2, TIn3, TIn4, TOut>(Func<TIn1, TIn2, TIn3, TIn4, TOut> fn) => new Function<TIn1, TIn2, TIn3, TIn4, TOut>(fn.Method.Name, fn);
        
        /// <summary>
        /// Makes a function that can be placed in functional expressions
        /// </summary>
        /// <param name="name">Name, for debugging purposes</param>
        /// <param name="fn">C# code implementing the function</param>
        // ReSharper disable once UnusedMember.Global
        public static Function<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> Function<TIn1, TIn2, TIn3, TIn4, TIn5, TOut>(string name, Func<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> fn) => new Function<TIn1, TIn2, TIn3, TIn4, TIn5, TOut>(name, fn);
        /// <summary>
        /// Makes a Function with the same name as the Func being passed in
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static Function<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> Method<TIn1, TIn2, TIn3, TIn4, TIn5, TOut>(Func<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> fn) => new Function<TIn1, TIn2, TIn3, TIn4, TIn5, TOut>(fn.Method.Name, fn);
        
        /// <summary>
        /// Makes a function that can be placed in functional expressions
        /// </summary>
        /// <param name="name">Name, for debugging purposes</param>
        /// <param name="fn">C# code implementing the function</param>
        // ReSharper disable once UnusedMember.Global
        public static Function<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut> Function<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut>(string name, Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut> fn) => new Function<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut>(name, fn);
        /// <summary>
        /// Makes a Function with the same name as the Func being passed in
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static Function<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut> Method<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut>(Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut> fn) => new Function<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut>(fn.Method.Name, fn);
        
        /// <summary>
        /// Makes a function that can be placed in functional expressions
        /// </summary>
        /// <param name="name">Name, for debugging purposes</param>
        /// <param name="fn">C# code implementing the function</param>
        // ReSharper disable once UnusedMember.Global
        public static Function<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut> Function<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut>(string name, Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut> fn) => new Function<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut>(name, fn);
        /// <summary>
        /// Makes a Function with the same name as the Func being passed in
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static Function<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut> Method<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut>(Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut> fn) => new Function<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut>(fn.Method.Name, fn);
        
        #endregion

    }
}
