namespace Should.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when a collection is unexpectedly not empty.
    /// </summary>
    public class EmptyException : AssertException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="EmptyException"/> class.
        /// </summary>
        public EmptyException()
            : base("Assert.Empty() failure") { }
    }
}