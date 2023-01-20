using System.Collections.Generic;

namespace TED
{
    /// <summary>
    /// Defines the EqualityComparer to use in indices for the specified type.
    /// This defines both how to compare equality and also how to compute hash codes.
    /// </summary>
    public static class Comparer<T>
    {
        /// <summary>
        /// Equality and hash function implementation to use for indexing.
        /// IMPORTANT: If you change this, do it before defining predicates.
        /// </summary>
        public static EqualityComparer<T> Default = EqualityComparer<T>.Default;
    }
}
