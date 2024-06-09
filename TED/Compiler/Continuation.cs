using System;
using System.Collections.Generic;
using System.Text;

namespace TED.Compiler
{
    public class Continuation
    {
        public readonly string Label;

        public Continuation(string label)
        {
            Label = label;
        }
        
        public string Invoke => $"goto {Label}";
    }
}
