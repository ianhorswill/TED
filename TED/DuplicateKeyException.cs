using System;
using System.Collections.Generic;
using System.Text;

namespace TED
{
    /// <summary>
    /// Exception indicating that a row is being added to a table that already contains a different row with the same key value
    /// </summary>
    public class DuplicateKeyException : Exception
    {
        /// <summary>
        /// Exception indicating that a row is being added to a table that already contains a different row with the same key value
        /// </summary>

        public DuplicateKeyException(TablePredicate p, object k) : base($"Attempt to add row with a key value {NullSafeToString(k)} to table predicate {NullSafeToString(p)}, which already contains a row with that key.")
        {}

        /// <summary>
        /// Convert value to a string, returning the null value as "null"
        /// </summary>
        private static string NullSafeToString(object? x) => ReferenceEquals(x, null) ? "null" : x.ToString();
    }
}
