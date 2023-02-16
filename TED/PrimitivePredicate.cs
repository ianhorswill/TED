using System.Collections.Generic;

namespace TED
{
    /// <summary>
    /// A primitive predicate (one computed by code rather than matching to a table) with one argument
    /// </summary>
    /// <typeparam name="T1">Type of the predicate's argument</typeparam>
    public abstract class PrimitivePredicate<T1> : Predicate
    {
        protected PrimitivePredicate(string name) : base(name)
        {
        }

        /// <summary>
        /// Return a goal representing a call to this primitive.
        /// This is the abstract syntax tree representing the call, not the internal form used in the interpreter
        /// </summary>
        public TED.Goal this[Term<T1> arg] => new Goal(this, arg);

        /// <summary>
        /// Map a Goal (AST for call) to a Call object (the internal form used by the interpreter)
        /// </summary>
        public abstract Call MakeCall(Goal g, GoalAnalyzer tc);

        /// <summary>
        /// Custom goal representation for this primitive
        /// </summary>
        public class Goal : TED.Goal
        {
            public readonly PrimitivePredicate<T1> Primitive;
            public readonly Term<T1> Arg1;

            public override Predicate Predicate => Primitive;

            public Goal(PrimitivePredicate<T1> predicate, Term<T1> arg1)  : base(new Term[] {arg1})
            {
                Primitive = predicate;
                Arg1 = arg1;
            }

            internal override TED.Goal RenameArguments(Substitution s)
                => new Goal(Primitive, s.Substitute(Arg1));

            /// <summary>
            /// Generate a custom Call object for a call to this particular predicate
            /// You'll want to fill this in with a constructor for a Call class specific to your predicate
            /// </summary>
            /// <param name="ga">Goal analyzer for the rule this goal appears in.  Used to do mode analysis of the variables in the call.</param>
            /// <returns>Instance of the custom call class for this particular primitive</returns>
            internal override Call MakeCall(GoalAnalyzer ga) => Primitive.MakeCall(this, ga);
        }
    }

    /// <summary>
    /// A primitive predicate (one computed by code rather than matching to a table) with one argument
    /// </summary>
    /// <typeparam name="T1">Type of the predicate's first argument</typeparam>
    /// <typeparam name="T2">Type of the predicate's second argument</typeparam>
    public abstract class PrimitivePredicate<T1, T2> : Predicate
    {
        protected PrimitivePredicate(string name) : base(name)
        {
        }

        /// <summary>
        /// Return a goal representing a call to this primitive.
        /// This is the abstract syntax tree representing the call, not the internal form used in the interpreter
        /// </summary>
        public TED.Goal this[Term<T1> arg1, Term<T2> arg2] => new Goal(this, arg1, arg2);

        /// <summary>
        /// Map a Goal (AST for call) to a Call object (the internal form used by the interpreter)
        /// </summary>
        public abstract Call MakeCall(Goal g, GoalAnalyzer tc);

        /// <summary>
        /// Custom goal class for this particular primitive
        /// </summary>
        public class Goal : TED.Goal
        {
            public readonly PrimitivePredicate<T1, T2> Primitive;
            public readonly Term<T1> Arg1;
            public readonly Term<T2> Arg2;

            public override Predicate Predicate => Primitive;


            public Goal(PrimitivePredicate<T1, T2> primitive, Term<T1> arg1, Term<T2> arg2) : base(new Term[] {arg1, arg2})
            {
                Primitive = primitive;
                Arg1 = arg1;
                Arg2 = arg2;
            }

            internal override TED.Goal RenameArguments(Substitution s)
                => new Goal(Primitive, s.Substitute(Arg1), s.Substitute(Arg2));

            /// <summary>
            /// Generate a custom Call object for a call to this particular predicate
            /// You'll want to fill this in with a constructor for a Call class specific to your predicate
            /// </summary>
            /// <param name="ga">Goal analyzer for the rule this goal appears in.  Used to do mode analysis of the variables in the call.</param>
            /// <returns>Instance of the custom call class for this particular primitive</returns>
            internal override Call MakeCall(GoalAnalyzer ga) => Primitive.MakeCall(this, ga);
        }
    }

