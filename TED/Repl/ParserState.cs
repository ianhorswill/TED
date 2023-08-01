using System;
using System.Collections.Generic;

namespace TED.Repl
{
    internal readonly struct ParserState
    {
        public delegate bool Continuation(ParserState s);

        public delegate bool Continuation<T>(ParserState s, T payload);

        public delegate bool Parser<T>(ParserState s, Continuation<T> k);

        public readonly string Text;
        public readonly int Position;
        public readonly Parser.SymbolTable SymbolTable;

        public ParserState(string text, int position, Parser.SymbolTable symbolTable)
        {
            Text = text;
            Position = position;
            SymbolTable = symbolTable;
        }

        public ParserState(string s) : this(s, 0, new Parser.SymbolTable())
        { }

        public ParserState JumpTo(int position) => new ParserState(Text, position, SymbolTable);

        public bool End => Position == Text.Length;

        public int RemainingChars => Text.Length - Position;

        public char Current => Text[Position];

        public bool Skip(System.Predicate<char> test, Continuation k, bool allowEmpty = true)
        {
            int i;
            for (i = Position; i < Text.Length && test(Text[i]); i++) ;
            if (i == Position && !allowEmpty)
                return false;
            return k(JumpTo(i));
        }

        public bool SkipWhitespace(Continuation k) => Skip(char.IsWhiteSpace, k);

        public ParserState SkipWhitespace()
        {
            int i;
            for (i = Position; i < Text.Length && char.IsWhiteSpace(Text[i]); i++) ;
            return new ParserState(Text, i, SymbolTable);
        }

        public bool ReadToken(System.Predicate<char> test, Continuation<string> k, bool allowEmpty = false)
        {
            int i;
            for (i = Position; i < Text.Length && test(Text[i]); i++) ;
            if (i == Position && !allowEmpty)
                return false;
            return k(JumpTo(i), Text.Substring(Position, i-Position));
        }

        public bool ReadToken<T>(System.Predicate<char> test, Func<string, T> func, Continuation<T> k, bool allowEmpty = false)
        {
            int i;
            for (i = Position; i < Text.Length && test(Text[i]); i++) ;
            if (i == Position && !allowEmpty)
                return false;
            return k(JumpTo(i), func(Text.Substring(Position, i-Position)));
        }

        public bool MatchSkippingWhitespace(string str, Continuation k)
            => SkipWhitespace(s => s.Match(str, k));

        public bool Match(string s, Continuation k)
        {
            if (RemainingChars < s.Length)
                return false;
            int i;
            for (i = 0; i < s.Length; i++)
                if (Text[Position + i] != s[i])
                    return false;
            return k(JumpTo(Position + i));
        }

        public bool MatchAnyOf(string matchingChars, Continuation k)
        {
            if (RemainingChars < 1)
                return false;

            if (!matchingChars.Contains(Text[Position]))
                    return false;
            return k(JumpTo(Position + 1));
        }

        public bool Star<T>(Parser<T> parser, Continuation<IList<T>> k)
        {
            var values = new List<T>();
            var state = this;
            while (parser(state, (newState, newElement) =>
                   {
                       state = newState;
                       values.Add(newElement);
                       return true;
                   })) ;
            return k(state, values);
        }

        public bool DelimitedList<T>(Parser<T> parser, string delimiter, Continuation<List<T>> k)
        {
            var values = new List<T>();
            var state = this;

            void SkipWhite() => state.SkipWhitespace(s =>
            {
                state = s;
                return true;
            });

            SkipWhite();
            bool more = false;
            while (!state.End && parser(state, (newState, newElement) =>
                   {
                       values.Add(newElement);
                       state = newState;
                       SkipWhite();
                       more = state.Match(delimiter, new2 => { state = new2;
                           SkipWhite();
                           return true; 
                       });
                       return true;
                   })
                   && more) ;
            SkipWhite();
            return k(state, values);
        }

        /// <summary>
        /// Matches payload surrounded by specified bracket characters
        /// </summary>
        /// <typeparam name="T">Type of payload output</typeparam>
        /// <param name="openOptions">Characters allowed as open bracket</param>
        /// <param name="payload"></param>
        /// <param name="closeOptions">Characters allowed as close bracket</param>
        /// <param name="k">Continuation called with parser state after close bracket and output of payload</param>
        /// <returns>True if successfully matched and continuation returns true.</returns>
        public bool Bracketed<T>(string openOptions, Parser<T> payload, string closeOptions, Continuation<T> k) =>
            SkipWhitespace().MatchAnyOf(openOptions,
                s1 => payload(s1.SkipWhitespace(),
                    (s2, pay) => 
                        s2.SkipWhitespace().MatchAnyOf(closeOptions,
                            s3 => k(s3, pay))));

        public bool CallExpression<TCalled, TArg>(Parser<TCalled> called, Parser<TArg> arg,
            Continuation<(TCalled, List<TArg>)> k)
        {
            bool Arglist(ParserState s, Continuation<List<TArg>> k) => s.DelimitedList(arg, ",", k);
            return called(SkipWhitespace(),
                (s1, target)
                    => s1.Bracketed<List<TArg>>("[(", Arglist, ")]", (s2, args) =>
                        k(s2, (target, args))));
        }

        public bool InfixOperator<TOperator, TArg>(Parser<TOperator> op, Parser<TArg> arg,
            Continuation<(TOperator, TArg, TArg)> k)
            => arg(SkipWhitespace(),
                (s1, left) =>
                    op(s1.SkipWhitespace(), 
                        (s2, target) =>
                            arg(s2.SkipWhitespace(),
                                (s3, right) =>
                                    k(s3, (target, left, right)))));

        public bool InfixOperator<TArg>(string op, Parser<TArg> arg,
            Continuation<(TArg, TArg)> k)
            => arg(SkipWhitespace(),
                (s1, left) =>
                    s1.SkipWhitespace().Match(op, 
                        (s2) =>
                            arg(s2.SkipWhitespace(),
                                (s3, right) =>
                                    k(s3, (left, right)))));

        public override string ToString() =>
            $"{Text.Substring(0, Position)}|{Text.Substring(Position, Text.Length - Position)}";
    }
}
