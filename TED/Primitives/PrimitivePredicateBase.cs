using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TED.Interpreter;

namespace TED.Primitives
{
    /// <summary>
    /// Untyped base class for primitive predicates
    /// </summary>
    public abstract class PrimitivePredicateBase : Predicate
    {
        /// <summary>
        /// Make a primitive predicate with the specified name
        /// </summary>
        protected PrimitivePredicateBase(string name) : base(name)
        { }

        internal Delegate? ConstantFolder;

        /// <summary>
        /// Name of C# procedure to call to implement this primitive, when compiling.
        /// </summary>
        public string? CompilationName;
        
        internal Interpreter.Goal FoldConstant(Interpreter.Goal goal)
        {
            if (ConstantFolder == null || !goal.IsConstant) return goal;
            var argValues = goal.Arguments.Select(a => ((IConstant)a).ValueUntyped).ToArray();
            return (Goal)(bool)ConstantFolder.DynamicInvoke(argValues);
        }
    }
}
