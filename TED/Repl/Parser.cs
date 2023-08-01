using System;
using System.Collections.Generic;
using System.Linq;
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
            => s.ReadToken(char.IsLetter, str => GetVariable(s, str), k);

        /// <summary>
        /// Get a variable with this name.
        /// If we already have a variable by this name, return that.
        /// If not, return a placeholder but don't add it to the symbol table
        /// because we don't yet know its type.  When it gets used in a goal, MakeGoal will
        /// make a new variable with this name and the correct type and add it to the symbol table.
        /// </summary>
        /// <param name="s">Parser state (for the symbol table)</param>
        /// <param name="name">Name of the variable</param>
        private static Term GetVariable(ParserState s, string name) 
            => s.SymbolTable.MaybeGetVariable(name)??(Var<object>) name;

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

        public bool Goal(ParserState s, Continuation<Goal> k)
            => ComparisonExpression(s, k) || SimpleGoal(s, k)
            || s.Bracketed("(", Goal, ")", k)
            || s.MatchSkippingWhitespace("!",
                s2=>Goal(s2,(s3, g) =>
                    k(s3, !g)));

        public bool SimpleGoal(ParserState s, Continuation<Goal> k)
            => s.CallExpression<TablePredicate,Term>(Predicate, Term,
                (s1, targetAndArgs) =>
                    k(s1, MakeGoal(targetAndArgs.Item1, targetAndArgs.Item2, s1.SymbolTable)));

        private bool ComparisonExpression(ParserState s, Continuation<Goal> k)
            => s.InfixOperator<string, Term>(ComparisonOperator, Term,
                (s1, info) =>
                    k(s1, info.Item2.MakeComparison(info.Item1, info.Item3)));
        
        private static readonly string[] ComparisonOperators =
        {
            "<", ">", "<=", ">=", "==", "!="
        };

        private static readonly char[] ComparisonChars = { '=', '!', '<', '>' };
        private static bool IsComparisonChar(char c) => ComparisonChars.Contains(c);

        private static bool ComparisonOperator(ParserState s, Continuation<string> k)
            => s.SkipWhitespace(s1 =>
                s.ReadToken(IsComparisonChar,
                    (s2, op) => ComparisonOperators.Contains(op)
                                && k(s2, op)));

        //public bool Disjunction(ParserState s, Continuation<Goal> k)
        //    => s.InfixOperator<Goal>("|", Conjunction, (s1, args) =>
        //        k(s1, args.Item1 | args.Item2))
        //    | Conjunction(s, k);

        //public bool Conjunction(ParserState s, Continuation<Goal> k)
        //    => s.InfixOperator<Goal>("&", Literal, (s1, args) =>
        //        k(s1, args.Item1 & args.Item2))
        //    | Literal(s, k);

        //public bool Literal(ParserState s, Continuation<Goal> k)
        //=> 
        
        public bool Body(ParserState s, Continuation<List<Goal>> k)
            => s.DelimitedList(Goal, ",", k); 

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

            public Term? MaybeGetVariable(string name) => variableTable.TryGetValue(name, out var v) ? v : null;
        }
    }
}