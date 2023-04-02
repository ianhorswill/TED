using TED.Interpreter;
using TED.Preprocessing;

namespace TED.Primitives
{
    /// <summary>
    /// A primitive predicate (one computed by code rather than matching to a table) with no arguments
    /// </summary>
    public abstract class PrimitivePredicate : Predicate
    {
        /// <summary>
        /// Make a primitive predicate with the specified name
        /// </summary>
        protected PrimitivePredicate(string name) : base(name) { }

        /// <summary>
        /// Return a goal representing a call to this primitive.
        /// </summary>
        public static implicit operator Interpreter.Goal(PrimitivePredicate prim) => new Goal(prim);

        /// <summary>
        /// Map a Goal (AST for call) to a Call object (the internal form used by the interpreter)
        /// </summary>
        public abstract Call MakeCall(Goal g, GoalAnalyzer tc);

        /// <summary>
        /// Custom goal representation for this primitive
        /// </summary>
        public class Goal : Interpreter.Goal
        {
            /// <summary>
            /// The primitive being called
            /// </summary>
            public readonly PrimitivePredicate Primitive;

            /// <inheritdoc />
            public override Predicate Predicate => Primitive;

            /// <summary>
            /// Make a new goal for this predicate
            /// </summary>
            internal Goal(PrimitivePredicate predicate) : base(new Term[] { })
            {
                Primitive = predicate;
            }

            internal override Interpreter.Goal RenameArguments(Substitution s) => new Goal(Primitive);

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
    /// <typeparam name="T1">Type of the predicate's argument</typeparam>
    public abstract class PrimitivePredicate<T1> : Predicate
    {
        /// <inheritdoc />
        protected PrimitivePredicate(string name) : base(name)
        {
        }

        /// <summary>
        /// Return a goal representing a call to this primitive.
        /// This is the abstract syntax tree representing the call, not the internal form used in the interpreter
        /// </summary>
        public Interpreter.Goal this[Term<T1> arg] => new Goal(this, arg);

        /// <summary>
        /// Map a Goal (AST for call) to a Call object (the internal form used by the interpreter)
        /// </summary>
        public abstract Call MakeCall(Goal g, GoalAnalyzer tc);

        /// <summary>
        /// Custom goal representation for this primitive
        /// </summary>
        public class Goal : Interpreter.Goal
        {
            /// <summary>
            /// The primitive being called
            /// </summary>
            public readonly PrimitivePredicate<T1> Primitive;
            /// <summary>
            /// First argument
            /// </summary>
            public readonly Term<T1> Arg1;

            /// <inheritdoc />
            public override Predicate Predicate => Primitive;

            /// <inheritdoc />
            public Goal(PrimitivePredicate<T1> predicate, Term<T1> arg1) : base(new Term[] { arg1 })
            {
                Primitive = predicate;
                Arg1 = arg1;
            }

            internal override Interpreter.Goal RenameArguments(Substitution s)
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
        /// <inheritdoc />
        protected PrimitivePredicate(string name) : base(name)
        {
        }

        /// <summary>
        /// Return a goal representing a call to this primitive.
        /// This is the abstract syntax tree representing the call, not the internal form used in the interpreter
        /// </summary>
        public Interpreter.Goal this[Term<T1> arg1, Term<T2> arg2] => new Goal(this, arg1, arg2);

        /// <summary>
        /// Map a Goal (AST for call) to a Call object (the internal form used by the interpreter)
        /// </summary>
        public abstract Call MakeCall(Goal g, GoalAnalyzer tc);

        /// <summary>
        /// Custom goal class for this particular primitive
        /// </summary>
        public class Goal : Interpreter.Goal
        {
            /// <summary>
            /// The primitive being called
            /// </summary>
            public readonly PrimitivePredicate<T1, T2> Primitive;
            /// <summary>
            /// First argument
            /// </summary>
            public readonly Term<T1> Arg1;
            /// <summary>
            /// Second argument
            /// </summary>
            public readonly Term<T2> Arg2;

            /// <inheritdoc />
            public override Predicate Predicate => Primitive;

            /// <inheritdoc />
            public Goal(PrimitivePredicate<T1, T2> primitive, Term<T1> arg1, Term<T2> arg2) : base(new Term[] { arg1, arg2 })
            {
                Primitive = primitive;
                Arg1 = arg1;
                Arg2 = arg2;
            }

            internal override Interpreter.Goal RenameArguments(Substitution s)
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
        /// <inheritdoc />
        protected PrimitivePredicate(string name) : base(name)
        {
        }

        /// <summary>
        /// Return a goal representing a call to this primitive.
        /// This is the abstract syntax tree representing the call, not the internal form used in the interpreter
        /// </summary>
        public Interpreter.Goal this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3] => new Goal(this, arg1, arg2, arg3);

