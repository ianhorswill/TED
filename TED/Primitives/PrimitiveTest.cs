using System;
using TED.Interpreter;
using TED.Preprocessing;

namespace TED.Primitives {
    /// <summary>
    /// A primitive that tests for truth using a C# function.
    /// </summary>
    public class PrimitiveTest : PrimitivePredicate {
        internal readonly Func<bool> Implementation;

        /// <summary>
        /// Make a primitive test, i.e. a predicate that can only be called on instantiated arguments.
        /// </summary>
        /// <param name="name">Name, for debugging purposes</param>
        /// <param name="implementation">C# implementation.
        ///     This will be called with the values of the arguments.
        ///     If it returns true, the call to the test succeeds, otherwise, it fails and the system backtracks</param>
        public PrimitiveTest(string name, Func<bool> implementation) : base(name) => Implementation = implementation;
        // This has no arguments, so should never be constant folded.

        /// <inheritdoc />
        public override Interpreter.Call MakeCall(Goal g, GoalAnalyzer tc) => new Call(this);

        private class Call : Interpreter.Call {
            private readonly PrimitiveTest predicate;
            private bool ready;

            public override IPattern ArgumentPattern => new Pattern();
            public Call(PrimitiveTest predicate) : base(predicate) => this.predicate = predicate;
            public override void Reset() => ready = true;
            public override bool NextSolution() {
                if (!ready) return false;
                ready = false;
                return predicate.Implementation();
            }
        }
    }

    /// <summary>
    /// A primitive that tests instantiated argument. Throws an instantiation exception if its argument is an unbound variable.
    /// </summary>
    /// <typeparam name="T1">Type of the argument to the predicate</typeparam>
    public class PrimitiveTest<T1> : PrimitivePredicate<T1> {
        internal readonly Func<T1, bool> Implementation;

        /// <summary>
        /// Make a primitive test, i.e. a predicate that can only be called on instantiated arguments.
        /// </summary>
        /// <param name="name">Name, for debugging purposes</param>
        /// <param name="implementation">C# implementation.
        /// This will be called with the values of the arguments.
        /// If it returns true, the call to the test succeeds, otherwise, it fails and the system backtracks</param>
        /// <param name="isFunctional">This predicate has no side-effects and so can be safely constant-folded.</param>
        public PrimitiveTest(string name, Func<T1, bool> implementation, bool isFunctional = true) : base(name) {
            Implementation = implementation;
            if (isFunctional)
                ConstantFolder = Implementation;
        }

        /// <inheritdoc />
        public override Interpreter.Call MakeCall(Goal g, GoalAnalyzer tc) {
            var i = tc.Emit(g.Arg1);
            if (!i.IsInstantiated)
                throw new InstantiationException(this, g.Arg1);
            return new Call(this, i.ValueCell);
        }

        private class Call : Interpreter.Call {
            private readonly PrimitiveTest<T1> predicate;
            private readonly ValueCell<T1> arg;
            private bool ready;

            public override IPattern ArgumentPattern => new Pattern<T1>(MatchOperation<T1>.Read(arg));

            public Call(PrimitiveTest<T1> predicate, ValueCell<T1> arg) : base(predicate) {
                this.predicate = predicate;
                this.arg = arg;
            }

            public override void Reset() => ready = true;

            public override bool NextSolution() {
                if (!ready) return false;
                ready = false;
                return predicate.Implementation(arg.Value);
            }
        }
    }

    /// <summary>
    /// A primitive that tests instantiated two arguments. Throws an instantiation exception if either argument is an unbound variable.
    /// </summary>
    /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
    /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
    public class PrimitiveTest<T1, T2> : PrimitivePredicate<T1, T2> {
        internal readonly Func<T1, T2, bool> Implementation;

        /// <summary>
        /// Make a primitive test, i.e. a predicate that can only be called on instantiated arguments.
        /// </summary>
        /// <param name="name">Name, for debugging purposes</param>
        /// <param name="implementation">C# implementation.
        /// This will be called with the values of the arguments.
        /// If it returns true, the call to the test succeeds, otherwise, it fails and the system backtracks</param>
        /// <param name="isFunctional">This predicate has no side-effects and so can be safely constant-folded</param>
        public PrimitiveTest(string name, Func<T1, T2, bool> implementation, bool isFunctional = true) : base(name) {
            Implementation = implementation;
            if (isFunctional)
                ConstantFolder = Implementation;
        }

