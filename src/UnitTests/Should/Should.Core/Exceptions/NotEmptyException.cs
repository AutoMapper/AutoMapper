namespace Should.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when a collection is unexpectedly empty.
    /// </summary>
    public class NotEmptyException : AssertException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="NotEmptyException"/> class.
        /// </summary>
        public NotEmptyException()
            : base("Assert.NotEmpty() failure") { }
    }
}