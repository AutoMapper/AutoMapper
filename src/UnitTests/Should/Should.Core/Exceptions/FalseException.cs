namespace Should.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when a value is unexpectedly true.
    /// </summary>
    public class FalseException : AssertException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="FalseException"/> class.
        /// </summary>
        /// <param name="userMessage">The user message to be display, or null for the default message</param>
        public FalseException(string userMessage)
            : base(userMessage ?? "Assert.False() Failure") { }
    }
}