        /// <summary>
        /// Map a Goal (AST for call) to a Call object (the internal form used by the interpreter)
        /// </summary>
        public abstract Call MakeCall(Goal g, GoalAnalyzer tc);

        /// <summary>
        /// Custom goal class for this particular primitive
        /// </summary>
        public class Goal : Interpreter.Goal
        {
            /// <summary>
            /// The primitive being called
            /// </summary>
            public readonly PrimitivePredicate<T1, T2, T3> Primitive;
            /// <summary>
            /// First argument
            /// </summary>
            public readonly Term<T1> Arg1;
            /// <summary>
            /// Second argument
            /// </summary>
            public readonly Term<T2> Arg2;
            /// <summary>
            /// Third argument
            /// </summary>
            public readonly Term<T3> Arg3;

            /// <inheritdoc />
            public override Predicate Predicate => Primitive;

            /// <inheritdoc />
            public Goal(PrimitivePredicate<T1, T2, T3> primitive, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3)
                : base(new Term[] { arg1, arg2, arg3 })
            {
                Primitive = primitive;
                Arg1 = arg1;
                Arg2 = arg2;
                Arg3 = arg3;
            }

            internal override Interpreter.Goal RenameArguments(Substitution s)
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
        /// <inheritdoc />
        protected PrimitivePredicate(string name) : base(name)
        {
        }

        /// <summary>
        /// Return a goal representing a call to this primitive.
        /// This is the abstract syntax tree representing the call, not the internal form used in the interpreter
        /// </summary>
        public Interpreter.Goal this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4] => new Goal(this, arg1, arg2, arg3, arg4);

        /// <summary>
        /// Map a Goal (AST for call) to a Call object (the internal form used by the interpreter)
        /// </summary>
        public abstract Call MakeCall(Goal g, GoalAnalyzer tc);

        /// <summary>
        /// Custom goal class for this particular primitive
        /// </summary>
        public class Goal : Interpreter.Goal
        {
            /// <summary>
            /// The primitive being called
            /// </summary>
            public readonly PrimitivePredicate<T1, T2, T3, T4> Primitive;
            /// <summary>
            /// First argument
            /// </summary>
            public readonly Term<T1> Arg1;
            /// <summary>
            /// Second argument
            /// </summary>
            public readonly Term<T2> Arg2;
            /// <summary>
            /// Third argument
            /// </summary>
            public readonly Term<T3> Arg3;
            /// <summary>
            /// Fourth argument
            /// </summary>
            public readonly Term<T4> Arg4;

            /// <inheritdoc />
            public override Predicate Predicate => Primitive;

            /// <inheritdoc />
            public Goal(PrimitivePredicate<T1, T2, T3, T4> primitive, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4)
                : base(new Term[] { arg1, arg2, arg3, arg4 })
            {
                Primitive = primitive;
                Arg1 = arg1;
                Arg2 = arg2;
                Arg3 = arg3;
                Arg4 = arg4;
            }

            internal override Interpreter.Goal RenameArguments(Substitution s)
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

    /// <summary>
    /// A primitive predicate (one computed by code rather than matching to a table) with one argument
    /// </summary>
    /// <typeparam name="T1">Type of the predicate's first argument</typeparam>
    /// <typeparam name="T2">Type of the predicate's second argument</typeparam>
    /// <typeparam name="T3">Type of the predicate's third argument</typeparam>
    /// <typeparam name="T4">Type of the predicate's fourth argument</typeparam>
    /// <typeparam name="T5">Type of the predicate's fifth argument</typeparam>
    public abstract class PrimitivePredicate<T1, T2, T3, T4, T5> : Predicate
    {
        /// <inheritdoc />
        protected PrimitivePredicate(string name) : base(name)
        {
        }

        /// <summary>
        /// Return a goal representing a call to this primitive.
        /// This is the abstract syntax tree representing the call, not the internal form used in the interpreter
        /// </summary>
        public Interpreter.Goal this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5] => new Goal(this, arg1, arg2, arg3, arg4, arg5);

        /// <summary>
        /// Map a Goal (AST for call) to a Call object (the internal form used by the interpreter)
        /// </summary>
        public abstract Call MakeCall(Goal g, GoalAnalyzer tc);

        /// <summary>
        /// Custom goal class for this particular primitive
        /// </summary>
        public class Goal : Interpreter.Goal
        {
            /// <summary>
            /// The primitive being called
            /// </summary>
            public readonly PrimitivePredicate<T1, T2, T3, T4, T5> Primitive;
            /// <summary>
            /// First argument
            /// </summary>
            public readonly Term<T1> Arg1;
            /// <summary>
            /// Second argument
            /// </summary>
            public readonly Term<T2> Arg2;
            /// <summary>
            /// Third argument
            /// </summary>
            public readonly Term<T3> Arg3;
            /// <summary>
            /// Fourth argument
            /// </summary>
            public readonly Term<T4> Arg4;
            /// <summary>
            /// Fifth argument
            /// </summary>
            public readonly Term<T5> Arg5;

