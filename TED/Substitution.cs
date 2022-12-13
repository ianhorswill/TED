using System.Collections.Generic;

namespace TED
{
    public class Substitution
    {
        private Dictionary<AnyTerm, AnyTerm> substitutions = new Dictionary<AnyTerm, AnyTerm>();

        public void ReplaceWith<T>(Var<T> old, Term<T> newV) => substitutions[old] = newV;

        public Term<T> Substitute<T>(Term<T> old)
        {
            if (!(old is Var<T> v))
            {
                if (!(old is Constant<AnyGoal> goalTerm))
                    return old;
                return (Term<T>)(object)(new Constant<AnyGoal>(goalTerm.Value.RenameArguments(this)));
            }
            if (!substitutions.TryGetValue(v, out AnyTerm newV))
            {
                newV = v.Clone();
                substitutions[v] = newV;
            }

            return (Term<T>)newV;
        }
    }
}
