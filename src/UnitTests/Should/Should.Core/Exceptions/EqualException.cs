namespace Should.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when two values are unexpectedly not equal.
    /// </summary>
    public class EqualException : AssertActualExpectedException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="EqualException"/> class.
        /// </summary>
        /// <param name="expected">The expected object value</param>
        /// <param name="actual">The actual object value</param>
        public EqualException(object expected,
                              object actual)
            : this(expected, actual, "Assert.Equal() Failure") { }

        /// <summary>
        /// Creates a new instance of the <see cref="EqualException"/> class.
        /// </summary>
        /// <param name="expected">The expected object value</param>
        /// <param name="actual">The actual object value</param>
        /// <param name="userMessage">The user message to be shown on failure</param>
        public EqualException(object expected, object actual, string userMessage)
            : base(expected, actual, userMessage) { }
    }
}