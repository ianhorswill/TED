using System.Collections.Generic;

namespace TED.Preprocessing
{
    /// <summary>
    /// Represents a substitution of terms for other terms.
    /// Used for expanding the bodies of Definitions (replacing formal vars with actual arguments)
    /// and hoisting functional expressions (replacing FEs with temp vars)
    /// </summary>
    internal class Substitution
    {
        internal Dictionary<Term, Term> Substitutions = new Dictionary<Term, Term>();
        /// <summary>
        /// If true, all variables not otherwise substituted should be substituted with copies of themselves.
        /// </summary>
        public readonly bool AlphaConvert;

        public Substitution(bool alphaConvert)
        {
            AlphaConvert = alphaConvert;
        }

        public Substitution(IEnumerable<(Term, Term)> mappings, bool alphaConvert)
        {
            AlphaConvert = alphaConvert;
            foreach (var (original, substitution) in mappings)
                Substitutions[original] = substitution;
        }

        public void ReplaceWith<T>(Var<T> old, Term<T> newV) => Substitutions[old] = newV;

        public Term<T> Substitute<T>(Term<T> old)
        {
            return old.ApplySubstitution(this);
            //switch (old)
            //{
            //    case Var<T> v:
            //        if (!Substitutions.TryGetValue(v, out AnyTerm newV))
            //        {
            //            if (!AlphaConvert)
            //                return old;
            //            newV = v.Clone();
            //            Substitutions[v] = newV;
            //        }

            //        return (Term<T>)newV;

            //    case FunctionalExpression<T> _:
            //        if (Substitutions.TryGetValue(old, out var newValue))
            //            return (Term<T>)newValue;
            //        return old;

            //    case Constant<AnyGoal> goalTerm:
            //        return (Term<T>)(object)(new Constant<AnyGoal>(goalTerm.Value.RenameArguments(this)));

            //    default:
            //        return old;

            //}
        }

        /// <summary>
        /// Apply substitution to a variable, returning the variable it's mapped to or the original variable.
        /// </summary>
        internal Term<T> SubstituteVariable<T>(Var<T> v)
        {
            if (Substitutions.TryGetValue(v, out var sub))
                return (Term<T>)sub;
            if (!AlphaConvert)
                return v;
            var newV = v.Clone();
            Substitutions[v] = newV;
            return (Term<T>)newV;
        }
    }
}
