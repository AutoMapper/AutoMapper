namespace Should.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when an object is unexpectedly null.
    /// </summary>
    public class NotNullException : AssertException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="NotNullException"/> class.
        /// </summary>
        public NotNullException()
            : this("Assert.NotNull() Failure") { }

        /// <summary>
        /// Creates a new instance of the <see cref="NotNullException"/> class with the given failure <paramref name="message"/>.
        /// </summary>
        public NotNullException(string message)
            : base(message) { }
    }
}