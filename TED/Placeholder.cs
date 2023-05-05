using System;
using System.Collections.Generic;
using System.Text;

namespace TED
{
    public class Placeholder
    {
        public static readonly Placeholder Singleton = new Placeholder();

        public override string ToString() => "__";
    }
}