            /// <inheritdoc />
            public override Predicate Predicate => Primitive;

            /// <inheritdoc />
            public Goal(PrimitivePredicate<T1, T2, T3, T4, T5> primitive, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5)
                : base(new Term[] { arg1, arg2, arg3, arg4, arg5 })
            {
                Primitive = primitive;
                Arg1 = arg1;
                Arg2 = arg2;
                Arg3 = arg3;
                Arg4 = arg4;
                Arg5 = arg5;
            }

            internal override Interpreter.Goal RenameArguments(Substitution s)
                => new Goal(Primitive, s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3), s.Substitute(Arg4), s.Substitute(Arg5));

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
    /// <typeparam name="T5">Type of the predicate's fifth argument</typeparam>
    /// <typeparam name="T6">Type of the predicate's sixth argument</typeparam>
    public abstract class PrimitivePredicate<T1, T2, T3, T4, T5, T6> : Predicate
    {
        /// <inheritdoc />
        protected PrimitivePredicate(string name) : base(name)
        {
        }

        /// <summary>
        /// Return a goal representing a call to this primitive.
        /// This is the abstract syntax tree representing the call, not the internal form used in the interpreter
        /// </summary>
        public Interpreter.Goal this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5, Term<T6> arg6] => new Goal(this, arg1, arg2, arg3, arg4, arg5, arg6);

        /// <summary>
        /// Map a Goal (AST for call) to a Call object (the internal form used by the interpreter)
        /// </summary>
        public abstract Call MakeCall(Goal g, GoalAnalyzer tc);

        /// <summary>
        /// Custom goal class for this particular primitive
        /// </summary>
        public class Goal : Interpreter.Goal
        {
            /// <summary>
            /// The primitive being called
            /// </summary>
            public readonly PrimitivePredicate<T1, T2, T3, T4, T5, T6> Primitive;
            /// <summary>
            /// First argument
            /// </summary>
            public readonly Term<T1> Arg1;
            /// <summary>
            /// Second argument
            /// </summary>
            public readonly Term<T2> Arg2;
            /// <summary>
            /// Third argument
            /// </summary>
            public readonly Term<T3> Arg3;
            /// <summary>
            /// Fourth argument
            /// </summary>
            public readonly Term<T4> Arg4;
            /// <summary>
            /// Fifth argument
            /// </summary>
            public readonly Term<T5> Arg5;
            /// <summary>
            /// Sixth argument
            /// </summary>
            public readonly Term<T6> Arg6;

            /// <inheritdoc />
            public override Predicate Predicate => Primitive;

            /// <inheritdoc />
            public Goal(PrimitivePredicate<T1, T2, T3, T4, T5, T6> primitive, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5, Term<T6> arg6)
                : base(new Term[] { arg1, arg2, arg3, arg4, arg5, arg6 })
            {
                Primitive = primitive;
                Arg1 = arg1;
                Arg2 = arg2;
                Arg3 = arg3;
                Arg4 = arg4;
                Arg5 = arg5;
                Arg6 = arg6;
            }

            internal override Interpreter.Goal RenameArguments(Substitution s)
                => new Goal(Primitive, s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3), s.Substitute(Arg4), s.Substitute(Arg5), s.Substitute(Arg6));

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
    /// <typeparam name="T5">Type of the predicate's fifth argument</typeparam>
    /// <typeparam name="T6">Type of the predicate's sixth argument</typeparam>
    /// <typeparam name="T7">Type of the predicate's seventh argument</typeparam>
    public abstract class PrimitivePredicate<T1, T2, T3, T4, T5, T6, T7> : Predicate
    {
        /// <inheritdoc />
        protected PrimitivePredicate(string name) : base(name)
        {
        }

        /// <summary>
        /// Return a goal representing a call to this primitive.
        /// This is the abstract syntax tree representing the call, not the internal form used in the interpreter
        /// </summary>
        public Interpreter.Goal this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5, Term<T6> arg6, Term<T7> arg7] => new Goal(this, arg1, arg2, arg3, arg4, arg5, arg6, arg7);

        /// <summary>
        /// Map a Goal (AST for call) to a Call object (the internal form used by the interpreter)
        /// </summary>
        public abstract Call MakeCall(Goal g, GoalAnalyzer tc);

