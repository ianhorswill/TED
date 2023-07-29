using System;
using System.Collections.Generic;
using System.Linq;
using TED.Interpreter;

namespace TED.Repl
{
    /// <summary>
    /// Support for Read/Evaluate/Print/Loops (REPLs), i.e. command line interpreters
    /// This converts strings containing code to executable queries and runs them, returning the results
    /// </summary>
    public class Repl
    {
        /// <summary>
        /// The collection of predicates to allow the user to call
        /// </summary>
        public readonly Program Program;

        /// <summary>
        /// Parser object to use to parse queries
        /// </summary>
        internal readonly Parser Parser;

        /// <summary>
        /// Called when the user types a term of the form $string or $"string".  Maps string to a Term.
        /// Fill this in to handle whatever application-specific term syntax you need.
        /// </summary>
        public Func<string, Term> ResolveConstant;
        
        /// <summary>
        /// Default constant resolver to use
        /// </summary>
        private static Term NullConstantResolver(string s) =>
            throw new Exception($"Can't resolve ${s} because no resolver has been defined for $constants.");

        /// <summary>
        /// Make a new Repl.
        /// </summary>
        /// <param name="program">Collection of predicates to be callable form the repl</param>
        /// <param name="resolveConstant">Constant resolver to use for $string terms, or null if $string shouldn't be supported</param>
        public Repl(Program program, Func<string, Term>? resolveConstant = null)
        {
            Program = program;
            ResolveConstant = resolveConstant??NullConstantResolver;
            Parser = new Parser(this);
        }

        /// <summary>
        /// Transform the query string into a TablePredicate with a single rule, whose columns/arguments
        /// are the variables of the query.
        /// </summary>
        /// <param name="name">Name to give to the TablePredicate that computes this query</param>
        /// <param name="goalString">String containing the query</param>
        /// <returns>TablePredicate that computes the results of the query</returns>
        public TablePredicate Query(string name, string goalString)
        {
            List<Goal> body = null!;
            Parser.SymbolTable variables = null!;
            if (!Parser.Body(new ParserState(goalString), (s, b) =>
                {
                    if (!s.End)
                        throw new Exception("Characters remaining after end of goal");
                    body = b;
                    variables = s.SymbolTable;
                    return true;
                }))
                throw new Exception("Syntax error");
            var predicate = TablePredicate.Create(name,
                variables.VariablesInOrder.Cast<IVariable>().ToArray());
            predicate.AddRule(body.ToArray());
            return predicate;
        }
    }
}