        /// <inheritdoc />
        public override Interpreter.Call MakeCall(Goal g, GoalAnalyzer tc) {
            var i1 = tc.Emit(g.Arg1);
            var i2 = tc.Emit(g.Arg2);
            if (!i1.IsInstantiated)
                throw new InstantiationException(this, g.Arg1);
            if (!i2.IsInstantiated)
                throw new InstantiationException(this, g.Arg2);
            return new Call(this, i1.ValueCell, i2.ValueCell);
        }

        private class Call : Interpreter.Call {
            private readonly PrimitiveTest<T1, T2> predicate;
            private readonly ValueCell<T1> arg1;
            private readonly ValueCell<T2> arg2;
            private bool ready;

            public override IPattern ArgumentPattern =>
                new Pattern<T1, T2>(MatchOperation<T1>.Read(arg1), MatchOperation<T2>.Read(arg2));

            public Call(PrimitiveTest<T1, T2> predicate, ValueCell<T1> arg1, ValueCell<T2> arg2) : base(predicate) {
                this.predicate = predicate;
                this.arg1 = arg1;
                this.arg2 = arg2;
            }

            public override void Reset() => ready = true;

            public override bool NextSolution() {
                if (!ready) return false;
                ready = false;
                return predicate.Implementation(arg1.Value, arg2.Value);
            }
        }
    }

    /// <summary>
    /// A primitive that tests instantiated two arguments. Throws an instantiation exception if either argument is an unbound variable.
    /// </summary>
    /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
    /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
    /// <typeparam name="T3">Type of the third argument to the predicate</typeparam>
    public class PrimitiveTest<T1, T2, T3> : PrimitivePredicate<T1, T2, T3> {
        internal readonly Func<T1, T2, T3, bool> Implementation;

        /// <summary>
        /// Make a primitive test, i.e. a predicate that can only be called on instantiated arguments.
        /// </summary>
        /// <param name="name">Name, for debugging purposes</param>
        /// <param name="implementation">C# implementation.
        /// This will be called with the values of the arguments.
        /// If it returns true, the call to the test succeeds, otherwise, it fails and the system backtracks</param>
        /// <param name="isFunctional">This predicate has no side-effects and so can be safely constant-folded</param>
        public PrimitiveTest(string name, Func<T1, T2, T3, bool> implementation, bool isFunctional = true) : base(name) {
            Implementation = implementation;
            if (isFunctional)
                ConstantFolder = Implementation;
        }

        /// <inheritdoc />
        public override Interpreter.Call MakeCall(Goal g, GoalAnalyzer tc) {
            var i1 = tc.Emit(g.Arg1);
            var i2 = tc.Emit(g.Arg2);
            var i3 = tc.Emit(g.Arg3);
            if (!i1.IsInstantiated)
                throw new InstantiationException(this, g.Arg1);
            if (!i2.IsInstantiated)
                throw new InstantiationException(this, g.Arg2);
            if (!i3.IsInstantiated)
                throw new InstantiationException(this, g.Arg3);
            return new Call(this, i1.ValueCell, i2.ValueCell, i3.ValueCell);
        }

        private class Call : Interpreter.Call {
            private readonly PrimitiveTest<T1, T2, T3> predicate;
            private readonly ValueCell<T1> arg1;
            private readonly ValueCell<T2> arg2;
            private readonly ValueCell<T3> arg3;
            private bool ready;

            public override IPattern ArgumentPattern =>
                new Pattern<T1, T2, T3>(MatchOperation<T1>.Read(arg1), MatchOperation<T2>.Read(arg2), MatchOperation<T3>.Read(arg3));

            public Call(PrimitiveTest<T1, T2, T3> predicate, ValueCell<T1> arg1, ValueCell<T2> arg2, ValueCell<T3> arg3) : base(predicate) {
                this.predicate = predicate;
                this.arg1 = arg1;
                this.arg2 = arg2;
                this.arg3 = arg3;
            }

            public override void Reset() => ready = true;

            public override bool NextSolution() {
                if (!ready) return false;
                ready = false;
                return predicate.Implementation(arg1.Value, arg2.Value, arg3.Value);
            }
        }
    }

