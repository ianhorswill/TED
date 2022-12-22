namespace TED
{
    public abstract class FunctionalExpression<T> : Term<T>
    {
        public override bool IsFunctionalExpression => true;

        internal override (AnyTerm Expression, AnyTerm Var, AnyGoal MatchGoal) HoistInfo()
        {
            var v = new Var<T>("temp");
            return (this, v, Language.Match(v, this));
        }
    }
}
