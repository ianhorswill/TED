using System;

namespace TED.Compiler
{
    /// <summary>
    /// Metadata stating the attached class contains the compiled rules for the named TED Program object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CompiledHelpersForAttribute : Attribute
    {
        /// <summary>
        /// Name of the table from whose rules this method is compiled.
        /// </summary>
        public readonly string ProgramName;

        public CompiledHelpersForAttribute(string programName)
        {
            ProgramName = programName;
        }
    }
}