    /// <summary>
    /// A primitive that tests instantiated two arguments. Throws an instantiation exception if either argument is an unbound variable.
    /// </summary>
    /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
    /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
    /// <typeparam name="T3">Type of the third argument to the predicate</typeparam>
    /// <typeparam name="T4">Type of the fourth argument to the predicate</typeparam>
    public class PrimitiveTest<T1, T2, T3, T4> : PrimitivePredicate<T1, T2, T3, T4> {
        internal readonly Func<T1, T2, T3, T4, bool> Implementation;

        /// <summary>
        /// Make a primitive test, i.e. a predicate that can only be called on instantiated arguments.
        /// </summary>
        /// <param name="name">Name, for debugging purposes</param>
        /// <param name="implementation">C# implementation.
        /// This will be called with the values of the arguments.
        /// If it returns true, the call to the test succeeds, otherwise, it fails and the system backtracks</param>
        /// <param name="isFunctional">This predicate has no side-effects and so can be safely constant-folded</param>
        public PrimitiveTest(string name, Func<T1, T2, T3, T4, bool> implementation, bool isFunctional = true) : base(name) {
            Implementation = implementation;
            if (isFunctional)
                ConstantFolder = Implementation;
        }

        /// <inheritdoc />
        public override Interpreter.Call MakeCall(Goal g, GoalAnalyzer tc) {
            var i1 = tc.Emit(g.Arg1);
            var i2 = tc.Emit(g.Arg2);
            var i3 = tc.Emit(g.Arg3);
            var i4 = tc.Emit(g.Arg4);
            if (!i1.IsInstantiated)
                throw new InstantiationException(this, g.Arg1);
            if (!i2.IsInstantiated)
                throw new InstantiationException(this, g.Arg2);
            if (!i3.IsInstantiated)
                throw new InstantiationException(this, g.Arg3);
            if (!i4.IsInstantiated)
                throw new InstantiationException(this, g.Arg4);
            return new Call(this, i1.ValueCell, i2.ValueCell, i3.ValueCell, i4.ValueCell);
        }

        private class Call : Interpreter.Call {
            private readonly PrimitiveTest<T1, T2, T3, T4> predicate;
            private readonly ValueCell<T1> arg1;
            private readonly ValueCell<T2> arg2;
            private readonly ValueCell<T3> arg3;
            private readonly ValueCell<T4> arg4;
            private bool ready;

            public override IPattern ArgumentPattern =>
                new Pattern<T1, T2, T3, T4>(MatchOperation<T1>.Read(arg1), MatchOperation<T2>.Read(arg2), MatchOperation<T3>.Read(arg3), MatchOperation<T4>.Read(arg4));

            public Call(PrimitiveTest<T1, T2, T3, T4> predicate, ValueCell<T1> arg1, ValueCell<T2> arg2, ValueCell<T3> arg3, ValueCell<T4> arg4) : base(predicate) {
                this.predicate = predicate;
                this.arg1 = arg1;
                this.arg2 = arg2;
                this.arg3 = arg3;
                this.arg4 = arg4;
            }

            public override void Reset() => ready = true;

            public override bool NextSolution() {
                if (!ready) return false;
                ready = false;
                return predicate.Implementation(arg1.Value, arg2.Value, arg3.Value, arg4.Value);
            }
        }
    }

    /// <summary>
    /// A primitive that tests instantiated two arguments. Throws an instantiation exception if either argument is an unbound variable.
    /// </summary>
    /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
    /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
    /// <typeparam name="T3">Type of the third argument to the predicate</typeparam>
    /// <typeparam name="T4">Type of the fourth argument to the predicate</typeparam>
    /// <typeparam name="T5">Type of the fifth argument to the predicate</typeparam>
    public class PrimitiveTest<T1, T2, T3, T4, T5> : PrimitivePredicate<T1, T2, T3, T4, T5> {
        internal readonly Func<T1, T2, T3, T4, T5, bool> Implementation;

