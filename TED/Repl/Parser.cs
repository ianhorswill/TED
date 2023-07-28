using System;
using System.Collections.Generic;
using static TED.Repl.ParserState;
using TED.Interpreter;

namespace TED.Repl
{
    internal class Parser
    {
        public readonly Repl Repl;

        public Parser(Repl repl)
        {
            Repl = repl;
        }

        public TablePredicate PredicateNamed(string name) 
            => Repl.Program.PredicateNamed(name)??throw new Exception($"There is no predicate named {name}");
        public static bool Identifier(ParserState s, Continuation<string> k) 
            => s.ReadToken(char.IsLetter, k);

        public bool Predicate(ParserState s, Continuation<TablePredicate> k)
            => s.ReadToken(char.IsLetter, PredicateNamed, (s1, p) => PredicateFieldReference(s1, p, k));

        private static bool PredicateFieldReference(ParserState s, TablePredicate predicate,
            Continuation<TablePredicate> k)
            => s.Match(".", s1 =>
                Identifier(s1, (s2, fieldName) =>
                    PredicateFieldReference(s2, LookupField(predicate, fieldName), k)))
                || k(s, predicate);

        private static TablePredicate LookupField(TablePredicate predicate, string fieldName)
        {
            var t = predicate.GetType();
            var f = t.GetField(fieldName);
            if (f != null)
            {
                if (f.FieldType.IsSubclassOf(typeof(TablePredicate)))
                    return (TablePredicate)f.GetValue(predicate);
                throw new Exception($"{predicate.Name}.{fieldName} is not a TablePredicate");
            }
            var p = t.GetProperty(fieldName);
            if (p != null)
            {
                if (p.PropertyType.IsSubclassOf(typeof(TablePredicate)))
                    return (TablePredicate)p.GetValue(predicate);
                throw new Exception($"{predicate.Name}.{fieldName} is not a TablePredicate");
            }

            throw new MissingFieldException($"{predicate.Name} has no field or property named {fieldName}");
        }

        public static bool Number(ParserState s, Continuation<Term> k)
            => s.ReadToken(char.IsDigit, (st, digits) => k(st, new Constant<int>(int.Parse(digits))));

        public static bool String(ParserState s, Continuation<Term> k)
            => s.Match("\"",
                s2 => s2.ReadToken(c => c != '"',
                    (s3, str) => s3.Match("\"",
                        s4 => k(s4, new Constant<string>(str)))));

        public static bool Variable(ParserState s, Continuation<Term> k) 
            => s.ReadToken(char.IsLetter, str => (Var<object>) str, k);

        public bool Term(ParserState s, Continuation<Term> k)
            => Number(s, k) || String(s, k) || Variable(s, k) || ExternalConstant(s,k);

        private bool ExternalConstant(ParserState s, Continuation<Term> k)
        {
            return s.Match("$",
                s2 => s2.ReadToken(char.IsLetter, (s3, str) => k(s3, Repl.ResolveConstant(str)))
                      || s2.Match("\"",
                          s3 => s3.ReadToken(c => c != '"',
                              (s4, str) => s4.Match("\"",
                                  s5 => k(s5, Repl.ResolveConstant(str))))));
        }

        public bool Goal(ParserState s, SymbolTable vars, Continuation<Goal> k)
            => Predicate(s,
                   (s1, predicate) => 
                       s1.MatchAnyOf("[(",
                           s2 => s2.DelimitedList<Term>(Term, ",",
                               (s3, args) => s3.MatchAnyOf("])",
                                   s4 => k(s4, MakeGoal(predicate, args, vars))))))
                        || s.Match("!", s5 => Goal(s5.SkipWhitespace(), vars, (s6, g) => k(s6, !g)));

        public bool Body(ParserState s, SymbolTable vars, Continuation<List<Goal>> k)
            => s.DelimitedList((gs, gk) => Goal(gs, vars, gk), ",", k); 

        public Goal MakeGoal(TablePredicate p, List<Term> args, SymbolTable vars)
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

            return p.GetGoal(finalArgs);
        }

        public class SymbolTable
        {
            private readonly Dictionary<string, Term> variableTable = new Dictionary<string, Term>();
            public List<Term> VariablesInOrder = new List<Term>();

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
                    VariablesInOrder.Add(variable);
                }
                return variable;
            }
        }
    }
}