    /// <summary>
    /// A primitive predicate (one computed by code rather than matching to a table) with one argument
    /// </summary>
    /// <typeparam name="T1">Type of the predicate's first argument</typeparam>
    /// <typeparam name="T2">Type of the predicate's second argument</typeparam>
    /// <typeparam name="T3">Type if the predicate's third argument</typeparam>
    public abstract class PrimitivePredicate<T1, T2, T3> : Predicate
    {
        protected PrimitivePredicate(string name) : base(name)
        {
        }

        /// <summary>
        /// Return a goal representing a call to this primitive.
        /// This is the abstract syntax tree representing the call, not the internal form used in the interpreter
        /// </summary>
        public TED.Goal this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3] => new Goal(this, arg1, arg2, arg3);

        /// <summary>
        /// Map a Goal (AST for call) to a Call object (the internal form used by the interpreter)
        /// </summary>
        public abstract Call MakeCall(Goal g, GoalAnalyzer tc);

        /// <summary>
        /// Custom goal class for this particular primitive
        /// </summary>
        public class Goal : TED.Goal
        {
            public readonly PrimitivePredicate<T1, T2, T3> Primitive;
            public readonly Term<T1> Arg1;
            public readonly Term<T2> Arg2;
            public readonly Term<T3> Arg3;

            public override Predicate Predicate => Primitive;

            public Goal(PrimitivePredicate<T1, T2, T3> primitive, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3) 
                : base(new Term[] { arg1, arg2, arg3 })
            {
                Primitive = primitive;
                Arg1 = arg1;
                Arg2 = arg2;
                Arg3 = arg3;
            }

            internal override TED.Goal RenameArguments(Substitution s)
                => new Goal(Primitive, s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3));

            /// <summary>
            /// Generate a custom Call object for a call to this particular predicate
            /// You'll want to fill this in with a constructor for a Call class specific to your predicate
            /// </summary>
            /// <param name="ga">Goal analyzer for the rule this goal appears in.  Used to do mode analysis of the variables in the call.</param>
            /// <returns>Instance of the custom call class for this particular primitive</returns>
            internal override Call MakeCall(GoalAnalyzer ga) => Primitive.MakeCall(this, ga);
        }
    }

    /// <summary>
    /// A primitive predicate (one computed by code rather than matching to a table) with one argument
    /// </summary>
    /// <typeparam name="T1">Type of the predicate's first argument</typeparam>
    /// <typeparam name="T2">Type of the predicate's second argument</typeparam>
    /// <typeparam name="T3">Type of the predicate's third argument</typeparam>
    /// <typeparam name="T4">Type of the predicate's fourth argument</typeparam>
    public abstract class PrimitivePredicate<T1, T2, T3, T4> : Predicate
    {
        protected PrimitivePredicate(string name) : base(name)
        {
        }

        /// <summary>
        /// Return a goal representing a call to this primitive.
        /// This is the abstract syntax tree representing the call, not the internal form used in the interpreter
        /// </summary>
        public TED.Goal this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4] => new Goal(this, arg1, arg2, arg3, arg4);

        /// <summary>
        /// Map a Goal (AST for call) to a Call object (the internal form used by the interpreter)
        /// </summary>
        public abstract Call MakeCall(Goal g, GoalAnalyzer tc);

        /// <summary>
        /// Custom goal class for this particular primitive
        /// </summary>
        public class Goal : TED.Goal
        {
            public readonly PrimitivePredicate<T1, T2, T3, T4> Primitive;
            public readonly Term<T1> Arg1;
            public readonly Term<T2> Arg2;
            public readonly Term<T3> Arg3;
            public readonly Term<T4> Arg4;

            public override Predicate Predicate => Primitive;

            public Goal(PrimitivePredicate<T1, T2, T3, T4> primitive, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4)
                : base(new Term[] { arg1, arg2, arg3, arg4 })
            {
                Primitive = primitive;
                Arg1 = arg1;
                Arg2 = arg2;
                Arg3 = arg3;
                Arg4 = arg4;
            }

            internal override TED.Goal RenameArguments(Substitution s)
                => new Goal(Primitive, s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3), s.Substitute(Arg4));

            /// <summary>
            /// Generate a custom Call object for a call to this particular predicate
            /// You'll want to fill this in with a constructor for a Call class specific to your predicate
            /// </summary>
            /// <param name="ga">Goal analyzer for the rule this goal appears in.  Used to do mode analysis of the variables in the call.</param>
            /// <returns>Instance of the custom call class for this particular primitive</returns>
            internal override Call MakeCall(GoalAnalyzer ga) => Primitive.MakeCall(this, ga);
        }
    }
}