        /// <summary>
        /// Make a primitive test, i.e. a predicate that can only be called on instantiated arguments.
        /// </summary>
        /// <param name="name">Name, for debugging purposes</param>
        /// <param name="implementation">C# implementation.
        /// This will be called with the values of the arguments.
        /// If it returns true, the call to the test succeeds, otherwise, it fails and the system backtracks</param>
        /// <param name="isFunctional">This predicate has no side-effects and so can be safely constant-folded</param>
        public PrimitiveTest(string name, Func<T1, T2, T3, T4, T5, bool> implementation, bool isFunctional = true) : base(name) {
            Implementation = implementation;
            if (isFunctional)
                ConstantFolder = Implementation;
        }

        /// <inheritdoc />
        public override Interpreter.Call MakeCall(Goal g, GoalAnalyzer tc) {
            var i1 = tc.Emit(g.Arg1);
            var i2 = tc.Emit(g.Arg2);
            var i3 = tc.Emit(g.Arg3);
            var i4 = tc.Emit(g.Arg4);
            var i5 = tc.Emit(g.Arg5);
            if (!i1.IsInstantiated)
                throw new InstantiationException(this, g.Arg1);
            if (!i2.IsInstantiated)
                throw new InstantiationException(this, g.Arg2);
            if (!i3.IsInstantiated)
                throw new InstantiationException(this, g.Arg3);
            if (!i4.IsInstantiated)
                throw new InstantiationException(this, g.Arg4);
            if (!i5.IsInstantiated)
                throw new InstantiationException(this, g.Arg5);
            return new Call(this, i1.ValueCell, i2.ValueCell, i3.ValueCell, i4.ValueCell, i5.ValueCell);
        }

        private class Call : Interpreter.Call {
            private readonly PrimitiveTest<T1, T2, T3, T4, T5> predicate;
            private readonly ValueCell<T1> arg1;
            private readonly ValueCell<T2> arg2;
            private readonly ValueCell<T3> arg3;
            private readonly ValueCell<T4> arg4;
            private readonly ValueCell<T5> arg5;
            private bool ready;

            public override IPattern ArgumentPattern =>
                new Pattern<T1, T2, T3, T4, T5>(MatchOperation<T1>.Read(arg1), MatchOperation<T2>.Read(arg2), MatchOperation<T3>.Read(arg3), MatchOperation<T4>.Read(arg4), MatchOperation<T5>.Read(arg5));

            public Call(PrimitiveTest<T1, T2, T3, T4, T5> predicate, ValueCell<T1> arg1, ValueCell<T2> arg2, ValueCell<T3> arg3, ValueCell<T4> arg4, ValueCell<T5> arg5) : base(predicate) {
                this.predicate = predicate;
                this.arg1 = arg1;
                this.arg2 = arg2;
                this.arg3 = arg3;
                this.arg4 = arg4;
                this.arg5 = arg5;
            }

            public override void Reset() => ready = true;

            public override bool NextSolution() {
                if (!ready) return false;
                ready = false;
                return predicate.Implementation(arg1.Value, arg2.Value, arg3.Value, arg4.Value, arg5.Value);
            }
        }
    }

    /// <summary>
    /// A primitive that tests instantiated two arguments. Throws an instantiation exception if either argument is an unbound variable.
    /// </summary>
    /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
    /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
    /// <typeparam name="T3">Type of the third argument to the predicate</typeparam>
    /// <typeparam name="T4">Type of the fourth argument to the predicate</typeparam>
    /// <typeparam name="T5">Type of the fifth argument to the predicate</typeparam>
    /// <typeparam name="T6">Type of the sixth argument to the predicate</typeparam>
    public class PrimitiveTest<T1, T2, T3, T4, T5, T6> : PrimitivePredicate<T1, T2, T3, T4, T5, T6> {
        internal readonly Func<T1, T2, T3, T4, T5, T6, bool> Implementation;

        /// <summary>
        /// Make a primitive test, i.e. a predicate that can only be called on instantiated arguments.
        /// </summary>
        /// <param name="name">Name, for debugging purposes</param>
        /// <param name="implementation">C# implementation.
        /// This will be called with the values of the arguments.
        /// If it returns true, the call to the test succeeds, otherwise, it fails and the system backtracks</param>
        /// <param name="isFunctional">This predicate has no side-effects and so can be safely constant-folded</param>
        public PrimitiveTest(string name, Func<T1, T2, T3, T4, T5, T6, bool> implementation, bool isFunctional = true) : base(name) {
            Implementation = implementation;
            if (isFunctional)
                ConstantFolder = Implementation;
        }

