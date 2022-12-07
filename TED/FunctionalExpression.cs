namespace TED
{
    public abstract class FunctionalExpression<T> : Term<T>
    {
        public override bool IsVariable => false;
    }
}
