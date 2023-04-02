namespace TED
{
    /// <summary>
    /// Base class for terms whose value is of type T but where the value is computed from other Terms by some function.
    /// </summary>
    public abstract class FunctionalExpression<T> : Term<T>, IFunctionalExpression
    {
        /// <inheritdoc />
        public (Term Expression, Term Var, Goal EvalGoal) HoistInfo()
        {
            var v = new Var<T>("temp");
            return (this, v, Language.Eval(v, this));
        }

        /// <inheritdoc />
        internal sealed override Term<T> ApplySubstitution(Substitution s)
        {
            if (s.Substitutions.TryGetValue(this, out var newValue))
                return (Term<T>)newValue;
            return RecursivelySubstitute(s);
        }

        internal abstract Term<T> RecursivelySubstitute(Substitution s);
    }
}
