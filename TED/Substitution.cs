using System.Collections.Generic;

namespace TED
{
    public class Substitution
    {
        private Dictionary<AnyTerm, AnyTerm> substitutions = new Dictionary<AnyTerm, AnyTerm>();
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
                substitutions[original] = substitution;
        }

        public void ReplaceWith<T>(Var<T> old, Term<T> newV) => substitutions[old] = newV;

        public Term<T> Substitute<T>(Term<T> old)
        {
            switch (old)
            {
                case Var<T> v:
                    if (!substitutions.TryGetValue(v, out AnyTerm newV))
                    {
                        if (!AlphaConvert)
                            return old;
                        newV = v.Clone();
                        substitutions[v] = newV;
                    }

                    return (Term<T>)newV;

                case FunctionalExpression<T> _:
                    if (substitutions.TryGetValue(old, out var newValue))
                        return (Term<T>)newValue;
                    return old;

                case Constant<AnyGoal> goalTerm:
                    return (Term<T>)(object)(new Constant<AnyGoal>(goalTerm.Value.RenameArguments(this)));

                default:
                    return old;

            }
        }
    }
}
