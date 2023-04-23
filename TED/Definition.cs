using System;
using System.Collections.Generic;
using System.Linq;
using TED.Interpreter;
using TED.Preprocessing;
using static TED.Preprocessing.Preprocessor;

namespace TED
{
    /// <summary>
    /// Untyped base class of all definitions
    /// A definition is basically a macro
    /// It's a predicate that gets in-lined whenever it's called.
    /// Definitions currently only allow one rule.
    /// </summary>
    public abstract class Definition : Predicate
    {
        /// <summary>
        /// Sequence of goals into which this definition should be expanded
        /// </summary>
        internal Goal[]? Body;

        /// <summary>
        /// Rules defining the Definition if it's defined by separate rules
        /// </summary>
        internal List<Rule>? Rules;

        /// <inheritdoc />
        protected Definition(string name) : base(name)
        {
        }

        internal static IEnumerable<Goal> SubstituteBody(IEnumerable<Goal> body, Substitution s) =>
            body.Select(g => g.RenameArguments(s).FoldConstant());


        internal Goal DefaultGoal;

        /// <summary>
        /// A call to this definition using it's "default" arguments
        /// </summary>
        public static implicit operator Goal(Definition p) => p.DefaultGoal;

        /// <summary>
        /// Goal representing a call to a definition
        /// This will get expanded in-line by the calling rule at preprocessing time
        /// </summary>
        public abstract class DefinitionGoal : Goal
        {
            /// <summary>
            /// The definition being called
            /// </summary>
            public readonly Definition Definition;

            /// <summary>
            /// Make a goal to call the definition with the specified arguments
            /// </summary>
            protected DefinitionGoal(Definition definition, Term[] arguments) : base(arguments)
            {
                Definition = definition;
            }

            /// <inheritdoc />
            public override Predicate Predicate => Definition;

            /// <summary>
            /// Make a substitution that will substitute the formal arguments of the definition
            /// with the actual arguments passed in this goal
            /// </summary>
            internal abstract Substitution MakeSubstitution();

            /// <summary>
            /// Generate a copy of the goals of the substitution with the actual arguments
            /// substituted for the formal arguments
            /// </summary>
            /// <returns>Substituted goals</returns>
            /// <exception cref="Exception">If not body has yet been specified for the definition</exception>
            public IEnumerable<Goal> Expand()
            {
                if (Definition.Rules != null)
                    return new[] { Language.Or[Definition.Rules.Select(r => Language.And[r.Expand(this)]).ToArray()] };
                if (Definition.Body == null)
                    throw new Exception($"No body defined for definition {Definition.Name}");
                var s = MakeSubstitution();
                return SubstituteBody(Definition.Body, s);
            }

            internal override Call MakeCall(GoalAnalyzer ga)
            {
                throw new InvalidOperationException("Definitions are not directly callable");
            }

            /// <summary>
            /// Add a new to the definition
            /// </summary>
            /// <param name="body"></param>
            public void If(params Goal[] body) => Definition.AddRule(this, CanonicalizeGoals(body).ToArray());

            /// <summary>
            /// Add a rule to the definition with no subgoals
            /// </summary>
            public void Fact() => If();
        }

        private void AddRule(DefinitionGoal head, Goal[] body)
        {
            if (Rules == null)
            {
                if (Body != null)
                    throw new InvalidOperationException(
                        $"You cannot add a rule to definition {Name} using If() because you've already added a definition for it using Is().");
                Rules = new List<Rule>();
            }
            Rules.Add(new Rule(head, body));
        }

        internal class Rule
        {
            public readonly Term[] FormalArguments;
            public Goal[] Body;

            public Rule(DefinitionGoal head, Goal[] body)
            {
                FormalArguments = head.Arguments;
                Body = body;
            }