        /// <inheritdoc />
        public override Interpreter.Call MakeCall(Goal g, GoalAnalyzer tc) {
            var i1 = tc.Emit(g.Arg1);
            var i2 = tc.Emit(g.Arg2);
            var i3 = tc.Emit(g.Arg3);
            var i4 = tc.Emit(g.Arg4);
            var i5 = tc.Emit(g.Arg5);
            var i6 = tc.Emit(g.Arg6);
            if (!i1.IsInstantiated)
                throw new InstantiationException(this, g.Arg1);
            if (!i2.IsInstantiated)
                throw new InstantiationException(this, g.Arg2);
            if (!i3.IsInstantiated)
                throw new InstantiationException(this, g.Arg3);
            if (!i4.IsInstantiated)
                throw new InstantiationException(this, g.Arg4);
            if (!i5.IsInstantiated)
                throw new InstantiationException(this, g.Arg5);
            if (!i6.IsInstantiated)
                throw new InstantiationException(this, g.Arg6);
            return new Call(this, i1.ValueCell, i2.ValueCell, i3.ValueCell, i4.ValueCell, i5.ValueCell, i6.ValueCell);
        }

        private class Call : Interpreter.Call {
            private readonly PrimitiveTest<T1, T2, T3, T4, T5, T6> predicate;
            private readonly ValueCell<T1> arg1;
            private readonly ValueCell<T2> arg2;
            private readonly ValueCell<T3> arg3;
            private readonly ValueCell<T4> arg4;
            private readonly ValueCell<T5> arg5;
            private readonly ValueCell<T6> arg6;
            private bool ready;

            public override IPattern ArgumentPattern =>
                new Pattern<T1, T2, T3, T4, T5, T6>(MatchOperation<T1>.Read(arg1), MatchOperation<T2>.Read(arg2), MatchOperation<T3>.Read(arg3), MatchOperation<T4>.Read(arg4), MatchOperation<T5>.Read(arg5), MatchOperation<T6>.Read(arg6));

            public Call(PrimitiveTest<T1, T2, T3, T4, T5, T6> predicate, ValueCell<T1> arg1, ValueCell<T2> arg2, ValueCell<T3> arg3, ValueCell<T4> arg4, ValueCell<T5> arg5, ValueCell<T6> arg6) : base(predicate) {
                this.predicate = predicate;
                this.arg1 = arg1;
                this.arg2 = arg2;
                this.arg3 = arg3;
                this.arg4 = arg4;
                this.arg5 = arg5;
                this.arg6 = arg6;
            }

            public override void Reset() => ready = true;

            public override bool NextSolution() {
                if (!ready) return false;
                ready = false;
                return predicate.Implementation(arg1.Value, arg2.Value, arg3.Value, arg4.Value, arg5.Value, arg6.Value);
            }
        }
    }

    /// <summary>
    /// A primitive that tests instantiated two arguments. Throws an instantiation exception if either argument is an unbound variable.
    /// </summary>
    /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
    /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
    /// <typeparam name="T3">Type of the third argument to the predicate</typeparam>
    /// <typeparam name="T4">Type of the fourth argument to the predicate</typeparam>
    /// <typeparam name="T5">Type of the fifth argument to the predicate</typeparam>
    /// <typeparam name="T6">Type of the sixth argument to the predicate</typeparam>
    /// <typeparam name="T7">Type of the seventh argument to the predicate</typeparam>
    public class PrimitiveTest<T1, T2, T3, T4, T5, T6, T7> : PrimitivePredicate<T1, T2, T3, T4, T5, T6, T7> {
        internal readonly Func<T1, T2, T3, T4, T5, T6, T7, bool> Implementation;

        /// <summary>
        /// Make a primitive test, i.e. a predicate that can only be called on instantiated arguments.
        /// </summary>
        /// <param name="name">Name, for debugging purposes</param>
        /// <param name="implementation">C# implementation.
        /// This will be called with the values of the arguments.
        /// If it returns true, the call to the test succeeds, otherwise, it fails and the system backtracks</param>
        /// <param name="isFunctional">This predicate has no side-effects and so can be safely constant-folded</param>
        public PrimitiveTest(string name, Func<T1, T2, T3, T4, T5, T6, T7, bool> implementation, bool isFunctional = true) : base(name) {
            Implementation = implementation;
            if (isFunctional)
                ConstantFolder = Implementation;
        }

