namespace TED.Tables
{
    /// <summary>
    /// Specifies indexing to be used for a column of a TablePredicate
    /// </summary>
    public enum IndexMode
    {
        /// <summary>
        /// No index
        /// </summary>
        None = 0,
        /// <summary>
        /// Index as a key
        /// </summary>
        Key = 1,
        /// <summary>
        /// Index but not as a key
        /// </summary>
        NonKey = 2
    }
}
