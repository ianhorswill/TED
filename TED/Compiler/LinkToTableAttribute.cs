using System;

namespace TED.Compiler
{
    /// <summary>
    /// Metadata stating that this field should contain the Table object for the specified TablePredicate
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class LinkToTableAttribute : Attribute
    {
        /// <summary>
        /// Name of the table from whose rules this method is compiled.
        /// </summary>
        public readonly string TableName;

        public LinkToTableAttribute(string tableName)
        {
            TableName = tableName;
        }
    }
}