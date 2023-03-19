namespace TED
{
    /// <summary>
    /// Shared RNG used by al all the randomization primitives
    /// </summary>
    public static class Random
    {
        /// <summary>
        /// Shared RNG used by TED.
        /// Set the seed on this if you want deterministic behavior.
        /// </summary>
        public static System.Random Rng = new System.Random();

        /// <summary>
        /// Generate a random float
        /// </summary>
        public static float Float() => (float)Rng.NextDouble();

        /// <summary>
        /// Generate a random Boolean with a specified probability of being true
        /// </summary>
        /// <param name="probability">Probability of returning true.</param>
        public static bool Roll(float probability) => Float() <= probability;

        /// <summary>
        /// Return a random integer from start to end-1.
        /// </summary>
        public static int InRangeExclusive(int start, int end) => start + Rng.Next() % (end - start);
    }
}
