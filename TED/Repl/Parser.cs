using System;
using System.Collections.Generic;
using System.Linq;
using static TED.Repl.ParserState;

namespace TED.Repl
{
    internal static class Parser
    {
        public static TablePredicate PredicateNamed(string name) => Simulation.Current!.Tables.First(t => t.Name == name);
        public static bool Identifier(ParserState s, Continuation<string> k) => s.ReadToken(char.IsLetter, k);
        public static bool Predicate(ParserState s, Continuation<TablePredicate> k) => s.ReadToken(char.IsLetter, PredicateNamed, k);

        public static bool Number(ParserState s, Continuation<Term> k)
            => s.ReadToken(char.IsDigit, (st, digits) => k(st, new Constant<int>(int.Parse(digits))));

        public static bool String(ParserState s, Continuation<Term> k)
            => s.Match("\"",
                s2 => s2.ReadToken(c => c != '"',
                    (s3, str) => s3.Match("\"",
                        s4 => k(s4, new Constant<string>(str)))));

        public static bool Variable(ParserState s, Continuation<Term> k) 
            => s.ReadToken(char.IsLetter, str => (Var<object>) str, k);

        public static bool Term(ParserState s, Continuation<Term> k)
            => Number(s, k) || String(s, k) || Variable(s, k);

        public static bool Goal(ParserState s, SymbolTable vars, Continuation<Goal> k)
            => Predicate(s,
                (s1, predicate) => s1.Match("[",
                    s2 => s2.DelimitedList<Term>(Term, ",",
                        (s3, args) => s3.Match("]",
                            s4 => k(s4, MakeGoal(predicate, args, vars))))));

        public static Goal MakeGoal(TablePredicate p, List<Term> args, SymbolTable vars)
        {
            var defaults = p.DefaultVariables;
            if (defaults.Length != args.Count)
                throw new ArgumentException(
                    $"Predicate {p.Name} expects {defaults.Length} arguments, but was passed {args.Count}");
            var finalArgs = new Term[defaults.Length];
            for (var i = 0; i < defaults.Length; i++)
            {
                if (args[i] is IVariable v)
                    finalArgs[i] = vars.GetVariable(v.VariableName, (Term)defaults[i]);
                else
                    finalArgs[i] = args[i];
            }

            return p[finalArgs];
        }

        public class SymbolTable
        {
            private readonly Dictionary<string, Term> variableTable = new Dictionary<string, Term>();

            /// <summary>
            /// Get the variable with this name, if one has already been made.  Otherwise, make one of the same type as defaultArg.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="defaultArg"></param>
            /// <returns></returns>
            public Term GetVariable(string name, Term defaultArg)
            {
                if (!variableTable.TryGetValue(name, out var variable))
                {
                    variable = defaultArg.MakeVariable(name);
                    variableTable[name] = variable;
                }
                return variable;
            }
        }
    }
}