            public Goal[] Expand(DefinitionGoal g)
            {
                var actualArguments = g.Arguments;
                var s = new Substitution(true);
                var equalities = new List<Goal>();
                for (var i = 0; i < actualArguments.Length; i++)
                {
                    var formal = FormalArguments[i];
                    var actual = actualArguments[i];

                    if (formal is IVariable)
                        // We can't type it because we know it's Term<T> but we don't know what T is
                        // but we know it passed the C# compiler's type checking, so the two must have the same T
                        s.ReplaceWithUntyped(formal, actual);
                    else if (actual is IVariable v)
                    {
                        // Formal is a variable, actual is a constant
                        equalities.Add(v.EquateTo(formal));
                    }
                    else
                    {
                        // formal and actual are both constants
                        if (!((IConstant)formal).IsSameConstant(actual))
                            return new Goal[] { Language.False };
                    }
                }

                return equalities.Concat(SubstituteBody(Body, s)).ToArray();
            }
        }
    }

    /// <summary>
    /// A one-argument predicate defined by a single rule.
    /// Calls to the predicate are always inlined, so the predicate doesn't have to have a finite extension.
    /// </summary>
    public class Definition<T1> : Definition
    {
        /// <summary>
        /// Make a one-argument predicate defined by a single rule.
        /// Calls to the predicate are always inlined, so the predicate doesn't have to have a finite extension.
        /// </summary>
        public Definition(string name, Var<T1> arg1) : base(name)
        {
            Arg1 = arg1;
        }

        /// <summary>
        /// First argument
        /// </summary>
        public readonly Var<T1> Arg1;

        /// <summary>
        /// Make a call to the predicate.  Since this is a definition, it will be inlined in the rule it's contained in.
        /// </summary>
        public Goal this[Term<T1> a1] => new Goal(this, a1);

        internal new Goal DefaultGoal => this[Arg1];

        /// <summary>
        /// Specify the sequence of goals into which calls to this definition should be transformed.
        /// </summary>
        /// <param name="body">Goals to substitute for the definition</param>
        /// <returns>The definition object</returns>
        /// <exception cref="InvalidOperationException">If a body has already been specified</exception>
        public Definition<T1> Is(params Interpreter.Goal[] body)
        {
            if (Body != null)
                throw new InvalidOperationException($"Attempt to add a second definition to {this.Name}");
            Body = CanonicalizeGoals(body).ToArray();
            return this;
        }

        /// <inheritdoc />
        public sealed class Goal : DefinitionGoal
        {
            private readonly Term<T1> arg1;

            /// <inheritdoc />
            public Goal(Definition<T1> definition, Term<T1> arg1) : base(definition, new Term[] { arg1 })
            {
                this.arg1 = arg1;
            }

            internal override Interpreter.Goal RenameArguments(Substitution s) =>
                new Goal((Definition<T1>)Definition, s.Substitute(arg1));

            internal override Substitution MakeSubstitution()
            {
                var s = new Substitution(true);
                var d = (Definition<T1>)Definition;
                s.ReplaceWith(d.Arg1, arg1);
                return s;
            }
        }
    }

    /// <summary>
    /// A two-argument predicate defined by a single rule.
    /// Calls to the predicate are always inlined, so the predicate doesn't have to have a finite extension.
    /// </summary>

    public sealed class Definition<T1, T2> : Definition
    {
        /// <summary>
        /// Make a two-argument predicate defined by a single rule.
        /// Calls to the predicate are always inlined, so the predicate doesn't have to have a finite extension.
        /// </summary>
        public Definition(string name, Var<T1> arg1, Var<T2> arg2) : base(name)
        {
            Arg1 = arg1;
            Arg2 = arg2;
        }

        /// <summary>
        /// First Argument
        /// </summary>
        public readonly Var<T1> Arg1;
        /// <summary>
        /// Second Argument
        /// </summary>
        public readonly Var<T2> Arg2;

        /// <summary>
        /// Make a call to the predicate.  Since this is a definition, it will be inlined in the rule it's contained in.
        /// </summary>
        public Goal this[Term<T1> a1, Term<T2> a2] => new Goal(this, a1, a2);

        internal new Goal DefaultGoal => this[Arg1, Arg2];

        /// <summary>
        /// Specify the sequence of goals into which calls to this definition should be transformed.
        /// </summary>
        /// <param name="body">Goals to substitute for the definition</param>
        /// <returns>The definition object</returns>
        /// <exception cref="InvalidOperationException">If a body has already been specified</exception>
        public Definition<T1, T2> Is(params Interpreter.Goal[] body)
        {
            if (Body != null)
                throw new InvalidOperationException($"Attempt to add a second definition to {this.Name}");
            Body = CanonicalizeGoals(body).ToArray();
            return this;
        }

