using System;

namespace TED
{
    /// <summary>
    /// A primitive that tests instantiated argument. Throws an instantiation exception if its argument is an unbound variable.
    /// </summary>
    /// <typeparam name="T1">Type of the argument to the predicate</typeparam>
    public class PrimitiveTest<T1> : PrimitivePredicate<T1>
    {
        internal readonly Predicate<T1> Implementation;

        /// <summary>
        /// Make a primitive test, i.e. a predicate that can only be called on instantiated arguments.
        /// </summary>
        /// <param name="name">Name, for debugging purposes</param>
        /// <param name="implementation">C# implementation.
        /// This will be called with the values of the arguments.
        /// If it returns true, the call to the test succeeds, otherwise, it fails and the system backtracks</param>
        public PrimitiveTest(string name, Predicate<T1> implementation) : base(name)
        {
            Implementation = implementation;
        }

        /// <inheritdoc />
        public override AnyCall MakeCall(Goal g, GoalAnalyzer tc)
        {
            var i = tc.Emit(g.Arg1);
            if (!i.IsInstantiated)
                throw new InstantiationException(this, g.Arg1);
            return new Call(this, i.ValueCell);
        }

        private class Call : AnyCall
        {
            private readonly PrimitiveTest<T1> _predicate;
            private readonly ValueCell<T1> arg;
            private bool ready;

            public override IPattern ArgumentPattern => new Pattern<T1>(MatchOperation<T1>.Read(arg));

            public Call(PrimitiveTest<T1> predicate, ValueCell<T1> arg) : base(predicate)
            {
                _predicate = predicate;
                this.arg = arg;
            }

            public override void Reset()
            {
                ready = true;
            }

            public override bool NextSolution()
            {
                if (!ready) return false;
                ready = false;
                return _predicate.Implementation(arg.Value);
            }
        }
    }

    /// <summary>
    /// A primitive that tests instantiated two arguments. Throws an instantiation exception if either argument is an unbound variable.
    /// </summary>
    /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
    /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
    public class PrimitiveTest<T1, T2> : PrimitivePredicate<T1, T2>
    {
        internal readonly Func<T1, T2, bool> Implementation;

        /// <summary>
        /// Make a primitive test, i.e. a predicate that can only be called on instantiated arguments.
        /// </summary>
        /// <param name="name">Name, for debugging purposes</param>
        /// <param name="implementation">C# implementation.
        /// This will be called with the values of the arguments.
        /// If it returns true, the call to the test succeeds, otherwise, it fails and the system backtracks</param>
        public PrimitiveTest(string name, Func<T1, T2, bool> implementation) : base(name)
        {
            Implementation = implementation;
        }

        /// <inheritdoc />
        public override AnyCall MakeCall(Goal g, GoalAnalyzer tc)
        {
            var i1 = tc.Emit(g.Arg1);
            var i2 = tc.Emit(g.Arg2);
            if (!i1.IsInstantiated)
                throw new InstantiationException(this, g.Arg1);
            if (!i2.IsInstantiated)
                throw new InstantiationException(this, g.Arg2);
            return new Call(this, i1.ValueCell, i2.ValueCell);
        }

        private class Call : AnyCall
        {
            private readonly PrimitiveTest<T1, T2> _predicate;
            private readonly ValueCell<T1> arg1;
            private readonly ValueCell<T2> arg2;
            private bool ready;

            public override IPattern ArgumentPattern =>
                new Pattern<T1, T2>(MatchOperation<T1>.Read(arg1), MatchOperation<T2>.Read(arg2));

            public Call(PrimitiveTest<T1, T2> predicate, ValueCell<T1> arg1, ValueCell<T2> arg2) : base(predicate)
            {
                _predicate = predicate;
                this.arg1 = arg1;
                this.arg2 = arg2;
            }

            public override void Reset()
            {
                ready = true;
            }

            public override bool NextSolution()
            {
                if (!ready) return false;
                ready = false;
                return _predicate.Implementation(arg1.Value, arg2.Value);
            }
        }
    }

}