        /// <summary>
        /// Custom goal class for this particular primitive
        /// </summary>
        public class Goal : Interpreter.Goal
        {
            /// <summary>
            /// The primitive being called
            /// </summary>
            public readonly PrimitivePredicate<T1, T2, T3, T4, T5, T6, T7> Primitive;
            /// <summary>
            /// First argument
            /// </summary>
            public readonly Term<T1> Arg1;
            /// <summary>
            /// Second argument
            /// </summary>
            public readonly Term<T2> Arg2;
            /// <summary>
            /// Third argument
            /// </summary>
            public readonly Term<T3> Arg3;
            /// <summary>
            /// Fourth argument
            /// </summary>
            public readonly Term<T4> Arg4;
            /// <summary>
            /// Fifth argument
            /// </summary>
            public readonly Term<T5> Arg5;
            /// <summary>
            /// Sixth argument
            /// </summary>
            public readonly Term<T6> Arg6;
            /// <summary>
            /// Seventh argument
            /// </summary>
            public readonly Term<T7> Arg7;

            /// <inheritdoc />
            public override Predicate Predicate => Primitive;

            /// <inheritdoc />
            public Goal(PrimitivePredicate<T1, T2, T3, T4, T5, T6, T7> primitive, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5, Term<T6> arg6, Term<T7> arg7)
                : base(new Term[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7 })
            {
                Primitive = primitive;
                Arg1 = arg1;
                Arg2 = arg2;
                Arg3 = arg3;
                Arg4 = arg4;
                Arg5 = arg5;
                Arg6 = arg6;
                Arg7 = arg7;
            }

            internal override Interpreter.Goal RenameArguments(Substitution s)
                => new Goal(Primitive, s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3), s.Substitute(Arg4), s.Substitute(Arg5), s.Substitute(Arg6), s.Substitute(Arg7));

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
    /// <typeparam name="T5">Type of the predicate's fifth argument</typeparam>
    /// <typeparam name="T6">Type of the predicate's sixth argument</typeparam>
    /// <typeparam name="T7">Type of the predicate's seventh argument</typeparam>
    /// <typeparam name="T8">Type of the predicate's eighth argument</typeparam>
    public abstract class PrimitivePredicate<T1, T2, T3, T4, T5, T6, T7, T8> : Predicate
    {
        /// <inheritdoc />
        protected PrimitivePredicate(string name) : base(name)
        {
        }

        /// <summary>
        /// Return a goal representing a call to this primitive.
        /// This is the abstract syntax tree representing the call, not the internal form used in the interpreter
        /// </summary>
        public Interpreter.Goal this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5, Term<T6> arg6, Term<T7> arg7, Term<T8> arg8] => new Goal(this, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);

        /// <summary>
        /// Map a Goal (AST for call) to a Call object (the internal form used by the interpreter)
        /// </summary>
        public abstract Call MakeCall(Goal g, GoalAnalyzer tc);

        /// <summary>
        /// Custom goal class for this particular primitive
        /// </summary>
        public class Goal : Interpreter.Goal
        {
            /// <summary>
            /// The primitive being called
            /// </summary>
            public readonly PrimitivePredicate<T1, T2, T3, T4, T5, T6, T7, T8> Primitive;
            /// <summary>
            /// First argument
            /// </summary>
            public readonly Term<T1> Arg1;
            /// <summary>
            /// Second argument
            /// </summary>
            public readonly Term<T2> Arg2;
            /// <summary>
            /// Third argument
            /// </summary>
            public readonly Term<T3> Arg3;
            /// <summary>
            /// Fourth argument
            /// </summary>
            public readonly Term<T4> Arg4;
            /// <summary>
            /// Fifth argument
            /// </summary>
            public readonly Term<T5> Arg5;
            /// <summary>
            /// Sixth argument
            /// </summary>
            public readonly Term<T6> Arg6;
            /// <summary>
            /// Seventh argument
            /// </summary>
            public readonly Term<T7> Arg7;
            /// <summary>
            /// Eighth argument
            /// </summary>
            public readonly Term<T8> Arg8;

            /// <inheritdoc />
            public override Predicate Predicate => Primitive;

            /// <inheritdoc />
            public Goal(PrimitivePredicate<T1, T2, T3, T4, T5, T6, T7, T8> primitive, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5, Term<T6> arg6, Term<T7> arg7, Term<T8> arg8)
                : base(new Term[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 })
            {
                Primitive = primitive;
                Arg1 = arg1;
                Arg2 = arg2;
                Arg3 = arg3;
                Arg4 = arg4;
                Arg5 = arg5;
                Arg6 = arg6;
                Arg7 = arg7;
                Arg8 = arg8;
            }

            internal override Interpreter.Goal RenameArguments(Substitution s)
                => new Goal(Primitive, s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3), s.Substitute(Arg4), s.Substitute(Arg5), s.Substitute(Arg6), s.Substitute(Arg7), s.Substitute(Arg8));

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
