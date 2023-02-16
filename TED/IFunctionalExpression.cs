using System;

namespace TED
{
    /// <summary>
    /// Functions as an untyped base class for FunctionalExpression[T]
    /// </summary>
    internal interface IFunctionalExpression
    {
        /// <summary>
        /// Return the information necessary to hoist a FunctionalExpression from a goal.
        /// This is only implemented for the FunctionalExpression class.
        /// </summary>
        /// <returns>The expression, a variable of the right type to hold it's value, and a call to the Eval primitive to compute it</returns>
        /// <exception cref="NotImplementedException">If this object isn't a FunctionalExpression</exception>
        public (Term Expression, Term Var, Goal EvalGoal) HoistInfo();
    }
}
