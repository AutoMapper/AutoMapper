using System;
using Should.Core.Assertions;
using Should.Core.Exceptions;

namespace Should
{
    /// <summary>
    /// Extensions which provide assertions to classes derived from <see cref="Boolean"/>.
    /// </summary>
    public static class DoubleAssertionExtensions
    {
        /// <summary>
        /// Verifies that two values are equal within a given tolerance.
        /// </summary>
        /// <param name="actual">The value to be compared against</param>
        /// <param name="expected">The expected value</param>
        /// <param name="tolerance">The +/- value for where the expected and actual are considered to be equal</param>
        /// <exception cref="EqualException">Thrown when the objects are not equal</exception>
        public static void ShouldEqual(this double actual, double expected, double tolerance)
        {
            Assert.Equal(expected, actual, tolerance);
        }

        /// <summary>
        /// Verifies that two values are equal within a given tolerance.
        /// </summary>
        /// <param name="actual">The value to be compared against</param>
        /// <param name="expected">The expected value</param>
        /// <param name="tolerance">The +/- value for where the expected and actual are considered to be equal</param>
        /// <param name="message">The user message to show on failure</param>
        /// <exception cref="EqualException">Thrown when the objects are not equal</exception>
        public static void ShouldEqual(this double actual, double expected, double tolerance, string message)
        {
            Assert.Equal(expected, actual, tolerance, message);
        }

        /// <summary>
        /// Verifies that two values are not equal within a given tolerance.
        /// </summary>
        /// <param name="actual">The value to be compared against</param>
        /// <param name="expected">The expected value</param>
        /// <param name="tolerance">The +/- value for where the expected and actual are considered to be equal</param>
        /// <exception cref="EqualException">Thrown when the objects are equal</exception>
        public static void ShouldNotEqual(this double actual, double expected, double tolerance)
        {
            Assert.NotEqual(expected, actual, tolerance);
        }

        /// <summary>
        /// Verifies that two values are not equal within a given tolerance.
        /// </summary>
        /// <param name="actual">The value to be compared against</param>
        /// <param name="expected">The expected value</param>
        /// <param name="tolerance">The +/- value for where the expected and actual are considered to be equal</param>
        /// <param name="message">The user message to show on failure</param>
        /// <exception cref="EqualException">Thrown when the objects are equal</exception>
        public static void ShouldNotEqual(this double actual, double expected, double tolerance, string message)
        {
            Assert.NotEqual(expected, actual, tolerance, message);
        }
    }
}