        /// <summary>
        /// Make a call to the predicate.  Since this is a definition, it will be inlined in the rule it's contained in.
        /// </summary>
        public class Goal : DefinitionGoal
        {
            /// <summary>
            /// First argument
            /// </summary>
            private readonly Term<T1> arg1;
            /// <summary>
            /// Second argument
            /// </summary>
            private readonly Term<T2> arg2;

            /// <inheritdoc />
            public Goal(Definition<T1, T2> definition, Term<T1> arg1, Term<T2> arg2) : base(definition,
                new Term[] { arg1, arg2 })
            {
                this.arg1 = arg1;
                this.arg2 = arg2;
            }

            internal override Interpreter.Goal RenameArguments(Substitution s) => new Goal((Definition<T1, T2>)Definition,
                s.Substitute(arg1), s.Substitute(arg2));

            internal override Substitution MakeSubstitution()
            {
                var s = new Substitution(true);
                var d = (Definition<T1, T2>)Definition;
                s.ReplaceWith(d.Arg1, arg1);
                s.ReplaceWith(d.Arg2, arg2);
                return s;
            }
        }
    }

    /// <summary>
    /// A three-argument predicate defined by a single rule.
    /// Calls to the predicate are always inlined, so the predicate doesn't have to have a finite extension.
    /// </summary>
    public class Definition<T1, T2, T3> : Definition
    {
        /// <summary>
        /// Make a three-argument predicate defined by a single rule.
        /// Calls to the predicate are always inlined, so the predicate doesn't have to have a finite extension.
        /// </summary>
        public Definition(string name, Var<T1> arg1, Var<T2> arg2, Var<T3> arg3) : base(name)
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
        }

        /// <summary>
        /// First Argument
        /// </summary>
        public readonly Var<T1> Arg1;
        /// <summary>
        /// Second Argument
        /// </summary>
        public readonly Var<T2> Arg2;
        /// <summary>
        /// Third argument
        /// </summary>
        public readonly Var<T3> Arg3;

        /// <summary>
        /// Make a call to the predicate.  Since this is a definition, it will be inlined in the rule it's contained in.
        /// </summary>

        /// <summary>
        /// Make a call to the predicate.  Since this is a definition, it will be inlined in the rule it's contained in.
        /// </summary>
        public Goal this[Term<T1> a1, Term<T2> a2, Term<T3> a3] => new Goal(this, a1, a2, a3);

        internal new Goal DefaultGoal => this[Arg1, Arg2, Arg3];

        /// <summary>
        /// Specify the sequence of goals into which calls to this definition should be transformed.
        /// </summary>
        /// <param name="body">Goals to substitute for the definition</param>
        /// <returns>The definition object</returns>
        /// <exception cref="InvalidOperationException">If a body has already been specified</exception>
        public Definition<T1, T2, T3> Is(params Interpreter.Goal[] body)
        {
            if (Body != null)
                throw new InvalidOperationException($"Attempt to add a second definition to {this.Name}");
            Body = CanonicalizeGoals(body).ToArray();
            return this;
        }

        public class Goal : DefinitionGoal
        {
            /// <summary>
            /// First argument
            /// </summary>
            private readonly Term<T1> arg1;
            /// <summary>
            /// Second argument
            /// </summary>
            private readonly Term<T2> arg2;
            /// <summary>
            /// Third argument
            /// </summary>
            private readonly Term<T3> arg3;

            /// <inheritdoc />
            public Goal(Definition<T1, T2, T3> definition, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3) : base(
                definition, new Term[] { arg1, arg2, arg3 })
            {
                this.arg1 = arg1;
                this.arg2 = arg2;
                this.arg3 = arg3;
            }

            internal override Interpreter.Goal RenameArguments(Substitution s) => new Goal((Definition<T1, T2, T3>)Definition,
                s.Substitute(arg1), s.Substitute(arg2), s.Substitute(arg3));

