using System;
using System.Collections.Generic;
using System.Text;

namespace TED.Interpreter
{
    /// <summary>
    /// Specifies a limitation on the binding state of an argument to a definition.
    /// </summary>
    public enum InstantiationConstraint
    {
        /// <summary>
        /// Argument must be an input (i.e. already bound)
        /// </summary>
        In,
        /// <summary>
        /// Argument must be an output (i.e. not yet bound)
        /// </summary>
        Out,
        /// <summary>
        /// Argument must be a literal constant
        /// </summary>
        Constant
    }
}
