using System;
using System.Collections.Generic;
using System.Linq;
using static TED.Preprocessor;

namespace TED
{
    /// <summary>
    /// Untyped base class of all definitions
    /// A definition is basically a macro
    /// It's a predicate that gets in-lined whenever it's called.
    /// Definitions currently only allow one rule.
    /// </summary>
    public abstract class Definition : AnyPredicate
    {
        public AnyGoal[]? Body;

        protected Definition(string name) : base(name)
        {
        }

        public abstract class AnyDefinitionGoal : AnyGoal
        {
            public readonly Definition Definition;

            protected AnyDefinitionGoal(Definition definition, AnyTerm[] arguments) : base(arguments)
            {
                Definition = definition;
            }

            public override AnyPredicate Predicate => Definition;

            public abstract Substitution MakeSubstitution();

            public IEnumerable<AnyGoal> Expand()
            {
                if (Definition.Body == null)
                    throw new Exception($"No body defined for definition {Definition.Name}");
                var s = MakeSubstitution();
                return Definition.Body.Select(g => g.RenameArguments(s));
            }

            internal override AnyCall MakeCall(GoalAnalyzer ga)
            {
                throw new InvalidOperationException("Definitions are not directly callable");
            }
        }
    }

    /// <summary>
    /// A one-argument predicate defined by a single rule.
    /// Calls to the predicate are always inlined, so the predicate doesn't have to have a finite extension.
    /// </summary>
    public class Definition<T1> : Definition
    {
        public Definition(string name, Var<T1> arg1) : base(name)
        {
            Arg1 = arg1;
        }

        public readonly Var<T1> Arg1;

        /// <summary>
        /// Make a call to the predicate.  Since this is a definition, it will be inlined in the rule it's contained in.
        /// </summary>
        public Goal this[Term<T1> a1] => new Goal(this, a1);

        public Definition<T1> IfAndOnlyIf(params AnyGoal[] body)
        {
            if (Body != null)
                throw new InvalidOperationException($"Attempt to add a second definition to {this.Name}");
            Body = CanonicalizeGoals(body).ToArray();
            return this;
        }

        public class Goal : AnyDefinitionGoal
        {
            public readonly Term<T1> Arg1;

            public Goal(Definition<T1> definition, Term<T1> arg1) : base(definition, new AnyTerm[] { arg1 })
            {
                Arg1 = arg1;
            }

            internal override AnyGoal RenameArguments(Substitution s) =>
                new Goal((Definition<T1>)Definition, s.Substitute(Arg1));

            public override Substitution MakeSubstitution()
            {
                var s = new Substitution(true);
                var d = (Definition<T1>)Definition;
                s.ReplaceWith(d.Arg1, Arg1);
                return s;
            }
        }
    }

    public class Definition<T1, T2> : Definition
    {
        public Definition(string name, Var<T1> arg1, Var<T2> arg2) : base(name)
        {
            Arg1 = arg1;
            Arg2 = arg2;
        }

        public readonly Var<T1> Arg1;
        public readonly Var<T2> Arg2;

        public Goal this[Term<T1> a1, Term<T2> a2] => new Goal(this, a1, a2);

        public Definition<T1, T2> IfAndOnlyIf(params AnyGoal[] body)
        {
            if (Body != null)
                throw new InvalidOperationException($"Attempt to add a second definition to {this.Name}");
            Body = CanonicalizeGoals(body).ToArray();
            return this;
        }

        /// <summary>
        /// Make a call to the predicate.  Since this is a definition, it will be inlined in the rule it's contained in.
        /// </summary>

        public class Goal : AnyDefinitionGoal
        {
            public readonly Term<T1> Arg1;
            public readonly Term<T2> Arg2;

            public Goal(Definition<T1, T2> definition, Term<T1> arg1, Term<T2> arg2) : base(definition,
                new AnyTerm[] { arg1, arg2 })
            {
                Arg1 = arg1;
                Arg2 = arg2;
            }

            internal override AnyGoal RenameArguments(Substitution s) => new Goal((Definition<T1, T2>)Definition,
                s.Substitute(Arg1), s.Substitute(Arg2));

            public override Substitution MakeSubstitution()
            {
                var s = new Substitution(true);
                var d = (Definition<T1, T2>)Definition;
                s.ReplaceWith(d.Arg1, Arg1);
                s.ReplaceWith(d.Arg2, Arg2);
                return s;
            }
        }
    }

    public class Definition<T1, T2, T3> : Definition
    {
        public Definition(string name, Var<T1> arg1, Var<T2> arg2, Var<T3> arg3) : base(name)
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
        }

