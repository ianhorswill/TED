using System;
using System.Collections.Generic;

namespace TED.Repl
{
    internal readonly struct ParserState
    {
        public delegate bool Continuation(ParserState s);

        public delegate bool Continuation<in T>(ParserState s, T payload);

        public delegate bool Parser<out T>(ParserState s, Continuation<T> k);

        public readonly string Text;
        public readonly int Position;

        public ParserState(string text, int position)
        {
            Text = text;
            Position = position;
        }

        public ParserState(string s) : this(s, 0)
        { }

        public ParserState JumpTo(int position) => new ParserState(Text, position);

        public bool End => Position == Text.Length;

        public int RemainingChars => Text.Length - Position;

        #if DEBUG
        /// <summary>
        /// Current next character, so you can check in the debugger
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public char Current => Text[Position];
        #endif

        public bool Skip(Predicate<char> test, Continuation k, bool allowEmpty = true)
        {
            int i;
            for (i = Position; i < Text.Length && test(Text[i]); i++)
            { }

            if (i == Position && !allowEmpty)
                return false;
            return k(JumpTo(i));
        }

        public bool SkipWhitespace(Continuation k) => Skip(char.IsWhiteSpace, k);

        public bool ReadToken(Predicate<char> test, Continuation<string> k, bool allowEmpty = false)
        {
            int i;
            for (i = Position; i < Text.Length && test(Text[i]); i++)
            { }

            if (i == Position && !allowEmpty)
                return false;
            return k(JumpTo(i), Text.Substring(Position, i-Position));
        }

        public bool ReadToken<T>(Predicate<char> test, Func<string, T> func, Continuation<T> k, bool allowEmpty = false)
        {
            int i;
            for (i = Position; i < Text.Length && test(Text[i]); i++)
            { }

            if (i == Position && !allowEmpty)
                return false;
            return k(JumpTo(i), func(Text.Substring(Position, i-Position)));
        }

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

        public bool Star<T>(Parser<T> parser, Continuation<IList<T>> k)
        {
            var values = new List<T>();
            var state = this;
            while (parser(state, (newState, newElement) =>
                   {
                       state = newState;
                       values.Add(newElement);
                       return true;
                   }))
            { }

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
            while (!state.End && parser(state, (newState, newElement) =>
                   {
                       values.Add(newElement);
                       state = newState;
                       SkipWhite();
                       return state.Match(delimiter, new2 => { state = new2;
                           SkipWhite();
                           return true; 
                       });
                   }))
            { }

            SkipWhite();
            return k(state, values);
        }

        public override string ToString() =>
            $"{Text.Substring(0, Position)}|{Text.Substring(Position, Text.Length - Position)}";
    }
}
