namespace Should.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when a test method exceeds the given timeout value
    /// </summary>
    public class TimeoutException : AssertException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="TimeoutException"/> class.
        /// </summary>
        /// <param name="timeout">The timeout value, in milliseconds</param>
        public TimeoutException(long timeout)
            : base(string.Format("Test execution time exceeded: {0}ms", timeout)) { }
    }
}