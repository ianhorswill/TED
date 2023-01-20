using System.Collections.Generic;

namespace TED
{
    public class Substitution
    {
        internal Dictionary<AnyTerm, AnyTerm> Substitutions = new Dictionary<AnyTerm, AnyTerm>();
        /// <summary>
        /// If true, all variables not otherwise substituted should be substituted with copies of themselves.
        /// </summary>
        public readonly bool AlphaConvert;

        public Substitution(bool alphaConvert)
        {
            AlphaConvert = alphaConvert;
        }

        public Substitution(IEnumerable<(AnyTerm, AnyTerm)> mappings, bool alphaConvert)
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
