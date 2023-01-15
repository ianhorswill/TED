using System;
using System.Linq;

namespace TED
{
    public class RuleExecutionException : Exception
    {
        /// <summary>
        /// Rule in which the exception occurred
        /// </summary>
        public readonly AnyRule Rule;

        public readonly AnyCall Call;
        
        internal RuleExecutionException(AnyRule rule, AnyCall call, Exception innerException) : 
            base($"{innerException!.GetType().Name} occurred while executing {call} in the rule:\n{rule}\nLocalVariables:\n{VariablesOfRule(rule)}", innerException)
        {
            Rule = rule;
            Call = call;
        }

        /// <summary>
        /// Names and current values of the local variables from the rule.
        /// </summary>
        public string LocalVariables => VariablesOfRule(Rule);

        private static string VariablesOfRule(AnyRule rule) => string.Join('\n', rule.ValueCells.Select(c => $"{c.Name}={c.BoxedValue}"));

        /// <summary>
        /// Predicate for which the rule is defined
        /// </summary>
        public TablePredicate Predicate => Rule.Predicate;
    }
}
