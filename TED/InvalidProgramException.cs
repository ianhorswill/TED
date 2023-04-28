using System;
using System.Collections.Generic;
using System.Text;

namespace TED
{
    /// <summary>
    /// Exception indicating a TED program is somehow malformed.
    /// </summary>
    public class InvalidProgramException : Exception
    {
        /// <summary>
        /// Exception indicating a TED program is somehow malformed.
        /// </summary>
        public InvalidProgramException(string message) : base(message)
        { }
    }
}
