using System;

namespace TED.Compiler
{
    /// <summary>
    /// Metadata stating that a method is the compiled version of the rules for the named table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CompiledRulesForAttribute : Attribute
    {
        /// <summary>
        /// Name of the table from whose rules this method is compiled.
        /// </summary>
        public readonly string TableName;

        public CompiledRulesForAttribute(string tableName)
        {
            TableName = tableName;
        }
    }
}
