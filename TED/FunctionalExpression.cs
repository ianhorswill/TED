﻿namespace TED
{
    /// <summary>
    /// Base class for terms whose value is of type T but where the value is computed from other Terms by some function.
    /// </summary>
    public abstract class FunctionalExpression<T> : Term<T>
    {
        public override bool IsFunctionalExpression => true;

        internal override (AnyTerm Expression, AnyTerm Var, AnyGoal EvalGoal) HoistInfo()
        {
            var v = new Var<T>("temp");
            return (this, v, Language.Eval(v, this));
        }
    }
}
