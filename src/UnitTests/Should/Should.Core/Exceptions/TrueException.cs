namespace Should.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when a value is unexpectedly false.
    /// </summary>
    public class TrueException : AssertException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="TrueException"/> class.
        /// </summary>
        /// <param name="userMessage">The user message to be displayed, or null for the default message</param>
        public TrueException(string userMessage)
            : base(userMessage ?? "Assert.True() Failure") { }
    }
}