            internal override Substitution MakeSubstitution()
            {
                var s = new Substitution(true);
                var d = (Definition<T1,T2,T3>)Definition;
                s.ReplaceWith(d.Arg1, arg1);
                s.ReplaceWith(d.Arg2, arg2);
                s.ReplaceWith(d.Arg3, arg3);
                return s;
            }
        }
    }

    /// <summary>
    /// A four-argument predicate defined by a single rule.
    /// Calls to the predicate are always inlined, so the predicate doesn't have to have a finite extension.
    /// </summary>
    public class Definition<T1, T2, T3, T4> : Definition
    {
        /// <summary>
        /// Make a four-argument predicate defined by a single rule.
        /// Calls to the predicate are always inlined, so the predicate doesn't have to have a finite extension.
        /// </summary>
        public Definition(string name, Var<T1> arg1, Var<T2> arg2, Var<T3> arg3, Var<T4> arg4) : base(name)
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
        }

        /// <summary>
        /// First Argument
        /// </summary>
        public readonly Var<T1> Arg1;
        /// <summary>
        /// Second Argument
        /// </summary>
        public readonly Var<T2> Arg2;
        /// <summary>
        /// Third argument
        /// </summary>
        public readonly Var<T3> Arg3;
        /// <summary>
        /// Fourth argument
        /// </summary>
        public readonly Var<T4> Arg4;

        /// <summary>
        /// Make a call to the predicate.  Since this is a definition, it will be inlined in the rule it's contained in.
        /// </summary>

        /// <summary>
        /// Make a call to the predicate.  Since this is a definition, it will be inlined in the rule it's contained in.
        /// </summary>
        public Goal this[Term<T1> a1, Term<T2> a2, Term<T3> a3, Term<T4> a4] => new Goal(this, a1, a2, a3, a4);

        internal new Goal DefaultGoal => this[Arg1, Arg2, Arg3, Arg4];

        /// <summary>
        /// Specify the sequence of goals into which calls to this definition should be transformed.
        /// </summary>
        /// <param name="body">Goals to substitute for the definition</param>
        /// <returns>The definition object</returns>
        /// <exception cref="InvalidOperationException">If a body has already been specified</exception>
        public Definition<T1, T2, T3, T4> Is(params Interpreter.Goal[] body)
        {
            if (Body != null)
                throw new InvalidOperationException($"Attempt to add a second definition to {this.Name}");
            Body = CanonicalizeGoals(body).ToArray();
            return this;
        }

        public class Goal : DefinitionGoal
        {
            /// <summary>
            /// First argument
            /// </summary>
            private readonly Term<T1> arg1;
            /// <summary>
            /// Second argument
            /// </summary>
            private readonly Term<T2> arg2;
            /// <summary>
            /// Third argument
            /// </summary>
            private readonly Term<T3> arg3;
            /// <summary>
            /// Fourth argument
            /// </summary>
            private readonly Term<T4> arg4;

            /// <inheritdoc />
            public Goal(Definition<T1, T2, T3, T4> definition, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3,
                Term<T4> arg4) : base(
                definition, new Term[] { arg1, arg2, arg3, arg4 })
            {
                this.arg1 = arg1;
                this.arg2 = arg2;
                this.arg3 = arg3;
                this.arg4 = arg4;
            }

            internal override Interpreter.Goal RenameArguments(Substitution s)
                => new Goal((Definition<T1, T2, T3, T4>)Definition,
                    s.Substitute(arg1), s.Substitute(arg2), s.Substitute(arg3), s.Substitute(arg4));

            internal override Substitution MakeSubstitution()
            {
                var s = new Substitution(true);
                var d = (Definition<T1,T2,T3,T4>)Definition;
                s.ReplaceWith(d.Arg1, arg1);
                s.ReplaceWith(d.Arg2, arg2);
                s.ReplaceWith(d.Arg3, arg3);
                s.ReplaceWith(d.Arg4, arg4);
                return s;
            }
        }
    }


    /// <summary>
    /// A five-argument predicate defined by a single rule.
    /// Calls to the predicate are always inlined, so the predicate doesn't have to have a finite extension.
    /// </summary>
    public class Definition<T1,T2,T3,T4,T5> : Definition
    {
        /// <summary>
        /// Make a five-argument predicate defined by a single rule.
        /// Calls to the predicate are always inlined, so the predicate doesn't have to have a finite extension.
        /// </summary>
        public Definition(string name, Var<T1> arg1, Var<T2> arg2, Var<T3> arg3, Var<T4> arg4, Var<T5> arg5) : base(name)
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
        }

        /// <summary>
        /// First Argument
        /// </summary>
        public readonly Var<T1> Arg1;
        /// <summary>
        /// Second Argument
        /// </summary>
        public readonly Var<T2> Arg2;
        /// <summary>
        /// Third argument
        /// </summary>
        public readonly Var<T3> Arg3;
        /// <summary>
        /// Fourth argument
        /// </summary>
        public readonly Var<T4> Arg4;
        /// <summary>
        /// Fifth argument
        /// </summary>
        public readonly Var<T5> Arg5;

        /// <summary>
        /// Make a call to the predicate.  Since this is a definition, it will be inlined in the rule it's contained in.
        /// </summary>

        /// <summary>
        /// Make a call to the predicate.  Since this is a definition, it will be inlined in the rule it's contained in.
        /// </summary>
        public Goal this[Term<T1> a1, Term<T2> a2, Term<T3> a3, Term<T4> a4, Term<T5> a5] 
            => new Goal(this, a1, a2, a3, a4, a5);

        internal new Goal DefaultGoal => this[Arg1, Arg2, Arg3, Arg4, Arg5];

        /// <summary>
        /// Specify the sequence of goals into which calls to this definition should be transformed.
        /// </summary>
        /// <param name="body">Goals to substitute for the definition</param>
        /// <returns>The definition object</returns>
        /// <exception cref="InvalidOperationException">If a body has already been specified</exception>
        public Definition<T1,T2,T3,T4,T5> Is(params Interpreter.Goal[] body)
        {
            if (Body != null)
                throw new InvalidOperationException($"Attempt to add a second definition to {this.Name}");
            Body = CanonicalizeGoals(body).ToArray();
            return this;
        }

        public class Goal : DefinitionGoal
        {
            /// <summary>
            /// First argument
            /// </summary>
            private readonly Term<T1> arg1;
            /// <summary>
            /// Second argument
            /// </summary>
            private readonly Term<T2> arg2;
            /// <summary>
            /// Third argument
            /// </summary>
            private readonly Term<T3> arg3;
            /// <summary>
            /// Fourth argument
            /// </summary>
            private readonly Term<T4> arg4;
            /// <summary>
            /// Fifth argument
            /// </summary>
            private readonly Term<T5> arg5;

            /// <inheritdoc />
            public Goal(Definition<T1,T2,T3,T4,T5> definition, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3,
                Term<T4> arg4, Term<T5> arg5) : base(
                definition, new Term[] { arg1, arg2, arg3, arg4, arg5 })
            {
                this.arg1 = arg1;
                this.arg2 = arg2;
                this.arg3 = arg3;
                this.arg4 = arg4;
                this.arg5 = arg5;
            }

            internal override Interpreter.Goal RenameArguments(Substitution s)
                => new Goal((Definition<T1,T2,T3,T4,T5>)Definition,
                    s.Substitute(arg1), s.Substitute(arg2), s.Substitute(arg3), s.Substitute(arg4), s.Substitute(arg5));

            internal override Substitution MakeSubstitution()
            {
                var s = new Substitution(true);
                var d = (Definition<T1,T2,T3,T4,T5>)Definition;
                s.ReplaceWith(d.Arg1, arg1);
                s.ReplaceWith(d.Arg2, arg2);
                s.ReplaceWith(d.Arg3, arg3);
                s.ReplaceWith(d.Arg4, arg4);
                s.ReplaceWith(d.Arg5, arg5);
                return s;
            }
        }
    }
    /// <summary>
    /// A six-argument predicate defined by a single rule.
    /// Calls to the predicate are always inlined, so the predicate doesn't have to have a finite extension.
    /// </summary>
    public class Definition<T1,T2,T3,T4,T5,T6> : Definition
    {
        /// <summary>
        /// Make a six-argument predicate defined by a single rule.
        /// Calls to the predicate are always inlined, so the predicate doesn't have to have a finite extension.
        /// </summary>
        public Definition(string name, Var<T1> arg1, Var<T2> arg2, Var<T3> arg3, Var<T4> arg4, Var<T5> arg5, Var<T6> arg6) : base(name)
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
            Arg6 = arg6;
        }

        /// <summary>
        /// First Argument
        /// </summary>
        public readonly Var<T1> Arg1;
        /// <summary>
        /// Second Argument
        /// </summary>
        public readonly Var<T2> Arg2;
        /// <summary>
        /// Third argument
        /// </summary>
        public readonly Var<T3> Arg3;
        /// <summary>
        /// Fourth argument
        /// </summary>
        public readonly Var<T4> Arg4;
        /// <summary>
        /// Fifth argument
        /// </summary>
        public readonly Var<T5> Arg5;
        /// <summary>
        /// Sixth argument
        /// </summary>
        public readonly Var<T6> Arg6;

        /// <summary>
        /// Make a call to the predicate.  Since this is a definition, it will be inlined in the rule it's contained in.
        /// </summary>
        public Goal this[Term<T1> a1, Term<T2> a2, Term<T3> a3, Term<T4> a4, Term<T5> a5, Term<T6> a6] 
            => new Goal(this, a1, a2, a3, a4, a5, a6);

        internal new Goal DefaultGoal => this[Arg1, Arg2, Arg3, Arg4, Arg5, Arg6];

        /// <summary>
        /// Specify the sequence of goals into which calls to this definition should be transformed.
        /// </summary>
        /// <param name="body">Goals to substitute for the definition</param>
        /// <returns>The definition object</returns>
        /// <exception cref="InvalidOperationException">If a body has already been specified</exception>
        public Definition<T1,T2,T3,T4,T5,T6> Is(params Interpreter.Goal[] body)
        {
            if (Body != null)
                throw new InvalidOperationException($"Attempt to add a second definition to {this.Name}");
            Body = CanonicalizeGoals(body).ToArray();
            return this;
        }

        public class Goal : DefinitionGoal
        {
            /// <summary>
            /// First argument
            /// </summary>
            private readonly Term<T1> arg1;
            /// <summary>
            /// Second argument
            /// </summary>
            private readonly Term<T2> arg2;
            /// <summary>
            /// Third argument
            /// </summary>
            private readonly Term<T3> arg3;
            /// <summary>
            /// Fourth argument
            /// </summary>
            private readonly Term<T4> arg4;
            /// <summary>
            /// Fourth argument
            /// </summary>
            private readonly Term<T5> arg5;
            /// <summary>
            /// Sixth argument
            /// </summary>
            private readonly Term<T6> arg6;

            /// <inheritdoc />
            public Goal(Definition<T1,T2,T3,T4,T5,T6> definition, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3,
                Term<T4> arg4, Term<T5> arg5, Term<T6> arg6) : base(
                definition, new Term[] { arg1, arg2, arg3, arg4, arg5, arg6 })
            {
                this.arg1 = arg1;
                this.arg2 = arg2;
                this.arg3 = arg3;
                this.arg4 = arg4;
                this.arg5 = arg5;
                this.arg6 = arg6;
            }

            internal override Interpreter.Goal RenameArguments(Substitution s)
                => new Goal((Definition<T1,T2,T3,T4,T5,T6>)Definition,
                    s.Substitute(arg1), s.Substitute(arg2), s.Substitute(arg3), s.Substitute(arg4), s.Substitute(arg5), s.Substitute(arg6));

            internal override Substitution MakeSubstitution()
            {
                var s = new Substitution(true);
                var d = (Definition<T1,T2,T3,T4,T5,T6>)Definition;
                s.ReplaceWith(d.Arg1, arg1);
                s.ReplaceWith(d.Arg2, arg2);
                s.ReplaceWith(d.Arg3, arg3);
                s.ReplaceWith(d.Arg4, arg4);
                s.ReplaceWith(d.Arg5, arg5);
                s.ReplaceWith(d.Arg6, arg6);
                return s;
            }
        }
    }
}