        public readonly Var<T1> Arg1;
        public readonly Var<T2> Arg2;
        public readonly Var<T3> Arg3;

        /// <summary>
        /// Make a call to the predicate.  Since this is a definition, it will be inlined in the rule it's contained in.
        /// </summary>

        public Goal this[Term<T1> a1, Term<T2> a2, Term<T3> a3] => new Goal(this, a1, a2, a3);

        public Definition<T1, T2, T3> IfAndOnlyIf(params AnyGoal[] body)
        {
            if (Body != null)
                throw new InvalidOperationException($"Attempt to add a second definition to {this.Name}");
            Body = CanonicalizeGoals(body).ToArray();
            return this;
        }

        public class Goal : AnyDefinitionGoal
        {
            public readonly Term<T1> Arg1;
            public readonly Term<T2> Arg2;
            public readonly Term<T3> Arg3;

            public Goal(Definition<T1, T2, T3> definition, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3) : base(
                definition, new AnyTerm[] { arg1, arg2, arg3 })
            {
                Arg1 = arg1;
                Arg2 = arg2;
                Arg3 = arg3;
            }

            internal override AnyGoal RenameArguments(Substitution s) => new Goal((Definition<T1, T2, T3>)Definition,
                s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3));

            public override Substitution MakeSubstitution()
            {
                var s = new Substitution(true);
                var d = (Definition<T1,T2,T3>)Definition;
                s.ReplaceWith(d.Arg1, Arg1);
                s.ReplaceWith(d.Arg2, Arg2);
                s.ReplaceWith(d.Arg3, Arg3);
                return s;
            }
        }
    }

    public class Definition<T1, T2, T3, T4> : Definition
    {
        public Definition(string name, Var<T1> arg1, Var<T2> arg2, Var<T3> arg3, Var<T4> arg4) : base(name)
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
        }

        public readonly Var<T1> Arg1;
        public readonly Var<T2> Arg2;
        public readonly Var<T3> Arg3;
        public readonly Var<T4> Arg4;

        /// <summary>
        /// Make a call to the predicate.  Since this is a definition, it will be inlined in the rule it's contained in.
        /// </summary>

        public Goal this[Term<T1> a1, Term<T2> a2, Term<T3> a3, Term<T4> a4] => new Goal(this, a1, a2, a3, a4);

        public Definition<T1, T2, T3, T4> IfAndOnlyIf(params AnyGoal[] body)
        {
            if (Body != null)
                throw new InvalidOperationException($"Attempt to add a second definition to {this.Name}");
            Body = CanonicalizeGoals(body).ToArray();
            return this;
        }

        public class Goal : AnyDefinitionGoal
        {
            public readonly Term<T1> Arg1;
            public readonly Term<T2> Arg2;
            public readonly Term<T3> Arg3;
            public readonly Term<T4> Arg4;

            public Goal(Definition<T1, T2, T3, T4> definition, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3,
                Term<T4> arg4) : base(
                definition, new AnyTerm[] { arg1, arg2, arg3, arg4 })
            {
                Arg1 = arg1;
                Arg2 = arg2;
                Arg3 = arg3;
                Arg4 = arg4;
            }

            internal override AnyGoal RenameArguments(Substitution s)
                => new Goal((Definition<T1, T2, T3, T4>)Definition,
                    s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3), s.Substitute(Arg4));

            public override Substitution MakeSubstitution()
            {
                var s = new Substitution(true);
                var d = (Definition<T1,T2,T3,T4>)Definition;
                s.ReplaceWith(d.Arg1, Arg1);
                s.ReplaceWith(d.Arg2, Arg2);
                s.ReplaceWith(d.Arg3, Arg3);
                s.ReplaceWith(d.Arg4, Arg4);
                return s;
            }
        }
    }


    public class Definition<T1,T2,T3,T4,T5> : Definition
    {
        public Definition(string name, Var<T1> arg1, Var<T2> arg2, Var<T3> arg3, Var<T4> arg4, Var<T5> arg5) : base(name)
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
        }

        public readonly Var<T1> Arg1;
        public readonly Var<T2> Arg2;
        public readonly Var<T3> Arg3;
        public readonly Var<T4> Arg4;
        public readonly Var<T5> Arg5;

        /// <summary>
        /// Make a call to the predicate.  Since this is a definition, it will be inlined in the rule it's contained in.
        /// </summary>

        public Goal this[Term<T1> a1, Term<T2> a2, Term<T3> a3, Term<T4> a4, Term<T5> a5] 
            => new Goal(this, a1, a2, a3, a4, a5);

        public Definition<T1,T2,T3,T4,T5> IfAndOnlyIf(params AnyGoal[] body)
        {
            if (Body != null)
                throw new InvalidOperationException($"Attempt to add a second definition to {this.Name}");
            Body = CanonicalizeGoals(body).ToArray();
            return this;
        }

        public class Goal : AnyDefinitionGoal
        {
            public readonly Term<T1> Arg1;
            public readonly Term<T2> Arg2;
            public readonly Term<T3> Arg3;
            public readonly Term<T4> Arg4;
            public readonly Term<T5> Arg5;

            public Goal(Definition<T1,T2,T3,T4,T5> definition, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3,
                Term<T4> arg4, Term<T5> arg5) : base(
                definition, new AnyTerm[] { arg1, arg2, arg3, arg4, arg5 })
            {
                Arg1 = arg1;
                Arg2 = arg2;
                Arg3 = arg3;
                Arg4 = arg4;
                Arg5 = arg5;
            }

            internal override AnyGoal RenameArguments(Substitution s)
                => new Goal((Definition<T1,T2,T3,T4,T5>)Definition,
                    s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3), s.Substitute(Arg4), s.Substitute(Arg5));

            public override Substitution MakeSubstitution()
            {
                var s = new Substitution(true);
                var d = (Definition<T1,T2,T3,T4,T5>)Definition;
                s.ReplaceWith(d.Arg1, Arg1);
                s.ReplaceWith(d.Arg2, Arg2);
                s.ReplaceWith(d.Arg3, Arg3);
                s.ReplaceWith(d.Arg4, Arg4);
                s.ReplaceWith(d.Arg5, Arg5);
                return s;
            }
        }
    }
        public class Definition<T1,T2,T3,T4,T5,T6> : Definition
    {
        public Definition(string name, Var<T1> arg1, Var<T2> arg2, Var<T3> arg3, Var<T4> arg4, Var<T5> arg5, Var<T6> arg6) : base(name)
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
            Arg6 = arg6;
        }

        public readonly Var<T1> Arg1;
        public readonly Var<T2> Arg2;
        public readonly Var<T3> Arg3;
        public readonly Var<T4> Arg4;
        public readonly Var<T5> Arg5;
        public readonly Var<T6> Arg6;

        /// <summary>
        /// Make a call to the predicate.  Since this is a definition, it will be inlined in the rule it's contained in.
        /// </summary>
        public Goal this[Term<T1> a1, Term<T2> a2, Term<T3> a3, Term<T4> a4, Term<T5> a5, Term<T6> a6] 
            => new Goal(this, a1, a2, a3, a4, a5, a6);

        public Definition<T1,T2,T3,T4,T5,T6> IfAndOnlyIf(params AnyGoal[] body)
        {
            if (Body != null)
                throw new InvalidOperationException($"Attempt to add a second definition to {this.Name}");
            Body = CanonicalizeGoals(body).ToArray();
            return this;
        }

        public class Goal : AnyDefinitionGoal
        {
            public readonly Term<T1> Arg1;
            public readonly Term<T2> Arg2;
            public readonly Term<T3> Arg3;
            public readonly Term<T4> Arg4;
            public readonly Term<T5> Arg5;
            public readonly Term<T6> Arg6;

            public Goal(Definition<T1,T2,T3,T4,T5,T6> definition, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3,
                Term<T4> arg4, Term<T5> arg5, Term<T6> arg6) : base(
                definition, new AnyTerm[] { arg1, arg2, arg3, arg4, arg5, arg6 })
            {
                Arg1 = arg1;
                Arg2 = arg2;
                Arg3 = arg3;
                Arg4 = arg4;
                Arg5 = arg5;
                Arg6 = arg6;
            }

            internal override AnyGoal RenameArguments(Substitution s)
                => new Goal((Definition<T1,T2,T3,T4,T5,T6>)Definition,
                    s.Substitute(Arg1), s.Substitute(Arg2), s.Substitute(Arg3), s.Substitute(Arg4), s.Substitute(Arg5), s.Substitute(Arg6));

            public override Substitution MakeSubstitution()
            {
                var s = new Substitution(true);
                var d = (Definition<T1,T2,T3,T4,T5,T6>)Definition;
                s.ReplaceWith(d.Arg1, Arg1);
                s.ReplaceWith(d.Arg2, Arg2);
                s.ReplaceWith(d.Arg3, Arg3);
                s.ReplaceWith(d.Arg4, Arg4);
                s.ReplaceWith(d.Arg5, Arg5);
                s.ReplaceWith(d.Arg6, Arg6);
                return s;
            }
        }
    }
}
