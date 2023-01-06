using System.Collections.Generic;

namespace TED
{
    /// <summary>
    /// A primitive predicate (one computed by code rather than matching to a table) with one argument
    /// </summary>
    /// <typeparam name="T1">Type of the predicate's argument</typeparam>
    public abstract class PrimitivePredicate<T1> : AnyPredicate
    {
        protected PrimitivePredicate(string name) : base(name)
        {
        }

        /// <summary>
        /// Return a goal representing a call to this primitive.
        /// This is the abstract syntax tree representing the call, not the internal form used in the interpreter
        /// </summary>
        public AnyGoal this[Term<T1> arg] => new Goal(this, arg);

        /// <summary>
        /// Map a Goal (AST for call) to a Call object (the internal form used by the interpreter)
        /// </summary>
        internal abstract AnyCall MakeCall(Goal g, GoalAnalyzer tc);

        /// <summary>
        /// Custom goal representation for this primitive
        /// </summary>
        public class Goal : AnyGoal
        {
            public readonly PrimitivePredicate<T1> Primitive;
            public readonly Term<T1> Arg1;

            public override AnyPredicate Predicate => Primitive;

            public Goal(PrimitivePredicate<T1> predicate, Term<T1> arg1)  : base(new AnyTerm[] {arg1})
            {
                Primitive = predicate;
                Arg1 = arg1;
            }

            internal override AnyGoal RenameArguments(Substitution s)
                => new Goal(Primitive, s.Substitute(Arg1));

            /// <summary>
            /// Generate a custom Call object for a call to this particular predicate
            /// You'll want to fill this in with a constructor for a Call class specific to your predicate
            /// </summary>
            /// <param name="ga">Goal analyzer for the rule this goal appears in.  Used to do mode analysis of the variables in the call.</param>
            /// <returns>Instance of the custom call class for this particular primitive</returns>
            internal override AnyCall MakeCall(GoalAnalyzer ga) => Primitive.MakeCall(this, ga);
        }
    }

    /// <summary>
    /// A primitive predicate (one computed by code rather than matching to a table) with one argument
    /// </summary>
    /// <typeparam name="T1">Type of the predicate's firstargument</typeparam>
    /// <typeparam name="T2">Type of the predicate's second argument</typeparam>
    public abstract class PrimitivePredicate<T1, T2> : AnyPredicate
    {
        protected PrimitivePredicate(string name) : base(name)
        {
        }

        /// <summary>
        /// Return a goal representing a call to this primitive.
        /// This is the abstract syntax tree representing the call, not the internal form used in the interpreter
        /// </summary>
        public AnyGoal this[Term<T1> arg1, Term<T2> arg2] => new Goal(this, arg1, arg2);

        /// <summary>
        /// Map a Goal (AST for call) to a Call object (the internal form used by the interpreter)
        /// </summary>
        internal abstract AnyCall MakeCall(Goal g, GoalAnalyzer tc);

        /// <summary>
        /// Custom goal class for this particular primitive
        /// </summary>
        public class Goal : AnyGoal
        {
            public readonly PrimitivePredicate<T1, T2> Primitive;
            public readonly Term<T1> Arg1;
            public readonly Term<T2> Arg2;

            public override AnyPredicate Predicate => Primitive;


            public Goal(PrimitivePredicate<T1, T2> primitive, Term<T1> arg1, Term<T2> arg2) : base(new AnyTerm[] {arg1, arg2})
            {
                Primitive = primitive;
                Arg1 = arg1;
                Arg2 = arg2;
            }

            internal override AnyGoal RenameArguments(Substitution s)
                => new Goal(Primitive, s.Substitute(Arg1), s.Substitute(Arg2));

            /// <summary>
            /// Generate a custom Call object for a call to this particular predicate
            /// You'll want to fill this in with a constructor for a Call class specific to your predicate
            /// </summary>
            /// <param name="ga">Goal analyzer for the rule this goal appears in.  Used to do mode analysis of the variables in the call.</param>
            /// <returns>Instance of the custom call class for this particular primitive</returns>
            internal override AnyCall MakeCall(GoalAnalyzer ga) => Primitive.MakeCall(this, ga);
        }
    }
}
