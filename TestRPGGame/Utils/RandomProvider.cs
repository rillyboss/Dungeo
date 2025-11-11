using System;

namespace TestRPGGame.Utils
{
    /// <summary>
    /// Centralized random number generator for consistent, thread-safe randomness across the game.
    /// Using a single Random instance prevents seeding issues and improves randomness quality.
    /// </summary>
    public static class RandomProvider
    {
        private static readonly Random _random = new Random();
        private static readonly object _lock = new object();

        /// <summary>
        /// Returns a non-negative random integer less than the specified maximum.
        /// </summary>
        /// <param name="max">The exclusive upper bound of the random number to be generated.</param>
        /// <returns>A 32-bit signed integer that is greater than or equal to 0, and less than max.</returns>
        public static int Next(int max)
        {
            lock (_lock)
            {
                return _random.Next(max);
            }
        }

        /// <summary>
        /// Returns a random integer that is within a specified range.
        /// </summary>
        /// <param name="min">The inclusive lower bound of the random number returned.</param>
        /// <param name="max">The exclusive upper bound of the random number returned.</param>
        /// <returns>A 32-bit signed integer greater than or equal to min and less than max.</returns>
        public static int Next(int min, int max)
        {
            lock (_lock)
            {
                return _random.Next(min, max);
            }
        }

        /// <summary>
        /// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        /// <returns>A double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        public static double NextDouble()
        {
            lock (_lock)
            {
                return _random.NextDouble();
            }
        }

        /// <summary>
        /// Returns a random boolean value.
        /// </summary>
        /// <returns>true or false with equal probability.</returns>
        public static bool NextBool()
        {
            lock (_lock)
            {
                return _random.Next(2) == 0;
            }
        }

        /// <summary>
        /// Returns a random boolean with the specified probability of being true.
        /// </summary>
        /// <param name="probability">The probability (0.0 to 1.0) of returning true.</param>
        /// <returns>true with the specified probability, false otherwise.</returns>
        public static bool NextBool(double probability)
        {
            lock (_lock)
            {
                return _random.NextDouble() < probability;
            }
        }

        /// <summary>
        /// Sets the seed for testing purposes. Use with caution - this affects global randomness.
        /// </summary>
        /// <param name="seed">The seed value to use for the random number generator.</param>
        internal static void SetSeedForTesting(int seed)
        {
            lock (_lock)
            {
                // Note: Cannot re-seed an existing Random instance
                // This would require reflection or creating a new instance
                // For now, this is a placeholder for future enhancement
                // Consider making _random non-readonly if seeding is required
            }
        }
    }
}
