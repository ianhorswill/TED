using System.Collections.Generic;

namespace TED
{
    /// <summary>
    /// Implementation of the typed In(element, collection) primitive 
    /// </summary>
    /// <typeparam name="T">Element type of the collection</typeparam>
    internal class InPrimitive<T> : PrimitivePredicate<T, ICollection<T>>
    {
        public static InPrimitive<T> Singleton = new InPrimitive<T>();

        private InPrimitive() : base("In")
        {
        }

        public override AnyCall MakeCall(Goal g, GoalAnalyzer tc)
        {
            var p = new Pattern<T, ICollection<T>>(tc.Emit(g.Arg1), tc.Emit(g.Arg2));
            if (!p.Arg2.IsInstantiated)
                throw new InstantiationException($"The second argument in {g} must be instantiated");
            if (p.Arg1.IsInstantiated)
                return new InCallTestMode(g, p);
            return new InCallGenerateMode(g, p);
        }

        internal class InCallGenerateMode : AnyCall
        {
            private readonly ValueCell<T> element;
            private readonly ValueCell<ICollection<T>> collection;

            private readonly Pattern<T, ICollection<T>> argumentPattern;

            public InCallGenerateMode(Goal goal, Pattern<T, ICollection<T>> pattern) : base(goal.Predicate)
            {
                element = pattern.Arg1.ValueCell;
                collection = pattern.Arg2.ValueCell;
                argumentPattern = pattern;
            }

            private IEnumerator<T>? enumerator;

            public override IPattern ArgumentPattern => argumentPattern;
            public override void Reset()
            {
                enumerator = collection.Value.GetEnumerator();
            }

            public override bool NextSolution()
            {
                if (enumerator == null)
                    return false;
                if (!enumerator.MoveNext())
                {
                    enumerator = null;
                    return false;
                }

                element.Value = enumerator.Current;
                return true;
            }
        }

        internal class InCallTestMode : AnyCall
        {
            private readonly ValueCell<T> element;
            private readonly ValueCell<ICollection<T>> collection;

            private readonly Pattern<T, ICollection<T>> argumentPattern;

            public InCallTestMode(Goal goal, Pattern<T, ICollection<T>> pattern) : base(goal.Predicate)
            {
                element = pattern.Arg1.ValueCell;
                collection = pattern.Arg2.ValueCell;
                argumentPattern = pattern;
            }

            private bool primed;

            public override IPattern ArgumentPattern => argumentPattern;
            public override void Reset()
            {
                primed = true;
            }

            public override bool NextSolution()
            {
                var success = primed && collection.Value.Contains(element.Value);
                primed = false;
                return success;
            }
        }
    }
}
