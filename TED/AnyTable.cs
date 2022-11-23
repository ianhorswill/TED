namespace TED
{
    /// <summary>
    /// Untyped base class for all tables
    /// Tables are essentially a custom version of the List class that is optimized for this application
    /// </summary>
    internal class AnyTable
    {
        /// <summary>
        /// Number of rows in the table, regardless of the size of the underlying array.
        /// </summary>
        public int Length { get; protected set; }

        /// <summary>
        /// Remove all rows from the table
        /// </summary>
        public void Clear()
        {
            Length = 0;
        }
    }
}
