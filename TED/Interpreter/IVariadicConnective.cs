namespace TED.Interpreter
{
    /// <summary>
    /// Interface for primitives like And, Or, and FirstOf, that take sequences of goals as arguments
    /// </summary>
    public interface IVariadicConnective
    {
        Goal this[params Goal[] goals] { get; }
    }
}
