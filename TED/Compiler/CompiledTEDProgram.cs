using System;

namespace TED.Compiler
{
    public abstract class CompiledTEDProgram
    {
        public abstract void Link(Program _program);

        protected static Random MakeRng() => Utilities.Random.MakeRng();
    }
}