        /// <inheritdoc />
        public override Interpreter.Call MakeCall(Goal g, GoalAnalyzer tc) {
            var i1 = tc.Emit(g.Arg1);
            var i2 = tc.Emit(g.Arg2);
            var i3 = tc.Emit(g.Arg3);
            var i4 = tc.Emit(g.Arg4);
            var i5 = tc.Emit(g.Arg5);
            var i6 = tc.Emit(g.Arg6);
            var i7 = tc.Emit(g.Arg7);
            if (!i1.IsInstantiated)
                throw new InstantiationException(this, g.Arg1);
            if (!i2.IsInstantiated)
                throw new InstantiationException(this, g.Arg2);
            if (!i3.IsInstantiated)
                throw new InstantiationException(this, g.Arg3);
            if (!i4.IsInstantiated)
                throw new InstantiationException(this, g.Arg4);
            if (!i5.IsInstantiated)
                throw new InstantiationException(this, g.Arg5);
            if (!i6.IsInstantiated)
                throw new InstantiationException(this, g.Arg6);
            if (!i7.IsInstantiated)
                throw new InstantiationException(this, g.Arg7);
            return new Call(this, i1.ValueCell, i2.ValueCell, i3.ValueCell, i4.ValueCell, i5.ValueCell, i6.ValueCell, i7.ValueCell);
        }

        private class Call : Interpreter.Call {
            private readonly PrimitiveTest<T1, T2, T3, T4, T5, T6, T7> predicate;
            private readonly ValueCell<T1> arg1;
            private readonly ValueCell<T2> arg2;
            private readonly ValueCell<T3> arg3;
            private readonly ValueCell<T4> arg4;
            private readonly ValueCell<T5> arg5;
            private readonly ValueCell<T6> arg6;
            private readonly ValueCell<T7> arg7;
            private bool ready;

            public override IPattern ArgumentPattern =>
                new Pattern<T1, T2, T3, T4, T5, T6, T7>(MatchOperation<T1>.Read(arg1), MatchOperation<T2>.Read(arg2), MatchOperation<T3>.Read(arg3), MatchOperation<T4>.Read(arg4), MatchOperation<T5>.Read(arg5), MatchOperation<T6>.Read(arg6), MatchOperation<T7>.Read(arg7));

            public Call(PrimitiveTest<T1, T2, T3, T4, T5, T6, T7> predicate, ValueCell<T1> arg1, ValueCell<T2> arg2, ValueCell<T3> arg3, ValueCell<T4> arg4, ValueCell<T5> arg5, ValueCell<T6> arg6, ValueCell<T7> arg7) : base(predicate) {
                this.predicate = predicate;
                this.arg1 = arg1;
                this.arg2 = arg2;
                this.arg3 = arg3;
                this.arg4 = arg4;
                this.arg5 = arg5;
                this.arg6 = arg6;
                this.arg7 = arg7;
            }

            public override void Reset() => ready = true;

            public override bool NextSolution() {
                if (!ready) return false;
                ready = false;
                return predicate.Implementation(arg1.Value, arg2.Value, arg3.Value, arg4.Value, arg5.Value, arg6.Value, arg7.Value);
            }
        }
    }

    /// <summary>
    /// A primitive that tests instantiated two arguments. Throws an instantiation exception if either argument is an unbound variable.
    /// </summary>
    /// <typeparam name="T1">Type of the first argument to the predicate</typeparam>
    /// <typeparam name="T2">Type of the second argument to the predicate</typeparam>
    /// <typeparam name="T3">Type of the third argument to the predicate</typeparam>
    /// <typeparam name="T4">Type of the fourth argument to the predicate</typeparam>
    /// <typeparam name="T5">Type of the fifth argument to the predicate</typeparam>
    /// <typeparam name="T6">Type of the sixth argument to the predicate</typeparam>
    /// <typeparam name="T7">Type of the seventh argument to the predicate</typeparam>
    /// <typeparam name="T8">Type of the eighth argument to the predicate</typeparam>
    public class PrimitiveTest<T1, T2, T3, T4, T5, T6, T7, T8> : PrimitivePredicate<T1, T2, T3, T4, T5, T6, T7, T8> {
        internal readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, bool> Implementation;

