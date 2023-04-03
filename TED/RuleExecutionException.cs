using System;
using System.Linq;
using TED.Interpreter;

namespace TED
{
    /// <summary>
    /// An exception that wraps an exception that occurred during the execution of a rule.
    /// This makes it possible to report back the TED-level code that triggered the exception.
    /// </summary>
    public class RuleExecutionException : Exception
    {
        /// <summary>
        /// Rule in which the exception occurred
        /// </summary>
        public readonly Rule Rule;

        /// <summary>
        /// The call from the rule that triggered the exception
        /// </summary>
        public readonly Call Call;
        
        internal RuleExecutionException(Rule rule, Call call, Exception innerException) : 
            base($"{innerException.GetType().Name} occurred while executing {call} in the rule:\n{rule}\nLocalVariables:\n{VariablesOfRule(rule)}", innerException)
        {
            Rule = rule;
            Call = call;
        }

        /// <summary>
        /// Names and current values of the local variables from the rule.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public string LocalVariables => VariablesOfRule(Rule);

        private static string VariablesOfRule(Rule rule) => string.Join('\n', rule.ValueCells.Select(c => $"{c.Name}={c.BoxedValue}"));

        /// <summary>
        /// Predicate for which the rule is defined
        /// </summary>
        public TablePredicate Predicate => Rule.Predicate;
    }
}
