﻿using System;

namespace TED.Interpreter
{
    /// <summary>
    /// Untyped base interface to identify Terms as being Var[T] for some T.
    /// THE ONLY CLASS THAT SHOULD IMPLEMENT THIS IS Var[T]!  This only exists to give us a way of asking if a Term is a Var[T]
    /// without knowing in advance what T is.
    /// </summary>
    public interface IVariable
    {
        /// <summary>
        /// Name of the variable
        /// </summary>
        public string VariableName { get; }

        /// <summary>
        /// The type of the variable's value
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Returns a goal that will succeed iff this variable matches the specified term
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Goal EquateTo(Term t);
    }
}