        /// <summary>
        /// Make a primitive test, i.e. a predicate that can only be called on instantiated arguments.
        /// </summary>
        /// <param name="name">Name, for debugging purposes</param>
        /// <param name="implementation">C# implementation.
        /// This will be called with the values of the arguments.
        /// If it returns true, the call to the test succeeds, otherwise, it fails and the system backtracks</param>
        /// <param name="isFunctional">This predicate has no side-effects and so can be safely constant-folded</param>
        public PrimitiveTest(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, bool> implementation, bool isFunctional = true) : base(name) {
            Implementation = implementation;
            if (isFunctional)
                ConstantFolder = Implementation;
        }

        /// <inheritdoc />
        public override Interpreter.Call MakeCall(Goal g, GoalAnalyzer tc) {
            var i1 = tc.Emit(g.Arg1);
            var i2 = tc.Emit(g.Arg2);
            var i3 = tc.Emit(g.Arg3);
            var i4 = tc.Emit(g.Arg4);
            var i5 = tc.Emit(g.Arg5);
            var i6 = tc.Emit(g.Arg6);
            var i7 = tc.Emit(g.Arg7);
            var i8 = tc.Emit(g.Arg8);
            if (!i1.IsInstantiated)
                throw new InstantiationException(this, g.Arg1);
            if (!i2.IsInstantiated)
                throw new InstantiationException(this, g.Arg2);
            if (!i3.IsInstantiated)
                throw new InstantiationException(this, g.Arg3);
            if (!i4.IsInstantiated)
                throw new InstantiationException(this, g.Arg4);
            if (!i5.IsInstantiated)
                throw new InstantiationException(this, g.Arg5);
            if (!i6.IsInstantiated)
                throw new InstantiationException(this, g.Arg6);
            if (!i7.IsInstantiated)
                throw new InstantiationException(this, g.Arg7);
            if (!i8.IsInstantiated)
                throw new InstantiationException(this, g.Arg8);
            return new Call(this, i1.ValueCell, i2.ValueCell, i3.ValueCell, i4.ValueCell, i5.ValueCell, i6.ValueCell, i7.ValueCell, i8.ValueCell);
        }

        private class Call : Interpreter.Call {
            private readonly PrimitiveTest<T1, T2, T3, T4, T5, T6, T7, T8> predicate;
            private readonly ValueCell<T1> arg1;
            private readonly ValueCell<T2> arg2;
            private readonly ValueCell<T3> arg3;
            private readonly ValueCell<T4> arg4;
            private readonly ValueCell<T5> arg5;
            private readonly ValueCell<T6> arg6;
            private readonly ValueCell<T7> arg7;
            private readonly ValueCell<T8> arg8;
            private bool ready;

            public override IPattern ArgumentPattern =>
                new Pattern<T1, T2, T3, T4, T5, T6, T7, T8>(MatchOperation<T1>.Read(arg1), MatchOperation<T2>.Read(arg2), MatchOperation<T3>.Read(arg3), MatchOperation<T4>.Read(arg4), MatchOperation<T5>.Read(arg5), MatchOperation<T6>.Read(arg6), MatchOperation<T7>.Read(arg7), MatchOperation<T8>.Read(arg8));

            public Call(PrimitiveTest<T1, T2, T3, T4, T5, T6, T7, T8> predicate, ValueCell<T1> arg1, ValueCell<T2> arg2, ValueCell<T3> arg3, ValueCell<T4> arg4, ValueCell<T5> arg5, ValueCell<T6> arg6, ValueCell<T7> arg7, ValueCell<T8> arg8) : base(predicate) {
                this.predicate = predicate;
                this.arg1 = arg1;
                this.arg2 = arg2;
                this.arg3 = arg3;
                this.arg4 = arg4;
                this.arg5 = arg5;
                this.arg6 = arg6;
                this.arg7 = arg7;
                this.arg8 = arg8;
            }

            public override void Reset() => ready = true;

            public override bool NextSolution() {
                if (!ready) return false;
                ready = false;
                return predicate.Implementation(arg1.Value, arg2.Value, arg3.Value, arg4.Value, arg5.Value, arg6.Value, arg7.Value, arg8.Value);
            }
        }
    }
}
