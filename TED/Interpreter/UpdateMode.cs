namespace TED.Interpreter
{
    /// <summary>
    /// The different types of tables that get updated in different ways
    /// </summary>
    public enum UpdateMode
    {
        /// <summary>
        /// This table is extensional and either not updated, or updated via .Accumulates, .Add, .Set, etc.
        /// </summary>
        BaseTable,
        /// <summary>
        /// This is an intensional table defined by rules
        /// </summary>
        Rules,
        /// <summary>
        /// This is an intensional table that is the output of an operator.
        /// </summary>
        Operator
    }
}
