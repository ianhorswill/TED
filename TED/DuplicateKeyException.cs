using System;
using System.Collections.Generic;
using System.Text;

namespace TED
{
    public class DuplicateKeyException : Exception
    {
        public DuplicateKeyException(TablePredicate p, object k) : base($"Attempt to add row with a key value {NullSafeToString(k)} to table predicate {NullSafeToString(p)}, which already contains a row with that key.")
        {}

        private static string NullSafeToString(object? x) => ReferenceEquals(x, null) ? "null" : x.ToString();
    }
}
