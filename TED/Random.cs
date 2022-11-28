using System;
using System.Collections.Generic;
using System.Text;

namespace TED
{
    public static class Random
    {
        public static System.Random Rng = new System.Random();

        public static float Float() => (float)Rng.NextDouble();
        public static bool Roll(float probability) => Float() <= probability;

        public static int InRangeExclusive(int start, int end) => start + Rng.Next() % (end - start);
    }
}
